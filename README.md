# Pata Backend

## Objetivo

Construir uma API para clínicas veterinárias gerenciarem seus cadastros e operações, permitindo que cada clínica tenha seus próprios usuários, veterinários, tutores e pets. O tutor pode acompanhar os pets associados a ele por um link específico da clínica.

O projeto também serve como ambiente de estudo e evolução de uma arquitetura de backend em camadas, CQRS, arquitetura hexagonal e aplicações multi-tenant.

## Descrição

A API foi construída com ASP.NET Core e .NET 10. O PostgreSQL é acessado por Entity Framework Core e Npgsql. Os controllers recebem DTOs, enviam comandos e consultas pelo MediatR e retornam modelos de resposta próprios da API.

O domínio inclui:

- **Organização:** clínicas, equipes, usuários e convites.
- **Atendimento:** veterinários, tutores, pets, consultas e prontuários.
- **Cobrança:** planos e preços globais; assinaturas, faturas, itens de fatura e pagamentos por clínica.
- **Auditoria:** registro de criação, edição e exclusão de entidades do tenant, com snapshots JSONB antes e depois.

## Regras de negócio

### Clínicas e isolamento de dados

- Cada clínica é um tenant identificado por um GUID.
- O tenant de uma requisição autenticada vem exclusivamente da claim `tenant_id` do JWT; a API não aceita que o cliente escolha esse valor no corpo ou na query string.
- As entidades pertencentes a uma clínica persistem `tenant_id`. Filtros globais do EF Core limitam consultas ao tenant atual, e o contexto valida o tenant das gravações.
- Chaves estrangeiras compostas incluem o tenant nas relações entre registros da clínica. Isso impede relacionar, por exemplo, um pet de uma clínica a um tutor de outra.
- Planos e preços são catálogos compartilhados. Dados operacionais, como assinaturas e faturas, pertencem à clínica.

### Tutores, pets e acesso do tutor

- Todo pet pertence obrigatoriamente a um tutor e à clínica que o cadastrou.
- Ao cadastrar um pet, a API procura o tutor pelo CPF dentro da clínica. Se não encontrar, cria um novo cadastro de tutor naquela clínica.
- Uma mesma pessoa pode ter cadastros locais em clínicas diferentes. Esses cadastros e seus acessos são independentes.
- Existe um link de acompanhamento por tutor e por clínica. Na mesma clínica, o link lista todos os pets associados àquele tutor.
- O link funciona como uma credencial de acesso: o token é aleatório, apenas seu hash é persistido e somente os dados necessários ao acompanhamento são retornados. A clínica pode gerar um novo link, invalidando o anterior, ou revogá-lo.
- O link é retornado pela API à clínica. O envio automático por e-mail ou WhatsApp ainda depende da integração com um provedor de comunicação.

### Usuários e veterinários

- Usuários da clínica são associados a uma equipe e pertencem ao tenant da clínica.
- Cadastros de veterinários também são isolados por clínica.
- A autenticação é validada pela API, mas a emissão de JWT, login e gestão de identidade são responsabilidade de um provedor externo e ainda não fazem parte deste projeto.

### Atendimento e cobrança

- Tutores, veterinários, animais e consultas são separados por tenant.
- Tutores e veterinários podem ser excluídos logicamente e recuperados.
- O estado atual de acompanhamento do pet é obtido pela consulta mais recente; pets sem consultas são retornados como `SemConsulta`.
- O modelo de cobrança possui estados definidos para planos, preços, assinaturas, faturas e pagamentos. A API já permite consultar e criar registros, mas ainda não implementa o ciclo completo de transição desses estados nem a integração com um provedor de pagamentos.

## Arquitetura e tópicos de estudo

### Estrutura atual

| Projeto | Responsabilidade |
| --- | --- |
| `Pata.Domain` | Entidades, regras de domínio, objetos de valor e contratos de repositório. |
| `Pata.Application` | Casos de uso, mensagens CQRS, handlers e abstrações consumidas pela aplicação. |
| `Pata.Infrastructure` | Entity Framework Core, PostgreSQL, mapeamentos e implementações de persistência. |
| `Pata.Api` | Controllers, DTOs, autenticação JWT, Swagger e composição das dependências. |

### CQRS

O projeto usa CQRS com MediatR para separar comandos que alteram estado de consultas de leitura. Controllers encaminham as mensagens; as respostas da API são projetadas em DTOs. O projeto não usa event sourcing.

### Arquitetura hexagonal

A estrutura busca aplicar a separação entre domínio, casos de uso e adaptadores. O domínio não depende do PostgreSQL; as abstrações ficam nas camadas internas e a Infrastructure implementa os adaptadores de persistência. A API é o adaptador de entrada HTTP. Essa arquitetura ainda está em evolução e pode ser refinada conforme os casos de uso crescerem.

### Multi-tenancy

