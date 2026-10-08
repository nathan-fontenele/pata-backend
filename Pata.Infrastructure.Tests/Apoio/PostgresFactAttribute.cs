namespace Pata.Infrastructure.Tests.Apoio;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PATA_TEST_POSTGRES")))
            Skip = "Configure PATA_TEST_POSTGRES para executar este teste de integração PostgreSQL.";
    }
}
