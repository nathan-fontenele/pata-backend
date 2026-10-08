namespace Pata.Domain.Comum;

public interface ITenantEntity
{
    Guid TenantId { get; }
}

public abstract class EntidadeTenant<TId> : Entidade<TId>, ITenantEntity
    where TId : notnull
{
    protected EntidadeTenant() { }

    protected EntidadeTenant(TId id, Guid tenantId) : base(id)
    {
        if (tenantId == Guid.Empty)
            throw new Pata.Domain.Excecoes.ErroDeValidacao("Tenant e obrigatorio.", nameof(tenantId));
        TenantId = tenantId;
    }

    public Guid TenantId { get; private set; }
}
