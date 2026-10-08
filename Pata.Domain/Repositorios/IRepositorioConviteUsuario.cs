using Pata.Domain.Entidades.Organizacao;

namespace Pata.Domain.Repositorios;

public interface IRepositorioConviteUsuario
{
    Task<AcessoOrganizacao> AceitarAuth0Async(string tokenHash, string auth0Sub, string email,
        DateTimeOffset agora, CancellationToken cancellationToken = default);
}
