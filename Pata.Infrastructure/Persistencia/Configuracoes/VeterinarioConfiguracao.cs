using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pata.Domain.Entidades.Veterinario;
using Pata.Domain.ObjetosValor;

namespace Pata.Infrastructure.Persistencia.Configuracoes;

internal sealed class VeterinarioConfiguracao : IEntityTypeConfiguration<Veterinario>
{
    public void Configure(EntityTypeBuilder<Veterinario> builder)
    {
        builder.ToTable("veterinarios");
        builder.HasKey(veterinario => veterinario.Id);
        builder.Property(veterinario => veterinario.Nome).HasMaxLength(Veterinario.TamanhoMaximoNome).IsRequired();
        builder.Property(veterinario => veterinario.Email)
            .HasConversion(email => email.Valor, valor => new Email(valor))
            .HasMaxLength(254).IsRequired();
        builder.Property(veterinario => veterinario.Telefone)
            .HasConversion(telefone => telefone.Valor, valor => new Telefone(valor))
            .HasMaxLength(11).IsRequired();
        builder.Property(veterinario => veterinario.Crmv)
            .HasConversion(crmv => crmv.Valor, valor => new Crmv(valor.Split('/')[0], valor.Split('/')[1]))
            .HasMaxLength(9).IsRequired();
        builder.HasIndex(veterinario => veterinario.Crmv).IsUnique();
        builder.Property(veterinario => veterinario.Especialidade)
            .HasMaxLength(Veterinario.TamanhoMaximoEspecialidade).IsRequired();
        TutorConfiguracao.ConfigurarAuditoria(builder);
    }
}
