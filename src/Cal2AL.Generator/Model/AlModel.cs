namespace Cal2AL.Generator.Model;

/// <summary>A property line <c>Name = Value;</c>. <see cref="Value"/> is already valid AL text (may span lines).</summary>
public sealed record AlProperty(string Name, string Value);

/// <summary>A variable, parameter or return value declaration.</summary>
public sealed class AlVariable
{
    /// <summary>Attribute lines written before the declaration, e.g. <c>[InDataSet]</c>.</summary>
    public List<string> Attributes { get; } = [];

    /// <summary>Name as it should be written (already quoted if needed). Empty for unnamed return values.</summary>
    public required string Name { get; init; }

    /// <summary>Type text as it should be written, e.g. <c>Record Customer temporary</c> or <c>Label 'Hi', Comment = 'x'</c>.</summary>
    public required string Type { get; init; }

    public bool IsVar { get; init; }

    public string Declaration => Name.Length == 0 ? Type : $"{(IsVar ? "var " : string.Empty)}{Name}: {Type}";
}

public enum AlMethodKind
{
    Procedure,
    Trigger,
}

/// <summary>A procedure or trigger with its body.</summary>
public sealed class AlMethod
{
    public AlMethodKind Kind { get; init; }
    public List<string> Attributes { get; } = [];

    /// <summary>Access modifier written before the keyword: <c>local</c>, <c>internal</c>, or empty.</summary>
    public string Access { get; init; } = string.Empty;

    /// <summary>Name as written, including <c>Var::Event</c> for .NET event triggers.</summary>
    public required string Name { get; init; }

    public List<AlVariable> Parameters { get; } = [];

    /// <summary>For triggers whose signature comes from the trigger definition, the parameter text verbatim.</summary>
    public string? ParameterText { get; init; }

    /// <summary>Return type, or null.</summary>
    public string? ReturnType { get; init; }

    /// <summary>Name of a named return value, or null.</summary>
    public string? ReturnName { get; init; }

    public List<AlVariable> Variables { get; } = [];

    /// <summary>Body lines without the enclosing begin/end, relative indentation preserved.</summary>
    public List<string> Body { get; } = [];

    /// <summary>Comment lines written immediately before the method (attributes come after them).</summary>
    public List<string> LeadingComments { get; } = [];
}

/// <summary>
/// A brace-delimited block: an object-level section (<c>fields</c>, <c>layout</c>), an element
/// (<c>field(1; "No."; Code[20])</c>, <c>area(Content)</c>, <c>dataitem(...)</c>) or a nested group.
/// </summary>
public sealed class AlSection
{
    public required string Header { get; set; }
    public List<AlProperty> Properties { get; } = [];
    public List<AlSection> Children { get; } = [];
    public List<AlMethod> Triggers { get; } = [];

    /// <summary>Lines such as report labels (<c>Name = 'Caption';</c>) written as-is inside the section.</summary>
    public List<string> Lines { get; } = [];

    /// <summary>Comment lines written before the header.</summary>
    public List<string> LeadingComments { get; } = [];
}

/// <summary>A complete AL object ready to be rendered.</summary>
public sealed class AlObject
{
    /// <summary>AL object keyword: table, page, report, codeunit, xmlport, query, tableextension, pageextension, enum.</summary>
    public required string Kind { get; init; }

    public required int Id { get; init; }

    /// <summary>Object name as written (quoted if needed).</summary>
    public required string Name { get; init; }

    /// <summary>For extension objects: the extended object's name as written.</summary>
    public string? Extends { get; init; }

    /// <summary>Comment lines written before the object declaration.</summary>
    public List<string> HeaderComments { get; } = [];

    /// <summary>Comment lines written at the top of the object body (the C/AL documentation trigger).</summary>
    public List<string> Documentation { get; } = [];

    public List<AlProperty> Properties { get; } = [];
    public List<AlSection> Sections { get; } = [];
    public List<AlVariable> Variables { get; } = [];
    public List<AlMethod> Triggers { get; } = [];
    public List<AlMethod> Procedures { get; } = [];

    /// <summary>The C/AL object this was converted from, for reporting.</summary>
    public string? SourceDescription { get; init; }
}
