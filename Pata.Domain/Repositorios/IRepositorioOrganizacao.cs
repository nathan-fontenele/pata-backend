using Pata.Domain.Entidades.Organizacao;

namespace Pata.Domain.Repositorios;

public interface IRepositorioOrganizacao
{
    Task<bool> ExisteSlugAsync(string slug, CancellationToken cancellationToken = default);

    Task CriarComPrimeiroAdminAsync(Organizacao organizacao, Equipe equipe, Usuario admin,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AcessoOrganizacao>> ListarAcessosAuth0Async(string auth0Sub,
        CancellationToken cancellationToken = default);

    Task<AcessoOrganizacao?> ObterAcessoAuth0Async(string auth0Sub, Guid tenantId,
        CancellationToken cancellationToken = default);
}
