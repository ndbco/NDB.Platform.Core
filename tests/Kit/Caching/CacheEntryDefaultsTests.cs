using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NDB.Platform.Kit.Caching;
using Xunit;

namespace NDB.Platform.Tests.Kit.Caching;

public sealed class CacheEntryDefaultsTests
{
    [Fact]
    public void DefaultMasterDataOptions_ShouldHaveCorrectSlidingExpiry()
    {
        var opts = CacheEntryDefaults.DefaultMasterDataOptions();
        opts.SlidingExpiration.Should().Be(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public void DefaultMasterDataOptions_ShouldHaveCorrectAbsoluteExpiry()
    {
        var opts = CacheEntryDefaults.DefaultMasterDataOptions();
        opts.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromSeconds(3600));
    }

    [Fact]
    public void DefaultMasterDataOptions_ShouldHaveNormalPriority()
    {
        var opts = CacheEntryDefaults.DefaultMasterDataOptions();
        opts.Priority.Should().Be(CacheItemPriority.Normal);
    }

    [Fact]
    public void DefaultMasterDataOptions_ShouldHaveSize1024()
    {
        var opts = CacheEntryDefaults.DefaultMasterDataOptions();
        opts.Size.Should().Be(1024);
    }

    [Fact]
    public void DefaultReferenceOptions_ShouldHave5MinuteSliding()
    {
        var opts = CacheEntryDefaults.DefaultReferenceOptions();
        opts.SlidingExpiration.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void DefaultReferenceOptions_ShouldHave24HourAbsolute()
    {
        var opts = CacheEntryDefaults.DefaultReferenceOptions();
        opts.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromHours(24));
    }

    [Fact]
    public void DefaultLookupOptions_ShouldHave30SecondSliding()
    {
        var opts = CacheEntryDefaults.DefaultLookupOptions();
        opts.SlidingExpiration.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void DefaultLookupOptions_ShouldHave5MinuteAbsolute()
    {
        var opts = CacheEntryDefaults.DefaultLookupOptions();
        opts.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void DefaultMasterDataOptions_ShouldReturnNewInstanceEachCall()
    {
        var opts1 = CacheEntryDefaults.DefaultMasterDataOptions();
        var opts2 = CacheEntryDefaults.DefaultMasterDataOptions();
        opts1.Should().NotBeSameAs(opts2);
    }
}
