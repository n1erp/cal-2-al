namespace Cal2AL.Converter.Code;

/// <summary>Where a trigger is declared, which decides its AL signature.</summary>
public enum TriggerOwner
{
    Table,
    TableField,
    Page,
    PageField,
    PageAction,
    Report,
    ReportDataItem,
    XmlPort,
    XmlPortTableElement,
    XmlPortFieldElement,
    XmlPortTextElement,
    Query,
    Codeunit,
}

/// <summary>An AL trigger signature.</summary>
public sealed record TriggerSignature(string Name, string Parameters = "", string? ReturnType = null);

/// <summary>
/// Maps C/AL trigger names to AL trigger signatures. Implicit parameters (the <c>Text</c> of a page field's OnLookup,
/// <c>Which</c> of OnFindRecord, …) keep the names C/AL used, so the existing code keeps compiling.
/// </summary>
public static class TriggerMap
{
    private static readonly Dictionary<TriggerOwner, Dictionary<string, TriggerSignature>> Map = new()
    {
        [TriggerOwner.Table] = Make(
            ("OnInsert", new("OnInsert")), ("OnModify", new("OnModify")), ("OnDelete", new("OnDelete")), ("OnRename", new("OnRename"))),
        [TriggerOwner.TableField] = Make(
            ("OnValidate", new("OnValidate")), ("OnLookup", new("OnLookup"))),
        [TriggerOwner.Page] = Make(
            ("OnInit", new("OnInit")),
            ("OnOpenPage", new("OnOpenPage")),
            ("OnClosePage", new("OnClosePage")),
            ("OnFindRecord", new("OnFindRecord", "Which: Text", "Boolean")),
            ("OnNextRecord", new("OnNextRecord", "Steps: Integer", "Integer")),
            ("OnAfterGetRecord", new("OnAfterGetRecord")),
            ("OnNewRecord", new("OnNewRecord", "BelowxRec: Boolean")),
            ("OnInsertRecord", new("OnInsertRecord", "BelowxRec: Boolean", "Boolean")),
            ("OnModifyRecord", new("OnModifyRecord", string.Empty, "Boolean")),
            ("OnDeleteRecord", new("OnDeleteRecord", string.Empty, "Boolean")),
            ("OnQueryClosePage", new("OnQueryClosePage", "CloseAction: Action", "Boolean")),
            ("OnAfterGetCurrRecord", new("OnAfterGetCurrRecord"))),
        [TriggerOwner.PageField] = Make(
            ("OnValidate", new("OnValidate")),
            ("OnLookup", new("OnLookup", "var Text: Text", "Boolean")),
            ("OnDrillDown", new("OnDrillDown")),
            ("OnAssistEdit", new("OnAssistEdit")),
            ("OnControlAddIn", new("OnControlAddIn", "Index: Integer; Data: Text"))),
        [TriggerOwner.PageAction] = Make(("OnAction", new("OnAction"))),
        [TriggerOwner.Report] = Make(
            ("OnInitReport", new("OnInitReport")), ("OnPreReport", new("OnPreReport")), ("OnPostReport", new("OnPostReport"))),
        [TriggerOwner.ReportDataItem] = Make(
            ("OnPreDataItem", new("OnPreDataItem")), ("OnAfterGetRecord", new("OnAfterGetRecord")), ("OnPostDataItem", new("OnPostDataItem"))),
        [TriggerOwner.XmlPort] = Make(
            ("OnInitXMLport", new("OnInitXmlPort")), ("OnPreXMLport", new("OnPreXmlPort")), ("OnPostXMLport", new("OnPostXmlPort"))),
        [TriggerOwner.XmlPortTableElement] = Make(
            ("Import::OnAfterInitRecord", new("OnAfterInitRecord")),
            ("Import::OnBeforeInsertRecord", new("OnBeforeInsertRecord")),
            ("Import::OnAfterInsertRecord", new("OnAfterInsertRecord")),
            ("Import::OnBeforeModifyRecord", new("OnBeforeModifyRecord")),
            ("Import::OnAfterModifyRecord", new("OnAfterModifyRecord")),
            ("Export::OnPreXMLItem", new("OnPreXmlItem")),
            ("Export::OnAfterGetRecord", new("OnAfterGetRecord"))),
        [TriggerOwner.XmlPortFieldElement] = Make(
            ("Import::OnAfterAssignField", new("OnAfterAssignField")),
            ("Export::OnBeforePassField", new("OnBeforePassField"))),
        [TriggerOwner.XmlPortTextElement] = Make(
            ("Import::OnAfterAssignVariable", new("OnAfterAssignVariable")),
            ("Export::OnBeforePassVariable", new("OnBeforePassVariable"))),
        [TriggerOwner.Query] = Make(("OnBeforeOpen", new("OnBeforeOpen"))),
        [TriggerOwner.Codeunit] = Make(("OnRun", new("OnRun"))),
    };

    private static Dictionary<string, TriggerSignature> Make(params (string Name, TriggerSignature Signature)[] entries) =>
        entries.ToDictionary(e => e.Name, e => e.Signature, StringComparer.OrdinalIgnoreCase);

    /// <summary>Returns the AL signature, or null when the trigger has no AL equivalent in this position.</summary>
    public static TriggerSignature? Find(TriggerOwner owner, string calTriggerName) =>
        Map[owner].GetValueOrDefault(calTriggerName.Trim());
}
