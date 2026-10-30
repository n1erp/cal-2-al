using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal static class AlExporterLists
{
	internal static readonly HashSet<PropertyType> IdentifierTextProperties = new HashSet<PropertyType>
	{
		PropertyType.AccessByPermission,
		PropertyType.ApplicationArea,
		PropertyType.AutoFormatType,
		PropertyType.AutoFormatExpression,
		PropertyType.CalcFields,
		PropertyType.CalcFormula,
		PropertyType.CardPageId,
		PropertyType.CaptionClass,
		PropertyType.ChartPartId,
		PropertyType.ColumnFilter,
		PropertyType.DataCaptionExpression,
		PropertyType.DataCaptionFields,
		PropertyType.DataItemLink,
		PropertyType.DataItemTable,
		PropertyType.DataItemTableFilter,
		PropertyType.DataItemTableView,
		PropertyType.DataSetFieldName,
		PropertyType.DataSource,
		PropertyType.EventFunction,
		PropertyType.EventSubscriberInstance,
		PropertyType.Fields,
		PropertyType.Format,
		PropertyType.FreezeColumnId,
		PropertyType.Key,
		PropertyType.KeyGroups,
		PropertyType.Image,
		PropertyType.IndentationColumnName,
		PropertyType.LinkFields,
		PropertyType.LinkTable,
		PropertyType.Layout,
		PropertyType.MinValue,
		PropertyType.MaxValue,
		PropertyType.Name,
		PropertyType.ODataKeyFields,
		PropertyType.PagePartId,
		PropertyType.PartType,
		PropertyType.Permissions,
		PropertyType.PromotedCategory,
		PropertyType.RunFormLink,
		PropertyType.RunFormView,
		PropertyType.RunObject,
		PropertyType.SiftLevelsToMaintain,
		PropertyType.StyleExpression,
		PropertyType.SourceTableView,
		PropertyType.SourceExpression,
		PropertyType.SqlDataType,
		PropertyType.SubpageLink,
		PropertyType.SubpageView,
		PropertyType.SubFormLink,
		PropertyType.SubFormView,
		PropertyType.TableIdExpression,
		PropertyType.TableRelation,
		PropertyType.TextType,
		PropertyType.TotalFields,
		PropertyType.WordMergeDataItem,
		PropertyType.DataItemLinkReference,
		PropertyType.DataSource,
		PropertyType.OrderBy,
		PropertyType.ValuesAllowed
	};

	internal static readonly HashSet<PropertyType> TemporarilyCommentedOutProperties = new HashSet<PropertyType> { PropertyType.TestTableRelation };

	internal static readonly HashSet<PropertyType> CommaSeperatedProperties = new HashSet<PropertyType>
	{
		PropertyType.DataCaptionFields,
		PropertyType.RequestFilterFields,
		PropertyType.Fields,
		PropertyType.IndentationControls,
		PropertyType.Key,
		PropertyType.ODataKeyFields,
		PropertyType.OptionString,
		PropertyType.SumIndexFields
	};

	internal static readonly HashSet<string> Keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"False", "True", "Div", "Mod", "And", "Or", "Xor", "Not", "Exit", "Begin",
		"Case", "Do", "DownTo", "Else", "End", "For", "ForEach", "If", "In", "Of",
		"Repeat", "Then", "To", "Until", "With", "While", "AssertError", "Var", "Break", "Trigger",
		"Program", "Procedure", "Function", "Event", "Var", "Array", "Local", "Codeunit", "Table", "Page",
		"Query", "XmlPort", "Report", "TableExtension", "PageExtension", "ControlAddIn", "Fields", "Field", "Keys", "Key",
		"FieldGroups", "FieldGroup", "Repeater", "CueGroup", "Action", "Actions", "Area", "Average", "Fixed", "RunOnClient",
		"Filter", "Const", "Sum", "Count", "None", "Min", "Max", "Object", "Layout", "Part",
		"Record", "Code", "Order", "Ascending", "Descending", "Temporary", "Integer", "Profile", "Schema"
	};
}
