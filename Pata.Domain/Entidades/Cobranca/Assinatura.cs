using Pata.Domain.Comum;
using Pata.Domain.Excecoes;

namespace Pata.Domain.Entidades.Cobranca;

public enum StatusAssinatura { Pendente, EmTeste, Ativa, Inadimplente, Suspensa, Cancelada }

public sealed class Assinatura : EntidadeTenant<Guid>
{
    private Assinatura() { }

    public Assinatura(Guid tenantId, Guid precoPlanoId, DateTimeOffset inicioEm,
        DateTimeOffset? testeInicio = null, DateTimeOffset? testeFim = null) : base(Guid.NewGuid(), tenantId)
    {
        if (precoPlanoId == Guid.Empty) throw new ErroDeValidacao("Preco do plano e obrigatorio.", nameof(precoPlanoId));
        if (inicioEm == default) throw new ErroDeValidacao("Inicio da assinatura e obrigatorio.", nameof(inicioEm));
        if (testeInicio.HasValue != testeFim.HasValue || (testeInicio.HasValue && testeFim <= testeInicio))
            throw new ErroDeValidacao("Periodo de teste invalido.");
        OrganizacaoId = tenantId;
        PrecoPlanoId = precoPlanoId;
        InicioEm = inicioEm;
        TesteInicio = testeInicio;
        TesteFim = testeFim;
        Status = testeFim.HasValue ? StatusAssinatura.EmTeste : StatusAssinatura.Pendente;
    }

    public Guid OrganizacaoId { get; private set; }
    public Guid PrecoPlanoId { get; private set; }
    public StatusAssinatura Status { get; private set; }
    public DateTimeOffset InicioEm { get; private set; }
    public DateTimeOffset? PeriodoAtualInicio { get; private set; }
    public DateTimeOffset? PeriodoAtualFim { get; private set; }
    public DateTimeOffset? TesteInicio { get; private set; }
    public DateTimeOffset? TesteFim { get; private set; }
    public DateTimeOffset? CancelarEm { get; private set; }
    public DateTimeOffset? CancelamentoSolicitadoEm { get; private set; }
    public DateTimeOffset? EncerradoEm { get; private set; }
}
