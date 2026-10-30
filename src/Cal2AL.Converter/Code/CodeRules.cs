using Cal2AL.Diagnostics;
using Cal2AL.Parser.Code;

namespace Cal2AL.Converter.Code;

/// <summary>A C/AL construct that needs attention in AL, and what to tell the developer about it.</summary>
public sealed record CodeRule(
    DiagnosticDescriptor Descriptor,
    string Construct,
    string Message,
    string? Resolution = null,
    DiagnosticSeverity? Severity = null);

/// <summary>
/// Knowledge about C/AL functions and statements whose behaviour or availability changed in AL. The rules only
/// produce diagnostics; they never rewrite or remove code, so the compiler and the developer see every occurrence.
/// </summary>
public static class CodeRules
{
    /// <summary>Global (unqualified) functions, keyed by upper-case name.</summary>
    public static readonly IReadOnlyDictionary<string, CodeRule> GlobalFunctions = new Dictionary<string, CodeRule>(StringComparer.OrdinalIgnoreCase)
    {
        ["ISSERVICETIER"] = new(DiagnosticDescriptors.UnsupportedFunction, "ISSERVICETIER",
            "ISSERVICETIER does not exist in AL.",
            "Code always runs on the service tier in Business Central, so ISSERVICETIER was always TRUE there: replace it with TRUE and remove the classic-client branch."),
        ["SHELL"] = new(DiagnosticDescriptors.UnsupportedFunction, "SHELL",
            "SHELL (starting a program on the client) does not exist in AL.",
            "Use Hyperlink() for URLs and documents, or a .NET/Azure Function based replacement for server-side processes."),
        ["ENVIRON"] = new(DiagnosticDescriptors.ObsoleteFunction, "ENVIRON",
            "ENVIRON (reading client environment variables) is not available in AL.",
            "Read the setting from setup data, a configuration table or the server instead."),
        ["CREATE"] = new(DiagnosticDescriptors.AutomationNotSupported, "CREATE",
            "CREATE instantiates an Automation (COM) object, which AL does not support."),
        ["UPLOAD"] = new(DiagnosticDescriptors.OnPremOnly, "UPLOAD",
            "UPLOAD stores the client file on the server file system, which is only possible on-premises.",
            "Use UploadIntoStream() and work with streams for cloud compatibility."),
        ["DOWNLOAD"] = new(DiagnosticDescriptors.OnPremOnly, "DOWNLOAD",
            "DOWNLOAD reads a file from the server file system, which is only possible on-premises.",
            "Use DownloadFromStream() with a stream for cloud compatibility."),
        ["TEMPORARYPATH"] = new(DiagnosticDescriptors.OnPremOnly, "TEMPORARYPATH",
            "TEMPORARYPATH exposes the server file system, which is only possible on-premises."),
        ["APPLICATIONPATH"] = new(DiagnosticDescriptors.OnPremOnly, "APPLICATIONPATH",
            "APPLICATIONPATH exposes the server file system, which is only possible on-premises."),
        ["EXPORTOBJECTS"] = new(DiagnosticDescriptors.UnsupportedFunction, "EXPORTOBJECTS",
            "EXPORTOBJECTS exports C/AL objects, which do not exist in Business Central."),
        ["IMPORTOBJECTS"] = new(DiagnosticDescriptors.UnsupportedFunction, "IMPORTOBJECTS",
            "IMPORTOBJECTS imports C/AL objects, which do not exist in Business Central."),
        ["RECORDLEVELLOCKING"] = new(DiagnosticDescriptors.ObsoleteFunction, "RECORDLEVELLOCKING",
            "RECORDLEVELLOCKING is a leftover of the native C/SIDE database; on SQL Server it was always TRUE.",
            "Replace it with TRUE and simplify the code."),
        ["YIELD"] = new(DiagnosticDescriptors.ObsoleteFunction, "YIELD",
            "YIELD has no effect on the service tier.",
            "Remove the call."),
    };

