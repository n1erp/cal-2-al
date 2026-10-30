using Cal2AL.Converter.Code;
using Cal2AL.Converter.Properties;
using Cal2AL.Core.Model;
using Cal2AL.Generator.Model;

namespace Cal2AL.Converter.Objects;

/// <summary>Converts codeunits, including the OnBeforeTestRun/OnAfterTestRun triggers of test runners.</summary>
public sealed class CodeunitConverter : ObjectConverterBase
{
    public override bool CanConvert(CalObject obj) => obj.Type == CalObjectType.Codeunit;

    protected override AlObject ConvertObject(ObjectContext context, MemberConverter members, PropertyConverter properties, ObjectConversionOutput output)
    {
        var obj = context.Object;
        var al = CreateObject(context, "codeunit");
        al.Properties.AddRange(properties.Convert(obj, PropertyScope.Codeunit, new PropertyContext
        {
            Path = $"Codeunit {obj.Name}",
            CurrentTableId = TableId(context, obj.Properties["TableNo"]),
        }));

        var orphans = new List<AlMethod>();
        ConvertTriggers(context, members, obj, TriggerOwner.Codeunit, string.Empty, al.Triggers, orphans);

        var isTestRunner = string.Equals(obj.Properties["Subtype"]?.Trim(), "TestRunner", StringComparison.OrdinalIgnoreCase);
        al.Variables.AddRange(members.ConvertGlobals(obj.Code.Variables));
        foreach (var procedure in obj.Code.Procedures)
        {
            if (procedure.IsEvent)
            {
                PlaceEvent(context, members, al, procedure);
            }
            else if (isTestRunner && IsTestRunnerTrigger(procedure))
            {
                al.Triggers.Add(ConvertTestRunnerTrigger(members, procedure));
            }
            else
            {
                al.Procedures.Add(members.ConvertProcedure(procedure));
            }
        }

        al.Procedures.AddRange(orphans);
        return al;
    }

    private static bool IsTestRunnerTrigger(CalProcedure procedure) =>
        procedure.Name.Equals("OnBeforeTestRun", StringComparison.OrdinalIgnoreCase)
        || procedure.Name.Equals("OnAfterTestRun", StringComparison.OrdinalIgnoreCase);

    /// <summary>In C/AL these were procedures with a fixed signature; in AL they are triggers.</summary>
    private static AlMethod ConvertTestRunnerTrigger(MemberConverter members, CalProcedure procedure)
    {
        var converted = members.ConvertProcedure(procedure);
        var trigger = new AlMethod
        {
            Kind = AlMethodKind.Trigger,
            Name = procedure.Name.Equals("OnBeforeTestRun", StringComparison.OrdinalIgnoreCase) ? "OnBeforeTestRun" : "OnAfterTestRun",
            ReturnType = converted.ReturnType,
            ReturnName = converted.ReturnName,
        };
        trigger.Parameters.AddRange(converted.Parameters);
        trigger.Variables.AddRange(converted.Variables);
        trigger.Body.AddRange(converted.Body);
        return trigger;
    }
}
