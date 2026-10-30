using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

[DebuggerDisplay("{ParentType} {Multiplicity} {ChildType} in {ContainerName}")]
public class ElementRelationExport
{
	internal const string ChangeElementsContainerName = "ChangedElements";

	public static readonly IReadOnlyCollection<ElementRelationExport> Definitions = new ElementRelationExport[332]
	{
		new ElementRelationExport(ElementType.Modification, ElementType.Change, CategoryType.Element)
		{
			ContainerName = "CHANGES",
			Multiplicity = MultiplicityType.ZeroToMany,
			SkipIfEmpty = true
		},
		new ElementRelationExport(ElementType.Report, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Report, ElementType.Procedure, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Report, ElementType.Event, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Report, ElementType.Documentation, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Report, ElementType.OnCreateHyperlink, CategoryType.Trigger)
		{
			Obsolete = true
		},
		new ElementRelationExport(ElementType.Report, ElementType.OnHyperlink, CategoryType.Trigger)
		{
			Obsolete = true
		},
		new ElementRelationExport(ElementType.Report, ElementType.OnInitReport, CategoryType.Trigger)
		{
			ExportOrder = 100,
			AttributeId = 18512
		},
		new ElementRelationExport(ElementType.Report, ElementType.DataItem, CategoryType.Element)
		{
			ContainerName = "DATASET",
			Multiplicity = MultiplicityType.ZeroToMany,
			ExportOrder = 100
		},
		new ElementRelationExport(ElementType.Report, ElementType.ReportItem, CategoryType.Element)
		{
			ContainerName = "DATAITEMS",
			Multiplicity = MultiplicityType.ZeroToMany,
			SkipIfEmpty = true,
			ExportOrder = 200,
			Obsolete = true,
			Unsupported = true
		},
		new ElementRelationExport(ElementType.Report, ElementType.OnPreReport, CategoryType.Trigger)
		{
			ExportOrder = 200,
			AttributeId = 18513
		},
		new ElementRelationExport(ElementType.Report, ElementType.RequestForm, CategoryType.Element)
		{
			ContainerName = "REQUESTFORM",
			SkipIfEmpty = true,
			ExportOrder = 300,
			Obsolete = true,
			Unsupported = true
		},
		new ElementRelationExport(ElementType.Report, ElementType.OnPostReport, CategoryType.Trigger)
		{
			ExportOrder = 300,
			AttributeId = 18514
		},
		new ElementRelationExport(ElementType.Report, ElementType.RequestPage, CategoryType.Element)
		{
			ContainerName = "REQUESTPAGE",
			ExportOrder = 400
		},
		new ElementRelationExport(ElementType.Report, ElementType.Label, CategoryType.Element)
		{
			ContainerName = "LABELS",
			Multiplicity = MultiplicityType.ZeroToMany,
			ExportOrder = 500
		},
		new ElementRelationExport(ElementType.OnDataPointDoubleClicked, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnSetBreakpoint, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnGetVariableValue, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnAddWatch, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.DataItem, ElementType.OnPreDataItem, CategoryType.Trigger)
		{
			ExportOrder = 100,
			AttributeId = 18529
		},
		new ElementRelationExport(ElementType.DataItem, ElementType.OnAfterGetRecord, CategoryType.Trigger)
		{
			ExportOrder = 200,
			AttributeId = 18530
		},
		new ElementRelationExport(ElementType.DataItem, ElementType.OnPostDataItem, CategoryType.Trigger)
		{
			ExportOrder = 300,
			AttributeId = 18531
		},
		new ElementRelationExport(ElementType.Query, ElementType.QueryElement, CategoryType.Element)
		{
			ContainerName = "ELEMENTS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Query, ElementType.Documentation, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Query, ElementType.OnBeforeOpen, CategoryType.Trigger)
		{
			AttributeId = 57700
		},
		new ElementRelationExport(ElementType.MenuSuite, ElementType.MenuNode, CategoryType.Element)
		{
			ContainerName = "MENUNODES",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnBeforeOpen, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.ImportOnBeforeModifyRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.ImportOnAfterModifyRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.XmlPort, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.XmlPort, ElementType.Procedure, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.XmlPort, ElementType.Event, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.XmlPort, ElementType.XmlPortElement, CategoryType.Element)
		{
			ContainerName = "ELEMENTS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.XmlPort, ElementType.XmlPortEvent, CategoryType.Element)
		{
			ContainerName = "EVENTS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.XmlPort, ElementType.RequestPage, CategoryType.Element)
		{
			ContainerName = "REQUESTPAGE",
			SkipIfEmpty = true
		},
		new ElementRelationExport(ElementType.XmlPort, ElementType.OnInitXmlPort, CategoryType.Trigger)
		{
			AttributeId = 54116
		},
		new ElementRelationExport(ElementType.XmlPort, ElementType.Documentation, CategoryType.Trigger),
		new ElementRelationExport(ElementType.XmlPort, ElementType.OnPreXmlPort, CategoryType.Trigger)
		{
			ExportOrder = 100,
			AttributeId = 54117
		},
		new ElementRelationExport(ElementType.XmlPort, ElementType.OnPostXmlPort, CategoryType.Trigger)
		{
			ExportOrder = 200,
			AttributeId = 54118
		},
		new ElementRelationExport(ElementType.ReportSection, ElementType.ReportControl, CategoryType.Element)
		{
			ContainerName = "CONTROLS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.ReportSection, ElementType.OnPostSection, CategoryType.Trigger),
		new ElementRelationExport(ElementType.ReportSection, ElementType.OnPreSection, CategoryType.Trigger),
		new ElementRelationExport(ElementType.ReportItem, ElementType.ReportSection, CategoryType.Element)
		{
			ContainerName = "SECTIONS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.ReportItem, ElementType.OnPostDataItem, CategoryType.Trigger),
		new ElementRelationExport(ElementType.ReportItem, ElementType.OnAfterGetRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.ReportItem, ElementType.OnPreDataItem, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Package, ElementType.Item, CategoryType.Element)
		{
			ContainerName = "ITEMS",
			Multiplicity = MultiplicityType.ZeroToMany,
			SkipIfEmpty = true
		},
		new ElementRelationExport(ElementType.Package, ElementType.Reference, CategoryType.Element)
		{
			ContainerName = "REFERENCES",
			Multiplicity = MultiplicityType.ZeroToMany,
			SkipIfEmpty = true
		},
		new ElementRelationExport(ElementType.Codeunit, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Codeunit, ElementType.Procedure, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Codeunit, ElementType.Event, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Codeunit, ElementType.OnRun, CategoryType.Trigger)
		{
			AttributeId = 26369
		},
		new ElementRelationExport(ElementType.Codeunit, ElementType.Documentation, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.Change, CategoryType.Element)
		{
			ContainerName = "ChangeCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.Variable, CategoryType.Identifier)
		{
			ContainerName = "VariableCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.Procedure, CategoryType.Method)
		{
			ContainerName = "PROCEDURECollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.Parameter, CategoryType.Identifier)
		{
			ContainerName = "ParameterCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.ReturnValue, CategoryType.Identifier)
		{
			ContainerName = "ReturnValueCollection",
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.Event, CategoryType.Method)
		{
			ContainerName = "EVENTCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.BaselineCode, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ResultingCode, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ReportSection, CategoryType.Element)
		{
			ContainerName = "ReportSectionCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.ReportItem, CategoryType.Element)
		{
			ContainerName = "ReportItemCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.ReportControl, CategoryType.Element)
		{
			ContainerName = "ReportControlCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.Item, CategoryType.Element)
		{
			ContainerName = "ItemCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.Reference, CategoryType.Element)
		{
			ContainerName = "ReferenceCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.MenuNode, CategoryType.Element)
		{
			ContainerName = "MenuNodeCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.DataportItem, CategoryType.Element)
		{
			ContainerName = "DataportItemCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.DataportField, CategoryType.Element)
		{
			ContainerName = "DataportFieldCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.RequestForm, CategoryType.Element)
		{
			ContainerName = "RequestForm",
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.MenuItem, CategoryType.Element)
		{
			ContainerName = "MenuItemCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.Control, CategoryType.Element)
		{
			ContainerName = "ControlCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.Action, CategoryType.Element)
		{
			ContainerName = "ActionCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.Field, CategoryType.Element)
		{
			ContainerName = "FieldCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.Key, CategoryType.Element)
		{
			ContainerName = "KeyCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.FieldGroup, CategoryType.Element)
		{
			ContainerName = "FieldGroupCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.FormControl, CategoryType.Element)
		{
			ContainerName = "FormControlCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.XmlPortElement, CategoryType.Element)
		{
			ContainerName = "XmlPortElementCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.XmlPortEvent, CategoryType.Element)
		{
			ContainerName = "XmlPortEventCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.RequestPage, CategoryType.Element)
		{
			ContainerName = "RequestPage",
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.OnPostSection, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnInitReport, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnPostReport, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnPostDataItem, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnDelete, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnInsert, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnModify, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnRename, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnValidate, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnLookup, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnRun, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnAction, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnInit, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnOpenPage, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnDrillDown, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnAfterGetRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnNewRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnFindRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnAssistEdit, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnModifyRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnInsertRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnDeleteRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnClosePage, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnQueryClosePage, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnNextRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnOpenForm, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnPush, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnFormat, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnAfterGetCurrRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnAfterValidate, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnDeactivate, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnBeforePutRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnAfterInput, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnActivateForm, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnActivate, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnCloseForm, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnQueryCloseForm, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnBeforeInput, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnTimer, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnDeactivateForm, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnBeforeExportRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnPreDataItem, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnBeforeImportRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnAfterImportRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnPreReport, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnPreSection, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ImportOnAfterAssignVariable, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ImportOnBeforeInsertRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ExportOnAfterGetRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ImportOnAfterInsertRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ExportOnBeforePassVariable, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ExportOnPreXmlItem, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnPreXmlPort, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnPostXmlPort, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ImportOnAfterAssignField, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnPreDataport, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.Label, CategoryType.Element)
		{
			ContainerName = "LabelCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.OnControlAddIn, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ExportOnBeforePassField, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnInitXmlPort, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ImportOnAfterInitRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnDataPointClicked, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnDataPointDoubleClicked, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnSetBreakpoint, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnGetVariableValue, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnAddWatch, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.DataItem, CategoryType.Element)
		{
			ContainerName = "DataItemCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.QueryElement, CategoryType.Element)
		{
			ContainerName = "QueryElementCollection",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.AddedChildren
		},
		new ElementRelationExport(ElementType.Change, ElementType.Documentation, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.NewBaselineCode, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OnBeforeOpen, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ImportOnBeforeModifyRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ImportOnAfterModifyRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.OriginalCode, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.ModifiedCode, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Change, ElementType.TargetCode, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Table, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Table, ElementType.Procedure, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Table, ElementType.Event, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Table, ElementType.Field, CategoryType.Element)
		{
			ContainerName = "FIELDS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Table, ElementType.Key, CategoryType.Element)
		{
			ContainerName = "KEYS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Table, ElementType.FieldGroup, CategoryType.Element)
		{
			ContainerName = "FIELDGROUPS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Table, ElementType.Documentation, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Table, ElementType.OnInsert, CategoryType.Trigger)
		{
			ExportOrder = 100,
			AttributeId = 4958
		},
		new ElementRelationExport(ElementType.Table, ElementType.OnModify, CategoryType.Trigger)
		{
			ExportOrder = 200,
			AttributeId = 4959
		},
		new ElementRelationExport(ElementType.Table, ElementType.OnDelete, CategoryType.Trigger)
		{
			ExportOrder = 300,
			AttributeId = 4960
		},
		new ElementRelationExport(ElementType.Table, ElementType.OnRename, CategoryType.Trigger)
		{
			ExportOrder = 400,
			AttributeId = 4961
		},
		new ElementRelationExport(ElementType.Page, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Page, ElementType.Procedure, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Page, ElementType.Event, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Page, ElementType.Control, CategoryType.Element)
		{
			ContainerName = "CONTROLS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Page, ElementType.Action, CategoryType.Element)
		{
			ContainerName = "ACTIONS",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.ActionList,
			AttributeId = 55242
		},
		new ElementRelationExport(ElementType.Page, ElementType.OnInit, CategoryType.Trigger)
		{
			AttributeId = 55140
		},
		new ElementRelationExport(ElementType.Page, ElementType.OnOpenPage, CategoryType.Trigger)
		{
			AttributeId = 55141
		},
		new ElementRelationExport(ElementType.Page, ElementType.Documentation, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Page, ElementType.OnClosePage, CategoryType.Trigger)
		{
			ExportOrder = 100,
			AttributeId = 55142
		},
		new ElementRelationExport(ElementType.Page, ElementType.OnFindRecord, CategoryType.Trigger)
		{
			ExportOrder = 200,
			AttributeId = 55143
		},
		new ElementRelationExport(ElementType.Page, ElementType.OnNextRecord, CategoryType.Trigger)
		{
			ExportOrder = 300,
			AttributeId = 55144
		},
		new ElementRelationExport(ElementType.Page, ElementType.OnAfterGetRecord, CategoryType.Trigger)
		{
			ExportOrder = 1000,
			AttributeId = 55145
		},
		new ElementRelationExport(ElementType.Page, ElementType.OnNewRecord, CategoryType.Trigger)
		{
			ExportOrder = 1050,
			AttributeId = 55146
		},
		new ElementRelationExport(ElementType.Page, ElementType.OnInsertRecord, CategoryType.Trigger)
		{
			ExportOrder = 1060,
			AttributeId = 55147
		},
		new ElementRelationExport(ElementType.Page, ElementType.OnModifyRecord, CategoryType.Trigger)
		{
			ExportOrder = 1065,
			AttributeId = 55148
		},
		new ElementRelationExport(ElementType.Page, ElementType.OnDeleteRecord, CategoryType.Trigger)
		{
			ExportOrder = 1070,
			AttributeId = 55149
		},
		new ElementRelationExport(ElementType.Page, ElementType.OnQueryClosePage, CategoryType.Trigger)
		{
			ExportOrder = 1080,
			AttributeId = 55150
		},
		new ElementRelationExport(ElementType.Page, ElementType.OnAfterGetCurrRecord, CategoryType.Trigger)
		{
			ExportOrder = 1100,
			AttributeId = 55151
		},
		new ElementRelationExport(ElementType.Form, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Form, ElementType.Procedure, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Form, ElementType.Event, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Form, ElementType.FormControl, CategoryType.Element)
		{
			ContainerName = "CONTROLS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Form, ElementType.OnInit, CategoryType.Trigger)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Form, ElementType.OnAfterGetRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Form, ElementType.OnNewRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Form, ElementType.OnFindRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Form, ElementType.OnModifyRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Form, ElementType.OnInsertRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Form, ElementType.OnDeleteRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Form, ElementType.OnNextRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Form, ElementType.OnOpenForm, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Form, ElementType.OnAfterGetCurrRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Form, ElementType.OnBeforePutRecord, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Form, ElementType.OnActivateForm, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Form, ElementType.OnQueryCloseForm, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Form, ElementType.Documentation, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Dataport, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Dataport, ElementType.Procedure, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Dataport, ElementType.Event, CategoryType.Method)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Dataport, ElementType.DataportItem, CategoryType.Element)
		{
			ContainerName = "DATAITEMS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Dataport, ElementType.RequestForm, CategoryType.Element)
		{
			ContainerName = "REQUESTFORM"
		},
		new ElementRelationExport(ElementType.Dataport, ElementType.OnPreDataport, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Dataport, ElementType.Documentation, CategoryType.Trigger),
		new ElementRelationExport(ElementType.DataportItem, ElementType.DataportField, CategoryType.Element)
		{
			ContainerName = "FIELDS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.DataportItem, ElementType.OnPreDataItem, CategoryType.Trigger),
		new ElementRelationExport(ElementType.RequestForm, ElementType.FormControl, CategoryType.Element)
		{
			ContainerName = "CONTROLS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.RequestForm, ElementType.OnOpenForm, CategoryType.Trigger),
		new ElementRelationExport(ElementType.MenuItem, ElementType.OnPush, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Control, ElementType.Action, CategoryType.Element)
		{
			ContainerName = "ACTIONS",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.ActionList,
			AttributeId = 55242
		},
		new ElementRelationExport(ElementType.Control, ElementType.OnValidate, CategoryType.Trigger)
		{
			AttributeId = 55170
		},
		new ElementRelationExport(ElementType.Control, ElementType.OnLookup, CategoryType.Trigger)
		{
			AttributeId = 55171
		},
		new ElementRelationExport(ElementType.Control, ElementType.OnDrillDown, CategoryType.Trigger)
		{
			AttributeId = 55172
		},
		new ElementRelationExport(ElementType.Control, ElementType.OnAssistEdit, CategoryType.Trigger)
		{
			AttributeId = 55173
		},
		new ElementRelationExport(ElementType.Control, ElementType.OnControlAddIn, CategoryType.Trigger)
		{
			AttributeId = 55174
		},
		new ElementRelationExport(ElementType.Control, ElementType.OnDataPointClicked, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Control, ElementType.OnDataPointDoubleClicked, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Control, ElementType.OnSetBreakpoint, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Control, ElementType.OnGetVariableValue, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Control, ElementType.OnAddWatch, CategoryType.Trigger),
		new ElementRelationExport(ElementType.Action, ElementType.OnAction, CategoryType.Trigger)
		{
			AttributeId = 55160
		},
		new ElementRelationExport(ElementType.Field, ElementType.OnValidate, CategoryType.Trigger)
		{
			AttributeId = 4962
		},
		new ElementRelationExport(ElementType.Field, ElementType.OnLookup, CategoryType.Trigger)
		{
			AttributeId = 4963
		},
		new ElementRelationExport(ElementType.FormControl, ElementType.MenuItem, CategoryType.Element)
		{
			ContainerName = "MENUITEMS",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.Menu
		},
		new ElementRelationExport(ElementType.FormControl, ElementType.OnValidate, CategoryType.Trigger),
		new ElementRelationExport(ElementType.FormControl, ElementType.OnLookup, CategoryType.Trigger),
		new ElementRelationExport(ElementType.FormControl, ElementType.OnDrillDown, CategoryType.Trigger),
		new ElementRelationExport(ElementType.FormControl, ElementType.OnAssistEdit, CategoryType.Trigger),
		new ElementRelationExport(ElementType.FormControl, ElementType.OnPush, CategoryType.Trigger),
		new ElementRelationExport(ElementType.FormControl, ElementType.OnFormat, CategoryType.Trigger),
		new ElementRelationExport(ElementType.FormControl, ElementType.OnAfterValidate, CategoryType.Trigger),
		new ElementRelationExport(ElementType.FormControl, ElementType.OnDeactivate, CategoryType.Trigger),
		new ElementRelationExport(ElementType.FormControl, ElementType.OnActivate, CategoryType.Trigger),
		new ElementRelationExport(ElementType.XmlPortElement, ElementType.ImportOnAfterAssignVariable, CategoryType.Trigger)
		{
			AttributeId = 54126
		},
		new ElementRelationExport(ElementType.XmlPortElement, ElementType.ExportOnBeforePassVariable, CategoryType.Trigger)
		{
			AttributeId = 54127
		},
		new ElementRelationExport(ElementType.XmlPortElement, ElementType.ImportOnAfterAssignField, CategoryType.Trigger)
		{
			AttributeId = 54146
		},
		new ElementRelationExport(ElementType.XmlPortElement, ElementType.ExportOnBeforePassField, CategoryType.Trigger)
		{
			AttributeId = 54147
		},
		new ElementRelationExport(ElementType.XmlPortElement, ElementType.ImportOnAfterInitRecord, CategoryType.Trigger)
		{
			ExportOrder = 100,
			AttributeId = 54136
		},
		new ElementRelationExport(ElementType.XmlPortElement, ElementType.ImportOnBeforeInsertRecord, CategoryType.Trigger)
		{
			ExportOrder = 200,
			AttributeId = 54137
		},
		new ElementRelationExport(ElementType.XmlPortElement, ElementType.ExportOnPreXmlItem, CategoryType.Trigger)
		{
			ExportOrder = 250,
			AttributeId = 54138
		},
		new ElementRelationExport(ElementType.XmlPortElement, ElementType.ExportOnAfterGetRecord, CategoryType.Trigger)
		{
			ExportOrder = 300,
			AttributeId = 54139
		},
		new ElementRelationExport(ElementType.XmlPortElement, ElementType.ImportOnAfterInsertRecord, CategoryType.Trigger)
		{
			ExportOrder = 400,
			AttributeId = 54140
		},
		new ElementRelationExport(ElementType.XmlPortElement, ElementType.ImportOnBeforeModifyRecord, CategoryType.Trigger)
		{
			ExportOrder = 500,
			AttributeId = 54141
		},
		new ElementRelationExport(ElementType.XmlPortElement, ElementType.ImportOnAfterModifyRecord, CategoryType.Trigger)
		{
			ExportOrder = 600,
			AttributeId = 54142
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.Control, CategoryType.Element)
		{
			ContainerName = "CONTROLS",
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.Action, CategoryType.Element)
		{
			ContainerName = "ACTIONS",
			Multiplicity = MultiplicityType.ZeroToMany,
			ContainerProperty = PropertyType.ActionList,
			AttributeId = 55242
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.OnInit, CategoryType.Trigger)
		{
			AttributeId = 55140
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.OnOpenPage, CategoryType.Trigger)
		{
			AttributeId = 55141
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.OnClosePage, CategoryType.Trigger)
		{
			ExportOrder = 100,
			AttributeId = 55142
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.OnFindRecord, CategoryType.Trigger)
		{
			ExportOrder = 200,
			AttributeId = 55143
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.OnNextRecord, CategoryType.Trigger)
		{
			ExportOrder = 300,
			AttributeId = 55144
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.OnAfterGetRecord, CategoryType.Trigger)
		{
			ExportOrder = 1000,
			AttributeId = 55145
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.OnNewRecord, CategoryType.Trigger)
		{
			ExportOrder = 1050,
			AttributeId = 55146
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.OnInsertRecord, CategoryType.Trigger)
		{
			ExportOrder = 1060,
			AttributeId = 55147
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.OnModifyRecord, CategoryType.Trigger)
		{
			ExportOrder = 1065,
			AttributeId = 55148
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.OnDeleteRecord, CategoryType.Trigger)
		{
			ExportOrder = 1070,
			AttributeId = 55149
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.OnQueryClosePage, CategoryType.Trigger)
		{
			ExportOrder = 1080,
			AttributeId = 55150
		},
		new ElementRelationExport(ElementType.RequestPage, ElementType.OnAfterGetCurrRecord, CategoryType.Trigger)
		{
			ExportOrder = 1100,
			AttributeId = 55151
		},
		new ElementRelationExport(ElementType.OnPostSection, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnInitReport, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Procedure, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Procedure, ElementType.Parameter, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Procedure, ElementType.ReturnValue, CategoryType.Identifier),
		new ElementRelationExport(ElementType.OnPostReport, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnPostDataItem, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnDelete, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnInsert, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnModify, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnRename, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnValidate, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnLookup, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnRun, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnAction, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnInit, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnOpenPage, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnDrillDown, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnAfterGetRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnNewRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnFindRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnAssistEdit, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnModifyRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnInsertRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnDeleteRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnClosePage, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnQueryClosePage, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnNextRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnOpenForm, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnPush, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnFormat, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnAfterGetCurrRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnAfterValidate, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnDeactivate, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnBeforePutRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Event, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Event, ElementType.Parameter, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Event, ElementType.ReturnValue, CategoryType.Identifier),
		new ElementRelationExport(ElementType.OnAfterInput, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnActivateForm, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnActivate, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnCloseForm, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnQueryCloseForm, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnBeforeInput, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnTimer, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnDeactivateForm, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnBeforeExportRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnPreDataItem, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnBeforeImportRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnAfterImportRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnPreReport, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnPreSection, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.ImportOnAfterAssignVariable, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.ImportOnBeforeInsertRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.ExportOnAfterGetRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.ImportOnAfterInsertRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.ExportOnBeforePassVariable, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.ExportOnPreXmlItem, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnPreXmlPort, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnPostXmlPort, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.ImportOnAfterAssignField, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnPreDataport, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.Label, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnControlAddIn, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.ExportOnBeforePassField, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnInitXmlPort, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.ImportOnAfterInitRecord, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		},
		new ElementRelationExport(ElementType.OnDataPointClicked, ElementType.Variable, CategoryType.Identifier)
		{
			Multiplicity = MultiplicityType.ZeroToMany
		}
	};

	public ElementType ParentType { get; private set; }

	public ElementType ChildType { get; private set; }

	public CategoryType ChildCategory { get; private set; }

	public string ContainerName { get; private set; }

	public MultiplicityType Multiplicity { get; set; }

	public bool SkipIfEmpty { get; private set; }

	public int ExportOrder { get; private set; }

	public PropertyType ContainerProperty { get; private set; }

	public int AttributeId { get; set; }

	public bool Obsolete { get; private set; }

	public bool Unsupported { get; private set; }

	public bool MultiplicityIsSingle
	{
		get
		{
			if (Multiplicity != MultiplicityType.ZeroToOne)
			{
				return Multiplicity == MultiplicityType.One;
			}
			return true;
		}
	}

	public ElementTypeInfo Parent => ElementTypeInfo.Lookup.GetValue(ParentType);

	public ElementTypeInfo Child => ElementTypeInfo.Lookup.GetValue(ChildType);

	internal ElementRelationExport(ElementType parentType, ElementType childType, CategoryType category)
	{
		ParentType = parentType;
		ChildType = childType;
		ChildCategory = category;
		Multiplicity = MultiplicityType.ZeroToOne;
	}
}
