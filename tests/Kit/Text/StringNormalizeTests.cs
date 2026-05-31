using FluentAssertions;
using NDB.Platform.Kit.Text;
using Xunit;

namespace NDB.Platform.Tests.Kit.Text;

public sealed class StringNormalizeTests
{
    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("  Multiple   Spaces  ", "multiple-spaces")]
    [InlineData("Special@#$Chars!", "special-chars")]
    [InlineData("Héllo Wörld", "hello-world")]
    public void ToSlug_ShouldProduceValidSlug(string input, string expected)
    {
        StringNormalize.ToSlug(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("HelloWorld", "hello-world")]
    [InlineData("MyAPIService", "my-api-service")]
    [InlineData("camelCase", "camel-case")]
    public void ToKebabCase_ShouldConvertCorrectly(string input, string expected)
    {
        StringNormalize.ToKebabCase(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("HelloWorld", "helloWorld")]
    [InlineData("HELLO", "hELLO")]
    public void ToCamelCase_ShouldMakeFirstCharLowercase(string input, string expected)
    {
        StringNormalize.ToCamelCase(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("hello world", "HelloWorld")]
    [InlineData("ndb platform", "NdbPlatform")]
    public void ToPascalCase_ShouldCapitalizeEachWord(string input, string expected)
    {
        StringNormalize.ToPascalCase(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("myAPIService", "my_api_service")]
    public void ToSnakeCase_ShouldConvertCorrectly(string input, string expected)
    {
        StringNormalize.ToSnakeCase(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("Héllo", "Hello")]
    [InlineData("café", "cafe")]
    [InlineData("résumé", "resume")]
    public void RemoveDiacritics_ShouldRemoveAccents(string input, string expected)
    {
        StringNormalize.RemoveDiacritics(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("  hello   world  ", "hello world")]
    [InlineData("a  b   c", "a b c")]
    public void TrimAndCollapseSpaces_ShouldNormalize(string input, string expected)
    {
        StringNormalize.TrimAndCollapseSpaces(input).Should().Be(expected);
    }

    [Fact]
    public void ToSlug_EmptyString_ShouldReturnEmpty()
    {
        StringNormalize.ToSlug("").Should().BeEmpty();
    }

    [Fact]
    public void TrimAndCollapseSpaces_WhitespaceOnly_ShouldReturnEmpty()
    {
        StringNormalize.TrimAndCollapseSpaces("   ").Should().BeEmpty();
    }
}
