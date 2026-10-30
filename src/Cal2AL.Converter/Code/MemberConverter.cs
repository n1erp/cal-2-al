using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;
using Cal2AL.Generator.Model;

namespace Cal2AL.Converter.Code;

/// <summary>Converts the code-bearing members of an object: global variables, procedures, triggers and events.</summary>
public sealed class MemberConverter
{
    private readonly ObjectContext context;
    private readonly TypeConverter types;
    private readonly AttributeConverter attributes;
    private readonly BodyConverter bodies;

    public MemberConverter(ObjectContext context)
    {
        this.context = context;
        types = new TypeConverter(context);
        attributes = new AttributeConverter(context);
        bodies = new BodyConverter(context);
    }

    public TypeConverter Types => types;

    private string ScopePath => $"{context.Object.Type} {context.Object.Name}";

    /// <summary>
    /// Registers every declared name before any code is converted, so that a trigger near the top of the object
    /// quotes a keyword-named variable that is declared further down.
    /// </summary>
    public void PrepareDeclarations()
    {
        var obj = context.Object;
        DeclareVariables(obj.Code.Variables);
        DeclareProcedures(obj.Code.Procedures);
        DeclareNode(obj);
    }

    private void DeclareVariables(IEnumerable<CalVariable> variables)
    {
        foreach (var v in variables)
        {
            types.DeclareName(v.Name);
        }
    }

    private void DeclareProcedures(IEnumerable<CalProcedure> procedures)
    {
        foreach (var p in procedures)
        {
            if (!p.IsEvent)
            {
                types.DeclareName(p.Name);
            }

            DeclareVariables(p.Parameters.Concat(p.Variables));
            if (p.ReturnValue is { Name.Length: > 0 } rv)
            {
                types.DeclareName(rv.Name);
            }
        }
    }

    private void DeclareNode(CalNode node)
    {
        foreach (var t in node.Triggers)
        {
            DeclareVariables(t.Variables);
        }

        foreach (var section in node.Sections)
        {
            DeclareVariables(section.Variables);
            DeclareProcedures(section.Procedures);
            foreach (var item in section.Items)
            {
                DeclareNode(item);
            }
        }
    }

    public List<AlVariable> ConvertGlobals(IEnumerable<CalVariable> variables)
    {
        var result = new List<AlVariable>();
        foreach (var v in variables)
        {
            context.CurrentMember = null;
            result.Add(types.ConvertVariable(v, isParameter: false, ScopePath));
        }

        return result;
    }

    public AlMethod ConvertProcedure(CalProcedure procedure)
    {
        context.CurrentMember = procedure.Name;
        var converted = attributes.Convert(procedure);
        var access = procedure.IsLocal ? "local" : converted.AccessModifier ?? string.Empty;

        var method = new AlMethod
        {
            Kind = AlMethodKind.Procedure,
            Access = access,
            Name = types.DeclareName(procedure.Name),
            ReturnType = procedure.ReturnValue is null ? null : types.ConvertType(procedure.ReturnValue, ScopePath, procedure.Position.Line),
            ReturnName = procedure.ReturnValue is { Name.Length: > 0 } rv ? types.DeclareName(rv.Name) : null,
        };

        method.Attributes.AddRange(converted.Attributes);
        if (context.Options.EmitScopeAttributes && !procedure.IsLocal && !converted.HasScopeAttribute && converted.AccessModifier is null
            && !procedure.Attributes.Any(a => a.StartsWith("External", StringComparison.OrdinalIgnoreCase))
            && context.Object.Type != CalObjectType.Modification)
        {
            method.Attributes.Add(context.Options.TargetRuntime >= new Version(4, 0) ? "[Scope('OnPrem')]" : "[Scope('Internal')]");
        }

        foreach (var p in procedure.Parameters)
        {
            method.Parameters.Add(types.ConvertVariable(p, isParameter: true, ScopePath));
        }

        AddLocalsAndBody(method, procedure.Variables, procedure.Body, procedure.Name);
        context.CurrentMember = null;
        return method;
    }

