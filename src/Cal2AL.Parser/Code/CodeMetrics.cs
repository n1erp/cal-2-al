namespace Cal2AL.Parser.Code;

/// <summary>Statement and database-operation counts of a parsed body.</summary>
public readonly record struct CodeMetrics(int Statements, int DatabaseOperations)
{
    /// <summary>Record methods that read or write the database.</summary>
    public static readonly HashSet<string> DatabaseMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "FIND", "FINDFIRST", "FINDLAST", "FINDSET", "NEXT", "GET", "INSERT", "MODIFY", "MODIFYALL", "DELETE",
        "DELETEALL", "RENAME", "CALCFIELDS", "CALCSUMS", "LOCKTABLE", "COUNT", "ISEMPTY", "COUNTAPPROX",
        "SETAUTOCALCFIELDS", "READISOLATION",
    };

    public static CodeMetrics Compute(CodeBodySyntax body)
    {
        var statements = 0;
        var db = 0;
        foreach (var node in body.AllNodes())
        {
            switch (node)
            {
                case BlockStatement or EmptyStatement:
                    break;
                case StatementSyntax:
                    statements++;
                    break;
                case MemberAccessExpression member when DatabaseMethods.Contains(member.MemberName):
                    db++;
                    break;
            }
        }

        // Unqualified calls inside table code (INSERT; IF FINDFIRST THEN …) act on Rec.
        foreach (var node in body.AllNodes())
        {
            if (node is ExpressionStatement { Expression: NameExpression n } && DatabaseMethods.Contains(n.Identifier))
            {
                db++;
            }
            else if (node is InvocationExpression { Target: NameExpression callee } && DatabaseMethods.Contains(callee.Identifier))
            {
                db++;
            }
        }

        return new CodeMetrics(statements, db);
    }
}

/// <summary>
/// Compares the executable content of two token streams. Comments, whitespace, letter case and identifier quoting are
/// ignored; everything else — every identifier, literal, operator and keyword, in order — must be identical. This is
/// how Cal2AL proves that converting a body did not change its logic.
/// </summary>
public static class TokenStreamComparer
{
    /// <summary>
    /// Normalises tokens for comparison. Date constants are compared by the date they denote, read with the rules of
    /// <paramref name="language"/> (C/AL: MMDDYY[YY]D in <paramref name="calDateOrder"/>; AL: yyyymmddD).
    /// </summary>
    public static IReadOnlyList<string> Normalize(IEnumerable<CodeToken> tokens, CodeLanguage language = CodeLanguage.CAL, CalDateOrder calDateOrder = CalDateOrder.MonthDayYear)
    {
        var result = new List<string>();
        foreach (var t in tokens)
        {
            if (t.IsTrivia || t.Kind == CodeTokenKind.EndOfInput)
            {
                continue;
            }

            result.Add(t.Kind switch
            {
                CodeTokenKind.Identifier or CodeTokenKind.QuotedIdentifier => "id:" + t.IdentifierName.ToUpperInvariant(),
                CodeTokenKind.String => "s:" + t.Text,
                CodeTokenKind.Date => "d:" + NormalizeDate(t.Text, language, calDateOrder),
                CodeTokenKind.Integer or CodeTokenKind.BigInteger or CodeTokenKind.Decimal
                    or CodeTokenKind.Time or CodeTokenKind.DateTime => "n:" + t.Text.ToUpperInvariant(),
                _ => "p:" + t.Text,
            });
        }

        return result;
    }

    private static string NormalizeDate(string text, CodeLanguage language, CalDateOrder order)
    {
        var ok = language == CodeLanguage.CAL
            ? DateLiteral.TryParseCal(text, order, out var value)
            : DateLiteral.TryParseAl(text, out value);
        if (!ok)
        {
            return "invalid " + text.ToUpperInvariant();
        }

        return value.IsBlank ? "blank" : value.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Returns null when equivalent, otherwise a description of the first difference.</summary>
    public static string? FindDifference(IEnumerable<CodeToken> original, IEnumerable<CodeToken> converted, CalDateOrder calDateOrder = CalDateOrder.MonthDayYear)
    {
        var a = Normalize(original, CodeLanguage.CAL, calDateOrder);
        var b = Normalize(converted, CodeLanguage.AL);
        var n = Math.Min(a.Count, b.Count);
        for (var i = 0; i < n; i++)
        {
            if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
            {
                return $"token {i + 1}: original '{a[i][(a[i].IndexOf(':') + 1)..]}' vs converted '{b[i][(b[i].IndexOf(':') + 1)..]}'";
            }
        }

        if (a.Count != b.Count)
        {
            return $"original has {a.Count} tokens, converted has {b.Count}";
        }

        return null;
    }
}
