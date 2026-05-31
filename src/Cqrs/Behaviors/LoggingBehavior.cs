using Mediator;
using Microsoft.Extensions.Logging;

namespace NDB.Platform.Cqrs.Behaviors;

/// <summary>Pipeline behavior: logs the request name, duration, and result status.</summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed partial class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IMessage
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>Initializes LoggingBehavior.</summary>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) =>
        _logger = logger;

    /// <inheritdoc/>
    public async ValueTask<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TRequest, TResponse> next)
    {
        var name = typeof(TRequest).Name;
        LogHandling(_logger, name);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await next(request, cancellationToken);
        sw.Stop();
        LogHandled(_logger, name, sw.ElapsedMilliseconds);
        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {Request}")]
    private static partial void LogHandling(ILogger logger, string request);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {Request} in {Elapsed}ms")]
    private static partial void LogHandled(ILogger logger, string request, long elapsed);
}
