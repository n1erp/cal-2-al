using System.Text;
using System.Text.RegularExpressions;
using Cal2AL.Converter.Symbols;
using Cal2AL.Generator;

namespace Cal2AL.Converter.Values;

/// <summary>Thrown when a property expression cannot be parsed.</summary>
public sealed class PropertyExpressionException(string message) : Exception(message);

/// <summary>
/// Converts the filter-style property values of C/AL — <c>TableRelation</c>, <c>CalcFormula</c>, table views,
/// page/report links and query filters — to AL syntax. Accepts both classic values (unquoted field names such as
/// <c>Customer No.=FIELD(No.)</c>) and new-syntax values (already quoted), and always produces AL layout:
/// lower-case keywords, quoted identifiers where needed, <c>" = "</c> around comparisons.
/// </summary>
public sealed partial class FilterExpressionConverter(SymbolTable symbols)
{
    /// <summary>Raised for Boolean constants whose field type could not be confirmed.</summary>
    public event Action<string>? AssumedBoolean;

    private static readonly string[] CalcMethods = ["SUM", "AVERAGE", "EXIST", "COUNT", "MIN", "MAX", "LOOKUP"];

    /// <summary>Joins continuation lines: a line break and the indentation after it become one space.</summary>
    public static string Flatten(string value) => ContinuationRegex().Replace(value, " ").Trim();

    [GeneratedRegex(@"\s*\n\s*")]
    private static partial Regex ContinuationRegex();

    // ---- TableRelation ---------------------------------------------------------------------------

    /// <param name="value">The property value.</param>
    /// <param name="currentTableId">Table that owns the field (for IF conditions and FIELD references).</param>
    public string ConvertTableRelation(string value, int? currentTableId)
    {
        var reader = new ExpressionReader(Flatten(value));
        var result = ParseRelation(reader, currentTableId);
        reader.SkipSpaces();
        if (!reader.AtEnd)
        {
            throw new PropertyExpressionException($"Unexpected text '{reader.Rest}' in TableRelation.");
        }

        return result;
    }

    private string ParseRelation(ExpressionReader reader, int? currentTableId)
    {
        reader.SkipSpaces();
        if (reader.TryKeyword("IF"))
        {
            reader.SkipSpaces();
            var conditions = ConvertConditionList(reader.ReadParenthesized(), currentTableId, currentTableId);
            var target = ParseTarget(reader, currentTableId, allowElse: true);
            var result = $"if ({conditions}) {target}";
            reader.SkipSpaces();
            if (reader.TryKeyword("ELSE"))
            {
                result += " else " + ParseRelation(reader, currentTableId);
            }

            return result;
        }

        return ParseTarget(reader, currentTableId, allowElse: true);
    }

    /// <summary>Parses <c>Table[.Field] [WHERE (conditions)]</c>.</summary>
    private string ParseTarget(ExpressionReader reader, int? currentTableId, bool allowElse)
    {
        reader.SkipSpaces();
        var table = reader.ReadTableName();
        var tableId = symbols.ResolveTableId(table);
        var sb = new StringBuilder(AlSyntax.IdentifierFromSource(table));

        if (reader.Peek() == '.')
        {
            reader.Next();
            var field = reader.ReadFieldNameUntilKeyword(allowElse ? ["WHERE", "ELSE"] : ["WHERE"]);
            sb.Append('.').Append(AlSyntax.IdentifierFromSource(field));
        }

        reader.SkipSpaces();
        if (reader.TryKeyword("WHERE"))
        {
            reader.SkipSpaces();
            sb.Append(" where(").Append(ConvertConditionList(reader.ReadParenthesized(), tableId, currentTableId)).Append(')');
        }

        return sb.ToString();
    }

    // ---- CalcFormula ---------------------------------------------------------------------------------

