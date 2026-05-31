using Mediator;

namespace NDB.Platform.Cqrs;

/// <summary>Marker for a command with no response (void equivalent).</summary>
public interface ICommand : IRequest { }

/// <summary>Marker for a command with a response.</summary>
/// <typeparam name="TResponse">The response type of the command.</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse> { }
