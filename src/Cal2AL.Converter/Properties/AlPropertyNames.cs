namespace Cal2AL.Converter.Properties;

/// <summary>
/// Property names the AL compiler accepts. Used only as a fallback for C/AL properties that have no explicit rule:
/// a known AL name is emitted (with a warning), an unknown one is reported and not emitted.
/// </summary>
public static class AlPropertyNames
{
    private static readonly HashSet<string> Names = new(StringComparer.OrdinalIgnoreCase)
    {
        "Description", "Caption", "Version", "Culture", "PublicKeyToken", "IsControlAddIn", "TableNo", "Permissions",
        "Subtype", "SingleInstance", "TestIsolation", "EventSubscriberInstance", "TestPermissions", "DataClassification",
        "DataPerCompany", "LookupPageId", "DrillDownPageId", "DataCaptionFields", "PasteIsValid", "LinkedObject",
        "LinkedInTransaction", "TableType", "CompressionType", "ExternalName", "ExternalSchema", "Scope", "ReplicateData",
        "AutoFormatType", "AutoFormatExpression", "BlankNumbers", "BlankZero", "SignDisplacement", "MinValue", "MaxValue",
        "NotBlank", "Numeric", "CharAllowed", "DateFormula", "ValuesAllowed", "OptionCaption", "ClosingDates",
        "DecimalPlaces", "AccessByPermission", "ExtendedDatatype", "Width", "CaptionClass", "InitValue", "FieldClass",
        "CalcFormula", "TableRelation", "Enabled", "Editable", "SqlDataType", "ValidateTableRelation", "TestTableRelation",
        "Compressed", "AutoIncrement", "SqlTimestamp", "OptionMembers", "ExternalType", "ExternalAccess",
        "OptionOrdinalValues", "SumIndexFields", "MaintainSqlIndex", "MaintainSiftIndex", "Clustered", "SqlIndex", "Unique",
        "SourceTable", "SourceTableTemporary", "SourceTableView", "CardPageId", "InsertAllowed", "ModifyAllowed",
        "DeleteAllowed", "AutoSplitKey", "SaveValues", "LinksAllowed", "MultipleNewLines", "PopulateAllFields", "ShowFilter",
        "InstructionalText", "DataCaptionExpression", "HelpLink", "ContextSensitiveHelpPage", "PageType", "DelayedInsert",
        "RefreshOnActivate", "PromotedActionCategories", "ODataKeyFields", "APIVersion", "EntityName", "EntitySetName",
        "APIGroup", "APIPublisher", "ChangeTrackingAllowed", "QueryCategory", "AdditionalSearchTerms", "Style",
        "ApplicationArea", "Visible", "StyleExpr", "ToolTip", "RowSpan", "ColumnSpan", "HideValue", "Importance", "MultiLine",
        "ShowCaption", "ShowMandatory", "Title", "QuickEntry", "Lookup", "DrillDown", "AssistEdit", "Image", "ODataEDMType",
        "GridLayout", "CuegroupLayout", "IndentationColumn", "IndentationControls", "FreezeColumn", "ShowAsTree",
        "SubPageView", "SubPageLink", "UpdatePropagation", "Provider", "RunPageMode", "Promoted", "PromotedIsBig",
        "PromotedOnly", "PromotedCategory", "Ellipsis", "ShortcutKey", "RunObject", "RunPageView", "RunPageLink",
        "RunPageOnRec", "InFooterBar", "Gesture", "IsHeader", "Filters", "OrderBy", "TransactionType",
        "DefaultFieldsValidation", "DefaultNamespace", "Direction", "Encoding", "FieldDelimiter", "FieldSeparator", "FileName",
        "Format", "FormatEvaluate", "InlineSchema", "Namespaces", "PreserveWhiteSpace", "RecordSeparator", "TableSeparator",
        "TextEncoding", "UseDefaultNamespace", "UseLax", "UseRequestPage", "XmlVersionNo", "LinkTable", "LinkTableForceInsert",
        "LinkFields", "RequestFilterFields", "RequestFilterHeading", "UseTemporary", "AutoReplace", "AutoSave", "AutoUpdate",
        "CalcFields", "TextType", "AutoCalcField", "FieldValidate", "Occurrence", "MaxOccurs", "MinOccurs", "XmlName",
        "NamespacePrefix", "Unbound", "UseSystemPrinter", "EnableExternalImages", "EnableHyperlinks",
        "EnableExternalAssemblies", "ProcessingOnly", "ShowPrintStatus", "PaperSourceFirstPage", "PaperSourceDefaultPage",
        "PaperSourceLastPage", "DefaultLayout", "WordMergeDataItem", "PdfFontEmbedding", "RDLCLayout", "WordLayout",
        "PreviewMode", "DataItemTableView", "DataItemLinkReference", "DataItemLink", "MaxIteration", "PrintOnlyIfDetail",
        "IncludeCaption", "TopNumberOfRows", "ReadState", "QueryType", "SqlJoinType", "DataItemTableFilter", "ColumnFilter",
        "Method", "ReverseSign", "ObsoleteState", "ObsoleteReason", "UsageCategory", "Extensible", "TreeInitialState",
    };

    public static bool Contains(string name) => Names.Contains(name);
}
