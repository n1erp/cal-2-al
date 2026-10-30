using System.Text.Json.Serialization;

namespace Cal2AL.Diagnostics;

/// <summary>How serious a diagnostic is.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<DiagnosticSeverity>))]
public enum DiagnosticSeverity
{
    /// <summary>Informational: something was changed deliberately and is worth knowing.</summary>
    Info = 0,

    /// <summary>The output is believed correct but should be reviewed.</summary>
    Warning = 1,

    /// <summary>The output will not compile or does not behave like the original; manual work is required.</summary>
    Error = 2,
}

/// <summary>Broad grouping of diagnostics, used for filtering and reporting.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<DiagnosticCategory>))]
public enum DiagnosticCategory
{
    Input,
    Parse,
    Structure,
    Property,
    Code,
    Reference,
    Delta,
    Output,
}

/// <summary>Where a diagnostic applies. Every field is optional because some diagnostics are file- or run-level.</summary>
public sealed record DiagnosticLocation
{
    public string? File { get; init; }
    public int? Line { get; init; }
    public int? Column { get; init; }
    public string? ObjectType { get; init; }
    public int? ObjectId { get; init; }
    public string? ObjectName { get; init; }

    /// <summary>Procedure or trigger name, for example <c>OnValidate</c> or <c>PostDocument</c>.</summary>
    public string? Member { get; init; }

    /// <summary>Field, control, action, data item or other element the member belongs to.</summary>
    public string? Element { get; init; }

    public static DiagnosticLocation None { get; } = new();

    public override string ToString()
    {
        var parts = new List<string>();
        if (ObjectType is not null)
        {
            parts.Add($"{ObjectType} {ObjectId} {ObjectName}".Trim());
        }

        if (Element is not null)
        {
            parts.Add(Element);
        }

        if (Member is not null)
        {
            parts.Add(Member);
        }

        var where = string.Join(" / ", parts);
        if (File is not null)
        {
            var pos = Line is null ? File : $"{File}({Line}{(Column is null ? string.Empty : "," + Column)})";
            where = where.Length == 0 ? pos : $"{pos}: {where}";
        }

        return where;
    }
}

/// <summary>
/// A structured message about the conversion. Diagnostics are the only channel through which Cal2AL
/// admits that something could not be converted exactly; they are never replaced by comments in the output.
/// </summary>
public sealed record Diagnostic
{
    public required string Code { get; init; }
    public required DiagnosticSeverity Severity { get; init; }
    public required DiagnosticCategory Category { get; init; }
    public required string Message { get; init; }
    public DiagnosticLocation Location { get; init; } = DiagnosticLocation.None;

    /// <summary>The C/AL construct involved, for example <c>CREATE(Automation)</c> or <c>CalcFormula</c>.</summary>
    public string? Construct { get; init; }

    /// <summary>Why the construct could not be converted exactly.</summary>
    public string? Reason { get; init; }

    /// <summary>What a developer should do about it.</summary>
    public string? SuggestedResolution { get; init; }

    /// <summary>The original source text, kept so nothing is lost even when it could not be emitted.</summary>
    public string? OriginalSource { get; init; }

    public override string ToString() => $"{Severity.ToString().ToLowerInvariant()} {Code}: {Message} [{Location}]";
}
