using Cal2AL.Converter.Code;
using Cal2AL.Converter.Properties;
using Cal2AL.Core.Model;
using Cal2AL.Generator.Model;

namespace Cal2AL.Converter.Objects;

/// <summary>Converts pages: properties, layout, actions, page triggers, code and control add-in events.</summary>
public sealed class PageConverter : ObjectConverterBase
{
    private PageBuilder? builder;

    public override bool CanConvert(CalObject obj) => obj.Type == CalObjectType.Page;

    protected override AlObject ConvertObject(ObjectContext context, MemberConverter members, PropertyConverter properties, ObjectConversionOutput output)
    {
        var obj = context.Object;
        var al = CreateObject(context, "page");
        var tableId = TableId(context, obj.Properties["SourceTable"]);
        var isApi = string.Equals(obj.Properties["PageType"]?.Trim(), "API", StringComparison.OrdinalIgnoreCase);

        al.Properties.AddRange(properties.Convert(obj, PropertyScope.Page, new PropertyContext
        {
            Path = $"Page {obj.Name}",
            CurrentTableId = tableId,
            TargetTableId = tableId,
            IsApiObject = isApi,
        }));

        builder = new PageBuilder(context, members, properties, tableId, isApi);
        if (builder.BuildLayout(obj.GetItems("CONTROLS")) is { } layout)
        {
            al.Sections.Add(layout);
        }

        if (builder.BuildActions(obj.GetSection("ACTIONS")?.Items) is { } actions)
        {
            al.Sections.Add(actions);
        }

        var orphans = new List<AlMethod>();
        ConvertTriggers(context, members, obj, TriggerOwner.Page, string.Empty, al.Triggers, orphans);
        AddCode(context, members, al);
        al.Procedures.AddRange(builder.OrphanProcedures);
        al.Procedures.AddRange(orphans);
        return al;
    }

    /// <summary>Control add-in events belong inside their usercontrol; .NET events stay at object level.</summary>
    protected override void PlaceEvent(ObjectContext context, MemberConverter members, AlObject al, CalProcedure eventProcedure)
    {
        AlSection? userControl = null;
        if (builder is not null)
        {
            if (eventProcedure.EventSourceId is int sourceId && sourceId < 0)
            {
                builder.UserControlsById.TryGetValue(-sourceId, out userControl);
            }

            if (userControl is null && eventProcedure.EventSource is { } source)
            {
                builder.UserControlsByName.TryGetValue(source, out userControl);
            }
        }

        if (userControl is not null)
        {
            userControl.Triggers.Add(members.ConvertEventTrigger(eventProcedure, eventProcedure.Name));
            return;
        }

        base.PlaceEvent(context, members, al, eventProcedure);
    }
}