    public string ConvertCalcFormula(string value, int? currentTableId)
    {
        var reader = new ExpressionReader(Flatten(value));
        reader.SkipSpaces();
        var negate = false;
        if (reader.Peek() == '-')
        {
            negate = true;
            reader.Next();
            reader.SkipSpaces();
        }

        var method = reader.ReadWord();
        if (!CalcMethods.Contains(method.ToUpperInvariant()))
        {
            throw new PropertyExpressionException($"Unknown CalcFormula method '{method}'.");
        }

        reader.SkipSpaces();
        var inner = new ExpressionReader(reader.ReadParenthesized());
        var target = ParseTarget(inner, currentTableId, allowElse: false);
        inner.SkipSpaces();
        if (!inner.AtEnd)
        {
            throw new PropertyExpressionException($"Unexpected text '{inner.Rest}' in CalcFormula.");
        }

        reader.SkipSpaces();
        if (!reader.AtEnd)
        {
            throw new PropertyExpressionException($"Unexpected text '{reader.Rest}' after CalcFormula.");
        }

        return $"{(negate ? "-" : string.Empty)}{method.ToLowerInvariant()}({target})";
    }

    // ---- Table views ---------------------------------------------------------------------------------

    /// <summary>Converts <c>SORTING(...) ORDER(...) WHERE(...)</c>.</summary>
    public string ConvertTableView(string value, int? tableId)
    {
        var reader = new ExpressionReader(Flatten(value));
        var parts = new List<string>();
        while (true)
        {
            reader.SkipSpaces();
            if (reader.AtEnd)
            {
                break;
            }

            var keyword = reader.ReadWord().ToUpperInvariant();
            reader.SkipSpaces();
            var content = reader.ReadParenthesized();
            switch (keyword)
            {
                case "SORTING":
                    parts.Add("sorting(" + string.Join(", ", SplitTopLevel(content, ',').Select(f => AlSyntax.IdentifierFromSource(f.Trim()))) + ")");
                    break;
                case "ORDER":
                    parts.Add("order(" + content.Trim().ToLowerInvariant() + ")");
                    break;
                case "WHERE":
                    parts.Add("where(" + ConvertConditionList(content, tableId, tableId) + ")");
                    break;
                default:
                    throw new PropertyExpressionException($"Unexpected '{keyword}' in table view; SORTING, ORDER or WHERE expected.");
            }
        }

        return string.Join(" ", parts);
    }

    // ---- Links and filter lists ----------------------------------------------------------------------

    /// <summary>
    /// Converts link lists such as <c>SubPageLink</c>, <c>RunPageLink</c>, report <c>DataItemLink</c> and XMLport
    /// <c>LinkFields</c>: <c>Field=FIELD(Other),Field2=CONST(x)</c>.
    /// </summary>
    public string ConvertLinks(string value, int? targetTableId, int? sourceTableId) =>
        ConvertConditionList(Flatten(value), targetTableId, sourceTableId);

    /// <summary>Converts query <c>DataItemLink</c>: <c>Customer No.=Customer.No.</c>.</summary>
    public string ConvertQueryLink(string value)
    {
        var items = SplitTopLevel(Flatten(value), ',');
        var result = new List<string>();
        foreach (var item in items)
        {
            var eq = IndexOfTopLevel(item, '=');
            if (eq < 0)
            {
                throw new PropertyExpressionException($"'=' expected in DataItemLink '{item}'.");
            }

            var left = AlSyntax.IdentifierFromSource(item[..eq].Trim());
            var right = item[(eq + 1)..].Trim();
            var dot = IndexOfTopLevel(right, '.');
            if (dot < 0)
            {
                throw new PropertyExpressionException($"'DataItem.Field' expected in DataItemLink '{item}'.");
            }

            var dataItem = AlSyntax.IdentifierFromSource(right[..dot].Trim());
            var field = AlSyntax.IdentifierFromSource(right[(dot + 1)..].Trim());
            result.Add($"{left} = {dataItem}.{field}");
        }

        return string.Join(", ", result);
    }

