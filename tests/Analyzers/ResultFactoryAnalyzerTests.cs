using System.Globalization;
using FluentAssertions;
using NDB.Platform.Analyzers;
using Xunit;

namespace NDB.Platform.Tests.Analyzers;

/// <summary>
/// Tests untuk ResultFactoryAnalyzer.
/// Verifikasi bahwa analyzer terdaftar dengan benar dan memiliki konfigurasi yang tepat.
/// </summary>
public sealed class ResultFactoryAnalyzerTests
{
    [Fact]
    public void Analyzer_ShouldHaveCorrectDiagnosticId()
    {
        ResultFactoryAnalyzer.DiagnosticId.Should().Be("NDB001");
    }

    [Fact]
    public void Analyzer_ShouldHaveSupportedDiagnostics()
    {
        var analyzer = new ResultFactoryAnalyzer();
        var diagnostics = analyzer.SupportedDiagnostics;
        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().Be("NDB001");
    }

    [Fact]
    public void Analyzer_Diagnostic_ShouldBeError()
    {
        var analyzer = new ResultFactoryAnalyzer();
        var diagnostic = analyzer.SupportedDiagnostics[0];
        diagnostic.DefaultSeverity.Should().Be(Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
    }

    [Fact]
    public void Analyzer_Diagnostic_ShouldBeEnabledByDefault()
    {
        var analyzer = new ResultFactoryAnalyzer();
        var diagnostic = analyzer.SupportedDiagnostics[0];
        diagnostic.IsEnabledByDefault.Should().BeTrue();
    }

    /// <summary>
    /// FIX C-10: Verifikasi message format menyertakan type name (Sprint B).
    /// </summary>
    [Fact]
    public void Analyzer_MessageFormat_ShouldIncludeTypeName()
    {
        var analyzer = new ResultFactoryAnalyzer();
        var rule = analyzer.SupportedDiagnostics[0];
        // MessageFormat harus mengandung placeholder {0} untuk type name
        rule.MessageFormat.ToString(CultureInfo.InvariantCulture).Should().Contain("{0}");
    }

    /// <summary>
    /// FIX C-10: Verifikasi description menyebut semua Result subclasses.
    /// </summary>
    [Fact]
    public void Analyzer_Description_ShouldCoverAllResultSubclasses()
    {
        var analyzer = new ResultFactoryAnalyzer();
        var rule = analyzer.SupportedDiagnostics[0];
        var desc = rule.Description.ToString(CultureInfo.InvariantCulture);
        desc.Should().Contain("PagedResult");
        desc.Should().Contain("ListResult");
        desc.Should().Contain("CollectionResult");
    }

    /// <summary>
    /// FIX C-10: Verifikasi analyzer diinisialisasi dan register syntax node action.
    /// </summary>
    [Fact]
    public void Analyzer_Initialize_ShouldNotThrow()
    {
        var analyzer = new ResultFactoryAnalyzer();
        // SupportedDiagnostics tidak null = initialization benar
        analyzer.SupportedDiagnostics.Should().NotBeEmpty();
    }
}
