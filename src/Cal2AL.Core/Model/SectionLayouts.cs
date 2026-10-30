namespace Cal2AL.Core.Model;

/// <summary>
/// Knowledge about the shape of TXT sections: how many leading positional values each kind of item has,
/// and which sections can appear in which object type. This describes the C/SIDE TXT format; it was
/// written from the format itself, not taken from the Microsoft importer.
/// </summary>
public static class SectionLayouts
{
    // Positional value names, for diagnostics and readable code.
    public static readonly string[] FieldColumns = ["FieldNo", "Enabled", "Name", "DataType"];
    public static readonly string[] KeyColumns = ["Enabled", "Key"];
    public static readonly string[] FieldGroupColumns = ["Id", "Name", "Fields"];
    public static readonly string[] PageControlColumns = ["Id", "Level", "ControlType"];
    public static readonly string[] PageActionColumns = ["Id", "Level", "ActionType"];
    public static readonly string[] ReportDataItemColumns = ["Id", "Indentation", "DataItemType", "Name"];
    public static readonly string[] ReportLabelColumns = ["Id", "Name"];
    public static readonly string[] XmlPortElementColumns = ["ElementId", "Indentation", "Name", "NodeType", "SourceType"];
    public static readonly string[] XmlPortEventColumns = ["Name"];
    public static readonly string[] QueryElementColumns = ["Id", "Level", "QueryElementType", "Name"];
    public static readonly string[] MenuNodeColumns = ["MenuNodeType", "MenuNodeId"];
    public static readonly string[] ChangeColumns = ["ChangeType"];
    public static readonly string[] FormControlColumns = ["Id", "ControlType", "X", "Y", "Width", "Height"];
    public static readonly string[] DataportFieldColumns = ["StartPos", "Width", "SourceExpr"];

    /// <summary>
    /// Returns the positional column names for items of <paramref name="sectionName"/> inside a node of
    /// <paramref name="ownerType"/>, or <c>null</c> when the section is unknown for that owner.
    /// </summary>
    /// <param name="objectType">Type of the object the section belongs to.</param>
    /// <param name="ownerType">"Object", "RequestPage", "Control", "Change", … — the kind of node that contains the section.</param>
    /// <param name="sectionName">Section or container name, e.g. FIELDS or ActionCollection.</param>
    public static string[]? GetColumns(CalObjectType objectType, string ownerType, string sectionName)
    {
        var s = sectionName.ToUpperInvariant();

        // Container properties used in delta files (ChangedElements=FieldCollection { ... }).
        switch (s)
        {
            case "FIELDCOLLECTION": return FieldColumns;
            case "KEYCOLLECTION": return KeyColumns;
            case "FIELDGROUPCOLLECTION": return FieldGroupColumns;
            case "CONTROLCOLLECTION": return PageControlColumns;
            case "ACTIONCOLLECTION": return PageActionColumns;
            case "DATAITEMCOLLECTION": return ReportDataItemColumns;
            case "QUERYELEMENTCOLLECTION": return QueryElementColumns;
            case "XMLPORTELEMENTCOLLECTION": return XmlPortElementColumns;
            case "XMLPORTEVENTCOLLECTION": return XmlPortEventColumns;
            case "LABELCOLLECTION": return ReportLabelColumns;
            case "CHANGECOLLECTION":
            case "CHANGES": return ChangeColumns;
            case "MENUNODECOLLECTION": return MenuNodeColumns;
        }

        return objectType switch
        {
            CalObjectType.Table => s switch
            {
                "FIELDS" => FieldColumns,
                "KEYS" => KeyColumns,
                "FIELDGROUPS" => FieldGroupColumns,
                _ => null,
            },
            CalObjectType.Page => s switch
            {
                "CONTROLS" => PageControlColumns,
                "ACTIONS" => PageActionColumns,
                _ => null,
            },
            CalObjectType.Report => s switch
            {
                "DATASET" => ReportDataItemColumns,
                "LABELS" => ReportLabelColumns,
                "CONTROLS" => PageControlColumns, // request page
                "ACTIONS" => PageActionColumns,  // request page
                _ => null,
            },
            CalObjectType.XmlPort => s switch
            {
                "ELEMENTS" => XmlPortElementColumns,
                "EVENTS" => XmlPortEventColumns,
                "CONTROLS" => PageControlColumns, // request page
                "ACTIONS" => PageActionColumns,
                _ => null,
            },
            CalObjectType.Query => s switch
            {
                "ELEMENTS" => QueryElementColumns,
                _ => null,
            },
            CalObjectType.MenuSuite => s == "MENUNODES" ? MenuNodeColumns : null,
            CalObjectType.Form => s == "CONTROLS" ? FormControlColumns : null,
            CalObjectType.Dataport => s == "FIELDS" ? DataportFieldColumns : null,
            CalObjectType.Modification => s switch
            {
                // Full sections can appear inside Insertion changes of a delta.
                "FIELDS" => FieldColumns,
                "KEYS" => KeyColumns,
                "FIELDGROUPS" => FieldGroupColumns,
                "CONTROLS" => PageControlColumns,
                "ACTIONS" => PageActionColumns,
                _ => null,
            },
            _ => null,
        };
    }

    /// <summary>Sections whose single body is a nested node (<c>REQUESTPAGE { PROPERTIES {…} CONTROLS {…} }</c>).</summary>
    public static bool IsSingleNodeSection(string sectionName) =>
        sectionName.Equals("REQUESTPAGE", StringComparison.OrdinalIgnoreCase)
        || sectionName.Equals("REQUESTFORM", StringComparison.OrdinalIgnoreCase)
        || sectionName.Equals("RequestPage", StringComparison.OrdinalIgnoreCase);

    /// <summary>Delta container properties whose content is C/AL declarations rather than <c>{ … }</c> items.</summary>
    public static bool IsDeclarationContainer(string containerName) =>
        containerName.ToUpperInvariant() is "VARIABLECOLLECTION" or "PROCEDURECOLLECTION" or "EVENTCOLLECTION"
            or "PARAMETERCOLLECTION" or "RETURNVALUECOLLECTION";

    /// <summary>Property names whose value is a block of code (used in delta files).</summary>
    public static bool IsCodeProperty(string propertyName) =>
        propertyName.ToUpperInvariant() is "ORIGINALCODE" or "MODIFIEDCODE" or "TARGETCODE" or "BASELINECODE"
            or "RESULTINGCODE" or "NEWBASELINECODE";

    /// <summary>Sections that only exist in pre-RTC (NAV 2009 classic) reports.</summary>
    public static bool IsClassicReportSection(string sectionName) =>
        sectionName.ToUpperInvariant() is "DATAITEMS" or "SECTIONS" or "REQUESTFORM";
}
