# Pata Backend

API ASP.NET Core com CQRS via MediatR, domínio separado da infraestrutura e persistência PostgreSQL com Entity Framework Core.

## Executar com Docker Compose

```bash
docker compose up --build
```

A API fica disponível em `http://localhost:8080` e o PostgreSQL em `localhost:5432`. A senha local padrão é `pata_local`; defina `POSTGRES_PASSWORD` no ambiente para alterá-la. O volume `pata_postgres` mantém os dados entre reinicializações. Na primeira inicialização a API cria o esquema do banco.

O Swagger UI fica em `http://localhost:8080/swagger` e a especificação OpenAPI em `http://localhost:8080/swagger/v1/swagger.json`.

## Rotas

- `GET /health`
- `POST /api/tutores`, `GET /api/tutores?pagina=1&tamanhoPagina=20`, `GET /api/tutores/cpf/{cpf}`
- `GET /api/tutores/excluidos?pagina=1&tamanhoPagina=20`
- `PUT` e `DELETE /api/tutores/{id}`; `POST /api/tutores/{id}/recuperacao`
- `POST /api/veterinarios`, `GET /api/veterinarios?pagina=1&tamanhoPagina=20`
- `GET /api/veterinarios/crmv/{uf}/{numero}` e `GET /api/veterinarios/excluidos`
- `PUT` e `DELETE /api/veterinarios/{id}`; `POST /api/veterinarios/{id}/recuperacao`

A API organiza as rotas em `TutoresController` e `VeterinariosController`. Os DTOs da API controlam os campos aceitos e retornados, e cada controller envia mensagens CQRS pelo `ISender`; handlers ficam na Application, enquanto repositórios e consultas de leitura ficam na Infrastructure. `PataDbContext` grava datas de auditoria em UTC e aplica exclusão lógica para tutores e veterinários.