    /// <summary>Converts a trigger with a known AL signature.</summary>
    public AlMethod ConvertTrigger(CalTrigger trigger, TriggerSignature signature, string memberDisplayName)
    {
        context.CurrentMember = memberDisplayName;
        var method = new AlMethod
        {
            Kind = AlMethodKind.Trigger,
            Name = signature.Name,
            ParameterText = signature.Parameters,
            ReturnType = signature.ReturnType,
        };
        AddLocalsAndBody(method, trigger.Variables, trigger.Body, memberDisplayName);
        context.CurrentMember = null;
        return method;
    }

    /// <summary>
    /// Keeps the code of a trigger that has no AL equivalent as a local procedure, so it is not lost.
    /// </summary>
    public AlMethod ConvertTriggerAsProcedure(CalTrigger trigger, string ownerName, string reason)
    {
        var name = AlSyntax.ToIdentifierText($"{ownerName}_{trigger.Name.Replace("::", "_", StringComparison.Ordinal)}");
        context.CurrentMember = trigger.Name;
        context.Report(DiagnosticDescriptors.TriggerWithoutAlEquivalent,
            $"Trigger '{trigger.Name}' on {ownerName} has no AL equivalent; its code was preserved as local procedure {name}().",
            trigger.Position.Line, construct: trigger.Name, reason: reason, original: trigger.Body.Text);
        var method = new AlMethod { Kind = AlMethodKind.Procedure, Access = "local", Name = name };
        method.LeadingComments.Add($"// Cal2AL: code of C/AL trigger {trigger.Name} on {ownerName}; see diagnostic {DiagnosticDescriptors.TriggerWithoutAlEquivalent.Code}.");
        AddLocalsAndBody(method, trigger.Variables, trigger.Body, trigger.Name);
        context.CurrentMember = null;
        return method;
    }

    /// <summary>Converts an <c>EVENT</c> declaration to an AL trigger with the given name (<c>Var::Event</c> or <c>Event</c>).</summary>
    public AlMethod ConvertEventTrigger(CalProcedure eventProcedure, string triggerName)
    {
        context.CurrentMember = $"{eventProcedure.EventSource}::{eventProcedure.Name}";
        var method = new AlMethod { Kind = AlMethodKind.Trigger, Name = triggerName };
        foreach (var p in eventProcedure.Parameters)
        {
            method.Parameters.Add(types.ConvertVariable(p, isParameter: true, ScopePath));
        }

        AddLocalsAndBody(method, eventProcedure.Variables, eventProcedure.Body, context.CurrentMember);
        context.CurrentMember = null;
        return method;
    }

    /// <summary>Keeps an event whose owner could not be found as a local procedure.</summary>
    public AlMethod ConvertEventAsProcedure(CalProcedure eventProcedure)
    {
        var name = AlSyntax.ToIdentifierText($"{eventProcedure.EventSource}_{eventProcedure.Name}");
        var isAutomation = eventProcedure.EventSource is { } source && context.AutomationVariables.Contains(source);
        context.Report(isAutomation ? DiagnosticDescriptors.AutomationNotSupported : DiagnosticDescriptors.UnresolvedEventOwner,
            isAutomation
                ? $"Event trigger {eventProcedure.EventSource}::{eventProcedure.Name} belongs to an Automation (COM) variable, which AL does not support; its code was preserved as local procedure {name}()."
                : $"Event trigger {eventProcedure.EventSource}::{eventProcedure.Name} belongs to no WithEvents variable or control add-in; its code was preserved as local procedure {name}().",
            eventProcedure.Position.Line, construct: $"EVENT {eventProcedure.EventSource}::{eventProcedure.Name}", original: eventProcedure.Body.Text);
        var method = new AlMethod { Kind = AlMethodKind.Procedure, Access = "local", Name = name };
        foreach (var p in eventProcedure.Parameters)
        {
            method.Parameters.Add(types.ConvertVariable(p, isParameter: true, ScopePath));
        }

        AddLocalsAndBody(method, eventProcedure.Variables, eventProcedure.Body, name);
        return method;
    }

    private void AddLocalsAndBody(AlMethod method, IEnumerable<CalVariable> locals, CalCodeBody body, string member)
    {
        foreach (var v in locals)
        {
            method.Variables.Add(types.ConvertVariable(v, isParameter: false, $"{ScopePath} - Method {member}"));
        }

        method.Body.AddRange(bodies.Convert(body, member));
    }
}
