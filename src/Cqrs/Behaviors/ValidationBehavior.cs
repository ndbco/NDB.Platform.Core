using FluentValidation;
using Mediator;
using NDB.Platform.Abstraction;

namespace NDB.Platform.Cqrs.Behaviors;

/// <summary>
/// Pipeline behavior: runs FluentValidation before the handler.
/// If validation fails and TResponse is Result&lt;T&gt;, returns Result.Validation.
/// If TResponse is not Result&lt;T&gt;, throws a ValidationException.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IMessage
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>Initializes ValidationBehavior.</summary>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) =>
        _validators = validators;

    /// <inheritdoc/>
    public async ValueTask<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TRequest, TResponse> next)
    {
        if (!_validators.Any())
            return await next(request, cancellationToken);

        var ctx = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(ctx))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next(request, cancellationToken);

        var errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

        // Attempt to return Result<T>.Validation if TResponse is Result<T>
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var method = responseType.GetMethod(
                nameof(Result<object>.Validation),
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            if (method is not null)
            {
                var result = method.Invoke(null, new object[] { errors });
                return (TResponse)result!;
            }
        }

        throw new ValidationException(failures);
    }
}
