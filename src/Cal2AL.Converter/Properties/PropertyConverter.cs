using System.Text.RegularExpressions;
using Cal2AL.Converter.Values;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;
using Cal2AL.Generator.Model;

namespace Cal2AL.Converter.Properties;

/// <summary>Context a property is converted in.</summary>
public sealed record PropertyContext
{
    /// <summary>Readable path for translations and diagnostics, e.g. <c>Field "No."</c>.</summary>
    public required string Path { get; init; }

    /// <summary>The table whose fields unqualified names refer to (the record of a table/page/data item).</summary>
    public int? CurrentTableId { get; init; }

    /// <summary>Target table for links and views (the linked page's table, the data item's table…).</summary>
    public int? TargetTableId { get; init; }

    /// <summary>For table fields: the field's data type (Code20, Option, Boolean, …).</summary>
    public string? FieldDataType { get; init; }

    /// <summary>For page controls: the group type (Group, Repeater, CueGroup, GridLayout, FixedLayout).</summary>
    public string? GroupType { get; init; }

    public bool IsApiObject { get; init; }
}

/// <summary>Applies <see cref="PropertyRules"/> to the properties of one node.</summary>
public sealed partial class PropertyConverter
{
    private static readonly string[] ApiOnlyProperties = ["EntityName", "EntitySetName", "APIPublisher", "APIGroup", "APIVersion"];
    private readonly ObjectContext context;
    private readonly FilterExpressionConverter filters;

    public PropertyConverter(ObjectContext context)
    {
        this.context = context;
        filters = new FilterExpressionConverter(context.Symbols);
        filters.AssumedBoolean += condition => context.Report(DiagnosticDescriptors.PropertyValueAdjusted,
            $"'{condition}' was converted to a Boolean constant; the field's type could not be checked because its table is not in the converted set.",
            line: currentLine, construct: condition);
    }

    /// <summary>Line of the property being converted, so filter callbacks can report a location.</summary>
    private int? currentLine;

    public FilterExpressionConverter Filters => filters;

    public List<AlProperty> Convert(CalNode node, PropertyScope scope, PropertyContext propertyContext, IReadOnlySet<string>? exclude = null)
    {
        var result = new List<AlProperty>();
        foreach (var property in node.Properties)
        {
            if (exclude?.Contains(property.Name) == true)
            {
                continue;
            }

            currentLine = property.Position.Line;
            var converted = ConvertOne(node, property, scope, propertyContext);
            if (converted is not null)
            {
                result.Add(converted);
            }
        }

        return result;
    }

    private AlProperty? ConvertOne(CalNode node, CalProperty property, PropertyScope scope, PropertyContext pc)
    {
        var line = property.Position.Line;
        var rule = PropertyRules.Find(scope, property.Name);
        if (rule is null)
        {
            return ConvertUnknown(property, scope, line);
        }

        switch (rule.Kind)
        {
            case ValueKind.Consumed:
                return null;
            case ValueKind.NotSupported:
                context.Report(DiagnosticDescriptors.PropertyNotSupported,
                    $"Property '{property.Name}' has no AL equivalent and was not emitted (value: '{Shorten(property.Value)}').",
                    line, construct: property.Name, reason: rule.Reason, original: property.ToString(), severity: rule.Severity);
                return null;
        }

        var inert = GetInertReason(node, property, rule, scope, pc);
        if (inert is not null)
        {
            context.Report(DiagnosticDescriptors.PropertyInert,
                $"Property '{property.Name}' was not emitted: {inert}",
                line, construct: property.Name, original: property.ToString());
            return null;
        }

        var alName = rule.AlName;
        if (rule.CalName.Equals("Layout", StringComparison.OrdinalIgnoreCase)
            && string.Equals(pc.GroupType, "CueGroup", StringComparison.OrdinalIgnoreCase))
        {
            alName = "CuegroupLayout";
        }

        try
        {
            var value = ConvertValue(property, rule, pc, line);
            return value is null ? null : new AlProperty(alName, value);
        }
        catch (PropertyExpressionException ex)
        {
            context.Report(DiagnosticDescriptors.PropertyValueNotParsed,
                $"The value of '{property.Name}' could not be converted ({ex.Message}); it was emitted unchanged.",
                line, construct: property.Name, reason: ex.Message, original: property.ToString());
            return new AlProperty(alName, FilterExpressionConverter.Flatten(property.Value));
        }
    }

    private AlProperty? ConvertUnknown(CalProperty property, PropertyScope scope, int line)
    {
        var lettersOnly = new string(property.Name.Where(char.IsLetter).ToArray());
        if (AlPropertyNames.Contains(lettersOnly))
        {
            context.Report(DiagnosticDescriptors.PropertyUnmapped,
                $"Property '{property.Name}' on a {scope} has no explicit rule; it was emitted as '{lettersOnly}' with a generic value conversion.",
                line, construct: property.Name, original: property.ToString());
            return new AlProperty(lettersOnly, GenericValue(property.Value));
        }

        context.Report(DiagnosticDescriptors.PropertyNotSupported,
            $"Property '{property.Name}' is not known in AL for a {scope} and was not emitted (value: '{Shorten(property.Value)}').",
            line, construct: property.Name, original: property.ToString());
        return null;
    }