    /// <summary>Converts query <c>OrderBy</c>: <c>Balance=Descending</c> → <c>descending(Balance)</c>.</summary>
    public static string ConvertOrderBy(string value)
    {
        var flat = Flatten(value);
        if (Regex.IsMatch(flat, @"^\s*(ascending|descending)\s*\(", RegexOptions.IgnoreCase))
        {
            return flat;
        }

        var result = new List<string>();
        foreach (var item in SplitTopLevel(flat, ','))
        {
            var eq = item.LastIndexOf('=');
            if (eq < 0)
            {
                throw new PropertyExpressionException($"'Column=Ascending|Descending' expected in OrderBy '{item}'.");
            }

            var direction = item[(eq + 1)..].Trim().ToLowerInvariant();
            if (direction is not ("ascending" or "descending"))
            {
                throw new PropertyExpressionException($"Unknown sort direction '{direction}' in OrderBy.");
            }

            result.Add($"{direction}({AlSyntax.IdentifierFromSource(item[..eq].Trim())})");
        }

        return string.Join(", ", result);
    }

    /// <summary>Converts a comma-separated list of <c>Field=CONST|FILTER|FIELD(...)</c> conditions.</summary>
    public string ConvertConditionList(string content, int? targetTableId, int? sourceTableId)
    {
        var conditions = SplitTopLevel(content, ',');
        return string.Join(", ", conditions.Select(c => ConvertCondition(c.Trim(), targetTableId, sourceTableId)));
    }

    private string ConvertCondition(string condition, int? targetTableId, int? sourceTableId)
    {
        var eq = IndexOfTopLevel(condition, '=');
        if (eq <= 0)
        {
            throw new PropertyExpressionException($"'Field=Value' expected but found '{condition}'.");
        }

        var fieldText = condition[..eq].Trim();
        var reader = new ExpressionReader(condition[(eq + 1)..].Trim());
        var function = reader.ReadWord().ToUpperInvariant();
        reader.SkipSpaces();
        var argument = reader.ReadParenthesized();
        reader.SkipSpaces();
        if (!reader.AtEnd)
        {
            throw new PropertyExpressionException($"Unexpected text '{reader.Rest}' in condition '{condition}'.");
        }

        var field = AlSyntax.IdentifierFromSource(fieldText);
        return function switch
        {
            "CONST" => $"{field} = const({ConvertConst(argument, targetTableId, fieldText)})",
            "FILTER" => $"{field} = filter({ConvertFilter(argument, targetTableId, fieldText)})",
            "FIELD" => $"{field} = field({ConvertFieldReference(argument)})",
            _ => throw new PropertyExpressionException($"CONST, FILTER or FIELD expected but found '{function}' in '{condition}'."),
        };
    }

    private static string ConvertFieldReference(string argument)
    {
        var trimmed = argument.Trim();
        var m = Regex.Match(trimmed, @"^(?<fn>UPPERLIMIT|LOWERLIMIT|FILTER)\s*\((?<inner>.*)\)$", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            return $"{m.Groups["fn"].Value.ToLowerInvariant()}({AlSyntax.IdentifierFromSource(m.Groups["inner"].Value.Trim())})";
        }

        return AlSyntax.IdentifierFromSource(trimmed);
    }

    private string ConvertConst(string argument, int? tableId, string fieldText)
    {
        var value = argument.Trim();
        if (value.Length == 0)
        {
            return "''";
        }

        if (value.StartsWith('\'') || value.StartsWith('"'))
        {
            return value;
        }

        var upper = value.ToUpperInvariant();
        if (upper is "YES" or "NO" or "TRUE" or "FALSE")
        {
            var field = tableId is int id ? symbols.FindField(id, fieldText) : null;
            var isBoolean = field is not null && field.DataType.Equals("Boolean", StringComparison.OrdinalIgnoreCase);
            if (field is null)
            {
                AssumedBoolean?.Invoke($"{AlSyntax.Unquote(fieldText)}=CONST({value})");
            }

            if (isBoolean || field is null)
            {
                return upper is "YES" or "TRUE" ? "true" : "false";
            }
        }

        return QuoteOperandIfNeeded(value);
    }

