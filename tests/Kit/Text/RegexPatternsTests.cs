using FluentAssertions;
using NDB.Platform.Kit.Text;
using Xunit;

namespace NDB.Platform.Tests.Kit.Text;

public sealed class RegexPatternsTests
{
    // Email
    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("user.name+tag@domain.co.id", true)]
    [InlineData("notanemail", false)]
    [InlineData("@nodomain.com", false)]
    [InlineData("user@", false)]
    public void Email_ShouldMatchCorrectly(string input, bool shouldMatch)
    {
        RegexPatterns.Email().IsMatch(input).Should().Be(shouldMatch);
    }

    // PhoneIndonesia
    [Theory]
    [InlineData("08123456789", true)]
    [InlineData("+628123456789", true)]
    [InlineData("628123456789", true)]
    [InlineData("12345", false)]
    [InlineData("08123456", false)]
    public void PhoneIndonesia_ShouldMatchCorrectly(string input, bool shouldMatch)
    {
        RegexPatterns.PhoneIndonesia().IsMatch(input).Should().Be(shouldMatch);
    }

    // NIK
    [Theory]
    [InlineData("3174012345678901", true)]
    [InlineData("317401234567890", false)] // 15 digit
    [InlineData("31740123456789012", false)] // 17 digit
    [InlineData("317401234567890a", false)] // contains letter
    public void Nik_ShouldMatchCorrectly(string input, bool shouldMatch)
    {
        RegexPatterns.Nik().IsMatch(input).Should().Be(shouldMatch);
    }

    // NPWP
    [Theory]
    [InlineData("01.234.567.8-901.234", true)]
    [InlineData("1234567890", false)]
    [InlineData("01.234.567.8-901.23", false)]
    public void Npwp_ShouldMatchCorrectly(string input, bool shouldMatch)
    {
        RegexPatterns.Npwp().IsMatch(input).Should().Be(shouldMatch);
    }

    // PostalCode
    [Theory]
    [InlineData("12345", true)]
    [InlineData("1234", false)]
    [InlineData("123456", false)]
    [InlineData("1234a", false)]
    public void PostalCodeIndonesia_ShouldMatchCorrectly(string input, bool shouldMatch)
    {
        RegexPatterns.PostalCodeIndonesia().IsMatch(input).Should().Be(shouldMatch);
    }

    // Alphanumeric
    [Theory]
    [InlineData("ABC123", true)]
    [InlineData("abc", true)]
    [InlineData("123", true)]
    [InlineData("abc-123", false)]
    [InlineData("", false)]
    public void Alphanumeric_ShouldMatchCorrectly(string input, bool shouldMatch)
    {
        RegexPatterns.Alphanumeric().IsMatch(input).Should().Be(shouldMatch);
    }
}
