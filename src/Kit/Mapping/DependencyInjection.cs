using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace NDB.Platform.Kit.Mapping;

/// <summary>Extension methods for registering Mapster services.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Mapster with auto-scanned profiles from the given assemblies.
    /// Also auto-scans and registers all <see cref="IServiceMapper"/> implementations as Scoped.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="assemblies">Assemblies to scan for IMapFrom/IMapTo/IMapObject and IServiceMapper implementations.</param>
    /// <exception cref="ArgumentException">Thrown if no assemblies are provided.</exception>
    public static IServiceCollection AddNdbMapping(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0)
            throw new ArgumentException("At least one assembly is required.", nameof(assemblies));

        // 1. Register Mapster TypeAdapterConfig
        var config = TypeAdapterConfig.GlobalSettings;
        AutoMappingProfile.Apply(config, assemblies);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        // 2. Auto-scan IServiceMapper implementations and register them
        foreach (var assembly in assemblies)
        {
            var mapperImpls = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false }
                            && typeof(IServiceMapper).IsAssignableFrom(t));

            foreach (var impl in mapperImpls)
            {
                // Register the implementation class as itself
                services.AddScoped(impl);

                // Register all interfaces extending IServiceMapper from the impl as Scoped (proxy to impl)
                var mapperInterfaces = impl.GetInterfaces()
                    .Where(i => i != typeof(IServiceMapper)
                                && typeof(IServiceMapper).IsAssignableFrom(i));

                foreach (var iface in mapperInterfaces)
                    services.AddScoped(iface, sp => sp.GetRequiredService(impl));
            }
        }

        return services;
    }
}
