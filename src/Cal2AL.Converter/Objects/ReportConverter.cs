using Cal2AL.Converter.Code;
using Cal2AL.Converter.Properties;
using Cal2AL.Converter.Values;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;
using Cal2AL.Generator.Model;

namespace Cal2AL.Converter.Objects;

/// <summary>Converts RDLC/Word reports: dataset, request page, labels, layouts, triggers and code.</summary>
public sealed class ReportConverter : ObjectConverterBase
{
    public override bool CanConvert(CalObject obj) => obj.Type == CalObjectType.Report;

    protected override AlObject ConvertObject(ObjectContext context, MemberConverter members, PropertyConverter properties, ObjectConversionOutput output)
    {
        var obj = context.Object;
        var al = CreateObject(context, "report");
        var orphans = new List<AlMethod>();

        var hasRdlc = obj.RdlData.Count > 0;
        var hasWord = obj.WordLayout.Count > 0;
        if (hasRdlc && !obj.Properties.Contains("DefaultLayout"))
        {
            al.Properties.Add(new AlProperty("DefaultLayout", "RDLC"));
        }
        else if (hasWord && !obj.Properties.Contains("DefaultLayout"))
        {
            al.Properties.Add(new AlProperty("DefaultLayout", "Word"));
        }

        if (hasRdlc)
        {
            var fileName = context.Run.LayoutFileName(obj, ".rdlc");
            al.Properties.Add(new AlProperty("RDLCLayout", AlSyntax.String(context.Run.LayoutReference(fileName))));
            output.Files.Add(new AuxiliaryFile(fileName, System.Text.Encoding.UTF8.GetBytes(RdlcText(obj)), AuxiliaryFileKind.RdlcLayout));
        }

        if (hasWord)
        {
            var fileName = context.Run.LayoutFileName(obj, ".docx");
            try
            {
                var bytes = DecodeLayout(obj.WordLayout);
                al.Properties.Add(new AlProperty("WordLayout", AlSyntax.String(context.Run.LayoutReference(fileName))));
                output.Files.Add(new AuxiliaryFile(fileName, bytes, AuxiliaryFileKind.WordLayout));
            }
            catch (FormatException ex)
            {
                context.Report(DiagnosticDescriptors.MissingRequiredPart,
                    $"The Word layout is not valid base64 and could not be extracted: {ex.Message}", reason: ex.Message);
            }
        }

        al.Properties.AddRange(properties.Convert(obj, PropertyScope.Report, new PropertyContext { Path = $"Report {obj.Name}" }));

        var dataset = new AlSection { Header = "dataset" };
        foreach (var root in BuildTree(obj.GetItems("DATASET"), 1))
        {
            dataset.Children.Add(ConvertDataSetNode(context, members, properties, root, parentTableId: null, orphans));
        }

        al.Sections.Add(dataset);

        if (obj.GetSection("REQUESTPAGE")?.Items.FirstOrDefault() is { } requestPage)
        {
            var section = RequestPageConverter.Convert(context, members, properties, requestPage, orphans);
            if (section is not null)
            {
                al.Sections.Add(section);
            }
        }

        var labels = obj.GetItems("LABELS");
        if (labels.Count > 0)
        {
            var section = new AlSection { Header = "labels" };
            foreach (var label in labels)
            {
                var name = label.GetPositional(1).Trim();
                context.CurrentElement = $"Label {name}";
                var caption = label.Properties["CaptionML"];
                var text = caption is null ? null : LabelConverter.Convert(context, caption, $"ReportLabel {name}", label.Position.Line);
                section.Lines.Add($"{AlSyntax.Identifier(name)} = {text ?? "''"};");
                foreach (var extra in label.Properties.Where(p => !p.Name.Equals("CaptionML", StringComparison.OrdinalIgnoreCase)))
                {
                    context.Report(DiagnosticDescriptors.PropertyNotSupported,
                        $"Property '{extra.Name}' of report label '{name}' cannot be expressed on an AL report label and was not emitted.",
                        extra.Position.Line, construct: extra.Name, original: extra.ToString());
                }
            }

            context.CurrentElement = null;
            al.Sections.Add(section);
        }

        ConvertTriggers(context, members, obj, TriggerOwner.Report, string.Empty, al.Triggers, orphans);
        AddCode(context, members, al);
        al.Procedures.AddRange(orphans);
        return al;
    }

    /// <summary>
    /// C/SIDE base64-encodes the Word layout in chunks, one per line, each with its own padding
    /// (<c>…hg==</c> in the middle of the block), so the lines are decoded one by one.
    /// </summary>
    internal static byte[] DecodeLayout(IEnumerable<string> lines)
    {
        var result = new List<byte>();
        foreach (var line in lines.Select(l => l.Trim()).Where(l => l.Length > 0))
        {
            result.AddRange(System.Convert.FromBase64String(line));
        }

        return [.. result];
    }

    private static string RdlcText(CalObject obj)
    {
        var lines = obj.RdlData.ToList();
        if (lines.Count > 0)
        {
            lines[0] = lines[0].TrimStart();
        }

        return string.Join("\r\n", lines) + "\r\n";
    }

