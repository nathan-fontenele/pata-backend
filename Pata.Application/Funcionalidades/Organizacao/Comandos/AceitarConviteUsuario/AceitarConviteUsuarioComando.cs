using Pata.Application.Comum.Mensagens;
using Pata.Domain.Entidades.Organizacao;

namespace Pata.Application.Funcionalidades.Organizacao.Comandos.AceitarConviteUsuario;

public sealed record AceitarConviteUsuarioComando(string TokenHash, string Auth0Sub, string Email,
    DateTimeOffset Agora) : IComando<AcessoOrganizacao>;
