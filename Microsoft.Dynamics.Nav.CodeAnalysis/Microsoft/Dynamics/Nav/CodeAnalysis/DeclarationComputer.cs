using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class DeclarationComputer
{
	public static void ComputeDeclarationsInSpan(SemanticModel model, TextSpan span, bool getSymbol, List<DeclarationInfo> builder, CancellationToken cancellationToken)
	{
		ComputeDeclarations(model, model.SyntaxTree.GetRoot(cancellationToken), (SyntaxNode node, int? level) => !node.Span.OverlapsWith(span) || InvalidLevel(level), getSymbol, builder, null, cancellationToken);
	}

	public static void ComputeDeclarationsInNode(SemanticModel model, SyntaxNode node, bool getSymbol, List<DeclarationInfo> builder, CancellationToken cancellationToken, int? levelsToCompute = null)
	{
		ComputeDeclarations(model, node, (SyntaxNode n, int? level) => InvalidLevel(level), getSymbol, builder, levelsToCompute, cancellationToken);
	}

	internal static DeclarationInfo GetDeclarationInfo(SemanticModel model, SyntaxNode node, bool getSymbol, IEnumerable<SyntaxNode> executableCodeBlocks, CancellationToken cancellationToken)
	{
		ISymbol declaredSymbol = GetDeclaredSymbol(model, node, getSymbol, cancellationToken);
		ImmutableArray<SyntaxNode> executableCodeBlocks2 = executableCodeBlocks?.Where((SyntaxNode c) => c != null).ToImmutableArrayOrEmpty() ?? ImmutableArray<SyntaxNode>.Empty;
		return new DeclarationInfo(node, executableCodeBlocks2, declaredSymbol);
	}

	internal static DeclarationInfo GetDeclarationInfo(SemanticModel model, SyntaxNode node, bool getSymbol, CancellationToken cancellationToken)
	{
		return GetDeclarationInfo(model, node, getSymbol, (IEnumerable<SyntaxNode>)null, cancellationToken);
	}

	internal static DeclarationInfo GetDeclarationInfo(SemanticModel model, SyntaxNode node, bool getSymbol, SyntaxNode executableCodeBlock, CancellationToken cancellationToken)
	{
		return GetDeclarationInfo(model, node, getSymbol, SpecializedCollections.SingletonEnumerable(executableCodeBlock), cancellationToken);
	}

	internal static DeclarationInfo GetDeclarationInfo(SemanticModel model, SyntaxNode node, bool getSymbol, CancellationToken cancellationToken, params SyntaxNode[] executableCodeBlocks)
	{
		return GetDeclarationInfo(model, node, getSymbol, executableCodeBlocks.AsEnumerable(), cancellationToken);
	}

	private static ISymbol GetDeclaredSymbol(SemanticModel model, SyntaxNode node, bool getSymbol, CancellationToken cancellationToken)
	{
		if (!getSymbol)
		{
			return null;
		}
		return model.GetDeclaredSymbol(node, cancellationToken);
	}

	private static bool InvalidLevel(int? level)
	{
		if (level.HasValue)
		{
			return level.Value <= 0;
		}
		return false;
	}

	private static int? DecrementLevel(int? level)
	{
		if (!level.HasValue)
		{
			return level;
		}
		return level - 1;
	}

	[SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
	private static void ComputeDeclarations(SemanticModel model, SyntaxNode node, Func<SyntaxNode, int?, bool> shouldSkip, bool getSymbol, List<DeclarationInfo> builder, int? levelsToCompute, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (shouldSkip(node, levelsToCompute))
		{
			return;
		}
		int? num = DecrementLevel(levelsToCompute);
		switch (node.Kind)
		{
		case SyntaxKind.VarSection:
			foreach (VariableDeclarationSyntax variable in ((VarSectionSyntax)node).Variables)
			{
				builder.Add(GetDeclarationInfo(model, variable, getSymbol, cancellationToken));
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.VariableDeclaration:
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.PageLayout:
			foreach (PageAreaSyntax area in ((PageLayoutSyntax)node).Areas)
			{
				ComputeDeclarations(model, area, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.PageArea:
		{
			PageAreaSyntax pageAreaSyntax = (PageAreaSyntax)node;
			foreach (ControlBaseSyntax control in pageAreaSyntax.Controls)
			{
				ComputeDeclarations(model, control, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			foreach (PropertySyntaxOrEmpty property in pageAreaSyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		}
		case SyntaxKind.PageGroup:
		{
			PageGroupSyntax pageGroupSyntax = (PageGroupSyntax)node;
			foreach (ControlBaseSyntax control2 in pageGroupSyntax.Controls)
			{
				ComputeDeclarations(model, control2, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			foreach (PropertySyntaxOrEmpty property2 in pageGroupSyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property2, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		}
		case SyntaxKind.PageField:
		{
			PageFieldSyntax pageFieldSyntax = (PageFieldSyntax)node;
			foreach (PropertySyntaxOrEmpty property3 in pageFieldSyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property3, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			foreach (TriggerDeclarationSyntax trigger in pageFieldSyntax.Triggers)
			{
				ComputeDeclarations(model, trigger, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, pageFieldSyntax.Expression, cancellationToken));
			break;
		}
		case SyntaxKind.PageAction:
		{
			PageActionSyntax pageActionSyntax = (PageActionSyntax)node;
			foreach (PropertySyntaxOrEmpty property4 in pageActionSyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property4, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			foreach (TriggerDeclarationSyntax trigger2 in pageActionSyntax.Triggers)
			{
				ComputeDeclarations(model, trigger2, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		}
		case SyntaxKind.PageActionList:
		{
			PageActionListSyntax pageActionListSyntax = (PageActionListSyntax)node;
			foreach (PageActionAreaSyntax area2 in pageActionListSyntax.Areas)
			{
				ComputeDeclarations(model, area2, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, pageActionListSyntax, getSymbol, cancellationToken));
			break;
		}
		case SyntaxKind.PageActionGroup:
		case SyntaxKind.PageActionArea:
		{
			ActionGroupBaseSyntax actionGroupBaseSyntax = (ActionGroupBaseSyntax)node;
			foreach (PropertySyntaxOrEmpty property5 in actionGroupBaseSyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property5, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			foreach (ActionBaseSyntax action in actionGroupBaseSyntax.Actions)
			{
				ComputeDeclarations(model, action, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		}
		case SyntaxKind.PageView:
			foreach (PropertySyntaxOrEmpty property6 in ((PageViewSyntax)node).PropertyList.Properties)
			{
				ComputeDeclarations(model, property6, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.PageViewList:
		{
			PageViewListSyntax pageViewListSyntax = (PageViewListSyntax)node;
			foreach (PageViewSyntax view in pageViewListSyntax.Views)
			{
				ComputeDeclarations(model, view, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, pageViewListSyntax, getSymbol, cancellationToken));
			break;
		}
		case SyntaxKind.FieldList:
			foreach (FieldSyntax field in ((FieldListSyntax)node).Fields)
			{
				ComputeDeclarations(model, field, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.Field:
		{
			FieldSyntax fieldSyntax = (FieldSyntax)node;
			foreach (TriggerDeclarationSyntax trigger3 in fieldSyntax.Triggers)
			{
				ComputeDeclarations(model, trigger3, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			foreach (PropertySyntaxOrEmpty property7 in fieldSyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property7, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		}
		case SyntaxKind.FieldExtensionList:
			foreach (FieldBaseSyntax field2 in ((FieldExtensionListSyntax)node).Fields)
			{
				ComputeDeclarations(model, field2, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.FieldModification:
		{
			FieldModificationSyntax fieldModificationSyntax = (FieldModificationSyntax)node;
			foreach (TriggerDeclarationSyntax trigger4 in fieldModificationSyntax.Triggers)
			{
				ComputeDeclarations(model, trigger4, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			foreach (PropertySyntaxOrEmpty property8 in fieldModificationSyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property8, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		}
		case SyntaxKind.KeyList:
			foreach (KeySyntax key in ((KeyListSyntax)node).Keys)
			{
				ComputeDeclarations(model, key, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.Key:
		{
			KeySyntax keySyntax = (KeySyntax)node;
			foreach (PropertySyntaxOrEmpty property9 in keySyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property9, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, keySyntax.Fields, cancellationToken));
			break;
		}
		case SyntaxKind.FieldGroupList:
			foreach (FieldGroupSyntax fieldGroup in ((FieldGroupListSyntax)node).FieldGroups)
			{
				ComputeDeclarations(model, fieldGroup, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.ReportDataSetSection:
			foreach (ReportDataItemSyntax dataItem in ((ReportDataSetSectionSyntax)node).DataItems)
			{
				ComputeDeclarations(model, dataItem, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.ReportDataItem:
		{
			ReportDataItemSyntax reportDataItemSyntax = (ReportDataItemSyntax)node;
			foreach (ReportDataItemElementSyntax element in reportDataItemSyntax.Elements)
			{
				ComputeDeclarations(model, element, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			foreach (TriggerDeclarationSyntax trigger5 in reportDataItemSyntax.Triggers)
			{
				ComputeDeclarations(model, trigger5, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		}
		case SyntaxKind.ReportColumn:
		{
			ReportColumnSyntax reportColumnSyntax = (ReportColumnSyntax)node;
			builder.Add(GetDeclarationInfo(model, node, getSymbol, reportColumnSyntax.SourceExpression, cancellationToken));
			break;
		}
		case SyntaxKind.ReportLabelsSection:
			foreach (ReportLabelBaseSyntax label in ((ReportLabelsSectionSyntax)node).Labels)
			{
				builder.Add(GetDeclarationInfo(model, label, getSymbol, cancellationToken));
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.PageExtensionLayout:
			foreach (ControlChangeBaseSyntax change in ((PageExtensionLayoutSyntax)node).Changes)
			{
				builder.Add(GetDeclarationInfo(model, change, getSymbol, cancellationToken));
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.PageExtensionActionList:
			foreach (ActionChangeBaseSyntax change2 in ((PageExtensionActionListSyntax)node).Changes)
			{
				builder.Add(GetDeclarationInfo(model, change2, getSymbol, cancellationToken));
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.PageExtensionViewList:
			foreach (ViewChangeBaseSyntax change3 in ((PageExtensionViewListSyntax)node).Changes)
			{
				builder.Add(GetDeclarationInfo(model, change3, getSymbol, cancellationToken));
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.XmlPortFieldElement:
		case SyntaxKind.XmlPortFieldAttribute:
		{
			XmlPortFieldNodeSyntax xmlPortFieldNodeSyntax = (XmlPortFieldNodeSyntax)node;
			XmlPortNodeDeclaration(model, shouldSkip, getSymbol, builder, cancellationToken, xmlPortFieldNodeSyntax, num);
			builder.Add(GetDeclarationInfo(model, node, getSymbol, xmlPortFieldNodeSyntax.SourceField, cancellationToken));
			break;
		}
		case SyntaxKind.XmlPortTextElement:
		case SyntaxKind.XmlPortTextAttribute:
		{
			XmlPortTextNodeSyntax xmlPortNode = (XmlPortTextNodeSyntax)node;
			XmlPortNodeDeclaration(model, shouldSkip, getSymbol, builder, cancellationToken, xmlPortNode, num);
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		}
		case SyntaxKind.XmlPortTableElement:
		{
			XmlPortTableElementSyntax xmlPortTableElementSyntax = (XmlPortTableElementSyntax)node;
			XmlPortNodeDeclaration(model, shouldSkip, getSymbol, builder, cancellationToken, xmlPortTableElementSyntax, num);
			builder.Add(GetDeclarationInfo(model, node, getSymbol, xmlPortTableElementSyntax.SourceTable, cancellationToken));
			break;
		}
		case SyntaxKind.Property:
		{
			PropertySyntax propertySyntax = (PropertySyntax)node;
			switch (propertySyntax.Value.Kind)
			{
			case SyntaxKind.BooleanExpressionPropertyValue:
			case SyntaxKind.ClientSideBooleanExpressionPropertyValue:
			case SyntaxKind.IntegerExpressionPropertyValue:
			case SyntaxKind.TextExpressionPropertyValue:
			case SyntaxKind.StyleExpressionPropertyValue:
			{
				ExpressionPropertyValueSyntax expressionPropertyValueSyntax = (ExpressionPropertyValueSyntax)propertySyntax.Value;
				builder.Add(GetDeclarationInfo(model, node, getSymbol, expressionPropertyValueSyntax.Expression, cancellationToken));
				break;
			}
			case SyntaxKind.QueryDataItemLinkPropertyValue:
			{
				QueryDataItemLinkPropertyValueSyntax queryDataItemLinkPropertyValueSyntax = (QueryDataItemLinkPropertyValueSyntax)propertySyntax.Value;
				builder.Add(GetDeclarationInfo(model, node, getSymbol, queryDataItemLinkPropertyValueSyntax.Joins.Node, cancellationToken));
				break;
			}
			case SyntaxKind.BooleanPropertyValue:
			{
				BooleanPropertyValueSyntax booleanPropertyValueSyntax = (BooleanPropertyValueSyntax)propertySyntax.Value;
				builder.Add(GetDeclarationInfo(model, node, getSymbol, booleanPropertyValueSyntax.Value, cancellationToken));
				break;
			}
			case SyntaxKind.DatePropertyValue:
			{
				DatePropertyValueSyntax datePropertyValueSyntax = (DatePropertyValueSyntax)propertySyntax.Value;
				builder.Add(GetDeclarationInfo(model, node, getSymbol, datePropertyValueSyntax.Value, cancellationToken));
				break;
			}
			case SyntaxKind.DateTimePropertyValue:
			{
				DateTimePropertyValueSyntax dateTimePropertyValueSyntax = (DateTimePropertyValueSyntax)propertySyntax.Value;
				builder.Add(GetDeclarationInfo(model, node, getSymbol, dateTimePropertyValueSyntax.Value, cancellationToken));
				break;
			}
			case SyntaxKind.DecimalPropertyValue:
			{
				DecimalPropertyValueSyntax decimalPropertyValueSyntax = (DecimalPropertyValueSyntax)propertySyntax.Value;
				builder.Add(GetDeclarationInfo(model, node, getSymbol, decimalPropertyValueSyntax.Value, cancellationToken));
				break;
			}
			case SyntaxKind.Int32PropertyValue:
			{
				Int32PropertyValueSyntax int32PropertyValueSyntax = (Int32PropertyValueSyntax)propertySyntax.Value;
				builder.Add(GetDeclarationInfo(model, node, getSymbol, int32PropertyValueSyntax.Value, cancellationToken));
				break;
			}
			case SyntaxKind.Int64PropertyValue:
			{
				Int64PropertyValueSyntax int64PropertyValueSyntax = (Int64PropertyValueSyntax)propertySyntax.Value;
				builder.Add(GetDeclarationInfo(model, node, getSymbol, int64PropertyValueSyntax.Value, cancellationToken));
				break;
			}
			case SyntaxKind.OptionValuesPropertyValue:
			{
				OptionValuesPropertyValueSyntax optionValuesPropertyValueSyntax = (OptionValuesPropertyValueSyntax)propertySyntax.Value;
				builder.Add(GetDeclarationInfo(model, node, getSymbol, optionValuesPropertyValueSyntax.Value, cancellationToken));
				break;
			}
			case SyntaxKind.StringPropertyValue:
			{
				StringPropertyValueSyntax stringPropertyValueSyntax = (StringPropertyValueSyntax)propertySyntax.Value;
				builder.Add(GetDeclarationInfo(model, node, getSymbol, stringPropertyValueSyntax.Value, cancellationToken));
				break;
			}
			case SyntaxKind.TimePropertyValue:
			{
				TimePropertyValueSyntax timePropertyValueSyntax = (TimePropertyValueSyntax)propertySyntax.Value;
				builder.Add(GetDeclarationInfo(model, node, getSymbol, timePropertyValueSyntax.Value, cancellationToken));
				break;
			}
			case SyntaxKind.ObjectReferencePropertyValue:
			{
				ObjectReferencePropertyValueSyntax objectReferencePropertyValueSyntax = (ObjectReferencePropertyValueSyntax)propertySyntax.Value;
				builder.Add(GetDeclarationInfo(model, node, getSymbol, objectReferencePropertyValueSyntax.ObjectNameOrId, cancellationToken));
				break;
			}
			case SyntaxKind.CommaSeparatedPropertyValue:
			{
				CommaSeparatedPropertyValueSyntax commaSeparatedPropertyValueSyntax = (CommaSeparatedPropertyValueSyntax)propertySyntax.Value;
				builder.Add(GetDeclarationInfo(model, node, getSymbol, commaSeparatedPropertyValueSyntax.Values, cancellationToken));
				break;
			}
			}
			break;
		}
		case SyntaxKind.MemberAttribute:
		{
			MemberAttributeSyntax memberAttributeSyntax = (MemberAttributeSyntax)node;
			builder.Add(GetDeclarationInfo(model, node, getSymbol, memberAttributeSyntax.ArgumentList, cancellationToken));
			break;
		}
		case SyntaxKind.EventDeclaration:
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.TriggerDeclaration:
		case SyntaxKind.MethodDeclaration:
			ComputeDeclaration(model, node, shouldSkip, getSymbol, builder, num, cancellationToken);
			break;
		case SyntaxKind.PageObject:
		{
			PageSyntax pageSyntax = (PageSyntax)node;
			if (pageSyntax.Layout != null)
			{
				ComputeDeclarations(model, pageSyntax.Layout, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			if (pageSyntax.Actions != null)
			{
				ComputeDeclarations(model, pageSyntax.Actions, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			if (pageSyntax.Views != null)
			{
				ComputeDeclarations(model, pageSyntax.Views, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			AddObjectDeclaration(model, pageSyntax, shouldSkip, getSymbol, builder, cancellationToken, num);
			break;
		}
		case SyntaxKind.PageExtensionObject:
		{
			PageExtensionSyntax pageExtensionSyntax = (PageExtensionSyntax)node;
			if (pageExtensionSyntax.Layout != null)
			{
				ComputeDeclarations(model, pageExtensionSyntax.Layout, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			if (pageExtensionSyntax.Actions != null)
			{
				ComputeDeclarations(model, pageExtensionSyntax.Actions, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			if (pageExtensionSyntax.Views != null)
			{
				ComputeDeclarations(model, pageExtensionSyntax.Views, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			AddObjectDeclaration(model, pageExtensionSyntax, shouldSkip, getSymbol, builder, cancellationToken, num);
			break;
		}
		case SyntaxKind.TableObject:
		{
			TableSyntax tableSyntax = (TableSyntax)node;
			if (tableSyntax.Fields != null)
			{
				ComputeDeclarations(model, tableSyntax.Fields, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			if (tableSyntax.Keys != null)
			{
				ComputeDeclarations(model, tableSyntax.Keys, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			if (tableSyntax.FieldGroups != null)
			{
				ComputeDeclarations(model, tableSyntax.FieldGroups, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			AddObjectDeclaration(model, tableSyntax, shouldSkip, getSymbol, builder, cancellationToken, num);
			break;
		}
		case SyntaxKind.ReportObject:
		{
			ReportSyntax reportSyntax = (ReportSyntax)node;
			if (reportSyntax.DataSet != null)
			{
				ComputeDeclarations(model, reportSyntax.DataSet, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			if (reportSyntax.Labels != null)
			{
				ComputeDeclarations(model, reportSyntax.Labels, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			if (reportSyntax.RequestPage != null)
			{
				ComputeDeclarations(model, reportSyntax.RequestPage, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			AddObjectDeclaration(model, reportSyntax, shouldSkip, getSymbol, builder, cancellationToken, num);
			break;
		}
		case SyntaxKind.RequestPage:
		{
			RequestPageSyntax requestPageSyntax = (RequestPageSyntax)node;
			if (requestPageSyntax.Layout != null)
			{
				ComputeDeclarations(model, requestPageSyntax.Layout, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			if (requestPageSyntax.Actions != null)
			{
				ComputeDeclarations(model, requestPageSyntax.Actions, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			AddObjectDeclaration(model, requestPageSyntax, shouldSkip, getSymbol, builder, cancellationToken, num);
			break;
		}
		case SyntaxKind.QueryObject:
		{
			QuerySyntax querySyntax = (QuerySyntax)node;
			if (querySyntax.Elements != null)
			{
				ComputeDeclarations(model, querySyntax.Elements, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			AddObjectDeclaration(model, querySyntax, shouldSkip, getSymbol, builder, cancellationToken, num);
			break;
		}
		case SyntaxKind.QueryElements:
			foreach (QueryDataItemSyntax dataItem2 in ((QueryElementsSyntax)node).DataItems)
			{
				ComputeDeclarations(model, dataItem2, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.QueryColumn:
		{
			QueryColumnSyntax queryColumnSyntax = (QueryColumnSyntax)node;
			foreach (PropertySyntaxOrEmpty property10 in queryColumnSyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property10, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, queryColumnSyntax.RelatedField, cancellationToken));
			break;
		}
		case SyntaxKind.QueryFilter:
		{
			QueryFilterSyntax queryFilterSyntax = (QueryFilterSyntax)node;
			foreach (PropertySyntaxOrEmpty property11 in queryFilterSyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property11, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, queryFilterSyntax.RelatedField, cancellationToken));
			break;
		}
		case SyntaxKind.QueryDataItem:
		{
			QueryDataItemSyntax queryDataItemSyntax = (QueryDataItemSyntax)node;
			foreach (PropertySyntaxOrEmpty property12 in queryDataItemSyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property12, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			foreach (QueryDataItemElementSyntax element2 in queryDataItemSyntax.Elements)
			{
				ComputeDeclarations(model, element2, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, queryDataItemSyntax.DataItemTable, cancellationToken));
			break;
		}
		case SyntaxKind.XmlPortObject:
		{
			XmlPortSyntax xmlPortSyntax = (XmlPortSyntax)node;
			if (xmlPortSyntax.XmlPortSchema != null)
			{
				ComputeDeclarations(model, xmlPortSyntax.XmlPortSchema, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			if (xmlPortSyntax.XmlPortRequestPage != null)
			{
				ComputeDeclarations(model, xmlPortSyntax.XmlPortRequestPage, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			AddObjectDeclaration(model, xmlPortSyntax, shouldSkip, getSymbol, builder, cancellationToken, num);
			break;
		}
		case SyntaxKind.XmlPortSchema:
			foreach (XmlPortNodeSyntax item in ((XmlPortSchemaSyntax)node).XmlPortSchema)
			{
				ComputeDeclarations(model, item, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
			break;
		case SyntaxKind.ControlAddChange:
		{
			ControlAddChangeSyntax controlAddChangeSyntax = (ControlAddChangeSyntax)node;
			foreach (ControlBaseSyntax control3 in controlAddChangeSyntax.Controls)
			{
				ComputeDeclarations(model, control3, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, controlAddChangeSyntax.Anchor, cancellationToken));
			break;
		}
		case SyntaxKind.ControlMoveChange:
		{
			ControlMoveChangeSyntax controlMoveChangeSyntax = (ControlMoveChangeSyntax)node;
			SeparatedSyntaxList<IdentifierNameSyntax> controls = controlMoveChangeSyntax.Controls;
			controls.Add(controlMoveChangeSyntax.Anchor);
			builder.Add(GetDeclarationInfo(model, node, getSymbol, controls, cancellationToken));
			break;
		}
		case SyntaxKind.ControlModifyChange:
		{
			ControlModifyChangeSyntax controlModifyChangeSyntax = (ControlModifyChangeSyntax)node;
			foreach (PropertySyntaxOrEmpty property13 in controlModifyChangeSyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property13, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			foreach (TriggerDeclarationSyntax trigger6 in controlModifyChangeSyntax.Triggers)
			{
				ComputeDeclarations(model, trigger6, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, controlModifyChangeSyntax.Name, cancellationToken));
			break;
		}
		case SyntaxKind.ActionAddChange:
		{
			ActionAddChangeSyntax actionAddChangeSyntax = (ActionAddChangeSyntax)node;
			foreach (ActionBaseSyntax action2 in actionAddChangeSyntax.Actions)
			{
				ComputeDeclarations(model, action2, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, actionAddChangeSyntax.Anchor, cancellationToken));
			break;
		}
		case SyntaxKind.ActionMoveChange:
		{
			ActionMoveChangeSyntax actionMoveChangeSyntax = (ActionMoveChangeSyntax)node;
			SeparatedSyntaxList<IdentifierNameSyntax> actions = actionMoveChangeSyntax.Actions;
			actions.Add(actionMoveChangeSyntax.Anchor);
			builder.Add(GetDeclarationInfo(model, node, getSymbol, actions, cancellationToken));
			break;
		}
		case SyntaxKind.ActionModifyChange:
		{
			ActionModifyChangeSyntax actionModifyChangeSyntax = (ActionModifyChangeSyntax)node;
			foreach (PropertySyntaxOrEmpty property14 in actionModifyChangeSyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property14, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			foreach (TriggerDeclarationSyntax trigger7 in actionModifyChangeSyntax.Triggers)
			{
				ComputeDeclarations(model, trigger7, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, actionModifyChangeSyntax.Name, cancellationToken));
			break;
		}
		case SyntaxKind.ViewAddChange:
		{
			ViewAddChangeSyntax viewAddChangeSyntax = (ViewAddChangeSyntax)node;
			foreach (PageViewSyntax view2 in viewAddChangeSyntax.Views)
			{
				ComputeDeclarations(model, view2, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, viewAddChangeSyntax.Anchor, cancellationToken));
			break;
		}
		case SyntaxKind.ViewMoveChange:
		{
			ViewMoveChangeSyntax viewMoveChangeSyntax = (ViewMoveChangeSyntax)node;
			SeparatedSyntaxList<IdentifierNameSyntax> views = viewMoveChangeSyntax.Views;
			views.Add(viewMoveChangeSyntax.Anchor);
			builder.Add(GetDeclarationInfo(model, node, getSymbol, views, cancellationToken));
			break;
		}
		case SyntaxKind.ViewModifyChange:
		{
			ViewModifyChangeSyntax viewModifyChangeSyntax = (ViewModifyChangeSyntax)node;
			foreach (PropertySyntaxOrEmpty property15 in viewModifyChangeSyntax.PropertyList.Properties)
			{
				ComputeDeclarations(model, property15, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			builder.Add(GetDeclarationInfo(model, node, getSymbol, viewModifyChangeSyntax.Name, cancellationToken));
			break;
		}
		case SyntaxKind.TableExtensionObject:
		{
			TableExtensionSyntax tableExtensionSyntax = (TableExtensionSyntax)node;
			if (tableExtensionSyntax.Fields != null)
			{
				ComputeDeclarations(model, tableExtensionSyntax.Fields, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			if (tableExtensionSyntax.Keys != null)
			{
				ComputeDeclarations(model, tableExtensionSyntax.Keys, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			AddObjectDeclaration(model, tableExtensionSyntax, shouldSkip, getSymbol, builder, cancellationToken, num);
			break;
		}
		case SyntaxKind.PageCustomizationObject:
		{
			PageCustomizationSyntax pageCustomizationSyntax = (PageCustomizationSyntax)node;
			if (pageCustomizationSyntax.Layout != null)
			{
				ComputeDeclarations(model, pageCustomizationSyntax.Layout, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			if (pageCustomizationSyntax.Actions != null)
			{
				ComputeDeclarations(model, pageCustomizationSyntax.Actions, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			if (pageCustomizationSyntax.Views != null)
			{
				ComputeDeclarations(model, pageCustomizationSyntax.Views, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			AddObjectDeclaration(model, pageCustomizationSyntax, shouldSkip, getSymbol, builder, cancellationToken, num);
			break;
		}
		case SyntaxKind.CodeunitObject:
		case SyntaxKind.ControlAddInObject:
		case SyntaxKind.ProfileObject:
		case SyntaxKind.DotNetPackage:
		{
			ObjectSyntax node2 = (ObjectSyntax)node;
			AddObjectDeclaration(model, node2, shouldSkip, getSymbol, builder, cancellationToken, num);
			break;
		}
		case SyntaxKind.CompilationUnit:
			foreach (ObjectSyntax @object in ((ObjectCompilationUnitSyntax)node).Objects)
			{
				ComputeDeclarations(model, @object, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			break;
		case SyntaxKind.EnumType:
		{
			EnumTypeSyntax enumTypeSyntax = (EnumTypeSyntax)node;
			_ = enumTypeSyntax.Values;
			foreach (EnumValueSyntax value in enumTypeSyntax.Values)
			{
				ComputeDeclarations(model, value, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			AddObjectDeclaration(model, enumTypeSyntax, shouldSkip, getSymbol, builder, cancellationToken, num);
			break;
		}
		case SyntaxKind.EnumExtensionType:
		{
			EnumExtensionTypeSyntax enumExtensionTypeSyntax = (EnumExtensionTypeSyntax)node;
			_ = enumExtensionTypeSyntax.Values;
			foreach (EnumValueSyntax value2 in enumExtensionTypeSyntax.Values)
			{
				ComputeDeclarations(model, value2, shouldSkip, getSymbol, builder, num, cancellationToken);
			}
			AddObjectDeclaration(model, enumExtensionTypeSyntax, shouldSkip, getSymbol, builder, cancellationToken, num);
			break;
		}
		default:
			DebugAssertHelper.Assert(!(node is ObjectSyntax), "The node should have been included in a case. Kind: " + node.Kind);
			break;
		}
	}

	private static void ComputeDeclaration(SemanticModel model, SyntaxNode node, Func<SyntaxNode, int?, bool> shouldSkip, bool getSymbol, List<DeclarationInfo> builder, int? newLevel, CancellationToken cancellationToken)
	{
		MethodOrTriggerDeclarationSyntax methodOrTriggerDeclarationSyntax = (MethodOrTriggerDeclarationSyntax)node;
		foreach (MemberAttributeSyntax attribute in methodOrTriggerDeclarationSyntax.Attributes)
		{
			ComputeDeclarations(model, attribute, shouldSkip, getSymbol, builder, newLevel, cancellationToken);
		}
		List<SyntaxNode> list = new List<SyntaxNode>(methodOrTriggerDeclarationSyntax.ParameterList.Parameters.Count + ((methodOrTriggerDeclarationSyntax.ReturnValue != null) ? 1 : 0) + ((methodOrTriggerDeclarationSyntax.Body != null) ? 1 : 0));
		foreach (ParameterSyntax parameter in methodOrTriggerDeclarationSyntax.ParameterList.Parameters)
		{
			list.Add(parameter);
		}
		if (methodOrTriggerDeclarationSyntax.ReturnValue != null)
		{
			list.Add(methodOrTriggerDeclarationSyntax.ReturnValue);
		}
		if (methodOrTriggerDeclarationSyntax.Variables != null)
		{
			ComputeDeclarations(model, methodOrTriggerDeclarationSyntax.Variables, shouldSkip, getSymbol, builder, newLevel, cancellationToken);
		}
		if (methodOrTriggerDeclarationSyntax.Body != null)
		{
			list.Add(methodOrTriggerDeclarationSyntax.Body);
		}
		builder.Add(GetDeclarationInfo(model, node, getSymbol, list, cancellationToken));
	}

	private static void XmlPortNodeDeclaration(SemanticModel model, Func<SyntaxNode, int?, bool> shouldSkip, bool getSymbol, List<DeclarationInfo> builder, CancellationToken cancellationToken, XmlPortNodeSyntax xmlPortNode, int? newLevel)
	{
		foreach (XmlPortNodeSyntax item in xmlPortNode.Schema)
		{
			ComputeDeclarations(model, item, shouldSkip, getSymbol, builder, newLevel, cancellationToken);
		}
		foreach (TriggerDeclarationSyntax trigger in xmlPortNode.Triggers)
		{
			ComputeDeclarations(model, trigger, shouldSkip, getSymbol, builder, newLevel, cancellationToken);
		}
		foreach (PropertySyntaxOrEmpty property in xmlPortNode.PropertyList.Properties)
		{
			ComputeDeclarations(model, property, shouldSkip, getSymbol, builder, newLevel, cancellationToken);
		}
	}

	private static void AddObjectDeclaration(SemanticModel model, ObjectSyntax node, Func<SyntaxNode, int?, bool> shouldSkip, bool getSymbol, List<DeclarationInfo> builder, CancellationToken cancellationToken, int? newLevel)
	{
		foreach (PropertySyntaxOrEmpty property in node.PropertyList.Properties)
		{
			ComputeDeclarations(model, property, shouldSkip, getSymbol, builder, newLevel, cancellationToken);
		}
		foreach (MemberSyntax member in node.Members)
		{
			ComputeDeclarations(model, member, shouldSkip, getSymbol, builder, newLevel, cancellationToken);
		}
		builder.Add(GetDeclarationInfo(model, node, getSymbol, cancellationToken));
	}
}
