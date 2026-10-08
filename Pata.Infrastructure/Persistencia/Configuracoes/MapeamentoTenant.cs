using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pata.Domain.Comum;

namespace Pata.Infrastructure.Persistencia.Configuracoes;

internal static class MapeamentoTenant
{
    public static void MapearTenant<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, ITenantEntity =>
        builder.Property<Guid>(nameof(ITenantEntity.TenantId)).HasColumnName("tenant_id").IsRequired();
}
