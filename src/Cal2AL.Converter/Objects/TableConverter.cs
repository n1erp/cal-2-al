using System.Text.RegularExpressions;
using Cal2AL.Converter.Code;
using Cal2AL.Converter.Properties;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;
using Cal2AL.Generator.Model;

namespace Cal2AL.Converter.Objects;

/// <summary>Converts tables: properties, triggers, fields (with their triggers), keys, field groups and code.</summary>
public sealed partial class TableConverter : ObjectConverterBase
{
    public override bool CanConvert(CalObject obj) => obj.Type == CalObjectType.Table;

    protected override AlObject ConvertObject(ObjectContext context, MemberConverter members, PropertyConverter properties, ObjectConversionOutput output)
    {
        var obj = context.Object;
        var al = CreateObject(context, "table");
        al.Properties.AddRange(properties.Convert(obj, PropertyScope.Table, new PropertyContext { Path = $"Table {obj.Name}", CurrentTableId = obj.Id }));

        var fields = ConvertFields(context, members, properties, obj.GetItems("FIELDS"), al.Procedures);
        al.Sections.Add(fields);

        var keys = ConvertKeys(context, properties, obj.GetItems("KEYS"));
        if (keys.Children.Count > 0)
        {
            al.Sections.Add(keys);
        }

        var groups = new AlSection { Header = "fieldgroups" };
        foreach (var item in obj.GetItems("FIELDGROUPS"))
        {
            context.CurrentElement = $"FieldGroup {item.GetPositional(1).Trim()}";
            var group = new AlSection
            {
                Header = $"fieldgroup({AlSyntax.IdentifierFromSource(item.GetPositional(1).Trim())}; {IdentifierList(item.GetPositional(2))})",
            };
            group.Properties.AddRange(properties.Convert(item, PropertyScope.FieldGroup, new PropertyContext { Path = $"FieldGroup {item.GetPositional(1).Trim()}", CurrentTableId = obj.Id }));
            groups.Children.Add(group);
        }

        context.CurrentElement = null;
        if (groups.Children.Count > 0)
        {
            al.Sections.Add(groups);
        }

        var orphans = new List<AlMethod>();
        ConvertTriggers(context, members, obj, TriggerOwner.Table, string.Empty, al.Triggers, orphans);
        AddCode(context, members, al);
        al.Procedures.AddRange(orphans);
        return al;
    }

    /// <summary>Converts a list of field items to a <c>fields</c> section. Shared with table extensions.</summary>
    internal static AlSection ConvertFields(ObjectContext context, MemberConverter members, PropertyConverter properties, IReadOnlyList<CalElement> items, List<AlMethod> orphanTarget, int? tableId = null)
    {
        var section = new AlSection { Header = "fields" };
        tableId ??= context.Object.Type == CalObjectType.Table ? context.Object.Id : null;
        foreach (var item in items)
        {
            var number = item.GetPositional(0).Trim();
            var name = item.GetPositional(2).Trim();
            if (AlSyntax.ContainsDoubleQuote(name))
            {
                context.Report(DiagnosticDescriptors.NameChanged,
                    $"Field name '{name}' contains a double quote, which AL identifiers cannot contain; it was renamed to {AlSyntax.Identifier(name)}.",
                    item.Position.Line, construct: name);
            }
            var dataType = item.GetPositional(3).Trim();
            context.CurrentElement = $"Field {number} \"{name}\"";

            var field = new AlSection
            {
                Header = $"field({number}; {AlSyntax.Identifier(name)}; {ConvertFieldType(context, item, dataType)})",
            };

            if (item.GetPositional(1).Trim().Equals("No", StringComparison.OrdinalIgnoreCase))
            {
                field.Properties.Add(new AlProperty("Enabled", "false"));
                context.Report(DiagnosticDescriptors.DisabledElementKept,
                    $"Field {number} \"{name}\" is disabled in C/AL; it was kept with Enabled = false.", item.Position.Line);
            }

            field.Properties.AddRange(properties.Convert(item, PropertyScope.Field, new PropertyContext
            {
                Path = $"Field {name}",
                CurrentTableId = tableId,
                FieldDataType = dataType,
            }));

            var orphans = new List<AlMethod>();
            ConvertTriggers(context, members, item, TriggerOwner.TableField, $"Field \"{name}\"", field.Triggers, orphans);
            orphanTarget.AddRange(orphans);
            section.Children.Add(field);
        }

        context.CurrentElement = null;
        return section;
    }

