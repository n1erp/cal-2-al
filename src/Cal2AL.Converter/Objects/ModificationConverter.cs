using System.Text;
using System.Text.RegularExpressions;
using Cal2AL.Converter.Code;
using Cal2AL.Converter.Properties;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;
using Cal2AL.Generator.Model;
using Cal2AL.Parser.Code;

namespace Cal2AL.Converter.Objects;

/// <summary>
/// Converts C/AL delta files (<c>OBJECT Modification</c>) into table and page extensions.
/// <list type="bullet">
/// <item>Inserted fields, keys, controls, actions, variables and procedures become extension members.</item>
/// <item>Property changes become <c>modify()</c> blocks where AL allows the property in an extension.</item>
/// <item>A code change that only adds lines before or after the unchanged original body becomes an extension trigger
/// (OnBefore…/OnAfter… or the extension's own trigger), after checking that the move cannot change behaviour.</item>
/// <item>Everything else is reported and saved, verbatim, in a migration artifact next to the output.</item>
/// </list>
/// </summary>
public sealed partial class ModificationConverter : ObjectConverterBase
{
    private static readonly HashSet<string> TableObjectModifiable = new(StringComparer.OrdinalIgnoreCase)
    {
        "CaptionML", "Description", "DataCaptionFields", "LookupPageID", "DrillDownPageID", "Permissions",
    };

    private static readonly HashSet<string> TableFieldModifiable = new(StringComparer.OrdinalIgnoreCase)
    {
        "CaptionML", "OptionCaptionML", "Description", "ToolTipML", "BlankZero", "BlankNumbers", "DecimalPlaces",
        "AutoFormatType", "AutoFormatExpr", "CaptionClass", "ExtendedDatatype", "Width", "AccessByPermission",
    };

    private static readonly HashSet<string> PageObjectModifiable = new(StringComparer.OrdinalIgnoreCase)
    {
        "CaptionML", "Description", "Editable", "InsertAllowed", "ModifyAllowed", "DeleteAllowed",
        "PromotedActionCategoriesML", "RefreshOnActivate", "DataCaptionExpr",
    };

    private static readonly HashSet<string> PageControlModifiable = new(StringComparer.OrdinalIgnoreCase)
    {
        "CaptionML", "ToolTipML", "Visible", "Editable", "Enabled", "Importance", "ShowMandatory", "Style", "StyleExpr",
        "HideValue", "QuickEntry", "Width", "MultiLine", "ColumnSpan", "RowSpan", "Description", "AccessByPermission",
        "BlankZero", "BlankNumbers", "ApplicationArea", "OptionCaptionML", "DrillDown", "Lookup", "AssistEdit",
    };

    private static readonly HashSet<string> PageActionModifiable = new(StringComparer.OrdinalIgnoreCase)
    {
        "CaptionML", "ToolTipML", "Visible", "Enabled", "Promoted", "PromotedCategory", "PromotedIsBig", "PromotedOnly",
        "Image", "ShortCutKey", "Description", "AccessByPermission", "ApplicationArea", "Ellipsis",
    };

    private readonly List<(CalElement Change, string Reason)> unconverted = [];

    public override bool CanConvert(CalObject obj) => obj.Type == CalObjectType.Modification;

