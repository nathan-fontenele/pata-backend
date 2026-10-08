namespace Pata.Application.Comum.Abstracoes;

public interface IContextoTenantDefinivel : IContextoTenant
{
    void DefinirTenant(Guid tenantId);
}
