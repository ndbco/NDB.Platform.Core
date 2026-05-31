namespace NDB.Platform.Kit.Mapping;

/// <summary>
/// Marker interface for Mapster service mappers.
/// Implementations are automatically registered as Scoped via <see cref="DependencyInjection.AddNdbMapping"/>.
/// </summary>
/// <remarks>
/// Usage pattern:
/// <code>
/// public interface IUserMapper : IServiceMapper
/// {
///     UserDetailResponse SetDetail(SetUser d);
/// }
///
/// public partial class UserMapper : IUserMapper
/// {
///     public UserDetailResponse SetDetail(SetUser d) =&gt; d.Adapt&lt;UserDetailResponse&gt;();
/// }
/// </code>
/// Inject in a handler via constructor injection of <c>IUserMapper</c>, then call <c>_mapper.SetDetail(entity)</c>.
/// </remarks>
public interface IServiceMapper { }
