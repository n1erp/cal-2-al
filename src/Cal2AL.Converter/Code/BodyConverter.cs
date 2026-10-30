using System.Text;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Parser.Code;

namespace Cal2AL.Converter.Code;

/// <summary>
/// Converts the statements of a trigger or procedure. The code is tokenised and parsed; the parse validates the code,
/// drives the analysis rules and counts statements and database operations. The output is produced by rewriting
/// tokens in place, so formatting, comments and every statement are kept. Only these rewrites are made:
/// <list type="bullet">
/// <item><c>{ … }</c> comments become <c>/* … */</c>;</item>
/// <item>date constants change from the export's regional order (<c>MMDDYY[YY]D</c> or <c>DDMMYY[YY]D</c>) to AL <c>yyyymmddD</c>;</item>
/// <item>identifiers that are keywords in AL are quoted.</item>
/// </list>
/// Afterwards the output is re-parsed as AL and compared token by token with the original; any other difference
/// is reported as an error (it would mean a defect in Cal2AL).
/// </summary>
public sealed class BodyConverter(ObjectContext context)
{
    public List<string> Convert(CalCodeBody body, string memberDisplayName)
    {
        if (body.IsEmpty)
        {
            return [];
        }

        var source = body.Text;
        var firstLine = body.FirstSourceLine;
        var tokens = CodeLexer.Tokenize(source, CodeLanguage.CAL, firstLine);
        context.CodeLines += body.Lines.Count;

        var reported = new HashSet<(string, int, string?)>();
        void Report(CodeRule rule, int line, string? construct)
        {
            if (!reported.Add((rule.Descriptor.Code, line, construct ?? rule.Construct)))
            {
                return;
            }

            context.Report(rule.Descriptor, rule.Message, line, construct: construct ?? rule.Construct,
                resolution: rule.Resolution, severity: rule.Severity, member: memberDisplayName);
        }

        CodeBodySyntax? parsed = null;
        try
        {
            parsed = CodeParser.ParseBody(tokens);
            var metrics = CodeMetrics.Compute(parsed);
            context.Statements += metrics.Statements;
            context.DatabaseOperations += metrics.DatabaseOperations;
            CodeRules.Analyze(parsed, source, context, Report);
        }
        catch (CodeParseException ex)
        {
            context.Report(DiagnosticDescriptors.CodeSyntaxError,
                $"Code could not be parsed: {ex.Message}",
                ex.Token.Line, construct: SourceLine(body, ex.Token.Line), reason: ex.Message, member: memberDisplayName);
            CodeRules.AnalyzeTokens(tokens, Report);
        }

        var output = Rewrite(tokens, memberDisplayName);

        if (context.Options.VerifyCodePreservation)
        {
            Verify(tokens, output, firstLine, memberDisplayName, parsed is not null);
        }

        return output.Split('\n').Select(l => l.TrimEnd()).ToList();
    }

    private string Rewrite(List<CodeToken> tokens, string member)
    {
        var sb = new StringBuilder();
        CodeToken? previousSignificant = null;
        foreach (var token in tokens)
        {
            switch (token.Kind)
            {
                case CodeTokenKind.BraceComment:
                    sb.Append(ConvertBraceComment(token, member));
                    break;

                case CodeTokenKind.Date:
                    sb.Append(ConvertDate(token, member));
                    break;

                case CodeTokenKind.Identifier
                    when context.IdentifiersToQuote.Contains(token.Text)
                         && previousSignificant?.Kind is not (CodeTokenKind.Dot or CodeTokenKind.DoubleColon):
                    sb.Append('"').Append(token.Text).Append('"');
                    break;

                default:
                    sb.Append(token.Text);
                    break;
            }

            if (!token.IsTrivia)
            {
                previousSignificant = token;
            }
        }

        return sb.ToString();
    }

    private string ConvertBraceComment(CodeToken token, string member)
    {
        var inner = token.Text.Length >= 2 && token.Text[^1] == '}' ? token.Text[1..^1] : token.Text[1..];
        if (inner.Contains("*/", StringComparison.Ordinal))
        {
            context.Report(DiagnosticDescriptors.CommentAdjusted,
                "A { } comment contained '*/', which would end an AL block comment early; it was written as '* /'.",
                token.Line, construct: "{ … */ … }", member: member);
            inner = inner.Replace("*/", "* /", StringComparison.Ordinal);
        }

        return "/*" + inner + "*/";
    }

    private static string Format(CalDateOrder order) => order == CalDateOrder.DayMonthYear ? "DDMMYY[YY]D" : "MMDDYY[YY]D";

    private string ConvertDate(CodeToken token, string member)
    {
        var order = context.DateOrder;
        if (!DateLiteral.TryParseCal(token.Text, order, out var value))
        {
            context.Report(DiagnosticDescriptors.DateConstant,
                $"'{token.Text}' is not a valid date when read as {Format(order)}; it was emitted unchanged and will not compile in AL.",
                token.Line, construct: token.Text, member: member, severity: DiagnosticSeverity.Error,
                resolution: "Write the date as an AL constant (yyyymmddD).");
            return token.Text;
        }

        if (value.IsBlank)
        {
            return token.Text;
        }

        var converted = DateLiteral.ToAl(value);
        context.Report(DiagnosticDescriptors.DateConstant,
            $"Date constant {token.Text} ({value.Date:yyyy-MM-dd}) was rewritten as {converted}: the export writes dates {Format(order)}, AL writes yyyymmddD."
            + (value.TwoDigitYear ? $" The two-digit year was read as {value.Date.Year} (00–{DateLiteral.CenturyPivot - 1} → 20xx)." : string.Empty),
            token.Line, construct: token.Text, member: member,
            severity: value.TwoDigitYear ? DiagnosticSeverity.Warning : DiagnosticSeverity.Info);
        return converted;
    }

    private void Verify(List<CodeToken> original, string output, int firstLine, string member, bool originalParsed)
    {
        var outputTokens = CodeLexer.Tokenize(output, CodeLanguage.AL, firstLine);
        var difference = TokenStreamComparer.FindDifference(original, outputTokens, context.DateOrder);
        if (difference is not null)
        {
            context.Report(DiagnosticDescriptors.PreservationCheckFailed,
                $"The converted code of '{member}' differs from the original: {difference}.",
                firstLine, member: member);
            return;
        }

        if (!originalParsed)
        {
            return;
        }

        try
        {
            var reparsed = CodeParser.ParseBody(outputTokens);
            var metrics = CodeMetrics.Compute(reparsed);
            context.OutputStatements += metrics.Statements;
            context.OutputDatabaseOperations += metrics.DatabaseOperations;
        }
        catch (CodeParseException ex)
        {
            context.Report(DiagnosticDescriptors.PreservationCheckFailed,
                $"The converted code of '{member}' no longer parses: {ex.Message}.", ex.Token.Line, member: member);
        }
    }

    private static string? SourceLine(CalCodeBody body, int line) =>
        body.Lines.FirstOrDefault(l => l.SourceLine == line)?.Text.Trim();
}
