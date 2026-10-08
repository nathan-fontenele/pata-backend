using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pata.Domain.Entidades.Organizacao;

namespace Pata.Infrastructure.Persistencia.Configuracoes;

internal sealed class UsuarioConfiguracao : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(item => item.Id);
        builder.MapearTenant();
        builder.HasAlternateKey(item => new { item.Id, item.TenantId });
        builder.HasOne<Equipe>().WithMany()
            .HasForeignKey(item => new { item.EquipeId, item.TenantId })
            .HasPrincipalKey(item => new { item.Id, item.TenantId })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.TenantId, item.Email }).IsUnique();
        builder.Property(item => item.Nome).HasMaxLength(100).IsRequired();
        builder.Property(item => item.Sobrenome).HasMaxLength(100).IsRequired();
        builder.Property(item => item.Email).HasMaxLength(254).IsRequired();
        builder.Property(item => item.Telefone).HasMaxLength(30);
        builder.Property(item => item.Cargo).HasMaxLength(100);
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
    }
}
