using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pata.Application.Comum.Abstracoes;
using Pata.Application.Funcionalidades.Tutor.Consultas;
using Pata.Application.Funcionalidades.Veterinario.Consultas;
using Pata.Application.Funcionalidades.Animal.Cadastro;
using Pata.Application.Funcionalidades.Animal.Portal;
using Pata.Domain.Repositorios;
using Pata.Infrastructure.Persistencia;
using Pata.Infrastructure.Persistencia.Consultas;
using Pata.Infrastructure.Persistencia.Repositorios;
using Pata.Infrastructure.Persistencia.Recursos;

namespace Pata.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' nao configurada.");

        services.AddSingleton(TimeProvider.System);
        services.AddDbContext<PataDbContext>(options =>
            options
                // O baseline foi escrito em SQL para preservar tabelas existentes e adicionar
                // tenant_id sem atribuir os dados legados a uma clínica. Como o snapshot
                // manual não representa os filtros dinâmicos por tenant do DbContext, o EF
                // detecta uma diferença de modelo que não altera o schema desta migration.
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()));
        services.AddScoped<IRepositorioTutor, RepositorioTutor>();
        services.AddScoped<IRepositorioVeterinario, RepositorioVeterinario>();
        services.AddScoped<IRepositorioConsulta, RepositorioConsulta>();
        services.AddScoped<IRepositorioOrganizacao, RepositorioOrganizacao>();
        services.AddScoped<IRepositorioConviteUsuario, RepositorioConviteUsuario>();
        services.AddScoped<IConsultaTutores, ConsultaTutores>();
        services.AddScoped<IConsultaVeterinarios, ConsultaVeterinarios>();
        services.AddScoped<RepositorioCadastroAnimal>();
        services.AddScoped<IRepositorioCadastroAnimal>(sp => sp.GetRequiredService<RepositorioCadastroAnimal>());
        services.AddScoped<IConsultaPortalTutor>(sp => sp.GetRequiredService<RepositorioCadastroAnimal>());
        services.AddScoped<IRelogio, Relogio>();
        return services;
    }
}

internal sealed class Relogio(TimeProvider timeProvider) : IRelogio
{
    public DateTime UtcAgora => timeProvider.GetUtcNow().UtcDateTime;
}
