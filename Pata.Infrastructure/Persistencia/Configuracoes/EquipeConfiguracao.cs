using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pata.Domain.Entidades.Organizacao;

namespace Pata.Infrastructure.Persistencia.Configuracoes;

internal sealed class EquipeConfiguracao : IEntityTypeConfiguration<Equipe>
{
    public void Configure(EntityTypeBuilder<Equipe> builder)
    {
        builder.ToTable("equipes");
        builder.HasKey(item => item.Id);
        builder.MapearTenant();
        builder.HasAlternateKey(item => new { item.Id, item.TenantId });
        builder.HasOne<Organizacao>().WithMany()
            .HasForeignKey(item => new { item.OrganizacaoId, item.TenantId })
            .HasPrincipalKey(item => new { item.Id, item.TenantId })
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(item => item.Nome).HasMaxLength(120).IsRequired();
        builder.Property(item => item.Descricao).HasMaxLength(500);
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
    }
}
