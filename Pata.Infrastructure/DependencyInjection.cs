using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pata.Application.Comum.Abstracoes;
using Pata.Application.Funcionalidades.Tutor.Consultas;
using Pata.Application.Funcionalidades.Veterinario.Consultas;
using Pata.Domain.Repositorios;
using Pata.Infrastructure.Persistencia;
using Pata.Infrastructure.Persistencia.Consultas;
using Pata.Infrastructure.Persistencia.Repositorios;

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
            options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()));
        services.AddScoped<IRepositorioTutor, RepositorioTutor>();
        services.AddScoped<IRepositorioVeterinario, RepositorioVeterinario>();
        services.AddScoped<IRepositorioConsulta, RepositorioConsulta>();
        services.AddScoped<IConsultaTutores, ConsultaTutores>();
        services.AddScoped<IConsultaVeterinarios, ConsultaVeterinarios>();
        services.AddScoped<IRelogio, Relogio>();
        services.AddScoped<IUsuarioAtual, UsuarioAtualSistema>();
        return services;
    }
}

internal sealed class Relogio(TimeProvider timeProvider) : IRelogio
{
    public DateTime UtcAgora => timeProvider.GetUtcNow().UtcDateTime;
}

internal sealed class UsuarioAtualSistema : IUsuarioAtual
{
    public string Identificador => "sistema";
    public string Perfil => "sistema";
    public IReadOnlyCollection<string> Papeis => ["sistema"];
}
