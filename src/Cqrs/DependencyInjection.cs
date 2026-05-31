using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NDB.Platform.Abstraction;
using NDB.Platform.Cqrs.Behaviors;
using NDB.Platform.Kit.Identifiers;
using System.Reflection;

namespace NDB.Platform.Cqrs;

/// <summary>Extension methods for registering CQRS services.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers FluentValidation validators and pipeline behaviors
    /// (LoggingBehavior, ValidationBehavior) for CQRS.
    /// Also registers <see cref="SystemActorAccessor"/> as a fallback <see cref="IActorAccessor"/>
    /// (can be overridden by <c>AddNdbJwt()</c> which registers <c>HttpContextActorAccessor</c>).
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="assemblies">Assemblies to scan for handlers and validators.</param>
    /// <remarks>
    /// <c>services.AddMediator()</c> must be called separately in the entry-point project
    /// (using the Mediator.SourceGenerator installed in that project) so that the handler
    /// dispatch table includes all handlers from all referenced assemblies.
    /// </remarks>
    public static IServiceCollection AddNdbCqrs(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // AddMediator() is NOT called here — the consuming project is responsible for
        // calling services.AddMediator() after AddNdbCqrs(), using the
        // Mediator.SourceGenerator installed in the entry-point project.
        // This ensures the dispatch table covers all public handlers from all assemblies.
        services.AddValidatorsFromAssemblies(assemblies, ServiceLifetime.Scoped);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register SystemActorAccessor as fallback (TryAdd = does not override if already registered)
        // Web projects: AddNdbJwt() will override with HttpContextActorAccessor (Scoped)
        services.TryAddSingleton<IActorAccessor, SystemActorAccessor>();

        return services;
    }
}
