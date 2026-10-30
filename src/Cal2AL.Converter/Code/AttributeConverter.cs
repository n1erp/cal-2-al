using System.Globalization;
using Cal2AL.Converter.Values;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;

namespace Cal2AL.Converter.Code;

/// <summary>Result of converting the attributes of one procedure.</summary>
public sealed record ConvertedAttributes(List<string> Attributes, string? AccessModifier, bool HasScopeAttribute);

/// <summary>Converts C/AL procedure attributes (<c>[EventSubscriber(...)]</c>, <c>[Integration]</c>, …) to AL.</summary>
public sealed class AttributeConverter(ObjectContext context)
{
    private static readonly HashSet<string> SameInAl = new(StringComparer.OrdinalIgnoreCase)
    {
        "TryFunction", "Test", "ConfirmHandler", "MessageHandler", "ModalPageHandler", "PageHandler", "ReportHandler",
        "RequestPageHandler", "StrMenuHandler", "FilterPageHandler", "HyperlinkHandler", "SessionSettingsHandler",
        "ServiceEnabled", "NonDebuggable", "Normal", "CatalogItemHandler",
    };

    public ConvertedAttributes Convert(CalProcedure procedure)
    {
        var attributes = new List<string>();
        string? access = null;
        var hasScope = false;
        var line = procedure.Position.Line;

        foreach (var raw in procedure.Attributes)
        {
            var (name, args) = Split(raw);
            switch (name.ToUpperInvariant())
            {
                case "EXTERNAL":
                    // C/AL [External]: callable from extensions. AL procedures are public by default — nothing to emit.
                    break;

                case "INTERNAL":
                    if (context.Options.TargetRuntime >= new Version(5, 0))
                    {
                        access = "internal";
                    }
                    else
                    {
                        attributes.Add("[Scope('OnPrem')]");
                        hasScope = true;
                    }

                    break;

                case "INTEGRATION":
                    attributes.Add($"[IntegrationEvent({Bool(args, 0)}, {Bool(args, 1)})]");
                    break;

                case "BUSINESS":
                    attributes.Add($"[BusinessEvent({Bool(args, 0)})]");
                    break;

                case "EVENTSUBSCRIBER":
                    attributes.Add(ConvertEventSubscriber(raw, args, line));
                    break;

                case "HANDLERFUNCTIONS":
                    attributes.Add($"[HandlerFunctions({AlSyntax.String(string.Join(",", args.Select(a => a.Trim())))})]");
                    break;

                case "TRANSACTIONMODEL":
                    attributes.Add($"[TransactionModel(TransactionModel::{First(args)})]");
                    break;

                case "TESTPERMISSIONS":
                    attributes.Add($"[TestPermissions(TestPermissions::{First(args)})]");
                    break;

                case "SENDNOTIFICATIONHANDLER":
                case "RECALLNOTIFICATIONHANDLER":
                {
                    var alName = name.ToUpperInvariant() == "SENDNOTIFICATIONHANDLER" ? "SendNotificationHandler" : "RecallNotificationHandler";
                    attributes.Add(args.Count == 0
                        ? $"[{alName}]"
                        : $"[{alName}({(First(args).Equals("Optional", StringComparison.OrdinalIgnoreCase) ? "true" : "false")})]");
                    break;
                }

                case "NONDEBUGGABLEATTRIBUTE":
                    attributes.Add("[NonDebuggable]");
                    break;

                case "LINESTART":
                    context.Report(DiagnosticDescriptors.AttributeDropped,
                        $"[{raw}] only records C/SIDE line numbers and has no meaning in AL; it was not emitted.",
                        line, construct: $"[{raw}]", member: procedure.Name);
                    break;

                case "TABLESYNCSETUP":
                case "CHECKPRECONDITION":
                case "UPGRADEPERCOMPANY":
                case "UPGRADEPERDATABASE":
                    context.Report(DiagnosticDescriptors.AttributeDropped,
                        $"[{raw}] belongs to the C/AL upgrade-codeunit model, which AL replaced with upgrade triggers. The procedure will no longer run automatically during an upgrade.",
                        line, construct: $"[{raw}]", member: procedure.Name, severity: DiagnosticSeverity.Error,
                        resolution: name.ToUpperInvariant() == "TABLESYNCSETUP"
                            ? "Table synchronisation is handled by schema upgrade in AL; move data-copy logic into OnUpgradePerCompany/OnUpgradePerDatabase."
                            : "Call this procedure from the OnUpgradePerCompany, OnUpgradePerDatabase or OnValidateUpgrade… trigger of an Upgrade codeunit.",
                        original: $"[{raw}]");
                    break;

                default:
                    if (SameInAl.Contains(name))
                    {
                        attributes.Add(args.Count == 0 && !raw.Contains('(') ? $"[{name}]" : $"[{raw}]");
                    }
                    else
                    {
                        context.Report(DiagnosticDescriptors.UnknownAttribute,
                            $"Attribute [{raw}] is not known to Cal2AL; it was emitted unchanged.",
                            line, construct: $"[{raw}]", member: procedure.Name);
                        attributes.Add($"[{raw}]");
                    }

                    break;
            }
        }

        return new ConvertedAttributes(attributes, access, hasScope);
    }

