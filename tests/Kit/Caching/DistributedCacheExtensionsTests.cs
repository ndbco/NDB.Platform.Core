using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using NDB.Platform.Kit.Caching;
using NSubstitute;
using Xunit;
using NdbCacheExt = NDB.Platform.Kit.Caching.DistributedCacheExtensions;

namespace NDB.Platform.Tests.Kit.Caching;

public sealed class DistributedCacheExtensionsTests
{
    private static readonly System.Text.Json.JsonSerializerOptions WebJsonOptions =
        new(System.Text.Json.JsonSerializerDefaults.Web);

    // ── GetOrSetAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrSetAsync_CacheMiss_ShouldCallFactory()
    {
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync("key", Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        var factoryCalled = false;

        var result = await cache.GetOrSetAsync<string>(
            "key",
            () => { factoryCalled = true; return Task.FromResult<string?>("value"); },
            NdbCacheExt.MasterDataOptions());

        factoryCalled.Should().BeTrue();
        result.Should().Be("value");
        await cache.Received(1).SetAsync("key", Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrSetAsync_CacheHit_ShouldNotCallFactory()
    {
        var cache = Substitute.For<IDistributedCache>();
        // Serialize "cached" into JSON bytes
        var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes("cached", WebJsonOptions);
        cache.GetAsync("key", Arg.Any<CancellationToken>()).Returns(bytes);
        var factoryCalled = false;

        var result = await cache.GetOrSetAsync<string>(
            "key",
            () => { factoryCalled = true; return Task.FromResult<string?>("new"); },
            NdbCacheExt.MasterDataOptions());

        factoryCalled.Should().BeFalse();
        result.Should().Be("cached");
        await cache.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrSetAsync_FactoryReturnsNull_ShouldNotCache()
    {
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync("key", Arg.Any<CancellationToken>()).Returns((byte[]?)null);

        var result = await cache.GetOrSetAsync<string>(
            "key",
            () => Task.FromResult<string?>(null),
            NdbCacheExt.MasterDataOptions());

        result.Should().BeNull();
        await cache.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrSetAsync_CancellationRequested_ShouldThrow()
    {
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync("key", Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await cache.GetOrSetAsync<string>(
            "key",
            () => Task.FromResult<string?>("value"),
            NdbCacheExt.MasterDataOptions(),
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetOrSetAsync_NullCache_ShouldThrowArgumentNullException()
    {
        IDistributedCache? nullCache = null;
        var act = async () => await nullCache!.GetOrSetAsync<string>(
            "key",
            () => Task.FromResult<string?>("v"),
            NdbCacheExt.MasterDataOptions());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetOrSetAsync_NullFactory_ShouldThrowArgumentNullException()
    {
        var cache = Substitute.For<IDistributedCache>();
        var act = async () => await cache.GetOrSetAsync<string>(
            "key",
            null!,
            NdbCacheExt.MasterDataOptions());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── TTL Presets ──────────────────────────────────────────────────────────

    [Fact]
    public void MasterDataOptions_ShouldHave15MinuteTtl()
    {
        var opts = NdbCacheExt.MasterDataOptions();
        opts.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void ReferenceDataOptions_ShouldHave30MinuteTtl()
    {
        var opts = NdbCacheExt.ReferenceDataOptions();
        opts.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public void ShortLivedOptions_ShouldHave5MinuteTtl()
    {
        var opts = NdbCacheExt.ShortLivedOptions();
        opts.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(5));
    }
}
