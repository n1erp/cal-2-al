using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;

namespace Cal2AL.Converter.Properties;

/// <summary>How a property value is converted.</summary>
public enum ValueKind
{
    /// <summary>Consumed elsewhere (e.g. part of the element header such as <c>field(1; Name; Type)</c>); not emitted as a property.</summary>
    Consumed,

    /// <summary>No AL equivalent: not emitted, reported with the original value.</summary>
    NotSupported,

    /// <summary><c>Yes</c>/<c>No</c> → <c>true</c>/<c>false</c>.</summary>
    Boolean,

    /// <summary>Boolean constant or a C/AL expression (page properties such as <c>Visible=ShowDetails</c>).</summary>
    BooleanOrExpression,

    /// <summary>Text, written as an AL string literal.</summary>
    Text,

    /// <summary>Written unchanged (numbers, expressions).</summary>
    Raw,

    /// <summary>Enumeration value; spaces, dots, dashes and slashes are removed unless a map entry exists.</summary>
    Enum,

    /// <summary>Multi-language text → AL label syntax plus translation files.</summary>
    Label,

    /// <summary>A single object reference (<c>Page21</c>, <c>Customer Card</c>).</summary>
    ObjectReference,

    /// <summary><c>RunObject</c>: object type keyword plus reference.</summary>
    RunObject,

    TableRelation,
    CalcFormula,
    TableView,

    /// <summary><c>Field=FIELD(x)</c> lists: SubPageLink, RunPageLink, report DataItemLink, XMLport LinkFields.</summary>
    Links,

    /// <summary>Query DataItemLink: <c>Field=DataItem.Field</c>.</summary>
    QueryLink,

    /// <summary>Query <c>ColumnFilter</c>/<c>DataItemTableFilter</c>: <c>Field=FILTER(x)</c> lists.</summary>
    FieldFilters,

    OrderBy,

    /// <summary>Comma-separated identifiers (field lists).</summary>
    IdentifierList,

    /// <summary>A single identifier.</summary>
    Identifier,

    Permissions,
    ApplicationArea,
    OptionMembers,
    InitValue,

    /// <summary>MinValue/MaxValue: typed like InitValue.</summary>
    TypedValue,

    ValuesAllowed,
    Namespaces,
    ApiVersion,
}

/// <summary>Conversion rule for one C/AL property.</summary>
public sealed record PropertyRule(string CalName, string AlName, ValueKind Kind)
{
    /// <summary>Object type expected by ObjectReference rules.</summary>
    public CalObjectType ReferenceType { get; init; } = CalObjectType.Unknown;

    /// <summary>Value renames for Enum rules (case-insensitive keys).</summary>
    public IReadOnlyDictionary<string, string>? EnumMap { get; init; }

    /// <summary>For NotSupported: why, and how severe the loss is.</summary>
    public string? Reason { get; init; }

    public DiagnosticSeverity Severity { get; init; } = DiagnosticSeverity.Warning;
}

/// <summary>The kind of element a property belongs to.</summary>
public enum PropertyScope
{
    Table,
    Field,
    Key,
    FieldGroup,
    Page,
    Control,
    Action,
    Report,
    DataItem,
    Column,
    RequestPage,
    XmlPort,
    XmlPortElement,
    Query,
    QueryDataItem,
    QueryColumn,
    Codeunit,
}

/// <summary>
/// The C/AL → AL property table. Each scope lists the properties C/SIDE can export for that element and what
/// happens to them. Properties not listed fall back to <see cref="AlPropertyNames"/>: if AL knows the name the
/// property is emitted with a generic conversion and a warning, otherwise it is reported as unsupported.
/// </summary>
public static class PropertyRules
{
    private static readonly Dictionary<string, string> CaseInsensitive = new(StringComparer.OrdinalIgnoreCase);

    private static Dictionary<string, string> Map(params (string From, string To)[] entries)
    {
        var d = new Dictionary<string, string>(CaseInsensitive, StringComparer.OrdinalIgnoreCase);
        foreach (var (from, to) in entries)
        {
            d[from] = to;
        }

        return d;
    }

    private static PropertyRule R(string cal, string al, ValueKind kind) => new(cal, al, kind);

    private static PropertyRule Same(string name, ValueKind kind) => new(name, name, kind);