    private static string GenericValue(string value)
    {
        var flat = FilterExpressionConverter.Flatten(value);
        return AlUtil.ToBoolean(flat) ?? flat;
    }

    /// <summary>Returns a reason when the property had no effect in C/AL and would be rejected by AL in this position.</summary>
    private static string? GetInertReason(CalNode node, CalProperty property, PropertyRule rule, PropertyScope scope, PropertyContext pc)
    {
        var name = rule.CalName.ToUpperInvariant();
        switch (scope)
        {
            case PropertyScope.Action when name is "PROMOTEDISBIG" or "PROMOTEDCATEGORY" or "PROMOTEDONLY":
                return node.Properties.IsYes("Promoted") ? null : "it only applies to promoted actions and Promoted is not set.";
            case PropertyScope.Field when name is "VALIDATETABLERELATION" or "TESTTABLERELATION":
                return node.Properties.Contains("TableRelation") ? null : "it only applies when TableRelation is set.";
            case PropertyScope.Field when name == "DECIMALPLACES":
                return IsType(pc.FieldDataType, "Decimal") ? null : $"it only applies to Decimal fields (this field is {pc.FieldDataType}).";
            case PropertyScope.Field when name == "BLANKZERO":
                return IsType(pc.FieldDataType, "Decimal", "Integer", "BigInteger", "Boolean", "Duration", "Option")
                    ? null
                    : $"it only applies to numeric, Boolean and Option fields (this field is {pc.FieldDataType}).";
            case PropertyScope.Control when name == "LAYOUT":
                return pc.GroupType?.ToUpperInvariant() is "GRIDLAYOUT" or "CUEGROUP" ? null : "it only applies to grid and cue groups.";
            case PropertyScope.DataItem when name == "DATAITEMTABLEVIEW":
                return property.Value.Trim().Length == 0 ? "the value is empty." : null;
        }

        if (ApiOnlyProperties.Contains(rule.CalName, StringComparer.OrdinalIgnoreCase) && !pc.IsApiObject)
        {
            return "it only applies to API pages and queries.";
        }

        return null;
    }

    private static bool IsType(string? dataType, params string[] types) =>
        dataType is null || types.Any(t => dataType.StartsWith(t, StringComparison.OrdinalIgnoreCase));

    private string? ConvertValue(CalProperty property, PropertyRule rule, PropertyContext pc, int line)
    {
        var value = property.Value;
        var flat = FilterExpressionConverter.Flatten(value);
        switch (rule.Kind)
        {
            case ValueKind.Boolean:
            {
                var b = AlUtil.ToBoolean(flat);
                if (b is null)
                {
                    throw new PropertyExpressionException($"Yes or No expected but found '{flat}'");
                }

                return b;
            }

            case ValueKind.BooleanOrExpression:
                return AlUtil.ToBoolean(flat) ?? flat;

            case ValueKind.Text:
                return AlSyntax.String(flat);

            case ValueKind.Raw:
                return flat;

            case ValueKind.NumberList:
                return string.Join(", ", flat.Trim('[', ']').Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

            case ValueKind.Enum:
                return ConvertEnum(flat, rule);

            case ValueKind.Label:
                return LabelConverter.Convert(context, value, $"{pc.Path} - Property {rule.AlName}", line);

            case ValueKind.ObjectReference:
                return ObjectReferences.Resolve(context, flat, rule.ReferenceType, line);

            case ValueKind.RunObject:
                return ObjectReferences.ResolveRunObject(context, flat, line);

            case ValueKind.TableRelation:
                return filters.ConvertTableRelation(value, pc.CurrentTableId);

            case ValueKind.CalcFormula:
                return filters.ConvertCalcFormula(value, pc.CurrentTableId);

            case ValueKind.TableView:
                return filters.ConvertTableView(value, pc.TargetTableId ?? pc.CurrentTableId);

            case ValueKind.Links:
                return filters.ConvertLinks(value, pc.TargetTableId, pc.CurrentTableId);

            case ValueKind.QueryLink:
                return filters.ConvertQueryLink(value);

            case ValueKind.FieldFilters:
                return filters.ConvertConditionList(flat, pc.TargetTableId ?? pc.CurrentTableId, pc.CurrentTableId);

            case ValueKind.OrderBy:
                return FilterExpressionConverter.ConvertOrderBy(value);

            case ValueKind.IdentifierList:
                return string.Join(", ", FilterExpressionConverter.SplitTopLevel(flat, ',').Select(f => AlSyntax.IdentifierFromSource(f.Trim())));

            case ValueKind.Identifier:
                return AlSyntax.IdentifierFromSource(flat);

            case ValueKind.Permissions:
                return ObjectReferences.ConvertPermissions(context, value, line);

            case ValueKind.ApplicationArea:
                return string.Join(", ", flat.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => AlSyntax.Identifier(a.TrimStart('#'))));

            case ValueKind.OptionMembers:
                return ConvertOptionMembers(value);

            case ValueKind.InitValue:
            case ValueKind.TypedValue:
                // InitValue=[ ] selects the blank first member of an option (" ,Open,Closed"); trimming would lose it.
                if (property.Value.Length > 0 && string.IsNullOrWhiteSpace(property.Value))
                {
                    return IsType(pc.FieldDataType, "Option") ? "\" \"" : AlSyntax.String(property.Value);
                }

                return ConvertTypedValue(property, flat, pc.FieldDataType, line);

            case ValueKind.ValuesAllowed:
                return IsType(pc.FieldDataType, "Code", "Text") && pc.FieldDataType is not null
                    ? string.Join(", ", flat.Split(',').Select(v => AlSyntax.String(v.Trim())))
                    : string.Join(", ", flat.Split(',').Select(v => v.Trim()));

            case ValueKind.Namespaces:
                return string.Join(", ", flat.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(pair =>
                {
                    var eq = pair.IndexOf('=');
                    var prefix = eq < 0 ? string.Empty : pair[..eq].Trim();
                    var uri = eq < 0 ? pair.Trim() : pair[(eq + 1)..].Trim();
                    return $"{(prefix.Length == 0 ? "\"\"" : AlSyntax.Identifier(prefix))} = {AlSyntax.String(uri)}";
                }));

