using System.Globalization;
using Cal2AL.Converter.Code;
using Cal2AL.Converter.Properties;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;
using Cal2AL.Generator.Model;

namespace Cal2AL.Converter.Objects;

/// <summary>Kinds of extra files a conversion can produce besides the .al file.</summary>
public enum AuxiliaryFileKind
{
    RdlcLayout,
    WordLayout,

    /// <summary>Original source kept because it could not (fully) be converted.</summary>
    MigrationArtifact,
}

public sealed record AuxiliaryFile(string FileName, byte[] Content, AuxiliaryFileKind Kind);

/// <summary>What converting one object produced.</summary>
public sealed class ObjectConversionOutput
{
    public AlObject? AlObject { get; set; }
    public List<AuxiliaryFile> Files { get; } = [];
}

/// <summary>
/// Converts one kind of C/AL object. Implementations may keep state for the object being converted, so the engine
/// calls <see cref="Convert"/> for one object at a time (which also keeps generated IDs and names deterministic).
/// </summary>
public interface IObjectConverter
{
    bool CanConvert(CalObject obj);

    ObjectConversionOutput Convert(ObjectContext context);
}

/// <summary>A section item with the items nested under it (rebuilt from C/AL's flat list plus level column).</summary>
public sealed record ElementNode(CalElement Element, List<ElementNode> Children);

/// <summary>Rebuilds element nesting from C/AL's level/indentation column.</summary>
public static class ElementTree
{
    public static List<ElementNode> Build(IReadOnlyList<CalElement> items, int levelColumn)
    {
        var roots = new List<ElementNode>();
        var stack = new Stack<(int Level, ElementNode Node)>();
        foreach (var item in items)
        {
            var levelText = item.GetPositional(levelColumn).Trim();
            var level = int.TryParse(levelText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l) ? l : 0;
            var node = new ElementNode(item, []);
            while (stack.Count > 0 && stack.Peek().Level >= level)
            {
                stack.Pop();
            }

            if (stack.Count == 0)
            {
                roots.Add(node);
            }
            else
            {
                stack.Peek().Node.Children.Add(node);
            }

            stack.Push((level, node));
        }

        return roots;
    }
}

/// <summary>Shared building blocks for the per-type converters.</summary>
public abstract class ObjectConverterBase : IObjectConverter
{
    public abstract bool CanConvert(CalObject obj);

    public ObjectConversionOutput Convert(ObjectContext context)
    {
        var output = new ObjectConversionOutput();
        var members = new MemberConverter(context);
        members.PrepareDeclarations();
        output.AlObject = ConvertObject(context, members, new PropertyConverter(context), output);
        return output;
    }

    protected abstract AlObject? ConvertObject(ObjectContext context, MemberConverter members, PropertyConverter properties, ObjectConversionOutput output);

    /// <summary>Creates the AL object with header comments (C/AL metadata) and the documentation trigger.</summary>
    protected static AlObject CreateObject(ObjectContext context, string kind, int? id = null, string? name = null, string? extends = null)
    {
        var obj = context.Object;
        var al = new AlObject
        {
            Kind = kind,
            Id = id ?? obj.Id,
            Name = name ?? AlSyntax.Identifier(obj.Name),
            Extends = extends,
            SourceDescription = obj.DisplayName,
        };

        if (context.Options.KeepObjectMetadata && obj.ObjectProperties.Count > 0)
        {
            var parts = new List<string>();
            foreach (var key in new[] { "Version List", "Date", "Time", "Modified" })
            {
                if (obj.ObjectProperties.TryGetValue(key, out var value) && value.Length > 0)
                {
                    parts.Add($"{key}={value}");
                }
            }

            if (parts.Count > 0)
            {
                var source = obj.Type == CalObjectType.Modification ? $"delta of {obj.Name}" : $"{obj.Type} {obj.Id} \"{obj.Name}\"";
                al.HeaderComments.Add($"// Converted by Cal2AL from C/AL {source} ({string.Join("; ", parts)})");
            }
        }

        al.Documentation.AddRange(obj.Code.Documentation);
        return al;
    }

    /// <summary>Converts global variables and procedures, and places event triggers.</summary>
    protected virtual void AddCode(ObjectContext context, MemberConverter members, AlObject al)
    {
        al.Variables.AddRange(members.ConvertGlobals(context.Object.Code.Variables));
        foreach (var procedure in context.Object.Code.Procedures)
        {
            if (procedure.IsEvent)
            {
                PlaceEvent(context, members, al, procedure);
            }
            else
            {
                al.Procedures.Add(ConvertProcedure(context, members, procedure));
            }
        }
    }

    protected virtual AlMethod ConvertProcedure(ObjectContext context, MemberConverter members, CalProcedure procedure) =>
        members.ConvertProcedure(procedure);

    /// <summary>
    /// Places an <c>EVENT</c> declaration. Events of a global DotNet WITHEVENTS variable become object-level
    /// <c>trigger Var::Event(...)</c>; anything else is kept as a procedure with an error.
    /// </summary>
    protected virtual void PlaceEvent(ObjectContext context, MemberConverter members, AlObject al, CalProcedure eventProcedure)
    {
        if (eventProcedure.EventSource is not null && context.WithEventsVariables.Contains(eventProcedure.EventSource))
        {
            var source = members.Types.DeclareName(eventProcedure.EventSource);
            al.Triggers.Add(members.ConvertEventTrigger(eventProcedure, $"{source}::{eventProcedure.Name}"));
            return;
        }

        al.Procedures.Add(members.ConvertEventAsProcedure(eventProcedure));
    }

    /// <summary>
    /// Converts the triggers of a node. Triggers with an AL equivalent go to <paramref name="target"/>; others are
    /// preserved as local procedures in <paramref name="orphans"/>.
    /// </summary>
    protected static void ConvertTriggers(
        ObjectContext context,
        MemberConverter members,
        CalNode node,
        TriggerOwner owner,
        string ownerName,
        List<AlMethod> target,
        List<AlMethod> orphans)
    {
        foreach (var trigger in node.Triggers)
        {
            var signature = TriggerMap.Find(owner, trigger.Name);
            if (signature is null)
            {
                orphans.Add(members.ConvertTriggerAsProcedure(trigger, ownerName,
                    $"AL has no '{trigger.Name}' trigger on a {owner}."));
                continue;
            }

            var display = ownerName.Length == 0 ? signature.Name : $"{ownerName} - {signature.Name}";
            target.Add(members.ConvertTrigger(trigger, signature, display));
        }
    }

    /// <summary>Rebuilds the nesting of C/AL's flat element lists from the level column.</summary>
    protected static List<ElementNode> BuildTree(IReadOnlyList<CalElement> items, int levelColumn) => ElementTree.Build(items, levelColumn);

    /// <summary>Resolves a table reference (Table18, 18, Customer) to its ID when possible.</summary>
    protected static int? TableId(ObjectContext context, string? reference) =>
        reference is null ? null : context.Symbols.ResolveTableId(reference);

    /// <summary>The AL name of a referenced table, or its number when unknown.</summary>
    protected static string TableReference(ObjectContext context, string reference, int? line) =>
        Values.ObjectReferences.Resolve(context, reference, CalObjectType.Table, line);

    protected static void ReportMissing(ObjectContext context, string what, int? line) =>
        context.Report(DiagnosticDescriptors.MissingRequiredPart, what, line);
}