    private string ConvertFilter(string argument, int? tableId, string fieldText)
    {
        var tokens = TokenizeFilter(argument.Trim());
        var sb = new StringBuilder();
        foreach (var (text, isOperator) in tokens)
        {
            if (isOperator)
            {
                sb.Append(text);
                continue;
            }

            var upper = text.ToUpperInvariant();
            if (upper is "YES" or "NO")
            {
                var field = tableId is int id ? symbols.FindField(id, fieldText) : null;
                if (field is null || field.DataType.Equals("Boolean", StringComparison.OrdinalIgnoreCase))
                {
                    if (field is null)
                    {
                        AssumedBoolean?.Invoke($"{AlSyntax.Unquote(fieldText)}=FILTER({argument})");
                    }

                    sb.Append(upper == "YES" ? "true" : "false");
                    continue;
                }
            }

            sb.Append(QuoteOperandIfNeeded(text));
        }

        return sb.ToString();
    }

    /// <summary>Splits a filter into operands and operators (<c>| &amp; .. &lt;&gt; &lt;= &gt;= &lt; &gt; = ( )</c>).</summary>
    private static List<(string Text, bool IsOperator)> TokenizeFilter(string filter)
    {
        var result = new List<(string, bool)>();
        var operand = new StringBuilder();
        void FlushOperand()
        {
            var t = operand.ToString().Trim();
            if (t.Length > 0)
            {
                result.Add((t, false));
            }

            operand.Clear();
        }

        for (var i = 0; i < filter.Length; i++)
        {
            var c = filter[i];
            if (c is '\'' or '"')
            {
                var end = filter.IndexOf(c, i + 1);
                while (end > 0 && end + 1 < filter.Length && filter[end + 1] == c)
                {
                    end = filter.IndexOf(c, end + 2);
                }

                end = end < 0 ? filter.Length - 1 : end;
                operand.Append(filter, i, end - i + 1);
                i = end;
                continue;
            }

            string? op = null;
            if (i + 1 < filter.Length)
            {
                var two = filter.Substring(i, 2);
                if (two is ".." or "<>" or "<=" or ">=")
                {
                    op = two;
                }
            }

            op ??= c is '|' or '&' or '<' or '>' or '=' or '(' or ')' ? c.ToString() : null;
            if (op is null)
            {
                operand.Append(c);
                continue;
            }

            FlushOperand();
            result.Add((op, true));
            i += op.Length - 1;
        }

        FlushOperand();
        return result;
    }

    [GeneratedRegex(@"^-?\d+([.,]\d+)?$")]
    private static partial Regex NumberRegex();

    /// <summary>Quotes a filter/const operand that is not a valid identifier, number, literal or wildcard pattern.</summary>
    private static string QuoteOperandIfNeeded(string operand)
    {
        if (operand.Length == 0 || operand[0] is '\'' or '"' || NumberRegex().IsMatch(operand)
            || AlSyntax.IsPlainIdentifier(operand) || operand.IndexOfAny(['*', '?', '@', '%']) >= 0
            || Regex.IsMatch(operand, @"^\d+[DT]$", RegexOptions.IgnoreCase))
        {
            return operand;
        }

        return AlSyntax.QuotedIdentifier(operand);
    }

    // ---- low-level helpers ---------------------------------------------------------------------------

    /// <summary>Splits on <paramref name="separator"/> outside quotes and parentheses.</summary>
    public static List<string> SplitTopLevel(string text, char separator)
    {
        var result = new List<string>();
        var depth = 0;
        var start = 0;
        char quote = '\0';
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quote != '\0')
            {
                if (c == quote)
                {
                    quote = '\0';
                }

                continue;
            }