    private static PropertyRule Ref(string cal, string al, CalObjectType type) => new(cal, al, ValueKind.ObjectReference) { ReferenceType = type };

    private static PropertyRule Consumed(string name) => new(name, name, ValueKind.Consumed);

    private static PropertyRule Unsupported(string name, string reason, DiagnosticSeverity severity = DiagnosticSeverity.Warning) =>
        new(name, name, ValueKind.NotSupported) { Reason = reason, Severity = severity };

    private static PropertyRule E(string cal, string al, params (string, string)[] map) =>
        new(cal, al, ValueKind.Enum) { EnumMap = map.Length == 0 ? null : Map(map) };

    /// <summary>Rules valid in every scope.</summary>
    private static readonly PropertyRule[] Common =
    [
        Consumed("Id"),
        Consumed("Name"),
        R("CaptionML", "Caption", ValueKind.Label),
        R("ToolTipML", "ToolTip", ValueKind.Label),
        R("OptionCaptionML", "OptionCaption", ValueKind.Label),
        R("InstructionalTextML", "InstructionalText", ValueKind.Label),
        R("PromotedActionCategoriesML", "PromotedActionCategories", ValueKind.Label),
        R("ReqFilterHeadingML", "RequestFilterHeading", ValueKind.Label),
        R("AdditionalSearchTermsML", "AdditionalSearchTerms", ValueKind.Label),
        Same("Description", ValueKind.Text),
        Same("Permissions", ValueKind.Permissions),
        Same("AccessByPermission", ValueKind.Permissions),
        Same("ApplicationArea", ValueKind.ApplicationArea),
        E("UsageCategory", "UsageCategory"),
        Same("Editable", ValueKind.BooleanOrExpression),
        Same("Enabled", ValueKind.BooleanOrExpression),
        Same("Visible", ValueKind.BooleanOrExpression),
        Same("AutoFormatType", ValueKind.Raw),
        R("AutoFormatExpr", "AutoFormatExpression", ValueKind.Raw),
        Same("CaptionClass", ValueKind.Raw),
        Same("DecimalPlaces", ValueKind.Raw),
        Same("BlankZero", ValueKind.Boolean),
        E("BlankNumbers", "BlankNumbers"),
        Same("Width", ValueKind.Raw),
        Same("NotBlank", ValueKind.Boolean),
        Same("Numeric", ValueKind.Boolean),
        Same("CharAllowed", ValueKind.Text),
        Same("DateFormula", ValueKind.Boolean),
        Same("ClosingDates", ValueKind.Boolean),
        Same("ValuesAllowed", ValueKind.ValuesAllowed),
        Same("SignDisplacement", ValueKind.Raw),
        Same("MinValue", ValueKind.TypedValue),
        Same("MaxValue", ValueKind.TypedValue),
        Same("TableRelation", ValueKind.TableRelation),
        E("ExtendedDatatype", "ExtendedDatatype", ("Phone No.", "PhoneNo"), ("E-Mail", "EMail")),
        R("ObsoleteState", "ObsoleteState", ValueKind.Enum),
        R("ObsoleteReason", "ObsoleteReason", ValueKind.Text),
    ];

    private static readonly PropertyRule[] TableRules =
    [
        Same("DataCaptionFields", ValueKind.IdentifierList),
        Ref("LookupPageID", "LookupPageId", CalObjectType.Page),
        Ref("DrillDownPageID", "DrillDownPageId", CalObjectType.Page),
        Same("PasteIsValid", ValueKind.Boolean),
        Same("LinkedObject", ValueKind.Boolean),
        Same("LinkedInTransaction", ValueKind.Boolean),
        Same("DataPerCompany", ValueKind.Boolean),
        E("TableType", "TableType"),
        Same("ExternalName", ValueKind.Text),
        Same("ExternalSchema", ValueKind.Text),
        E("DataClassification", "DataClassification"),
        Same("ReplicateData", ValueKind.Boolean),
        E("CompressionType", "CompressionType"),
        Unsupported("IncludeDataInDesc", "Obsolete C/SIDE property without effect.", DiagnosticSeverity.Info),
    ];

