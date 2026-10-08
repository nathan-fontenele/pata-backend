using Pata.Domain.Entidades.Tutor;
using Pata.Domain.Excecoes;

namespace Pata.Domain.Tests.Entidades;

public sealed class AcessoPortalTutorTests
{
    private static readonly DateTimeOffset Agora = new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void DeveCriarAcessoVinculadoAoTutorETenant()
    {
        var tenantId = Guid.NewGuid();
        var tutorId = Guid.NewGuid();

        var acesso = new AcessoPortalTutor(tenantId, tutorId, new string('A', 64), Agora);

        Assert.Equal(tenantId, acesso.TenantId);
        Assert.Equal(tutorId, acesso.TutorId);
        Assert.Equal(Agora, acesso.CriadoEm);
        Assert.Null(acesso.RevogadoEm);
    }

    [Fact]
    public void DeveRevogarEAceitarRotacaoDoLink()
    {
        var acesso = new AcessoPortalTutor(Guid.NewGuid(), Guid.NewGuid(), new string('A', 64), Agora);
        var revogadoEm = Agora.AddDays(1);

        acesso.Revogar(revogadoEm);
        Assert.Equal(revogadoEm, acesso.RevogadoEm);

        var rotacionadoEm = Agora.AddDays(2);
        acesso.RotacionarToken(new string('B', 64), rotacionadoEm);

        Assert.Equal(new string('B', 64), acesso.TokenHash);
        Assert.Equal(rotacionadoEm, acesso.CriadoEm);
        Assert.Null(acesso.RevogadoEm);
    }

    [Fact]
    public void NaoDeveRevogarLinkDuasVezes()
    {
        var acesso = new AcessoPortalTutor(Guid.NewGuid(), Guid.NewGuid(), new string('A', 64), Agora);
        acesso.Revogar(Agora.AddDays(1));

        Assert.Throws<RegraDeNegocioException>(() => acesso.Revogar(Agora.AddDays(2)));
    }

    [Fact]
    public void NaoDeveCriarAcessoSemTenantTutorOuToken()
    {
        Assert.Throws<ErroDeValidacao>(() => new AcessoPortalTutor(Guid.Empty, Guid.NewGuid(), "hash", Agora));
        Assert.Throws<ErroDeValidacao>(() => new AcessoPortalTutor(Guid.NewGuid(), Guid.Empty, "hash", Agora));
        Assert.Throws<ErroDeValidacao>(() => new AcessoPortalTutor(Guid.NewGuid(), Guid.NewGuid(), " ", Agora));
    }
}
