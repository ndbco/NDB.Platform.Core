using IdGen;

namespace NDB.Platform.Kit.Identifiers;

/// <summary>ID generation utilities: Snowflake, ULID, NanoId.</summary>
public static class IdGenerator
{
    // Snowflake — generatorId 0, epoch = 2024-01-01
    private static readonly IdGen.IdGenerator SnowflakeGenerator = new(
        0,
        new IdGeneratorOptions(
            timeSource: new DefaultTimeSource(
                new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc))));

    /// <summary>Generates a Snowflake ID (64-bit long, monotonically increasing).</summary>
    public static long NewSnowflake() => SnowflakeGenerator.CreateId();

    /// <summary>Generates a ULID (Universally Unique Lexicographically Sortable Identifier).</summary>
    public static Ulid NewUlid() => Ulid.NewUlid();

    /// <summary>Generates a URL-friendly NanoId random identifier.</summary>
    /// <param name="size">Length of the ID. Default: 21.</param>
    public static string NewNanoId(int size = 21) => NanoidDotNet.Nanoid.Generate(size: size);
}