    private static readonly PropertyRule[] FieldRules =
    [
        Consumed("FieldNo"),
        Consumed("DataType"),
        R("OptionString", "OptionMembers", ValueKind.OptionMembers),
        E("FieldClass", "FieldClass"),
        Same("CalcFormula", ValueKind.CalcFormula),
        Same("InitValue", ValueKind.InitValue),
        Same("ValidateTableRelation", ValueKind.Boolean),
        Same("TestTableRelation", ValueKind.Boolean),
        Same("AutoIncrement", ValueKind.Boolean),
        Same("Compressed", ValueKind.Boolean),
        E("SubType", "Subtype"),
        E("SQL Data Type", "SqlDataType"),
        E("SQLDataType", "SqlDataType"),
        R("SQL Timestamp", "SqlTimestamp", ValueKind.Boolean),
        Same("ExternalName", ValueKind.Text),
        Same("ExternalType", ValueKind.Text),
        E("ExternalAccess", "ExternalAccess"),
        Same("OptionOrdinalValues", ValueKind.Raw),
        E("DataClassification", "DataClassification"),
        Same("Title", ValueKind.Boolean),
        Unsupported("AltSearchField", "AL has no alternative search field; lookups search all fields shown on the page."),
        Unsupported("Volatile", "Obsolete C/SIDE property; it has no effect on SQL Server.", DiagnosticSeverity.Info),
        Unsupported("TableIDExpr", "Not supported in AL table relations; implement the dynamic relation in OnLookup/OnValidate.", DiagnosticSeverity.Warning),
        Consumed("Extensible"),
        Consumed("EnumTypeName"),
        Consumed("EnumTypeId"),
    ];

    private static readonly PropertyRule[] KeyRules =
    [
        Consumed("Key"),
        Consumed("Enabled"),
        Same("SumIndexFields", ValueKind.IdentifierList),
        Same("Clustered", ValueKind.Boolean),
        R("MaintainSQLIndex", "MaintainSqlIndex", ValueKind.Boolean),
        R("MaintainSIFTIndex", "MaintainSiftIndex", ValueKind.Boolean),
        R("SQLIndex", "SqlIndex", ValueKind.IdentifierList),
        Same("Unique", ValueKind.Boolean),
        Unsupported("KeyGroups", "Key groups do not exist in AL; all enabled keys are maintained.", DiagnosticSeverity.Info),
        Unsupported("SIFTLevelsToMaintain", "SIFT levels cannot be configured in AL; SQL Server indexed views are used.", DiagnosticSeverity.Info),
    ];

    private static readonly PropertyRule[] PageRules =
    [
        Ref("SourceTable", "SourceTable", CalObjectType.Table),
        E("PageType", "PageType"),
        Ref("CardPageID", "CardPageId", CalObjectType.Page),
        Same("SourceTableView", ValueKind.TableView),
        Same("SourceTableTemporary", ValueKind.Boolean),
        Same("InsertAllowed", ValueKind.BooleanOrExpression),
        Same("ModifyAllowed", ValueKind.BooleanOrExpression),
        Same("DeleteAllowed", ValueKind.BooleanOrExpression),
        Same("AutoSplitKey", ValueKind.Boolean),
        Same("SaveValues", ValueKind.Boolean),
        Same("LinksAllowed", ValueKind.Boolean),
        Same("MultipleNewLines", ValueKind.Boolean),
        Same("PopulateAllFields", ValueKind.Boolean),
        Same("ShowFilter", ValueKind.Boolean),
        Same("DelayedInsert", ValueKind.Boolean),
        Same("RefreshOnActivate", ValueKind.Boolean),
        R("DataCaptionExpr", "DataCaptionExpression", ValueKind.Raw),
        Same("DataCaptionFields", ValueKind.IdentifierList),
        Same("ODataKeyFields", ValueKind.IdentifierList),
        Same("APIVersion", ValueKind.ApiVersion),
        Same("EntityName", ValueKind.Text),
        Same("EntitySetName", ValueKind.Text),
        Same("APIPublisher", ValueKind.Text),
        Same("APIGroup", ValueKind.Text),
        Same("ChangeTrackingAllowed", ValueKind.Boolean),
        Same("HelpLink", ValueKind.Text),
        Unsupported("TimerInterval", "Timer-driven pages do not exist in AL; use a control add-in or a scheduled job to refresh data."),
        Unsupported("TimerUpdate", "Timer-driven pages do not exist in AL; use a control add-in or a scheduled job to refresh data."),
    ];

