using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pata.Domain.Entidades.Organizacao;

namespace Pata.Infrastructure.Persistencia.Configuracoes;

internal sealed class OrganizacaoConfiguracao : IEntityTypeConfiguration<Organizacao>
{
    public void Configure(EntityTypeBuilder<Organizacao> builder)
    {
        builder.ToTable("organizacoes");
        builder.HasKey(item => item.Id);
        builder.MapearTenant();
        builder.HasAlternateKey(item => new { item.Id, item.TenantId });
        builder.HasIndex(item => item.Slug).IsUnique();
        builder.Property(item => item.Nome).HasMaxLength(150).IsRequired();
        builder.Property(item => item.Slug).HasMaxLength(80).IsRequired();
        builder.Property(item => item.Cnpj).HasMaxLength(14).IsRequired();
        builder.Property(item => item.Email).HasMaxLength(254).IsRequired();
        builder.Property(item => item.LogoUrl).HasMaxLength(2048);
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
    }
}
