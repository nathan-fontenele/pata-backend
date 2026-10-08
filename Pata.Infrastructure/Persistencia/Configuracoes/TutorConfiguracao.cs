using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pata.Domain.Entidades.Tutor;
using Pata.Domain.ObjetosValor;

namespace Pata.Infrastructure.Persistencia.Configuracoes;

internal sealed class TutorConfiguracao : IEntityTypeConfiguration<Tutor>
{
    public void Configure(EntityTypeBuilder<Tutor> builder)
    {
        builder.ToTable("tutores");
        builder.HasKey(tutor => tutor.Id);
        builder.MapearTenant();
        builder.HasAlternateKey(tutor => new { tutor.Id, tutor.TenantId });
        builder.Property(tutor => tutor.Nome).HasMaxLength(150).IsRequired();
        builder.Property(tutor => tutor.Cpf)
            .HasConversion(cpf => cpf.Valor, valor => new Cpf(valor))
            .HasMaxLength(11).IsRequired();
        builder.HasIndex(tutor => new { tutor.TenantId, tutor.Cpf }).IsUnique();
        builder.Property(tutor => tutor.Email)
            .HasConversion(email => email.Valor, valor => new Email(valor))
            .HasMaxLength(254).IsRequired();
        builder.Property(tutor => tutor.Telefone)
            .HasConversion(telefone => telefone.Valor, valor => new Telefone(valor))
            .HasMaxLength(11).IsRequired();
        ConfigurarAuditoria(builder);

        builder.HasMany(tutor => tutor.Animais)
            .WithOne()
            .HasForeignKey(animal => new { animal.TutorId, animal.TenantId })
            .HasPrincipalKey(tutor => new { tutor.Id, tutor.TenantId })
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(tutor => tutor.Animais).HasField("_animais").UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    internal static void ConfigurarAuditoria<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.Property<DateTime>("CriadoEm").IsRequired();
        builder.Property<string>("CriadoPor").HasMaxLength(150).IsRequired();
        builder.Property<DateTime?>("AtualizadoEm");
        builder.Property<string?>("AtualizadoPor").HasMaxLength(150);
        builder.Property<bool>("Excluido").IsRequired();
        builder.Property<DateTime?>("ExcluidoEm");
        builder.Property<string?>("ExcluidoPor").HasMaxLength(150);
    }
}
