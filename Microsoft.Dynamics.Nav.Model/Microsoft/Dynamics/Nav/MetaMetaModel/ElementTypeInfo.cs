using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaMetaModel;

public class ElementTypeInfo
{
	private static ElementTypeLookup lookup;

	private CategoryType category;

	private int categoryIndex;

	private ElementType elementType;

	private string name;

	private ModelType model;

	private PropertyType idProperty;

	private PropertyType nameProperty;

	private PropertyType typeProperty;

	private ElementPropertyLookup properties;

	private ElementRelationLookup relations;

	private static readonly ElementRelation[] EmptyElementRelations = new ElementRelation[0];

	public CategoryType Category => category;

	public int CategoryIndex => categoryIndex;

	public ElementType ElementType => elementType;

	public string Name
	{
		get
		{
			if (name.Length == 0)
			{
				name = EnumNameLookup<ElementType>.EnumToString(elementType);
			}
			return name;
		}
	}

	public ModelType Model => model;

	public PropertyType IdProperty => idProperty;

	public PropertyType NameProperty => nameProperty;

	public PropertyType TypeProperty => typeProperty;

	public bool HasCodeLines
	{
		get
		{
			if (category != CategoryType.Method)
			{
				return category == CategoryType.Trigger;
			}
			return true;
		}
	}

	public static ElementTypeLookup Lookup
	{
		get
		{
			if (lookup == null)
			{
				lookup = new ElementTypeLookup(GetDefinitions());
				(from et in lookup.Values
					group et by et.Category).ForEach((IGrouping<CategoryType, ElementTypeInfo> eg) =>
				{
					int index = 0;
					eg.ForEach((ElementTypeInfo e) =>
					{
						e.categoryIndex = index++;
					});
				});
			}
			return lookup;
		}
	}

	public ElementPropertyLookup Properties => properties;

	public ElementRelationLookup Relations => relations ?? (relations = new ElementRelationLookup(this, EmptyElementRelations));

