namespace NDB.Platform.Kit.Mapping;

/// <summary>
/// Implement this interface to register a two-way mapping between
/// <typeparamref name="TSource"/> and <typeparamref name="TTarget"/> using Mapster auto-scan.
/// </summary>
/// <typeparam name="TSource">The first type.</typeparam>
/// <typeparam name="TTarget">The second type.</typeparam>
public interface IMapObject<TSource, TTarget> { }
