using Pata.Domain.Comum;
using Pata.Domain.Excecoes;

namespace Pata.Domain.Entidades.Tutor;

/// <summary>Token de acesso do tutor limitado ao cadastro dele em uma clínica.</summary>
public sealed class AcessoPortalTutor : EntidadeTenant<Guid>
{
    private AcessoPortalTutor() { }

    public AcessoPortalTutor(Guid tenantId, Guid tutorId, string tokenHash, DateTimeOffset criadoEm)
        : base(Guid.NewGuid(), tenantId)
    {
        if (tutorId == Guid.Empty) throw new ErroDeValidacao("Tutor e obrigatorio.", nameof(tutorId));
        TutorId = tutorId;
        AtualizarToken(tokenHash, criadoEm);
    }

    public Guid TutorId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset? RevogadoEm { get; private set; }

    public void RotacionarToken(string tokenHash, DateTimeOffset criadoEm) => AtualizarToken(tokenHash, criadoEm);

    public void Revogar(DateTimeOffset revogadoEm)
    {
        if (RevogadoEm.HasValue) throw new RegraDeNegocioException("O link ja foi revogado.");
        if (revogadoEm == default) throw new ErroDeValidacao("Data de revogacao e obrigatoria.", nameof(revogadoEm));
        RevogadoEm = revogadoEm;
    }

    private void AtualizarToken(string tokenHash, DateTimeOffset criadoEm)
    {
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ErroDeValidacao("Hash do token e obrigatorio.", nameof(tokenHash));
        if (criadoEm == default) throw new ErroDeValidacao("Data de criacao e obrigatoria.", nameof(criadoEm));
        TokenHash = tokenHash;
        CriadoEm = criadoEm;
        RevogadoEm = null;
    }
}
