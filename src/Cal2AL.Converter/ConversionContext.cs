using Cal2AL.Converter.Symbols;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;

namespace Cal2AL.Converter;

/// <summary>State shared by all objects of one conversion run.</summary>
public sealed class RunContext(ConversionOptions options, SymbolTable symbols)
{
    public ConversionOptions Options { get; } = options;
    public SymbolTable Symbols { get; } = symbols;
    public TranslationCollector Translations { get; } = new();
    public DotNetCollector DotNet { get; } = new(options.DotNetTypePrefix);
    public EnumCollector Enums { get; } = new();
    public ExtensionIdAllocator ExtensionIds { get; } = new(options.ExtensionStartId);

    /// <summary>File name for an extracted layout, e.g. <c>CustomerList.Report.rdlc</c>.</summary>
    public string LayoutFileName(CalObject obj, string extension) =>
        $"{Generator.AlSyntax.FileNameSafe(obj.Name)}.{obj.Type}{extension}";

    /// <summary>The path written into RDLCLayout/WordLayout: relative to the app root, forward slashes.</summary>
    public string LayoutReference(string fileName) =>
        string.IsNullOrEmpty(Options.LayoutFolder) ? fileName : $"{Options.LayoutFolder.Trim('/', '\\').Replace('\\', '/')}/{fileName}";
}

/// <summary>
/// State for converting one object: where diagnostics go, which element/member is being converted, and the set of
/// names already used (AL requires unique control, action and data item names).
/// </summary>
public sealed class ObjectContext(RunContext run, CalObject obj)
{
    private readonly HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);

    public RunContext Run { get; } = run;
    public CalObject Object { get; } = obj;
    public ConversionOptions Options => Run.Options;
    public SymbolTable Symbols => Run.Symbols;
    public List<Diagnostic> Diagnostics { get; } = [];

    /// <summary>The element being converted (field, control, action, …), for diagnostic locations.</summary>
    public string? CurrentElement { get; set; }

    /// <summary>The trigger or procedure being converted, for diagnostic locations.</summary>
    public string? CurrentMember { get; set; }

    /// <summary>Identifiers that are declared in this object and must be quoted wherever they appear in code.</summary>
    public HashSet<string> IdentifiersToQuote { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Names of DotNet variables declared WITHEVENTS (global), used to place event triggers.</summary>
    public HashSet<string> WithEventsVariables { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Names of Automation/OCX variables, used to flag CREATE calls.</summary>
    public HashSet<string> AutomationVariables { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Object references already reported as unresolved (reported once per object).</summary>
    public HashSet<(CalObjectType Type, int Id)> ReportedUnresolvedReferences { get; } = [];

    /// <summary>Statement and database-operation counts accumulated over all bodies.</summary>
    public int Statements { get; set; }

    public int DatabaseOperations { get; set; }
    public int OutputStatements { get; set; }
    public int OutputDatabaseOperations { get; set; }
    public int CodeLines { get; set; }

    public DiagnosticLocation Location(int? line = null, int? column = null, string? member = null, string? element = null) => new()
    {
        File = Object.SourceFile,
        Line = line,
        Column = column,
        ObjectType = Object.Type.ToString(),
        ObjectId = Object.Id,
        ObjectName = Object.Name,
        Member = member ?? CurrentMember,
        Element = element ?? CurrentElement,
    };

    public Diagnostic Report(
        DiagnosticDescriptor descriptor,
        string message,
        int? line = null,
        string? construct = null,
        string? reason = null,
        string? resolution = null,
        string? original = null,
        DiagnosticSeverity? severity = null,
        string? member = null,
        string? element = null)
    {
        var diagnostic = descriptor.Create(message, Location(line, null, member, element), construct, reason, resolution, original, severity);
        Diagnostics.Add(diagnostic);
        return diagnostic;
    }

    /// <summary>
    /// Reserves <paramref name="preferred"/> as a name in this object. If it is taken (case-insensitively),
    /// <paramref name="fallback"/> is used instead and an info diagnostic is reported.
    /// </summary>
    public string ReserveName(string preferred, string fallback, int? line = null)
    {
        if (usedNames.Add(preferred))
        {
            return preferred;
        }

        var candidate = fallback;
        var n = 2;
        while (!usedNames.Add(candidate))
        {
            candidate = fallback + "_" + n++;
        }

        Report(DiagnosticDescriptors.NameMadeUnique,
            $"The name '{preferred}' is already used in this object; '{candidate}' is used instead.",
            line, construct: preferred);
        return candidate;
    }

    /// <summary>Marks a name as used without diagnostics (e.g. the object's own data item names).</summary>
    public bool TryReserveName(string name) => usedNames.Add(name);
}
