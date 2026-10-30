using System.Collections;

namespace Cal2AL.Core.Model;

/// <summary>A 1-based line/column position in the source file.</summary>
public readonly record struct SourcePosition(int Line, int Column)
{
    public static SourcePosition None { get; } = new(0, 0);

    public bool IsKnown => Line > 0;

    public override string ToString() => IsKnown ? $"{Line},{Column}" : "?";
}

/// <summary>One <c>Name=Value</c> property as written in the TXT file.</summary>
public sealed class CalProperty(string name, string value, SourcePosition position)
{
    /// <summary>Property name exactly as written, e.g. <c>CaptionML</c> or <c>Version List</c>.</summary>
    public string Name { get; } = name;

    /// <summary>
    /// The value with C/SIDE's wrapping removed: no surrounding <c>[ ]</c>, <c>]]</c> unescaped,
    /// continuation lines joined with <c>\n</c> and their alignment indentation removed.
    /// </summary>
    public string Value { get; set; } = value;

    public SourcePosition Position { get; } = position;

    public override string ToString() => $"{Name}={Value}";
}

/// <summary>An ordered property list with case-insensitive lookup by name.</summary>
public sealed class PropertyCollection : IEnumerable<CalProperty>
{
    private readonly List<CalProperty> items = [];

    public int Count => items.Count;

    public void Add(CalProperty property) => items.Add(property);