    /// <summary>Methods on built-in objects, keyed by "OBJECT.METHOD" (upper case).</summary>
    public static readonly IReadOnlyDictionary<string, CodeRule> Members = new Dictionary<string, CodeRule>(StringComparer.OrdinalIgnoreCase)
    {
        ["CURRREPORT.CREATETOTALS"] = new(DiagnosticDescriptors.ObsoleteFunction, "CurrReport.CREATETOTALS",
            "CurrReport.CREATETOTALS belongs to classic report sections and has no effect in RDLC/Word reports.",
            "Calculate totals in the dataset or in the layout."),
        ["CURRREPORT.TOTALSCAUSEDBY"] = new(DiagnosticDescriptors.ObsoleteFunction, "CurrReport.TOTALSCAUSEDBY",
            "CurrReport.TOTALSCAUSEDBY belongs to classic report sections.",
            "Calculate group totals in the dataset or in the layout."),
        ["CURRREPORT.SHOWOUTPUT"] = new(DiagnosticDescriptors.ObsoleteFunction, "CurrReport.SHOWOUTPUT",
            "CurrReport.SHOWOUTPUT controlled classic report sections; RDLC and Word layouts ignore it.",
            "Control visibility in the layout instead."),
        ["CURRREPORT.NEWPAGEPERRECORD"] = new(DiagnosticDescriptors.ObsoleteFunction, "CurrReport.NEWPAGEPERRECORD",
            "CurrReport.NEWPAGEPERRECORD controlled classic report sections.",
            "Use page breaks in the layout instead."),
        ["CURRPAGE.UPDATECONTROLS"] = new(DiagnosticDescriptors.ObsoleteFunction, "CurrPage.UPDATECONTROLS",
            "CurrPage.UPDATECONTROLS is a classic-form function.",
            "Use CurrPage.Update(false)."),
    };

    /// <summary>Objects whose every use is an error (classic client).</summary>
    public static readonly IReadOnlyDictionary<string, CodeRule> ClassicObjects = new Dictionary<string, CodeRule>(StringComparer.OrdinalIgnoreCase)
    {
        ["FORM"] = new(DiagnosticDescriptors.FormReference, "FORM", "Reference to a classic form (FORM)."),
        ["CURRFORM"] = new(DiagnosticDescriptors.FormReference, "CURRFORM", "Reference to the current classic form (CURRFORM).",
            "Use CurrPage with the corresponding page functions."),
        ["DATAPORT"] = new(DiagnosticDescriptors.UnsupportedFunction, "DATAPORT", "Reference to a classic dataport (DATAPORT).",
            "Use the XMLport that replaced the dataport (XMLPORT.RUN/IMPORT/EXPORT)."),
        ["CURRDATAPORT"] = new(DiagnosticDescriptors.UnsupportedFunction, "CURRDATAPORT", "Reference to the current classic dataport (CURRDATAPORT).",
            "Use currXMLport in the XMLport that replaced the dataport."),
    };

