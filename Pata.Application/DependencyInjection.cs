using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Pata.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] additionalAssemblies)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            foreach (var assembly in additionalAssemblies)
                config.RegisterServicesFromAssembly(assembly);
        });
        return services;
    }
}
