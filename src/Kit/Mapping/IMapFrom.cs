namespace NDB.Platform.Kit.Mapping;

/// <summary>
/// Implement this interface on a DTO to register a mapping from
/// <typeparamref name="TSource"/> using Mapster auto-scan.
/// </summary>
/// <typeparam name="TSource">The source type for the mapping.</typeparam>
public interface IMapFrom<TSource> { }