    [GeneratedRegex(@"^(?<name>Code|Text)(?<len>\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex LengthTypeRegex();

    internal static string ConvertFieldType(ObjectContext context, CalElement field, string dataType)
    {
        var m = LengthTypeRegex().Match(dataType);
        if (m.Success)
        {
            var baseName = m.Groups["name"].Value.Equals("Code", StringComparison.OrdinalIgnoreCase) ? "Code" : "Text";
            return $"{baseName}[{m.Groups["len"].Value}]";
        }

        switch (dataType.ToUpperInvariant())
        {
            case "OPTION" when field.Properties.IsYes("Extensible") && field.Properties.Contains("EnumTypeName"):
                return ConvertExtensibleOption(context, field);
            case "BLOB":
                return "Blob";
            case "GUID":
                return "Guid";
            case "RECORDID":
                return "RecordId";
            case "DATEFORMULA":
                return "DateFormula";
            case "TABLEFILTER":
                return "TableFilter";
            case "BIGINTEGER":
                return "BigInteger";
            case "DATETIME":
                return "DateTime";
            case "MEDIASET":
                return "MediaSet";
            case "BINARY":
                context.Report(DiagnosticDescriptors.UnsupportedDataType,
                    $"Field type {dataType} does not exist in AL.", field.Position.Line, construct: dataType,
                    resolution: "Change the field to Blob, Code or Text and migrate the data.");
                return dataType;
        }

        return dataType.Length > 0 ? char.ToUpperInvariant(dataType[0]) + dataType[1..] : dataType;
    }

    private static string ConvertExtensibleOption(ObjectContext context, CalElement field)
    {
        var enumName = field.Properties["EnumTypeName"]!.Trim();
        var idText = field.Properties["EnumTypeId"];
        var members = (field.Properties["OptionString"] ?? string.Empty).Split(',');
        var captions = new List<string?>();
        var mlCaptions = field.Properties["OptionCaptionML"];
        var enuCaptions = mlCaptions is null ? null : Values.MultiLanguageText.Parse(mlCaptions)?.FirstOrDefault(e => e.LanguageCode == "ENU")?.Text.Split(',');
        for (var i = 0; i < members.Length; i++)
        {
            captions.Add(enuCaptions is not null && i < enuCaptions.Length ? enuCaptions[i] : null);
        }

        if (int.TryParse(idText, out var enumId))
        {
            var error = context.Run.Enums.Add(new EnumDeclaration(enumId, enumName, true, members, captions));
            if (error is not null)
            {
                context.Report(DiagnosticDescriptors.EnumGenerated, error, field.Position.Line, severity: DiagnosticSeverity.Error);
            }
            else
            {
                context.Report(DiagnosticDescriptors.EnumGenerated,
                    $"Extensible option field became enum {enumId} {AlSyntax.Identifier(enumName)} (generated as a separate enum object).", field.Position.Line);
            }
        }
        else
        {
            context.Report(DiagnosticDescriptors.MissingRequiredPart,
                $"Extensible option field has no valid EnumTypeId ('{idText}'); the enum could not be generated.", field.Position.Line);
        }

        return $"Enum {AlSyntax.Identifier(enumName)}";
    }

    internal static AlSection ConvertKeys(ObjectContext context, PropertyConverter properties, IReadOnlyList<CalElement> items)
    {
        var section = new AlSection { Header = "keys" };
        var index = 1;
        foreach (var item in items)
        {
            var fields = item.GetPositional(1);
            context.CurrentElement = $"Key {fields.Trim()}";
            if (fields.Trim().Length == 0)
            {
                // C/SIDE exports placeholder rows such as "{ No ; }": a disabled key without fields. AL cannot declare
                // a key without fields, and there is nothing to carry over.
                context.Report(DiagnosticDescriptors.DisabledElementKept,
                    "A key without fields (a disabled C/SIDE placeholder) was not emitted; AL keys need at least one field.",
                    item.Position.Line, construct: "KEYS");
                continue;
            }

            var key = new AlSection { Header = $"key(Key{index}; {IdentifierList(fields)})" };
            if (item.GetPositional(0).Trim().Equals("No", StringComparison.OrdinalIgnoreCase))
            {
                key.Properties.Add(new AlProperty("Enabled", "false"));
                context.Report(DiagnosticDescriptors.DisabledElementKept,
                    $"Key {fields.Trim()} is disabled in C/AL; it was kept with Enabled = false (Txt2Al dropped disabled keys).", item.Position.Line);
            }

            key.Properties.AddRange(properties.Convert(item, PropertyScope.Key, new PropertyContext
            {
                Path = $"Key {fields.Trim()}",
                CurrentTableId = context.Object.Type == CalObjectType.Table ? context.Object.Id : null,
            }));
            section.Children.Add(key);
            index++;
        }

        context.CurrentElement = null;
        return section;
    }

    internal static string IdentifierList(string value) =>
        string.Join(", ", Values.FilterExpressionConverter.SplitTopLevel(Values.FilterExpressionConverter.Flatten(value), ',')
            .Select(f => AlSyntax.IdentifierFromSource(f.Trim())));
}