    private static readonly PropertyRule[] ControlRules =
    [
        Consumed("SourceExpr"),
        Consumed("ControlType"),
        Consumed("GroupType"),
        Consumed("ContainerType"),
        Consumed("PartType"),
        Consumed("PagePartID"),
        Consumed("SystemPartID"),
        Consumed("ChartPartID"),
        Consumed("ControlAddIn"),
        Consumed("Level"),
        E("Importance", "Importance"),
        Same("QuickEntry", ValueKind.BooleanOrExpression),
        E("Style", "Style"),
        Same("StyleExpr", ValueKind.Raw),
        Same("HideValue", ValueKind.BooleanOrExpression),
        Same("MultiLine", ValueKind.Boolean),
        Same("ShowCaption", ValueKind.Boolean),
        Same("Lookup", ValueKind.Boolean),
        Same("DrillDown", ValueKind.Boolean),
        Same("AssistEdit", ValueKind.Boolean),
        Ref("LookupPageID", "LookupPageId", CalObjectType.Page),
        Ref("DrillDownPageID", "DrillDownPageId", CalObjectType.Page),
        Same("SubPageLink", ValueKind.Links),
        Same("SubPageView", ValueKind.TableView),
        E("UpdatePropagation", "UpdatePropagation"),
        R("ProviderID", "Provider", ValueKind.Identifier),
        R("FreezeColumnID", "FreezeColumn", ValueKind.Identifier),
        R("IndentationColumnName", "IndentationColumn", ValueKind.Raw),
        Same("IndentationControls", ValueKind.IdentifierList),
        Same("ShowAsTree", ValueKind.Boolean),
        E("TreeInitialState", "TreeInitialState"),
        R("Layout", "GridLayout", ValueKind.Enum),
        Same("ColumnSpan", ValueKind.Raw),
        Same("RowSpan", ValueKind.Raw),
        Same("ShowMandatory", ValueKind.BooleanOrExpression),
        Same("Title", ValueKind.Boolean),
        Same("IsHeader", ValueKind.Boolean),
        Same("Image", ValueKind.Identifier),
        Same("ODataEDMType", ValueKind.Text),
        Unsupported("TimerInterval", "Timer-driven controls do not exist in AL."),
    ];

    private static readonly PropertyRule[] ActionRules =
    [
        Consumed("ActionType"),
        Consumed("ActionContainerType"),
        Consumed("Level"),
        Same("Promoted", ValueKind.Boolean),
        Same("PromotedIsBig", ValueKind.Boolean),
        Same("PromotedOnly", ValueKind.Boolean),
        E("PromotedCategory", "PromotedCategory"),
        Same("RunObject", ValueKind.RunObject),
        Same("RunPageLink", ValueKind.Links),
        Same("RunPageView", ValueKind.TableView),
        E("RunPageMode", "RunPageMode"),
        Same("RunPageOnRec", ValueKind.Boolean),
        R("ShortCutKey", "ShortcutKey", ValueKind.Text),
        Same("Ellipsis", ValueKind.Boolean),
        Same("Image", ValueKind.Identifier),
        E("Gesture", "Gesture"),
        Same("InFooterBar", ValueKind.Boolean),
        E("Scope", "Scope"),
        Same("IsHeader", ValueKind.Boolean),
    ];

    private static readonly PropertyRule[] ReportRules =
    [
        Same("ProcessingOnly", ValueKind.Boolean),
        Same("UseRequestPage", ValueKind.Boolean),
        Same("ShowPrintStatus", ValueKind.Boolean),
        E("TransactionType", "TransactionType"),
        Same("UseSystemPrinter", ValueKind.Boolean),
        Same("EnableHyperlinks", ValueKind.Boolean),
        Same("EnableExternalImages", ValueKind.Boolean),
        Same("EnableExternalAssemblies", ValueKind.Boolean),
        E("PreviewMode", "PreviewMode"),
        E("PaperSourceDefaultPage", "PaperSourceDefaultPage"),
        E("PaperSourceFirstPage", "PaperSourceFirstPage"),
        E("PaperSourceLastPage", "PaperSourceLastPage"),
        E("DefaultLayout", "DefaultLayout"),
        Same("WordMergeDataItem", ValueKind.Identifier),
        E("PDFFontEmbedding", "PdfFontEmbedding"),
    ];