    /// <summary>Static methods of the File type, which are only available on-premises.</summary>
    public static readonly HashSet<string> FileStaticMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "EXISTS", "ERASE", "COPY", "RENAME", "UPLOAD", "DOWNLOAD", "DOWNLOADTOFILE", "UPLOADINTOSTREAM", "DOWNLOADFROMSTREAM",
        "GETSTAMP", "SETSTAMP", "ISPATHTEMPORARY",
    };

    /// <summary>Finds the rules that apply to a parsed body and reports them through <paramref name="report"/>.</summary>
    public static void Analyze(CodeBodySyntax body, string source, ObjectContext context, Action<CodeRule, int, string?> report)
    {
        foreach (var node in body.AllNodes())
        {
            switch (node)
            {
                case WithStatement with:
                    report(new CodeRule(DiagnosticDescriptors.WithStatement, "WITH",
                        $"WITH {Text(with.Record, source)} DO — implicit member access."), with.Line, null);
                    break;

                case InvocationExpression { Target: NameExpression callee } call:
                    if (callee.Identifier.Equals("CREATE", StringComparison.OrdinalIgnoreCase)
                        && call.Arguments.Count > 0 && call.Arguments[0] is NameExpression target
                        && !context.AutomationVariables.Contains(target.Identifier))
                    {
                        // CREATE on something that is not an Automation variable: not the COM function (e.g. a local procedure).
                        break;
                    }

                    if (GlobalFunctions.TryGetValue(callee.Identifier, out var rule))
                    {
                        report(rule, call.Line, Text(call, source));
                    }

                    break;

                case ExpressionStatement { Expression: NameExpression name } statement when GlobalFunctions.TryGetValue(name.Identifier, out var bareRule)
                    && !name.Identifier.Equals("CREATE", StringComparison.OrdinalIgnoreCase):
                    report(bareRule, statement.Line, name.Identifier);
                    break;

                case NameExpression { Identifier: var id } n when id.Equals("ISSERVICETIER", StringComparison.OrdinalIgnoreCase)
                    || id.Equals("RECORDLEVELLOCKING", StringComparison.OrdinalIgnoreCase):
                    // used as a value (IF ISSERVICETIER THEN …)
                    if (GlobalFunctions.TryGetValue(id, out var valueRule))
                    {
                        report(valueRule, n.Line, id);
                    }

                    break;

                case MemberAccessExpression { Target: NameExpression owner } member:
                    if (ClassicObjects.TryGetValue(owner.Identifier, out var classic))
                    {
                        report(classic, member.Line, $"{owner.Identifier}.{member.MemberName}");
                    }
                    else if (Members.TryGetValue($"{owner.Identifier}.{member.MemberName}", out var memberRule))
                    {
                        report(memberRule, member.Line, $"{owner.Identifier}.{member.MemberName}");
                    }
                    else if (owner.Identifier.Equals("FILE", StringComparison.OrdinalIgnoreCase) && FileStaticMethods.Contains(member.MemberName))
                    {
                        report(new CodeRule(DiagnosticDescriptors.OnPremOnly, $"FILE.{member.MemberName}",
                            $"FILE.{member.MemberName} works on the server file system, which is only possible on-premises."), member.Line, $"FILE.{member.MemberName}");
                    }

                    break;

                case OptionAccessExpression { Target: NameExpression owner } option when ClassicObjects.TryGetValue(owner.Identifier, out var classicOption):
                    report(classicOption, option.Line, $"{owner.Identifier}::{option.Name.Text}");
                    break;
            }
        }
    }

    /// <summary>Token-based fallback used when a body could not be parsed.</summary>
    public static void AnalyzeTokens(IReadOnlyList<CodeToken> tokens, Action<CodeRule, int, string?> report)
    {
        var significant = tokens.Where(t => !t.IsTrivia && t.Kind != CodeTokenKind.EndOfInput).ToList();
        for (var i = 0; i < significant.Count; i++)
        {
            var t = significant[i];
            if (t.Kind != CodeTokenKind.Identifier)
            {
                continue;
            }

            var previous = i > 0 ? significant[i - 1].Kind : CodeTokenKind.Semicolon;
            if (previous is CodeTokenKind.Dot or CodeTokenKind.DoubleColon)
            {
                continue;
            }

            var next = i + 1 < significant.Count ? significant[i + 1].Kind : CodeTokenKind.EndOfInput;
            if (t.Is("WITH"))
            {
                report(new CodeRule(DiagnosticDescriptors.WithStatement, "WITH", "WITH statement — implicit member access."), t.Line, null);
            }
            else if (ClassicObjects.TryGetValue(t.Text, out var classic) && next is CodeTokenKind.Dot or CodeTokenKind.DoubleColon)
            {
                report(classic, t.Line, t.Text);
            }
            else if (GlobalFunctions.TryGetValue(t.Text, out var rule) && !t.Is("CREATE"))
            {
                report(rule, t.Line, t.Text);
            }
        }
    }

    private static string Text(SyntaxNode node, string source)
    {
        var start = Math.Clamp(node.FirstToken.Start, 0, source.Length);
        var end = Math.Clamp(node.LastToken.End, start, source.Length);
        var text = string.Join(" ", source[start..end].Split((char[])['\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()));
        return text.Length > 80 ? text[..80] + "…" : text;
    }
}
