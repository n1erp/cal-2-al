using Cal2AL.Converter.Code;
using Cal2AL.Converter.Properties;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;
using Cal2AL.Generator.Model;

namespace Cal2AL.Converter.Objects;

/// <summary>Converts XMLports: schema tree, node triggers, request page and code.</summary>
public sealed class XmlPortConverter : ObjectConverterBase
{
    public override bool CanConvert(CalObject obj) => obj.Type == CalObjectType.XmlPort;

    protected override AlObject ConvertObject(ObjectContext context, MemberConverter members, PropertyConverter properties, ObjectConversionOutput output)
    {
        var obj = context.Object;
        var al = CreateObject(context, "xmlport");
        var orphans = new List<AlMethod>();
        al.Properties.AddRange(properties.Convert(obj, PropertyScope.XmlPort, new PropertyContext { Path = $"XMLport {obj.Name}" }));

        var schema = new AlSection { Header = "schema" };
        foreach (var root in BuildTree(obj.GetItems("ELEMENTS"), 1))
        {
            schema.Children.Add(ConvertNode(context, members, properties, root, orphans));
        }

        if (schema.Children.Count == 0)
        {
            ReportMissing(context, "The XMLport has no elements; AL requires at least one node in the schema.", obj.Position.Line);
        }

        al.Sections.Add(schema);

        foreach (var item in obj.GetItems("EVENTS"))
        {
            context.Report(DiagnosticDescriptors.TriggerWithoutAlEquivalent,
                "An entry in the legacy XMLport EVENTS section has no AL equivalent and was not converted; its content is in this diagnostic.",
                item.Position.Line, construct: "EVENTS", original: string.Join(";", item.Positional));
        }

        if (obj.GetSection("REQUESTPAGE")?.Items.FirstOrDefault() is { } requestPage
            && RequestPageConverter.Convert(context, members, properties, requestPage, orphans) is { } section)
        {
            al.Sections.Add(section);
        }

        ConvertTriggers(context, members, obj, TriggerOwner.XmlPort, string.Empty, al.Triggers, orphans);
        AddCode(context, members, al);
        al.Procedures.AddRange(orphans);
        return al;
    }

    private static AlSection ConvertNode(ObjectContext context, MemberConverter members, PropertyConverter properties, ElementNode node, List<AlMethod> orphans)
    {
        var item = node.Element;
        var nodeName = item.GetPositional(2).Trim();
        var nodeType = item.GetPositional(3).Trim();
        var sourceType = item.GetPositional(4).Trim();
        var line = item.Position.Line;
        var isAttribute = nodeType.Equals("Attribute", StringComparison.OrdinalIgnoreCase);
        context.CurrentElement = $"Node {nodeName}";

        if (nodeName.Length == 0)
        {
            ReportMissing(context, "An XMLport node has no name.", line);
        }

        var preferred = item.Properties["VariableName"]?.Trim() is { Length: > 0 } variable ? variable : nodeName;
        // Node names repeat (LineNo under several tables); AL needs unique names, so a repeat becomes LineNo_2 and keeps
        // its XML name through XmlName.
        var fallback = preferred.Length > 0 ? preferred : $"Node{line}";
        var name = context.ReserveName(fallback, fallback, line);
        var identifier = AlSyntax.Identifier(name);
        TriggerOwner owner;
        string header;
        var extra = new List<AlProperty>();

        switch (sourceType.ToUpperInvariant())
        {
            case "TABLE":
            {
                owner = TriggerOwner.XmlPortTableElement;
                var table = item.Properties["SourceTable"];
                if (table is null)
                {
                    ReportMissing(context, $"Table node '{nodeName}' has no SourceTable.", line);
                }

                var tableName = table is null ? "0" : TableReference(context, table, line);
                header = $"{(isAttribute ? "tableattribute" : "tableelement")}({identifier}; {tableName})";
                break;
            }

            case "FIELD":
            {
                owner = TriggerOwner.XmlPortFieldElement;
                var source = item.Properties["SourceField"];
                string field;
                if (source is null)
                {
                    ReportMissing(context, $"Field node '{nodeName}' has no SourceField.", line);
                    field = "''";
                }
                else
                {
                    var separator = source.IndexOf("::", StringComparison.Ordinal);
                    field = separator < 0
                        ? AlSyntax.IdentifierFromSource(source.Trim())
                        : $"{AlSyntax.IdentifierFromSource(source[..separator].Trim())}.{AlSyntax.IdentifierFromSource(source[(separator + 2)..].Trim())}";
                }

                header = $"{(isAttribute ? "fieldattribute" : "fieldelement")}({identifier}; {field})";
                break;
            }

            default:
            {
                owner = TriggerOwner.XmlPortTextElement;
                if (!sourceType.Equals("Text", StringComparison.OrdinalIgnoreCase))
                {
                    context.Report(DiagnosticDescriptors.PropertyValueNotParsed,
                        $"Unknown XMLport source type '{sourceType}' on node '{nodeName}'; it was converted as a text node.", line);
                }

                header = $"{(isAttribute ? "textattribute" : "textelement")}({identifier})";
                if (string.Equals(item.Properties["DataType"]?.Trim(), "BigText", StringComparison.OrdinalIgnoreCase))
                {
                    extra.Add(new AlProperty("TextType", "BigText"));
                }

                break;
            }
        }

        var section = new AlSection { Header = header };
        if (!string.Equals(name, nodeName, StringComparison.Ordinal) && nodeName.Length > 0)
        {
            section.Properties.Add(new AlProperty("XmlName", AlSyntax.String(nodeName)));
        }

        section.Properties.AddRange(extra);
        section.Properties.AddRange(properties.Convert(item, PropertyScope.XmlPortElement, new PropertyContext
        {
            Path = $"Node {nodeName}",
            CurrentTableId = owner == TriggerOwner.XmlPortTableElement ? TableId(context, item.Properties["SourceTable"]) : null,
            TargetTableId = TableId(context, item.Properties["SourceTable"]),
        }));

        if (isAttribute && node.Children.Count > 0)
        {
            context.Report(DiagnosticDescriptors.MissingRequiredPart, $"Attribute '{nodeName}' has child nodes, which is not valid.", line);
        }

        foreach (var child in node.Children)
        {
            section.Children.Add(ConvertNode(context, members, properties, child, orphans));
        }

        context.CurrentElement = $"Node {nodeName}";
        ConvertTriggers(context, members, item, owner, name, section.Triggers, orphans);
        context.CurrentElement = null;
        return section;
    }
}