    private static readonly PropertyRule[] DataItemRules =
    [
        Consumed("DataItemTable"),
        Consumed("DataItemVarName"),
        Consumed("Indentation"),
        Same("DataItemTableView", ValueKind.TableView),
        Same("DataItemLink", ValueKind.Links),
        Same("DataItemLinkReference", ValueKind.Identifier),
        R("ReqFilterFields", "RequestFilterFields", ValueKind.IdentifierList),
        Same("CalcFields", ValueKind.IdentifierList),
        Same("PrintOnlyIfDetail", ValueKind.Boolean),
        Same("MaxIteration", ValueKind.Raw),
        R("Temporary", "UseTemporary", ValueKind.Boolean),
    ];

    private static readonly PropertyRule[] ColumnRules =
    [
        Consumed("SourceExpr"),
        Consumed("Indentation"),
        Same("IncludeCaption", ValueKind.Boolean),
        R("OptionString", "OptionMembers", ValueKind.OptionMembers),
    ];

    private static readonly PropertyRule[] XmlPortRules =
    [
        E("Direction", "Direction"),
        E("Encoding", "Encoding", ("UTF-8", "UTF8"), ("UTF-16", "UTF16"), ("ISO-8859-2", "ISO88592")),
        E("Format", "Format", ("Xml", "Xml"), ("Variable Text", "VariableText"), ("Fixed Text", "FixedText")),
        Same("FieldDelimiter", ValueKind.Text),
        Same("FieldSeparator", ValueKind.Text),
        Same("RecordSeparator", ValueKind.Text),
        Same("TableSeparator", ValueKind.Text),
        Same("UseRequestPage", ValueKind.Boolean),
        Same("DefaultFieldsValidation", ValueKind.Boolean),
        E("FormatEvaluate", "FormatEvaluate", ("C/SIDE Format/Evaluate", "Legacy"), ("XML Format/Evaluate", "Xml")),
        Same("DefaultNamespace", ValueKind.Text),
        Same("UseDefaultNamespace", ValueKind.Boolean),
        Same("Namespaces", ValueKind.Namespaces),
        Same("InlineSchema", ValueKind.Boolean),
        E("TransactionType", "TransactionType"),
        Same("PreserveWhiteSpace", ValueKind.Boolean),
        Same("FileName", ValueKind.Text),
        E("TextEncoding", "TextEncoding", ("MS-DOS", "MSDos"), ("UTF-8", "UTF8"), ("UTF-16", "UTF16"), ("Windows", "Windows")),
        Same("UseLax", ValueKind.Boolean),
        E("XMLVersionNo", "XmlVersionNo", ("1.0", "V10"), ("1.1", "V11")),
    ];

    private static readonly PropertyRule[] XmlPortElementRules =
    [
        Consumed("SourceTable"),
        Consumed("VariableName"),
        Consumed("SourceField"),
        Consumed("NodeType"),
        Consumed("SourceType"),
        Consumed("Indentation"),
        Consumed("ElementId"),
        Consumed("DataType"),
        E("MinOccurs", "MinOccurs"),
        E("MaxOccurs", "MaxOccurs"),
        E("Occurrence", "Occurrence"),
        Same("LinkTable", ValueKind.Identifier),
        Same("LinkFields", ValueKind.Links),
        Same("LinkTableForceInsert", ValueKind.Boolean),
        Same("AutoSave", ValueKind.Boolean),
        Same("AutoUpdate", ValueKind.Boolean),
        Same("AutoReplace", ValueKind.Boolean),
        Same("SourceTableView", ValueKind.TableView),
        Same("CalcFields", ValueKind.IdentifierList),
        R("ReqFilterFields", "RequestFilterFields", ValueKind.IdentifierList),
        R("Temporary", "UseTemporary", ValueKind.Boolean),
        E("FieldValidate", "FieldValidate", ("TRUE", "Yes"), ("FALSE", "No")),
        Same("Width", ValueKind.Raw),
        Same("Unbound", ValueKind.Boolean),
        Same("NamespacePrefix", ValueKind.Text),
        E("TextType", "TextType"),
        Same("Format", ValueKind.Text),
        Same("AutoCalcField", ValueKind.Boolean),
    ];

