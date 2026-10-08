using Pata.Application.Comum.Mensagens;

namespace Pata.Application.Funcionalidades.Organizacao.Comandos.CriarOrganizacao;

public sealed record CriarOrganizacaoComando(
    string Nome,
    string Slug,
    string Cnpj,
    string Email,
    string? LogoUrl,
    string Auth0Sub,
    string NomeAdmin,
    string SobrenomeAdmin,
    string EmailAdmin) : IComando<Guid>;