	private static IEnumerable<ElementTypeInfo> GetDefinitions()
	{
		return new ElementTypeInfo[117]
		{
			new ElementTypeInfo(CategoryType.Unknown, ElementType.Unknown),
			new ElementTypeInfo(CategoryType.Object, ElementType.Modification)
			{
				model = ModelType.DevBase,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.ChangeType, PropertyType.Date, PropertyType.Id, PropertyType.Modified, PropertyType.Name, PropertyType.ResidualHint, PropertyType.Target, PropertyType.TargetPageType, PropertyType.Time, PropertyType.VersionList).DefineRelations(new ElementRelation(ElementType.Modification, ElementType.Change, MultiplicityType.ZeroToMany, "CHANGES")),
			new ElementTypeInfo(CategoryType.Element, ElementType.Change)
			{
				model = ModelType.DevBase,
				typeProperty = PropertyType.ChangeType
			}.DefineProperties(PropertyType.AddedChildren, PropertyType.BaselineValue, PropertyType.ChangeType, PropertyType.ConflictHint, PropertyType.IncludeSiblings, PropertyType.InsertAfter, PropertyType.ModifiedValue, PropertyType.NewBaselineValue, PropertyType.OriginalValue, PropertyType.Property, PropertyType.ResidualHint, PropertyType.ResultingValue, PropertyType.Target, PropertyType.TargetValue).DefineRelations(new ElementRelation(ElementType.Change, ElementType.Action, MultiplicityType.ZeroToMany, "ActionCollection"), new ElementRelation(ElementType.Change, ElementType.BaselineCode), new ElementRelation(ElementType.Change, ElementType.Change, MultiplicityType.ZeroToMany, "ChangeCollection"), new ElementRelation(ElementType.Change, ElementType.Control, MultiplicityType.ZeroToMany, "ControlCollection"), new ElementRelation(ElementType.Change, ElementType.DataItem, MultiplicityType.ZeroToMany, "DataItemCollection"), new ElementRelation(ElementType.Change, ElementType.DataportField, MultiplicityType.ZeroToMany, "DataportFieldCollection"), new ElementRelation(ElementType.Change, ElementType.DataportItem, MultiplicityType.ZeroToMany, "DataportItemCollection"), new ElementRelation(ElementType.Change, ElementType.Documentation), new ElementRelation(ElementType.Change, ElementType.Event, MultiplicityType.ZeroToMany, "EVENTCollection"), new ElementRelation(ElementType.Change, ElementType.ExportOnAfterGetRecord), new ElementRelation(ElementType.Change, ElementType.ExportOnBeforePassField), new ElementRelation(ElementType.Change, ElementType.ExportOnBeforePassVariable), new ElementRelation(ElementType.Change, ElementType.ExportOnPreXmlItem), new ElementRelation(ElementType.Change, ElementType.Field, MultiplicityType.ZeroToMany, "FieldCollection"), new ElementRelation(ElementType.Change, ElementType.FieldGroup, MultiplicityType.ZeroToMany, "FieldGroupCollection"), new ElementRelation(ElementType.Change, ElementType.FormControl, MultiplicityType.ZeroToMany, "FormControlCollection"), new ElementRelation(ElementType.Change, ElementType.ImportOnAfterAssignField), new ElementRelation(ElementType.Change, ElementType.ImportOnAfterAssignVariable), new ElementRelation(ElementType.Change, ElementType.ImportOnAfterInitRecord), new ElementRelation(ElementType.Change, ElementType.ImportOnAfterInsertRecord), new ElementRelation(ElementType.Change, ElementType.ImportOnAfterModifyRecord), new ElementRelation(ElementType.Change, ElementType.ImportOnBeforeInsertRecord), new ElementRelation(ElementType.Change, ElementType.ImportOnBeforeModifyRecord), new ElementRelation(ElementType.Change, ElementType.Item, MultiplicityType.ZeroToMany, "ItemCollection"), new ElementRelation(ElementType.Change, ElementType.Key, MultiplicityType.ZeroToMany, "KeyCollection"), new ElementRelation(ElementType.Change, ElementType.Label, MultiplicityType.ZeroToMany, "LabelCollection"), new ElementRelation(ElementType.Change, ElementType.MenuItem, MultiplicityType.ZeroToMany, "MenuItemCollection"), new ElementRelation(ElementType.Change, ElementType.MenuNode, MultiplicityType.ZeroToMany, "MenuNodeCollection"), new ElementRelation(ElementType.Change, ElementType.ModifiedCode), new ElementRelation(ElementType.Change, ElementType.NewBaselineCode), new ElementRelation(ElementType.Change, ElementType.OnAction), new ElementRelation(ElementType.Change, ElementType.OnActivate), new ElementRelation(ElementType.Change, ElementType.OnActivateForm), new ElementRelation(ElementType.Change, ElementType.OnAddWatch), new ElementRelation(ElementType.Change, ElementType.OnAfterGetCurrRecord), new ElementRelation(ElementType.Change, ElementType.OnAfterGetRecord), new ElementRelation(ElementType.Change, ElementType.OnAfterImportRecord), new ElementRelation(ElementType.Change, ElementType.OnAfterInput), new ElementRelation(ElementType.Change, ElementType.OnAfterValidate), new ElementRelation(ElementType.Change, ElementType.OnAssistEdit), new ElementRelation(ElementType.Change, ElementType.OnBeforeExportRecord), new ElementRelation(ElementType.Change, ElementType.OnBeforeImportRecord), new ElementRelation(ElementType.Change, ElementType.OnBeforeInput), new ElementRelation(ElementType.Change, ElementType.OnBeforeOpen), new ElementRelation(ElementType.Change, ElementType.OnBeforePutRecord), new ElementRelation(ElementType.Change, ElementType.OnCloseForm), new ElementRelation(ElementType.Change, ElementType.OnClosePage), new ElementRelation(ElementType.Change, ElementType.OnControlAddIn), new ElementRelation(ElementType.Change, ElementType.OnDataPointClicked), new ElementRelation(ElementType.Change, ElementType.OnDataPointDoubleClicked), new ElementRelation(ElementType.Change, ElementType.OnDeactivate), new ElementRelation(ElementType.Change, ElementType.OnDeactivateForm), new ElementRelation(ElementType.Change, ElementType.OnDelete), new ElementRelation(ElementType.Change, ElementType.OnDeleteRecord), new ElementRelation(ElementType.Change, ElementType.OnDrillDown), new ElementRelation(ElementType.Change, ElementType.OnFindRecord), new ElementRelation(ElementType.Change, ElementType.OnFormat), new ElementRelation(ElementType.Change, ElementType.OnGetVariableValue), new ElementRelation(ElementType.Change, ElementType.OnInit), new ElementRelation(ElementType.Change, ElementType.OnInitReport), new ElementRelation(ElementType.Change, ElementType.OnInitXmlPort), new ElementRelation(ElementType.Change, ElementType.OnInsert), new ElementRelation(ElementType.Change, ElementType.OnInsertRecord), new ElementRelation(ElementType.Change, ElementType.OnLookup), new ElementRelation(ElementType.Change, ElementType.OnModify), new ElementRelation(ElementType.Change, ElementType.OnModifyRecord), new ElementRelation(ElementType.Change, ElementType.OnNewRecord), new ElementRelation(ElementType.Change, ElementType.OnNextRecord), new ElementRelation(ElementType.Change, ElementType.OnOpenForm), new ElementRelation(ElementType.Change, ElementType.OnOpenPage), new ElementRelation(ElementType.Change, ElementType.OnPostDataItem), new ElementRelation(ElementType.Change, ElementType.OnPostReport), new ElementRelation(ElementType.Change, ElementType.OnPostSection), new ElementRelation(ElementType.Change, ElementType.OnPostXmlPort), new ElementRelation(ElementType.Change, ElementType.OnPreDataItem), new ElementRelation(ElementType.Change, ElementType.OnPreDataport), new ElementRelation(ElementType.Change, ElementType.OnPreReport), new ElementRelation(ElementType.Change, ElementType.OnPreSection), new ElementRelation(ElementType.Change, ElementType.OnPreXmlPort), new ElementRelation(ElementType.Change, ElementType.OnPush), new ElementRelation(ElementType.Change, ElementType.OnQueryCloseForm), new ElementRelation(ElementType.Change, ElementType.OnQueryClosePage), new ElementRelation(ElementType.Change, ElementType.OnRename), new ElementRelation(ElementType.Change, ElementType.OnRun), new ElementRelation(ElementType.Change, ElementType.OnSetBreakpoint), new ElementRelation(ElementType.Change, ElementType.OnTimer), new ElementRelation(ElementType.Change, ElementType.OnValidate), new ElementRelation(ElementType.Change, ElementType.OriginalCode), new ElementRelation(ElementType.Change, ElementType.Parameter, MultiplicityType.ZeroToMany, "ParameterCollection"), new ElementRelation(ElementType.Change, ElementType.Procedure, MultiplicityType.ZeroToMany, "PROCEDURECollection"), new ElementRelation(ElementType.Change, ElementType.QueryElement, MultiplicityType.ZeroToMany, "QueryElementCollection"), new ElementRelation(ElementType.Change, ElementType.Reference, MultiplicityType.ZeroToMany, "ReferenceCollection"), new ElementRelation(ElementType.Change, ElementType.ReportControl, MultiplicityType.ZeroToMany, "ReportControlCollection"), new ElementRelation(ElementType.Change, ElementType.ReportItem, MultiplicityType.ZeroToMany, "ReportItemCollection"), new ElementRelation(ElementType.Change, ElementType.ReportSection, MultiplicityType.ZeroToMany, "ReportSectionCollection"), new ElementRelation(ElementType.Change, ElementType.RequestForm, MultiplicityType.ZeroToOne, "RequestForm"), new ElementRelation(ElementType.Change, ElementType.RequestPage, MultiplicityType.ZeroToOne, "RequestPage"), new ElementRelation(ElementType.Change, ElementType.ResultingCode), new ElementRelation(ElementType.Change, ElementType.ReturnValue, MultiplicityType.ZeroToOne, "ReturnValueCollection"), new ElementRelation(ElementType.Change, ElementType.TargetCode), new ElementRelation(ElementType.Change, ElementType.Variable, MultiplicityType.ZeroToMany, "VariableCollection"), new ElementRelation(ElementType.Change, ElementType.XmlPortElement, MultiplicityType.ZeroToMany, "XmlPortElementCollection"), new ElementRelation(ElementType.Change, ElementType.XmlPortEvent, MultiplicityType.ZeroToMany, "XmlPortEventCollection")),
			new ElementTypeInfo(CategoryType.Identifier, ElementType.Variable)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.DataType, PropertyType.Dimensions, PropertyType.ExternalDataType, PropertyType.Id, PropertyType.InDataSet, PropertyType.Length, PropertyType.Name, PropertyType.NonDebuggable, PropertyType.OptionString, PropertyType.RunOnClient, PropertyType.SecurityFiltering, PropertyType.Subtype, PropertyType.SuppressDispose, PropertyType.Temporary, PropertyType.TextConstString, PropertyType.WithEvents),
			new ElementTypeInfo(CategoryType.Method, ElementType.Procedure)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.Attributes, PropertyType.EventFunction, PropertyType.EventPublishedObject, PropertyType.EventType, PropertyType.FunctionVisibility, PropertyType.Id, PropertyType.LineStart, PropertyType.Local, PropertyType.Name, PropertyType.NonDebuggable, PropertyType.ServiceEnabled, PropertyType.TestPermissions, PropertyType.TryFunction).DefineRelations(new ElementRelation(ElementType.Procedure, ElementType.Parameter, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Procedure, ElementType.ReturnValue), new ElementRelation(ElementType.Procedure, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Identifier, ElementType.Parameter)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.AsVar, PropertyType.DataType, PropertyType.Dimensions, PropertyType.ExternalDataType, PropertyType.Id, PropertyType.Length, PropertyType.Name, PropertyType.OptionString, PropertyType.RunOnClient, PropertyType.SecurityFiltering, PropertyType.Subtype, PropertyType.SuppressDispose, PropertyType.Temporary, PropertyType.TextConstString),
			new ElementTypeInfo(CategoryType.Identifier, ElementType.ReturnValue)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.DataType, PropertyType.Dimensions, PropertyType.Id, PropertyType.Length, PropertyType.Name),
			new ElementTypeInfo(CategoryType.Method, ElementType.Event)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.EventSource, PropertyType.EventSourceId, PropertyType.Id, PropertyType.Name).DefineRelations(new ElementRelation(ElementType.Event, ElementType.Parameter, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Event, ElementType.ReturnValue), new ElementRelation(ElementType.Event, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.BaselineCode)
			{
				model = ModelType.DevBase
			}.DefineProperties(),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.ResultingCode)
			{
				model = ModelType.DevBase
			}.DefineProperties(),
			new ElementTypeInfo(CategoryType.Object, ElementType.Report)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.AccessByPermission, PropertyType.AdditionalSearchTermsML, PropertyType.ApplicationArea, PropertyType.BottomMargin, PropertyType.CaptionML, PropertyType.Date, PropertyType.DefaultLayout, PropertyType.Description, PropertyType.DeviceFontName, PropertyType.EnableExternalAssemblies, PropertyType.EnableExternalImages, PropertyType.EnableHyperlinks, PropertyType.HorizontalGrid, PropertyType.Id, PropertyType.LeftMargin, PropertyType.Modified, PropertyType.Name, PropertyType.Orientation, PropertyType.PaperSize, PropertyType.PaperSourceDefaultPage, PropertyType.PaperSourceFirstPage, PropertyType.PaperSourceLastPage, PropertyType.PDFFontEmbedding, PropertyType.Permissions, PropertyType.PreviewMode, PropertyType.ProcessingOnly, PropertyType.RdlData, PropertyType.RightMargin, PropertyType.ShowPrintStatus, PropertyType.Time, PropertyType.TopMargin, PropertyType.TransactionType, PropertyType.UsageCategory, PropertyType.UseReqForm, PropertyType.UseRequestPage, PropertyType.UseSystemPrinter, PropertyType.VersionList, PropertyType.WordLayout, PropertyType.WordMergeDataItem).DefineRelations(new ElementRelation(ElementType.Report, ElementType.DataItem, MultiplicityType.ZeroToMany, "DATASET"), new ElementRelation(ElementType.Report, ElementType.Documentation), new ElementRelation(ElementType.Report, ElementType.Event, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Report, ElementType.Label, MultiplicityType.ZeroToMany, "LABELS"), new ElementRelation(ElementType.Report, ElementType.OnCreateHyperlink), new ElementRelation(ElementType.Report, ElementType.OnHyperlink), new ElementRelation(ElementType.Report, ElementType.OnInitReport, MultiplicityType.ZeroToOne, null, 18512), new ElementRelation(ElementType.Report, ElementType.OnPostReport, MultiplicityType.ZeroToOne, null, 18514), new ElementRelation(ElementType.Report, ElementType.OnPreReport, MultiplicityType.ZeroToOne, null, 18513), new ElementRelation(ElementType.Report, ElementType.Procedure, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Report, ElementType.ReportItem, MultiplicityType.ZeroToMany, "DATAITEMS"), new ElementRelation(ElementType.Report, ElementType.RequestForm, MultiplicityType.ZeroToOne, "REQUESTFORM"), new ElementRelation(ElementType.Report, ElementType.RequestPage, MultiplicityType.ZeroToOne, "REQUESTPAGE"), new ElementRelation(ElementType.Report, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Object, ElementType.MenuSuite)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.Date, PropertyType.Id, PropertyType.Modified, PropertyType.Name, PropertyType.Time, PropertyType.VersionList).DefineRelations(new ElementRelation(ElementType.MenuSuite, ElementType.MenuNode, MultiplicityType.ZeroToMany, "MENUNODES")),
			new ElementTypeInfo(CategoryType.Object, ElementType.XmlPort)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.CaptionML, PropertyType.Date, PropertyType.DefaultFieldsValidation, PropertyType.DefaultNamespace, PropertyType.Direction, PropertyType.Encoding, PropertyType.FieldDelimiter, PropertyType.FieldSeparator, PropertyType.FileName, PropertyType.Format, PropertyType.FormatEvaluate, PropertyType.Id, PropertyType.InlineSchema, PropertyType.Modified, PropertyType.Name, PropertyType.Namespaces, PropertyType.Permissions, PropertyType.PreserveWhiteSpace, PropertyType.PreviewMode, PropertyType.ReadState, PropertyType.RecordSeparator, PropertyType.TableSeparator, PropertyType.TextEncoding, PropertyType.Time, PropertyType.TransactionType, PropertyType.UseDefaultNamespace, PropertyType.UseLax, PropertyType.UseRequestForm, PropertyType.UseRequestPage, PropertyType.VersionList, PropertyType.XmlVersionNo).DefineRelations(new ElementRelation(ElementType.XmlPort, ElementType.Documentation), new ElementRelation(ElementType.XmlPort, ElementType.Event, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.XmlPort, ElementType.OnInitXmlPort, MultiplicityType.ZeroToOne, null, 54116), new ElementRelation(ElementType.XmlPort, ElementType.OnPostXmlPort, MultiplicityType.ZeroToOne, null, 54118), new ElementRelation(ElementType.XmlPort, ElementType.OnPreXmlPort, MultiplicityType.ZeroToOne, null, 54117), new ElementRelation(ElementType.XmlPort, ElementType.Procedure, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.XmlPort, ElementType.RequestPage, MultiplicityType.ZeroToOne, "REQUESTPAGE"), new ElementRelation(ElementType.XmlPort, ElementType.Variable, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.XmlPort, ElementType.XmlPortElement, MultiplicityType.ZeroToMany, "ELEMENTS"), new ElementRelation(ElementType.XmlPort, ElementType.XmlPortEvent, MultiplicityType.ZeroToMany, "EVENTS")),
			new ElementTypeInfo(CategoryType.Element, ElementType.ReportSection)
			{
				typeProperty = PropertyType.SectionType
			}.DefineProperties(PropertyType.KeepWithNext, PropertyType.PlaceInBottom, PropertyType.PrintOnEveryPage, PropertyType.SectionHeight, PropertyType.SectionType, PropertyType.SectionWidth).DefineRelations(new ElementRelation(ElementType.ReportSection, ElementType.OnPostSection), new ElementRelation(ElementType.ReportSection, ElementType.OnPreSection), new ElementRelation(ElementType.ReportSection, ElementType.ReportControl, MultiplicityType.ZeroToMany, "CONTROLS")),
			new ElementTypeInfo(CategoryType.Element, ElementType.ReportItem)
			{
				nameProperty = PropertyType.DataItemVarName
			}.DefineProperties(PropertyType.CalcFields, PropertyType.DataItemIndent, PropertyType.DataItemLink, PropertyType.DataItemLinkReference, PropertyType.DataItemTable, PropertyType.DataItemTableView, PropertyType.DataItemVarName, PropertyType.GroupTotalFields, PropertyType.MaxIteration, PropertyType.NewPagePerGroup, PropertyType.NewPagePerRecord, PropertyType.PrintOnlyIfDetail, PropertyType.RequestFilterFields, PropertyType.RequestFilterHeadingML, PropertyType.TotalFields).DefineRelations(new ElementRelation(ElementType.ReportItem, ElementType.OnAfterGetRecord), new ElementRelation(ElementType.ReportItem, ElementType.OnPostDataItem), new ElementRelation(ElementType.ReportItem, ElementType.OnPreDataItem), new ElementRelation(ElementType.ReportItem, ElementType.ReportSection, MultiplicityType.ZeroToMany, "SECTIONS")),
			new ElementTypeInfo(CategoryType.Element, ElementType.ReportControl)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name,
				typeProperty = PropertyType.ControlType
			}.DefineProperties(PropertyType.AutoCalcField, PropertyType.AutoFormatExpression, PropertyType.AutoFormatType, PropertyType.BlankNumbers, PropertyType.BlankZero, PropertyType.Border, PropertyType.BorderWidth, PropertyType.CaptionClass, PropertyType.CaptionML, PropertyType.ControlHeight, PropertyType.ControlType, PropertyType.ControlWidth, PropertyType.DataSetFieldName, PropertyType.DecimalPlaces, PropertyType.Divisor, PropertyType.Enabled, PropertyType.FontBold, PropertyType.FontItalic, PropertyType.FontName, PropertyType.FontSize, PropertyType.FontUnderline, PropertyType.ForeColor, PropertyType.Format, PropertyType.HorizontalAlign, PropertyType.HorizontalGlue, PropertyType.Id, PropertyType.LeaderDots, PropertyType.Multiline, PropertyType.Name, PropertyType.OptionCaptionML, PropertyType.OptionString, PropertyType.PadChar, PropertyType.ParentControl, PropertyType.PasswordText, PropertyType.ShapeStyle, PropertyType.SourceExpression, PropertyType.VerticalAlign, PropertyType.Visible, PropertyType.X, PropertyType.Y),
			new ElementTypeInfo(CategoryType.Object, ElementType.Package)
			{
				model = ModelType.DevBase,
				nameProperty = PropertyType.Name,
				typeProperty = PropertyType.PackageType
			}.DefineProperties(PropertyType.Date, PropertyType.ExplicitItems, PropertyType.Id, PropertyType.Label, PropertyType.Modified, PropertyType.Name, PropertyType.Namespace, PropertyType.PackageType, PropertyType.ReleaseDate, PropertyType.Time, PropertyType.Version, PropertyType.VersionList).DefineRelations(new ElementRelation(ElementType.Package, ElementType.Item, MultiplicityType.ZeroToMany, "ITEMS"), new ElementRelation(ElementType.Package, ElementType.Reference, MultiplicityType.ZeroToMany, "REFERENCES")),
			new ElementTypeInfo(CategoryType.Element, ElementType.Item)
			{
				model = ModelType.DevBase,
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.Id, PropertyType.Name, PropertyType.ObjectType),
			new ElementTypeInfo(CategoryType.Element, ElementType.Reference)
			{
				model = ModelType.DevBase,
				nameProperty = PropertyType.Reference
			}.DefineProperties(PropertyType.MaxCompatibleVersion, PropertyType.MinCompatibleVersion, PropertyType.Reference),
			new ElementTypeInfo(CategoryType.Object, ElementType.Codeunit)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name,
				typeProperty = PropertyType.CodeunitSubtype
			}.DefineProperties(PropertyType.CFRONTMayUsePermissions, PropertyType.CodeunitSubtype, PropertyType.Date, PropertyType.EventSubscriberInstance, PropertyType.Id, PropertyType.Modified, PropertyType.Name, PropertyType.Permissions, PropertyType.SingleInstance, PropertyType.TableNo, PropertyType.TestIsolation, PropertyType.TestPermissions, PropertyType.Time, PropertyType.VersionList).DefineRelations(new ElementRelation(ElementType.Codeunit, ElementType.Documentation), new ElementRelation(ElementType.Codeunit, ElementType.Event, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Codeunit, ElementType.OnRun, MultiplicityType.ZeroToOne, null, 26369), new ElementRelation(ElementType.Codeunit, ElementType.Procedure, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Codeunit, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Object, ElementType.Table)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.CaptionML, PropertyType.CompressionType, PropertyType.DataCaptionFields, PropertyType.DataClassification, PropertyType.DataPerCompany, PropertyType.Date, PropertyType.Description, PropertyType.DrillDownFormId, PropertyType.DrillDownPageId, PropertyType.ExternalName, PropertyType.ExternalSchema, PropertyType.Id, PropertyType.IncludeDataInDesc, PropertyType.LinkedInTransaction, PropertyType.LinkedObject, PropertyType.LookupFormId, PropertyType.LookupPageId, PropertyType.Modified, PropertyType.Name, PropertyType.ObsoleteReason, PropertyType.ObsoleteState, PropertyType.PasteIsValid, PropertyType.Permissions, PropertyType.ReplicateData, PropertyType.TableType, PropertyType.Time, PropertyType.TrackingObject, PropertyType.VersionList).DefineRelations(new ElementRelation(ElementType.Table, ElementType.Documentation), new ElementRelation(ElementType.Table, ElementType.Event, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Table, ElementType.Field, MultiplicityType.ZeroToMany, "FIELDS"), new ElementRelation(ElementType.Table, ElementType.FieldGroup, MultiplicityType.ZeroToMany, "FIELDGROUPS"), new ElementRelation(ElementType.Table, ElementType.Key, MultiplicityType.ZeroToMany, "KEYS"), new ElementRelation(ElementType.Table, ElementType.OnDelete, MultiplicityType.ZeroToOne, null, 4960), new ElementRelation(ElementType.Table, ElementType.OnInsert, MultiplicityType.ZeroToOne, null, 4958), new ElementRelation(ElementType.Table, ElementType.OnModify, MultiplicityType.ZeroToOne, null, 4959), new ElementRelation(ElementType.Table, ElementType.OnRename, MultiplicityType.ZeroToOne, null, 4961), new ElementRelation(ElementType.Table, ElementType.Procedure, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Table, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Object, ElementType.Page)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name,
				typeProperty = PropertyType.PageType
			}.DefineProperties(PropertyType.AccessByPermission, PropertyType.ActionList, PropertyType.AdditionalSearchTermsML, PropertyType.APIGroup, PropertyType.APIPublisher, PropertyType.APIVersion, PropertyType.ApplicationArea, PropertyType.AutoSplitKey, PropertyType.CaptionML, PropertyType.CardFormId, PropertyType.CardPageId, PropertyType.ChangeTrackingAllowed, PropertyType.DataCaptionExpression, PropertyType.DataCaptionFields, PropertyType.Date, PropertyType.DelayedInsert, PropertyType.DeleteAllowed, PropertyType.Description, PropertyType.Editable, PropertyType.EntityName, PropertyType.EntitySetName, PropertyType.Id, PropertyType.InsertAllowed, PropertyType.InstructionalTextML, PropertyType.LinksAllowed, PropertyType.Modified, PropertyType.ModifyAllowed, PropertyType.MultipleNewLines, PropertyType.Name, PropertyType.ODataKeyFields, PropertyType.PageType, PropertyType.Permissions, PropertyType.PopulateAllFields, PropertyType.PromotedActionCategoriesML, PropertyType.QueryCategory, PropertyType.RefreshOnActivate, PropertyType.SaveValues, PropertyType.ShowFilter, PropertyType.SourceTable, PropertyType.SourceTableTemporary, PropertyType.SourceTableView, PropertyType.Time, PropertyType.TimerUpdate, PropertyType.UsageCategory, PropertyType.VersionList).DefineRelations(new ElementRelation(ElementType.Page, ElementType.Action, MultiplicityType.ZeroToMany, "ACTIONS", 55242), new ElementRelation(ElementType.Page, ElementType.Control, MultiplicityType.ZeroToMany, "CONTROLS"), new ElementRelation(ElementType.Page, ElementType.Documentation), new ElementRelation(ElementType.Page, ElementType.Event, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Page, ElementType.OnAfterGetCurrRecord, MultiplicityType.ZeroToOne, null, 55151), new ElementRelation(ElementType.Page, ElementType.OnAfterGetRecord, MultiplicityType.ZeroToOne, null, 55145), new ElementRelation(ElementType.Page, ElementType.OnClosePage, MultiplicityType.ZeroToOne, null, 55142), new ElementRelation(ElementType.Page, ElementType.OnDeleteRecord, MultiplicityType.ZeroToOne, null, 55149), new ElementRelation(ElementType.Page, ElementType.OnFindRecord, MultiplicityType.ZeroToOne, null, 55143), new ElementRelation(ElementType.Page, ElementType.OnInit, MultiplicityType.ZeroToOne, null, 55140), new ElementRelation(ElementType.Page, ElementType.OnInsertRecord, MultiplicityType.ZeroToOne, null, 55147), new ElementRelation(ElementType.Page, ElementType.OnModifyRecord, MultiplicityType.ZeroToOne, null, 55148), new ElementRelation(ElementType.Page, ElementType.OnNewRecord, MultiplicityType.ZeroToOne, null, 55146), new ElementRelation(ElementType.Page, ElementType.OnNextRecord, MultiplicityType.ZeroToOne, null, 55144), new ElementRelation(ElementType.Page, ElementType.OnOpenPage, MultiplicityType.ZeroToOne, null, 55141), new ElementRelation(ElementType.Page, ElementType.OnQueryClosePage, MultiplicityType.ZeroToOne, null, 55150), new ElementRelation(ElementType.Page, ElementType.Procedure, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Page, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Object, ElementType.Form)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.ActiveControlOnOpen, PropertyType.AutoPosition, PropertyType.AutoSplitKey, PropertyType.BackColor, PropertyType.BorderStyle, PropertyType.CalcFields, PropertyType.CaptionBar, PropertyType.CaptionML, PropertyType.DataCaptionExpression, PropertyType.DataCaptionFields, PropertyType.Date, PropertyType.DelayedInsert, PropertyType.DeleteAllowed, PropertyType.Description, PropertyType.Editable, PropertyType.Height, PropertyType.Id, PropertyType.InsertAllowed, PropertyType.LinksAllowed, PropertyType.LookupMode, PropertyType.Maximizable, PropertyType.Minimizable, PropertyType.Modified, PropertyType.ModifyAllowed, PropertyType.MultipleNewLines, PropertyType.Name, PropertyType.Permissions, PropertyType.PopulateAllFields, PropertyType.SavePosAndSize, PropertyType.SaveTableView, PropertyType.SaveValues, PropertyType.Sizeable, PropertyType.SourceTable, PropertyType.SourceTablePlacement, PropertyType.SourceTableTemporary, PropertyType.SourceTableView, PropertyType.TableBoxId, PropertyType.Time, PropertyType.TimerInterval, PropertyType.UpdateOnActivate, PropertyType.VersionList, PropertyType.Width).DefineRelations(new ElementRelation(ElementType.Form, ElementType.Documentation), new ElementRelation(ElementType.Form, ElementType.Event, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Form, ElementType.FormControl, MultiplicityType.ZeroToMany, "CONTROLS"), new ElementRelation(ElementType.Form, ElementType.OnActivateForm), new ElementRelation(ElementType.Form, ElementType.OnAfterGetCurrRecord), new ElementRelation(ElementType.Form, ElementType.OnAfterGetRecord), new ElementRelation(ElementType.Form, ElementType.OnBeforePutRecord), new ElementRelation(ElementType.Form, ElementType.OnDeleteRecord), new ElementRelation(ElementType.Form, ElementType.OnFindRecord), new ElementRelation(ElementType.Form, ElementType.OnInit, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Form, ElementType.OnInsertRecord), new ElementRelation(ElementType.Form, ElementType.OnModifyRecord), new ElementRelation(ElementType.Form, ElementType.OnNewRecord), new ElementRelation(ElementType.Form, ElementType.OnNextRecord), new ElementRelation(ElementType.Form, ElementType.OnOpenForm), new ElementRelation(ElementType.Form, ElementType.OnQueryCloseForm), new ElementRelation(ElementType.Form, ElementType.Procedure, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Form, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Object, ElementType.Dataport)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.CaptionML, PropertyType.DataItemSeparator, PropertyType.Date, PropertyType.FieldEndDelimiter, PropertyType.FieldSeparator, PropertyType.FieldStartDelimiter, PropertyType.FileFormat, PropertyType.Id, PropertyType.Import, PropertyType.Modified, PropertyType.Name, PropertyType.Time, PropertyType.UseReqForm, PropertyType.VersionList).DefineRelations(new ElementRelation(ElementType.Dataport, ElementType.DataportItem, MultiplicityType.ZeroToMany, "DATAITEMS"), new ElementRelation(ElementType.Dataport, ElementType.Documentation), new ElementRelation(ElementType.Dataport, ElementType.Event, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Dataport, ElementType.OnPreDataport), new ElementRelation(ElementType.Dataport, ElementType.Procedure, MultiplicityType.ZeroToMany), new ElementRelation(ElementType.Dataport, ElementType.RequestForm, MultiplicityType.ZeroToOne, "REQUESTFORM"), new ElementRelation(ElementType.Dataport, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Element, ElementType.MenuNode)
			{
				nameProperty = PropertyType.Name,
				typeProperty = PropertyType.MenuNodeType
			}.DefineProperties(PropertyType.AccessByPermission, PropertyType.ApplicationArea, PropertyType.CaptionML, PropertyType.Deleted, PropertyType.DepartmentCategory, PropertyType.Enabled, PropertyType.FirstChild, PropertyType.Image, PropertyType.Inherited, PropertyType.IsDepartmentPage, PropertyType.IsShortcut, PropertyType.MemberOfMenu, PropertyType.MenuNodeId, PropertyType.MenuNodeType, PropertyType.Name, PropertyType.NextNodeId, PropertyType.ParentNodeId, PropertyType.PositionInGroup, PropertyType.RunObjectId, PropertyType.RunObjectType, PropertyType.ShortcutParameters, PropertyType.ShortcutURL, PropertyType.Visible, PropertyType.VisibleInDesign),
			new ElementTypeInfo(CategoryType.Element, ElementType.DataportItem)
			{
				nameProperty = PropertyType.DataItemVarName
			}.DefineProperties(PropertyType.AutoReplace, PropertyType.AutoSave, PropertyType.AutoUpdate, PropertyType.DataItemTable, PropertyType.DataItemTableView, PropertyType.DataItemVarName, PropertyType.RequestFilterFields).DefineRelations(new ElementRelation(ElementType.DataportItem, ElementType.DataportField, MultiplicityType.ZeroToMany, "FIELDS"), new ElementRelation(ElementType.DataportItem, ElementType.OnPreDataItem)),
			new ElementTypeInfo(CategoryType.Element, ElementType.DataportField)
			{
				nameProperty = PropertyType.SourceExpression
			}.DefineProperties(PropertyType.Enabled, PropertyType.SourceExpression, PropertyType.StartPos, PropertyType.Width),
			new ElementTypeInfo(CategoryType.Element, ElementType.RequestForm).DefineProperties(PropertyType.CaptionML, PropertyType.Height, PropertyType.InsertAllowed, PropertyType.LookupMode, PropertyType.Permissions, PropertyType.SaveValues, PropertyType.SourceTable, PropertyType.Width).DefineRelations(new ElementRelation(ElementType.RequestForm, ElementType.FormControl, MultiplicityType.ZeroToMany, "CONTROLS"), new ElementRelation(ElementType.RequestForm, ElementType.OnOpenForm)),
			new ElementTypeInfo(CategoryType.Element, ElementType.MenuItem)
			{
				idProperty = PropertyType.MenuItemId,
				nameProperty = PropertyType.Name,
				typeProperty = PropertyType.MenuItemType
			}.DefineProperties(PropertyType.CaptionML, PropertyType.Ellipsis, PropertyType.Enabled, PropertyType.MenuItemId, PropertyType.MenuItemType, PropertyType.MenuLevel, PropertyType.Name, PropertyType.PushAction, PropertyType.RunCommand, PropertyType.RunFormLink, PropertyType.RunFormLinkType, PropertyType.RunFormOnRec, PropertyType.RunFormView, PropertyType.RunObject, PropertyType.ShortcutKey, PropertyType.ToolTipML, PropertyType.UpdateOnAction, PropertyType.Visible).DefineRelations(new ElementRelation(ElementType.MenuItem, ElementType.OnPush)),
			new ElementTypeInfo(CategoryType.Element, ElementType.Control)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name,
				typeProperty = PropertyType.ControlType
			}.DefineProperties(PropertyType.AccessByPermission, PropertyType.ActionList, PropertyType.ApplicationArea, PropertyType.AssistEdit, PropertyType.AutoFormatExpression, PropertyType.AutoFormatType, PropertyType.BackColor, PropertyType.BlankNumbers, PropertyType.BlankZero, PropertyType.CaptionClass, PropertyType.CaptionML, PropertyType.CharAllowed, PropertyType.ChartPartId, PropertyType.ClosingDates, PropertyType.ColumnSpan, PropertyType.ContainerType, PropertyType.ControlAddIn, PropertyType.ControlType, PropertyType.DateFormula, PropertyType.DecimalPlaces, PropertyType.Description, PropertyType.DrillDown, PropertyType.DrillDownFormId, PropertyType.DrillDownPageId, PropertyType.Editable, PropertyType.Enabled, PropertyType.EntityName, PropertyType.EntitySetName, PropertyType.ExtendedDataType, PropertyType.FreezeColumnId, PropertyType.GroupType, PropertyType.HideValue, PropertyType.Id, PropertyType.Image, PropertyType.ImplicitType, PropertyType.Importance, PropertyType.IndentationColumnName, PropertyType.IndentationControls, PropertyType.InstructionalTextML, PropertyType.Layout, PropertyType.Level, PropertyType.Lookup, PropertyType.LookupFormId, PropertyType.LookupPageId, PropertyType.MaxValue, PropertyType.MinValue, PropertyType.Multiline, PropertyType.Name, PropertyType.NotBlank, PropertyType.Numeric, PropertyType.ODataEdmType, PropertyType.OptionCaptionML, PropertyType.PagePartId, PropertyType.PartType, PropertyType.ProviderId, PropertyType.QuickEntry, PropertyType.RowSpan, PropertyType.ShowAsTree, PropertyType.ShowCaption, PropertyType.ShowFilter, PropertyType.ShowMandatory, PropertyType.SourceExpression, PropertyType.Style, PropertyType.StyleExpression, PropertyType.SubFormLink, PropertyType.SubFormView, PropertyType.SubpageLink, PropertyType.SubpageView, PropertyType.SystemPartId, PropertyType.TableRelation, PropertyType.ToolTipML, PropertyType.UpdatePropagation, PropertyType.ValuesAllowed, PropertyType.Visible, PropertyType.Width).DefineRelations(new ElementRelation(ElementType.Control, ElementType.Action, MultiplicityType.ZeroToMany, "ACTIONS", 55242), new ElementRelation(ElementType.Control, ElementType.OnAddWatch), new ElementRelation(ElementType.Control, ElementType.OnAssistEdit, MultiplicityType.ZeroToOne, null, 55173), new ElementRelation(ElementType.Control, ElementType.OnControlAddIn, MultiplicityType.ZeroToOne, null, 55174), new ElementRelation(ElementType.Control, ElementType.OnDataPointClicked), new ElementRelation(ElementType.Control, ElementType.OnDataPointDoubleClicked), new ElementRelation(ElementType.Control, ElementType.OnDrillDown, MultiplicityType.ZeroToOne, null, 55172), new ElementRelation(ElementType.Control, ElementType.OnGetVariableValue), new ElementRelation(ElementType.Control, ElementType.OnLookup, MultiplicityType.ZeroToOne, null, 55171), new ElementRelation(ElementType.Control, ElementType.OnSetBreakpoint), new ElementRelation(ElementType.Control, ElementType.OnValidate, MultiplicityType.ZeroToOne, null, 55170)),
			new ElementTypeInfo(CategoryType.Element, ElementType.Action)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name,
				typeProperty = PropertyType.ActionType
			}.DefineProperties(PropertyType.AccessByPermission, PropertyType.ActionContainerType, PropertyType.ActionType, PropertyType.ApplicationArea, PropertyType.CaptionML, PropertyType.Description, PropertyType.Ellipsis, PropertyType.Enabled, PropertyType.Gesture, PropertyType.Id, PropertyType.Image, PropertyType.InFooterBar, PropertyType.IsHeader, PropertyType.Level, PropertyType.Name, PropertyType.Promoted, PropertyType.PromotedCategory, PropertyType.PromotedIsBig, PropertyType.PromotedOnly, PropertyType.RunFormLink, PropertyType.RunFormMode, PropertyType.RunFormOnRec, PropertyType.RunFormView, PropertyType.RunObject, PropertyType.RunPageLink, PropertyType.RunPageMode, PropertyType.RunPageOnRec, PropertyType.RunPageView, PropertyType.Scope, PropertyType.ShortcutKey, PropertyType.ToolTipML, PropertyType.Visible).DefineRelations(new ElementRelation(ElementType.Action, ElementType.OnAction, MultiplicityType.ZeroToOne, null, 55160)),
			new ElementTypeInfo(CategoryType.Element, ElementType.Field)
			{
				idProperty = PropertyType.FieldNo,
				nameProperty = PropertyType.Name,
				typeProperty = PropertyType.DataType
			}.DefineProperties(PropertyType.AccessByPermission, PropertyType.AltSearchField, PropertyType.AutoFormatExpression, PropertyType.AutoFormatType, PropertyType.AutoIncrement, PropertyType.BlankNumbers, PropertyType.BlankZero, PropertyType.BlobSubtype, PropertyType.CalcFormula, PropertyType.CaptionClass, PropertyType.CaptionML, PropertyType.CharAllowed, PropertyType.ClosingDates, PropertyType.Compressed, PropertyType.DataClassification, PropertyType.DataType, PropertyType.DateFormula, PropertyType.DecimalPlaces, PropertyType.Description, PropertyType.Editable, PropertyType.Enabled, PropertyType.EnumTypeId, PropertyType.EnumTypeName, PropertyType.ExtendedDataType, PropertyType.Extensible, PropertyType.ExternalAccess, PropertyType.ExternalName, PropertyType.ExternalType, PropertyType.FieldClass, PropertyType.FieldNo, PropertyType.Image, PropertyType.InitValue, PropertyType.MaxValue, PropertyType.MinValue, PropertyType.Name, PropertyType.NotBlank, PropertyType.Numeric, PropertyType.ObsoleteReason, PropertyType.ObsoleteState, PropertyType.OptionCaptionML, PropertyType.OptionOrdinalValues, PropertyType.OptionString, PropertyType.Owner, PropertyType.ReplicateData, PropertyType.SignDisplacement, PropertyType.SqlDataType, PropertyType.SqlTimestamp, PropertyType.StandardDayTimeUnit, PropertyType.TableIdExpression, PropertyType.TableRelation, PropertyType.TestTableRelation, PropertyType.Title, PropertyType.ValidateTableRelation, PropertyType.ValuesAllowed, PropertyType.Volatile, PropertyType.Width).DefineRelations(new ElementRelation(ElementType.Field, ElementType.OnLookup, MultiplicityType.ZeroToOne, null, 4963), new ElementRelation(ElementType.Field, ElementType.OnValidate, MultiplicityType.ZeroToOne, null, 4962)),
			new ElementTypeInfo(CategoryType.Element, ElementType.Key)
			{
				nameProperty = PropertyType.Key
			}.DefineProperties(PropertyType.Clustered, PropertyType.Enabled, PropertyType.Key, PropertyType.KeyGroups, PropertyType.MaintainSiftIndex, PropertyType.MaintainSqlIndex, PropertyType.ObsoleteReason, PropertyType.ObsoleteState, PropertyType.SiftLevelsToMaintain, PropertyType.SqlIndex, PropertyType.SumIndexFields, PropertyType.Unique),
			new ElementTypeInfo(CategoryType.Element, ElementType.FieldGroup)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.CaptionML, PropertyType.Fields, PropertyType.Id, PropertyType.Name),
			new ElementTypeInfo(CategoryType.Element, ElementType.FormControl)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name,
				typeProperty = PropertyType.ControlType
			}.DefineProperties(PropertyType.AssistEdit, PropertyType.AutoCalcField, PropertyType.AutoEnter, PropertyType.AutoFormatExpression, PropertyType.AutoFormatType, PropertyType.AutoRepeat, PropertyType.BackColor, PropertyType.BackTransparent, PropertyType.Bitmap, PropertyType.BitmapList, PropertyType.BitmapPos, PropertyType.BlankNumbers, PropertyType.BlankZero, PropertyType.Border, PropertyType.BorderColor, PropertyType.BorderStyle, PropertyType.BorderWidth, PropertyType.Cancel, PropertyType.CaptionClass, PropertyType.CaptionML, PropertyType.ClearOnLookup, PropertyType.ClosingDates, PropertyType.ControlHeight, PropertyType.ControlType, PropertyType.ControlWidth, PropertyType.DateFormula, PropertyType.DecimalPlaces, PropertyType.Default, PropertyType.Description, PropertyType.Divisor, PropertyType.DrillDown, PropertyType.DrillDownFormId, PropertyType.DrillDownPageId, PropertyType.DropDown, PropertyType.Editable, PropertyType.Ellipsis, PropertyType.Enabled, PropertyType.Focusable, PropertyType.FocusOnClick, PropertyType.FontBold, PropertyType.FontItalic, PropertyType.FontName, PropertyType.FontSize, PropertyType.ForeColor, PropertyType.HeadingHeight, PropertyType.HorizontalAlign, PropertyType.HorizontalGlue, PropertyType.Id, PropertyType.InColumn, PropertyType.InColumnHeading, PropertyType.InFrame, PropertyType.InlineEditing, PropertyType.InMatrix, PropertyType.InMatrixHeading, PropertyType.InPage, PropertyType.InvalidActionAppearance, PropertyType.LeaderDots, PropertyType.Lookup, PropertyType.LookupFormId, PropertyType.LookupPageId, PropertyType.MatrixColumnWidth, PropertyType.MatrixSourceTable, PropertyType.MaxLength, PropertyType.MaxValue, PropertyType.Menu, PropertyType.MinValue, PropertyType.Multiline, PropertyType.Name, PropertyType.NextControl, PropertyType.NotBlank, PropertyType.OptionCaptionML, PropertyType.OptionString, PropertyType.OptionValue, PropertyType.PageNamesML, PropertyType.ParentControl, PropertyType.PasswordText, PropertyType.PermanentAssist, PropertyType.PushAction, PropertyType.RowHeight, PropertyType.RunCommand, PropertyType.RunFormLink, PropertyType.RunFormLinkType, PropertyType.RunFormView, PropertyType.RunObject, PropertyType.ShapeStyle, PropertyType.ShowCaption, PropertyType.SourceExpression, PropertyType.SubFormId, PropertyType.SubFormLink, PropertyType.SubFormView, PropertyType.TableRelation, PropertyType.ToolTipML, PropertyType.TopLineOnly, PropertyType.UpdateOnAction, PropertyType.ValidateTableRelation, PropertyType.ValuesAllowed, PropertyType.VerticalAlign, PropertyType.VerticalGlue, PropertyType.Visible, PropertyType.X, PropertyType.Y).DefineRelations(new ElementRelation(ElementType.FormControl, ElementType.MenuItem, MultiplicityType.ZeroToMany, "MENUITEMS"), new ElementRelation(ElementType.FormControl, ElementType.OnActivate), new ElementRelation(ElementType.FormControl, ElementType.OnAfterValidate), new ElementRelation(ElementType.FormControl, ElementType.OnAssistEdit), new ElementRelation(ElementType.FormControl, ElementType.OnDeactivate), new ElementRelation(ElementType.FormControl, ElementType.OnDrillDown), new ElementRelation(ElementType.FormControl, ElementType.OnFormat), new ElementRelation(ElementType.FormControl, ElementType.OnLookup), new ElementRelation(ElementType.FormControl, ElementType.OnPush), new ElementRelation(ElementType.FormControl, ElementType.OnValidate)),
			new ElementTypeInfo(CategoryType.Element, ElementType.XmlPortElement)
			{
				nameProperty = PropertyType.Name,
				typeProperty = PropertyType.NodeType
			}.DefineProperties(PropertyType.AutoCalcField, PropertyType.AutoReplace, PropertyType.AutoSave, PropertyType.AutoUpdate, PropertyType.CalcFields, PropertyType.ElementId, PropertyType.FieldValidate, PropertyType.Indentation, PropertyType.LinkFields, PropertyType.LinkTable, PropertyType.LinkTableForceInsert, PropertyType.MaxOccurs, PropertyType.MinOccurs, PropertyType.Name, PropertyType.NamespacePrefix, PropertyType.NodeType, PropertyType.Occurrence, PropertyType.RequestFilterFields, PropertyType.RequestFilterHeadingML, PropertyType.SourceField, PropertyType.SourceTable, PropertyType.SourceTableView, PropertyType.SourceType, PropertyType.Temporary, PropertyType.TextType, PropertyType.Unbound, PropertyType.VariableName, PropertyType.Width, PropertyType.XmlPortElementDataType).DefineRelations(new ElementRelation(ElementType.XmlPortElement, ElementType.ExportOnAfterGetRecord, MultiplicityType.ZeroToOne, null, 54139), new ElementRelation(ElementType.XmlPortElement, ElementType.ExportOnBeforePassField, MultiplicityType.ZeroToOne, null, 54147), new ElementRelation(ElementType.XmlPortElement, ElementType.ExportOnBeforePassVariable, MultiplicityType.ZeroToOne, null, 54127), new ElementRelation(ElementType.XmlPortElement, ElementType.ExportOnPreXmlItem, MultiplicityType.ZeroToOne, null, 54138), new ElementRelation(ElementType.XmlPortElement, ElementType.ImportOnAfterAssignField, MultiplicityType.ZeroToOne, null, 54146), new ElementRelation(ElementType.XmlPortElement, ElementType.ImportOnAfterAssignVariable, MultiplicityType.ZeroToOne, null, 54126), new ElementRelation(ElementType.XmlPortElement, ElementType.ImportOnAfterInitRecord, MultiplicityType.ZeroToOne, null, 54136), new ElementRelation(ElementType.XmlPortElement, ElementType.ImportOnAfterInsertRecord, MultiplicityType.ZeroToOne, null, 54140), new ElementRelation(ElementType.XmlPortElement, ElementType.ImportOnAfterModifyRecord, MultiplicityType.ZeroToOne, null, 54142), new ElementRelation(ElementType.XmlPortElement, ElementType.ImportOnBeforeInsertRecord, MultiplicityType.ZeroToOne, null, 54137), new ElementRelation(ElementType.XmlPortElement, ElementType.ImportOnBeforeModifyRecord, MultiplicityType.ZeroToOne, null, 54141)),
			new ElementTypeInfo(CategoryType.Element, ElementType.XmlPortEvent)
			{
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.Name),
			new ElementTypeInfo(CategoryType.Element, ElementType.RequestPage).DefineProperties(PropertyType.ActionList, PropertyType.AutoSplitKey, PropertyType.CaptionML, PropertyType.CardPageId, PropertyType.DataCaptionExpression, PropertyType.DataCaptionFields, PropertyType.DeleteAllowed, PropertyType.Description, PropertyType.Editable, PropertyType.InsertAllowed, PropertyType.InstructionalTextML, PropertyType.LinksAllowed, PropertyType.ModifyAllowed, PropertyType.MultipleNewLines, PropertyType.Permissions, PropertyType.PopulateAllFields, PropertyType.RefreshOnActivate, PropertyType.SaveValues, PropertyType.ShowFilter, PropertyType.SourceTable, PropertyType.SourceTableTemporary, PropertyType.SourceTableView, PropertyType.TimerUpdate).DefineRelations(new ElementRelation(ElementType.RequestPage, ElementType.Action, MultiplicityType.ZeroToMany, "ACTIONS", 55242), new ElementRelation(ElementType.RequestPage, ElementType.Control, MultiplicityType.ZeroToMany, "CONTROLS"), new ElementRelation(ElementType.RequestPage, ElementType.OnAfterGetCurrRecord, MultiplicityType.ZeroToOne, null, 55151), new ElementRelation(ElementType.RequestPage, ElementType.OnAfterGetRecord, MultiplicityType.ZeroToOne, null, 55145), new ElementRelation(ElementType.RequestPage, ElementType.OnClosePage, MultiplicityType.ZeroToOne, null, 55142), new ElementRelation(ElementType.RequestPage, ElementType.OnDeleteRecord, MultiplicityType.ZeroToOne, null, 55149), new ElementRelation(ElementType.RequestPage, ElementType.OnFindRecord, MultiplicityType.ZeroToOne, null, 55143), new ElementRelation(ElementType.RequestPage, ElementType.OnInit, MultiplicityType.ZeroToOne, null, 55140), new ElementRelation(ElementType.RequestPage, ElementType.OnInsertRecord, MultiplicityType.ZeroToOne, null, 55147), new ElementRelation(ElementType.RequestPage, ElementType.OnModifyRecord, MultiplicityType.ZeroToOne, null, 55148), new ElementRelation(ElementType.RequestPage, ElementType.OnNewRecord, MultiplicityType.ZeroToOne, null, 55146), new ElementRelation(ElementType.RequestPage, ElementType.OnNextRecord, MultiplicityType.ZeroToOne, null, 55144), new ElementRelation(ElementType.RequestPage, ElementType.OnOpenPage, MultiplicityType.ZeroToOne, null, 55141), new ElementRelation(ElementType.RequestPage, ElementType.OnQueryClosePage, MultiplicityType.ZeroToOne, null, 55150)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnPostSection).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnPostSection, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnInitReport).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnInitReport, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnPostReport).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnPostReport, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnPostDataItem).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnPostDataItem, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnDelete).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnDelete, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnInsert).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnInsert, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnModify).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnModify, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnRename).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnRename, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnValidate).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnValidate, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnLookup).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnLookup, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnRun).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnRun, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnAction).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnAction, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnInit).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnInit, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnOpenPage).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnOpenPage, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnDrillDown).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnDrillDown, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnAfterGetRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnAfterGetRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnNewRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnNewRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnFindRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnFindRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnAssistEdit).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnAssistEdit, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnModifyRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnModifyRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnInsertRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnInsertRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnDeleteRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnDeleteRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnClosePage).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnClosePage, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnQueryClosePage).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnQueryClosePage, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnNextRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnNextRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnOpenForm).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnOpenForm, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnPush).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnPush, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnFormat).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnFormat, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnAfterGetCurrRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnAfterGetCurrRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnAfterValidate).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnAfterValidate, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnDeactivate).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnDeactivate, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnBeforePutRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnBeforePutRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnAfterInput).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnAfterInput, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnActivateForm).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnActivateForm, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnActivate).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnActivate, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnCloseForm).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnCloseForm, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnQueryCloseForm).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnQueryCloseForm, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnBeforeInput).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnBeforeInput, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnTimer).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnTimer, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnDeactivateForm).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnDeactivateForm, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnBeforeExportRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnBeforeExportRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnPreDataItem).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnPreDataItem, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnBeforeImportRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnBeforeImportRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnAfterImportRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnAfterImportRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnPreReport).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnPreReport, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnPreSection).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnPreSection, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.ImportOnAfterAssignVariable).DefineProperties().DefineRelations(new ElementRelation(ElementType.ImportOnAfterAssignVariable, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.ImportOnBeforeInsertRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.ImportOnBeforeInsertRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.ExportOnAfterGetRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.ExportOnAfterGetRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.ImportOnAfterInsertRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.ImportOnAfterInsertRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.ExportOnBeforePassVariable).DefineProperties().DefineRelations(new ElementRelation(ElementType.ExportOnBeforePassVariable, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.ExportOnPreXmlItem).DefineProperties().DefineRelations(new ElementRelation(ElementType.ExportOnPreXmlItem, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnPreXmlPort).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnPreXmlPort, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnPostXmlPort).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnPostXmlPort, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.ImportOnAfterAssignField).DefineProperties().DefineRelations(new ElementRelation(ElementType.ImportOnAfterAssignField, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnPreDataport).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnPreDataport, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Element, ElementType.Label)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.CaptionML, PropertyType.Description, PropertyType.Id, PropertyType.Name).DefineRelations(new ElementRelation(ElementType.Label, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnControlAddIn).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnControlAddIn, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.ExportOnBeforePassField).DefineProperties().DefineRelations(new ElementRelation(ElementType.ExportOnBeforePassField, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnInitXmlPort).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnInitXmlPort, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.ImportOnAfterInitRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.ImportOnAfterInitRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnDataPointClicked)
			{
				model = ModelType.Learning
			}.DefineProperties().DefineRelations(new ElementRelation(ElementType.OnDataPointClicked, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnDataPointDoubleClicked)
			{
				model = ModelType.Learning
			}.DefineProperties().DefineRelations(new ElementRelation(ElementType.OnDataPointDoubleClicked, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnSetBreakpoint)
			{
				model = ModelType.Learning
			}.DefineProperties().DefineRelations(new ElementRelation(ElementType.OnSetBreakpoint, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnGetVariableValue)
			{
				model = ModelType.Learning
			}.DefineProperties().DefineRelations(new ElementRelation(ElementType.OnGetVariableValue, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnAddWatch)
			{
				model = ModelType.Learning
			}.DefineProperties().DefineRelations(new ElementRelation(ElementType.OnAddWatch, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Element, ElementType.DataItem)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name,
				typeProperty = PropertyType.ReportDataItemDataType
			}.DefineProperties(PropertyType.AutoCalcField, PropertyType.AutoFormatExpression, PropertyType.AutoFormatType, PropertyType.CalcFields, PropertyType.DataItemLink, PropertyType.DataItemLinkReference, PropertyType.DataItemTable, PropertyType.DataItemTableView, PropertyType.DecimalPlaces, PropertyType.Description, PropertyType.Id, PropertyType.IncludeCaption, PropertyType.Indentation, PropertyType.MaxIteration, PropertyType.Name, PropertyType.OptionCaptionML, PropertyType.OptionString, PropertyType.PrintOnlyIfDetail, PropertyType.ReportDataItemDataType, PropertyType.RequestFilterFields, PropertyType.RequestFilterHeadingML, PropertyType.SourceExpression, PropertyType.Temporary, PropertyType.UniqueId).DefineRelations(new ElementRelation(ElementType.DataItem, ElementType.OnAfterGetRecord, MultiplicityType.ZeroToOne, null, 18530), new ElementRelation(ElementType.DataItem, ElementType.OnPostDataItem, MultiplicityType.ZeroToOne, null, 18531), new ElementRelation(ElementType.DataItem, ElementType.OnPreDataItem, MultiplicityType.ZeroToOne, null, 18529)),
			new ElementTypeInfo(CategoryType.Object, ElementType.Query)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name
			}.DefineProperties(PropertyType.APIGroup, PropertyType.APIPublisher, PropertyType.APIVersion, PropertyType.CaptionML, PropertyType.Date, PropertyType.Description, PropertyType.EntityName, PropertyType.EntitySetName, PropertyType.Id, PropertyType.Modified, PropertyType.Name, PropertyType.OrderBy, PropertyType.Permissions, PropertyType.QueryCategory, PropertyType.QueryType, PropertyType.ReadState, PropertyType.Time, PropertyType.TopNumberOfRows, PropertyType.TransactionType, PropertyType.VersionList).DefineRelations(new ElementRelation(ElementType.Query, ElementType.Documentation), new ElementRelation(ElementType.Query, ElementType.OnBeforeOpen, MultiplicityType.ZeroToOne, null, 57700), new ElementRelation(ElementType.Query, ElementType.QueryElement, MultiplicityType.ZeroToMany, "ELEMENTS")),
			new ElementTypeInfo(CategoryType.Element, ElementType.QueryElement)
			{
				idProperty = PropertyType.Id,
				nameProperty = PropertyType.Name,
				typeProperty = PropertyType.QueryElementType
			}.DefineProperties(PropertyType.CaptionML, PropertyType.ColumnFilter, PropertyType.DataItemLink, PropertyType.DataItemLinkType, PropertyType.DataItemTable, PropertyType.DataItemTableFilter, PropertyType.DataSource, PropertyType.Description, PropertyType.Id, PropertyType.Level, PropertyType.Method, PropertyType.MethodType, PropertyType.Name, PropertyType.QueryElementType, PropertyType.ReverseSign, PropertyType.SqlJoinType),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.Documentation).DefineProperties(),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.NewBaselineCode)
			{
				model = ModelType.DevBase
			}.DefineProperties(),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnBeforeOpen).DefineProperties().DefineRelations(new ElementRelation(ElementType.OnBeforeOpen, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.ImportOnBeforeModifyRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.ImportOnBeforeModifyRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.ImportOnAfterModifyRecord).DefineProperties().DefineRelations(new ElementRelation(ElementType.ImportOnAfterModifyRecord, ElementType.Variable, MultiplicityType.ZeroToMany)),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OriginalCode)
			{
				model = ModelType.DevBase
			}.DefineProperties(),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.ModifiedCode)
			{
				model = ModelType.DevBase
			}.DefineProperties(),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.TargetCode)
			{
				model = ModelType.DevBase
			}.DefineProperties(),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnCreateHyperlink).DefineProperties(),
			new ElementTypeInfo(CategoryType.Trigger, ElementType.OnHyperlink).DefineProperties()
		};
	}

	internal ElementTypeInfo(CategoryType category, ElementType elementType)
	{
		this.category = category;
		this.elementType = elementType;
		name = string.Empty;
		model = ModelType.CSide;
		idProperty = PropertyType.Unknown;
		nameProperty = PropertyType.Unknown;
		typeProperty = PropertyType.Unknown;
	}

	private ElementTypeInfo DefineProperties(params PropertyType[] definedProperties)
	{
		properties = new ElementPropertyLookup(this, definedProperties.Select((PropertyType pt) => new ElementProperty(this, PropertyTypeInfo.Lookup.GetValue(pt))));
		return this;
	}

	private ElementTypeInfo DefineRelations(params ElementRelation[] definedRelations)
	{
		relations = new ElementRelationLookup(this, definedRelations);
		return this;
	}

	public override string ToString()
	{
		return Name;
	}
}
