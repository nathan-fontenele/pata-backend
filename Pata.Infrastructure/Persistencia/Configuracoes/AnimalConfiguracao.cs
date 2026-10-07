using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pata.Domain.Entidades.Animal;

namespace Pata.Infrastructure.Persistencia.Configuracoes;

internal sealed class AnimalConfiguracao : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        builder.ToTable("animais");
        builder.HasKey(animal => animal.Id);
        builder.Property(animal => animal.Nome).HasMaxLength(100).IsRequired();
        builder.Property(animal => animal.Raca).HasMaxLength(100).IsRequired();
        builder.Property(animal => animal.Especie).HasConversion<string>().HasMaxLength(30);
    }
}
