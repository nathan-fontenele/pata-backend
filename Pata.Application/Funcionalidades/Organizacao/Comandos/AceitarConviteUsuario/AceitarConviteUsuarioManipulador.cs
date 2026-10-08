using Pata.Application.Comum.Mensagens;
using Pata.Domain.Entidades.Organizacao;
using Pata.Domain.Repositorios;

namespace Pata.Application.Funcionalidades.Organizacao.Comandos.AceitarConviteUsuario;

public sealed class AceitarConviteUsuarioManipulador(IRepositorioConviteUsuario repositorio)
    : IManipuladorComando<AceitarConviteUsuarioComando, AcessoOrganizacao>
{
    public Task<AcessoOrganizacao> Handle(AceitarConviteUsuarioComando comando, CancellationToken cancellationToken) =>
        repositorio.AceitarAuth0Async(comando.TokenHash, comando.Auth0Sub, comando.Email,
            comando.Agora, cancellationToken);
}