    protected override AlObject? ConvertObject(ObjectContext context, MemberConverter members, PropertyConverter properties, ObjectConversionOutput output)
    {
        unconverted.Clear();
        var obj = context.Object;
        var target = ElementReference.Parse(obj.Properties["Target"] ?? obj.ModificationTarget ?? string.Empty);
        var targetType = CalObjectTypeExtensions.ParseObjectType(target.Kind ?? string.Empty);
        var targetName = target.Name ?? (target.Id is int tid ? context.Symbols.GetName(targetType, tid) : null) ?? $"{targetType} {target.Id}";

        var state = new ExtensionState(context, members, properties, targetType, target.Id, targetName);
        AlObject? al = null;
        if (targetType is CalObjectType.Table or CalObjectType.Page)
        {
            var kind = targetType == CalObjectType.Table ? "tableextension" : "pageextension";
            var id = context.Run.ExtensionIds.Next();
            var name = ExtensionName(targetName);
            al = CreateObject(context, kind, id, AlSyntax.Identifier(name), AlSyntax.Identifier(targetName));
            state.Object = al;
            state.TableId = targetType == CalObjectType.Table ? target.Id : (target.Id is int pageId ? context.Symbols.GetPageSourceTableId(pageId.ToString(System.Globalization.CultureInfo.InvariantCulture)) : null);

            foreach (var change in obj.GetItems("CHANGES"))
            {
                ConvertChange(state, change);
            }

            state.Finish();
            AddCode(context, members, al);
        }
        // A delta of a report, codeunit, … produces no AL object, only diagnostics and the artifact.
        else
        {
            context.Report(DiagnosticDescriptors.DeltaTargetNotSupported,
                $"The delta targets {targetType} '{targetName}'; only tables and pages can be extended in AL. All changes were saved in the migration artifact.",
                obj.Position.Line, construct: $"Modification of {targetType}");
            foreach (var change in obj.GetItems("CHANGES"))
            {
                unconverted.Add((change, $"{targetType} objects cannot be extended."));
            }

        }

        if (unconverted.Count > 0)
        {
            output.Files.Add(new AuxiliaryFile(
                $"{AlSyntax.FileNameSafe(targetName)}.{targetType}.delta.txt",
                Encoding.UTF8.GetBytes(BuildArtifact(obj)),
                AuxiliaryFileKind.MigrationArtifact));
        }

        return al;
    }

    private static string ExtensionName(string targetName)
    {
        var name = $"{targetName} Ext";
        return name.Length <= 30 ? name : $"{targetName[..26].TrimEnd()} Ext";
    }

    private void ConvertChange(ExtensionState state, CalElement change)
    {
        var context = state.Context;
        var changeType = change.GetPositional(0).Trim();
        context.CurrentElement = $"Change {changeType}";
        switch (changeType.ToUpperInvariant())
        {
            case "INSERTION":
                ConvertInsertion(state, change);
                break;
            case "PROPERTYMODIFICATION":
            case "PROPERTYINSERTION":
                ConvertPropertyChange(state, change);
                break;
            case "CODEMODIFICATION":
                ConvertCodeModification(state, change);
                break;
            default:
                Unsupported(context, change, $"A '{changeType}' change cannot be expressed in an AL extension (extensions can only add to or adjust the base object).");
                break;
        }

        context.CurrentElement = null;
    }

    private void Unsupported(ObjectContext context, CalElement change, string reason, DiagnosticDescriptor? descriptor = null)
    {
        unconverted.Add((change, reason));
        context.Report(descriptor ?? DiagnosticDescriptors.DeltaUnsupportedChange, reason, change.Position.Line,
            construct: change.GetPositional(0).Trim(), reason: reason);
    }

    // ---- Insertions --------------------------------------------------------------------------------

    private void ConvertInsertion(ExtensionState state, CalElement change)
    {
        var context = state.Context;
        var collection = change.Sections.FirstOrDefault();
        if (collection is null)
        {
            Unsupported(context, change, "The insertion has no ChangedElements.");
            return;
        }

        var anchorText = change.Properties["InsertAfter"];
        var anchor = anchorText is null ? null : ElementReference.Parse(anchorText);
        var section = collection.Name.ToUpperInvariant();

        if (section == "VARIABLECOLLECTION")
        {
            state.Object!.Variables.AddRange(state.Members.ConvertGlobals(collection.Variables));
            return;
        }

        if (section == "PROCEDURECOLLECTION")
        {
            foreach (var procedure in collection.Procedures)
            {
                state.Object!.Procedures.Add(procedure.IsEvent ? state.Members.ConvertEventAsProcedure(procedure) : state.Members.ConvertProcedure(procedure));
            }

            return;
        }

        switch (state.TargetType, section)
        {
            case (CalObjectType.Table, "FIELDCOLLECTION"):
            {
                var fields = TableConverter.ConvertFields(context, state.Members, state.Properties, collection.Items, state.Orphans, state.TableId);
                state.Section("fields").Children.AddRange(fields.Children);
                return;
            }

            case (CalObjectType.Table, "KEYCOLLECTION"):
            {
                var keys = TableConverter.ConvertKeys(context, state.Properties, collection.Items);
                foreach (var key in keys.Children)
                {
                    // Base table keys are named Key1, Key2, … by convention; avoid clashing with them.
                    key.Header = KeyHeaderRegex().Replace(key.Header, m => $"key({AlSyntax.FileNameSafe(state.ExtensionName)}{m.Groups[1].Value}", 1);
                }

                state.Section("keys").Children.AddRange(keys.Children);
                return;
            }

            case (CalObjectType.Page, "CONTROLCOLLECTION"):
            {
                var controls = state.Builder.BuildControls(collection.Items);
                state.Section("layout").Children.Add(Placement(state, change, anchor, controls, isAction: false));
                return;
            }

            case (CalObjectType.Page, "ACTIONCOLLECTION"):
            {
                var actions = state.Builder.BuildActionItems(collection.Items);
                state.Section("actions").Children.Add(Placement(state, change, anchor, actions, isAction: true));
                return;
            }
        }

        Unsupported(context, change, $"Inserting into '{collection.Name}' of a {state.TargetType} cannot be expressed in an AL extension.");
    }

