using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pata.Domain.Entidades.Tutor;

namespace Pata.Infrastructure.Persistencia.Configuracoes;

internal sealed class AcessoPortalTutorConfiguracao : IEntityTypeConfiguration<AcessoPortalTutor>
{
    public void Configure(EntityTypeBuilder<AcessoPortalTutor> builder)
    {
        builder.ToTable("acessos_portal_tutor");
        builder.HasKey(item => item.Id);
        builder.MapearTenant();
        builder.HasOne<Tutor>().WithMany()
            .HasForeignKey(item => new { item.TutorId, item.TenantId })
            .HasPrincipalKey(item => new { item.Id, item.TenantId })
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(item => new { item.TenantId, item.TutorId }).IsUnique();
        builder.HasIndex(item => item.TokenHash).IsUnique();
        builder.Property(item => item.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(item => item.CriadoEm).IsRequired();
    }
}
