using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
public static class SyntaxNodeExtensions
{
	internal const string DefaultIndentation = "  ";

	private static readonly Func<SyntaxTriviaList, int, SyntaxToken> FindSkippedTokenForward = (SyntaxTriviaList l, int p) => FindTokenHelper.FindSkippedTokenForward(GetSkippedTokens(l), p);

	private static readonly Func<SyntaxTriviaList, int, SyntaxToken> FindSkippedTokenBackward = (SyntaxTriviaList l, int p) => FindTokenHelper.FindSkippedTokenBackward(GetSkippedTokens(l), p);

	[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
	private static readonly Func<SyntaxTriviaList, int, SyntaxToken> findSkippedTokenForward = (SyntaxTriviaList l, int p) => FindTokenHelper.FindSkippedTokenForward(GetSkippedTokens(l), p);

	[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
	private static readonly Func<SyntaxTriviaList, int, SyntaxToken> findSkippedTokenBackward = (SyntaxTriviaList l, int p) => FindTokenHelper.FindSkippedTokenBackward(GetSkippedTokens(l), p);

	private static ImmutableDictionary<string, PropertyTypeInfo> PageControlProperties = ObjectParser.PageFieldProperties.Values.Concat(ObjectParser.PageGroupProperties.Values).Concat(ObjectParser.PagePartProperties.Values).Distinct(PropertyTypeInfo.EqualityComparer)
		.ToImmutableDictionary((PropertyTypeInfo p) => p.Name.ToUpperInvariant());

	private static ImmutableDictionary<string, PropertyTypeInfo> PageActionProperties = ObjectParser.PageActionGroupProperties.Values.Concat(ObjectParser.PageActionAreaProperties.Values).Concat(ObjectParser.PageActionProperties.Values).Concat(ObjectParser.PageActionSeparatorProperties.Values)
		.Distinct(PropertyTypeInfo.EqualityComparer)
		.ToImmutableDictionary((PropertyTypeInfo p) => p.Name.ToUpperInvariant());

	internal static SyntaxList<PropertySyntaxOrEmpty> GetPropertyList(this SyntaxNode node)
	{
		if (node == null)
		{
			return default;
		}
		return node.Kind switch
		{
			SyntaxKind.ProfileObject => ((ProfileSyntax)node).PropertyList.Properties, 
			SyntaxKind.EnumType => ((EnumTypeSyntax)node).PropertyList.Properties, 
			SyntaxKind.EnumValue => ((EnumValueSyntax)node).PropertyList.Properties, 
			SyntaxKind.EnumExtensionType => ((EnumExtensionTypeSyntax)node).PropertyList.Properties, 
			SyntaxKind.DotNetPackage => ((DotNetPackageSyntax)node).PropertyList.Properties, 
			SyntaxKind.DotNetAssembly => ((DotNetAssemblySyntax)node).PropertyList.Properties, 
			SyntaxKind.DotNetTypeDeclaration => ((DotNetTypeDeclarationSyntax)node).PropertyList.Properties, 
			SyntaxKind.CodeunitObject => ((CodeunitSyntax)node).PropertyList.Properties, 
			SyntaxKind.TableObject => ((TableSyntax)node).PropertyList.Properties, 
			SyntaxKind.Field => ((FieldSyntax)node).PropertyList.Properties, 
			SyntaxKind.FieldModification => ((FieldModificationSyntax)node).PropertyList.Properties, 
			SyntaxKind.Key => ((KeySyntax)node).PropertyList.Properties, 
			SyntaxKind.FieldGroupAddChange => ((FieldGroupAddChangeSyntax)node).PropertyList.Properties, 
			SyntaxKind.FieldGroup => ((FieldGroupSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageObject => ((PageSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageField => ((PageFieldSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageLabel => ((PageLabelSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageGroup => ((PageGroupSyntax)node).PropertyList.Properties, 
			SyntaxKind.PagePart => ((PagePartSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageSystemPart => ((PageSystemPartSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageChartPart => ((PageChartPartSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageUserControl => ((PageUserControlSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageArea => ((PageAreaSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageView => ((PageViewSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageActionGroup => ((PageActionGroupSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageActionArea => ((PageActionAreaSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageAction => ((PageActionSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageActionSeparator => ((PageActionSeparatorSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageExtensionObject => ((PageExtensionSyntax)node).PropertyList.Properties, 
			SyntaxKind.PageCustomizationObject => ((PageCustomizationSyntax)node).PropertyList.Properties, 
			SyntaxKind.TableExtensionObject => ((TableExtensionSyntax)node).PropertyList.Properties, 
			SyntaxKind.ControlModifyChange => ((ControlModifyChangeSyntax)node).PropertyList.Properties, 
			SyntaxKind.ViewModifyChange => ((ViewModifyChangeSyntax)node).PropertyList.Properties, 
			SyntaxKind.ActionModifyChange => ((ActionModifyChangeSyntax)node).PropertyList.Properties, 
			SyntaxKind.QueryObject => ((QuerySyntax)node).PropertyList.Properties, 
			SyntaxKind.QueryDataItem => ((QueryDataItemSyntax)node).PropertyList.Properties, 
			SyntaxKind.QueryColumn => ((QueryColumnSyntax)node).PropertyList.Properties, 
			SyntaxKind.QueryFilter => ((QueryFilterSyntax)node).PropertyList.Properties, 
			SyntaxKind.ReportObject => ((ReportSyntax)node).PropertyList.Properties, 
			SyntaxKind.ReportDataItem => ((ReportDataItemSyntax)node).PropertyList.Properties, 
			SyntaxKind.ReportColumn => ((ReportColumnSyntax)node).PropertyList.Properties, 
			SyntaxKind.RequestPage => ((RequestPageSyntax)node).PropertyList.Properties, 
			SyntaxKind.XmlPortObject => ((XmlPortSyntax)node).PropertyList.Properties, 
			SyntaxKind.XmlPortTableElement => ((XmlPortTableElementSyntax)node).PropertyList.Properties, 
			SyntaxKind.XmlPortFieldElement => ((XmlPortFieldElementSyntax)node).PropertyList.Properties, 
			SyntaxKind.XmlPortFieldAttribute => ((XmlPortFieldAttributeSyntax)node).PropertyList.Properties, 
			SyntaxKind.XmlPortTextElement => ((XmlPortTextElementSyntax)node).PropertyList.Properties, 
			SyntaxKind.XmlPortTextAttribute => ((XmlPortTextAttributeSyntax)node).PropertyList.Properties, 
			SyntaxKind.ControlAddInObject => ((ControlAddInSyntax)node).PropertyList.Properties, 
			_ => default, 
		};
	}

	internal static IdentifierNameSyntax GetIdenfitierNameSyntax(this SyntaxNode node)
	{
		if (node == null)
		{
			return null;
		}
		return node.Kind switch
		{
			SyntaxKind.ProfileObject => ((ProfileSyntax)node).Name, 
			SyntaxKind.EnumType => ((EnumTypeSyntax)node).Name, 
			SyntaxKind.EnumValue => ((EnumValueSyntax)node).Name, 
			SyntaxKind.EnumExtensionType => ((EnumExtensionTypeSyntax)node).Name, 
			SyntaxKind.DotNetPackage => ((DotNetPackageSyntax)node).Name, 
			SyntaxKind.CodeunitObject => ((CodeunitSyntax)node).Name, 
			SyntaxKind.TableObject => ((TableSyntax)node).Name, 
			SyntaxKind.Field => ((FieldSyntax)node).Name, 
			SyntaxKind.FieldModification => ((FieldModificationSyntax)node).Name, 
			SyntaxKind.Key => ((KeySyntax)node).Name, 
			SyntaxKind.FieldGroup => ((FieldGroupSyntax)node).Name, 
			SyntaxKind.PageObject => ((PageSyntax)node).Name, 
			SyntaxKind.PageField => ((PageFieldSyntax)node).Name, 
			SyntaxKind.PageLabel => ((PageLabelSyntax)node).Name, 
			SyntaxKind.PageGroup => ((PageGroupSyntax)node).Name, 
			SyntaxKind.PagePart => ((PagePartSyntax)node).Name, 
			SyntaxKind.PageSystemPart => ((PageSystemPartSyntax)node).Name, 
			SyntaxKind.PageChartPart => ((PageChartPartSyntax)node).Name, 
			SyntaxKind.PageUserControl => ((PageUserControlSyntax)node).Name, 
			SyntaxKind.PageArea => ((PageAreaSyntax)node).Name, 
			SyntaxKind.PageView => ((PageViewSyntax)node).Name, 
			SyntaxKind.PageActionGroup => ((PageActionGroupSyntax)node).Name, 
			SyntaxKind.PageActionArea => ((PageActionAreaSyntax)node).Name, 
			SyntaxKind.PageAction => ((PageActionSyntax)node).Name, 
			SyntaxKind.PageActionSeparator => ((PageActionSeparatorSyntax)node).Name, 
			SyntaxKind.PageExtensionObject => ((PageExtensionSyntax)node).Name, 
			SyntaxKind.PageCustomizationObject => ((PageCustomizationSyntax)node).Name, 
			SyntaxKind.TableExtensionObject => ((TableExtensionSyntax)node).Name, 
			SyntaxKind.ControlModifyChange => ((ControlModifyChangeSyntax)node).Name, 
			SyntaxKind.ViewModifyChange => ((ViewModifyChangeSyntax)node).Name, 
			SyntaxKind.ActionModifyChange => ((ActionModifyChangeSyntax)node).Name, 
			SyntaxKind.QueryObject => ((QuerySyntax)node).Name, 
			SyntaxKind.QueryDataItem => ((QueryDataItemSyntax)node).Name, 
			SyntaxKind.QueryColumn => ((QueryColumnSyntax)node).Name, 
			SyntaxKind.QueryFilter => ((QueryFilterSyntax)node).Name, 
			SyntaxKind.ReportObject => ((ReportSyntax)node).Name, 
			SyntaxKind.ReportDataItem => ((ReportDataItemSyntax)node).Name, 
			SyntaxKind.ReportColumn => ((ReportColumnSyntax)node).Name, 
			SyntaxKind.ReportLabelMultilanguage => ((ReportLabelMultilanguageSyntax)node).Name, 
			SyntaxKind.ReportLabel => ((ReportLabelSyntax)node).Name, 
			SyntaxKind.RequestPage => ((RequestPageSyntax)node).Name, 
			SyntaxKind.XmlPortObject => ((XmlPortSyntax)node).Name, 
			SyntaxKind.XmlPortTableElement => ((XmlPortTableElementSyntax)node).Name, 
			SyntaxKind.XmlPortFieldElement => ((XmlPortFieldElementSyntax)node).Name, 
			SyntaxKind.XmlPortFieldAttribute => ((XmlPortFieldAttributeSyntax)node).Name, 
			SyntaxKind.XmlPortTextElement => ((XmlPortTextElementSyntax)node).Name, 
			SyntaxKind.XmlPortTextAttribute => ((XmlPortTextAttributeSyntax)node).Name, 
			SyntaxKind.ControlAddInObject => ((ControlAddInSyntax)node).Name, 
			SyntaxKind.MemberAttribute => ((MemberAttributeSyntax)node).Name, 
			SyntaxKind.VariableDeclaration => ((VariableDeclarationSyntax)node).Name, 
			SyntaxKind.Parameter => ((ParameterSyntax)node).Name, 
			SyntaxKind.ReturnValue => ((ReturnValueSyntax)node).Name, 
			SyntaxKind.TriggerDeclaration => ((TriggerDeclarationSyntax)node).Name, 
			SyntaxKind.EventTriggerDeclaration => ((EventTriggerDeclarationSyntax)node).Name, 
			SyntaxKind.MethodDeclaration => ((MethodDeclarationSyntax)node).Name, 
			SyntaxKind.EventDeclaration => ((EventDeclarationSyntax)node).Name, 
			SyntaxKind.MemberAccessExpression => ((MemberAccessExpressionSyntax)node).Name, 
			SyntaxKind.OptionAccessExpression => ((OptionAccessExpressionSyntax)node).Name, 
			_ => null, 
		};
	}

	internal static (SyntaxToken, SyntaxToken) GetBraces(this SyntaxNode node)
	{
		switch (node?.Kind)
		{
		case SyntaxKind.ProfileObject:
		{
			ProfileSyntax profileSyntax = (ProfileSyntax)node;
			return ValueTuple.Create(profileSyntax.OpenBraceToken, profileSyntax.CloseBraceToken);
		}
		case SyntaxKind.EnumType:
		{
			EnumTypeSyntax enumTypeSyntax = (EnumTypeSyntax)node;
			return ValueTuple.Create(enumTypeSyntax.OpenBraceToken, enumTypeSyntax.CloseBraceToken);
		}
		case SyntaxKind.EnumValue:
		{
			EnumValueSyntax enumValueSyntax = (EnumValueSyntax)node;
			return ValueTuple.Create(enumValueSyntax.OpenBraceToken, enumValueSyntax.CloseBraceToken);
		}
		case SyntaxKind.EnumExtensionType:
		{
			EnumExtensionTypeSyntax enumExtensionTypeSyntax = (EnumExtensionTypeSyntax)node;
			return ValueTuple.Create(enumExtensionTypeSyntax.OpenBraceToken, enumExtensionTypeSyntax.CloseBraceToken);
		}
		case SyntaxKind.DotNetPackage:
		{
			DotNetPackageSyntax dotNetPackageSyntax = (DotNetPackageSyntax)node;
			return ValueTuple.Create(dotNetPackageSyntax.OpenBraceToken, dotNetPackageSyntax.CloseBraceToken);
		}
		case SyntaxKind.DotNetAssembly:
		{
			DotNetAssemblySyntax dotNetAssemblySyntax = (DotNetAssemblySyntax)node;
			return ValueTuple.Create(dotNetAssemblySyntax.OpenBraceToken, dotNetAssemblySyntax.CloseBraceToken);
		}
		case SyntaxKind.DotNetTypeDeclaration:
		{
			DotNetTypeDeclarationSyntax dotNetTypeDeclarationSyntax = (DotNetTypeDeclarationSyntax)node;
			return ValueTuple.Create(dotNetTypeDeclarationSyntax.OpenBraceToken, dotNetTypeDeclarationSyntax.CloseBraceToken);
		}
		case SyntaxKind.CodeunitObject:
		{
			CodeunitSyntax codeunitSyntax = (CodeunitSyntax)node;
			return ValueTuple.Create(codeunitSyntax.OpenBraceToken, codeunitSyntax.CloseBraceToken);
		}
		case SyntaxKind.TableObject:
		{
			TableSyntax tableSyntax = (TableSyntax)node;
			return ValueTuple.Create(tableSyntax.OpenBraceToken, tableSyntax.CloseBraceToken);
		}
		case SyntaxKind.FieldList:
		{
			FieldListSyntax fieldListSyntax = (FieldListSyntax)node;
			return ValueTuple.Create(fieldListSyntax.OpenBraceToken, fieldListSyntax.CloseBraceToken);
		}
		case SyntaxKind.FieldExtensionList:
		{
			FieldExtensionListSyntax fieldExtensionListSyntax = (FieldExtensionListSyntax)node;
			return ValueTuple.Create(fieldExtensionListSyntax.OpenBraceToken, fieldExtensionListSyntax.CloseBraceToken);
		}
		case SyntaxKind.Field:
		{
			FieldSyntax fieldSyntax = (FieldSyntax)node;
			return ValueTuple.Create(fieldSyntax.OpenBraceToken, fieldSyntax.CloseBraceToken);
		}
		case SyntaxKind.FieldModification:
		{
			FieldModificationSyntax fieldModificationSyntax = (FieldModificationSyntax)node;
			return ValueTuple.Create(fieldModificationSyntax.OpenBraceToken, fieldModificationSyntax.CloseBraceToken);
		}
		case SyntaxKind.KeyList:
		{
			KeyListSyntax keyListSyntax = (KeyListSyntax)node;
			return ValueTuple.Create(keyListSyntax.OpenBraceToken, keyListSyntax.CloseBraceToken);
		}
		case SyntaxKind.Key:
		{
			KeySyntax keySyntax = (KeySyntax)node;
			return ValueTuple.Create(keySyntax.OpenBraceToken, keySyntax.CloseBraceToken);
		}
		case SyntaxKind.FieldGroupList:
		{
			FieldGroupListSyntax fieldGroupListSyntax = (FieldGroupListSyntax)node;
			return ValueTuple.Create(fieldGroupListSyntax.OpenBraceToken, fieldGroupListSyntax.CloseBraceToken);
		}
		case SyntaxKind.FieldGroupExtensionList:
		{
			FieldGroupExtensionListSyntax fieldGroupExtensionListSyntax = (FieldGroupExtensionListSyntax)node;
			return ValueTuple.Create(fieldGroupExtensionListSyntax.OpenBraceToken, fieldGroupExtensionListSyntax.CloseBraceToken);
		}
		case SyntaxKind.FieldGroupAddChange:
		{
			FieldGroupAddChangeSyntax fieldGroupAddChangeSyntax = (FieldGroupAddChangeSyntax)node;
			return ValueTuple.Create(fieldGroupAddChangeSyntax.OpenBraceToken, fieldGroupAddChangeSyntax.CloseBraceToken);
		}
		case SyntaxKind.FieldGroup:
		{
			FieldGroupSyntax fieldGroupSyntax = (FieldGroupSyntax)node;
			return ValueTuple.Create(fieldGroupSyntax.OpenBraceToken, fieldGroupSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageObject:
		{
			PageSyntax pageSyntax = (PageSyntax)node;
			return ValueTuple.Create(pageSyntax.OpenBraceToken, pageSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageLayout:
		{
			PageLayoutSyntax pageLayoutSyntax = (PageLayoutSyntax)node;
			return ValueTuple.Create(pageLayoutSyntax.OpenBraceToken, pageLayoutSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageField:
		{
			PageFieldSyntax pageFieldSyntax = (PageFieldSyntax)node;
			return ValueTuple.Create(pageFieldSyntax.OpenBraceToken, pageFieldSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageLabel:
		{
			PageLabelSyntax pageLabelSyntax = (PageLabelSyntax)node;
			return ValueTuple.Create(pageLabelSyntax.OpenBraceToken, pageLabelSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageGroup:
		{
			PageGroupSyntax pageGroupSyntax = (PageGroupSyntax)node;
			return ValueTuple.Create(pageGroupSyntax.OpenBraceToken, pageGroupSyntax.CloseBraceToken);
		}
		case SyntaxKind.PagePart:
		{
			PagePartSyntax pagePartSyntax = (PagePartSyntax)node;
			return ValueTuple.Create(pagePartSyntax.OpenBraceToken, pagePartSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageSystemPart:
		{
			PageSystemPartSyntax pageSystemPartSyntax = (PageSystemPartSyntax)node;
			return ValueTuple.Create(pageSystemPartSyntax.OpenBraceToken, pageSystemPartSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageChartPart:
		{
			PageChartPartSyntax pageChartPartSyntax = (PageChartPartSyntax)node;
			return ValueTuple.Create(pageChartPartSyntax.OpenBraceToken, pageChartPartSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageUserControl:
		{
			PageUserControlSyntax pageUserControlSyntax = (PageUserControlSyntax)node;
			return ValueTuple.Create(pageUserControlSyntax.OpenBraceToken, pageUserControlSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageArea:
		{
			PageAreaSyntax pageAreaSyntax = (PageAreaSyntax)node;
			return ValueTuple.Create(pageAreaSyntax.OpenBraceToken, pageAreaSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageActionList:
		{
			PageActionListSyntax pageActionListSyntax = (PageActionListSyntax)node;
			return ValueTuple.Create(pageActionListSyntax.OpenBraceToken, pageActionListSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageViewList:
		{
			PageViewListSyntax pageViewListSyntax = (PageViewListSyntax)node;
			return ValueTuple.Create(pageViewListSyntax.OpenBraceToken, pageViewListSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageView:
		{
			PageViewSyntax pageViewSyntax = (PageViewSyntax)node;
			return ValueTuple.Create(pageViewSyntax.OpenBraceToken, pageViewSyntax.CloseBraceToken);
		}
		case SyntaxKind.GroupActionList:
		{
			GroupActionListSyntax groupActionListSyntax = (GroupActionListSyntax)node;
			return ValueTuple.Create(groupActionListSyntax.OpenBraceToken, groupActionListSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageActionGroup:
		{
			PageActionGroupSyntax pageActionGroupSyntax = (PageActionGroupSyntax)node;
			return ValueTuple.Create(pageActionGroupSyntax.OpenBraceToken, pageActionGroupSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageActionArea:
		{
			PageActionAreaSyntax pageActionAreaSyntax = (PageActionAreaSyntax)node;
			return ValueTuple.Create(pageActionAreaSyntax.OpenBraceToken, pageActionAreaSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageAction:
		{
			PageActionSyntax pageActionSyntax = (PageActionSyntax)node;
			return ValueTuple.Create(pageActionSyntax.OpenBraceToken, pageActionSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageActionSeparator:
		{
			PageActionSeparatorSyntax pageActionSeparatorSyntax = (PageActionSeparatorSyntax)node;
			return ValueTuple.Create(pageActionSeparatorSyntax.OpenBraceToken, pageActionSeparatorSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageExtensionObject:
		{
			PageExtensionSyntax pageExtensionSyntax = (PageExtensionSyntax)node;
			return ValueTuple.Create(pageExtensionSyntax.OpenBraceToken, pageExtensionSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageCustomizationObject:
		{
			PageCustomizationSyntax pageCustomizationSyntax = (PageCustomizationSyntax)node;
			return ValueTuple.Create(pageCustomizationSyntax.OpenBraceToken, pageCustomizationSyntax.CloseBraceToken);
		}
		case SyntaxKind.TableExtensionObject:
		{
			TableExtensionSyntax tableExtensionSyntax = (TableExtensionSyntax)node;
			return ValueTuple.Create(tableExtensionSyntax.OpenBraceToken, tableExtensionSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageExtensionLayout:
		{
			PageExtensionLayoutSyntax pageExtensionLayoutSyntax = (PageExtensionLayoutSyntax)node;
			return ValueTuple.Create(pageExtensionLayoutSyntax.OpenBraceToken, pageExtensionLayoutSyntax.CloseBraceToken);
		}
		case SyntaxKind.ControlAddChange:
		{
			ControlAddChangeSyntax controlAddChangeSyntax = (ControlAddChangeSyntax)node;
			return ValueTuple.Create(controlAddChangeSyntax.OpenBraceToken, controlAddChangeSyntax.CloseBraceToken);
		}
		case SyntaxKind.ControlModifyChange:
		{
			ControlModifyChangeSyntax controlModifyChangeSyntax = (ControlModifyChangeSyntax)node;
			return ValueTuple.Create(controlModifyChangeSyntax.OpenBraceToken, controlModifyChangeSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageExtensionViewList:
		{
			PageExtensionViewListSyntax pageExtensionViewListSyntax = (PageExtensionViewListSyntax)node;
			return ValueTuple.Create(pageExtensionViewListSyntax.OpenBraceToken, pageExtensionViewListSyntax.CloseBraceToken);
		}
		case SyntaxKind.ViewAddChange:
		{
			ViewAddChangeSyntax viewAddChangeSyntax = (ViewAddChangeSyntax)node;
			return ValueTuple.Create(viewAddChangeSyntax.OpenBraceToken, viewAddChangeSyntax.CloseBraceToken);
		}
		case SyntaxKind.ViewModifyChange:
		{
			ViewModifyChangeSyntax viewModifyChangeSyntax = (ViewModifyChangeSyntax)node;
			return ValueTuple.Create(viewModifyChangeSyntax.OpenBraceToken, viewModifyChangeSyntax.CloseBraceToken);
		}
		case SyntaxKind.PageExtensionActionList:
		{
			PageExtensionActionListSyntax pageExtensionActionListSyntax = (PageExtensionActionListSyntax)node;
			return ValueTuple.Create(pageExtensionActionListSyntax.OpenBraceToken, pageExtensionActionListSyntax.CloseBraceToken);
		}
		case SyntaxKind.ActionAddChange:
		{
			ActionAddChangeSyntax actionAddChangeSyntax = (ActionAddChangeSyntax)node;
			return ValueTuple.Create(actionAddChangeSyntax.OpenBraceToken, actionAddChangeSyntax.CloseBraceToken);
		}
		case SyntaxKind.ActionModifyChange:
		{
			ActionModifyChangeSyntax actionModifyChangeSyntax = (ActionModifyChangeSyntax)node;
			return ValueTuple.Create(actionModifyChangeSyntax.OpenBraceToken, actionModifyChangeSyntax.CloseBraceToken);
		}
		case SyntaxKind.QueryObject:
		{
			QuerySyntax querySyntax = (QuerySyntax)node;
			return ValueTuple.Create(querySyntax.OpenBraceToken, querySyntax.CloseBraceToken);
		}
		case SyntaxKind.QueryElements:
		{
			QueryElementsSyntax queryElementsSyntax = (QueryElementsSyntax)node;
			return ValueTuple.Create(queryElementsSyntax.OpenBraceToken, queryElementsSyntax.CloseBraceToken);
		}
		case SyntaxKind.QueryDataItem:
		{
			QueryDataItemSyntax queryDataItemSyntax = (QueryDataItemSyntax)node;
			return ValueTuple.Create(queryDataItemSyntax.OpenBraceToken, queryDataItemSyntax.CloseBraceToken);
		}
		case SyntaxKind.QueryColumn:
		{
			QueryColumnSyntax queryColumnSyntax = (QueryColumnSyntax)node;
			return ValueTuple.Create(queryColumnSyntax.OpenBraceToken, queryColumnSyntax.CloseBraceToken);
		}
		case SyntaxKind.QueryFilter:
		{
			QueryFilterSyntax queryFilterSyntax = (QueryFilterSyntax)node;
			return ValueTuple.Create(queryFilterSyntax.OpenBraceToken, queryFilterSyntax.CloseBraceToken);
		}
		case SyntaxKind.ReportObject:
		{
			ReportSyntax reportSyntax = (ReportSyntax)node;
			return ValueTuple.Create(reportSyntax.OpenBraceToken, reportSyntax.CloseBraceToken);
		}
		case SyntaxKind.ReportDataSetSection:
		{
			ReportDataSetSectionSyntax reportDataSetSectionSyntax = (ReportDataSetSectionSyntax)node;
			return ValueTuple.Create(reportDataSetSectionSyntax.OpenBraceToken, reportDataSetSectionSyntax.CloseBraceToken);
		}
		case SyntaxKind.ReportDataItem:
		{
			ReportDataItemSyntax reportDataItemSyntax = (ReportDataItemSyntax)node;
			return ValueTuple.Create(reportDataItemSyntax.OpenBraceToken, reportDataItemSyntax.CloseBraceToken);
		}
		case SyntaxKind.ReportColumn:
		{
			ReportColumnSyntax reportColumnSyntax = (ReportColumnSyntax)node;
			return ValueTuple.Create(reportColumnSyntax.OpenBraceToken, reportColumnSyntax.CloseBraceToken);
		}
		case SyntaxKind.ReportLabelsSection:
		{
			ReportLabelsSectionSyntax reportLabelsSectionSyntax = (ReportLabelsSectionSyntax)node;
			return ValueTuple.Create(reportLabelsSectionSyntax.OpenBraceToken, reportLabelsSectionSyntax.CloseBraceToken);
		}
		case SyntaxKind.RequestPage:
		{
			RequestPageSyntax requestPageSyntax = (RequestPageSyntax)node;
			return ValueTuple.Create(requestPageSyntax.OpenBraceToken, requestPageSyntax.CloseBraceToken);
		}
		case SyntaxKind.XmlPortObject:
		{
			XmlPortSyntax xmlPortSyntax = (XmlPortSyntax)node;
			return ValueTuple.Create(xmlPortSyntax.OpenBraceToken, xmlPortSyntax.CloseBraceToken);
		}
		case SyntaxKind.XmlPortSchema:
		{
			XmlPortSchemaSyntax xmlPortSchemaSyntax = (XmlPortSchemaSyntax)node;
			return ValueTuple.Create(xmlPortSchemaSyntax.OpenBraceToken, xmlPortSchemaSyntax.CloseBraceToken);
		}
		case SyntaxKind.XmlPortTableElement:
		{
			XmlPortTableElementSyntax xmlPortTableElementSyntax = (XmlPortTableElementSyntax)node;
			return ValueTuple.Create(xmlPortTableElementSyntax.OpenBraceToken, xmlPortTableElementSyntax.CloseBraceToken);
		}
		case SyntaxKind.XmlPortFieldElement:
		{
			XmlPortFieldElementSyntax xmlPortFieldElementSyntax = (XmlPortFieldElementSyntax)node;
			return ValueTuple.Create(xmlPortFieldElementSyntax.OpenBraceToken, xmlPortFieldElementSyntax.CloseBraceToken);
		}
		case SyntaxKind.XmlPortFieldAttribute:
		{
			XmlPortFieldAttributeSyntax xmlPortFieldAttributeSyntax = (XmlPortFieldAttributeSyntax)node;
			return ValueTuple.Create(xmlPortFieldAttributeSyntax.OpenBraceToken, xmlPortFieldAttributeSyntax.CloseBraceToken);
		}
		case SyntaxKind.XmlPortTextElement:
		{
			XmlPortTextElementSyntax xmlPortTextElementSyntax = (XmlPortTextElementSyntax)node;
			return ValueTuple.Create(xmlPortTextElementSyntax.OpenBraceToken, xmlPortTextElementSyntax.CloseBraceToken);
		}
		case SyntaxKind.XmlPortTextAttribute:
		{
			XmlPortTextAttributeSyntax xmlPortTextAttributeSyntax = (XmlPortTextAttributeSyntax)node;
			return ValueTuple.Create(xmlPortTextAttributeSyntax.OpenBraceToken, xmlPortTextAttributeSyntax.CloseBraceToken);
		}
		case SyntaxKind.ControlAddInObject:
		{
			ControlAddInSyntax controlAddInSyntax = (ControlAddInSyntax)node;
			return ValueTuple.Create(controlAddInSyntax.OpenBraceToken, controlAddInSyntax.CloseBraceToken);
		}
		default:
			return default;
		}
	}

	public static TRoot ReplaceNode<TRoot>(this TRoot root, SyntaxNode oldNode, SyntaxNode newNode) where TRoot : SyntaxNode
	{
		return (TRoot)root.ReplaceCore(new SyntaxNode[1] { oldNode }, (SyntaxNode o, SyntaxNode r) => newNode);
	}

	public static ApplicationObjectSyntax GetContainingApplicationObjectSyntax(this SyntaxNode node)
	{
		return node.GetContainingSyntax<ApplicationObjectSyntax>();
	}

	public static ObjectSyntax GetContainingObjectSyntax(this SyntaxNode node)
	{
		return node.GetContainingSyntax<ObjectSyntax>();
	}

	public static Location GetApplicationObjectFirstLocationIncludingTrivia(this SyntaxNode node)
	{
		ObjectSyntax containingApplicationObjectSyntax = node.GetContainingApplicationObjectSyntax();
		if (containingApplicationObjectSyntax.GetLeadingTrivia().Any())
		{
			return containingApplicationObjectSyntax.GetLeadingTrivia().First().GetLocation();
		}
		return containingApplicationObjectSyntax.GetLocation();
	}

	public static TRoot ReplaceToken<TRoot>(this TRoot root, SyntaxToken oldToken, SyntaxToken newToken) where TRoot : SyntaxNode
	{
		return (TRoot)root.ReplaceCore<SyntaxNode>(null, null, new SyntaxToken[1] { oldToken }, (SyntaxToken o, SyntaxToken r) => newToken);
	}

	public static TRoot ReplaceTokens<TRoot>(this TRoot root, IEnumerable<SyntaxToken> tokens, Func<SyntaxToken, SyntaxToken, SyntaxToken> computeReplacementToken) where TRoot : SyntaxNode
	{
		return (TRoot)root.ReplaceCore<SyntaxNode>(null, null, tokens, computeReplacementToken);
	}

	public static TRoot ReplaceNodes<TRoot, TNode>(this TRoot root, IEnumerable<TNode> nodes, Func<TNode, TNode, SyntaxNode> computeReplacementNode) where TRoot : SyntaxNode where TNode : SyntaxNode
	{
		return (TRoot)root.ReplaceCore(nodes, computeReplacementNode);
	}

	public static TRoot ReplaceSyntax<TRoot>(this TRoot root, IEnumerable<SyntaxNode> nodes, Func<SyntaxNode, SyntaxNode, SyntaxNode> computeReplacementNode, IEnumerable<SyntaxToken> tokens, Func<SyntaxToken, SyntaxToken, SyntaxToken> computeReplacementToken, IEnumerable<SyntaxTrivia> trivia, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia) where TRoot : SyntaxNode
	{
		return (TRoot)root.ReplaceCore(nodes, computeReplacementNode, tokens, computeReplacementToken, trivia, computeReplacementTrivia);
	}

	public static TNode NormalizeWhiteSpace<TNode>(this TNode node, string indentation = "  ", bool elasticTrivia = false) where TNode : SyntaxNode
	{
		return (TNode)node.NormalizeWhiteSpaceCore(indentation, elasticTrivia);
	}

	public static TNode WithAnnotations<TNode>(this TNode node, params SyntaxAnnotation[] annotations) where TNode : SyntaxNode
	{
		return (TNode)node.Node.SetAnnotations(annotations).CreateRed();
	}

	public static SyntaxTriviaList GetLeadingTrivia(this SyntaxNode node)
	{
		return node.GetFirstToken(includeZeroWidth: false, includeSkipped: true).LeadingTrivia;
	}

	public static SyntaxTriviaList GetTrailingTrivia(this SyntaxNode node)
	{
		return node.GetLastToken(includeZeroWidth: false, includeSkipped: true).TrailingTrivia;
	}

	public static TSyntax WithTriviaFrom<TSyntax>(this TSyntax syntax, SyntaxNode node) where TSyntax : SyntaxNode
	{
		return syntax.WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
	}

	public static TSyntax WithoutTrivia<TSyntax>(this TSyntax syntax) where TSyntax : SyntaxNode
	{
		return syntax.WithoutLeadingTrivia().WithoutTrailingTrivia();
	}

	public static TSyntax WithLeadingTrivia<TSyntax>(this TSyntax node, SyntaxTriviaList trivia) where TSyntax : SyntaxNode
	{
		SyntaxToken firstToken = node.GetFirstToken(includeZeroWidth: true);
		SyntaxToken newToken = firstToken.WithLeadingTrivia(trivia);
		return node.ReplaceToken(firstToken, newToken);
	}

	public static TSyntax WithLeadingTrivia<TSyntax>(this TSyntax node, IEnumerable<SyntaxTrivia> trivia) where TSyntax : SyntaxNode
	{
		SyntaxToken firstToken = node.GetFirstToken(includeZeroWidth: true);
		SyntaxToken newToken = firstToken.WithLeadingTrivia(trivia);
		return node.ReplaceToken(firstToken, newToken);
	}

	public static TSyntax WithoutLeadingTrivia<TSyntax>(this TSyntax node) where TSyntax : SyntaxNode
	{
		return node.WithLeadingTrivia((IEnumerable<SyntaxTrivia>)null);
	}

	public static TSyntax WithLeadingTrivia<TSyntax>(this TSyntax node, params SyntaxTrivia[] trivia) where TSyntax : SyntaxNode
	{
		return node.WithLeadingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public static TSyntax WithTrailingTrivia<TSyntax>(this TSyntax node, SyntaxTriviaList trivia) where TSyntax : SyntaxNode
	{
		SyntaxToken lastToken = node.GetLastToken(includeZeroWidth: true);
		SyntaxToken newToken = lastToken.WithTrailingTrivia(trivia);
		return node.ReplaceToken(lastToken, newToken);
	}

	public static TSyntax WithTrailingTrivia<TSyntax>(this TSyntax node, IEnumerable<SyntaxTrivia> trivia) where TSyntax : SyntaxNode
	{
		SyntaxToken lastToken = node.GetLastToken(includeZeroWidth: true);
		SyntaxToken newToken = lastToken.WithTrailingTrivia(trivia);
		return node.ReplaceToken(lastToken, newToken);
	}

	public static TSyntax WithoutTrailingTrivia<TSyntax>(this TSyntax node) where TSyntax : SyntaxNode
	{
		return node.WithTrailingTrivia((IEnumerable<SyntaxTrivia>)null);
	}

	public static TSyntax WithTrailingTrivia<TSyntax>(this TSyntax node, params SyntaxTrivia[] trivia) where TSyntax : SyntaxNode
	{
		return node.WithTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public static TNode WithAdditionalAnnotations<TNode>(this TNode node, params SyntaxAnnotation[] annotations) where TNode : SyntaxNode
	{
		return (TNode)node.WithAdditionalAnnotationsInternal(annotations);
	}

	public static TNode WithAdditionalAnnotations<TNode>(this TNode node, IEnumerable<SyntaxAnnotation> annotations) where TNode : SyntaxNode
	{
		return (TNode)node.WithAdditionalAnnotationsInternal(annotations);
	}

	public static TNode WithoutAnnotations<TNode>(this TNode node, params SyntaxAnnotation[] annotations) where TNode : SyntaxNode
	{
		return (TNode)node.GetNodeWithoutAnnotations(annotations);
	}

	public static TNode WithoutAnnotations<TNode>(this TNode node, IEnumerable<SyntaxAnnotation> annotations) where TNode : SyntaxNode
	{
		return (TNode)node.GetNodeWithoutAnnotations(annotations);
	}

	public static TNode WithoutAnnotations<TNode>(this TNode node, AnnotationKind annotationKind) where TNode : SyntaxNode
	{
		if (node.HasAnnotations(annotationKind))
		{
			return node.WithoutAnnotations(node.GetAnnotations(annotationKind).ToArray());
		}
		return node;
	}

	public static bool IsKind(this SyntaxNode node, SyntaxKind kind)
	{
		if (node != null)
		{
			return node.Kind == kind;
		}
		return false;
	}

	public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2)
	{
		if (node != null)
		{
			if (node.Kind != kind1)
			{
				return node.Kind == kind2;
			}
			return true;
		}
		return false;
	}

	public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3)
	{
		if (node != null)
		{
			if (node.Kind != kind1 && node.Kind != kind2)
			{
				return node.Kind == kind3;
			}
			return true;
		}
		return false;
	}

	public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3, SyntaxKind kind4)
	{
		if (node != null)
		{
			if (node.Kind != kind1 && node.Kind != kind2 && node.Kind != kind3)
			{
				return node.Kind == kind4;
			}
			return true;
		}
		return false;
	}

	public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3, SyntaxKind kind4, SyntaxKind kind5)
	{
		if (node != null)
		{
			if (node.Kind != kind1 && node.Kind != kind2 && node.Kind != kind3 && node.Kind != kind4)
			{
				return node.Kind == kind5;
			}
			return true;
		}
		return false;
	}

	public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3, SyntaxKind kind4, SyntaxKind kind5, SyntaxKind kind6)
	{
		if (node != null)
		{
			if (node.Kind != kind1 && node.Kind != kind2 && node.Kind != kind3 && node.Kind != kind4 && node.Kind != kind5)
			{
				return node.Kind == kind6;
			}
			return true;
		}
		return false;
	}

	public static SyntaxToken FindTokenOnRightOfPosition(this SyntaxNode root, int position, bool includeSkipped = true)
	{
		Func<SyntaxTriviaList, int, SyntaxToken> skippedTokenFinder = (includeSkipped ? FindSkippedTokenForward : null);
		return root.FindTokenOnRightOfPosition<CompilationUnitSyntax>(position, skippedTokenFinder, includeSkipped);
	}

	public static SyntaxToken FindTokenOnLeftOfPosition(this SyntaxNode root, int position, bool includeSkipped = true)
	{
		Func<SyntaxTriviaList, int, SyntaxToken> skippedTokenFinder = (includeSkipped ? FindSkippedTokenBackward : null);
		return root.FindTokenOnLeftOfPosition<CompilationUnitSyntax>(position, skippedTokenFinder, includeSkipped);
	}

	[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
	public static SyntaxNodeOrToken ChildThatContainsPosition(this SyntaxNode self, int position, out int childIndex)
	{
		ChildSyntaxList childSyntaxList = self.ChildNodesAndTokens();
		int num = 0;
		int num2 = childSyntaxList.Count - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num) / 2;
			SyntaxNodeOrToken result = childSyntaxList[num3];
			TextSpan fullSpan = result.FullSpan;
			if (position < fullSpan.Start)
			{
				num2 = num3 - 1;
				continue;
			}
			if (position >= fullSpan.End)
			{
				num = num3 + 1;
				continue;
			}
			childIndex = num3;
			return result;
		}
		DebugAssertHelper.Assert(!self.FullSpan.Contains(position), "Position is valid. How could we not find a child?");
		throw new ArgumentOutOfRangeException("position");
	}

	internal static SyntaxKind GetLiteralExpressionSyntaxKind(this SyntaxNode syntax)
	{
		if (syntax.Kind == SyntaxKind.IdentifierOrLiteralExpression)
		{
			return ((IdentifierOrLiteralExpressionSyntax)syntax).Identifier.GetLiteralExpressionSyntaxKind();
		}
		if (syntax.Kind != SyntaxKind.LiteralExpression)
		{
			return SyntaxKind.None;
		}
		return ((LiteralExpressionSyntax)syntax).Literal.Kind;
	}

	internal static int GetObjectIdOrDefault(this ObjectIdSyntax objectId)
	{
		SyntaxToken? syntaxToken = objectId?.Value;
		if (!syntaxToken.HasValue || syntaxToken.Value.IsMissing)
		{
			return 0;
		}
		return (int)objectId?.Value.Value;
	}

	public static string Unquoted(this NameSyntax name)
	{
		return name.Kind switch
		{
			SyntaxKind.QualifiedName => ((QualifiedNameSyntax)name).Unquoted(), 
			SyntaxKind.IdentifierName => ((IdentifierNameSyntax)name).Unquoted(), 
			_ => throw ExceptionUtilities.UnexpectedValue(name.Kind), 
		};
	}

	public static PropertyValueSyntax GetPropertyValue(this SyntaxNode node, string propertyName)
	{
		return node.GetProperty(propertyName)?.Value;
	}

	public static PropertySyntax GetProperty(this SyntaxNode node, string propertyName)
	{
		foreach (PropertySyntaxOrEmpty property in node.GetPropertyList())
		{
			if (property.Kind != SyntaxKind.EmptyProperty)
			{
				PropertySyntax propertySyntax = (PropertySyntax)property;
				if (SemanticFacts.IsSameName(propertySyntax.Name.Identifier.ValueText, propertyName))
				{
					return propertySyntax;
				}
			}
		}
		return null;
	}

	private static string Unquoted(this QualifiedNameSyntax name)
	{
		return name.Left.Unquoted() + "." + name.Right.Unquoted();
	}

	private static string Unquoted(this SimpleNameSyntax name)
	{
		return name.Identifier.ValueText.UnquoteIdentifier();
	}

	internal static bool IsKind(this InternalSyntaxToken token, params SyntaxKind[] kinds)
	{
		if (token != null)
		{
			return kinds.Any((SyntaxKind k) => k == token.Kind);
		}
		return false;
	}

	internal static bool IsKind(this InternalSyntaxToken token, SyntaxKind kind)
	{
		return token.Kind == kind;
	}

	internal static InternalSyntaxList<T> ToInternalList<T>(this SyntaxNode node) where T : InternalSyntaxNode
	{
		return node?.Node.ToInternalList<T>() ?? default(InternalSyntaxList<T>);
	}

	internal static InternalSeparatedSyntaxList<T> ToInternalSeparatedList<T>(this SyntaxNode node) where T : InternalSyntaxNode
	{
		if (node == null)
		{
			return default;
		}
		return new InternalSeparatedSyntaxList<T>(node.Node.ToInternalList<T>());
	}

	internal static InternalSyntaxList<T> ToInternalList<T>(this InternalSyntaxNode node) where T : InternalSyntaxNode
	{
		return new InternalSyntaxList<T>(node);
	}

	internal static SyntaxNode SkipParens(this SyntaxNode expression)
	{
		while (expression != null && expression.Kind == SyntaxKind.ParenthesizedExpression)
		{
			expression = ((ParenthesizedExpressionSyntax)expression).Expression;
		}
		return expression;
	}

	internal static SyntaxAnnotation GetTriggerTypeAnnotation(this SyntaxNode node)
	{
		return node.GetAnnotations(AnnotationKind.TriggerType).SingleOrDefault();
	}

	internal static SyntaxAnnotation GetAttributeTypeAnnotation(this SyntaxNode node)
	{
		return node.GetAnnotations(AnnotationKind.AttributeType).SingleOrDefault();
	}

	internal static object GetPropertyValue(this PropertyValueSyntax node)
	{
		if (node != null)
		{
			switch (node.Kind)
			{
			case SyntaxKind.BooleanPropertyValue:
				return ((BooleanPropertyValueSyntax)node).Value.GetLiteralValue();
			case SyntaxKind.StringPropertyValue:
				return ((StringPropertyValueSyntax)node).Value.GetLiteralValue();
			case SyntaxKind.Int32PropertyValue:
				return ((Int32PropertyValueSyntax)node).Value.GetLiteralValue();
			case SyntaxKind.Int64PropertyValue:
				return ((Int64PropertyValueSyntax)node).Value.GetLiteralValue();
			case SyntaxKind.DecimalPropertyValue:
				return ((DecimalPropertyValueSyntax)node).Value.GetLiteralValue();
			case SyntaxKind.TimePropertyValue:
				return ((TimePropertyValueSyntax)node).Value.GetLiteralValue();
			case SyntaxKind.DatePropertyValue:
				return ((DatePropertyValueSyntax)node).Value.GetLiteralValue();
			case SyntaxKind.DateTimePropertyValue:
				return ((DateTimePropertyValueSyntax)node).Value.GetLiteralValue();
			case SyntaxKind.EnumPropertyValue:
				return ((EnumPropertyValueSyntax)node).Value.GetIdentifierOrLiteralValue();
			}
		}
		return null;
	}

	internal static SyntaxNode GetFirstParent(this SyntaxNode node, SyntaxKind kind)
	{
		return node.Ancestors().FirstOrDefault((SyntaxNode n) => n.Kind == kind);
	}

	internal static SyntaxNode GetFirstParent(this SyntaxNode node, Func<SyntaxNode, bool> predicate)
	{
		return node.Ancestors().FirstOrDefault(predicate);
	}

	internal static string GetPropertyValueText(this PropertyValueSyntax node)
	{
		if (node.Kind.IsCalculationFormulaStatementKind())
		{
			return node.ToString();
		}
		switch (node.Kind)
		{
		case SyntaxKind.EnumPropertyValue:
			return ((EnumPropertyValueSyntax)node).Value.GetIdentifierOrLiteralValue();
		case SyntaxKind.BooleanPropertyValue:
			return ((BooleanPropertyValueSyntax)node).Value.Value.ValueText;
		case SyntaxKind.DecimalPropertyValue:
			return ((DecimalPropertyValueSyntax)node).Value.ToString();
		case SyntaxKind.Int64PropertyValue:
			return ((Int64PropertyValueSyntax)node).Value.ToString();
		case SyntaxKind.DatePropertyValue:
			return ((DatePropertyValueSyntax)node).Value.ToString();
		case SyntaxKind.TimePropertyValue:
			return ((TimePropertyValueSyntax)node).Value.ToString();
		case SyntaxKind.DateTimePropertyValue:
			return ((DateTimePropertyValueSyntax)node).Value.ToString();
		case SyntaxKind.StringPropertyValue:
			return ((StringPropertyValueSyntax)node).Value.Value.Text;
		case SyntaxKind.Int32PropertyValue:
			return ((Int32PropertyValueSyntax)node).Value.ToString();
		case SyntaxKind.OptionValuesPropertyValue:
			return ((OptionValuesPropertyValueSyntax)node).Value?.ToString();
		case SyntaxKind.OptionValuePropertyValue:
			return ((OptionValuePropertyValueSyntax)node).Value.Identifier.ValueText;
		case SyntaxKind.PageFieldReferencePropertyValue:
		case SyntaxKind.TableFilterPropertyValue:
			return node.ToString();
		default:
			return null;
		}
	}

	internal static string GetIdentifierOrLiteralValue(this SyntaxNode node)
	{
		if (node.Kind == SyntaxKind.IdentifierOrLiteralPropertyValue)
		{
			return ((IdentifierOrLiteralPropertyValueSyntax)node).Identifier.GetIdentifierOrLiteralValue();
		}
		if (node.Kind == SyntaxKind.IdentifierOrLiteralExpression)
		{
			return ((IdentifierOrLiteralExpressionSyntax)node).Identifier.GetIdentifierOrLiteralValue();
		}
		if (node.Kind == SyntaxKind.IdentifierName)
		{
			IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)node;
			if (identifierNameSyntax.IsMissing)
			{
				return null;
			}
			return identifierNameSyntax.Unquoted();
		}
		if (node.Kind == SyntaxKind.IdentifierNameOrEmpty)
		{
			IdentifierNameOrEmptySyntax identifierNameOrEmptySyntax = (IdentifierNameOrEmptySyntax)node;
			if (identifierNameOrEmptySyntax.IsMissing)
			{
				return null;
			}
			return identifierNameOrEmptySyntax.IdentifierName.Unquoted();
		}
		if (node.Kind.IsLiteral())
		{
			return node.ToString().UnquoteLiteral();
		}
		if (node.Kind == SyntaxKind.LiteralExpression)
		{
			return ((LiteralExpressionSyntax)node).Literal.ToString().UnquoteLiteral();
		}
		if (node.Kind == SyntaxKind.ObjectId)
		{
			return ((ObjectIdSyntax)node).Value.ToFullString();
		}
		return null;
	}

	internal static int? GetLiteralValue(this Int32SignedLiteralValueSyntax node)
	{
		if (node.Number.IsMissing)
		{
			return null;
		}
		int num = (int)node.Number.Value;
		return (node.Sign.Kind == SyntaxKind.MinusToken) ? (-num) : num;
	}

	internal static long? GetLiteralValue(this Int64SignedLiteralValueSyntax node)
	{
		if (node.Number.IsMissing)
		{
			return null;
		}
		long num = (long)node.Number.Value;
		return (node.Sign.Kind == SyntaxKind.MinusToken) ? (-num) : num;
	}

	internal static string GetLiteralValue(this StringLiteralValueSyntax node)
	{
		if (node.Value.IsMissing)
		{
			return null;
		}
		return node.Value.ValueText.UnquoteLiteral();
	}

	internal static decimal? GetLiteralValue(this DecimalSignedLiteralValueSyntax node)
	{
		if (node.Number.IsMissing)
		{
			return null;
		}
		decimal num = (decimal)node.Number.Value;
		return (node.Sign.Kind == SyntaxKind.MinusToken) ? (-num) : num;
	}

	internal static object GetLiteralValue(this LiteralValueSyntax node)
	{
		DebugAssertHelper.Assert(node != null);
		return node.Kind switch
		{
			SyntaxKind.BooleanLiteralValue => ((BooleanLiteralValueSyntax)node).Value.Kind == SyntaxKind.TrueKeyword, 
			SyntaxKind.Int32SignedLiteralValue => ((Int32SignedLiteralValueSyntax)node).GetLiteralValue(), 
			SyntaxKind.Int64SignedLiteralValue => ((Int64SignedLiteralValueSyntax)node).GetLiteralValue(), 
			SyntaxKind.StringLiteralValue => ((StringLiteralValueSyntax)node).GetLiteralValue(), 
			SyntaxKind.DecimalSignedLiteralValue => ((DecimalSignedLiteralValueSyntax)node).GetLiteralValue(), 
			SyntaxKind.TimeLiteralValue => ((TimeLiteralValueSyntax)node).Value.Value, 
			SyntaxKind.DateLiteralValue => ((DateLiteralValueSyntax)node).Value.Value, 
			SyntaxKind.DateTimeLiteralValue => ((DateTimeLiteralValueSyntax)node).Value.Value, 
			_ => null, 
		};
	}

	internal static object GetLiteralExpressionValue(this LiteralExpressionSyntax node)
	{
		DebugAssertHelper.Assert(node != null);
		DebugAssertHelper.Assert(node.Literal != null);
		return node.Literal.GetLiteralValue();
	}

	internal static bool IsKindAndNotMissing(this SyntaxToken token, SyntaxKind kind)
	{
		if (!token.IsMissing)
		{
			return token.IsKind(kind);
		}
		return false;
	}

	internal static bool IsAttributeArgument(this SyntaxKind kind)
	{
		if ((uint)(kind - 424) <= 3u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPossibleAttributeArgument(this InternalSyntaxToken token)
	{
		SyntaxKind kind = token.Kind;
		if ((uint)(kind - 2) <= 2u || (uint)(kind - 10) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPossibleParameter(this InternalSyntaxToken token)
	{
		if (token.Kind != SyntaxKind.VarKeyword)
		{
			return token.Kind.IsTokenIdentifier();
		}
		return true;
	}

	internal static void SplitMemberAccess(this SyntaxNode syntax, out string lhs, out string rhs)
	{
		lhs = null;
		rhs = null;
		if (syntax.Kind == SyntaxKind.IdentifierName)
		{
			lhs = syntax.GetIdentifierOrLiteralValue();
		}
		else if (syntax.Kind == SyntaxKind.MemberAccessExpression)
		{
			MemberAccessExpressionSyntax memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)syntax;
			lhs = memberAccessExpressionSyntax.Expression.GetIdentifierOrLiteralValue()?.UnquoteIdentifier();
			rhs = memberAccessExpressionSyntax.Name.GetIdentifierOrLiteralValue()?.UnquoteIdentifier();
		}
		else if (syntax.Kind == SyntaxKind.IdentifierOrLiteralPropertyValue)
		{
			((IdentifierOrLiteralPropertyValueSyntax)syntax).Identifier.SplitMemberAccess(out lhs, out rhs);
		}
		else if (syntax.Kind == SyntaxKind.MemberAccessOrIdentifierPropertyValue)
		{
			((MemberAccessOrIdentifierPropertyValueSyntax)syntax).Identifier.SplitMemberAccess(out lhs, out rhs);
		}
	}

	internal static IEnumerable<TNode> GetAncestors<TNode>(this SyntaxNode node) where TNode : SyntaxNode
	{
		for (SyntaxNode current = node.Parent; current != null; current = (current as IStructuredTriviaSyntax)?.ParentTrivia.Token.Parent ?? current.Parent)
		{
			if (current is TNode val)
			{
				yield return val;
			}
		}
	}

	internal static TNode GetAncestor<TNode>(this SyntaxNode node) where TNode : SyntaxNode
	{
		if (node == null)
		{
			return null;
		}
		return node.GetAncestors<TNode>().FirstOrDefault();
	}

	internal static bool IsInMethodParameterList(this SyntaxNode node)
	{
		if (node.IsKind(SyntaxKind.Parameter))
		{
			if (!node.ParentIsKind(SyntaxKind.MethodDeclaration))
			{
				return node.ParentIsKind(SyntaxKind.EventDeclaration);
			}
			return true;
		}
		return false;
	}

	internal static bool CanHaveAssociatedLocalBinder(this SyntaxNode syntax)
	{
		if (!syntax.IsLocalDeclaration())
		{
			return syntax is StatementSyntax;
		}
		return true;
	}

	internal static bool IsLocalDeclaration(this SyntaxNode syntax)
	{
		if (syntax.Kind != SyntaxKind.VariableDeclaration)
		{
			return false;
		}
		if (syntax.ParentIsKind(SyntaxKind.VarSection))
		{
			return syntax.Parent.ParentIsKind(SyntaxKind.MethodDeclaration);
		}
		return false;
	}

	internal static bool ParentIsKind(this SyntaxNode syntax, SyntaxKind kind)
	{
		if (syntax.Parent != null)
		{
			return syntax.Parent.IsKind(kind);
		}
		return false;
	}

	internal static IEnumerable<PropertyTypeInfo> GetAllDefinedPropertyNames(this SyntaxNode syntax)
	{
		return syntax.GetPropertyTypeInfos().Values;
	}

	internal static ImmutableDictionary<string, PropertyTypeInfo> GetPropertyTypeInfos(this SyntaxNode syntax)
	{
		SyntaxNode syntaxNode;
		if (syntax.Kind != SyntaxKind.Property)
		{
			if (!SyntaxTokenExtensions.PropertyDeclaringNodes.Contains(syntax.Kind))
			{
				return ImmutableDictionary<string, PropertyTypeInfo>.Empty;
			}
			syntaxNode = syntax;
		}
		else
		{
			syntaxNode = syntax.Parent.Parent;
			if (syntaxNode == null)
			{
				return ImmutableDictionary<string, PropertyTypeInfo>.Empty;
			}
		}
		return syntaxNode.GetDefinedPropertyTypeInfos();
	}

	internal static PropertyTypeInfo GetPropertyTypeInfo(this PropertySyntax propertySyntax)
	{
		if (propertySyntax == null)
		{
			return null;
		}
		string key = propertySyntax.Name.Identifier.ValueText.ToUpperInvariant();
		if (!propertySyntax.GetPropertyTypeInfos().TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	internal static PropertyTypeInfo GetPropertyTypeInfo(this PropertyValueSyntax propertyValue)
	{
		return (propertyValue?.GetAncestor<PropertySyntax>())?.GetPropertyTypeInfo();
	}

	internal static string GetName(this ActionAddChangeSyntax change)
	{
		return "ActionAdd" + (change.Parent as PageExtensionActionListSyntax)?.Changes.IndexOf(change).ToString(CultureInfo.InvariantCulture);
	}

	internal static string GetName(this ControlAddChangeSyntax change)
	{
		return "ControlAdd" + (change.Parent as PageExtensionLayoutSyntax)?.Changes.IndexOf(change).ToString(CultureInfo.InvariantCulture);
	}

	internal static string GetName(this ViewAddChangeSyntax change)
	{
		return "ViewAdd" + (change.Parent as PageExtensionViewListSyntax)?.Changes.IndexOf(change).ToString(CultureInfo.InvariantCulture);
	}

	internal static string GetName(this EventTriggerDeclarationSyntax eventTrigger)
	{
		return string.Concat(eventTrigger.Publisher.Unquoted(), str2: eventTrigger.Name.Unquoted(), str1: eventTrigger.ColonColonToken.ToString());
	}

	internal static string GetTypeAlias(this DotNetTypeDeclarationSyntax typeDeclaration)
	{
		if (typeDeclaration.TypeAlias != null && !typeDeclaration.TypeAlias.IsMissing)
		{
			return typeDeclaration.TypeAlias.Unquoted();
		}
		return GetTypeAlias(typeDeclaration.TypeName);
		static string GetTypeAlias(NameSyntax name)
		{
			return name.Kind switch
			{
				SyntaxKind.QualifiedName => GetTypeAlias(((QualifiedNameSyntax)name).Right), 
				SyntaxKind.IdentifierName => name.Unquoted(), 
				_ => throw ExceptionUtilities.UnexpectedValue(name.Kind), 
			};
		}
	}

	internal static string ToReportingString(this SyntaxNode node)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (SyntaxNode syntaxNode = node; syntaxNode != null; syntaxNode = syntaxNode.Parent)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" <- ");
			}
			stringBuilder.Append(syntaxNode.Kind);
			if (syntaxNode.IsMissing)
			{
				stringBuilder.Append("(missing)");
			}
		}
		return stringBuilder.ToString();
	}

	private static T GetContainingSyntax<T>(this SyntaxNode node) where T : SyntaxNode
	{
		T val = node as T;
		if (val == node)
		{
			return val;
		}
		return node.Parent.GetContainingSyntax<T>();
	}

	private static IEnumerable<SyntaxToken> GetSkippedTokens(SyntaxTriviaList list)
	{
		if (!HasSkippedTokens(list))
		{
			return SpecializedCollections.EmptyEnumerable<SyntaxToken>();
		}
		ArrayBuilder<SyntaxToken> arrayBuilder = null;
		try
		{
			arrayBuilder = ArrayBuilder<SyntaxToken>.GetInstance();
			foreach (SyntaxTrivia item in list)
			{
				if (item.Kind == SyntaxKind.SkippedTokensTrivia)
				{
					arrayBuilder.Add(((SyntaxNodeOrToken)(SkippedTokensTriviaSyntax)item.GetStructure()).AsToken());
				}
			}
			return arrayBuilder.ToArray();
		}
		finally
		{
			arrayBuilder?.Free();
		}
	}

	public static string GetNameStringValue(this SyntaxNode syntaxNode)
	{
		return syntaxNode.GetIdenfitierNameSyntax()?.Identifier.ValueText.UnquoteIdentifier();
	}

	private static bool HasSkippedTokens(SyntaxTriviaList list)
	{
		return list.Any((SyntaxTrivia trivia) => trivia.Kind == SyntaxKind.SkippedTokensTrivia);
	}

	internal static ImmutableDictionary<string, PropertyTypeInfo> GetDefinedPropertyTypeInfos(this SyntaxNode node)
	{
		if (node == null)
		{
			return ImmutableDictionary<string, PropertyTypeInfo>.Empty;
		}
		switch (node.Kind)
		{
		case SyntaxKind.ProfileObject:
			return ObjectParser.ProfileProperties;
		case SyntaxKind.CodeunitObject:
			return ObjectParser.CodeunitProperties;
		case SyntaxKind.TableObject:
			return ObjectParser.TableProperties;
		case SyntaxKind.Field:
			return ObjectParser.FieldProperties;
		case SyntaxKind.FieldModification:
			return ObjectParser.FieldProperties;
		case SyntaxKind.Key:
			return ObjectParser.KeyProperties;
		case SyntaxKind.FieldGroup:
			return ObjectParser.FieldGroupProperties;
		case SyntaxKind.PageField:
			return ObjectParser.PageFieldProperties;
		case SyntaxKind.PageLabel:
			return ObjectParser.PageLabelProperties;
		case SyntaxKind.PageGroup:
			return ObjectParser.PageGroupProperties;
		case SyntaxKind.PagePart:
			return ObjectParser.PagePartProperties;
		case SyntaxKind.PageSystemPart:
			return ObjectParser.PageSystemPartProperties;
		case SyntaxKind.PageChartPart:
			return ObjectParser.PageChartPartProperties;
		case SyntaxKind.PageUserControl:
			return ObjectParser.PageUserControlProperties;
		case SyntaxKind.PageArea:
			return ObjectParser.PageAreaProperties;
		case SyntaxKind.PageActionGroup:
			return ObjectParser.PageActionGroupProperties;
		case SyntaxKind.PageActionArea:
			return ObjectParser.PageActionAreaProperties;
		case SyntaxKind.PageAction:
			return ObjectParser.PageActionProperties;
		case SyntaxKind.PageActionSeparator:
			return ObjectParser.PageActionSeparatorProperties;
		case SyntaxKind.PageObject:
		case SyntaxKind.PageExtensionObject:
		case SyntaxKind.PageCustomizationObject:
			return ObjectParser.PageProperties;
		case SyntaxKind.TableExtensionObject:
			return ObjectParser.TableProperties;
		case SyntaxKind.ControlModifyChange:
			return PageControlProperties;
		case SyntaxKind.ActionModifyChange:
			return PageActionProperties;
		case SyntaxKind.ViewModifyChange:
		case SyntaxKind.PageView:
			return ObjectParser.PageViewProperties;
		case SyntaxKind.QueryObject:
			return ObjectParser.QueryProperties;
		case SyntaxKind.QueryDataItem:
			return ObjectParser.QueryDataItemProperties;
		case SyntaxKind.QueryColumn:
			return ObjectParser.QueryColumnProperties;
		case SyntaxKind.QueryFilter:
			return ObjectParser.QueryFilterProperties;
		case SyntaxKind.ReportObject:
			return ObjectParser.ReportProperties;
		case SyntaxKind.ReportDataItem:
			return ObjectParser.ReportDataItemProperties;
		case SyntaxKind.ReportColumn:
			return ObjectParser.ReportColumnProperties;
		case SyntaxKind.RequestPage:
			return ObjectParser.RequestPageProperties;
		case SyntaxKind.XmlPortObject:
			return ObjectParser.XmlPortProperties;
		case SyntaxKind.XmlPortTableElement:
			return ObjectParser.XmlPortTableElementProperties;
		case SyntaxKind.XmlPortFieldElement:
			return ObjectParser.XmlPortFieldElementProperties;
		case SyntaxKind.XmlPortFieldAttribute:
			return ObjectParser.XmlPortFieldAttributeProperties;
		case SyntaxKind.XmlPortTextElement:
			return ObjectParser.XmlPortTextElementProperties;
		case SyntaxKind.XmlPortTextAttribute:
			return ObjectParser.XmlPortTextAttributeProperties;
		case SyntaxKind.ControlAddInObject:
			return ObjectParser.ControlAddInObjectProperties;
		case SyntaxKind.DotNetAssembly:
			return ObjectParser.DotNetAssemblyProperties;
		case SyntaxKind.DotNetTypeDeclaration:
			return ObjectParser.DotNetTypeDeclarationProperties;
		case SyntaxKind.EnumType:
			return ObjectParser.EnumTypeProperties;
		case SyntaxKind.EnumValue:
			return ObjectParser.EnumValueProperties;
		default:
			return ImmutableDictionary<string, PropertyTypeInfo>.Empty;
		}
	}
}
