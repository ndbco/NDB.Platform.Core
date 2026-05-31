using Mediator;

namespace NDB.Platform.Cqrs;

/// <summary>Marker for a query (read-only operation).</summary>
/// <typeparam name="TResponse">The response type of the query.</typeparam>
public interface IQuery<TResponse> : IRequest<TResponse> { }
