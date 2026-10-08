using Pata.Domain.Excecoes;

namespace Pata.Domain.Comum;

public abstract class RaizAgregadaAuditavel<TId> : Entidade<TId>, IAuditavel, IExcluivel, ITenantEntity
    where TId : notnull
{
    protected RaizAgregadaAuditavel()
    {
    }

    protected RaizAgregadaAuditavel(TId id) : base(id)
    {
    }

    protected RaizAgregadaAuditavel(TId id, Guid tenantId) : base(id)
    {
        if (tenantId == Guid.Empty)
            throw new ErroDeValidacao("Tenant e obrigatorio.", nameof(tenantId));
        TenantId = tenantId;
    }

    public Guid TenantId { get; private set; }

    public DateTime CriadoEm { get; private set; }
    public string CriadoPor { get; private set; } = default!;
    public DateTime? AtualizadoEm { get; private set; }
    public string? AtualizadoPor { get; private set; }
    public bool Excluido { get; private set; }
    public DateTime? ExcluidoEm { get; private set; }
    public string? ExcluidoPor { get; private set; }

    public void Excluir(DateTime excluidoEm, string excluidoPor)
    {
        if (Excluido)
            return;

        if (excluidoEm == default)
            throw new ErroDeValidacao("A data de exclusao e obrigatoria.", nameof(excluidoEm));

        if (excluidoEm.Kind != DateTimeKind.Utc)
            throw new ErroDeValidacao("A data de exclusao deve estar em UTC.", nameof(excluidoEm));

        if (string.IsNullOrWhiteSpace(excluidoPor))
            throw new ErroDeValidacao("O usuario da exclusao e obrigatorio.", nameof(excluidoPor));

        Excluido = true;
        ExcluidoEm = excluidoEm;
        ExcluidoPor = excluidoPor.Trim();
    }

    public void Recuperar()
    {
        if (!Excluido)
            return;

        Excluido = false;
        ExcluidoEm = null;
        ExcluidoPor = null;
    }
}