            switch (c)
            {
                case '"' or '\'':
                    quote = c;
                    break;
                case '(':
                    depth++;
                    break;
                case ')':
                    depth--;
                    break;
                default:
                    if (c == separator && depth == 0)
                    {
                        result.Add(text[start..i]);
                        start = i + 1;
                    }

                    break;
            }
        }

        result.Add(text[start..]);
        return result.Where(s => s.Trim().Length > 0).ToList();
    }

    private static int IndexOfTopLevel(string text, char target)
    {
        var depth = 0;
        char quote = '\0';
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quote != '\0')
            {
                if (c == quote)
                {
                    quote = '\0';
                }

                continue;
            }

            if (c is '"' or '\'')
            {
                quote = c;
            }
            else if (c == '(')
            {
                depth++;
            }
            else if (c == ')')
            {
                depth--;
            }
            else if (c == target && depth == 0)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Character reader for property expressions, aware of quotes and parentheses.</summary>
    private sealed class ExpressionReader(string text)
    {
        private int pos;

        public bool AtEnd => pos >= text.Length;
        public int Position => pos;
        public string Rest => text[pos..];

        public char Peek() => pos < text.Length ? text[pos] : '\0';

        public char Next() => pos < text.Length ? text[pos++] : '\0';

        public void SkipSpaces()
        {
            while (pos < text.Length && char.IsWhiteSpace(text[pos]))
            {
                pos++;
            }
        }

        public string ReadWord()
        {
            var start = pos;
            while (pos < text.Length && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_'))
            {
                pos++;
            }

            if (pos == start)
            {
                throw new PropertyExpressionException($"Word expected but found '{Rest}'.");
            }

            return text[start..pos];
        }

        /// <summary>True (and consumed) when the next word is <paramref name="keyword"/>.</summary>
        public bool TryKeyword(string keyword)
        {
            if (pos + keyword.Length > text.Length
                || string.Compare(text, pos, keyword, 0, keyword.Length, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            var after = pos + keyword.Length < text.Length ? text[pos + keyword.Length] : ' ';
            if (char.IsLetterOrDigit(after) || after == '_')
            {
                return false;
            }

            pos += keyword.Length;
            return true;
        }

        /// <summary>Reads <c>( … )</c> and returns the content without the outer parentheses.</summary>
        public string ReadParenthesized()
        {
            SkipSpaces();
            if (Peek() != '(')
            {
                throw new PropertyExpressionException($"'(' expected but found '{Rest}'.");
            }

            var start = pos + 1;
            var depth = 0;
            char quote = '\0';
            for (; pos < text.Length; pos++)
            {
                var c = text[pos];
                if (quote != '\0')
                {
                    if (c == quote)
                    {
                        quote = '\0';
                    }

                    continue;
                }

                if (c is '"' or '\'')
                {
                    quote = c;
                }
                else if (c == '(')
                {
                    depth++;
                }
                else if (c == ')')
                {
                    depth--;
                    if (depth == 0)
                    {
                        pos++;
                        return text[start..(pos - 1)];
                    }
                }
            }

            throw new PropertyExpressionException("Unbalanced parentheses.");
        }

        /// <summary>A quoted name, or an unquoted name up to '.', whitespace or end.</summary>
        public string ReadTableName()
        {
            if (Peek() == '"')
            {
                var end = text.IndexOf('"', pos + 1);
                while (end > 0 && end + 1 < text.Length && text[end + 1] == '"')
                {
                    end = text.IndexOf('"', end + 2);
                }

                if (end < 0)
                {
                    throw new PropertyExpressionException("Unterminated quoted name.");
                }

                var name = text[pos..(end + 1)];
                pos = end + 1;
                return name;
            }

            var start = pos;
            while (pos < text.Length && text[pos] != '.' && !char.IsWhiteSpace(text[pos]) && text[pos] != '(')
            {
                pos++;
            }

            if (pos == start)
            {
                throw new PropertyExpressionException($"Table name expected but found '{Rest}'.");
            }

            return text[start..pos];
        }

        /// <summary>
        /// A quoted field name, or an unquoted one (which may contain spaces, dots and parentheses) up to one of the
        /// given keywords preceded by whitespace, or the end.
        /// </summary>
        public string ReadFieldNameUntilKeyword(string[] keywords)
        {
            if (Peek() == '"')
            {
                return ReadTableName();
            }

            var best = text.Length;
            foreach (var keyword in keywords)
            {
                var m = Regex.Match(text[pos..], @"\s" + keyword + @"(\s|\(|$)", RegexOptions.IgnoreCase);
                if (m.Success && pos + m.Index < best)
                {
                    best = pos + m.Index;
                }
            }

            var name = text[pos..best].Trim();
            pos = best;
            if (name.Length == 0)
            {
                throw new PropertyExpressionException("Field name expected.");
            }

            return name;
        }
    }
}
