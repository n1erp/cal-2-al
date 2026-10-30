using System.CommandLine;
using System.Globalization;
using Cal2AL.Converter;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Parser.Code;

namespace Cal2AL.CLI;

public enum Verbosity
{
    Quiet,
    Normal,
    Verbose,
}

public enum FailLevel
{
    Error,
    Warning,
    Never,
}

/// <summary>The options shared by the commands, and their translation into <see cref="ConversionOptions"/>.</summary>
public sealed class CliOptions
{
    public Option<bool> Verbose { get; } = new("--verbose", "-v")
    {
        Description = "Show every diagnostic and per-object statistics.",
        Recursive = true,
    };

    public Option<bool> Quiet { get; } = new("--quiet", "-q")
    {
        Description = "Only print errors and the final result line.",
        Recursive = true,
    };

    public Option<bool> NoColor { get; } = new("--no-color")
    {
        Description = "Disable colours and progress animation (also honours the NO_COLOR environment variable).",
        Recursive = true,
    };

    public Option<string> Source { get; } = new("--source", "-s")
    {
        Description = "A C/AL .txt or .delta file, or a folder that is searched recursively for them.",
        Required = true,
    };

    public Option<string> Target { get; } = new("--target", "-t")
    {
        Description = "Output folder for the AL project. Created if missing; existing files with the same names are overwritten.",
        Required = true,
    };

    public Option<string?> Report { get; } = new("--report", "-r")
    {
        Description = "Write the JSON report to this file (convert: defaults to <target>/.cal2al/report.json).",
    };

    public Option<FailLevel> FailOn { get; } = new("--fail-on")
    {
        Description = "Exit with code 1 when a diagnostic of this severity or worse is reported.",
        DefaultValueFactory = _ => FailLevel.Error,
    };

    public Option<string[]> ObjectTypes { get; } = new("--object-types", "--types")
    {
        Description = "Only convert these object types (Table, Page, Report, Codeunit, XMLport, Query, Modification).",
        AllowMultipleArgumentsPerToken = true,
    };

    public Option<string?> Ids { get; } = new("--ids")
    {
        Description = "Only convert object IDs in these ranges, e.g. 50000..50099,50200.",
    };

    public Option<OutputStructure> Structure { get; } = new("--structure")
    {
        Description = "Output folder layout: Mirror (sub-folders of the source), ByType or Flat.",
        DefaultValueFactory = _ => OutputStructure.Mirror,
    };

    public Option<int> ExtensionStartId { get; } = new("--extension-start-id")
    {
        Description = "First ID for generated table and page extensions.",
        DefaultValueFactory = _ => 70000000,
    };

    public Option<string> Runtime { get; } = new("--runtime")
    {
        Description = "Target AL runtime version, e.g. 12.0.",
        DefaultValueFactory = _ => "12.0",
    };

    public Option<string> Encoding { get; } = new("--encoding")
    {
        Description = "Code page for files that are not UTF-8 (C/SIDE exports use the OEM code page, usually 850).",
        DefaultValueFactory = _ => "850",
    };

    public Option<CalDateOrder> DateOrder { get; } = new("--date-order")
    {
        Description = "Order of day and month in C/AL date constants: Auto (detect per file), MonthDayYear or DayMonthYear.",
        DefaultValueFactory = _ => CalDateOrder.Auto,
    };

    public Option<bool> ScopeAttributes { get; } = new("--scope-attributes")
    {
        Description = "Add [Scope('OnPrem')] to procedures without [External], as Txt2Al does.",
    };

    public Option<bool> NoMetadata { get; } = new("--no-metadata")
    {
        Description = "Do not write the C/AL Version List/Date/Time comment above each object.",
    };

    public Option<bool> NoTranslations { get; } = new("--no-translations")
    {
        Description = "Do not write XLIFF files for non-ENU captions and text constants.",
    };

    public Option<bool> NoVerify { get; } = new("--no-verify")
    {
        Description = "Skip the token-by-token check that generated code matches the original.",
    };

    public Option<string> FilePattern { get; } = new("--file-pattern")
    {
        Description = "File name pattern for objects. Tokens: {name} {type} {id}.",
        DefaultValueFactory = _ => "{name}.{type}",
    };

