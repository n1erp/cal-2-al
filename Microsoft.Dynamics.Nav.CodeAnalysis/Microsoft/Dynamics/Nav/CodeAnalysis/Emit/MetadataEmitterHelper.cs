using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class MetadataEmitterHelper
{
	internal static int? GetBoundFieldExpressionId(this BoundEqualsPropertyExpression expression)
	{
		BoundPropertyRhsExpression right = expression.Right;
		if (right.Value == null || right.Value.Kind != BoundKind.ConstOrFieldAccessValueExpression)
		{
			return null;
		}
		BoundConstOrFieldAccessValueExpression boundConstOrFieldAccessValueExpression = (BoundConstOrFieldAccessValueExpression)right.Value;
		if (boundConstOrFieldAccessValueExpression.Value.Kind != BoundKind.FieldAccess)
		{
			return null;
		}
		BoundFieldAccess boundFieldAccess = (BoundFieldAccess)boundConstOrFieldAccessValueExpression.Value;
		DebugAssertHelper.Assert(boundFieldAccess.FieldSymbol != null);
		return boundFieldAccess.FieldSymbol.Id;
	}

	internal static string GetFormatExpressionName(int controlId)
	{
		return string.Format(CultureInfo.InvariantCulture, "Control{0}_Format", controlId);
	}

	internal static string GetDataItemTableViewName(int dataItemIdOrIndex)
	{
		return string.Format(CultureInfo.InvariantCulture, "DataItem{0}TableView", dataItemIdOrIndex);
	}

	internal static string GetFieldPrefixedAttributeValues(IEnumerable<FieldSymbol> fields)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		int num = 0;
		foreach (FieldSymbol field in fields)
		{
			if (num > 0)
			{
				instance.Builder.Append(',');
			}
			instance.Builder.Append("Field").Append(field.Id);
			num++;
		}
		return instance.ToStringAndFree();
	}

	internal static string GetContainerType(AreaKind kind)
	{
		return kind switch
		{
			AreaKind.Content => "ContentArea", 
			AreaKind.FactBoxes => "FactBoxArea", 
			AreaKind.RoleCenter => "RoleCenterArea", 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	internal static string GetContainerType(ActionAreaKind kind)
	{
		return kind switch
		{
			ActionAreaKind.Creation => "NewDocumentItems", 
			ActionAreaKind.Embedding => "HomeItems", 
			ActionAreaKind.Navigation => "RelatedInformation", 
			ActionAreaKind.Processing => "ActionItems", 
			ActionAreaKind.Reporting => "Reports", 
			ActionAreaKind.Sections => "ActivityButtons", 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	internal static string GetControlType(ControlKind kind)
	{
		switch (kind)
		{
		case ControlKind.Group:
			return "ControlGroupDefinition";
		case ControlKind.Repeater:
			return "RepeaterDefinition";
		case ControlKind.Fixed:
			return "ColumnLayoutDefinition";
		case ControlKind.Grid:
			return "GridLayoutDefinition";
		case ControlKind.CueGroup:
			return "StackGroupDefinition";
		case ControlKind.Field:
		case ControlKind.Label:
		case ControlKind.UserControl:
			return "ControlDefinition";
		case ControlKind.Part:
			return "InfopartPageDefinition";
		case ControlKind.SystemPart:
			return "InfopartSystemDefinition";
		case ControlKind.ChartPart:
			return "InfopartChartDefinition";
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	internal static string GetSystemPartType(PageSystemPartKind kind)
	{
		DebugAssertHelper.Assert(kind != PageSystemPartKind.None);
		if (kind == PageSystemPartKind.Links)
		{
			return "RecordLinks";
		}
		return kind.ToString();
	}

	internal static bool IsIndentationColumnExpression(BoundExpression indentationColumn)
	{
		DebugAssertHelper.Assert(indentationColumn != null);
		if (indentationColumn.Kind != BoundKind.FieldAccess)
		{
			return true;
		}
		BoundFieldAccess boundFieldAccess = (BoundFieldAccess)indentationColumn;
		if (boundFieldAccess.ReceiverOpt == null || boundFieldAccess.ReceiverOpt.Kind != BoundKind.Global)
		{
			return true;
		}
		return ((BoundGlobal)boundFieldAccess.ReceiverOpt).GlobalSymbol.VariableKind != VariableKind.Synthesized;
	}

	internal static string GetRunObjectTypeMetadataValue(SymbolKind kind)
	{
		if (kind == SymbolKind.XmlPort)
		{
			return "XMLport";
		}
		return kind.ToString();
	}

	internal static string GenerateMemberId(int controlId, int applicationObjectId, SymbolKind applicationObjectKind, string format = "B")
	{
		return new Guid(applicationObjectId, (short)controlId, (controlId == 0) ? ((short)1) : ((short)0), (byte)applicationObjectKind, 0, 0, 0, 131, 107, 210, 210).ToString(format);
	}

	internal static string GeneratePageControlGuid(int controlId, int applicationObjectId, SymbolKind applicationObjectKind, string format = "B")
	{
		byte e = (byte)(controlId >> 24);
		byte f = (byte)(controlId >> 16);
		short b = (short)controlId;
		return new Guid(applicationObjectId, b, (controlId == 0) ? ((short)1) : ((short)0), (byte)applicationObjectKind, e, f, 0, 131, 107, 210, 210).ToString(format);
	}

	internal static string GetSemanticKind(ControlSymbol controlSymbol)
	{
		if (PageMemberSymbolHelpers.IsContainedInGivenControlArea(controlSymbol, AreaKind.FactBoxes))
		{
			return "FactBoxes";
		}
		return "Content";
	}

	internal static string GetSemanticKind(ActionSymbol actionSymbol)
	{
		return "Action";
	}

	internal static string GetSemanticKind(ViewSymbol viewSymbol)
	{
		return "View";
	}
}
