using Pata.Domain.Comum;
using Pata.Domain.Excecoes;

namespace Pata.Domain.Entidades.Organizacao;

public sealed class ConviteUsuario : EntidadeTenant<Guid>
{
    private ConviteUsuario() { }

    public ConviteUsuario(Guid tenantId, Guid usuarioId, string tokenHash, DateTimeOffset criadoEm,
        DateTimeOffset expiraEm, Guid? convidadoPorUsuarioId) : base(Guid.NewGuid(), tenantId)
    {
        if (usuarioId == Guid.Empty) throw new ErroDeValidacao("Usuario e obrigatorio.", nameof(usuarioId));
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ErroDeValidacao("Hash do token e obrigatorio.", nameof(tokenHash));
        if (criadoEm == default || expiraEm <= criadoEm) throw new ErroDeValidacao("Validade do convite invalida.");
        UsuarioId = usuarioId;
        TokenHash = tokenHash.Trim();
        CriadoEm = criadoEm;
        ExpiraEm = expiraEm;
        ConvidadoPorUsuarioId = convidadoPorUsuarioId;
    }

    public Guid UsuarioId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset ExpiraEm { get; private set; }
    public DateTimeOffset? AceitoEm { get; private set; }
    public DateTimeOffset? RevogadoEm { get; private set; }
    public Guid? ConvidadoPorUsuarioId { get; private set; }

    public void Aceitar(DateTimeOffset agora)
    {
        if (AceitoEm.HasValue || RevogadoEm.HasValue || ExpiraEm <= agora)
            throw new RegraDeNegocioException("O convite esta expirado, revogado ou ja foi aceito.");
        AceitoEm = agora;
    }
}
