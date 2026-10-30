using Cal2AL.Converter.Code;
using Cal2AL.Converter.Properties;
using Cal2AL.Converter.Values;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;
using Cal2AL.Generator.Model;

namespace Cal2AL.Converter.Objects;

/// <summary>
/// Builds the <c>layout</c> and <c>actions</c> sections of pages and request pages from C/AL's flat CONTROLS and
/// ACTIONS lists. Every control and action gets a unique AL name: its Name property, else its source expression
/// (fields), else its ENU caption, else <c>Control&lt;id&gt;</c>/<c>Action&lt;id&gt;</c>.
/// </summary>
public sealed class PageBuilder(ObjectContext context, MemberConverter members, PropertyConverter properties, int? sourceTableId, bool isApi)
{
    private static readonly Dictionary<string, string> AreaByContainerType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ContentArea"] = "Content",
        ["FactBoxArea"] = "FactBoxes",
        ["RoleCenterArea"] = "RoleCenter",
    };

    private static readonly Dictionary<string, string> AreaByActionContainerType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["NewDocumentItems"] = "Creation",
        ["HomeItems"] = "Embedding",
        ["RelatedInformation"] = "Navigation",
        ["ActionItems"] = "Processing",
        ["Reports"] = "Reporting",
        ["ActivityButtons"] = "Sections",
    };

    private static readonly Dictionary<string, string> GroupKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Group"] = "group",
        ["Repeater"] = "repeater",
        ["CueGroup"] = "cuegroup",
        ["FixedLayout"] = "fixed",
        ["GridLayout"] = "grid",
    };

    /// <summary>User controls by C/AL control ID and by name, for placing control add-in event triggers.</summary>
    public Dictionary<int, AlSection> UserControlsById { get; } = [];

    public Dictionary<string, AlSection> UserControlsByName { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Code of triggers that have no AL equivalent, preserved as local procedures of the object.</summary>
    public List<AlMethod> OrphanProcedures { get; } = [];

    public AlSection? BuildLayout(IReadOnlyList<CalElement> controls)
    {
        if (controls.Count == 0)
        {
            return null;
        }

        var layout = new AlSection { Header = "layout" };
        foreach (var root in ElementTree.Build(controls, 1))
        {
            layout.Children.Add(BuildControl(root));
        }

        ResolveProviders(layout);
        return layout;
    }

    /// <summary>Builds controls without an enclosing layout section (for page extension insertions).</summary>
    public List<AlSection> BuildControls(IReadOnlyList<CalElement> controls) =>
        ElementTree.Build(controls, 1).Select(BuildControl).ToList();

    /// <summary>Builds actions without an enclosing actions section (for page extension insertions).</summary>
    public List<AlSection> BuildActionItems(IReadOnlyList<CalElement> actions) =>
        ElementTree.Build(actions, 1).Select(BuildAction).ToList();

    public AlSection? BuildActions(IReadOnlyList<CalElement>? actions)
    {
        if (actions is null || actions.Count == 0)
        {
            return null;
        }

        var section = new AlSection { Header = "actions" };
        AlSection? implicitArea = null;
        foreach (var root in ElementTree.Build(actions, 1))
        {
            var converted = BuildAction(root);
            if (root.Element.GetPositional(2).Trim().Equals("ActionContainer", StringComparison.OrdinalIgnoreCase))
            {
                section.Children.Add(converted);
                continue;
            }

            // C/AL allowed actions outside a container on some page types (e.g. ListPart); AL requires an area.
            if (implicitArea is null)
            {
                implicitArea = new AlSection { Header = "area(Processing)" };
                section.Children.Add(implicitArea);
                context.Report(DiagnosticDescriptors.PropertyValueAdjusted,
                    "Actions without an action container were placed in area(Processing).", root.Element.Position.Line);
            }

            implicitArea.Children.Add(converted);
        }

        return section;
    }

    /// <summary>AL names of controls by C/AL control ID, for ProviderID references.</summary>
    private readonly Dictionary<string, string> controlNamesById = new(StringComparer.Ordinal);

    /// <summary>
    /// C/AL <c>ProviderID=1120022</c> refers to another part by control ID; AL <c>Provider</c> needs that part's name,
    /// which is only known once every control has been named.
    /// </summary>
    private void ResolveProviders(AlSection section)
    {
        for (var i = 0; i < section.Properties.Count; i++)
        {
            var property = section.Properties[i];
            if (property.Name == "Provider")
            {
                var id = AlSyntax.Unquote(property.Value.Trim());
                if (controlNamesById.TryGetValue(id, out var name))
                {
                    section.Properties[i] = property with { Value = AlSyntax.Identifier(name) };
                }
                else
                {
                    context.Report(DiagnosticDescriptors.UnresolvedObjectReference,
                        $"ProviderID {id} does not match a control of this page; the value was kept.", construct: "ProviderID", severity: DiagnosticSeverity.Warning);
                }
            }
        }

        foreach (var child in section.Children)
        {
            ResolveProviders(child);
        }
    }

    /// <summary>The AL name most recently given to a control or action (areas have none).</summary>
    private string? lastReservedName;

    private AlSection BuildControl(ElementNode node)
    {
        var item = node.Element;
        var id = item.GetPositional(0).Trim();
        var controlType = item.GetPositional(2).Trim();
        var line = item.Position.Line;
        context.CurrentElement = $"Control {id}";
        lastReservedName = null;

        AlSection section;
        string? groupType = null;
        var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        switch (controlType.ToUpperInvariant())
        {
            case "CONTAINER":
            {
                var containerType = item.Properties["ContainerType"]?.Trim() ?? "ContentArea";
                if (!AreaByContainerType.TryGetValue(containerType, out var area))
                {
                    context.Report(DiagnosticDescriptors.PropertyValueNotParsed,
                        $"Unknown container type '{containerType}'; area(Content) was used.", line);
                    area = "Content";
                }

                section = new AlSection { Header = $"area({area})" };
                if (item.Properties.Contains("CaptionML"))
                {
                    exclude.Add("CaptionML");
                    context.Report(DiagnosticDescriptors.PropertyInert,
                        "The caption of a container control is not shown and AL areas have no caption; it was not emitted.", line);
                }

                break;
            }

            case "GROUP":
            {
                groupType = item.Properties["GroupType"]?.Trim() ?? "Group";
                if (!GroupKeywords.TryGetValue(groupType, out var keyword))
                {
                    context.Report(DiagnosticDescriptors.PropertyValueNotParsed, $"Unknown group type '{groupType}'; group was used.", line);
                    keyword = "group";
                }

                section = new AlSection { Header = $"{keyword}({ControlName(item, id, allowSourceExpr: false)})" };
                break;
            }

            case "FIELD":
            {
                var addIn = item.Properties["ControlAddIn"];
                var sourceExpr = item.Properties["SourceExpr"];
                if (!string.IsNullOrWhiteSpace(addIn))
                {
                    var addInName = AlSyntax.Unquote(AlSyntax.UnquoteString(addIn.Trim())).Split(';')[0].Trim();
                    section = new AlSection { Header = $"usercontrol({ControlName(item, id, allowSourceExpr: false)}; {AlSyntax.QuotedIdentifier(addInName)})" };
                    if (int.TryParse(id, out var numericId))
                    {
                        UserControlsById[numericId] = section;
                    }

                    if (item.Properties["Name"] is { } controlName)
                    {
                        UserControlsByName[controlName.Trim()] = section;
                    }
                }
                else if (string.IsNullOrWhiteSpace(sourceExpr))
                {
                    section = new AlSection { Header = $"label({ControlName(item, id, allowSourceExpr: false)})" };
                }
                else
                {
                    section = new AlSection { Header = $"field({ControlName(item, id, allowSourceExpr: true)}; {FilterExpressionConverter.Flatten(sourceExpr)})" };
                }

                break;
            }

            case "PART":
                section = BuildPart(item, id, line);
                break;

            default:
                context.Report(DiagnosticDescriptors.PropertyValueNotParsed, $"Unknown control type '{controlType}'; it was converted as a group.", line);
                section = new AlSection { Header = $"group({ControlName(item, id, allowSourceExpr: false)})" };
                break;
        }

        var alName = lastReservedName;
        if (alName is not null)
        {
            controlNamesById[id] = alName;
        }

        var element = alName is null ? $"Control {id}" : $"Control {id} {alName}";
        context.CurrentElement = element;
        int? targetTable = null;
        if (item.Properties["PagePartID"] is { } partPage)
        {
            targetTable = context.Symbols.GetPageSourceTableId(partPage);
        }

        section.Properties.AddRange(properties.Convert(item, PropertyScope.Control, new PropertyContext
        {
            Path = element,
            CurrentTableId = sourceTableId,
            TargetTableId = targetTable,
            GroupType = groupType,
            IsApiObject = isApi,
        }, exclude));

        foreach (var child in node.Children)
        {
            section.Children.Add(BuildControl(child));
        }

        if (item.GetSection("ACTIONS") is { Items.Count: > 0 } controlActions)
        {
            var nested = new AlSection { Header = "actions" };
            foreach (var root in ElementTree.Build(controlActions.Items, 1))
            {
                nested.Children.Add(BuildAction(root));
            }

            section.Children.Add(nested);
        }

        context.CurrentElement = element;
        var isField = controlType.Equals("Field", StringComparison.OrdinalIgnoreCase);
        foreach (var trigger in item.Triggers)
        {
            var signature = isField ? TriggerMap.Find(TriggerOwner.PageField, trigger.Name) : null;
            if (signature is null)
            {
                OrphanProcedures.Add(members.ConvertTriggerAsProcedure(trigger, alName ?? $"Control{id}", $"AL has no '{trigger.Name}' trigger on a {controlType.ToLowerInvariant()} control."));
                continue;
            }

            section.Triggers.Add(members.ConvertTrigger(trigger, signature, $"{element} - {signature.Name}"));
        }

        context.CurrentElement = null;
        return section;
    }

    private AlSection BuildPart(CalElement item, string id, int line)
    {
        var partType = item.Properties["PartType"]?.Trim();
        var name = ControlName(item, id, allowSourceExpr: false);
        if (string.Equals(partType, "System", StringComparison.OrdinalIgnoreCase) || item.Properties.Contains("SystemPartID"))
        {
            var system = item.Properties["SystemPartID"]?.Trim() ?? "Notes";
            if (system.Equals("RecordLinks", StringComparison.OrdinalIgnoreCase))
            {
                system = "Links";
            }

            return new AlSection { Header = $"systempart({name}; {system})" };
        }

        if (string.Equals(partType, "Chart", StringComparison.OrdinalIgnoreCase) || item.Properties.Contains("ChartPartID"))
        {
            var chart = item.Properties["ChartPartID"]?.Trim() ?? string.Empty;
            context.Report(DiagnosticDescriptors.PropertyValueAdjusted,
                $"Chart part '{chart}' was converted to chartpart(); verify that the chart exists in Business Central.", line);
            return new AlSection { Header = $"chartpart({name}; {AlSyntax.QuotedIdentifier(AlSyntax.Unquote(chart))})" };
        }

        var page = item.Properties["PagePartID"];
        if (page is null)
        {
            context.Report(DiagnosticDescriptors.MissingRequiredPart, $"Part control {id} has no PagePartID.", line);
            return new AlSection { Header = $"part({name}; 0)" };
        }

        return new AlSection { Header = $"part({name}; {ObjectReferences.Resolve(context, page, CalObjectType.Page, line)})" };
    }

    private AlSection BuildAction(ElementNode node)
    {
        var item = node.Element;
        var id = item.GetPositional(0).Trim();
        var actionType = item.GetPositional(2).Trim();
        var line = item.Position.Line;
        context.CurrentElement = $"Action {id}";
        lastReservedName = null;

        string header;
        var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        switch (actionType.ToUpperInvariant())
        {
            case "ACTIONCONTAINER":
            {
                var containerType = item.Properties["ActionContainerType"]?.Trim() ?? "ActionItems";
                if (!AreaByActionContainerType.TryGetValue(containerType, out var area))
                {
                    context.Report(DiagnosticDescriptors.PropertyValueNotParsed, $"Unknown action container type '{containerType}'; area(Processing) was used.", line);
                    area = "Processing";
                }

                header = $"area({area})";
                if (item.Properties.Contains("CaptionML"))
                {
                    exclude.Add("CaptionML");
                    context.Report(DiagnosticDescriptors.PropertyInert,
                        "AL action areas have no caption; the caption of the action container was not emitted.", line);
                }

                break;
            }

            case "ACTIONGROUP":
                header = $"group({ActionName(item, id)})";
                break;
            case "SEPARATOR":
                header = $"separator({ActionName(item, id)})";
                break;
            default:
                header = $"action({ActionName(item, id)})";
                break;
        }

        var actionName = lastReservedName;
        var element = actionName is null ? $"Action {id}" : $"Action {id} {actionName}";
        context.CurrentElement = element;
        var section = new AlSection { Header = header };
        int? targetTable = null;
        if (item.Properties["RunObject"] is { } runObject && runObject.TrimStart().StartsWith("Page", StringComparison.OrdinalIgnoreCase))
        {
            targetTable = context.Symbols.GetPageSourceTableId(runObject.Trim()[4..].Trim());
        }

        section.Properties.AddRange(properties.Convert(item, PropertyScope.Action, new PropertyContext
        {
            Path = element,
            CurrentTableId = sourceTableId,
            TargetTableId = targetTable,
            IsApiObject = isApi,
        }, exclude));

        foreach (var child in node.Children)
        {
            section.Children.Add(BuildAction(child));
        }

        context.CurrentElement = element;
        foreach (var trigger in item.Triggers)
        {
            var signature = TriggerMap.Find(TriggerOwner.PageAction, trigger.Name);
            if (signature is null)
            {
                OrphanProcedures.Add(members.ConvertTriggerAsProcedure(trigger, actionName ?? $"Action{id}", $"AL has no '{trigger.Name}' trigger on an action."));
                continue;
            }

            section.Triggers.Add(members.ConvertTrigger(trigger, signature, $"{element} - {signature.Name}"));
        }

        context.CurrentElement = null;
        return section;
    }

    private string ControlName(CalElement item, string id, bool allowSourceExpr)
    {
        var fallback = $"Control{id}";
        var preferred = PreferredName(item, allowSourceExpr) ?? fallback;
        lastReservedName = context.ReserveName(preferred, fallback, item.Position.Line);
        return AlSyntax.Identifier(lastReservedName);
    }

    private string ActionName(CalElement item, string id)
    {
        var fallback = $"Action{id}";
        var preferred = PreferredName(item, allowSourceExpr: false) ?? fallback;
        lastReservedName = context.ReserveName(preferred, fallback, item.Position.Line);
        return AlSyntax.Identifier(lastReservedName);
    }

    private static string? PreferredName(CalElement item, bool allowSourceExpr)
    {
        var name = item.Properties["Name"]?.Trim();
        if (!string.IsNullOrEmpty(name))
        {
            return AlSyntax.Unquote(name);
        }

        if (allowSourceExpr && item.Properties["SourceExpr"] is { } expr && IsSimpleIdentifier(FilterExpressionConverter.Flatten(expr)))
        {
            return AlSyntax.Unquote(FilterExpressionConverter.Flatten(expr));
        }

        if (item.Properties["CaptionML"] is { } caption
            && MultiLanguageText.Parse(caption)?.FirstOrDefault(e => e.LanguageCode == "ENU") is { Text.Length: > 0 and <= 120 } enu
            && !enu.Text.Contains('"'))
        {
            var text = enu.Text.Replace("&", string.Empty, StringComparison.Ordinal).Trim();
            return text.Length > 0 ? text : null;
        }

        return null;
    }

    private static bool IsSimpleIdentifier(string expression)
    {
        var e = expression.Trim();
        if (AlSyntax.IsQuoted(e))
        {
            return !e[1..^1].Contains('"');
        }

        return AlSyntax.IsPlainIdentifier(e);
    }
}

