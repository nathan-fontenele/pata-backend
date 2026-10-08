using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pata.Infrastructure.Persistencia.Migrations;

[DbContext(typeof(PataDbContext))]
[Migration("202610080001_InitialSchema")]
public sealed class InitialSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        const string resourceName =
            "Pata.Infrastructure.Persistencia.Migrations.202610080001_InitialSchema.sql";
        using var stream = typeof(InitialSchema).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Recurso de migration nao encontrado: {resourceName}.");
        using var reader = new StreamReader(stream);
        migrationBuilder.Sql(reader.ReadToEnd());
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException("Esta migration preserva tabelas e dados existentes e nao pode ser revertida destrutivamente.");

    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PataDbContext).Assembly);
}
