using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pata.Infrastructure.Persistencia.Migrations;

[DbContext(typeof(PataDbContext))]
[Migration("202610080002_Auth0Admin")]
public sealed class Auth0Admin : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        const string resourceName = "Pata.Infrastructure.Persistencia.Migrations.202610080002_Auth0Admin.sql";
        using var stream = typeof(Auth0Admin).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Recurso de migration nao encontrado: {resourceName}.");
        using var reader = new StreamReader(stream);
        migrationBuilder.Sql(reader.ReadToEnd());
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException("Esta migration adiciona vinculos Auth0 sem remover dados existentes.");

    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PataDbContext).Assembly);
}
