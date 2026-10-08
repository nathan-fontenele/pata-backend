using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pata.Domain.Entidades.Auditoria;

namespace Pata.Infrastructure.Persistencia.Configuracoes;

internal sealed class AuditoriaConfiguracao : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> builder)
    {
        builder.ToTable("auditorias");
        builder.HasKey(item => item.Id);
        builder.MapearTenant();
        builder.HasIndex(item => new { item.TenantId, item.EntidadeTipo, item.EntidadeId, item.CriadoEm });
        builder.Property(item => item.EntidadeTipo).HasMaxLength(150).IsRequired();
        builder.Property(item => item.Acao).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(item => item.CriadoPor).HasMaxLength(150).IsRequired();
        builder.Property(item => item.EditadoPor).HasMaxLength(150);
        builder.Property(item => item.ExcluidoPor).HasMaxLength(150);
        builder.Property(item => item.EntidadeAntes).HasColumnType("jsonb");
        builder.Property(item => item.EntidadeDepois).HasColumnType("jsonb");
    }
}
