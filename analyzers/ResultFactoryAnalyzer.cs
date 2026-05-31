using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NDB.Platform.Analyzers;

/// <summary>
/// Roslyn analyzer NDB001: Prevents direct instantiation of Result, Result&lt;T&gt;,
/// PagedResult&lt;T&gt;, ListResult&lt;T&gt;, and CollectionResult&lt;T&gt; via constructor.
/// Enforces use of static factory methods (e.g. Result.Success(), Result.NotFound()).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ResultFactoryAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Diagnostic ID for this analyzer.</summary>
    public const string DiagnosticId = "NDB001";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        id: DiagnosticId,
        title: "Use Result factory methods instead of constructor",
        messageFormat: "'{0}' must be created via factory methods (e.g. Result.Success(), PagedResult.Success()). Direct constructor invocation is not allowed.",
        category: "NDB.Design",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Result<T>, Result, PagedResult<T>, ListResult<T>, and CollectionResult<T> use private/protected constructors. Use static factory methods: Success(), NotFound(), BadRequest(), Unauthorized(), Forbidden(), Error(), Conflict(), Validation().");

    // Blocked types in Roslyn MetadataName format: "Namespace.TypeName`Arity"
    // Non-generic: "NDB.Platform.Abstraction.Result"
    // Generic:     "NDB.Platform.Abstraction.Result`1" (backtick + arity)
    private static readonly ImmutableArray<string> BlockedTypeKeys = ImmutableArray.Create(
        "NDB.Platform.Abstraction.Result",
        "NDB.Platform.Abstraction.Result`1",
        "NDB.Platform.Abstraction.PagedResult`1",
        "NDB.Platform.Abstraction.ListResult`1",
        "NDB.Platform.Abstraction.CollectionResult`1"
    );

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        if (!(context.Node is ObjectCreationExpressionSyntax oc)) return;

        var typeInfo = context.SemanticModel.GetTypeInfo(oc, context.CancellationToken);
        if (!(typeInfo.Type is INamedTypeSymbol named)) return;

        // For generic types, use ConstructedFrom to get the open generic definition
        // e.g. PagedResult<string>.ConstructedFrom == PagedResult<T>
        var definition = named.IsGenericType ? named.ConstructedFrom : named;

        // Build lookup key: "Namespace.MetadataName"
        var ns = definition.ContainingNamespace != null
            ? definition.ContainingNamespace.ToDisplayString()
            : string.Empty;
        var key = string.IsNullOrEmpty(ns)
            ? definition.MetadataName
            : $"{ns}.{definition.MetadataName}";

        if (!IsBlocked(key)) return;

        var diagnostic = Diagnostic.Create(Rule, oc.GetLocation(), named.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsBlocked(string key)
    {
        foreach (var blocked in BlockedTypeKeys)
        {
            if (string.Equals(blocked, key, StringComparison.Ordinal))
                return true;
        }
        return false;
    }
}
