using System.Text.RegularExpressions;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;

namespace Cal2AL.Converter.Values;

/// <summary>
/// Converts references to objects. Classic exports refer by number (<c>Page21</c>, <c>Record 18</c>,
/// <c>TableData 21=rimd</c>); AL prefers names. A reference is turned into the object's name when that object is
/// part of the converted set; otherwise the number is kept (AL accepts numeric IDs) and an info diagnostic says so.
/// </summary>
public static partial class ObjectReferences
{
    [GeneratedRegex(@"^(?<type>TableData|Table|Page|Report|Codeunit|XMLport|Query|MenuSuite|Form|Dataport|System)\s*(?<rest>.*)$", RegexOptions.IgnoreCase)]
    private static partial Regex TypedReferenceRegex();

    /// <summary>Resolves a reference whose type is known from context (SourceTable, TableNo, LookupPageID, a Record subtype…).</summary>
    public static string Resolve(ObjectContext context, string reference, CalObjectType expectedType, int? line)
    {
        var trimmed = reference.Trim();
        var m = TypedReferenceRegex().Match(trimmed);
        if (m.Success && m.Groups["rest"].Value.Trim().Length > 0)
        {
            var type = CalObjectTypeExtensions.ParseObjectType(m.Groups["type"].Value);
            if (type != CalObjectType.Unknown)
            {
                expectedType = type;
                trimmed = m.Groups["rest"].Value.Trim();
            }
        }

        if (int.TryParse(trimmed, out var id))
        {
            var name = context.Symbols.GetName(expectedType, id);
            if (name is not null)
            {
                return AlSyntax.Identifier(name);
            }

            if (context.ReportedUnresolvedReferences.Add((expectedType, id)))
            {
                context.Report(DiagnosticDescriptors.UnresolvedObjectReference,
                    $"{expectedType} {id} is not in the converted set; references to it were kept as ID {id}.",
                    line, construct: $"{expectedType} {id}");
            }
            return id.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return AlSyntax.IdentifierFromSource(trimmed);
    }

    /// <summary>Converts <c>RunObject=Page 21</c> / <c>Page "Customer Card"</c> to <c>Page "Customer Card"</c>.</summary>
    public static string ResolveRunObject(ObjectContext context, string value, int? line)
    {
        var trimmed = value.Trim();
        var m = TypedReferenceRegex().Match(trimmed);
        if (!m.Success)
        {
            throw new PropertyExpressionException($"'<ObjectType> <ID or name>' expected but found '{trimmed}'.");
        }

        var type = CalObjectTypeExtensions.ParseObjectType(m.Groups["type"].Value);
        var reference = Resolve(context, m.Groups["rest"].Value, type, line);
        return $"{type.ToAlTypeName().Replace("Record", "Table", StringComparison.Ordinal)} {reference}";
    }

    /// <summary>Converts <c>TableData 21=rimd,Codeunit 80=X</c> permission lists.</summary>
    public static string ConvertPermissions(ObjectContext context, string value, int? line)
    {
        var items = FilterExpressionConverter.SplitTopLevel(FilterExpressionConverter.Flatten(value), ',');
        var result = new List<string>();
        foreach (var raw in items)
        {
            var item = raw.Trim();
            var eq = item.LastIndexOf('=');
            if (eq < 0)
            {
                throw new PropertyExpressionException($"'<Type> <ID>=<permissions>' expected but found '{item}'.");
            }

            var target = item[..eq].Trim();
            var permissions = item[(eq + 1)..].Trim();
            var m = TypedReferenceRegex().Match(target);
            if (!m.Success)
            {
                throw new PropertyExpressionException($"Object type expected in permission '{item}'.");
            }

            var typeWord = m.Groups["type"].Value;
            var type = CalObjectTypeExtensions.ParseObjectType(typeWord);
            var reference = Resolve(context, m.Groups["rest"].Value, type, line);
            var keyword = typeWord.Equals("TableData", StringComparison.OrdinalIgnoreCase) ? "TableData" : type.ToAlTypeName().Replace("Record", "Table", StringComparison.Ordinal);
            result.Add($"{keyword} {reference} = {permissions}");
        }

        return string.Join(",\n", result);
    }
}
