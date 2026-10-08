using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pata.Domain.Entidades.Cobranca;
using Pata.Domain.Entidades.Organizacao;

namespace Pata.Infrastructure.Persistencia.Configuracoes;

internal sealed class PlanoConfiguracao : IEntityTypeConfiguration<Plano>
{
    public void Configure(EntityTypeBuilder<Plano> builder)
    {
        builder.ToTable("planos");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => item.Codigo).IsUnique();
        builder.Property(item => item.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(item => item.Nome).HasMaxLength(120).IsRequired();
        builder.Property(item => item.Descricao).HasMaxLength(2000);
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
    }
}

internal sealed class PrecoPlanoConfiguracao : IEntityTypeConfiguration<PrecoPlano>
{
    public void Configure(EntityTypeBuilder<PrecoPlano> builder)
    {
        builder.ToTable("precos_plano");
        builder.HasKey(item => item.Id);
        builder.HasOne<Plano>().WithMany().HasForeignKey(item => item.PlanoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => item.Codigo).IsUnique();
        builder.Property(item => item.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(item => item.Valor).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.Moeda).HasMaxLength(3).IsRequired();
        builder.Property(item => item.IntervaloUnidade).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
    }
}

internal sealed class AssinaturaConfiguracao : IEntityTypeConfiguration<Assinatura>
{
    public void Configure(EntityTypeBuilder<Assinatura> builder)
    {
        builder.ToTable("assinaturas");
        builder.HasKey(item => item.Id);
        builder.MapearTenant();
        builder.HasAlternateKey(item => new { item.Id, item.TenantId });
        builder.HasOne<Organizacao>().WithMany()
            .HasForeignKey(item => new { item.OrganizacaoId, item.TenantId })
            .HasPrincipalKey(item => new { item.Id, item.TenantId }).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PrecoPlano>().WithMany().HasForeignKey(item => item.PrecoPlanoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.TenantId, item.Status });
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
    }
}

internal sealed class FaturaConfiguracao : IEntityTypeConfiguration<Fatura>
{
    public void Configure(EntityTypeBuilder<Fatura> builder)
    {
        builder.ToTable("faturas");
        builder.HasKey(item => item.Id);
        builder.MapearTenant();
        builder.HasAlternateKey(item => new { item.Id, item.TenantId });
        builder.HasOne<Assinatura>().WithMany()
            .HasForeignKey(item => new { item.AssinaturaId, item.TenantId })
            .HasPrincipalKey(item => new { item.Id, item.TenantId }).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.TenantId, item.Numero }).IsUnique();
        builder.Property(item => item.Numero).HasMaxLength(80).IsRequired();
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(item => item.Moeda).HasMaxLength(3).IsRequired();
        builder.Property(item => item.Subtotal).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.DescontoTotal).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.Total).HasPrecision(18, 2).IsRequired();
        builder.HasMany(item => item.Itens).WithOne().HasForeignKey(item => new { item.FaturaId, item.TenantId })
            .HasPrincipalKey(item => new { item.Id, item.TenantId }).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(item => item.Itens).HasField("_itens").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ItemFaturaConfiguracao : IEntityTypeConfiguration<ItemFatura>
{
    public void Configure(EntityTypeBuilder<ItemFatura> builder)
    {
        builder.ToTable("itens_fatura");
        builder.HasKey(item => item.Id);
        builder.MapearTenant();
        builder.HasOne<PrecoPlano>().WithMany().HasForeignKey(item => item.PrecoPlanoId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(item => item.Descricao).HasMaxLength(500).IsRequired();
        builder.Property(item => item.Quantidade).HasPrecision(18, 4).IsRequired();
        builder.Property(item => item.ValorUnitario).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.Desconto).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.Total).HasPrecision(18, 2).IsRequired();
    }
}

internal sealed class PagamentoConfiguracao : IEntityTypeConfiguration<Pagamento>
{
    public void Configure(EntityTypeBuilder<Pagamento> builder)
    {
        builder.ToTable("pagamentos");
        builder.HasKey(item => item.Id);
        builder.MapearTenant();
        builder.HasOne<Fatura>().WithMany()
            .HasForeignKey(item => new { item.FaturaId, item.TenantId })
            .HasPrincipalKey(item => new { item.Id, item.TenantId }).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.TenantId, item.ChaveIdempotencia }).IsUnique();
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(item => item.Valor).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.Moeda).HasMaxLength(3).IsRequired();
        builder.Property(item => item.Metodo).HasMaxLength(50).IsRequired();
        builder.Property(item => item.Provedor).HasMaxLength(80).IsRequired();
        builder.Property(item => item.ReferenciaExterna).HasMaxLength(200);
        builder.Property(item => item.ChaveIdempotencia).HasMaxLength(120).IsRequired();
        builder.Property(item => item.CodigoFalha).HasMaxLength(100);
        builder.Property(item => item.MensagemFalha).HasMaxLength(1000);
    }
}
