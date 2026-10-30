using System.Text;

namespace Cal2AL.Generator;

/// <summary>Lexical helpers for writing AL: identifier and string quoting, keyword checks.</summary>
public static class AlSyntax
{
    /// <summary>
    /// Words that must be quoted when used as an identifier in AL declarations. This is deliberately broader than the
    /// set of words the AL parser strictly reserves: quoting is always legal, so being generous is safe, while missing
    /// a word breaks compilation.
    /// </summary>
    private static readonly HashSet<string> QuoteWhenUsedAsName = new(StringComparer.OrdinalIgnoreCase)
    {
        "action", "actions", "and", "area", "array", "ascending", "asserterror", "average", "begin", "break", "case",
        "code", "codeunit", "const", "controladdin", "count", "cuegroup", "descending", "div", "do", "downto", "else",
        "end", "enum", "enumextension", "event", "exit", "false", "field", "fieldgroup", "fieldgroups", "fields", "filter",
        "fixed", "for", "foreach", "function", "grid", "group", "if", "implements", "in", "interface", "internal", "key",
        "keys", "label", "labels", "layout", "local", "max", "min", "mod", "not", "object", "of", "or", "order", "page",
        "pagecustomization", "pageextension", "part", "procedure", "profile", "program", "protected", "query", "record",
        "repeat", "repeater", "report", "requestpage", "runonclient", "schema", "separator", "sum", "table",
        "tableextension", "temporary", "then", "to", "trigger", "true", "until", "var", "while", "with", "xmlport",
        "xor", "dotnet", "assembly", "type", "elements", "dataset", "dataitem", "column", "none", "integer",
    };

    /// <summary>
    /// Words that are reserved in AL statements but were ordinary identifiers in C/AL. An identifier with one of these
    /// names must be quoted wherever it appears in code, not only in its declaration.
    /// </summary>
    private static readonly HashSet<string> ReservedInStatements = new(StringComparer.OrdinalIgnoreCase)
    {
        "trigger", "internal", "protected", "interface", "implements", "procedure", "event", "break", "var", "local",
        "array", "temporary", "program", "function",
    };

    public static bool IsPlainIdentifier(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (!(char.IsAsciiLetter(value[0]) || value[0] == '_'))
        {
            return false;
        }

        for (var i = 1; i < value.Length; i++)
        {
            if (!(char.IsAsciiLetterOrDigit(value[i]) || value[i] == '_'))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsQuoted(string value) =>
        value.Length >= 2 && value[0] == '"' && value[^1] == '"';

    /// <summary>True when <paramref name="name"/> must be quoted to be used as an identifier.</summary>
    public static bool NeedsQuotes(string name) =>
        !IsPlainIdentifier(name) || QuoteWhenUsedAsName.Contains(name);

    /// <summary>True for names that must be quoted in code as well as in declarations.</summary>
    public static bool IsReservedInStatements(string name) => ReservedInStatements.Contains(name);

    /// <summary>Quotes a raw (unquoted) name when needed. Embedded double quotes are doubled.</summary>
    public static string Identifier(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "\"\"";
        }

        return NeedsQuotes(name) ? QuotedIdentifier(name) : name;
    }

    /// <summary>
    /// Always quotes the name. AL quoted identifiers cannot contain a double quote, so one inside the name is
    /// replaced by an apostrophe (callers report this with <see cref="ContainsDoubleQuote"/>).
    /// </summary>
    public static string QuotedIdentifier(string name) => "\"" + name.Replace('"', '\'') + "\"";

    public static bool ContainsDoubleQuote(string name) => name.Contains('"');

    /// <summary>
    /// Normalises a name that may already be quoted (as in new-syntax exports) and re-quotes it if needed.
    /// </summary>
    public static string IdentifierFromSource(string value)
    {
        var trimmed = value.Trim();
        return Identifier(Unquote(trimmed));
    }

    /// <summary>Removes surrounding double quotes from an identifier and undoubles inner quotes.</summary>
    public static string Unquote(string value)
    {
        var trimmed = value.Trim();
        return IsQuoted(trimmed) ? trimmed[1..^1].Replace("\"\"", "\"", StringComparison.Ordinal) : trimmed;
    }

    /// <summary>An AL string literal: single quotes, embedded quotes doubled.</summary>
    public static string String(string value) => "'" + value.Replace("'", "''", StringComparison.Ordinal) + "'";

    /// <summary>Removes surrounding single quotes from a string literal and undoubles inner quotes.</summary>
    public static string UnquoteString(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length >= 2 && trimmed[0] == '\'' && trimmed[^1] == '\''
            ? trimmed[1..^1].Replace("''", "'", StringComparison.Ordinal)
            : trimmed;
    }

    /// <summary>Makes a string usable in a file name: letters and digits only (Txt2Al-compatible).</summary>
    public static string FileNameSafe(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
        }

        return sb.Length == 0 ? "Object" : sb.ToString();
    }

    /// <summary>Makes a valid unquoted AL identifier from arbitrary text (used for generated names and aliases).</summary>
    public static string ToIdentifierText(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            sb.Append(char.IsAsciiLetterOrDigit(c) || c == '_' ? c : '_');
        }

        var result = sb.ToString().Trim('_');
        if (result.Length == 0)
        {
            return "_";
        }

        return char.IsAsciiDigit(result[0]) ? "_" + result : result;
    }
}
