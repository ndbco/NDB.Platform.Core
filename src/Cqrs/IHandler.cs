using Mediator;

namespace NDB.Platform.Cqrs;

/// <summary>Handler alias for a command with no response.</summary>
/// <typeparam name="TCommand">The command type handled.</typeparam>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand { }

/// <summary>Handler alias for a command with a response.</summary>
/// <typeparam name="TCommand">The command type handled.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse> { }

/// <summary>Handler alias for a query.</summary>
/// <typeparam name="TQuery">The query type handled.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse> { }