            case ValueKind.ApiVersion:
                return string.Join(", ", flat.Split(',').Select(v => AlSyntax.String(AlSyntax.UnquoteString(v.Trim()))));

            default:
                return flat;
        }
    }

    [GeneratedRegex(@"[\s\-/.]")]
    private static partial Regex EnumStripRegex();

    private static string ConvertEnum(string value, PropertyRule rule)
    {
        var unquoted = AlSyntax.Unquote(value);
        if (rule.EnumMap is not null && rule.EnumMap.TryGetValue(unquoted, out var mapped))
        {
            return mapped;
        }

        var stripped = EnumStripRegex().Replace(unquoted, string.Empty);
        return AlSyntax.IsPlainIdentifier(stripped) ? stripped : AlSyntax.Identifier(unquoted);
    }

    private static string ConvertOptionMembers(string value)
    {
        // Option strings are a single line; continuation lines would be part of member names, so join them.
        var members = value.Replace("\n", string.Empty, StringComparison.Ordinal).Split(',');
        return string.Join(",", members.Select(m => m.Length == 0 ? string.Empty : AlSyntax.Identifier(m)));
    }

    private string? ConvertTypedValue(CalProperty property, string value, string? dataType, int line)
    {
        var type = (dataType ?? string.Empty).ToUpperInvariant();
        if (type.StartsWith("CODE", StringComparison.Ordinal) || type.StartsWith("TEXT", StringComparison.Ordinal))
        {
            return AlSyntax.String(AlSyntax.UnquoteString(value));
        }

        switch (type)
        {
            case "BOOLEAN":
                return AlUtil.ToBoolean(value) ?? value;
            case "OPTION":
                return AlSyntax.IdentifierFromSource(value);
            case "DECIMAL":
                return value.Replace(',', '.');
            case "DATEFORMULA":
                return AlSyntax.String(AlSyntax.UnquoteString(value));
            case "TIME":
            {
                var m = Regex.Match(value, @"^(\d{1,2}):(\d{2})(?::(\d{2}))?$");
                if (m.Success)
                {
                    var seconds = m.Groups[3].Success ? m.Groups[3].Value : "00";
                    return $"{int.Parse(m.Groups[1].Value):00}{m.Groups[2].Value}{seconds}T";
                }

                return value;
            }

            case "DATE":
                if (value.Trim() is "0D" or "")
                {
                    return "0D";
                }

                context.Report(DiagnosticDescriptors.PropertyValueNotParsed,
                    $"Date value '{value}' of '{property.Name}' depends on the regional settings used for the export; it was emitted unchanged.",
                    line, construct: property.Name, original: property.ToString(),
                    resolution: "Rewrite the value as an AL date constant (yyyymmddD) or set it in code.");
                return value;
            case "DATETIME":
                context.Report(DiagnosticDescriptors.PropertyNotSupported,
                    $"'{property.Name}' on a DateTime field cannot be expressed as an AL constant (AL only has 0DT); it was not emitted.",
                    line, construct: property.Name, original: property.ToString(), severity: DiagnosticSeverity.Error,
                    resolution: "Assign the value in the OnInsert trigger with CreateDateTime().");
                return null;
            default:
                return value;
        }
    }

    private static string Shorten(string value)
    {
        var flat = value.Replace("\n", " ", StringComparison.Ordinal);
        return flat.Length > 80 ? flat[..80] + "…" : flat;
    }
}

/// <summary>Small helpers shared by converters.</summary>
public static class AlUtil
{
    /// <summary>Returns "true"/"false" for Yes/No/TRUE/FALSE, otherwise null.</summary>
    public static string? ToBoolean(string value) => value.Trim().ToUpperInvariant() switch
    {
        "YES" or "TRUE" => "true",
        "NO" or "FALSE" => "false",
        _ => null,
    };
}