    private static AlSection Placement(ExtensionState state, CalElement change, ElementReference? anchor, List<AlSection> items, bool isAction)
    {
        string header;
        if (anchor is null || anchor.Mode == AnchorMode.First)
        {
            var area = anchor?.Name ?? (isAction ? "Processing" : "Content");
            header = $"addfirst({MapArea(area, isAction)})";
        }
        else
        {
            header = $"addafter({state.AnchorName(anchor, isAction, change.Position.Line)})";
        }

        // C/AL deltas can start with the container itself; AL adds into an existing area instead.
        var section = new AlSection { Header = header };
        foreach (var item in items)
        {
            if (item.Header.StartsWith("area(", StringComparison.OrdinalIgnoreCase))
            {
                section.Header = $"addlast({item.Header[5..^1]})";
                section.Children.AddRange(item.Children);
            }
            else
            {
                section.Children.Add(item);
            }
        }

        return section;
    }

    private static string MapArea(string name, bool isAction) => (isAction, name.ToUpperInvariant()) switch
    {
        (true, "ACTIONITEMS") => "Processing",
        (true, "RELATEDINFORMATION") => "Navigation",
        (true, "REPORTS") => "Reporting",
        (true, "NEWDOCUMENTITEMS") => "Creation",
        (false, "CONTENTAREA") => "Content",
        (false, "FACTBOXAREA") => "FactBoxes",
        _ => AlSyntax.Identifier(name),
    };

    // ---- Property changes --------------------------------------------------------------------------

    private void ConvertPropertyChange(ExtensionState state, CalElement change)
    {
        var context = state.Context;
        var propertyName = change.Properties["Property"]?.Trim();
        var value = change.Properties["ModifiedValue"] ?? change.Properties["Value"];
        if (propertyName is null || value is null)
        {
            Unsupported(context, change, "The property change has no Property or ModifiedValue.");
            return;
        }

        if (propertyName.Equals("Version List", StringComparison.OrdinalIgnoreCase))
        {
            context.Report(DiagnosticDescriptors.ObjectMetadataKept,
                $"Version List change '{change.Properties["OriginalValue"]}' → '{value}' has no AL equivalent; it is shown in the header comment.",
                change.Position.Line, construct: "Version List", severity: DiagnosticSeverity.Info);
            return;
        }

        var targetText = change.Properties["Target"];
        var target = targetText is null ? null : ElementReference.Parse(targetText);
        var kind = target?.Kind?.ToUpperInvariant();
        var isObject = target is null || kind is null || kind is "TABLE" or "PAGE";

        var (scope, allowed) = (state.TargetType, isObject, kind) switch
        {
            (CalObjectType.Table, true, _) => (PropertyScope.Table, TableObjectModifiable),
            (CalObjectType.Table, false, "FIELD") => (PropertyScope.Field, TableFieldModifiable),
            (CalObjectType.Page, true, _) => (PropertyScope.Page, PageObjectModifiable),
            (CalObjectType.Page, false, "CONTROL") => (PropertyScope.Control, PageControlModifiable),
            (CalObjectType.Page, false, "ACTION") => (PropertyScope.Action, PageActionModifiable),
            _ => (PropertyScope.Table, (HashSet<string>?)null),
        };

        if (allowed is null || !allowed.Contains(propertyName))
        {
            Unsupported(context, change, $"Property '{propertyName}' cannot be changed {(isObject ? "on the object" : $"on a {target?.Kind?.ToLowerInvariant()}")} by an AL extension.");
            return;
        }

        var node = new CalElement { SectionName = "Change", Position = change.Position };
        node.Properties.Add(new CalProperty(propertyName, value, change.Position));
        FieldSymbolHint(state, target, out var fieldType);
        var converted = state.Properties.Convert(node, scope, new PropertyContext
        {
            Path = isObject ? $"{state.TargetType} {state.TargetName}" : $"{target!.Kind} {target.Name ?? target.Id?.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            CurrentTableId = state.TableId,
            FieldDataType = fieldType,
        });

        if (isObject)
        {
            state.Object!.Properties.AddRange(converted);
            return;
        }

        var modify = state.Modify(target!, isAction: kind == "ACTION", change.Position.Line);
        modify.Properties.AddRange(converted);
    }

