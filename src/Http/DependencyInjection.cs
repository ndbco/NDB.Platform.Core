using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NDB.Platform.Http.Handlers;

namespace NDB.Platform.Http;

/// <summary>Extension methods for registering Http services with resilience.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers a typed HttpClient with resilience (retry, timeout),
    /// CorrelationIdHandler, and PiiScrubLoggingHandler.
    /// </summary>
    /// <typeparam name="TClient">The typed client interface.</typeparam>
    /// <typeparam name="TImplementation">The typed client implementation.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="name">Logical name of the HttpClient.</param>
    /// <param name="configure">Optional HttpClient configuration action (base address, etc.).</param>
    /// <returns>IHttpClientBuilder for further configuration.</returns>
    public static IHttpClientBuilder AddNdbHttp<TClient, TImplementation>(
        this IServiceCollection services,
        string name,
        Action<HttpClient>? configure = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        var builder = services.AddHttpClient<TClient, TImplementation>(name, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            configure?.Invoke(client);
        });

        builder
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<PiiScrubLoggingHandler>()
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
            });

        services.TryAddTransient<CorrelationIdHandler>();
        services.TryAddTransient<PiiScrubLoggingHandler>();
        return builder;
    }
}
