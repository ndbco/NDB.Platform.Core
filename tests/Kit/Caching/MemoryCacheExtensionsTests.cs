using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NDB.Platform.Kit.Caching;
using Xunit;

namespace NDB.Platform.Tests.Kit.Caching;

public sealed class MemoryCacheExtensionsTests : IDisposable
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    [Fact]
    public async Task GetOrSetAsync_CacheMiss_ShouldCallFactory()
    {
        var factoryCalled = false;
        var result = await _cache.GetOrSetAsync<string>(
            "key",
            () =>
            {
                factoryCalled = true;
                return Task.FromResult<string?>("value");
            },
            CacheEntryDefaults.DefaultMasterDataOptions());

        factoryCalled.Should().BeTrue();
        result.Should().Be("value");
    }

    [Fact]
    public async Task GetOrSetAsync_CacheHit_ShouldNotCallFactory()
    {
        _cache.Set("key", "cached");
        var factoryCalled = false;

        var result = await _cache.GetOrSetAsync<string>(
            "key",
            () =>
            {
                factoryCalled = true;
                return Task.FromResult<string?>("new-value");
            },
            CacheEntryDefaults.DefaultMasterDataOptions());

        factoryCalled.Should().BeFalse();
        result.Should().Be("cached");
    }

    [Fact]
    public async Task GetOrSetAsync_FactoryReturnsNull_ShouldNotCache()
    {
        var callCount = 0;
        await _cache.GetOrSetAsync<string>(
            "key",
            () =>
            {
                callCount++;
                return Task.FromResult<string?>(null);
            },
            CacheEntryDefaults.DefaultMasterDataOptions());

        // Second call — factory should be called again because null was not cached
        await _cache.GetOrSetAsync<string>(
            "key",
            () =>
            {
                callCount++;
                return Task.FromResult<string?>(null);
            },
            CacheEntryDefaults.DefaultMasterDataOptions());

        callCount.Should().Be(2);
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldCacheResult()
    {
        var callCount = 0;
        await _cache.GetOrSetAsync<string>(
            "key",
            () =>
            {
                callCount++;
                return Task.FromResult<string?>("value");
            },
            CacheEntryDefaults.DefaultMasterDataOptions());

        // Second call — should use cache
        await _cache.GetOrSetAsync<string>(
            "key",
            () =>
            {
                callCount++;
                return Task.FromResult<string?>("value");
            },
            CacheEntryDefaults.DefaultMasterDataOptions());

        callCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOrSetAsync_DefaultOverload_ShouldUseMasterDataOptions()
    {
        var result = await _cache.GetOrSetAsync<string>(
            "str-key",
            () => Task.FromResult<string?>("hello-42"));

        result.Should().Be("hello-42");
    }

    [Fact]
    public async Task GetOrSetAsync_CancellationToken_ShouldRespectCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await _cache.GetOrSetAsync<string>(
            "key",
            () => Task.FromResult<string?>("value"),
            CacheEntryDefaults.DefaultMasterDataOptions(),
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetOrSetAsync_NullCache_ShouldThrowArgumentNullException()
    {
        MemoryCache? nullCache = null;
        var act = async () => await nullCache!.GetOrSetAsync<string>(
            "key",
            () => Task.FromResult<string?>("value"),
            CacheEntryDefaults.DefaultMasterDataOptions());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetOrSetAsync_NullFactory_ShouldThrowArgumentNullException()
    {
        var act = async () => await _cache.GetOrSetAsync<string>(
            "key",
            null!,
            CacheEntryDefaults.DefaultMasterDataOptions());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    public void Dispose() => _cache.Dispose();
}