    public CalProperty? Find(string name) =>
        items.Find(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

    public string? this[string name] => Find(name)?.Value;

    public bool Contains(string name) => Find(name) is not null;

    public bool Remove(string name) => items.RemoveAll(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

    /// <summary>True when the property is set to <c>Yes</c> or <c>TRUE</c>.</summary>
    public bool IsYes(string name) => this[name]?.Trim().ToUpperInvariant() is "YES" or "TRUE";

    public IEnumerator<CalProperty> GetEnumerator() => items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>A line of C/AL code with its position in the TXT file.</summary>
public sealed record CodeLine(string Text, int SourceLine);

/// <summary>
/// The statements between <c>BEGIN</c> and the matching <c>END;</c> of a trigger or procedure. The lines keep their
/// indentation relative to the block, exactly as C/SIDE exported them.
/// </summary>
public sealed class CalCodeBody(IReadOnlyList<CodeLine> lines)
{
    public IReadOnlyList<CodeLine> Lines { get; } = lines;

    public static CalCodeBody Empty { get; } = new([]);

    public bool IsEmpty => Lines.Count == 0 || Lines.All(l => string.IsNullOrWhiteSpace(l.Text));

    public int FirstSourceLine => Lines.Count > 0 ? Lines[0].SourceLine : 0;

    public string Text => string.Join("\n", Lines.Select(l => l.Text));
}

/// <summary>Data type of a variable, parameter, return value or table field.</summary>
public sealed class CalDataType
{
    /// <summary>Base type name as written, e.g. <c>Record</c>, <c>Code</c>, <c>Text</c>, <c>Option</c>, <c>DotNet</c>, <c>TextConst</c>.</summary>
    public required string Name { get; init; }

    /// <summary>Length for Code/Text (<c>Code20</c> and <c>Code[20]</c> both give 20).</summary>
    public int? Length { get; init; }

    /// <summary>
    /// Object subtype for Record/Codeunit/Page/Report/XMLport/Query/TestPage: either a number (<c>18</c>) or a name
    /// (<c>Customer</c>, <c>"Sales Header"</c>), as written.
    /// </summary>
    public string? Subtype { get; init; }

    /// <summary>Option members for Option variables (<c>' ,Item,Resource'</c> without quotes).</summary>
    public string? OptionString { get; init; }

    /// <summary>Multi-language value of a TextConst (<c>ENU=…;DEU=…</c>).</summary>
    public string? TextConstValue { get; init; }

    /// <summary>The assembly-qualified type string of DotNet/Automation/OCX variables.</summary>
    public string? ExternalTypeName { get; init; }

    public override string ToString() => Name
        + (Length is null ? string.Empty : $"[{Length}]")
        + (Subtype is null ? string.Empty : " " + Subtype)
        + (ExternalTypeName is null ? string.Empty : " " + ExternalTypeName);
}

/// <summary>A variable, parameter or named/unnamed return value.</summary>
public sealed class CalVariable
{
    /// <summary>Name without quotes; empty for an unnamed return value.</summary>
    public required string Name { get; init; }

    public int? Id { get; init; }
    public required CalDataType Type { get; init; }

    /// <summary>For parameters: passed by reference (<c>VAR</c>).</summary>
    public bool IsVar { get; init; }

    public bool IsTemporary { get; init; }

    /// <summary>Array dimensions as written between the brackets of <c>ARRAY [..] OF</c>, e.g. <c>10</c> or <c>2,3</c>.</summary>
    public string? Dimensions { get; init; }

    public bool InDataSet { get; init; }
    public bool RunOnClient { get; init; }
    public bool WithEvents { get; init; }
    public bool SuppressDispose { get; init; }
    public bool NonDebuggable { get; init; }
    public string? SecurityFiltering { get; init; }
    public SourcePosition Position { get; init; }

    public override string ToString() => $"{(IsVar ? "VAR " : string.Empty)}{Name} : {(Dimensions is null ? string.Empty : $"ARRAY [{Dimensions}] OF ")}{(IsTemporary ? "TEMPORARY " : string.Empty)}{Type}";
}

/// <summary>A trigger written as a property (<c>OnValidate=BEGIN … END;</c>).</summary>
public sealed class CalTrigger
{
    /// <summary>Trigger name as written, e.g. <c>OnValidate</c> or <c>Import::OnAfterInsertRecord</c>.</summary>
    public required string Name { get; init; }

    public List<CalVariable> Variables { get; init; } = [];
    public required CalCodeBody Body { get; init; }
    public SourcePosition Position { get; init; }
}

/// <summary>A procedure, or a .NET/control add-in event trigger declared with <c>EVENT</c>, in a CODE section.</summary>
public sealed class CalProcedure
{
    public required string Name { get; init; }
    public int? Id { get; init; }
    public bool IsLocal { get; init; }

    /// <summary>Attribute texts without the outer brackets, in order, e.g. <c>EventSubscriber(Table,18,OnAfterDeleteEvent,"",Skip,Skip)</c>.</summary>
    public List<string> Attributes { get; init; } = [];

    public List<CalVariable> Parameters { get; init; } = [];
    public CalVariable? ReturnValue { get; init; }
    public List<CalVariable> Variables { get; init; } = [];
    public required CalCodeBody Body { get; init; }

    /// <summary>True for <c>EVENT Source@Id::Name@Id(...)</c> declarations.</summary>
    public bool IsEvent { get; init; }

    /// <summary>For events: the variable or control that raises the event.</summary>
    public string? EventSource { get; init; }

    /// <summary>For events: the ID of the source variable (positive) or control (negative).</summary>
    public int? EventSourceId { get; init; }

    public SourcePosition Position { get; init; }

    public override string ToString() => IsEvent ? $"EVENT {EventSource}::{Name}" : $"PROCEDURE {Name}";
}

/// <summary>Anything that can carry properties, triggers and nested sections: objects, section items, request pages.</summary>
public abstract class CalNode
{
    public PropertyCollection Properties { get; } = new();
    public List<CalTrigger> Triggers { get; } = [];

    /// <summary>Nested sections in file order (FIELDS, KEYS, CONTROLS, ACTIONS, DATASET, REQUESTPAGE, …).</summary>
    public List<CalSection> Sections { get; } = [];

    public SourcePosition Position { get; init; }

    public CalSection? GetSection(string name) =>
        Sections.Find(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<CalElement> GetItems(string sectionName) => GetSection(sectionName)?.Items ?? (IReadOnlyList<CalElement>)[];

    public CalTrigger? FindTrigger(string name) =>
        Triggers.Find(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
}

/// <summary>A section such as <c>FIELDS { … }</c>, <c>ACTIONS { … }</c> or a container property value <c>ActionList=ACTIONS { … }</c>.</summary>
public sealed class CalSection(string name, SourcePosition position)
{
    public string Name { get; } = name;
    public SourcePosition Position { get; } = position;
    public List<CalElement> Items { get; } = [];

    /// <summary>Declarations inside delta containers such as <c>VariableCollection</c> or <c>ParameterCollection</c>.</summary>
    public List<CalVariable> Variables { get; } = [];

    /// <summary>Procedures inside delta containers such as <c>PROCEDURECollection</c>.</summary>
    public List<CalProcedure> Procedures { get; } = [];
}

/// <summary>One <c>{ … }</c> item in a section: a field, key, control, action, data item, XMLport node, label, change, …</summary>
public sealed class CalElement : CalNode
{
    public required string SectionName { get; init; }

    /// <summary>The leading semicolon-separated values (field number, name, type; control id, level, type; …).</summary>
    public List<string> Positional { get; init; } = [];

    public string GetPositional(int index) => index < Positional.Count ? Positional[index] : string.Empty;

    public int LastSourceLine { get; set; }
}

/// <summary>The CODE section of an object.</summary>
public sealed class CalCodeSection
{
    public List<CalVariable> Variables { get; } = [];
    public List<CalProcedure> Procedures { get; } = [];

    /// <summary>The documentation trigger (<c>BEGIN { … } END.</c>) lines.</summary>
    public List<string> Documentation { get; } = [];
}

/// <summary>A complete C/AL object parsed from a TXT file.</summary>
public sealed class CalObject : CalNode
{
    public required CalObjectType Type { get; init; }
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string SourceFile { get; init; }

    /// <summary>The original text of the object, kept for migration artifacts and diagnostics.</summary>
    public required string RawText { get; init; }

    public int EndLine { get; init; }

    /// <summary>OBJECT-PROPERTIES values: Date, Time, Modified, Version List.</summary>
    public Dictionary<string, string> ObjectProperties { get; } = new(StringComparer.OrdinalIgnoreCase);

    public CalCodeSection Code { get; } = new();

    /// <summary>RDLDATA lines (the RDLC layout XML), if any.</summary>
    public List<string> RdlData { get; } = [];

    /// <summary>WORDLAYOUT lines (base64), if any.</summary>
    public List<string> WordLayout { get; } = [];

    public InputDialect Dialect { get; set; }

    /// <summary>
    /// Set when the parser recognised the object but deliberately did not parse its body because the format has no
    /// AL equivalent (forms, dataports, menu suites, classic reports with sections).
    /// </summary>
    public string? UnsupportedReason { get; init; }

    /// <summary>For Modification objects: the target as written, e.g. <c>"Customer"(Table 18)</c>.</summary>
    public string? ModificationTarget { get; init; }

    public string DisplayName => $"{Type} {Id} {Name}";

    public override string ToString() => DisplayName;
}