    private static void FieldSymbolHint(ExtensionState state, ElementReference? target, out string? fieldType)
    {
        fieldType = null;
        if (state.TargetType == CalObjectType.Table && state.TableId is int tableId && target?.Name is { } name)
        {
            fieldType = state.Context.Symbols.FindField(tableId, name)?.DataType;
        }
    }

    // ---- Code changes ------------------------------------------------------------------------------

    private void ConvertCodeModification(ExtensionState state, CalElement change)
    {
        var context = state.Context;
        var targetText = change.Properties["Target"]?.Trim() ?? string.Empty;
        var original = change.Triggers.FirstOrDefault(t => t.Name.Equals("OriginalCode", StringComparison.OrdinalIgnoreCase));
        var modified = change.Triggers.FirstOrDefault(t => t.Name.Equals("ModifiedCode", StringComparison.OrdinalIgnoreCase));
        if (modified is null)
        {
            Unsupported(context, change, "The code modification has no ModifiedCode.", DiagnosticDescriptors.DeltaCodeModification);
            return;
        }

        var m = CodeTargetRegex().Match(targetText);
        var elementText = m.Success ? m.Groups["element"].Value : string.Empty;
        var triggerName = m.Success ? m.Groups["trigger"].Value : targetText;
        var element = elementText.Length == 0 ? null : ElementReference.Parse(elementText);

        var split = CodeSplit.Compute(original?.Body ?? CalCodeBody.Empty, modified.Body);
        if (split is null)
        {
            Unsupported(context, change,
                $"The change to '{targetText}' alters existing lines; AL extensions cannot change code in the base object.",
                DiagnosticDescriptors.DeltaCodeModification);
            return;
        }

        var originalLocals = new HashSet<string>((original?.Variables ?? []).Select(v => v.Name), StringComparer.OrdinalIgnoreCase);
        var unsafeReason = split.SafetyProblem(originalLocals);
        if (unsafeReason is not null)
        {
            Unsupported(context, change, $"The lines added to '{targetText}' cannot be moved to an extension trigger: {unsafeReason}", DiagnosticDescriptors.DeltaCodeModification);
            return;
        }

        var placements = ExtensionTriggers(state, element, triggerName, split.OriginalIsEmpty);
        if ((split.Before.Count > 0 && placements.Before is null) || (split.After.Count > 0 && placements.After is null))
        {
            Unsupported(context, change,
                $"AL has no extension trigger that runs {(split.Before.Count > 0 && placements.Before is null ? "before" : "after")} '{targetText}'.",
                DiagnosticDescriptors.DeltaCodeModification);
            return;
        }

        List<AlMethod> target;
        if (element is null)
        {
            target = state.Object!.Triggers;
        }
        else
        {
            target = state.Modify(element, isAction: placements.IsAction, change.Position.Line).Triggers;
        }

        var display = element is null ? triggerName : $"{elementText}.{triggerName}";
        foreach (var (lines, signature, position) in new[] { (split.Before, placements.Before, "before"), (split.After, placements.After, "after") })
        {
            if (lines.Count == 0 || signature is null)
            {
                continue;
            }

            var usedLocals = UsedVariables(modified.Variables, lines);
            var trigger = new CalTrigger { Name = signature.Name, Variables = usedLocals, Body = new CalCodeBody(lines), Position = change.Position };
            target.Add(state.Members.ConvertTrigger(trigger, signature, $"{display} ({position})"));
            context.Report(DiagnosticDescriptors.DeltaCodeExtended,
                $"{lines.Count} line(s) added {position} the code of '{display}' were converted to extension trigger {signature.Name}.",
                change.Position.Line, construct: "CodeModification", severity: DiagnosticSeverity.Info);
        }
    }

