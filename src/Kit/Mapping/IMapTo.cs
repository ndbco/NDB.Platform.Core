namespace NDB.Platform.Kit.Mapping;

/// <summary>
/// Implement this interface on a type to register a mapping to
/// <typeparamref name="TTarget"/> using Mapster auto-scan.
/// </summary>
/// <typeparam name="TTarget">The target type for the mapping.</typeparam>
public interface IMapTo<TTarget> { }