    private static readonly PropertyRule[] QueryRules =
    [
        Same("OrderBy", ValueKind.OrderBy),
        Same("TopNumberOfRows", ValueKind.Raw),
        E("ReadState", "ReadState"),
        E("QueryType", "QueryType"),
        Same("EntityName", ValueKind.Text),
        Same("EntitySetName", ValueKind.Text),
        Same("APIPublisher", ValueKind.Text),
        Same("APIGroup", ValueKind.Text),
        Same("APIVersion", ValueKind.ApiVersion),
    ];

    private static readonly PropertyRule[] QueryDataItemRules =
    [
        Consumed("DataItemTable"),
        Consumed("QueryElementType"),
        Consumed("Level"),
        Same("DataItemLink", ValueKind.QueryLink),
        E("SQLJoinType", "SqlJoinType", ("Left Outer Join", "LeftOuterJoin"), ("Inner Join", "InnerJoin"), ("Right Outer Join", "RightOuterJoin"), ("Full Outer Join", "FullOuterJoin"), ("Cross Join", "CrossJoin")),
        E("DataItemLinkType", "SqlJoinType", ("Use Default Values if No Match", "LeftOuterJoin"), ("Exclude Row If No Match", "InnerJoin"), ("SQL Advanced Options", "LeftOuterJoin")),
        Same("DataItemTableFilter", ValueKind.FieldFilters),
    ];

    private static readonly PropertyRule[] QueryColumnRules =
    [
        Consumed("DataSource"),
        Consumed("QueryElementType"),
        Consumed("Level"),
        Consumed("MethodType"),
        E("Method", "Method"),
        Same("ReverseSign", ValueKind.Boolean),
        Same("ColumnFilter", ValueKind.FieldFilters),
    ];

    private static readonly PropertyRule[] CodeunitRules =
    [
        E("Subtype", "Subtype"),
        Ref("TableNo", "TableNo", CalObjectType.Table),
        Same("SingleInstance", ValueKind.Boolean),
        E("EventSubscriberInstance", "EventSubscriberInstance", ("Static-Automatic", "StaticAutomatic")),
        E("TestIsolation", "TestIsolation"),
        E("TestPermissions", "TestPermissions"),
        Unsupported("CFRONTMayUsePermissions", "C/FRONT is not available in Business Central.", DiagnosticSeverity.Info),
    ];

    private static readonly Dictionary<PropertyScope, Dictionary<string, PropertyRule>> Tables = Build();

    private static Dictionary<PropertyScope, Dictionary<string, PropertyRule>> Build()
    {
        var result = new Dictionary<PropertyScope, Dictionary<string, PropertyRule>>();
        void Add(PropertyScope scope, params PropertyRule[][] sets)
        {
            var d = new Dictionary<string, PropertyRule>(StringComparer.OrdinalIgnoreCase);
            foreach (var rule in Common)
            {
                d[rule.CalName] = rule;
            }

            foreach (var set in sets)
            {
                foreach (var rule in set)
                {
                    d[rule.CalName] = rule;
                }
            }

            result[scope] = d;
        }

        Add(PropertyScope.Table, TableRules);
        Add(PropertyScope.Field, FieldRules);
        Add(PropertyScope.Key, KeyRules);
        Add(PropertyScope.FieldGroup, [Consumed("Fields")]);
        Add(PropertyScope.Page, PageRules);
        Add(PropertyScope.RequestPage, PageRules);
        Add(PropertyScope.Control, ControlRules);
        Add(PropertyScope.Action, ActionRules);
        Add(PropertyScope.Report, ReportRules);
        Add(PropertyScope.DataItem, DataItemRules);
        Add(PropertyScope.Column, ColumnRules);
        Add(PropertyScope.XmlPort, XmlPortRules);
        Add(PropertyScope.XmlPortElement, XmlPortElementRules);
        Add(PropertyScope.Query, QueryRules);
        Add(PropertyScope.QueryDataItem, QueryDataItemRules);
        Add(PropertyScope.QueryColumn, QueryColumnRules);
        Add(PropertyScope.Codeunit, CodeunitRules);
        return result;
    }

    public static PropertyRule? Find(PropertyScope scope, string calName) =>
        Tables[scope].GetValueOrDefault(calName);

    /// <summary>All rules of a scope, for documentation.</summary>
    public static IReadOnlyCollection<PropertyRule> GetRules(PropertyScope scope) => Tables[scope].Values;
}