    private static List<CalVariable> UsedVariables(IEnumerable<CalVariable> variables, IReadOnlyList<CodeLine> lines)
    {
        var names = CodeLexer.Tokenize(string.Join("\n", lines.Select(l => l.Text)))
            .Where(t => t.IsIdentifier).Select(t => t.IdentifierName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return variables.Where(v => names.Contains(v.Name)).ToList();
    }

    private sealed record TriggerPlacement(TriggerSignature? Before, TriggerSignature? After, bool IsAction);

    private static TriggerPlacement ExtensionTriggers(ExtensionState state, ElementReference? element, string trigger, bool originalIsEmpty)
    {
        var kind = element?.Kind?.ToUpperInvariant();
        switch (state.TargetType, element is null ? "OBJECT" : kind)
        {
            case (CalObjectType.Table, "OBJECT") when TriggerMap.Find(TriggerOwner.Table, trigger) is { } t:
                return new(new TriggerSignature($"OnBefore{t.Name[2..]}"), new TriggerSignature(t.Name), false);
            case (CalObjectType.Table, "FIELD") when trigger.Equals("OnValidate", StringComparison.OrdinalIgnoreCase):
                return new(new TriggerSignature("OnBeforeValidate"), new TriggerSignature("OnAfterValidate"), false);
            case (CalObjectType.Page, "OBJECT") when TriggerMap.Find(TriggerOwner.Page, trigger) is { } p:
                // A page extension trigger runs after the base trigger; triggers with a result can only be added.
                return p.ReturnType is null || originalIsEmpty ? new(null, p, false) : new(null, null, false);
            case (CalObjectType.Page, "CONTROL") when trigger.Equals("OnValidate", StringComparison.OrdinalIgnoreCase):
                return new(new TriggerSignature("OnBeforeValidate"), new TriggerSignature("OnAfterValidate"), false);
            case (CalObjectType.Page, "CONTROL") when originalIsEmpty && TriggerMap.Find(TriggerOwner.PageField, trigger) is { } f:
                return new(null, f, false);
            case (CalObjectType.Page, "ACTION") when trigger.Equals("OnAction", StringComparison.OrdinalIgnoreCase):
                return originalIsEmpty
                    ? new(null, new TriggerSignature("OnAction"), true)
                    : new(new TriggerSignature("OnBeforeAction"), new TriggerSignature("OnAfterAction"), true);
            default:
                return new(null, null, kind == "ACTION");
        }
    }

    // ---- Artifact ----------------------------------------------------------------------------------

    private string BuildArtifact(CalObject obj)
    {
        var lines = obj.RawText.Split('\n');
        var sb = new StringBuilder();
        sb.Append("// Cal2AL migration artifact: changes from ").Append(Path.GetFileName(obj.SourceFile))
          .Append(" that could not be converted into an AL extension. Re-implement them manually.\r\n");
        foreach (var (change, reason) in unconverted)
        {
            sb.Append("\r\n// ---- line ").Append(change.Position.Line).Append(": ").Append(reason).Append("\r\n");
            var from = change.Position.Line - obj.Position.Line;
            var to = Math.Max(change.LastSourceLine, change.Position.Line) - obj.Position.Line;
            for (var i = Math.Max(0, from); i <= to && i < lines.Length; i++)
            {
                sb.Append(lines[i].TrimEnd('\r')).Append("\r\n");
            }
        }

        return sb.ToString();
    }

    [GeneratedRegex(@"^key\(Key(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex KeyHeaderRegex();

    [GeneratedRegex(@"^(?<element>.+)\.(?<trigger>(?:Import::|Export::)?On\w+)$", RegexOptions.IgnoreCase)]
    private static partial Regex CodeTargetRegex();

    /// <summary>Collects the sections of the extension object while changes are converted.</summary>
    private sealed class ExtensionState(ObjectContext context, MemberConverter members, PropertyConverter properties, CalObjectType targetType, int? targetId, string targetName)
    {
        private readonly Dictionary<string, AlSection> sections = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, AlSection> modifies = new(StringComparer.OrdinalIgnoreCase);
        private PageBuilder? builder;

        public ObjectContext Context { get; } = context;
        public MemberConverter Members { get; } = members;
        public PropertyConverter Properties { get; } = properties;
        public CalObjectType TargetType { get; } = targetType;
        public int? TargetId { get; } = targetId;
        public string TargetName { get; } = targetName;
        public AlObject? Object { get; set; }
        public int? TableId { get; set; }
        public List<AlMethod> Orphans { get; } = [];
        public string ExtensionName => AlSyntax.Unquote(Object?.Name ?? TargetName);

        public PageBuilder Builder => builder ??= new PageBuilder(Context, Members, Properties, TableId, isApi: false);

        public AlSection Section(string header)
        {
            if (!sections.TryGetValue(header, out var section))
            {
                section = new AlSection { Header = header };
                sections[header] = section;
            }

            return section;
        }

        /// <summary>The <c>modify(...)</c> block of a field, control or action, placed in the right section.</summary>
        public AlSection Modify(ElementReference element, bool isAction, int line)
        {
            var name = AnchorName(element, isAction, line);
            var key = $"{(isAction ? "a" : "c")}:{name}";
            if (!modifies.TryGetValue(key, out var modify))
            {
                modify = new AlSection { Header = $"modify({name})" };
                modifies[key] = modify;
                var parent = TargetType == CalObjectType.Table ? "fields" : isAction ? "actions" : "layout";
                Section(parent).Children.Add(modify);
            }

            return modify;
        }

        /// <summary>The AL name of an element of the base object, as used by addafter/modify.</summary>
        public string AnchorName(ElementReference element, bool isAction, int line)
        {
            if (element.Name is { Length: > 0 } name)
            {
                return AlSyntax.Identifier(name);
            }

            if (TargetType == CalObjectType.Table && TableId is int tableId && element.Id is int fieldNo
                && Context.Symbols.GetFields(tableId)?.FirstOrDefault(f => f.No == fieldNo) is { } field)
            {
                return AlSyntax.Identifier(field.Name);
            }

            var fallback = $"{(isAction ? "Action" : "Control")}{element.Id}";
            Context.Report(DiagnosticDescriptors.DeltaAnchorUnverified,
                $"'{element.Text}' has no name in the delta; '{fallback}' was used as the AL name of the base element.", line,
                construct: element.Text);
            return fallback;
        }

        public void Finish()
        {
            foreach (var header in new[] { "fields", "keys", "layout", "actions" })
            {
                if (sections.TryGetValue(header, out var section) && section.Children.Count > 0)
                {
                    Object!.Sections.Add(section);
                }
            }

            Object!.Procedures.AddRange(Orphans);
            if (builder is not null)
            {
                Object.Procedures.AddRange(builder.OrphanProcedures);
            }
        }
    }
}

public enum AnchorMode
{
    After,
    First,
}

/// <summary>
/// A reference to an element of the base object as C/SIDE writes it in deltas:
/// <c>"No."(Field 1)</c>, <c>Control 12</c>, <c>NoSeriesMgt(Variable 1002)</c>, or the new-syntax forms
/// <c>AddAfter:Name</c> / <c>AddFirst:ContentArea</c>.
/// </summary>
public sealed partial record ElementReference(string Text, string? Name, string? Kind, int? Id, AnchorMode Mode)
{
    public static ElementReference Parse(string text)
    {
        var value = text.Trim();
        var mode = AnchorMode.After;
        if (value.StartsWith("AddFirst:", StringComparison.OrdinalIgnoreCase))
        {
            mode = AnchorMode.First;
            value = value["AddFirst:".Length..].Trim();
        }
        else if (value.StartsWith("AddAfter:", StringComparison.OrdinalIgnoreCase))
        {
            value = value["AddAfter:".Length..].Trim();
        }

        var withName = NamedRegex().Match(value);
        if (withName.Success)
        {
            var name = withName.Groups["name"].Value.Trim();
            return new ElementReference(text, name.Length == 0 ? null : AlSyntax.Unquote(name), withName.Groups["kind"].Value,
                int.Parse(withName.Groups["id"].Value, System.Globalization.CultureInfo.InvariantCulture), mode);
        }

        var bare = BareRegex().Match(value);
        if (bare.Success)
        {
            return new ElementReference(text, null, bare.Groups["kind"].Value,
                int.Parse(bare.Groups["id"].Value, System.Globalization.CultureInfo.InvariantCulture), mode);
        }

        return new ElementReference(text, value.Length == 0 ? null : AlSyntax.Unquote(value), null, null, mode);
    }

    [GeneratedRegex(@"^(?<name>.*?)\s*\((?<kind>[A-Za-z]+)\s+(?<id>\d+)\)$")]
    private static partial Regex NamedRegex();

    [GeneratedRegex(@"^(?<kind>[A-Za-z]+)\s+(?<id>\d+)$")]
    private static partial Regex BareRegex();
}

/// <summary>
/// Splits a modified body into lines added before and after an unchanged copy of the original body.
/// Returns null when original lines were changed or removed.
/// </summary>
internal sealed class CodeSplit
{
    public required List<CodeLine> Before { get; init; }
    public required List<CodeLine> After { get; init; }
    public required CalCodeBody Original { get; init; }
    public bool OriginalIsEmpty => Original.IsEmpty;

    public static CodeSplit? Compute(CalCodeBody original, CalCodeBody modified)
    {
        var o = original.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Text)).ToList();
        var m = modified.Lines;
        if (o.Count == 0)
        {
            return new CodeSplit { Before = [], After = m.ToList(), Original = original };
        }

        // Find the original block as a contiguous run of lines inside the modified body.
        for (var start = 0; start + o.Count <= m.Count; start++)
        {
            var mi = start;
            var matched = 0;
            while (matched < o.Count && mi < m.Count)
            {
                if (string.IsNullOrWhiteSpace(m[mi].Text) && matched > 0)
                {
                    mi++;
                    continue;
                }

                if (!Same(m[mi].Text, o[matched].Text))
                {
                    break;
                }

                matched++;
                mi++;
            }

            if (matched == o.Count)
            {
                return new CodeSplit
                {
                    Before = Trim(m.Take(start).ToList()),
                    After = Trim(m.Skip(mi).ToList()),
                    Original = original,
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Why moving the added lines out of the base trigger could change behaviour, or null when it cannot:
    /// an EXIT in the original (the added "after" code would then run on paths where it did not), an EXIT in the
    /// "before" code (it would no longer skip the original), or added code that shares local variables with the
    /// original body.
    /// </summary>
    public string? SafetyProblem(IReadOnlySet<string> originalLocals)
    {
        var original = Tokens(Original.Lines);
        if (After.Count > 0 && original.Any(t => t.Is("EXIT")))
        {
            return "the original code contains EXIT, so the added lines did not always run";
        }

        var before = Tokens(Before);
        if (before.Any(t => t.Is("EXIT")))
        {
            return "the added lines contain EXIT, which skipped the original code";
        }

        var added = before.Concat(Tokens(After)).Where(t => t.IsIdentifier).Select(t => t.IdentifierName);
        var shared = added.FirstOrDefault(originalLocals.Contains);
        return shared is null ? null : $"they use local variable '{shared}' of the original code";
    }

    private static List<CodeToken> Tokens(IEnumerable<CodeLine> lines) =>
        CodeLexer.Tokenize(string.Join("\n", lines.Select(l => l.Text))).Where(t => !t.IsTrivia).ToList();

    private static bool Same(string a, string b) => string.Equals(a.Trim(), b.Trim(), StringComparison.Ordinal);

    private static List<CodeLine> Trim(List<CodeLine> lines)
    {
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0].Text))
        {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1].Text))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return lines;
    }
}
