namespace Cal2AL.Core.Model;

/// <summary>C/AL object types that can appear in a C/SIDE text export.</summary>
public enum CalObjectType
{
    Unknown = 0,
    Table,
    Page,
    Report,
    Codeunit,
    XmlPort,
    Query,
    MenuSuite,

    /// <summary>Classic client form (NAV 2009 and earlier). No AL equivalent.</summary>
    Form,

    /// <summary>Classic dataport (NAV 2009 and earlier). No AL equivalent.</summary>
    Dataport,

    /// <summary>Delta file object (<c>OBJECT Modification "Customer"(Table 18)</c>).</summary>
    Modification,
}

/// <summary>Which flavour of C/SIDE export an object came from.</summary>
public enum InputDialect
{
    /// <summary>Not enough evidence either way.</summary>
    Unknown = 0,

    /// <summary>A normal C/SIDE export: object references by number, unquoted field names in properties.</summary>
    Classic,

    /// <summary><c>finsql.exe Command=ExportToNewSyntax</c>: object references by name, AL-style property values.</summary>
    NewSyntax,
}

public static class CalObjectTypeExtensions
{
    /// <summary>Parses the object type keyword used in TXT files and in C/AL code (<c>Table</c>, <c>XMLport</c>, …).</summary>
    public static CalObjectType ParseObjectType(string text)
    {
        return text.Trim().ToUpperInvariant() switch
        {
            "TABLE" or "TABLEDATA" or "DATABASE" => CalObjectType.Table,
            "PAGE" => CalObjectType.Page,
            "REPORT" => CalObjectType.Report,
            "CODEUNIT" => CalObjectType.Codeunit,
            "XMLPORT" => CalObjectType.XmlPort,
            "QUERY" => CalObjectType.Query,
            "MENUSUITE" => CalObjectType.MenuSuite,
            "FORM" => CalObjectType.Form,
            "DATAPORT" => CalObjectType.Dataport,
            "MODIFICATION" => CalObjectType.Modification,
            _ => CalObjectType.Unknown,
        };
    }

    /// <summary>The AL keyword for the object type (lower case, as the AL formatter writes it).</summary>
    public static string ToAlKeyword(this CalObjectType type) => type switch
    {
        CalObjectType.Table => "table",
        CalObjectType.Page => "page",
        CalObjectType.Report => "report",
        CalObjectType.Codeunit => "codeunit",
        CalObjectType.XmlPort => "xmlport",
        CalObjectType.Query => "query",
        _ => type.ToString().ToLowerInvariant(),
    };

    /// <summary>The type name AL uses in declarations and option access (<c>Record</c>, <c>Page</c>, <c>XmlPort</c>, …).</summary>
    public static string ToAlTypeName(this CalObjectType type) => type switch
    {
        CalObjectType.Table => "Record",
        CalObjectType.XmlPort => "XmlPort",
        _ => type.ToString(),
    };

    /// <summary>The prefix used in AL object-ID expressions such as <c>Database::Customer</c>.</summary>
    public static string ToAlIdPrefix(this CalObjectType type) => type switch
    {
        CalObjectType.Table => "Database",
        CalObjectType.XmlPort => "XmlPort",
        _ => type.ToString(),
    };

    public static bool HasAlEquivalent(this CalObjectType type) =>
        type is CalObjectType.Table or CalObjectType.Page or CalObjectType.Report or CalObjectType.Codeunit
            or CalObjectType.XmlPort or CalObjectType.Query or CalObjectType.Modification;
}