    private string ConvertEventSubscriber(string raw, IReadOnlyList<string> args, int line)
    {
        if (args.Count < 3)
        {
            context.Report(DiagnosticDescriptors.UnknownAttribute,
                $"[{raw}] does not have the expected arguments (ObjectType, ObjectId, EventName, ElementName, OnMissingLicense, OnMissingPermission); it was emitted unchanged.",
                line, construct: $"[{raw}]", severity: DiagnosticSeverity.Error);
            return $"[{raw}]";
        }

        var objectTypeText = args[0].Trim();
        var objectType = CalObjectTypeExtensions.ParseObjectType(objectTypeText);
        var objectIdText = args[1].Trim();
        string objectReference;
        if (int.TryParse(objectIdText, NumberStyles.None, CultureInfo.InvariantCulture, out var objectId) && objectType != CalObjectType.Unknown)
        {
            var name = context.Symbols.GetName(objectType, objectId);
            objectReference = name is null ? objectIdText : $"{objectType.ToAlIdPrefix()}::{AlSyntax.Identifier(name)}";
            if (name is null)
            {
                context.Report(DiagnosticDescriptors.UnresolvedObjectReference,
                    $"{objectType} {objectId} (event publisher) is not in the converted set; the ID was kept.",
                    line, construct: $"[{raw}]");
            }
        }
        else
        {
            objectReference = objectIdText;
        }

        var eventName = AlSyntax.Unquote(args[2].Trim());
        var element = args.Count > 3 ? AlSyntax.Unquote(args[3].Trim()) : string.Empty;
        var skipLicense = args.Count > 4 && args[4].Trim().Equals("Skip", StringComparison.OrdinalIgnoreCase);
        var skipPermission = args.Count > 5 && args[5].Trim().Equals("Skip", StringComparison.OrdinalIgnoreCase);
        var alObjectType = objectType == CalObjectType.Unknown ? objectTypeText : objectType.ToString();
        return $"[EventSubscriber(ObjectType::{alObjectType}, {objectReference}, {AlSyntax.String(eventName)}, {AlSyntax.String(element)}, {(skipLicense ? "true" : "false")}, {(skipPermission ? "true" : "false")})]";
    }

    private static string Bool(IReadOnlyList<string> args, int index)
    {
        if (index >= args.Count)
        {
            return "false";
        }

        var v = args[index].Trim();
        return v.Equals("DEFAULT", StringComparison.OrdinalIgnoreCase) ? "false" : AlUtilForAttributes.ToBool(v);
    }

    private static string First(IReadOnlyList<string> args) => args.Count > 0 ? args[0].Trim() : string.Empty;

    /// <summary>Splits <c>Name(a,b,"c,d")</c> into the name and top-level arguments.</summary>
    public static (string Name, IReadOnlyList<string> Args) Split(string attribute)
    {
        var open = attribute.IndexOf('(');
        if (open < 0)
        {
            return (attribute.Trim(), []);
        }

        var close = attribute.LastIndexOf(')');
        var inner = close > open ? attribute[(open + 1)..close] : attribute[(open + 1)..];
        var args = FilterExpressionConverter.SplitTopLevel(inner, ',');
        // keep empty arguments ("" for ElementName) — SplitTopLevel drops blank items, so split manually when needed
        if (inner.Contains(",,", StringComparison.Ordinal) || inner.EndsWith(',') || inner.StartsWith(','))
        {
            args = inner.Split(',').ToList();
        }

        return (attribute[..open].Trim(), args);
    }

    private static class AlUtilForAttributes
    {
        public static string ToBool(string v) => v.ToUpperInvariant() switch
        {
            "TRUE" or "YES" => "true",
            "FALSE" or "NO" => "false",
            _ => v,
        };
    }
}
