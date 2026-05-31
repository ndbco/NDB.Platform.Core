using Mapster;
using System.Reflection;

namespace NDB.Platform.Kit.Mapping;

/// <summary>
/// Scans assemblies and registers Mapster configuration based on implementations of
/// IMapFrom&lt;T&gt;, IMapTo&lt;T&gt;, and IMapObject&lt;S,T&gt;.
/// </summary>
public static class AutoMappingProfile
{
    /// <summary>
    /// Scans the given assemblies and applies TypeAdapterConfig entries based on marker interfaces.
    /// </summary>
    /// <param name="config">The Mapster TypeAdapterConfig to populate.</param>
    /// <param name="assemblies">Assemblies to scan.</param>
    public static void Apply(TypeAdapterConfig config, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetExportedTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .ToList();

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .ToList();

                foreach (var iface in interfaces)
                {
                    var genericDef = iface.GetGenericTypeDefinition();
                    var args = iface.GetGenericArguments();

                    if (genericDef == typeof(IMapFrom<>) && args.Length == 1)
                    {
                        // IMapFrom<TSource> on type → map: TSource → type
                        config.NewConfig(args[0], type);
                    }
                    else if (genericDef == typeof(IMapTo<>) && args.Length == 1)
                    {
                        // IMapTo<TTarget> on type → map: type → TTarget
                        config.NewConfig(type, args[0]);
                    }
                    else if (genericDef == typeof(IMapObject<,>) && args.Length == 2)
                    {
                        // IMapObject<TSource,TTarget> → two-way mapping
                        config.NewConfig(args[0], args[1]);
                        config.NewConfig(args[1], args[0]);
                    }
                }
            }
        }
    }
}