    public Option<string> ExtensionFilePattern { get; } = new("--extension-file-pattern")
    {
        Description = "File name pattern for extensions. Tokens: {name} {type} {id} {targetName}.",
        DefaultValueFactory = _ => "{name}.{type}",
    };

    public Verbosity GetVerbosity(ParseResult result) =>
        result.GetValue(Quiet) ? Verbosity.Quiet : result.GetValue(Verbose) ? Verbosity.Verbose : Verbosity.Normal;

    /// <summary>
    /// Builds conversion options from the options present on the invoked command; options a command does not have
    /// keep the <see cref="ConversionOptions"/> defaults. Returns an error message for invalid values.
    /// </summary>
    public (ConversionOptions? Options, string? Error) BuildConversionOptions(ParseResult result)
    {
        var defaults = new ConversionOptions();
        T Get<T>(Option<T> option, T fallback) => result.GetResult(option) is null ? fallback : result.GetValue(option)!;

        var types = new HashSet<CalObjectType>();
        foreach (var token in Get(ObjectTypes, []).SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)))
        {
            var type = CalObjectTypeExtensions.ParseObjectType(token);
            if (type == CalObjectType.Unknown)
            {
                return (null, $"Unknown object type '{token}'.");
            }

            types.Add(type);
        }

        var (ranges, rangeError) = ParseIdRanges(Get(Ids, null));
        if (rangeError is not null)
        {
            return (null, rangeError);
        }

        var runtimeText = Get(Runtime, defaults.TargetRuntime.ToString());
        if (!Version.TryParse(runtimeText, out var runtime))
        {
            return (null, $"Invalid runtime version '{runtimeText}'. Use a version such as 12.0.");
        }

        var encoding = Get(Encoding, defaults.FallbackEncoding);
        try
        {
            Parser.Txt.TextDecoder.ResolveEncoding(encoding);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
        {
            return (null, $"Unknown encoding '{encoding}': {ex.Message}");
        }

        return (defaults with
        {
            ObjectTypes = types,
            IdRanges = ranges,
            ExtensionStartId = Get(ExtensionStartId, defaults.ExtensionStartId),
            TargetRuntime = runtime,
            FallbackEncoding = encoding,
            CalDateOrder = Get(DateOrder, defaults.CalDateOrder),
            EmitScopeAttributes = Get(ScopeAttributes, defaults.EmitScopeAttributes),
            KeepObjectMetadata = !Get(NoMetadata, !defaults.KeepObjectMetadata),
            GenerateTranslations = !Get(NoTranslations, !defaults.GenerateTranslations),
            VerifyCodePreservation = !Get(NoVerify, !defaults.VerifyCodePreservation),
            Structure = Get(Structure, defaults.Structure),
            ObjectFileNamePattern = Get(FilePattern, defaults.ObjectFileNamePattern),
            ExtensionFileNamePattern = Get(ExtensionFilePattern, defaults.ExtensionFileNamePattern),
        }, null);
    }

    /// <summary>Parses <c>50000..50099,50200</c>.</summary>
    public static (IReadOnlyList<(int From, int To)> Ranges, string? Error) ParseIdRanges(string? text)
    {
        var ranges = new List<(int, int)>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return (ranges, null);
        }

        foreach (var part in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var bounds = part.Split("..", StringSplitOptions.TrimEntries);
            if (bounds.Length is < 1 or > 2
                || !int.TryParse(bounds[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var from)
                || !int.TryParse(bounds[^1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var to)
                || to < from)
            {
                return (ranges, $"Invalid ID range '{part}'. Use 50000..50099 or 50200.");
            }

            ranges.Add((from, to));
        }

        return (ranges, null);
    }

    /// <summary>The options as typed, for the report.</summary>
    public static Dictionary<string, string> Echo(ParseResult result)
    {
        var echo = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var child in result.CommandResult.Children.OfType<System.CommandLine.Parsing.OptionResult>())
        {
            echo[child.Option.Name] = string.Join(" ", child.Tokens.Select(t => t.Value));
        }

        return echo;
    }

    public static bool ReachesFailLevel(ConversionReport report, FailLevel level) => level switch
    {
        FailLevel.Error => report.ErrorCount > 0,
        FailLevel.Warning => report.ErrorCount + report.WarningCount > 0,
        _ => false,
    };
}
