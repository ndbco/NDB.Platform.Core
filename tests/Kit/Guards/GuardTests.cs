using FluentAssertions;
using NDB.Platform.Kit.Guards;
using Xunit;

namespace NDB.Platform.Tests.Kit.Guards;

public sealed class GuardTests
{
    // AgainstNull tests
    [Fact]
    public void AgainstNull_NonNullValue_ShouldReturnValue()
    {
        var obj = new object();
        var result = Guard.AgainstNull(obj, "param");
        result.Should().BeSameAs(obj);
    }

    [Fact]
    public void AgainstNull_NullValue_ShouldThrowArgumentNullException()
    {
        string? nullString = null;
        var act = () => Guard.AgainstNull(nullString, "myParam");
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("myParam");
    }

    // AgainstEmpty tests
    [Fact]
    public void AgainstEmpty_NonEmptyString_ShouldReturnValue()
    {
        var result = Guard.AgainstEmpty("hello", "param");
        result.Should().Be("hello");
    }

    [Fact]
    public void AgainstEmpty_EmptyString_ShouldThrowArgumentException()
    {
        var act = () => Guard.AgainstEmpty("", "name");
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void AgainstEmpty_WhitespaceString_ShouldThrowArgumentException()
    {
        var act = () => Guard.AgainstEmpty("   ", "name");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AgainstEmpty_NullString_ShouldThrowArgumentException()
    {
        var act = () => Guard.AgainstEmpty(null, "name");
        act.Should().Throw<ArgumentException>();
    }

    // AgainstDefault tests
    [Fact]
    public void AgainstDefault_NonDefaultGuid_ShouldReturnValue()
    {
        var id = Guid.NewGuid();
        var result = Guard.AgainstDefault(id, "id");
        result.Should().Be(id);
    }

    [Fact]
    public void AgainstDefault_DefaultGuid_ShouldThrowArgumentException()
    {
        var act = () => Guard.AgainstDefault(Guid.Empty, "id");
        act.Should().Throw<ArgumentException>()
            .WithParameterName("id");
    }

    [Fact]
    public void AgainstDefault_DefaultInt_ShouldThrow()
    {
        var act = () => Guard.AgainstDefault(0, "count");
        act.Should().Throw<ArgumentException>();
    }

    // AgainstNegative (int) tests
    [Fact]
    public void AgainstNegative_Int_Zero_ShouldReturnZero()
    {
        Guard.AgainstNegative(0, "x").Should().Be(0);
    }

    [Fact]
    public void AgainstNegative_Int_Positive_ShouldReturnValue()
    {
        Guard.AgainstNegative(5, "x").Should().Be(5);
    }

    [Fact]
    public void AgainstNegative_Int_Negative_ShouldThrow()
    {
        var act = () => Guard.AgainstNegative(-1, "amount");
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("amount");
    }

    // AgainstNegative (double) tests
    [Fact]
    public void AgainstNegative_Double_Negative_ShouldThrow()
    {
        var act = () => Guard.AgainstNegative(-0.001, "rate");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // AgainstOutOfRange tests
    [Fact]
    public void AgainstOutOfRange_InRange_ShouldReturnValue()
    {
        Guard.AgainstOutOfRange(5, 1, 10, "val").Should().Be(5);
    }

    [Fact]
    public void AgainstOutOfRange_BelowMin_ShouldThrow()
    {
        var act = () => Guard.AgainstOutOfRange(0, 1, 10, "age");
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("age");
    }

    [Fact]
    public void AgainstOutOfRange_AboveMax_ShouldThrow()
    {
        var act = () => Guard.AgainstOutOfRange(11, 1, 10, "age");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // AgainstInvalidEnum tests
    [Fact]
    public void AgainstInvalidEnum_ValidValue_ShouldReturnValue()
    {
        var result = Guard.AgainstInvalidEnum(DayOfWeek.Monday, "day");
        result.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void AgainstInvalidEnum_InvalidValue_ShouldThrow()
    {
        var invalidDay = (DayOfWeek)99;
        var act = () => Guard.AgainstInvalidEnum(invalidDay, "day");
        act.Should().Throw<ArgumentException>()
            .WithParameterName("day");
    }
}
