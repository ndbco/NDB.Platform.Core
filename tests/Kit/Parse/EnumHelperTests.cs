using System.ComponentModel;
using FluentAssertions;
using NDB.Platform.Kit.Parse;
using Xunit;

namespace NDB.Platform.Tests.Kit.Parse;

public sealed class EnumHelperTests
{
    private enum TestStatus
    {
        [Description("Aktif")]
        Active,
        [Description("Tidak Aktif")]
        Inactive,
        Pending
    }

    [Fact]
    public void TryParseEnum_ValidName_ShouldReturnTrue()
    {
        var success = EnumHelper.TryParseEnum<DayOfWeek>("Monday", out var result);
        success.Should().BeTrue();
        result.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void TryParseEnum_CaseInsensitive_ShouldWork()
    {
        var success = EnumHelper.TryParseEnum<DayOfWeek>("monday", out var result);
        success.Should().BeTrue();
        result.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void TryParseEnum_InvalidName_ShouldReturnFalse()
    {
        var success = EnumHelper.TryParseEnum<DayOfWeek>("InvalidDay", out _);
        success.Should().BeFalse();
    }

    [Fact]
    public void ListValues_ShouldReturnAllEnumValues()
    {
        var values = EnumHelper.ListValues<DayOfWeek>();
        values.Should().HaveCount(7);
    }

    [Fact]
    public void GetDescription_WithDescriptionAttribute_ShouldReturnDescription()
    {
        var desc = EnumHelper.GetDescription(TestStatus.Active);
        desc.Should().Be("Aktif");
    }

    [Fact]
    public void GetDescription_WithoutDescriptionAttribute_ShouldReturnToString()
    {
        var desc = EnumHelper.GetDescription(TestStatus.Pending);
        desc.Should().Be("Pending");
    }

    [Fact]
    public void GetEnumFromDescription_ShouldFindByDescription()
    {
        var result = EnumHelper.GetEnumFromDescription<TestStatus>("Tidak Aktif");
        result.Should().Be(TestStatus.Inactive);
    }

    [Fact]
    public void GetEnumFromDescription_NotFound_ShouldReturnDefault()
    {
        var result = EnumHelper.GetEnumFromDescription<TestStatus>("Unknown");
        result.Should().Be(default(TestStatus));
    }
}
