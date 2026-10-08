using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pata.Domain.Entidades.Organizacao;

namespace Pata.Infrastructure.Persistencia.Configuracoes;

internal sealed class ConviteUsuarioConfiguracao : IEntityTypeConfiguration<ConviteUsuario>
{
    public void Configure(EntityTypeBuilder<ConviteUsuario> builder)
    {
        builder.ToTable("convites_usuario");
        builder.HasKey(item => item.Id);
        builder.MapearTenant();
        builder.HasOne<Usuario>().WithMany()
            .HasForeignKey(item => new { item.UsuarioId, item.TenantId })
            .HasPrincipalKey(item => new { item.Id, item.TenantId })
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Usuario>().WithMany()
            .HasForeignKey(item => new { item.ConvidadoPorUsuarioId, item.TenantId })
            .HasPrincipalKey(item => new { item.Id, item.TenantId })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => item.TokenHash).IsUnique();
        builder.Property(item => item.TokenHash).HasMaxLength(256).IsRequired();
        builder.Property(item => item.ExpiraEm).IsRequired();
    }
}
