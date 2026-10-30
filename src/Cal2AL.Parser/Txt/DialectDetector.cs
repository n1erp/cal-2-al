using System.Text.RegularExpressions;
using Cal2AL.Core.Model;

namespace Cal2AL.Parser.Txt;

/// <summary>
/// Decides whether an object was exported classically or with ExportToNewSyntax by counting evidence:
/// object references by number (<c>Record 18</c>, <c>SourceTable=Table18</c>) point to classic, references by
/// name and quoted field names in filter properties point to new syntax.
/// </summary>
public static partial class DialectDetector
{
    [GeneratedRegex(@"^(Table|Page|Report|Codeunit|XMLport|Query)\s?\d+$", RegexOptions.IgnoreCase)]
    private static partial Regex ClassicReferenceRegex();

    // A quoted field name directly before '=FIELD(' / '=CONST(' / '=FILTER(' only occurs in new-syntax filters.
    [GeneratedRegex(@"""\s*=\s*(FIELD|CONST|FILTER)\(", RegexOptions.IgnoreCase)]
    private static partial Regex QuotedFilterFieldRegex();

    // An unquoted field name containing a space or dot before '=FIELD(' etc. only occurs in classic filters.
    [GeneratedRegex(@"[A-Za-z0-9][ .][A-Za-z0-9.]*\s*=\s*(FIELD|CONST|FILTER)\(", RegexOptions.IgnoreCase)]
    private static partial Regex UnquotedFilterFieldRegex();

    private static readonly string[] ObjectReferenceProperties =
        ["SourceTable", "DataItemTable", "LookupPageID", "DrillDownPageID", "CardPageID", "PagePartID", "RunObject"];

    private static readonly string[] FilterProperties =
        ["TableRelation", "CalcFormula", "SubPageLink", "RunPageLink", "DataItemLink", "SourceTableView", "DataItemTableView", "RunPageView", "SubPageView"];

    public static InputDialect Detect(CalObject obj)
    {
        var classic = 0;
        var newSyntax = 0;

        foreach (var variable in AllVariables(obj))
        {
            var subtype = variable.Type.Subtype;
            if (subtype is null)
            {
                continue;
            }

            if (subtype.All(char.IsDigit))
            {
                classic++;
            }
            else
            {
                newSyntax++;
            }
        }

        foreach (var property in AllProperties(obj))
        {
            if (ObjectReferenceProperties.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
            {
                if (ClassicReferenceRegex().IsMatch(property.Value.Trim()))
                {
                    classic++;
                }
                else if (property.Value.Trim().StartsWith('"'))
                {
                    newSyntax++;
                }
            }
            else if (FilterProperties.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
            {
                if (QuotedFilterFieldRegex().IsMatch(property.Value))
                {
                    newSyntax++;
                }
                else if (UnquotedFilterFieldRegex().IsMatch(property.Value))
                {
                    classic++;
                }
            }
        }

        if (classic == newSyntax)
        {
            return InputDialect.Unknown;
        }

        return classic > newSyntax ? InputDialect.Classic : InputDialect.NewSyntax;
    }

    private static IEnumerable<CalVariable> AllVariables(CalObject obj)
    {
        foreach (var v in obj.Code.Variables)
        {
            yield return v;
        }

        foreach (var p in obj.Code.Procedures)
        {
            foreach (var v in p.Parameters.Concat(p.Variables))
            {
                yield return v;
            }

            if (p.ReturnValue is not null)
            {
                yield return p.ReturnValue;
            }
        }
    }

    private static IEnumerable<CalProperty> AllProperties(CalNode node)
    {
        foreach (var p in node.Properties)
        {
            yield return p;
        }

        foreach (var section in node.Sections)
        {
            foreach (var item in section.Items)
            {
                foreach (var p in AllProperties(item))
                {
                    yield return p;
                }
            }
        }
    }
}