Atualmente, todas as clínicas usam a mesma instância PostgreSQL, o mesmo banco e o mesmo schema. O isolamento é feito por `tenant_id`, filtros de consulta e restrições nas relações. O acesso do tutor por link é uma exceção controlada: o token identifica um único cadastro de tutor em uma única clínica, e a consulta pública retorna apenas os pets associados a ele.

**Pools e silos no banco ainda não estão implementados.** São uma etapa futura de estudo para comparar formas de distribuir tenants entre bancos ou grupos de bancos, avaliando isolamento, custo, operação, escalabilidade e movimentação de tenants.

## Executar localmente

```bash
docker compose up --build
```

- API: `http://localhost:8080`
- Swagger UI: `http://localhost:8080/swagger`
- PostgreSQL: `localhost:5432`
- Health check: `http://localhost:8080/health`

O Compose usa `pata_local` como senha local padrão e mantém os dados no volume `pata_postgres`. Configure `POSTGRES_PASSWORD` para alterá-la. Configure também `JWT_ISSUER`, `JWT_AUDIENCE` e `JWT_SIGNING_KEY`; a chave padrão é apenas para desenvolvimento e deve ser substituída fora do ambiente local.

Na inicialização, a aplicação usa `EnsureCreated` para criar o schema se o banco estiver vazio. Isso não atualiza um schema que já existe. Migrações do EF Core ainda não foram configuradas; portanto, alterações no modelo podem exigir uma migração ou a recriação do banco local.

## Autenticação

Exceto health check, documentação Swagger e endpoint de acompanhamento por link, as rotas de recursos exigem um JWT assinado pelo provedor de identidade. O token deve conter `sub` (identificador do usuário) e `tenant_id` (GUID da clínica). A API valida emissor, audiência, assinatura e validade. O Swagger permite informar o token pelo botão **Authorize**.

## Rotas principais

### Clínica, equipe e usuários

- `GET /api/organizacao`
- `GET/POST /api/organizacao/equipes` e `GET /api/organizacao/equipes/{id}`
- `GET/POST /api/organizacao/usuarios` e `GET /api/organizacao/usuarios/{id}`
- `GET/POST /api/organizacao/convites`

### Tutores e pets

- `POST /api/animais` cadastra um pet e cria ou reutiliza o tutor pelo CPF no tenant atual.
- `GET /api/animais` e `GET /api/animais/{id}` listam e consultam pets da clínica.
- `GET /api/portal/tutores/{token}` é o endpoint público do link de acompanhamento.
- `POST /api/tutores/{id}/link-acompanhamento` gera ou rotaciona o link.
- `DELETE /api/tutores/{id}/link-acompanhamento` revoga o link.
- A API também oferece operações de consulta, atualização, exclusão e recuperação de tutores.

### Veterinários

- `GET/POST /api/veterinarios`
- `GET /api/veterinarios/crmv/{uf}/{numero}`
- A API também oferece atualização, exclusão e recuperação de veterinários.

### Planos e cobrança

- `GET /api/planos` e `GET /api/planos/{id}`
- `POST /api/planos` cria um plano e exige o papel `admin`.
- `GET/POST /api/planos/{id}/precos`; a criação de planos e preços exige o papel `admin`.
- `GET/POST /api/assinaturas` e `GET /api/assinaturas/{id}`
- `GET/POST /api/faturas` e `GET /api/faturas/{id}`
- `POST /api/faturas/{id}/itens`
- `GET /api/pagamentos`, `GET /api/pagamentos/{id}` e `POST /api/faturas/{id}/pagamentos`

### Auditoria

- `GET /api/auditorias`
- `GET /api/auditorias/{id}`

## Estado do projeto

O projeto está em desenvolvimento. CQRS, persistência PostgreSQL, autenticação baseada em JWT e isolamento multi-tenant fazem parte da implementação atual. Integrações de identidade e comunicação, transições completas de cobrança, migrações do banco e a exploração de pools e silos são trabalhos futuros.

## Testes

Os testes acompanham as camadas da solução:

| Projeto | Escopo |
| --- | --- |
| `Pata.Domain.Tests` | Testes unitários das regras de domínio e objetos de valor. |
| `Pata.Application.Tests` | Testes dos casos de uso com repositórios e serviços falsos. |
| `Pata.Infrastructure.Tests` | Mapeamentos do EF Core, filtros de tenant e chaves estrangeiras compostas; inclui um teste de isolamento real no PostgreSQL. |
| `Pata.Api.Tests` | Contratos de rota, autenticação dos controllers e campos expostos pelos DTOs. |

Para executar os testes unitários e de contrato:

```bash
dotnet test Pata.slnx
```

O teste PostgreSQL é habilitado somente quando `PATA_TEST_POSTGRES` contém uma connection string para um banco de teste dedicado. Ele cria e remove um schema temporário, então o usuário do banco precisa ter permissão para criar e remover schemas.

```bash
PATA_TEST_POSTGRES="Host=localhost;Port=5432;Database=pata_test;Username=pata;Password=pata_local" dotnet test Pata.Infrastructure.Tests/Pata.Infrastructure.Tests.csproj
```
