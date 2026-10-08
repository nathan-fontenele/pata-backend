namespace Pata.Application.Comum.Abstracoes;

public interface IContextoTenant
{
    Guid TenantId { get; }
}
