using Cal2AL.Converter.Code;
using Cal2AL.Converter.Properties;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;
using Cal2AL.Generator.Model;

namespace Cal2AL.Converter.Objects;

/// <summary>Converts queries: data items, columns and filters, triggers and code.</summary>
public sealed class QueryConverter : ObjectConverterBase
{
    public override bool CanConvert(CalObject obj) => obj.Type == CalObjectType.Query;

    protected override AlObject ConvertObject(ObjectContext context, MemberConverter members, PropertyConverter properties, ObjectConversionOutput output)
    {
        var obj = context.Object;
        var al = CreateObject(context, "query");
        var orphans = new List<AlMethod>();
        al.Properties.AddRange(properties.Convert(obj, PropertyScope.Query, new PropertyContext { Path = $"Query {obj.Name}" }));

        var elements = new AlSection { Header = "elements" };
        var roots = BuildTree(obj.GetItems("ELEMENTS"), 1);
        foreach (var root in roots)
        {
            elements.Children.Add(ConvertNode(context, properties, root, null));
        }

        if (roots.Count != 1)
        {
            ReportMissing(context, $"A query must have exactly one top-level data item; this one has {roots.Count}.", obj.Position.Line);
        }

        al.Sections.Add(elements);
        ConvertTriggers(context, members, obj, TriggerOwner.Query, string.Empty, al.Triggers, orphans);
        AddCode(context, members, al);
        al.Procedures.AddRange(orphans);
        return al;
    }

    private static AlSection ConvertNode(ObjectContext context, PropertyConverter properties, ElementNode node, int? parentTableId)
    {
        var item = node.Element;
        var id = item.GetPositional(0).Trim();
        var type = item.GetPositional(2).Trim();
        var name = item.GetPositional(3).Trim();
        var line = item.Position.Line;
        context.CurrentElement = $"{type} {name}";
        var reserved = context.ReserveName(name.Length > 0 ? name : $"{type}{id}", $"{type}{id}", line);
        var identifier = AlSyntax.Identifier(reserved);

        if (type.Equals("DataItem", StringComparison.OrdinalIgnoreCase))
        {
            var tableRef = item.Properties["DataItemTable"];
            if (tableRef is null)
            {
                ReportMissing(context, $"Data item '{reserved}' has no DataItemTable.", line);
            }

            var tableId = TableId(context, tableRef);
            var section = new AlSection { Header = $"dataitem({identifier}; {(tableRef is null ? "0" : TableReference(context, tableRef, line))})" };
            section.Properties.AddRange(properties.Convert(item, PropertyScope.QueryDataItem, new PropertyContext
            {
                Path = $"DataItem {reserved}",
                CurrentTableId = tableId,
                TargetTableId = tableId,
            }));

            foreach (var child in node.Children)
            {
                section.Children.Add(ConvertNode(context, properties, child, tableId));
            }

            context.CurrentElement = null;
            return section;
        }

        var keyword = type.ToUpperInvariant() switch
        {
            "COLUMN" => "column",
            "FILTER" => "filter",
            _ => null,
        };

        if (keyword is null)
        {
            context.Report(DiagnosticDescriptors.PropertyValueNotParsed,
                $"Unknown query element type '{type}'; it was converted as a column.", line);
            keyword = "column";
        }

        var dataSource = item.Properties["DataSource"];
        if (dataSource is null)
        {
            ReportMissing(context, $"Query {keyword} '{reserved}' has no DataSource.", line);
        }

        var column = new AlSection { Header = $"{keyword}({identifier}; {(dataSource is null ? "''" : AlSyntax.IdentifierFromSource(dataSource.Trim()))})" };
        column.Properties.AddRange(properties.Convert(item, PropertyScope.QueryColumn, new PropertyContext
        {
            Path = $"{type} {reserved}",
            CurrentTableId = parentTableId,
            TargetTableId = parentTableId,
        }));

        if (node.Children.Count > 0)
        {
            context.Report(DiagnosticDescriptors.MissingRequiredPart, $"Query {keyword} '{reserved}' has nested elements, which is not valid.", line);
        }

        context.CurrentElement = null;
        return column;
    }
}
