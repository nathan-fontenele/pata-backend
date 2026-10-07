using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pata.Domain.Entidades.Consulta;
using Pata.Domain.Entidades.Prontuario;

namespace Pata.Infrastructure.Persistencia.Configuracoes;

internal sealed class ConsultaConfiguracao : IEntityTypeConfiguration<Consulta>
{
    public void Configure(EntityTypeBuilder<Consulta> builder)
    {
        builder.ToTable("consultas");
        builder.HasKey(consulta => consulta.Id);
        builder.Property(consulta => consulta.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(consulta => consulta.DataHora).IsRequired();
        builder.HasIndex(consulta => new { consulta.VeterinarioId, consulta.DataHora });

        builder.OwnsOne(consulta => consulta.Prontuario, prontuario =>
        {
            prontuario.ToTable("prontuarios");
            prontuario.WithOwner().HasForeignKey(nameof(Prontuario.ConsultaId));
            prontuario.HasKey(nameof(Prontuario.Id));
            prontuario.Property(item => item.Diagnostico).HasMaxLength(2000);
            prontuario.Property(item => item.Prescricao).HasMaxLength(4000);
            prontuario.OwnsMany(item => item.Sintomas, sintomas =>
            {
                sintomas.ToTable("sintomas");
                sintomas.WithOwner().HasForeignKey("ProntuarioId");
                sintomas.Property<int>("Id");
                sintomas.HasKey("Id");
                sintomas.Property(sintoma => sintoma.Descricao).HasMaxLength(Sintoma.TamanhoMaximoDescricao).IsRequired();
            });
            prontuario.Navigation(item => item.Sintomas).HasField("_sintomas").UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}
