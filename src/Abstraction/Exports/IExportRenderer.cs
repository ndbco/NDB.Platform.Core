namespace NDB.Platform.Abstraction.Exports;

/// <summary>
/// Dataset for export rendering — columns and rows of tabular data.
/// </summary>
public sealed record ExportDataset
{
    /// <summary>Column header names.</summary>
    public IReadOnlyList<string> Columns { get; init; } = [];

    /// <summary>Data rows (each row is a list of values per column).</summary>
    public IReadOnlyList<IReadOnlyList<object?>> Rows { get; init; } = [];

    /// <summary>Sheet name (for XLSX). Default: "Export".</summary>
    public string SheetName { get; init; } = "Export";
}

/// <summary>
/// Abstraction for export file renderers (CSV, XLSX, etc.).
/// The implementation is chosen in the consuming project based on the required format.
/// </summary>
public interface IExportRenderer
{
    /// <summary>Renders the dataset to a byte array ready for download.</summary>
    Task<byte[]> RenderAsync(ExportDataset dataset, CancellationToken ct = default);

    /// <summary>MIME type of the output (e.g. "text/csv", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet").</summary>
    string MimeType { get; }

    /// <summary>File extension of the output without a leading dot (e.g. "csv", "xlsx").</summary>
    string Extension { get; }
}
