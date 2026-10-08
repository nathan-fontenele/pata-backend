using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pata.Application.Comum.Abstracoes;
using Pata.Application.Funcionalidades.Animal.Portal;
using Pata.Domain.Entidades.Animal.Enum;
using Pata.Domain.Entidades.Tutor;
using Pata.Domain.ObjetosValor;
using Pata.Infrastructure.Tests.Apoio;
using Pata.Infrastructure.Persistencia;
using Pata.Infrastructure.Persistencia.Recursos;
using TutorAgregado = Pata.Domain.Entidades.Tutor.Tutor;

namespace Pata.Infrastructure.Tests.Persistencia;

public sealed class PortalTutorPostgresTests
{
    [PostgresFact]
    public async Task LinkDeUmaClinicaDeveMostrarTodosOsPetsDoTutorSomenteNaquelaClinica()
    {
        var connectionString = Environment.GetEnvironmentVariable("PATA_TEST_POSTGRES")!;
        var schema = $"pata_test_{Guid.NewGuid():N}";
        var adminConnection = new NpgsqlConnectionStringBuilder(connectionString) { SearchPath = "public" }.ConnectionString;
        await using (var connection = new NpgsqlConnection(adminConnection))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", connection);
            await command.ExecuteNonQueryAsync();
        }

        try
        {
            var isolatedConnection = new NpgsqlConnectionStringBuilder(connectionString) { SearchPath = schema }.ConnectionString;
            var tenantClinicaA = Guid.NewGuid();
            var tenantClinicaB = Guid.NewGuid();
            var hashA = new string('A', 64);
            var hashB = new string('B', 64);

            await CadastrarTutorESeusPets(isolatedConnection, tenantClinicaA, "clinica-a", hashA,
                ["Mel", "Toby"]);
            await CadastrarTutorESeusPets(isolatedConnection, tenantClinicaB, "clinica-b", hashB,
                ["Luna"]);

            var opcoesLeitura = new DbContextOptionsBuilder<PataDbContext>().UseNpgsql(isolatedConnection).Options;
            await using var contextoLeitura = new PataDbContext(opcoesLeitura, TimeProvider.System,
                new TenantFalso(tenantClinicaA), new UsuarioFalso());
            IConsultaPortalTutor consulta = new RepositorioCadastroAnimal(contextoLeitura,
                new TenantFalso(tenantClinicaA));

            var resultadoA = await consulta.ConsultarAsync(hashA, CancellationToken.None);
            var resultadoB = await consulta.ConsultarAsync(hashB, CancellationToken.None);

            Assert.NotNull(resultadoA);
            Assert.Equal(["Mel", "Toby"], resultadoA.Pets.Select(pet => pet.Nome).Order().ToArray());
            Assert.NotNull(resultadoB);
            Assert.Equal(["Luna"], resultadoB.Pets.Select(pet => pet.Nome).ToArray());
        }
        finally
        {
            await using var connection = new NpgsqlConnection(adminConnection);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE", connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    private static async Task CadastrarTutorESeusPets(string connectionString, Guid tenantId, string slug,
        string tokenHash, IReadOnlyList<string> pets)
    {
        var options = new DbContextOptionsBuilder<PataDbContext>().UseNpgsql(connectionString).Options;
        await using var contexto = new PataDbContext(options, TimeProvider.System,
            new TenantFalso(tenantId), new UsuarioFalso());
        await contexto.Database.EnsureCreatedAsync();

        var organizacao = new Pata.Domain.Entidades.Organizacao.Organizacao(
            $"Clinica {slug}", slug, "12345678000199", $"{slug}@exemplo.com");
        contexto.Organizacoes.Add(organizacao);
        await contexto.SaveChangesAsync();

        var tutor = new TutorAgregado(tenantId, "Maria Silva", new Cpf("52998224725"),
            new Email("maria@exemplo.com"), new Telefone("11912345678"));
        foreach (var nome in pets)
            tutor.AdicionarAnimal(nome, Especie.Cachorro, "Vira-lata", new DateOnly(2022, 1, 1), new DateOnly(2026, 10, 7));
        contexto.Tutores.Add(tutor);
        contexto.AcessosPortalTutor.Add(new AcessoPortalTutor(tenantId, tutor.Id, tokenHash,
            new DateTimeOffset(2026, 10, 7, 12, 0, 0, TimeSpan.Zero)));
        await contexto.SaveChangesAsync();
    }

    private sealed class TenantFalso(Guid id) : IContextoTenant
    {
        public Guid TenantId { get; } = id;
    }

    private sealed class UsuarioFalso : IUsuarioAtual
    {
        public string Identificador => "infra-tests";
        public string Perfil => "Teste";
        public IReadOnlyCollection<string> Papeis => ["admin"];
    }
}