    private static AlSection ConvertDataSetNode(ObjectContext context, MemberConverter members, PropertyConverter properties, ElementNode node, int? parentTableId, List<AlMethod> orphans)
    {
        var item = node.Element;
        var id = item.GetPositional(0).Trim();
        var type = item.GetPositional(2).Trim();
        var name = item.GetPositional(3).Trim();
        var line = item.Position.Line;

        if (type.Equals("Column", StringComparison.OrdinalIgnoreCase))
        {
            context.CurrentElement = $"Column {name}";
            var columnName = context.ReserveName(name.Length > 0 ? name : $"Column{id}", $"Column{id}", line);
            var sourceExpr = item.Properties["SourceExpr"];
            if (sourceExpr is null)
            {
                ReportMissing(context, $"Column '{columnName}' has no SourceExpr.", line);
            }

            var column = new AlSection { Header = $"column({AlSyntax.Identifier(columnName)}; {(sourceExpr is null ? "''" : FilterExpressionConverter.Flatten(sourceExpr))})" };
            column.Properties.AddRange(properties.Convert(item, PropertyScope.Column, new PropertyContext { Path = $"Column {columnName}", CurrentTableId = parentTableId }));
            if (node.Children.Count > 0)
            {
                context.Report(DiagnosticDescriptors.MissingRequiredPart, $"Column '{columnName}' has nested elements, which is not valid.", line);
            }

            context.CurrentElement = null;
            return column;
        }

        context.CurrentElement = name.Length > 0 ? $"DataItem {name}" : $"DataItem {id}";
        var tableRef = item.Properties["DataItemTable"];
        var tableId = TableId(context, tableRef);
        var tableName = tableRef is null ? "0" : TableReference(context, tableRef, line);
        var dataItemName = name.Length > 0 ? name : (tableId is int t && context.Symbols.GetName(CalObjectType.Table, t) is { } n ? n : $"DataItem{id}");
        dataItemName = context.ReserveName(dataItemName, $"DataItem{id}", line);
        context.CurrentElement = $"DataItem {dataItemName}";
        if (tableRef is null)
        {
            ReportMissing(context, $"Data item '{dataItemName}' has no DataItemTable.", line);
        }

        var section = new AlSection { Header = $"dataitem({AlSyntax.Identifier(dataItemName)}; {tableName})" };
        section.Properties.AddRange(properties.Convert(item, PropertyScope.DataItem, new PropertyContext
        {
            Path = $"DataItem {dataItemName}",
            CurrentTableId = parentTableId,
            TargetTableId = tableId,
        }));

        foreach (var child in node.Children)
        {
            section.Children.Add(ConvertDataSetNode(context, members, properties, child, tableId, orphans));
        }

        context.CurrentElement = $"DataItem {dataItemName}";
        ConvertTriggers(context, members, item, TriggerOwner.ReportDataItem, $"DataItem {dataItemName}", section.Triggers, orphans);
        context.CurrentElement = null;
        return section;
    }
}

/// <summary>Converts the request page of a report or XMLport.</summary>
public static class RequestPageConverter
{
    public static AlSection? Convert(ObjectContext context, MemberConverter members, PropertyConverter properties, CalElement requestPage, List<AlMethod> orphans)
    {
        var isEmpty = requestPage.Properties.Count == 0 && requestPage.Triggers.Count == 0
            && requestPage.Sections.All(s => s.Items.Count == 0);
        if (isEmpty)
        {
            return null;
        }

        var tableId = context.Symbols.ResolveTableId(requestPage.Properties["SourceTable"] ?? string.Empty);
        var section = new AlSection { Header = "requestpage" };
        context.CurrentElement = "RequestPage";
        section.Properties.AddRange(properties.Convert(requestPage, PropertyScope.RequestPage, new PropertyContext
        {
            Path = "RequestPage",
            CurrentTableId = tableId,
            TargetTableId = tableId,
        }));

        var builder = new PageBuilder(context, members, properties, tableId, isApi: false);
        if (builder.BuildLayout(requestPage.GetItems("CONTROLS")) is { } layout)
        {
            section.Children.Add(layout);
        }

        if (builder.BuildActions(requestPage.GetSection("ACTIONS")?.Items) is { } actions)
        {
            section.Children.Add(actions);
        }

        context.CurrentElement = "RequestPage";
        foreach (var trigger in requestPage.Triggers)
        {
            var signature = TriggerMap.Find(TriggerOwner.Page, trigger.Name);
            if (signature is null)
            {
                orphans.Add(members.ConvertTriggerAsProcedure(trigger, "RequestPage", $"AL has no '{trigger.Name}' trigger on a request page."));
                continue;
            }

            section.Triggers.Add(members.ConvertTrigger(trigger, signature, $"RequestPage - {signature.Name}"));
        }

        orphans.AddRange(builder.OrphanProcedures);
        context.CurrentElement = null;
        return section;
    }
}
