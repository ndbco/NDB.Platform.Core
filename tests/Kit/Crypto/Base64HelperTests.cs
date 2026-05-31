using FluentAssertions;
using NDB.Platform.Kit.Crypto;
using Xunit;

namespace NDB.Platform.Tests.Kit.Crypto;

public sealed class Base64HelperTests
{
    [Fact]
    public void Encode_ShouldProduceValidBase64()
    {
        var encoded = Base64Helper.Encode("hello world");
        encoded.Should().Be("aGVsbG8gd29ybGQ=");
    }

    [Fact]
    public void Decode_ShouldReturnOriginalString()
    {
        var decoded = Base64Helper.Decode("aGVsbG8gd29ybGQ=");
        decoded.Should().Be("hello world");
    }

    [Fact]
    public void EncodeDecodeRoundTrip_ShouldReturnOriginal()
    {
        var original = "Test string dengan unicode: 123!@#";
        var encoded = Base64Helper.Encode(original);
        var decoded = Base64Helper.Decode(encoded);
        decoded.Should().Be(original);
    }

    [Fact]
    public void EncodeUrlSafe_ShouldNotContainPaddingOrSpecialChars()
    {
        var encoded = Base64Helper.EncodeUrlSafe("binary>>data++here==");
        encoded.Should().NotContain("+");
        encoded.Should().NotContain("/");
        encoded.Should().NotContain("=");
    }

    [Fact]
    public void DecodeUrlSafe_ShouldReturnOriginal()
    {
        var original = "some data with special chars: +/=";
        var encoded = Base64Helper.EncodeUrlSafe(original);
        var decoded = Base64Helper.DecodeUrlSafe(encoded);
        decoded.Should().Be(original);
    }

    [Fact]
    public void EncodeDecodeUrlSafeRoundTrip_ShouldWork()
    {
        var original = "NDB Platform Token: xyz123";
        var encoded = Base64Helper.EncodeUrlSafe(original);
        var decoded = Base64Helper.DecodeUrlSafe(encoded);
        decoded.Should().Be(original);
    }
}
