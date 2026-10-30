using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cal2AL.Diagnostics;

/// <summary>Outcome of converting one C/AL object.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ObjectConversionStatus>))]
public enum ObjectConversionStatus
{
    /// <summary>Converted; no warnings or errors.</summary>
    Converted,

    /// <summary>Converted; review the warnings.</summary>
    ConvertedWithWarnings,

    /// <summary>AL was written, but it contains constructs that need manual work (error diagnostics).</summary>
    RequiresManualWork,

    /// <summary>No AL could be produced; the original was saved as a migration artifact.</summary>
    Failed,

    /// <summary>Filtered out by command-line options.</summary>
    Skipped,
}

/// <summary>Counts used to show that business logic was carried over, not dropped.</summary>
public sealed record ObjectMetrics
{
    public int Fields { get; init; }
    public int Keys { get; init; }
    public int Controls { get; init; }
    public int Actions { get; init; }
    public int DataItems { get; init; }
    public int Procedures { get; init; }
    public int Triggers { get; init; }
    public int Variables { get; init; }
    public int CodeLines { get; init; }

    /// <summary>Statements found by parsing the original C/AL bodies.</summary>
    public int Statements { get; init; }

    /// <summary>Calls to record database operations (FIND*, GET, INSERT, MODIFY, DELETE, CALCFIELDS, …) in the original.</summary>
    public int DatabaseOperations { get; init; }

    /// <summary>Statements found by re-parsing the generated AL bodies; must equal <see cref="Statements"/>.</summary>
    public int OutputStatements { get; init; }

    public int OutputDatabaseOperations { get; init; }
}

/// <summary>Result of converting (or analysing) one object.</summary>
public sealed record ObjectConversionResult
{
    public required string ObjectType { get; init; }
    public required int ObjectId { get; init; }
    public required string ObjectName { get; init; }
    public required string SourceFile { get; init; }
    public int SourceLine { get; init; }
    public string? Dialect { get; init; }
    public ObjectConversionStatus Status { get; set; }
    public List<string> OutputFiles { get; init; } = [];
    public List<Diagnostic> Diagnostics { get; init; } = [];
    public ObjectMetrics Metrics { get; set; } = new();

    public int ErrorCount => Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);
    public int WarningCount => Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);

    /// <summary>Derives <see cref="Status"/> from the diagnostics unless the object failed or was skipped.</summary>
    public void UpdateStatusFromDiagnostics()
    {
        if (Status is ObjectConversionStatus.Failed or ObjectConversionStatus.Skipped)
        {
            return;
        }

        Status = ErrorCount > 0
            ? ObjectConversionStatus.RequiresManualWork
            : WarningCount > 0 ? ObjectConversionStatus.ConvertedWithWarnings : ObjectConversionStatus.Converted;
    }
}

/// <summary>Per-object-type statistics.</summary>
public sealed record ObjectTypeSummary(string ObjectType, int Total, int Converted, int ConvertedWithWarnings, int RequiresManualWork, int Failed, int Skipped);

/// <summary>The complete, serialisable report of a run.</summary>
public sealed class ConversionReport
{
    public string Tool { get; init; } = "cal2al";
    public string ToolVersion { get; init; } = typeof(ConversionReport).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";
    public required string Command { get; init; }
    public required string Source { get; init; }
    public string? Target { get; init; }
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset FinishedAt { get; set; }
    public Dictionary<string, string> Options { get; init; } = new(StringComparer.Ordinal);
    public int FilesRead { get; set; }
    public List<ObjectConversionResult> Objects { get; init; } = [];

    /// <summary>Diagnostics that are not tied to one object (unreadable files, text between objects, …).</summary>
    public List<Diagnostic> RunDiagnostics { get; init; } = [];

    public List<string> GeneratedFiles { get; init; } = [];

    public IReadOnlyList<ObjectTypeSummary> Summary => Objects
        .GroupBy(o => o.ObjectType, StringComparer.OrdinalIgnoreCase)
        .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
        .Select(g => new ObjectTypeSummary(
            g.Key,
            g.Count(),
            g.Count(o => o.Status == ObjectConversionStatus.Converted),
            g.Count(o => o.Status == ObjectConversionStatus.ConvertedWithWarnings),
            g.Count(o => o.Status == ObjectConversionStatus.RequiresManualWork),
            g.Count(o => o.Status == ObjectConversionStatus.Failed),
            g.Count(o => o.Status == ObjectConversionStatus.Skipped)))
        .ToArray();

    public IEnumerable<Diagnostic> AllDiagnostics => RunDiagnostics.Concat(Objects.SelectMany(o => o.Diagnostics));

    public int ErrorCount => AllDiagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);
    public int WarningCount => AllDiagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);
    public int InfoCount => AllDiagnostics.Count(d => d.Severity == DiagnosticSeverity.Info);
    public int FailedObjects => Objects.Count(o => o.Status == ObjectConversionStatus.Failed);

    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public void WriteJson(string path)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(path, ToJson());
    }
}
