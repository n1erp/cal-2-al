using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class ObjectParser : LanguageParser
{
	internal static ImmutableDictionary<string, PropertyTypeInfo> CodeunitProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> FieldProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> KeyProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageActionProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageActionAreaProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageActionGroupProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageAreaProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageFieldProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageGroupProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageLabelProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PagePartProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageSystemPartProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageChartPartProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> QueryProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> QueryColumnProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> QueryDataItemProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> QueryFilterProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> ReportProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> ReportDataItemProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> ReportColumnProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> RequestPageProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> TableProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> XmlPortProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> XmlPortTextElementProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> XmlPortFieldElementProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> XmlPortTableElementProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> XmlPortFieldAttributeProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> XmlPortTextAttributeProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> FieldGroupProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageActionSeparatorProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> EnumValueProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> DotNetAssemblyProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> DotNetTypeDeclarationProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageViewProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> ProfileProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageCustomizationObjectProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> ControlAddInObjectProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageUserControlProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> EnumTypeProperties { get; }

	private static PropertyTypeInfo LookupAnyActionProperty(string name)
	{
		return LookupPageActionProperty(name) ?? LookupPageActionGroupProperty(name) ?? LookupPageActionAreaProperty(name);
	}

	private InternalPageExtensionActionListSyntax ParsePageExtensionActions()
	{
		if (CurrentToken.Kind != SyntaxKind.ActionsKeyword)
		{
			return null;
		}
		if (IsIncremental && CurrentNodeKind == SyntaxKind.PageExtensionActionList)
		{
			return (InternalPageExtensionActionListSyntax)EatNode();
		}
		InternalSyntaxToken actionsKeyword = EatToken(SyntaxKind.ActionsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalActionChangeBaseSyntax> changes = ParseChangeActionList();
		return ContextAwareSyntaxFactory.PageExtensionActionList(actionsKeyword, openBraceToken, changes, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalActionChangeBaseSyntax> ParseChangeActionList()
	{
		InternalSyntaxListBuilder<InternalActionChangeBaseSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalActionChangeBaseSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind.IsPageChangeKeyword())
			{
				InternalActionChangeBaseSyntax node = ParseActionChange();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalActionChangeBaseSyntax ParseActionChange()
	{
		if (IsIncremental && (CurrentNodeKind == SyntaxKind.ActionAddChange || CurrentNodeKind == SyntaxKind.ActionModifyChange))
		{
			return (InternalActionChangeBaseSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatToken();
		switch (internalSyntaxToken.Kind)
		{
		case SyntaxKind.AddFirstKeyword:
		case SyntaxKind.AddLastKeyword:
		case SyntaxKind.AddBeforeKeyword:
		case SyntaxKind.AddAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken3 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor2 = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken2 = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken2 = EatToken(SyntaxKind.OpenBraceToken);
			InternalSyntaxList<InternalActionBaseSyntax> actions2 = ParseActionList();
			InternalSyntaxToken closeBraceToken2 = EatToken(SyntaxKind.CloseBraceToken);
			return ContextAwareSyntaxFactory.ActionAddChange(internalSyntaxToken, openParenthesisToken3, anchor2, closeParenthesisToken2, openBraceToken2, actions2, closeBraceToken2);
		}
		case SyntaxKind.MoveFirstKeyword:
		case SyntaxKind.MoveLastKeyword:
		case SyntaxKind.MoveBeforeKeyword:
		case SyntaxKind.MoveAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken2 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor = ParseIdentifierName();
			InternalSyntaxToken startToken = EatToken(SyntaxKind.SemicolonToken);
			InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> actions = ParseCommaSeparatedIdentifierNames(ref startToken);
			return ContextAwareSyntaxFactory.ActionMoveChange(internalSyntaxToken, openParenthesisToken2, anchor, startToken, actions, EatToken(SyntaxKind.CloseParenToken));
		}
		case SyntaxKind.ModifyKeyword:
		{
			InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax name = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
			InternalPropertyListSyntax propertyList = ParsePropertyList(LookupAnyActionProperty, asExtension: false, deferErrorChecking: true);
			InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetActionExtensionTriggerInfo);
			InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			return ContextAwareSyntaxFactory.ActionModifyChange(internalSyntaxToken, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(internalSyntaxToken.Kind);
		}
	}

	private InternalPageActionListSyntax ParsePageActions()
	{
		if (CurrentToken.Kind != SyntaxKind.ActionsKeyword)
		{
			return null;
		}
		if (IsIncremental && CurrentNodeKind == SyntaxKind.PageActionList)
		{
			return (InternalPageActionListSyntax)EatNode();
		}
		InternalSyntaxToken actionsKeyword = EatToken(SyntaxKind.ActionsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalPageActionAreaSyntax> areas = ParseActionAreas();
		return ContextAwareSyntaxFactory.PageActionList(actionsKeyword, openBraceToken, areas, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalPageActionAreaSyntax> ParseActionAreas()
	{
		InternalSyntaxListBuilder<InternalPageActionAreaSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalPageActionAreaSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind == SyntaxKind.PageAreaKeyword)
			{
				internalSyntaxListBuilder.Add(ParseActionArea());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalPageActionAreaSyntax ParseActionArea()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.PageActionArea)
		{
			return (InternalPageActionAreaSyntax)EatNode();
		}
		InternalSyntaxToken actionKeyword = EatToken(SyntaxKind.PageAreaKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupPageActionAreaProperty);
		InternalSyntaxList<InternalActionBaseSyntax> actions = ParseActionList();
		return ContextAwareSyntaxFactory.PageActionArea(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, actions, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalActionBaseSyntax> ParseActionList(bool disallowGroups = false)
	{
		InternalSyntaxListBuilder<InternalActionBaseSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalActionBaseSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind.IsPageActionOrGroupKeyword())
			{
				internalSyntaxListBuilder.Add(ParseAction(disallowGroups));
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalActionBaseSyntax ParseAction(bool disallowGroups = false)
	{
		if (IsIncremental && (CurrentNodeKind == SyntaxKind.PageActionGroup || CurrentNodeKind == SyntaxKind.PageAction || CurrentNodeKind == SyntaxKind.PageActionSeparator))
		{
			return (InternalActionBaseSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatToken();
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		switch (internalSyntaxToken.Kind)
		{
		case SyntaxKind.ActionKeyword:
		{
			InternalPropertyListSyntax propertyList3 = ParsePropertyList(LookupPageActionProperty);
			InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetActionTriggerInfo);
			return ContextAwareSyntaxFactory.PageAction(internalSyntaxToken, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList3, triggers, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.PageGroupKeyword:
		{
			InternalPropertyListSyntax propertyList2 = ParsePropertyList(LookupPageActionGroupProperty);
			InternalSyntaxList<InternalActionBaseSyntax> actions = ParseActionList();
			InternalPageActionGroupSyntax internalPageActionGroupSyntax = ContextAwareSyntaxFactory.PageActionGroup(internalSyntaxToken, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList2, actions, EatToken(SyntaxKind.CloseBraceToken));
			if (disallowGroups)
			{
				internalPageActionGroupSyntax = AddErrorToFirstToken(internalPageActionGroupSyntax, ErrorCode.ERR_GroupingOfActionsNotAllowed);
			}
			return internalPageActionGroupSyntax;
		}
		case SyntaxKind.SeparatorKeyword:
		{
			InternalPropertyListSyntax propertyList = ParsePropertyList(LookupPageActionSeparatorProperty);
			return ContextAwareSyntaxFactory.PageActionSeparator(internalSyntaxToken, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, EatToken(SyntaxKind.CloseBraceToken));
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(internalSyntaxToken.Kind);
		}
	}

	private static PropertyTypeInfo LookupAnyControlProperty(string name)
	{
		return LookupPageFieldProperty(name) ?? LookupPageGroupProperty(name) ?? LookupPagePartProperty(name) ?? LookupPageAreaProperty(name);
	}

	private InternalPageExtensionLayoutSyntax ParsePageExtensionLayout()
	{
		if (CurrentToken.Kind != SyntaxKind.PageLayoutKeyword)
		{
			return null;
		}
		if (IsIncremental && CurrentNodeKind == SyntaxKind.PageExtensionLayout)
		{
			return (InternalPageExtensionLayoutSyntax)EatNode();
		}
		InternalSyntaxToken layoutKeyword = EatToken(SyntaxKind.PageLayoutKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalControlChangeBaseSyntax> changes = ParseChangeControlList();
		return ContextAwareSyntaxFactory.PageExtensionLayout(layoutKeyword, openBraceToken, changes, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalControlChangeBaseSyntax> ParseChangeControlList()
	{
		InternalSyntaxListBuilder<InternalControlChangeBaseSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalControlChangeBaseSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind.IsPageChangeKeyword())
			{
				internalSyntaxListBuilder.Add(ParseControlChange());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalControlChangeBaseSyntax ParseControlChange()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.ControlAddChange)
		{
			return (InternalControlChangeBaseSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatToken();
		switch (internalSyntaxToken.Kind)
		{
		case SyntaxKind.AddFirstKeyword:
		case SyntaxKind.AddLastKeyword:
		case SyntaxKind.AddBeforeKeyword:
		case SyntaxKind.AddAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken3 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor2 = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken2 = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken2 = EatToken(SyntaxKind.OpenBraceToken);
			InternalSyntaxList<InternalControlBaseSyntax> controls2 = ParseControlList();
			InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			return ContextAwareSyntaxFactory.ControlAddChange(internalSyntaxToken, openParenthesisToken3, anchor2, closeParenthesisToken2, openBraceToken2, controls2, closeBraceToken);
		}
		case SyntaxKind.MoveFirstKeyword:
		case SyntaxKind.MoveLastKeyword:
		case SyntaxKind.MoveBeforeKeyword:
		case SyntaxKind.MoveAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken2 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor = ParseIdentifierName();
			InternalSyntaxToken startToken = EatToken(SyntaxKind.SemicolonToken);
			InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> controls = ParseCommaSeparatedIdentifierNames(ref startToken);
			return ContextAwareSyntaxFactory.ControlMoveChange(internalSyntaxToken, openParenthesisToken2, anchor, startToken, controls, EatToken(SyntaxKind.CloseParenToken));
		}
		case SyntaxKind.ModifyKeyword:
		{
			InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax name = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
			InternalPropertyListSyntax propertyList = ParsePropertyList(LookupAnyControlProperty, asExtension: false, deferErrorChecking: true);
			InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetControlExtensionTriggerInfo);
			return ContextAwareSyntaxFactory.ControlModifyChange(internalSyntaxToken, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, EatToken(SyntaxKind.CloseBraceToken));
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(internalSyntaxToken.Kind);
		}
	}

	private static PropertyTypeInfo LookupAnyViewProperty(string name)
	{
		return LookupPageViewProperty(name);
	}

	private InternalPageExtensionViewListSyntax ParsePageExtensionViews()
	{
		if (CurrentToken.Kind != SyntaxKind.ViewsKeyword)
		{
			return null;
		}
		if (IsIncremental && CurrentNodeKind == SyntaxKind.PageExtensionViewList)
		{
			return (InternalPageExtensionViewListSyntax)EatNode();
		}
		InternalSyntaxToken viewsKeyword = EatToken(SyntaxKind.ViewsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalViewChangeBaseSyntax> changes = ParseChangeViewList();
		InternalPageExtensionViewListSyntax node = ContextAwareSyntaxFactory.PageExtensionViewList(viewsKeyword, openBraceToken, changes, EatToken(SyntaxKind.CloseBraceToken));
		return CheckFeatureAvailability(node, Feature.Views);
	}

	private InternalSyntaxList<InternalViewChangeBaseSyntax> ParseChangeViewList()
	{
		InternalSyntaxListBuilder<InternalViewChangeBaseSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalViewChangeBaseSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind.IsPageChangeKeyword())
			{
				InternalViewChangeBaseSyntax node = ParseViewChange();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalViewChangeBaseSyntax ParseViewChange()
	{
		if (IsIncremental && (CurrentNodeKind == SyntaxKind.ViewAddChange || CurrentNodeKind == SyntaxKind.ViewModifyChange))
		{
			return (InternalViewChangeBaseSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatToken();
		switch (internalSyntaxToken.Kind)
		{
		case SyntaxKind.AddFirstKeyword:
		case SyntaxKind.AddLastKeyword:
		{
			InternalSyntaxToken openBraceToken3 = EatToken(SyntaxKind.OpenBraceToken);
			InternalSyntaxList<InternalPageViewSyntax> views4 = ParseViewList();
			return ContextAwareSyntaxFactory.ViewAddChange(internalSyntaxToken, null, null, null, openBraceToken3, views4, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.AddBeforeKeyword:
		case SyntaxKind.AddAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken3 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor2 = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken2 = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken2 = EatToken(SyntaxKind.OpenBraceToken);
			InternalSyntaxList<InternalPageViewSyntax> views3 = ParseViewList();
			return ContextAwareSyntaxFactory.ViewAddChange(internalSyntaxToken, openParenthesisToken3, anchor2, closeParenthesisToken2, openBraceToken2, views3, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.MoveFirstKeyword:
		case SyntaxKind.MoveLastKeyword:
		{
			InternalSyntaxToken startToken2 = EatToken(SyntaxKind.OpenParenToken);
			InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> views2 = ParseCommaSeparatedIdentifierNames(ref startToken2);
			return ContextAwareSyntaxFactory.ViewMoveChange(internalSyntaxToken, startToken2, null, null, views2, EatToken(SyntaxKind.CloseParenToken));
		}
		case SyntaxKind.MoveBeforeKeyword:
		case SyntaxKind.MoveAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken2 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor = ParseIdentifierName();
			InternalSyntaxToken startToken = EatToken(SyntaxKind.SemicolonToken);
			InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> views = ParseCommaSeparatedIdentifierNames(ref startToken);
			return ContextAwareSyntaxFactory.ViewMoveChange(internalSyntaxToken, openParenthesisToken2, anchor, startToken, views, EatToken(SyntaxKind.CloseParenToken));
		}
		case SyntaxKind.ModifyKeyword:
		{
			InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax name = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
			InternalPropertyListSyntax propertyList = ParsePropertyList(LookupAnyViewProperty, asExtension: false, deferErrorChecking: true);
			InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			return ContextAwareSyntaxFactory.ViewModifyChange(internalSyntaxToken, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(internalSyntaxToken.Kind);
		}
	}

	private InternalPageLayoutSyntax ParsePageLayout()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.PageLayout)
		{
			return (InternalPageLayoutSyntax)EatNode();
		}
		if (CurrentToken.Kind != SyntaxKind.PageLayoutKeyword)
		{
			return null;
		}
		InternalSyntaxToken layoutKeyword = EatToken(SyntaxKind.PageLayoutKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalPageAreaSyntax> areas = ParseAreas();
		return ContextAwareSyntaxFactory.PageLayout(layoutKeyword, openBraceToken, areas, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalPageAreaSyntax> ParseAreas()
	{
		InternalSyntaxListBuilder<InternalPageAreaSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalPageAreaSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind == SyntaxKind.PageAreaKeyword)
			{
				InternalPageAreaSyntax node = ParseArea();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalPageAreaSyntax ParseArea()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.PageArea)
		{
			return (InternalPageAreaSyntax)EatNode();
		}
		InternalSyntaxToken controlKeyword = EatToken(SyntaxKind.PageAreaKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupPageAreaProperty);
		InternalSyntaxList<InternalControlBaseSyntax> controls = ParseControlList();
		return ContextAwareSyntaxFactory.PageArea(controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, controls, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalControlBaseSyntax> ParseControlList()
	{
		InternalSyntaxListBuilder<InternalControlBaseSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalControlBaseSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind.IsPageControlOrGroupKeyword())
			{
				internalSyntaxListBuilder.Add(ParseControl());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalControlBaseSyntax ParseControl()
	{
		if (IsIncremental && CurrentNode.IsKind(SyntaxKind.PageField, SyntaxKind.PageLabel, SyntaxKind.PagePart, SyntaxKind.PageSystemPart, SyntaxKind.PageChartPart, SyntaxKind.PageGroup))
		{
			return (InternalControlBaseSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatToken();
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = null;
		InternalCodeExpressionSyntax expression = null;
		InternalObjectNameOrIdSyntax partName = null;
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = null;
		InternalObjectNameReferenceSyntax controlAddIn = null;
		switch (internalSyntaxToken.Kind)
		{
		case SyntaxKind.FieldKeyword:
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			expression = ParseInLexerMode(LexerMode.Expression, ParseExpression);
			break;
		case SyntaxKind.PagePartKeyword:
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			partName = ParseObjectReferenceSyntax();
			break;
		case SyntaxKind.PageSystemPartKeyword:
		case SyntaxKind.PageChartPartKeyword:
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			internalIdentifierNameSyntax = ParseIdentifierName();
			break;
		case SyntaxKind.PageUserControlKeyword:
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			controlAddIn = ParseObjectNameReferenceSyntax();
			break;
		}
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		switch (internalSyntaxToken.Kind)
		{
		case SyntaxKind.FieldKeyword:
		{
			InternalPropertyListSyntax propertyList3 = ParsePropertyList(LookupPageFieldProperty);
			InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetControlTriggerInfo);
			return ContextAwareSyntaxFactory.PageField(internalSyntaxToken, openParenthesisToken, name, semicolonToken, expression, closeParenthesisToken, openBraceToken, propertyList3, triggers, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.LabelKeyword:
		{
			InternalPropertyListSyntax propertyList2 = ParsePropertyList(LookupPageLabelProperty);
			return ContextAwareSyntaxFactory.PageLabel(internalSyntaxToken, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList2, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.PagePartKeyword:
		{
			InternalPropertyListSyntax propertyList4 = ParsePropertyList(LookupPagePartProperty);
			return ContextAwareSyntaxFactory.PagePart(internalSyntaxToken, openParenthesisToken, name, semicolonToken, partName, closeParenthesisToken, openBraceToken, propertyList4, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.PageSystemPartKeyword:
		{
			InternalPropertyListSyntax propertyList7 = ParsePropertyList(LookupPageSystemPartProperty);
			return ContextAwareSyntaxFactory.PageSystemPart(internalSyntaxToken, openParenthesisToken, name, semicolonToken, internalIdentifierNameSyntax, closeParenthesisToken, openBraceToken, propertyList7, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.PageChartPartKeyword:
		{
			InternalPropertyListSyntax propertyList6 = ParsePropertyList(LookupPageChartPartProperty);
			return ContextAwareSyntaxFactory.PageChartPart(internalSyntaxToken, openParenthesisToken, name, semicolonToken, internalIdentifierNameSyntax, closeParenthesisToken, openBraceToken, propertyList6, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.PageUserControlKeyword:
		{
			InternalPropertyListSyntax propertyList5 = ParsePropertyList(LookupPageUserControlProperty);
			InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers2 = ParseTriggerList();
			return ContextAwareSyntaxFactory.PageUserControl(internalSyntaxToken, openParenthesisToken, name, semicolonToken, controlAddIn, closeParenthesisToken, openBraceToken, propertyList5, triggers2, EatToken(SyntaxKind.CloseBraceToken));
		}
		default:
		{
			InternalPropertyListSyntax propertyList = ParsePropertyList(LookupPageGroupProperty);
			InternalSyntaxList<InternalControlBaseSyntax> controls = ParseControlList();
			InternalGroupActionListSyntax internalGroupActionListSyntax = ParseGroupActionList();
			if (internalGroupActionListSyntax != null && !SyntaxFacts.HasGroupActionList(internalSyntaxToken.Kind))
			{
				internalGroupActionListSyntax = AddErrorToFirstToken(internalGroupActionListSyntax, ErrorCode.ERR_ActionsAreNotAllowedOnThisControlType);
			}
			return ContextAwareSyntaxFactory.PageGroup(internalSyntaxToken, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, controls, internalGroupActionListSyntax, EatToken(SyntaxKind.CloseBraceToken));
		}
		}
	}

	private InternalGroupActionListSyntax ParseGroupActionList()
	{
		if (CurrentToken.Kind != SyntaxKind.ActionsKeyword)
		{
			return null;
		}
		if (IsIncremental && CurrentNodeKind == SyntaxKind.GroupActionList)
		{
			return (InternalGroupActionListSyntax)EatNode();
		}
		InternalSyntaxToken actionsKeyword = EatToken(SyntaxKind.ActionsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalActionBaseSyntax> actions = ParseActionList(disallowGroups: true);
		return ContextAwareSyntaxFactory.GroupActionList(actionsKeyword, openBraceToken, actions, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalPageViewListSyntax ParsePageViews()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.PageViewList)
		{
			return (InternalPageViewListSyntax)EatNode();
		}
		if (CurrentToken.Kind != SyntaxKind.ViewsKeyword)
		{
			return null;
		}
		InternalSyntaxToken viewsKeyword = EatToken(SyntaxKind.ViewsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalPageViewSyntax> views = ParseViewList();
		InternalPageViewListSyntax node = ContextAwareSyntaxFactory.PageViewList(viewsKeyword, openBraceToken, views, EatToken(SyntaxKind.CloseBraceToken));
		return CheckFeatureAvailability(node, Feature.Views);
	}

	private InternalSyntaxList<InternalPageViewSyntax> ParseViewList()
	{
		InternalSyntaxListBuilder<InternalPageViewSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalPageViewSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind == SyntaxKind.ViewKeyword)
			{
				internalSyntaxListBuilder.Add(ParseView());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalPageViewSyntax ParseView()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.PageView)
		{
			return (InternalPageViewSyntax)EatNode();
		}
		InternalSyntaxToken viewKeyword = EatToken();
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupPageViewProperty);
		InternalPageExtensionLayoutSyntax layout = ParsePageExtensionLayout();
		InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
		return ContextAwareSyntaxFactory.PageView(viewKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, layout, closeBraceToken);
	}

	private InternalEnumExtensionTypeSyntax ParseEnumExtension()
	{
		InternalSyntaxToken enumExtensionKeyword = EatToken(SyntaxKind.EnumExtensionKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		GlobalLogger.LogVerbose("Parsing EnumExtension {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken extendsKeyword = EatToken(SyntaxKind.ExtendsKeyword);
		InternalObjectNameOrIdSyntax baseObject = ParseObjectReferenceSyntax();
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupEnumTypeProperty);
		InternalSyntaxList<InternalEnumValueSyntax> values = ParseEnumValueList();
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		InternalEnumExtensionTypeSyntax node = ContextAwareSyntaxFactory.EnumExtensionType(enumExtensionKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, extendsKeyword, baseObject, openBraceToken, propertyList, values, default, closeBraceToken);
		return CheckFeatureAvailability(node, Feature.Enum);
	}

	private InternalEnumTypeSyntax ParseEnum()
	{
		InternalSyntaxToken enumToken = EatToken(SyntaxKind.EnumKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		GlobalLogger.LogVerbose("Parsing Enum {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupEnumTypeProperty);
		InternalSyntaxList<InternalEnumValueSyntax> values = ParseEnumValueList();
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		InternalEnumTypeSyntax node = ContextAwareSyntaxFactory.EnumType(enumToken, internalObjectIdSyntax, internalIdentifierNameSyntax, openBraceToken, propertyList, values, default, closeBraceToken);
		return CheckFeatureAvailability(node, Feature.Enum);
	}

	private InternalSyntaxList<InternalEnumValueSyntax> ParseEnumValueList()
	{
		InternalSyntaxListBuilder<InternalEnumValueSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalEnumValueSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind == SyntaxKind.EnumValueKeyword)
			{
				InternalEnumValueSyntax node = ParseEnumValue();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalEnumValueSyntax ParseEnumValue()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.EnumValue)
		{
			return (InternalEnumValueSyntax)EatNode();
		}
		InternalSyntaxToken enumValueToken = EatToken(SyntaxKind.EnumValueKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSyntaxToken id = EatToken(SyntaxKind.Int32LiteralToken);
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupEnumValueProperty);
		return ContextAwareSyntaxFactory.EnumValue(enumValueToken, openParenthesisToken, id, semicolonToken, name, closeParenthesisToken, openBraceToken, propertyList, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalControlAddInSyntax ParseControlAddIn()
	{
		InternalSyntaxToken controlAddInKeyword = EatToken(SyntaxKind.ControlAddInKeyword);
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		GlobalLogger.LogVerbose("Parsing ControlAddIn {0}.", internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupControlAddInObjectProperty);
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList();
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return ContextAwareSyntaxFactory.ControlAddIn(controlAddInKeyword, internalIdentifierNameSyntax, openBraceToken, propertyList, members, closeBraceToken);
	}

	private InternalDotNetPackageSyntax ParseDotNet()
	{
		InternalSyntaxToken dotNetKeyword = EatToken(SyntaxKind.DotNetKeyword);
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseOptionalName();
		GlobalLogger.LogVerbose("Parsing DotNet {0}.", internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList((string s) => (PropertyTypeInfo)null);
		InternalSyntaxList<InternalDotNetAssemblySyntax> assemblies = ParseAssemblies();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberListAndReportErrorIfNotEmpty();
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		InternalDotNetPackageSyntax node = ContextAwareSyntaxFactory.DotNetPackage(dotNetKeyword, internalIdentifierNameSyntax, openBraceToken, propertyList, assemblies, members, closeBraceToken);
		return CheckFeatureAvailability(node, Feature.DotNet);
	}

	private InternalSyntaxList<InternalMemberSyntax> ParseMemberListAndReportErrorIfNotEmpty()
	{
		InternalSyntaxList<InternalMemberSyntax> result = ParseMemberList();
		if (result.Count == 0)
		{
			return result;
		}
		InternalSyntaxListBuilder<InternalMemberSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalMemberSyntax>();
		try
		{
			for (int i = 0; i < result.Count; i++)
			{
				internalSyntaxListBuilder.Add(AddError(result[i], ErrorCode.ERR_MemberNotAllowedInContext));
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalSyntaxList<InternalDotNetAssemblySyntax> ParseAssemblies()
	{
		InternalSyntaxListBuilder<InternalDotNetAssemblySyntax> internalSyntaxListBuilder = Pool.Allocate<InternalDotNetAssemblySyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind == SyntaxKind.AssemblyKeyword)
			{
				InternalDotNetAssemblySyntax node = ParseAssembly();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalDotNetAssemblySyntax ParseAssembly()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.DotNetAssembly)
		{
			return (InternalDotNetAssemblySyntax)EatNode();
		}
		InternalSyntaxToken assemblyKeyword = EatToken(SyntaxKind.AssemblyKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalNameSyntax assemblyName = ParseQualifiedName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupDotNetAssemblyProperty);
		InternalSyntaxList<InternalDotNetTypeDeclarationSyntax> typeDeclarations = ParseDotNetTypeDeclarations();
		InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
		return ContextAwareSyntaxFactory.DotNetAssembly(assemblyKeyword, openParenthesisToken, assemblyName, closeParenthesisToken, openBraceToken, propertyList, typeDeclarations, closeBraceToken);
	}

	private InternalSyntaxList<InternalDotNetTypeDeclarationSyntax> ParseDotNetTypeDeclarations()
	{
		InternalSyntaxListBuilder<InternalDotNetTypeDeclarationSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalDotNetTypeDeclarationSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind == SyntaxKind.TypeKeyword)
			{
				InternalDotNetTypeDeclarationSyntax node = ParseDotNetTypeDeclaration();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalDotNetTypeDeclarationSyntax ParseDotNetTypeDeclaration()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.DotNetTypeDeclaration)
		{
			return (InternalDotNetTypeDeclarationSyntax)EatNode();
		}
		InternalSyntaxToken type = EatToken(SyntaxKind.TypeKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalNameSyntax typeName = ParseQualifiedName();
		ParseOptionalTypeAlias(CurrentToken, out var semiColon, out var sourceExpression);
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupDotNetTypeDeclarationProperty);
		InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
		return ContextAwareSyntaxFactory.DotNetTypeDeclaration(type, openParenthesisToken, typeName, semiColon, sourceExpression, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	private void ParseOptionalTypeAlias(InternalSyntaxToken ct, out InternalSyntaxToken semiColon, out InternalIdentifierNameSyntax sourceExpression)
	{
		if (ct.Kind == SyntaxKind.SemicolonToken)
		{
			semiColon = EatToken(SyntaxKind.SemicolonToken);
			sourceExpression = ParseIdentifierName();
		}
		else
		{
			semiColon = null;
			sourceExpression = null;
		}
	}

	private InternalIdentifierNameSyntax ParseOptionalName()
	{
		if (CurrentToken.Kind != SyntaxKind.OpenBraceToken)
		{
			return ParseIdentifierName();
		}
		return null;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pti")]
	internal InternalPropertyValueSyntax ParseCommaSeparatedIdentifierOrLiteralPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken startToken)
	{
		return ContextAwareSyntaxFactory.CommaSeparatedIdentifierOrLiteralPropertyValue(ParseCommaSeparatedLiteralOrIdentifier(ref startToken));
	}

	private static bool IsValidLiteralNodeForList(InternalSyntaxToken token)
	{
		if (!token.Kind.IsLiteral() && token.Kind != SyntaxKind.MinusToken)
		{
			return token.Kind.IsTokenIdentifier();
		}
		return true;
	}

	private InternalSeparatedSyntaxList<InternalIdentifierOrLiteralExpressionSyntax> ParseCommaSeparatedLiteralOrIdentifier(ref InternalSyntaxToken startToken, SyntaxKind closeTokenKind = SyntaxKind.SemicolonToken)
	{
		return ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, IsValidLiteralNodeForList, SyntaxKind.CommaToken, closeTokenKind, SkipBadCommaSeparatedLiteralOrIdentifier, ParseIdentifierOrLiteralExpression);
	}

	private PostSkipAction SkipBadCommaSeparatedLiteralOrIdentifier(ref InternalSyntaxToken startToken, InternalSeparatedSyntaxListBuilder<InternalIdentifierOrLiteralExpressionSyntax> list, SyntaxKind expected)
	{
		Func<ParserBase, bool> isNotExpectedFunction = (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || !IsValidLiteralNodeForList(p.CurrentToken);
		Func<ParserBase, bool> abortFunction = (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.IsTerminator();
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, isNotExpectedFunction, abortFunction, expected);
	}

	private InternalFilterExpressionValueSyntax ParseFilterExpressionValue(SyntaxKind kind = SyntaxKind.OrFilterKeyword)
	{
		InternalFilterExpressionValueSyntax internalFilterExpressionValueSyntax = ParseNextFilterExpressionValue(kind);
		while (CurrentToken.Kind == kind)
		{
			InternalSyntaxToken internalSyntaxToken = EatToken();
			InternalFilterExpressionValueSyntax right = ParseNextFilterExpressionValue(kind);
			internalFilterExpressionValueSyntax = ContextAwareSyntaxFactory.BinaryFilterExpressionValue(internalSyntaxToken.Kind.ToBinaryFilterExpressionKind(), internalFilterExpressionValueSyntax, internalSyntaxToken, right);
		}
		return internalFilterExpressionValueSyntax;
	}

	private InternalFilterExpressionValueSyntax ParseUnaryOrLiteralExpressionValue()
	{
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			return ContextAwareSyntaxFactory.ParenthesizedFilterExpressionValue(EatToken(SyntaxKind.OpenParenToken), ParseFilterExpressionValue(), EatToken(SyntaxKind.CloseParenToken));
		}
		if (!TryParseIdentifierOrLiteralOrRangeFilterExpressionValue(out var filterValue) && (!TryParseUnaryFilterExpressionValue(out filterValue) || filterValue == null))
		{
			filterValue = CreateInvalidFilterExpressionValueSyntax(string.Empty);
			return AddTrailingSkippedSyntax(filterValue, AddError(EatToken(), ErrorCode.ERR_ExpressionExpected));
		}
		return filterValue;
	}

	private bool TryParseIdentifierOrLiteralOrRangeFilterExpressionValue(out InternalFilterExpressionValueSyntax filterValue)
	{
		filterValue = null;
		InternalIdentifierOrLiteralExpressionSyntax internalIdentifierOrLiteralExpressionSyntax = TryParseIdentifierOrLiteralExpression();
		if (internalIdentifierOrLiteralExpressionSyntax == null)
		{
			return false;
		}
		InternalSyntaxToken internalSyntaxToken = EatTokenIfKind(SyntaxKind.DotDotToken);
		if (internalSyntaxToken == null)
		{
			filterValue = ContextAwareSyntaxFactory.UnaryFilterExpressionValue(SyntaxKind.UnaryEqualsFilterExpression, null, internalIdentifierOrLiteralExpressionSyntax);
		}
		else
		{
			InternalIdentifierOrLiteralExpressionSyntax internalIdentifierOrLiteralExpressionSyntax2 = TryParseIdentifierOrLiteralExpression();
			filterValue = ((internalIdentifierOrLiteralExpressionSyntax2 != null) ? ((InternalFilterExpressionValueSyntax)ContextAwareSyntaxFactory.BinaryFilterExpressionValue(SyntaxKind.RangeBetweenFilterExpression, internalIdentifierOrLiteralExpressionSyntax, internalSyntaxToken, internalIdentifierOrLiteralExpressionSyntax2)) : ((InternalFilterExpressionValueSyntax)ContextAwareSyntaxFactory.RangeFromFilterExpressionValue(internalIdentifierOrLiteralExpressionSyntax, internalSyntaxToken)));
		}
		return true;
	}

	private bool TryParseUnaryFilterExpressionValue(out InternalFilterExpressionValueSyntax filterValue)
	{
		filterValue = null;
		if (!CurrentToken.Kind.IsFilterTokenUnary() && CurrentToken.Kind != SyntaxKind.DotDotToken)
		{
			return false;
		}
		SyntaxKind kind = CurrentToken.Kind.ToUnaryFilterExpressionKind();
		InternalSyntaxToken operatorToken = EatToken();
		InternalIdentifierOrLiteralExpressionSyntax identifier = ParseIdentifierOrLiteralExpression();
		filterValue = ContextAwareSyntaxFactory.UnaryFilterExpressionValue(kind, operatorToken, identifier);
		return true;
	}

	private InternalFilterExpressionValueSyntax CreateInvalidFilterExpressionValueSyntax(string text)
	{
		InternalSyntaxToken identifier = InternalSyntaxFactory.Identifier(text);
		InternalIdentifierOrLiteralExpressionSyntax identifier2 = ContextAwareSyntaxFactory.IdentifierOrLiteralExpression(ContextAwareSyntaxFactory.IdentifierName(identifier));
		InternalFilterExpressionValueSyntax node = ContextAwareSyntaxFactory.UnaryFilterExpressionValue(SyntaxKind.UnaryEqualsFilterExpression, null, identifier2);
		return AddError(node, ErrorCode.ERR_InvalidFilterExpression);
	}

	private InternalFilterExpressionValueSyntax ParseNextFilterExpressionValue(SyntaxKind kind)
	{
		if (kind == SyntaxKind.OrFilterKeyword)
		{
			return ParseFilterExpressionValue(SyntaxKind.AndFilterKeyword);
		}
		return ParseUnaryOrLiteralExpressionValue();
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	private InternalPropertyValueSyntax ParseOrderByPropertyValue(PropertyTypeInfo propertyTypeInfo, ref InternalSyntaxToken equalsToken)
	{
		return ParseOrderByPropertyValue(ref equalsToken);
	}

	internal InternalPropertyValueSyntax ParseOrderByPropertyValue(ref InternalSyntaxToken equalsToken)
	{
		InternalSeparatedSyntaxList<InternalOrderByExpressionSyntax> order = ParseSeparatedList(ref equalsToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => IsValidOrderSpecifierKeyword(token) || token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, SyntaxKind.SemicolonToken, SkipBadOrderByItems, ParseOrderByElement);
		return ContextAwareSyntaxFactory.OrderByPropertyValue(order);
	}

	private PostSkipAction SkipBadOrderByItems(ref InternalSyntaxToken startToken, InternalSeparatedSyntaxListBuilder<InternalOrderByExpressionSyntax> list, SyntaxKind expected)
	{
		Func<ParserBase, bool> isNotExpectedFunction = (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || !IsValidOrderSpecifierKeyword(p.CurrentToken);
		Func<ParserBase, bool> abortFunction = (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.IsTerminator();
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, isNotExpectedFunction, abortFunction, expected);
	}

	private static bool IsValidOrderSpecifierKeyword(InternalSyntaxNode node)
	{
		if (node.Kind != SyntaxKind.AscendingKeyword)
		{
			return node.Kind == SyntaxKind.DescendingKeyword;
		}
		return true;
	}

	private InternalOrderByExpressionSyntax ParseOrderByElement()
	{
		InternalSyntaxToken orderSpecifier = EatToken(new List<SyntaxKind>
		{
			SyntaxKind.DescendingKeyword,
			SyntaxKind.AscendingKeyword
		}, ErrorCode.ERR_ExpectedAscendingOrDescendingKeyword);
		InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> sortingFields = ParseCommaSeparatedIdentifierNames(ref startToken);
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return ContextAwareSyntaxFactory.OrderByExpression(orderSpecifier, startToken, sortingFields, closeParenthesisToken);
	}

	private InternalPageCustomizationSyntax ParsePageCustomization()
	{
		InternalSyntaxToken pageCustomizationKeyword = EatToken(SyntaxKind.PageCustomizationKeyword);
		InternalObjectIdSyntax objectId = null;
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		GlobalLogger.LogVerbose("Parsing PageCustomization {0}.", internalIdentifierNameSyntax);
		InternalSyntaxToken customizesKeyword = EatToken(SyntaxKind.CustomizesKeyword);
		InternalObjectNameOrIdSyntax baseObject = ParseObjectReferenceSyntax();
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupPageProperty, asExtension: true);
		InternalPageExtensionLayoutSyntax layout = ParsePageExtensionLayout();
		InternalPageExtensionActionListSyntax actions = ParsePageExtensionActions();
		InternalPageExtensionViewListSyntax views = ParsePageExtensionViews();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList((string s) => (TriggerTypeInfo)null);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return ContextAwareSyntaxFactory.PageCustomization(pageCustomizationKeyword, objectId, internalIdentifierNameSyntax, customizesKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken);
	}

	private InternalProfileSyntax ParseProfile()
	{
		InternalSyntaxToken profileToken = EatToken(SyntaxKind.ProfileKeyword);
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		GlobalLogger.LogVerbose("Parsing Profile {0}.", internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupProfileProperty);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return ContextAwareSyntaxFactory.Profile(profileToken, internalIdentifierNameSyntax, openBraceToken, propertyList, default, closeBraceToken);
	}

	private InternalQuerySyntax ParseQuery()
	{
		InternalSyntaxToken queryKeyword = EatToken(SyntaxKind.QueryKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		GlobalLogger.LogVerbose("Parsing Query {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupQueryProperty);
		InternalQueryElementsSyntax elements = ParseQueryElements();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetQueryTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return ContextAwareSyntaxFactory.Query(queryKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, openBraceToken, propertyList, elements, members, closeBraceToken);
	}

	private InternalQueryElementsSyntax ParseQueryElements()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.QueryElements)
		{
			return (InternalQueryElementsSyntax)EatNode();
		}
		if (CurrentToken.Kind != SyntaxKind.QueryElementsKeyword)
		{
			return null;
		}
		InternalSyntaxToken queryElementsKeyword = EatToken(SyntaxKind.QueryElementsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalQueryDataItemSyntax> dataItems = ParseQueryDataItems();
		return ContextAwareSyntaxFactory.QueryElements(queryElementsKeyword, openBraceToken, dataItems, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalQueryDataItemSyntax> ParseQueryDataItems()
	{
		InternalSyntaxListBuilder<InternalQueryDataItemSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalQueryDataItemSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind == SyntaxKind.DataItemKeyword)
			{
				InternalQueryDataItemSyntax node = ParseQueryDataItem();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalQueryDataItemSyntax ParseQueryDataItem()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.DataItemKeyword)
		{
			return (InternalQueryDataItemSyntax)EatNode();
		}
		InternalSyntaxToken dataItemKeyword = EatToken(SyntaxKind.DataItemKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalObjectNameOrIdSyntax dataItemTable = ParseObjectReferenceSyntax();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupQueryDataItemProperty);
		InternalSyntaxList<InternalQueryDataItemElementSyntax> elements = ParseQueryDataItemElementList();
		return ContextAwareSyntaxFactory.QueryDataItem(dataItemKeyword, openParenthesisToken, name, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalQueryDataItemElementSyntax> ParseQueryDataItemElementList()
	{
		InternalSyntaxListBuilder<InternalQueryDataItemElementSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalQueryDataItemElementSyntax>();
		try
		{
			while (!IsEndOfFile && IsCurrentTokenDataItemElement())
			{
				internalSyntaxListBuilder.Add(ParseQueryDataItemElement());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private bool IsCurrentTokenDataItemElement()
	{
		if (CurrentToken.Kind != SyntaxKind.DataItemKeyword && CurrentToken.Kind != SyntaxKind.ColumnKeyword)
		{
			return CurrentToken.Kind == SyntaxKind.FilterKeyword;
		}
		return true;
	}

	private InternalQueryDataItemElementSyntax ParseQueryDataItemElement()
	{
		if (IsIncremental && IsCurrentNodeDataItemElement())
		{
			return (InternalQueryDataItemElementSyntax)EatNode();
		}
		return CurrentToken.Kind switch
		{
			SyntaxKind.DataItemKeyword => (InternalQueryDataItemElementSyntax)ParseQueryDataItem(), 
			SyntaxKind.ColumnKeyword => ParseQueryColumn(), 
			SyntaxKind.FilterKeyword => ParseQueryFilter(), 
			_ => throw ExceptionUtilities.UnexpectedValue(CurrentToken.Kind), 
		};
	}

	private bool IsCurrentNodeDataItemElement()
	{
		if (CurrentNodeKind != SyntaxKind.DataItemKeyword && CurrentNodeKind != SyntaxKind.ColumnKeyword)
		{
			return CurrentNodeKind == SyntaxKind.FilterKeyword;
		}
		return true;
	}

	private InternalQueryColumnSyntax ParseQueryColumn()
	{
		InternalSyntaxToken columnKeyword = EatToken(SyntaxKind.ColumnKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken ct = CurrentToken;
		ParseOptionalQueryColumnSourceExpression(ct, out var semiColon, out var sourceExpression);
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupQueryColumnProperty);
		return ContextAwareSyntaxFactory.QueryColumn(columnKeyword, openParenthesisToken, name, semiColon, sourceExpression, closeParenthesisToken, openBraceToken, propertyList, EatToken(SyntaxKind.CloseBraceToken));
	}

	private void ParseOptionalQueryColumnSourceExpression(InternalSyntaxToken ct, out InternalSyntaxToken semiColon, out InternalIdentifierNameSyntax sourceExpression)
	{
		if (ct.Kind == SyntaxKind.SemicolonToken)
		{
			semiColon = EatToken(SyntaxKind.SemicolonToken);
			sourceExpression = ParseIdentifierName();
		}
		else
		{
			semiColon = null;
			sourceExpression = null;
		}
	}

	private InternalQueryFilterSyntax ParseQueryFilter()
	{
		InternalSyntaxToken filterKeyword = EatToken(SyntaxKind.FilterKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalIdentifierNameSyntax relatedField = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupQueryFilterProperty);
		return ContextAwareSyntaxFactory.QueryFilter(filterKeyword, openParenthesisToken, name, semicolonToken, relatedField, closeParenthesisToken, openBraceToken, propertyList, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalXmlPortSyntax ParseXmlPort()
	{
		InternalSyntaxToken xmlPortKeyword = EatToken(SyntaxKind.XmlPortKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		GlobalLogger.LogVerbose("Parsing XmlPort {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupXmlPortProperty);
		InternalXmlPortSchemaSyntax xmlPortSchema = ParseXmlPortLayout();
		InternalRequestPageSyntax xmlPortRequestPage = ParseXmlPortRequestPage();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetXmlPortTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return ContextAwareSyntaxFactory.XmlPort(xmlPortKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, openBraceToken, propertyList, xmlPortSchema, xmlPortRequestPage, members, closeBraceToken);
	}

	private InternalRequestPageSyntax ParseXmlPortRequestPage()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.RequestPage)
		{
			return (InternalRequestPageSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = CurrentToken;
		if (internalSyntaxToken == null || internalSyntaxToken.Kind != SyntaxKind.RequestPageKeyword)
		{
			return null;
		}
		InternalSyntaxToken requestPageKeyword = EatToken(SyntaxKind.RequestPageKeyword);
		InternalSyntaxToken internalSyntaxToken2 = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupPageProperty);
		InternalPageLayoutSyntax layout = ParsePageLayout();
		InternalPageActionListSyntax actions = ParsePageActions();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetPageTriggerInfo);
		InternalSyntaxToken node;
		if (internalSyntaxToken2.IsMissing)
		{
			node = InternalSyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken);
			node = WithAdditionalDiagnostics(node, GetExpectedTokenError(SyntaxKind.CloseBraceToken, CurrentToken.Kind));
		}
		else
		{
			node = EatToken(SyntaxKind.CloseBraceToken);
		}
		return ContextAwareSyntaxFactory.RequestPage(requestPageKeyword, internalSyntaxToken2, null, propertyList, layout, actions, members, node);
	}

	private InternalXmlPortSchemaSyntax ParseXmlPortLayout()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.XmlPortSchema)
		{
			return (InternalXmlPortSchemaSyntax)EatNode();
		}
		if (CurrentToken.Kind != SyntaxKind.XmlPortSchemaKeyword)
		{
			return null;
		}
		InternalSyntaxToken xmlPortSchemaKeyword = EatToken(SyntaxKind.XmlPortSchemaKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalXmlPortNodeSyntax> xmlPortSchema = ParseXmlPortSchemaList();
		return ContextAwareSyntaxFactory.XmlPortSchema(xmlPortSchemaKeyword, openBraceToken, xmlPortSchema, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalXmlPortTableElementSyntax ParseXmlPortTableElement()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.XmlPortTableElementKeyword)
		{
			return (InternalXmlPortTableElementSyntax)EatNode();
		}
		InternalSyntaxToken xmlPortTableElementKeyword = EatToken(SyntaxKind.XmlPortTableElementKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalObjectNameOrIdSyntax sourceTable = ParseObjectReferenceSyntax();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupXmlPortTableElementProperty);
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = ParseXmlPortSchemaList();
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetXmlPortTableElementTriggerInfo);
		return ContextAwareSyntaxFactory.XmlPortTableElement(xmlPortTableElementKeyword, openParenthesisToken, name, semicolonToken, sourceTable, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalXmlPortNodeSyntax> ParseXmlPortSchemaList()
	{
		InternalSyntaxListBuilder<InternalXmlPortNodeSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalXmlPortNodeSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind.IsXmlPortNodeKeyword())
			{
				internalSyntaxListBuilder.Add(ParseXmlPortNode());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalXmlPortNodeSyntax ParseXmlPortNode()
	{
		if (IsIncremental && CurrentToken.Kind.IsXmlPortNode())
		{
			return (InternalXmlPortNodeSyntax)EatNode();
		}
		return CurrentToken.Kind switch
		{
			SyntaxKind.XmlPortTableElementKeyword => (InternalXmlPortNodeSyntax)ParseXmlPortTableElement(), 
			SyntaxKind.XmlPortFieldElementKeyword => ParseXmlPortFieldElement(), 
			SyntaxKind.XmlPortTextElementKeyword => ParseXmlPortTextElement(), 
			SyntaxKind.XmlPortFieldAttributeKeyword => ParseXmlPortFieldAttribute(), 
			SyntaxKind.XmlPortTextAttributeKeyword => ParseXmlPortTextAttribute(), 
			_ => throw ExceptionUtilities.UnexpectedValue(CurrentToken.Kind), 
		};
	}

	private InternalXmlPortFieldNodeSyntax ParseXmlPortFieldElement()
	{
		InternalSyntaxToken xmlPortFieldElementKeyword = EatToken(SyntaxKind.XmlPortFieldElementKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalCodeExpressionSyntax sourceField = ParseMemberAccessExpressionOrIdentifier();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupXmlPortFieldElementProperty);
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = ParseXmlPortSchemaList();
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetXmlPortFieldElementTriggerInfo);
		return ContextAwareSyntaxFactory.XmlPortFieldElement(xmlPortFieldElementKeyword, openParenthesisToken, name, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalXmlPortFieldNodeSyntax ParseXmlPortFieldAttribute()
	{
		InternalSyntaxToken xmlPortFieldAttributeKeyword = EatToken(SyntaxKind.XmlPortFieldAttributeKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalCodeExpressionSyntax sourceField = ParseMemberAccessExpressionOrIdentifier();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupXmlPortFieldAttributeProperty);
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = ParseXmlPortSchemaList();
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetXmlPortFieldAttributeTriggerInfo);
		return ContextAwareSyntaxFactory.XmlPortFieldAttribute(xmlPortFieldAttributeKeyword, openParenthesisToken, name, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalXmlPortTextNodeSyntax ParseXmlPortTextElement()
	{
		InternalSyntaxToken xmlPortTextElementKeyword = EatToken(SyntaxKind.XmlPortTextElementKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupXmlPortTextElementProperty);
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = ParseXmlPortSchemaList();
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetXmlPortTextElementTriggerInfo);
		return ContextAwareSyntaxFactory.XmlPortTextElement(xmlPortTextElementKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalXmlPortTextNodeSyntax ParseXmlPortTextAttribute()
	{
		InternalSyntaxToken xmlPortTextAttributeKeyword = EatToken(SyntaxKind.XmlPortTextAttributeKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupXmlPortTextAttributeProperty);
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = ParseXmlPortSchemaList();
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetXmlPortTextAttributeTriggerInfo);
		return ContextAwareSyntaxFactory.XmlPortTextAttribute(xmlPortTextAttributeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalReportSyntax ParseReport()
	{
		InternalSyntaxToken reportKeyword = EatToken(SyntaxKind.ReportKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		GlobalLogger.LogVerbose("Parsing Report {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupReportProperty);
		InternalReportDataSetSectionSyntax dataSet = ParseReportDataSetSection();
		InternalRequestPageSyntax requestPage = ParseRequestPage();
		InternalReportLabelsSectionSyntax labels = ParseReportLabelsSection();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetReportTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return ContextAwareSyntaxFactory.Report(reportKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, openBraceToken, propertyList, dataSet, requestPage, labels, members, closeBraceToken);
	}

	private InternalRequestPageSyntax ParseRequestPage()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.RequestPage)
		{
			return (InternalRequestPageSyntax)EatNode();
		}
		if (CurrentToken.Kind != SyntaxKind.RequestPageKeyword)
		{
			return null;
		}
		InternalSyntaxToken requestPageKeyword = EatToken(SyntaxKind.RequestPageKeyword);
		InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupRequestPageProperty);
		InternalPageLayoutSyntax layout = ParsePageLayout();
		InternalPageActionListSyntax actions = ParsePageActions();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetPageTriggerInfo);
		InternalSyntaxToken node;
		if (internalSyntaxToken.IsMissing)
		{
			node = InternalSyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken);
			node = WithAdditionalDiagnostics(node, GetExpectedTokenError(SyntaxKind.CloseBraceToken, CurrentToken.Kind));
		}
		else
		{
			node = EatToken(SyntaxKind.CloseBraceToken);
		}
		return ContextAwareSyntaxFactory.RequestPage(requestPageKeyword, internalSyntaxToken, null, propertyList, layout, actions, members, node);
	}

	private InternalReportDataSetSectionSyntax ParseReportDataSetSection()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.ReportDataSetSection)
		{
			return (InternalReportDataSetSectionSyntax)EatNode();
		}
		if (CurrentToken.Kind != SyntaxKind.DataSetKeyword)
		{
			return null;
		}
		InternalSyntaxToken dataSetKeyword = EatToken(SyntaxKind.DataSetKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalReportDataItemSyntax> dataItems = ParseReportDataItems();
		return ContextAwareSyntaxFactory.ReportDataSetSection(dataSetKeyword, openBraceToken, dataItems, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalReportDataItemSyntax> ParseReportDataItems()
	{
		InternalSyntaxListBuilder<InternalReportDataItemSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalReportDataItemSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind == SyntaxKind.DataItemKeyword)
			{
				InternalReportDataItemSyntax node = ParseReportDataItem();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalReportDataItemSyntax ParseReportDataItem()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.DataItemKeyword)
		{
			return (InternalReportDataItemSyntax)EatNode();
		}
		InternalSyntaxToken dataItemKeyword = EatToken(SyntaxKind.DataItemKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalObjectNameOrIdSyntax dataItemTable = ParseObjectReferenceSyntax();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupReportDataItemProperty);
		InternalSyntaxList<InternalReportDataItemElementSyntax> elements = ParseDataItemElementList();
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetReportDataItemTriggerInfo);
		return ContextAwareSyntaxFactory.ReportDataItem(dataItemKeyword, openParenthesisToken, name, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalReportDataItemElementSyntax> ParseDataItemElementList()
	{
		InternalSyntaxListBuilder<InternalReportDataItemElementSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalReportDataItemElementSyntax>();
		try
		{
			while (!IsEndOfFile && (CurrentToken.Kind == SyntaxKind.DataItemKeyword || CurrentToken.Kind == SyntaxKind.ColumnKeyword))
			{
				internalSyntaxListBuilder.Add(ParseReportDataItemElement());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalReportDataItemElementSyntax ParseReportDataItemElement()
	{
		if (IsIncremental && (CurrentNodeKind == SyntaxKind.DataItemKeyword || CurrentNodeKind == SyntaxKind.ColumnKeyword))
		{
			return (InternalReportDataItemElementSyntax)EatNode();
		}
		return CurrentToken.Kind switch
		{
			SyntaxKind.DataItemKeyword => (InternalReportDataItemElementSyntax)ParseReportDataItem(), 
			SyntaxKind.ColumnKeyword => ParseReportColumn(), 
			_ => throw ExceptionUtilities.UnexpectedValue(CurrentToken.Kind), 
		};
	}

	private InternalReportColumnSyntax ParseReportColumn()
	{
		InternalSyntaxToken columnKeyword = EatToken(SyntaxKind.ColumnKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalCodeExpressionSyntax sourceExpression = ParseInLexerMode(LexerMode.Expression, ParseExpression);
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupReportColumnProperty);
		return ContextAwareSyntaxFactory.ReportColumn(columnKeyword, openParenthesisToken, name, semicolonToken, sourceExpression, closeParenthesisToken, openBraceToken, propertyList, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalReportLabelsSectionSyntax ParseReportLabelsSection()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.ReportLabelsSection)
		{
			return (InternalReportLabelsSectionSyntax)EatNode();
		}
		if (CurrentToken.Kind != SyntaxKind.LabelsKeyword)
		{
			return null;
		}
		InternalSyntaxToken labelsKeyword = EatToken(SyntaxKind.LabelsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalReportLabelBaseSyntax> labels = ParseReportLabels();
		return ContextAwareSyntaxFactory.ReportLabelsSection(labelsKeyword, openBraceToken, labels, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalReportLabelBaseSyntax> ParseReportLabels()
	{
		InternalSyntaxListBuilder<InternalReportLabelBaseSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalReportLabelBaseSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind.IsTokenIdentifier())
			{
				internalSyntaxListBuilder.Add(ParseReportLabel());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalReportLabelBaseSyntax ParseReportLabel()
	{
		if (CurrentToken.Kind == SyntaxKind.LabelKeyword)
		{
			InternalSyntaxToken labelKeyword = EatToken(SyntaxKind.LabelKeyword);
			InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax name = ParseIdentifierName();
			InternalSyntaxToken startToken = EatToken(SyntaxKind.SemicolonToken);
			InternalCommaSeparatedIdentifierEqualsStringListSyntax caption = ParseCommaSeparatedIdentifierEqualsStringList(ref startToken, SyntaxKind.CloseParenToken);
			InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
			return ContextAwareSyntaxFactory.ReportLabelMultilanguage(labelKeyword, openParenthesisToken, name, startToken, caption, closeParenthesisToken);
		}
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalLabelSyntax label = ParseLabel();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		return ContextAwareSyntaxFactory.ReportLabel(name2, equalsToken, label, semicolonToken);
	}

	private InternalTableExtensionSyntax ParseTableExtension()
	{
		InternalSyntaxToken tableExtensionKeyword = EatToken(SyntaxKind.TableExtensionKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		GlobalLogger.LogVerbose("Parsing TableExtension {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken extendsKeyword = EatToken(SyntaxKind.ExtendsKeyword);
		InternalObjectNameOrIdSyntax baseObject = ParseObjectReferenceSyntax();
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupTableProperty, asExtension: true);
		InternalFieldExtensionListSyntax fields = ((CurrentToken.Kind == SyntaxKind.FieldsKeyword) ? ParseFieldExtensionList() : null);
		InternalKeyListSyntax keys = ((CurrentToken.Kind == SyntaxKind.KeysKeyword) ? ParseKeyList() : null);
		InternalFieldGroupExtensionListSyntax fieldGroups = ((CurrentToken.Kind == SyntaxKind.FieldGroupsKeyword) ? ParseFieldGroupExtensionList() : null);
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetTableExtensionTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return ContextAwareSyntaxFactory.TableExtension(tableExtensionKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, extendsKeyword, baseObject, openBraceToken, propertyList, fields, keys, fieldGroups, members, closeBraceToken);
	}

	private InternalFieldExtensionListSyntax ParseFieldExtensionList()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.FieldExtensionList)
		{
			return (InternalFieldExtensionListSyntax)EatNode();
		}
		InternalSyntaxToken fieldsKeyword = EatToken(SyntaxKind.FieldsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalFieldBaseSyntax> fields = ParseExtensionFields();
		return ContextAwareSyntaxFactory.FieldExtensionList(fieldsKeyword, openBraceToken, fields, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalFieldBaseSyntax> ParseExtensionFields()
	{
		InternalSyntaxListBuilder<InternalFieldBaseSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalFieldBaseSyntax>();
		try
		{
			while (!IsEndOfFile)
			{
				InternalFieldBaseSyntax internalFieldBaseSyntax = null;
				switch (CurrentToken.Kind)
				{
				case SyntaxKind.FieldKeyword:
					internalFieldBaseSyntax = ParseField();
					break;
				case SyntaxKind.ModifyKeyword:
					internalFieldBaseSyntax = ParseFieldModification();
					break;
				}
				if (internalFieldBaseSyntax == null)
				{
					break;
				}
				internalSyntaxListBuilder.Add(internalFieldBaseSyntax);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalFieldBaseSyntax ParseFieldModification()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.FieldModification)
		{
			return (InternalFieldBaseSyntax)EatNode();
		}
		InternalSyntaxToken fieldKeyword = EatToken(SyntaxKind.ModifyKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupFieldProperty);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetFieldExtensionTriggerInfo);
		return ContextAwareSyntaxFactory.FieldModification(fieldKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalFieldGroupExtensionListSyntax ParseFieldGroupExtensionList()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.FieldGroupExtensionList)
		{
			return (InternalFieldGroupExtensionListSyntax)EatNode();
		}
		InternalSyntaxToken fieldGroupsKeyword = EatToken(SyntaxKind.FieldGroupsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalFieldGroupChangeBaseSyntax> changes = ParseChangeFieldGroupList();
		InternalFieldGroupExtensionListSyntax node = ContextAwareSyntaxFactory.FieldGroupExtensionList(fieldGroupsKeyword, openBraceToken, changes, EatToken(SyntaxKind.CloseBraceToken));
		return CheckFeatureAvailability(node, Feature.FieldGroupExtension);
	}

	private InternalSyntaxList<InternalFieldGroupChangeBaseSyntax> ParseChangeFieldGroupList()
	{
		InternalSyntaxListBuilder<InternalFieldGroupChangeBaseSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalFieldGroupChangeBaseSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind.IsFieldGroupChangeKeyword())
			{
				internalSyntaxListBuilder.Add(ParseFieldGroupChange());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalFieldGroupChangeBaseSyntax ParseFieldGroupChange()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.FieldGroupAddChange)
		{
			return (InternalFieldGroupChangeBaseSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatToken();
		SyntaxKind kind = internalSyntaxToken.Kind;
		if (kind == SyntaxKind.AddLastKeyword)
		{
			InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor = ParseIdentifierName();
			InternalSyntaxToken startToken = EatToken(SyntaxKind.SemicolonToken);
			InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields = ParseCommaSeparatedIdentifierNames(ref startToken, SyntaxKind.CloseParenToken);
			InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
			InternalPropertyListSyntax propertyList = ContextAwareSyntaxFactory.PropertyList(default);
			InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			return ContextAwareSyntaxFactory.FieldGroupAddChange(internalSyntaxToken, openParenthesisToken, anchor, startToken, fields, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
		}
		throw ExceptionUtilities.UnexpectedValue(internalSyntaxToken.Kind);
	}

	private InternalSyntaxList<InternalMemberAttributeSyntax> ParseMemberAttributeList()
	{
		InternalSyntaxListBuilder<InternalMemberAttributeSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalMemberAttributeSyntax>();
		try
		{
			while (CurrentToken.Kind == SyntaxKind.OpenBracketToken)
			{
				InternalMemberAttributeSyntax node = ParseAttribute();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalMemberAttributeSyntax ParseAttribute()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.MemberAttribute)
		{
			return (InternalMemberAttributeSyntax)EatNode();
		}
		InternalSyntaxToken openBracketToken = EatToken(SyntaxKind.OpenBracketToken);
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		AttributeTypeInfo attributeInfo = AttributeDefinitions.GetAttributeInfo(internalIdentifierNameSyntax.Identifier.GetValueText());
		InternalAttributeArgumentListSyntax argumentList = ParseAttributeArgumentList();
		InternalSyntaxToken closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
		InternalMemberAttributeSyntax internalMemberAttributeSyntax = ContextAwareSyntaxFactory.MemberAttribute(openBracketToken, internalIdentifierNameSyntax, argumentList, closeBracketToken);
		if (attributeInfo != null)
		{
			internalMemberAttributeSyntax = internalMemberAttributeSyntax.WithAnnotations(new SyntaxAnnotation[1] { attributeInfo.AttributeAnnotation });
		}
		return internalMemberAttributeSyntax;
	}

	private void SkipOrphanedAttributes(ref InternalSyntaxNode lastGoodNode, InternalSyntaxList<InternalMemberAttributeSyntax> orphanedAttributeList)
	{
		for (int i = 0; i < orphanedAttributeList.Count; i++)
		{
			lastGoodNode = AddTrailingSkippedSyntax(lastGoodNode, AddError(orphanedAttributeList[i], ErrorCode.ERR_AttributeNotOnAValidSymbol));
		}
	}

	private InternalAttributeArgumentListSyntax ParseAttributeArgumentList()
	{
		InternalAttributeArgumentListSyntax result = null;
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenParenToken);
			InternalSeparatedSyntaxList<InternalAttributeArgumentSyntax> arguments = ParseAttributeArguments(ref startToken);
			result = ContextAwareSyntaxFactory.AttributeArgumentList(startToken, arguments, EatToken(SyntaxKind.CloseParenToken));
		}
		return result;
	}

	private InternalSeparatedSyntaxList<InternalAttributeArgumentSyntax> ParseAttributeArguments(ref InternalSyntaxToken startToken)
	{
		return ParseSeparatedList(ref startToken, SyntaxKind.Int32LiteralToken, (InternalSyntaxToken token) => token.IsPossibleAttributeArgument(), SyntaxKind.CommaToken, SyntaxKind.CloseParenToken, SkipBadAttributeArgumentTokens, ParseAttributeArgument);
	}

	private PostSkipAction SkipBadAttributeArgumentTokens(ref InternalSyntaxToken openParen, InternalSeparatedSyntaxListBuilder<InternalAttributeArgumentSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref openParen, list, (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || (expected != SyntaxKind.CommaToken && !CurrentToken.IsPossibleAttributeArgument()), (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.CurrentToken.Kind == SyntaxKind.CloseBracketToken || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken || p.CurrentToken.Kind == SyntaxKind.LocalKeyword || p.CurrentToken.Kind == SyntaxKind.ProcedureKeyword || p.CurrentToken.Kind == SyntaxKind.TriggerKeyword || p.CurrentToken.Kind == SyntaxKind.EndOfFileToken || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken, expected);
	}

	private InternalAttributeArgumentSyntax ParseAttributeArgument()
	{
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.Int32LiteralToken:
		case SyntaxKind.StringLiteralToken:
			return ContextAwareSyntaxFactory.LiteralAttributeArgument(ContextAwareSyntaxFactory.LiteralExpression(ParseLiteralValue()));
		case SyntaxKind.IdentifierToken:
		{
			InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseVar();
			if (internalCodeExpressionSyntax.Kind == SyntaxKind.OptionAccessExpression)
			{
				return ContextAwareSyntaxFactory.OptionAccessAttributeArgument(internalCodeExpressionSyntax as InternalOptionAccessExpressionSyntax);
			}
			return AddError(ContextAwareSyntaxFactory.InvalidAttributeArgument(internalCodeExpressionSyntax), ErrorCode.ERR_InvalidAttributeArgumentSyntax, internalCodeExpressionSyntax.ToString());
		}
		default:
			return AddError(ContextAwareSyntaxFactory.InvalidAttributeArgument(ParserBase.MissingToken(SyntaxKind.InvalidAttributeArgument)), ErrorCode.ERR_InvalidAttributeArgumentSyntax, CurrentToken.Text);
		}
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	internal InternalPropertyValueSyntax ParseTableViewPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		InternalSortingSyntax sorting = ParseSorting();
		InternalOrderSyntax order = ParseOrder();
		InternalWhereExpressionSyntax tableFilter = ParseTableFilters();
		return ContextAwareSyntaxFactory.TableViewPropertyValue(sorting, order, tableFilter);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	private InternalPropertyValueSyntax ParseFiltersPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		InternalWhereExpressionSyntax tableFilter = ParseTableFilters(tokenExpected: true);
		return ContextAwareSyntaxFactory.FiltersPropertyValue(tableFilter);
	}

	private InternalOrderSyntax ParseOrder()
	{
		InternalSyntaxToken internalSyntaxToken = EatTokenIfKind(SyntaxKind.OrderKeyword);
		if (internalSyntaxToken == null)
		{
			return null;
		}
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSyntaxToken orderBy = EatToken(new List<SyntaxKind>
		{
			SyntaxKind.DescendingKeyword,
			SyntaxKind.AscendingKeyword
		}, ErrorCode.ERR_ExpectedAscendingOrDescendingKeyword);
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return ContextAwareSyntaxFactory.Order(internalSyntaxToken, openParenthesisToken, orderBy, closeParenthesisToken);
	}

	private InternalSortingSyntax ParseSorting()
	{
		InternalSyntaxToken internalSyntaxToken = EatTokenIfKind(SyntaxKind.SortingKeyword);
		if (internalSyntaxToken == null)
		{
			return null;
		}
		InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> sortingFields = ParseCommaSeparatedIdentifierNames(ref startToken);
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return ContextAwareSyntaxFactory.Sorting(internalSyntaxToken, startToken, sortingFields, closeParenthesisToken);
	}

	private InternalPageExtensionSyntax ParsePageExtension()
	{
		InternalSyntaxToken pageExtensionKeyword = EatToken(SyntaxKind.PageExtensionKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		GlobalLogger.LogVerbose("Parsing PageExtension {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken extendsKeyword = EatToken(SyntaxKind.ExtendsKeyword);
		InternalObjectNameOrIdSyntax baseObject = ParseObjectReferenceSyntax();
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupPageProperty, asExtension: true);
		InternalPageExtensionLayoutSyntax layout = ParsePageExtensionLayout();
		InternalPageExtensionActionListSyntax actions = ParsePageExtensionActions();
		InternalPageExtensionViewListSyntax views = ParsePageExtensionViews();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetPageExtensionTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return ContextAwareSyntaxFactory.PageExtension(pageExtensionKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, extendsKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken);
	}

	[SuppressMessage("Nav.Reliability", "DN0002:AvoidImplictTimeZoneConversions", MessageId = "System.DateTime.get_Now")]
	private InternalCodeunitSyntax ParseCodeunit()
	{
		InternalSyntaxToken codeunitKeyword = EatToken(SyntaxKind.CodeunitKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		GlobalLogger.LogVerbose("Parsing Codeunit {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList;
		InternalSyntaxList<InternalMemberSyntax> members;
		if (!internalIdentifierNameSyntax.IsMissing && !internalSyntaxToken.IsMissing)
		{
			propertyList = ParsePropertyList(LookupCodeunitProperty);
			members = ParseMemberList(TriggerDefinitions.GetCodeunitTriggerInfo);
		}
		else
		{
			propertyList = ContextAwareSyntaxFactory.PropertyList(default);
			members = default;
		}
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(internalSyntaxToken);
		return ContextAwareSyntaxFactory.Codeunit(codeunitKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, internalSyntaxToken, propertyList, members, closeBraceToken);
	}

	private InternalPageSyntax ParsePage()
	{
		InternalSyntaxToken pageKeyword = EatToken(SyntaxKind.PageKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		GlobalLogger.LogVerbose("Parsing Page {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupPageProperty);
		InternalPageLayoutSyntax layout = ParsePageLayout();
		InternalPageActionListSyntax actions = ParsePageActions();
		InternalPageViewListSyntax views = ParsePageViews();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetPageTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return ContextAwareSyntaxFactory.Page(pageKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	internal InternalPropertyValueSyntax ParseCalculationFormulaPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		InternalSyntaxToken sign = EatTokenIfKind(SyntaxKind.MinusToken);
		InternalSyntaxToken internalSyntaxToken = EatTokenIfCondition((SyntaxKind kind) => kind.IsCalculationFormulaMethodKind());
		if (internalSyntaxToken == null)
		{
			internalSyntaxToken = InternalSyntaxFactory.MissingToken(SyntaxKind.LookupCalculationFormulaKeyword);
			internalSyntaxToken = AddError(internalSyntaxToken, ErrorCode.ERR_InvalidCalculationFormulaMethod);
		}
		SyntaxKind calcFormulaStatementKind = GetCalcFormulaStatementKind(internalSyntaxToken.Kind);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalCodeExpressionSyntax internalCodeExpressionSyntax = null;
		InternalIdentifierNameSyntax table = null;
		if (internalSyntaxToken.Kind == SyntaxKind.ExistCalculationFormulaKeyword || internalSyntaxToken.Kind == SyntaxKind.CountCalculationFormulaKeyword)
		{
			table = ParseIdentifierName();
		}
		else
		{
			internalCodeExpressionSyntax = ParseMemberAccessExpressionOrIdentifier();
			if (internalCodeExpressionSyntax.Kind != SyntaxKind.MemberAccessExpression)
			{
				internalCodeExpressionSyntax = ContextAwareSyntaxFactory.MemberAccessExpression(internalCodeExpressionSyntax, CreateMissingToken(SyntaxKind.DotToken, internalCodeExpressionSyntax.Kind, !internalSyntaxToken.ContainsDiagnostics), ParserBase.CreateMissingIdentifierName());
			}
		}
		InternalWhereExpressionSyntax whereExpression = ParseTableFilters();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		if (internalSyntaxToken.Kind == SyntaxKind.ExistCalculationFormulaKeyword || internalSyntaxToken.Kind == SyntaxKind.CountCalculationFormulaKeyword)
		{
			return ContextAwareSyntaxFactory.TableCalculationFormula(calcFormulaStatementKind, sign, internalSyntaxToken, openParenthesisToken, table, whereExpression, closeParenthesisToken);
		}
		return ContextAwareSyntaxFactory.FieldCalculationFormula(calcFormulaStatementKind, sign, internalSyntaxToken, openParenthesisToken, internalCodeExpressionSyntax as InternalMemberAccessExpressionSyntax, whereExpression, closeParenthesisToken);
	}

	private InternalPropertyExpressionSyntax ParsePropertyExpression()
	{
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalSyntaxToken internalSyntaxToken = EatTokenIfCondition((SyntaxKind k) => k == SyntaxKind.FieldFormulaKeyword || k == SyntaxKind.ConstFormulaKeyword || k == SyntaxKind.FilterFormulaKeyword);
		if (internalSyntaxToken != null)
		{
			switch (internalSyntaxToken.Kind)
			{
			case SyntaxKind.FieldFormulaKeyword:
				return ParseFieldPropertyExpression(internalIdentifierNameSyntax, equalsToken, internalSyntaxToken);
			case SyntaxKind.ConstFormulaKeyword:
				return ParseConstPropertyExpression(internalIdentifierNameSyntax, equalsToken, internalSyntaxToken);
			case SyntaxKind.FilterFormulaKeyword:
				return ParseFilterExpression(internalIdentifierNameSyntax, equalsToken, internalSyntaxToken);
			}
		}
		return ContextAwareSyntaxFactory.InvalidPropertyExpression(internalIdentifierNameSyntax, equalsToken, null, AddError(EatToken(SyntaxKind.FieldKeyword), ErrorCode.ERR_ExpectedFieldFilterOrConstKeyword));
	}

	private InternalPropertyExpressionSyntax ParseFieldPropertyExpression(InternalIdentifierNameSyntax lhs, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeyword)
	{
		InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSyntaxToken internalSyntaxToken2 = EatTokenIfCondition((SyntaxKind k) => k == SyntaxKind.FilterFormulaKeyword || k == SyntaxKind.UpperLimitFormulaKeyword);
		if (internalSyntaxToken2 == null)
		{
			internalSyntaxToken2 = ParseIdentifierToken();
		}
		if (internalSyntaxToken2.Kind == SyntaxKind.IdentifierToken)
		{
			return ContextAwareSyntaxFactory.SimpleFieldExpression(lhs, equalsToken, fieldKeyword, internalSyntaxToken, ContextAwareSyntaxFactory.IdentifierName(internalSyntaxToken2), EatToken(SyntaxKind.CloseParenToken));
		}
		return internalSyntaxToken2.Kind switch
		{
			SyntaxKind.FilterFormulaKeyword => ContextAwareSyntaxFactory.FieldFilterExpression(lhs, equalsToken, fieldKeyword, internalSyntaxToken, internalSyntaxToken2, EatToken(SyntaxKind.OpenParenToken), ParseIdentifierName(), EatToken(SyntaxKind.CloseParenToken), EatToken(SyntaxKind.CloseParenToken)), 
			SyntaxKind.UpperLimitFormulaKeyword => ParseFieldUpperLimitFilterExpressionSyntax(lhs, equalsToken, fieldKeyword, internalSyntaxToken, internalSyntaxToken2), 
			_ => ContextAwareSyntaxFactory.InvalidPropertyExpression(lhs, equalsToken, fieldKeyword, internalSyntaxToken2), 
		};
	}

	private InternalPropertyExpressionSyntax ParseFieldUpperLimitFilterExpressionSyntax(InternalIdentifierNameSyntax lhs, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeyword, InternalSyntaxToken openParToken, InternalSyntaxToken upperLimitKeyword)
	{
		InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxKind.OpenParenToken);
		if (CurrentToken.Kind == SyntaxKind.FilterFormulaKeyword)
		{
			return ContextAwareSyntaxFactory.FieldUpperLimitFilterExpression(lhs, equalsToken, fieldKeyword, openParToken, upperLimitKeyword, internalSyntaxToken, EatToken(SyntaxKind.FilterFormulaKeyword), EatToken(SyntaxKind.OpenParenToken), ParseIdentifierName(), EatToken(SyntaxKind.CloseParenToken), EatToken(SyntaxKind.CloseParenToken), EatToken(SyntaxKind.CloseParenToken));
		}
		return ContextAwareSyntaxFactory.FieldUpperLimitExpression(lhs, equalsToken, fieldKeyword, openParToken, upperLimitKeyword, internalSyntaxToken, ParseIdentifierName(), EatToken(SyntaxKind.CloseParenToken), EatToken(SyntaxKind.CloseParenToken));
	}

	private InternalPropertyExpressionSyntax ParseConstPropertyExpression(InternalIdentifierNameSyntax lhs, InternalSyntaxToken equalsToken, InternalSyntaxToken constKeyword)
	{
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierOrLiteralExpressionSyntax identifier = TryParseIdentifierOrLiteralExpression();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return ContextAwareSyntaxFactory.ConstExpression(lhs, equalsToken, constKeyword, openParenthesisToken, identifier, closeParenthesisToken);
	}

	[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax.ObjectParser.TryParseFilterValue(Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax.InternalFilterExpressionValueSyntax@,Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax.InternalSyntaxToken@)")]
	private InternalFilterExpressionSyntax ParseFilterExpression(InternalIdentifierNameSyntax lhs, InternalSyntaxToken equalsToken, InternalSyntaxToken filterFormulaKeyword)
	{
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalFilterExpressionValueSyntax internalFilterExpressionValueSyntax = ParseFilterExpressionValue();
		DebugAssertHelper.Assert(internalFilterExpressionValueSyntax != null);
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return ContextAwareSyntaxFactory.FilterExpression(lhs, equalsToken, filterFormulaKeyword, openParenthesisToken, internalFilterExpressionValueSyntax, closeParenthesisToken);
	}

	private static SyntaxKind GetCalcFormulaStatementKind(SyntaxKind calcformulaMethodKind)
	{
		return calcformulaMethodKind switch
		{
			SyntaxKind.AverageCalculationFormulaKeyword => SyntaxKind.AverageCalculationFormulaStatement, 
			SyntaxKind.CountCalculationFormulaKeyword => SyntaxKind.CountCalculationFormulaStatement, 
			SyntaxKind.MinCalculationFormulaKeyword => SyntaxKind.MinCalculationFormulaStatement, 
			SyntaxKind.MaxCalculationFormulaKeyword => SyntaxKind.MaxCalculationFormulaStatement, 
			SyntaxKind.ExistCalculationFormulaKeyword => SyntaxKind.ExistCalculationFormulaStatement, 
			SyntaxKind.LookupCalculationFormulaKeyword => SyntaxKind.LookupCalculationFormulaStatement, 
			SyntaxKind.SumCalculationFormulaKeyword => SyntaxKind.SumCalculationFormulaStatement, 
			_ => SyntaxKind.None, 
		};
	}

	private InternalTableRelationPropertyValueSyntax ParseTableRelationPropertyValue(PropertyTypeInfo pti)
	{
		string name = pti.Name;
		InternalIfTableRelationSyntax internalIfTableRelationSyntax = ParseIfTableRelation();
		InternalMemberAccessOrIdentifierPropertyValueSyntax relatedTableField = ParseMemberAccessOrIdentifierPropertyValue();
		InternalWhereExpressionSyntax tableFilter = ParseTableFilters();
		InternalSyntaxToken internalSyntaxToken = EatTokenIfKind(SyntaxKind.ElseKeyword);
		InternalElseTableRelationSyntax elseExpression = null;
		if (internalSyntaxToken != null)
		{
			if (internalIfTableRelationSyntax == null)
			{
				internalSyntaxToken = AddError(internalSyntaxToken, ErrorCode.ERR_ElseWithoutIf, name);
			}
			InternalTableRelationPropertyValueSyntax elseTableRelationCondition = ParseTableRelationPropertyValue(pti);
			elseExpression = ContextAwareSyntaxFactory.ElseTableRelation(internalSyntaxToken, elseTableRelationCondition);
		}
		return ContextAwareSyntaxFactory.TableRelationPropertyValue(internalIfTableRelationSyntax, relatedTableField, tableFilter, elseExpression);
	}

	private InternalIfTableRelationSyntax ParseIfTableRelation()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.IfTableRelationExpression)
		{
			return (InternalIfTableRelationSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatTokenIfKind(SyntaxKind.IfKeyword);
		if (internalSyntaxToken == null)
		{
			return null;
		}
		InternalTableFilterExpressionSyntax ifTableRelationCondition = ParseTableRelationCondition();
		return ContextAwareSyntaxFactory.IfTableRelation(internalSyntaxToken, ifTableRelationCondition);
	}

	private InternalTableFilterExpressionSyntax ParseTableRelationCondition()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.TableFilterExpression)
		{
			return (InternalTableFilterExpressionSyntax)EatNode();
		}
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> conditions = ParseTableRelationConditionValue();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return ContextAwareSyntaxFactory.TableFilterExpression(openParenthesisToken, conditions, closeParenthesisToken);
	}

	internal InternalWhereExpressionSyntax ParseTableFilters(bool tokenExpected = false)
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.WhereExpression)
		{
			return (InternalWhereExpressionSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatTokenIfKind(SyntaxKind.WhereFormulaKeyword);
		if (internalSyntaxToken == null)
		{
			if (!tokenExpected)
			{
				return null;
			}
			InternalSyntaxToken node = InternalSyntaxFactory.MissingToken(SyntaxKind.WhereFormulaKeyword);
			internalSyntaxToken = AddError(node, ErrorCode.ERR_InvalidFilterExpression);
		}
		InternalTableFilterExpressionSyntax filter = ParseTableRelationCondition();
		return ContextAwareSyntaxFactory.WhereExpression(internalSyntaxToken, filter);
	}

	private InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> ParseTableRelationConditionValue()
	{
		InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> result = default;
		InternalSeparatedSyntaxListBuilder<InternalPropertyExpressionSyntax> item = Pool.AllocateSeparated<InternalPropertyExpressionSyntax>();
		try
		{
			InternalPropertyExpressionSyntax internalPropertyExpressionSyntax = ParsePropertyExpression();
			if (internalPropertyExpressionSyntax != null)
			{
				item.Add(internalPropertyExpressionSyntax);
				while (CurrentToken.Kind == SyntaxKind.CommaToken)
				{
					item.AddSeparator(CurrentToken);
					EatToken();
					item.Add(ParsePropertyExpression());
				}
				return item.ToList();
			}
			return result;
		}
		finally
		{
			Pool.Free(item);
		}
	}

	private InternalTableSyntax ParseTable()
	{
		InternalSyntaxToken tableKeyword = EatToken(SyntaxKind.TableKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		GlobalLogger.LogVerbose("Parsing Table {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupTableProperty);
		InternalFieldListSyntax fields = ParseFieldList();
		InternalKeyListSyntax keys = ParseKeyList();
		InternalFieldGroupListSyntax fieldGroups = ParseFieldGroupList();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetTableTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return ContextAwareSyntaxFactory.Table(tableKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, openBraceToken, propertyList, fields, keys, fieldGroups, members, closeBraceToken);
	}

	private InternalFieldGroupListSyntax ParseFieldGroupList()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.FieldGroupList)
		{
			return (InternalFieldGroupListSyntax)EatNode();
		}
		return ParseKeysOrFieldGroups(SyntaxKind.FieldGroupsKeyword, SyntaxKind.FieldGroup, SyntaxKind.FieldGroupKeyword, ContextAwareSyntaxFactory.FieldGroupList, ContextAwareSyntaxFactory.FieldGroup, LookupFieldGroupProperty);
	}

	private InternalKeyListSyntax ParseKeyList()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.KeyList)
		{
			return (InternalKeyListSyntax)EatNode();
		}
		return ParseKeysOrFieldGroups(SyntaxKind.KeysKeyword, SyntaxKind.Key, SyntaxKind.KeyKeyword, ContextAwareSyntaxFactory.KeyList, ContextAwareSyntaxFactory.Key, LookupKeyProperty);
	}

	private InternalFieldListSyntax ParseFieldList()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.FieldList)
		{
			return (InternalFieldListSyntax)EatNode();
		}
		InternalSyntaxToken fieldsKeyword = EatToken(SyntaxKind.FieldsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalFieldSyntax> fields = ParseFields();
		return ContextAwareSyntaxFactory.FieldList(fieldsKeyword, openBraceToken, fields, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalFieldSyntax> ParseFields()
	{
		InternalSyntaxListBuilder<InternalFieldSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalFieldSyntax>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind == SyntaxKind.FieldKeyword)
			{
				InternalFieldSyntax node = ParseField();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalFieldSyntax ParseField()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.Field)
		{
			return (InternalFieldSyntax)EatNode();
		}
		InternalSyntaxToken fieldKeyword = EatToken(SyntaxKind.FieldKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSyntaxToken no = EatToken(SyntaxKind.Int32LiteralToken);
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken semicolonToken2 = EatToken(SyntaxKind.SemicolonToken);
		InternalDataTypeSyntax type = ParseTableFieldType();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(LookupFieldProperty);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetFieldTriggerInfo);
		return ContextAwareSyntaxFactory.Field(fieldKeyword, openParenthesisToken, no, semicolonToken, name, semicolonToken2, type, closeParenthesisToken, openBraceToken, propertyList, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalDataTypeSyntax ParseTableFieldType()
	{
		if (SyntaxFacts.IsOptionType(CurrentToken))
		{
			return ParseOptionDataType(skipParsingOptionValues: true);
		}
		return ParseDatatype();
	}

	private U ParseKeysOrFieldGroups<U, V>(SyntaxKind listKindKeyword, SyntaxKind memberKind, SyntaxKind memberKindKeyword, Func<InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxList<V>, InternalSyntaxToken, U> createList, Func<InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxToken, InternalIdentifierNameSyntax, InternalSyntaxToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax>, InternalSyntaxToken, InternalSyntaxToken, InternalPropertyListSyntax, InternalSyntaxToken, V> createMember, Func<string, PropertyTypeInfo> lookupProperty) where U : InternalSyntaxNode where V : InternalKeyFieldGroupBaseSyntax
	{
		if (CurrentToken.Kind != listKindKeyword)
		{
			return null;
		}
		InternalSyntaxToken arg = EatToken(listKindKeyword);
		InternalSyntaxToken arg2 = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<V> arg3 = ParseKeyOrFieldGroupList(memberKind, memberKindKeyword, createMember, lookupProperty);
		return createList(arg, arg2, arg3, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<T> ParseKeyOrFieldGroupList<T>(SyntaxKind memberKind, SyntaxKind keywordKind, Func<InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxToken, InternalIdentifierNameSyntax, InternalSyntaxToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax>, InternalSyntaxToken, InternalSyntaxToken, InternalPropertyListSyntax, InternalSyntaxToken, T> createNode, Func<string, PropertyTypeInfo> lookupProperty) where T : InternalKeyFieldGroupBaseSyntax
	{
		InternalSyntaxListBuilder<T> internalSyntaxListBuilder = Pool.Allocate<T>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind == keywordKind)
			{
				if (IsIncremental && CurrentNodeKind == memberKind)
				{
					internalSyntaxListBuilder.Add((T)EatNode());
				}
				else
				{
					internalSyntaxListBuilder.Add(ParseKeyOrFieldGroup(keywordKind, createNode, lookupProperty));
				}
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private T ParseKeyOrFieldGroup<T>(SyntaxKind keywordKind, Func<InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxToken, InternalIdentifierNameSyntax, InternalSyntaxToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax>, InternalSyntaxToken, InternalSyntaxToken, InternalPropertyListSyntax, InternalSyntaxToken, T> createNode, Func<string, PropertyTypeInfo> lookupProperty) where T : InternalKeyFieldGroupBaseSyntax
	{
		InternalSyntaxToken arg = EatToken(keywordKind);
		InternalSyntaxToken arg2 = EatToken(SyntaxKind.OpenParenToken);
		InternalSyntaxToken arg3 = null;
		InternalSyntaxToken arg4 = null;
		if (CurrentToken.Kind == SyntaxKind.Int32LiteralToken)
		{
			arg3 = AddError(EatToken(SyntaxKind.Int32LiteralToken), ErrorCode.WRN_DeprecatedIdSyntax);
			arg4 = EatToken(SyntaxKind.SemicolonToken);
		}
		InternalIdentifierNameSyntax arg5 = ParseIdentifierName();
		InternalSyntaxToken startToken = EatToken(SyntaxKind.SemicolonToken);
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> arg6 = ParseCommaSeparatedIdentifierNames(ref startToken, SyntaxKind.CloseParenToken);
		InternalSyntaxToken arg7 = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken arg8 = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax arg9 = ParsePropertyList(lookupProperty);
		return createNode(arg, arg2, arg3, arg4, arg5, startToken, arg6, arg7, arg8, arg9, EatToken(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	internal InternalPropertyValueSyntax ParseMultilanguagePropertyValue(PropertyTypeInfo propertyTypeInfo, ref InternalSyntaxToken equalsToken)
	{
		return ContextAwareSyntaxFactory.MultilanguagePropertyValue(ParseCommaSeparatedIdentifierEqualsStringList(ref equalsToken));
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	internal InternalPropertyValueSyntax ParseLabelPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ContextAwareSyntaxFactory.LabelPropertyValue(ParseLabel());
	}

	internal InternalPropertyValueSyntax ParseInt32LiteralPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ContextAwareSyntaxFactory.Int32PropertyValue(ParseInt32LiteralValue());
	}

	internal InternalPropertyValueSyntax ParseStringLiteralPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ContextAwareSyntaxFactory.StringPropertyValue(ParseStringLiteralValue());
	}

	internal InternalTimePropertyValueSyntax ParseTimeLiteralPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ContextAwareSyntaxFactory.TimePropertyValue(ParseTimeLiteralValue());
	}

	internal InternalDatePropertyValueSyntax ParseDateLiteralPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ContextAwareSyntaxFactory.DatePropertyValue(ParseDateLiteralValue());
	}

	internal InternalDateTimePropertyValueSyntax ParseDateTimeLiterlaPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ContextAwareSyntaxFactory.DateTimePropertyValue(ParseDateTimeLiteralValue());
	}

	internal InternalPropertyValueSyntax ParseBooleanPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ContextAwareSyntaxFactory.BooleanPropertyValue(ParseBooleanLiteralValue());
	}

	internal InternalPropertyValueSyntax ParseStyleExpressionPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ParseExpressionPropertyValue(ContextAwareSyntaxFactory.StyleExpressionPropertyValue);
	}

	internal InternalPropertyValueSyntax ParseIntegerExpressionPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ParseExpressionPropertyValue(ContextAwareSyntaxFactory.IntegerExpressionPropertyValue);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	internal InternalPropertyValueSyntax ParseObjectReferencePropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ContextAwareSyntaxFactory.ObjectReferencePropertyValue(ParseObjectReferenceSyntax());
	}

	internal InternalPropertyValueSyntax ParseEnumLiteralPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		EnumPropertyTypeInfo enumPropertyTypeInfo = (EnumPropertyTypeInfo)propertyTypeInfo;
		if (!internalIdentifierNameSyntax.IsMissing)
		{
			return ContextAwareSyntaxFactory.EnumPropertyValue(internalIdentifierNameSyntax);
		}
		InternalSyntaxToken invalidToken = AddError(internalIdentifierNameSyntax.GetFirstToken(), ErrorCode.ERR_InvalidPropertyOptionValue, propertyTypeInfo.Name, GetEnumOptionsDiagnosticText(enumPropertyTypeInfo));
		return ContextAwareSyntaxFactory.InvalidPropertyValue(invalidToken);
	}

	internal InternalPropertyValueSyntax ParsePermissionPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken equalsToken)
	{
		if (pti.Kind == PropertyKind.AccessByPermission)
		{
			return ParseAccessByPermission();
		}
		return ParsePermissionSet(ref equalsToken);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pti")]
	internal InternalPropertyValueSyntax ParseCommaSeparatedPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken startToken)
	{
		return ContextAwareSyntaxFactory.CommaSeparatedPropertyValue(ParseCommaSeparatedIdentifierNames(ref startToken));
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pti")]
	internal InternalPropertyValueSyntax ParseCommaSeparatedIdentifierEqualsStringListPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken startToken)
	{
		InternalCommaSeparatedIdentifierEqualsStringListSyntax list = ParseCommaSeparatedIdentifierEqualsStringList(ref startToken);
		return ContextAwareSyntaxFactory.CommaSeparatedIdentifierEqualsStringListPropertyValue(list);
	}

	internal InternalPropertyValueSyntax ParseCommaSeparatedStringsPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken startToken)
	{
		return ContextAwareSyntaxFactory.CommaSeparatedStringsPropertyValue(ParseCommaSeparatedStrings(ref startToken));
	}

	private InternalSeparatedSyntaxList<InternalStringLiteralValueSyntax> ParseCommaSeparatedStrings(ref InternalSyntaxToken startToken, SyntaxKind closeTokenKind = SyntaxKind.SemicolonToken)
	{
		return ParseSeparatedList(ref startToken, SyntaxKind.StringLiteralToken, (InternalSyntaxToken token) => token.Kind == SyntaxKind.StringLiteralToken, SyntaxKind.CommaToken, closeTokenKind, SkipBadCommaSeparatedStringLiteralToken, ParseStringLiteralValue);
	}

	private InternalCommaSeparatedIdentifierEqualsStringListSyntax ParseCommaSeparatedIdentifierEqualsStringList(ref InternalSyntaxToken startToken, SyntaxKind closeTokenKind = SyntaxKind.SemicolonToken)
	{
		InternalSeparatedSyntaxList<InternalIdentifierEqualsStringSyntax> values = ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, closeTokenKind, SkipBadCommaSeparatedToken, ParseIdentifierEqualsString);
		return ContextAwareSyntaxFactory.CommaSeparatedIdentifierEqualsStringList(values);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	internal InternalPropertyValueSyntax ParseQueryDataItemLinkPropertyValue(PropertyTypeInfo info, ref InternalSyntaxToken equalsToken)
	{
		InternalSeparatedSyntaxList<InternalQueryDataItemLinkExpressionSyntax> joins = ParseSeparatedList(ref equalsToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, SyntaxKind.SemicolonToken, SkipBadCommaSeparatedToken, ParseQueryDataItemLinkExpression);
		return ContextAwareSyntaxFactory.QueryDataItemLinkPropertyValue(joins);
	}

	internal InternalQueryDataItemLinkExpressionSyntax ParseQueryDataItemLinkExpression()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.QueryDataItemLinkExpression)
		{
			return (InternalQueryDataItemLinkExpressionSyntax)EatNode();
		}
		InternalIdentifierNameSyntax sourceField = ParseIdentifierName();
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseMemberAccessExpressionOrIdentifier();
		if (internalCodeExpressionSyntax.Kind != SyntaxKind.MemberAccessExpression)
		{
			internalCodeExpressionSyntax = ContextAwareSyntaxFactory.MemberAccessExpression(internalCodeExpressionSyntax, CreateMissingToken(SyntaxKind.DotToken, internalCodeExpressionSyntax.Kind, reportError: false), ParserBase.CreateMissingIdentifierName());
		}
		return ContextAwareSyntaxFactory.QueryDataItemLinkExpression(sourceField, equalsToken, internalCodeExpressionSyntax as InternalMemberAccessExpressionSyntax);
	}

	private InternalPropertyValueSyntax ParseExpressionPropertyValue(Func<InternalCodeExpressionSyntax, InternalPropertyValueSyntax> func)
	{
		return func(ParseInLexerMode(LexerMode.Expression, ParseExpression));
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	internal InternalPropertyValueSyntax ParseTextExpressionPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ParseExpressionPropertyValue(ContextAwareSyntaxFactory.TextExpressionPropertyValue);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	internal InternalPropertyValueSyntax ParseClientSideBooleanExpressionPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ParseExpressionPropertyValue(ContextAwareSyntaxFactory.ClientSideBooleanExpressionPropertyValue);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	internal InternalPropertyValueSyntax ParseBooleanExpressionPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ParseExpressionPropertyValue(ContextAwareSyntaxFactory.BooleanExpressionPropertyValue);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	internal InternalPropertyValueSyntax ParseQualifiedObjectReferencePropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		InternalSyntaxToken internalSyntaxToken = null;
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.CodeunitKeyword:
		case SyntaxKind.TableKeyword:
		case SyntaxKind.PageKeyword:
		case SyntaxKind.ReportKeyword:
		case SyntaxKind.XmlPortKeyword:
			return ContextAwareSyntaxFactory.QualifiedObjectReferencePropertyValue(EatToken(), ParseObjectReferenceSyntax());
		case SyntaxKind.QueryKeyword:
			if ((internalSyntaxToken = CheckFeatureAvailability(CurrentToken, Feature.RunQueryObject)) == CurrentToken)
			{
				return ContextAwareSyntaxFactory.QualifiedObjectReferencePropertyValue(EatToken(), ParseObjectReferenceSyntax());
			}
			break;
		}
		if (internalSyntaxToken == null)
		{
			internalSyntaxToken = AddError(CreateMissingToken(SyntaxKind.CodeunitKeyword, CurrentToken.Kind, reportError: false), ErrorCode.ERR_ExpectedApplicationObjectKeyword, SyntaxFacts.SupportedApplicationObjectReferencesString);
		}
		return ContextAwareSyntaxFactory.InvalidPropertyValue(internalSyntaxToken);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	private InternalPropertyValueSyntax ParsePageFieldReferencePropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ContextAwareSyntaxFactory.PageFieldReferencePropertyValue(ParseIdentifierName());
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	internal InternalPropertyValueSyntax ParseReportDataItemLinkPropertyValue(PropertyTypeInfo info, ref InternalSyntaxToken equalsToken)
	{
		InternalSeparatedSyntaxList<InternalReportDataItemLinkExpressionSyntax> joins = ParseSeparatedList(ref equalsToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, SyntaxKind.SemicolonToken, SkipBadCommaSeparatedToken, ParseReportDataItemLinkExpression);
		return ContextAwareSyntaxFactory.ReportDataItemLinkPropertyValue(joins);
	}

	internal InternalReportDataItemLinkExpressionSyntax ParseReportDataItemLinkExpression()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.ReportDataItemLinkExpression)
		{
			return (InternalReportDataItemLinkExpressionSyntax)EatNode();
		}
		InternalIdentifierNameSyntax sourceField = ParseIdentifierName();
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalSyntaxToken fieldKeywordToken = EatToken(SyntaxKind.FieldFormulaKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax relatedField = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return ContextAwareSyntaxFactory.ReportDataItemLinkExpression(sourceField, equalsToken, fieldKeywordToken, openParenthesisToken, relatedField, closeParenthesisToken);
	}

	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	internal InternalPropertyValueSyntax ParseShortcutPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		throw ExceptionUtilities.SkippedForNow;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	internal InternalPropertyValueSyntax ParseTableFilterPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> conditions = ParseTableRelationConditionValue();
		return ContextAwareSyntaxFactory.TableFilterPropertyValue(conditions);
	}

	internal InternalPropertyValueSyntax ParseOptionValuesPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken startToken)
	{
		return ContextAwareSyntaxFactory.OptionValuesPropertyValue(ParseOptionMembersSyntax(ref startToken));
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pti")]
	internal InternalPropertyValueSyntax ParseDecimalPlacesPropertyValue(PropertyTypeInfo pti)
	{
		return ContextAwareSyntaxFactory.DecimalPlacesPropertyValue(ParseDecimalPlacesSyntax());
	}

	private static string GetEnumOptionsDiagnosticText(EnumPropertyTypeInfo enumPropertyTypeInfo)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		foreach (string option in enumPropertyTypeInfo.Options)
		{
			if (instance.Length > 0)
			{
				instance.Builder.Append(',');
			}
			if (instance.Length > 80)
			{
				instance.Builder.Append("...");
				break;
			}
			instance.Builder.Append(option);
		}
		return instance.ToStringAndFree();
	}

	private InternalPropertyValueSyntax ParseAccessByPermission()
	{
		InternalSeparatedSyntaxListBuilder<InternalPermissionSyntax> internalSeparatedSyntaxListBuilder = Pool.AllocateSeparated<InternalPermissionSyntax>();
		try
		{
			InternalPermissionSyntax node = ParsePermissionSyntax();
			internalSeparatedSyntaxListBuilder.Add(node);
			return ContextAwareSyntaxFactory.PermissionPropertyValue(internalSeparatedSyntaxListBuilder);
		}
		finally
		{
			Pool.Free(internalSeparatedSyntaxListBuilder);
		}
	}

	private InternalPropertyValueSyntax ParsePermissionSet(ref InternalSyntaxToken startToken)
	{
		InternalSeparatedSyntaxList<InternalPermissionSyntax> permissionProperties = ParseSeparatedList(ref startToken, SyntaxKind.TableDataKeyword, (InternalSyntaxToken token) => token.Kind == SyntaxKind.TableDataKeyword, SyntaxKind.CommaToken, SyntaxKind.SemicolonToken, SkipBadPermissionSyntaxToken, ParsePermissionSyntax);
		return ContextAwareSyntaxFactory.PermissionPropertyValue(permissionProperties);
	}

	private InternalDecimalPlacesSyntax ParseDecimalPlacesSyntax()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.DecimalPlaces)
		{
			return (InternalDecimalPlacesSyntax)EatNode();
		}
		InternalSyntaxToken minimumToken = EatToken(SyntaxKind.Int32LiteralToken);
		InternalSyntaxToken colonToken = null;
		InternalSyntaxToken maximumToken = null;
		if (CurrentToken.Kind == SyntaxKind.ColonToken)
		{
			colonToken = EatToken();
			if (CurrentToken.Kind == SyntaxKind.Int32LiteralToken)
			{
				maximumToken = EatToken();
			}
		}
		return ContextAwareSyntaxFactory.DecimalPlaces(minimumToken, colonToken, maximumToken);
	}

	private InternalMemberAccessOrIdentifierPropertyValueSyntax ParseMemberAccessOrIdentifierPropertyValue()
	{
		InternalCodeExpressionSyntax identifier = ParseMemberAccessExpressionOrIdentifier();
		return ContextAwareSyntaxFactory.MemberAccessOrIdentifierPropertyValue(identifier);
	}

	private InternalCodeExpressionSyntax ParseMemberAccessExpressionOrIdentifier()
	{
		if (!CurrentToken.Kind.IsTokenIdentifier())
		{
			return AddTrailingSkippedSyntax(ParserBase.CreateMissingIdentifierName(), AddError(EatToken(), ErrorCode.ERR_IdentifierExpected));
		}
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		InternalSyntaxToken internalSyntaxToken = EatTokenIfKind(SyntaxKind.DotToken);
		if (internalSyntaxToken != null)
		{
			InternalIdentifierNameSyntax name = (CurrentToken.Kind.IsTokenIdentifier() ? ParseIdentifierName() : AddTrailingSkippedSyntax(ParserBase.CreateMissingIdentifierName(), AddError(EatToken(), ErrorCode.ERR_IdentifierExpected)));
			return ContextAwareSyntaxFactory.MemberAccessExpression(internalIdentifierNameSyntax, internalSyntaxToken, name);
		}
		return internalIdentifierNameSyntax;
	}

	private InternalIdentifierOrLiteralExpressionSyntax ParseIdentifierOrLiteralExpression()
	{
		InternalIdentifierOrLiteralExpressionSyntax internalIdentifierOrLiteralExpressionSyntax = TryParseIdentifierOrLiteralExpression();
		if (internalIdentifierOrLiteralExpressionSyntax != null)
		{
			return internalIdentifierOrLiteralExpressionSyntax;
		}
		InternalSyntaxToken identifier = AddError(ParserBase.CreateMissingIdentifierToken(), ErrorCode.ERR_ExpectedIdentifierOrLiteral);
		InternalIdentifierNameSyntax identifier2 = ContextAwareSyntaxFactory.IdentifierName(identifier);
		return ContextAwareSyntaxFactory.IdentifierOrLiteralExpression(identifier2);
	}

	private InternalIdentifierOrLiteralExpressionSyntax TryParseIdentifierOrLiteralExpression()
	{
		InternalSyntaxToken internalSyntaxToken = CurrentToken;
		InternalSyntaxToken internalSyntaxToken2 = ParseIdentifierToken(allowMissingIdentifier: true);
		if (internalSyntaxToken2 != null)
		{
			InternalIdentifierNameSyntax identifier = ContextAwareSyntaxFactory.IdentifierName(internalSyntaxToken2);
			return ContextAwareSyntaxFactory.IdentifierOrLiteralExpression(identifier);
		}
		if (internalSyntaxToken.Kind.IsPossibleSignedLiteralToken())
		{
			InternalLiteralExpressionSyntax identifier2 = ContextAwareSyntaxFactory.LiteralExpression(ParseLiteralValue());
			return ContextAwareSyntaxFactory.IdentifierOrLiteralExpression(identifier2);
		}
		return null;
	}

	private InternalPermissionSyntax ParsePermissionSyntax()
	{
		InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxFacts.SupportedPermissionKeywordKinds, ErrorCode.ERR_ExpectedApplicationObjectKeyword, SyntaxFacts.SupportedPermissionApplicationObjectsString);
		InternalObjectNameOrIdSyntax objectReference = ParseObjectReferenceSyntax();
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalSyntaxToken permissionValues = GetPermissionValues(internalSyntaxToken.Kind == SyntaxKind.TableDataKeyword);
		return ContextAwareSyntaxFactory.Permission(internalSyntaxToken, objectReference, equalsToken, permissionValues);
	}

	private InternalSyntaxToken GetPermissionValues(bool isTableData)
	{
		InternalSyntaxToken internalSyntaxToken = ParseIdentifierToken();
		string text = internalSyntaxToken.Text.ToUpperInvariant();
		if (text.Length == 0)
		{
			return null;
		}
		if (isTableData && text.Length > 4)
		{
			return AddError(internalSyntaxToken, ErrorCode.ERR_InvalidPermissionValue, "RIMD");
		}
		if (!isTableData)
		{
			if (text.Length != 1 || text[0] != 'X')
			{
				internalSyntaxToken = AddError(internalSyntaxToken, ErrorCode.ERR_InvalidPermissionValue, "X");
			}
			return internalSyntaxToken;
		}
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c != 'R' && c != 'I' && c != 'M' && c != 'D')
			{
				return AddError(internalSyntaxToken, ErrorCode.ERR_InvalidPermissionValue, "RIMD");
			}
			for (int j = i + 1; j < text.Length; j++)
			{
				if (text[j] == c)
				{
					return AddError(internalSyntaxToken, ErrorCode.ERR_InvalidPermissionValue, "RIMD");
				}
			}
		}
		return internalSyntaxToken;
	}

	private PostSkipAction SkipBadPermissionSyntaxToken(ref InternalSyntaxToken firstToken, InternalSeparatedSyntaxListBuilder<InternalPermissionSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref firstToken, list, (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || (expected == SyntaxKind.TableDataKeyword && p.CurrentToken.Kind != SyntaxKind.TableDataKeyword), (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.IsTerminator(), expected);
	}

	private InternalPropertyValueSyntax ParseLiteralPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.Int32LiteralToken:
		case SyntaxKind.Int64LiteralToken:
		case SyntaxKind.DecimalLiteralToken:
		case SyntaxKind.MinusToken:
			return ParseNumericPropertyValue();
		case SyntaxKind.StringLiteralToken:
			return ParseStringLiteralPropertyValue(propertyTypeInfo);
		case SyntaxKind.TimeLiteralToken:
			return ParseTimeLiteralPropertyValue(propertyTypeInfo);
		case SyntaxKind.DateLiteralToken:
			return ParseDateLiteralPropertyValue(propertyTypeInfo);
		case SyntaxKind.DateTimeLiteralToken:
			return ParseDateTimeLiterlaPropertyValue(propertyTypeInfo);
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.TrueKeyword:
			return ParseBooleanPropertyValue(propertyTypeInfo);
		case SyntaxKind.IdentifierToken:
			return ParseOptionValuePropertyValue();
		default:
		{
			InternalSyntaxToken invalidToken = AddError(CreateMissingToken(SyntaxKind.StringLiteralToken, CurrentToken.Kind, reportError: false), ErrorCode.ERR_UnexpectedToken, propertyTypeInfo.Name);
			return ContextAwareSyntaxFactory.InvalidPropertyValue(invalidToken);
		}
		}
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	private InternalPropertyValueSyntax ParseImagePropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ContextAwareSyntaxFactory.ImagePropertyValue(ParseIdentifierName());
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyTypeInfo")]
	private InternalPropertyValueSyntax ParseMemberReferencePropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ContextAwareSyntaxFactory.MemberReferencePropertyValue(ParseIdentifierName());
	}

	private InternalPropertyValueSyntax ParseOptionValuePropertyValue()
	{
		return ContextAwareSyntaxFactory.OptionValuePropertyValue(ParseIdentifierName());
	}

	private InternalPropertyValueSyntax ParseNumericPropertyValue()
	{
		InternalLiteralValueSyntax internalLiteralValueSyntax = ParseNumericLiteralValue();
		return internalLiteralValueSyntax.Kind switch
		{
			SyntaxKind.Int32SignedLiteralValue => (InternalPropertyValueSyntax)ContextAwareSyntaxFactory.Int32PropertyValue((InternalInt32SignedLiteralValueSyntax)internalLiteralValueSyntax), 
			SyntaxKind.Int64SignedLiteralValue => ContextAwareSyntaxFactory.Int64PropertyValue((InternalInt64SignedLiteralValueSyntax)internalLiteralValueSyntax), 
			SyntaxKind.DecimalSignedLiteralValue => ContextAwareSyntaxFactory.DecimalPropertyValue((InternalDecimalSignedLiteralValueSyntax)internalLiteralValueSyntax), 
			_ => throw ExceptionUtilities.UnexpectedValue(CurrentToken.Kind), 
		};
	}

	static ObjectParser()
	{
		CodeunitProperties = GetCodeunitProperties();
		FieldProperties = GetFieldProperties();
		KeyProperties = GetKeyProperties();
		PageProperties = GetPageProperties();
		PageActionProperties = GetPageActionProperties();
		PageActionAreaProperties = GetPageActionAreaProperties();
		PageActionGroupProperties = GetPageActionGroupProperties();
		PageAreaProperties = GetPageAreaProperties();
		PageFieldProperties = GetPageFieldProperties();
		PageGroupProperties = GetPageGroupProperties();
		PageLabelProperties = GetPageLabelProperties();
		PagePartProperties = GetPagePartProperties();
		PageSystemPartProperties = GetPageSystemPartProperties();
		PageChartPartProperties = GetPageChartPartProperties();
		QueryProperties = GetQueryProperties();
		QueryColumnProperties = GetQueryColumnProperties();
		QueryDataItemProperties = GetQueryDataItemProperties();
		QueryFilterProperties = GetQueryFilterProperties();
		ReportProperties = GetReportProperties();
		ReportDataItemProperties = GetReportDataItemProperties();
		ReportColumnProperties = GetReportColumnProperties();
		RequestPageProperties = GetRequestPageProperties();
		TableProperties = GetTableProperties();
		XmlPortProperties = GetXmlPortProperties();
		XmlPortTextElementProperties = GetXmlPortTextElementProperties();
		XmlPortFieldElementProperties = GetXmlPortFieldElementProperties();
		XmlPortTableElementProperties = GetXmlPortTableElementProperties();
		XmlPortFieldAttributeProperties = GetXmlPortFieldAttributeProperties();
		XmlPortTextAttributeProperties = GetXmlPortTextAttributeProperties();
		FieldGroupProperties = GetFieldGroupProperties();
		PageActionSeparatorProperties = GetPageActionSeparatorProperties();
		EnumValueProperties = GetEnumValueProperties();
		DotNetAssemblyProperties = GetDotNetAssemblyProperties();
		DotNetTypeDeclarationProperties = GetDotNetTypeDeclarationProperties();
		PageViewProperties = GetPageViewProperties();
		ProfileProperties = GetProfileProperties();
		PageCustomizationObjectProperties = GetPageCustomizationObjectProperties();
		ControlAddInObjectProperties = GetControlAddInObjectProperties();
		PageUserControlProperties = GetPageUserControlProperties();
		EnumTypeProperties = GetEnumTypeProperties();
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetCodeunitProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("TABLENO", new PropertyTypeInfo(PropertyKind.TableNo, "TableNo", "String", "ObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit"));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit"));
			instance.Add("SUBTYPE", new EnumPropertyTypeInfo(PropertyKind.Subtype, "Subtype", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Normal,Test,TestRunner,Upgrade,Install", null, "Normal", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: true, "Codeunit"));
			instance.Add("SINGLEINSTANCE", new PropertyTypeInfo(PropertyKind.SingleInstance, "SingleInstance", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit"));
			instance.Add("TESTISOLATION", new EnumPropertyTypeInfo(PropertyKind.TestIsolation, "TestIsolation", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, new List<string> { "Subtype(TestRunner)" }, null, null, "Disabled,Codeunit,Function", null, "Disabled", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Codeunit"));
			instance.Add("EVENTSUBSCRIBERINSTANCE", new EnumPropertyTypeInfo(PropertyKind.EventSubscriberInstance, "EventSubscriberInstance", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "StaticAutomatic,Manual", null, "StaticAutomatic", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Codeunit"));
			instance.Add("TESTPERMISSIONS", new EnumPropertyTypeInfo(PropertyKind.TestPermissions, "TestPermissions", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, new List<string> { "Subtype(Test)" }, null, null, "InheritFromTestCodeunit,Restrictive,NonRestrictive,Disabled", null, "Restrictive", isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Codeunit"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupCodeunitProperty(string name)
	{
		PropertyTypeInfo value = null;
		CodeunitProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetFieldProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("AUTOFORMATTYPE", new PropertyTypeInfo(PropertyKind.AutoFormatType, "AutoFormatType", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("AUTOFORMATEXPRESSION", new PropertyTypeInfo(PropertyKind.AutoFormatExpression, "AutoFormatExpression", "String", "TextExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("BLANKNUMBERS", new EnumPropertyTypeInfo(PropertyKind.BlankNumbers, "BlankNumbers", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Date,DateTime,Decimal,Duration,Enum,Integer,Option,Time)" }, null, "DontBlank,BlankNeg,BlankNegAndZero,BlankZero,BlankZeroAndPos,BlankPos", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Field_PageField"));
			instance.Add("BLANKZERO", new PropertyTypeInfo(PropertyKind.BlankZero, "BlankZero", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Integer,Decimal,Duration,Enum,Option)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("SIGNDISPLACEMENT", new PropertyTypeInfo(PropertyKind.SignDisplacement, "SignDisplacement", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)" }, null, "0", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("MINVALUE", new PropertyTypeInfo(PropertyKind.MinValue, "MinValue", "String", "Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("MAXVALUE", new PropertyTypeInfo(PropertyKind.MaxValue, "MaxValue", "String", "Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("NOTBLANK", new PropertyTypeInfo(PropertyKind.NotBlank, "NotBlank", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("NUMERIC", new PropertyTypeInfo(PropertyKind.Numeric, "Numeric", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, new List<string> { "Type(Code,Text)" }, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("CHARALLOWED", new PropertyTypeInfo(PropertyKind.CharAllowed, "CharAllowed", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(Char,Code,Label,String,Text,TextConst)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, CharAllowedPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("DATEFORMULA", new PropertyTypeInfo(PropertyKind.DateFormula, "DateFormula", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, new List<string> { "Type(Char,Code,Label,String,Text,TextConst)" }, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("VALUESALLOWED", new PropertyTypeInfo(PropertyKind.ValuesAllowed, "ValuesAllowed", "String", "CommaSeparatedIdentifierOrLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedIdentifierOrLiteralPropertyValue(pti, ref equals);
			}, null, new List<string> { "Type(Boolean,Enum,Option,Integer,BigInteger,Decimal,Code,Text)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("OPTIONCAPTIONML", new PropertyTypeInfo(PropertyKind.OptionCaptionML, "OptionCaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, new List<string> { "Type(Option)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("OPTIONCAPTION", new PropertyTypeInfo(PropertyKind.OptionCaption, "OptionCaption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, new List<string> { "Type(Option)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("CLOSINGDATES", new PropertyTypeInfo(PropertyKind.ClosingDates, "ClosingDates", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, new List<string> { "Type(Date)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("DECIMALPLACES", new PropertyTypeInfo(PropertyKind.DecimalPlaces, "DecimalPlaces", "String", "DecimalPlaces", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseDecimalPlacesPropertyValue(pti);
			}, null, new List<string> { "Type(Decimal)" }, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, DecimalPlacesPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("EXTENDEDDATATYPE", new EnumPropertyTypeInfo(PropertyKind.ExtendedDatatype, "ExtendedDatatype", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, "None,PhoneNo,URL,EMail,Ratio,Masked,Person", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Field_PageField"));
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Customizations, WidthPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("CAPTIONCLASS", new PropertyTypeInfo(PropertyKind.CaptionClass, "CaptionClass", "String", "TextExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field"));
			instance.Add("DATACLASSIFICATION", new EnumPropertyTypeInfo(PropertyKind.DataClassification, "DataClassification", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, new List<string> { "FieldClass(Normal)" }, null, null, "CustomerContent,EndUserIdentifiableInformation,AccountData,EndUserPseudonymousIdentifiers,OrganizationIdentifiableInformation,SystemMetadata,ToBeClassified", null, "CustomerContent", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Field"));
			instance.Add("INITVALUE", new PropertyTypeInfo(PropertyKind.InitValue, "InitValue", "String", "Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Enum,Integer,Label,Option,String,Text,Time,TextConst)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field"));
			instance.Add("FIELDCLASS", new EnumPropertyTypeInfo(PropertyKind.FieldClass, "FieldClass", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, "Normal,FlowField,FlowFilter", null, "Normal", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Field"));
			instance.Add("CALCFORMULA", new PropertyTypeInfo(PropertyKind.CalcFormula, "CalcFormula", "String", "CalculationFormula", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCalculationFormulaPropertyValue(pti);
			}, new List<string> { "FieldClass(FlowField)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Field"));
			instance.Add("TABLERELATION", new PropertyTypeInfo(PropertyKind.TableRelation, "TableRelation", "String", "TableRelation", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableRelationPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Field"));
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field"));
			instance.Add("SQLDATATYPE", new EnumPropertyTypeInfo(PropertyKind.SqlDataType, "SqlDataType", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(Code)" }, null, "Varchar,Integer,Variant,BigInteger", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, "SQL_Data_Type", null, null, emitAsAttribute: true, prependObjectName: false, "Field"));
			instance.Add("VALIDATETABLERELATION", new PropertyTypeInfo(PropertyKind.ValidateTableRelation, "ValidateTableRelation", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "TableRelation" }, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field"));
			instance.Add("TESTTABLERELATION", new PropertyTypeInfo(PropertyKind.TestTableRelation, "TestTableRelation", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "TableRelation" }, null, null, "true", isObsolete: true, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field"));
			instance.Add("SUBTYPE", new EnumPropertyTypeInfo(PropertyKind.Subtype, "Subtype", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BLOB)" }, null, "Json,UserDefined,Bitmap,Memo", null, "UserDefined", isObsolete: false, generateMetadata: true, ModificationKind.None, null, "SubType", null, null, emitAsAttribute: true, prependObjectName: true, "Field"));
			instance.Add("COMPRESSED", new PropertyTypeInfo(PropertyKind.Compressed, "Compressed", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, new List<string> { "Type(BLOB)" }, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field"));
			instance.Add("AUTOINCREMENT", new PropertyTypeInfo(PropertyKind.AutoIncrement, "AutoIncrement", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field"));
			instance.Add("SQLTIMESTAMP", new PropertyTypeInfo(PropertyKind.SqlTimestamp, "SqlTimestamp", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "FieldClass(Normal)" }, new List<string> { "Type(BigInteger)" }, new List<string> { "TableType(Normal,ExternalSQL)" }, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, "SQL_Timestamp", null, null, emitAsAttribute: true, null, prependObjectName: false, "Field"));
			instance.Add("OPTIONMEMBERS", new PropertyTypeInfo(PropertyKind.OptionMembers, "OptionMembers", "String", "OptionValues", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseOptionValuesPropertyValue(pti, ref equals);
			}, null, new List<string> { "Type(Option)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, "OptionString", null, null, emitAsAttribute: true, null, prependObjectName: false, "Field"));
			instance.Add("EXTERNALNAME", new PropertyTypeInfo(PropertyKind.ExternalName, "ExternalName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, new List<string> { "TableType(CRM,ExternalSQL,Exchange,MicrosoftGraph)" }, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field"));
			instance.Add("EXTERNALTYPE", new PropertyTypeInfo(PropertyKind.ExternalType, "ExternalType", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, new List<string> { "TableType(CRM,ExternalSQL,Exchange,MicrosoftGraph)" }, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field"));
			instance.Add("EXTERNALACCESS", new EnumPropertyTypeInfo(PropertyKind.ExternalAccess, "ExternalAccess", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, new List<string> { "TableType(CRM)" }, "Full,Insert,Modify,Read", null, "Full", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Field"));
			instance.Add("OPTIONORDINALVALUES", new PropertyTypeInfo(PropertyKind.OptionOrdinalValues, "OptionOrdinalValues", "String", "CommaSeparatedIdentifierOrLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedIdentifierOrLiteralPropertyValue(pti, ref equals);
			}, null, new List<string> { "Type(Option)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, "Integer", prependObjectName: false, "Field"));
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "No,Pending,Removed", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Field_Table_Key"));
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "ObsoleteState(Pending,Removed)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_Table_Key"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupFieldProperty(string name)
	{
		PropertyTypeInfo value = null;
		FieldProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetKeyProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Key"));
			instance.Add("SUMINDEXFIELDS", new PropertyTypeInfo(PropertyKind.SumIndexFields, "SumIndexFields", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, "Field", prependObjectName: false, "Key"));
			instance.Add("MAINTAINSQLINDEX", new PropertyTypeInfo(PropertyKind.MaintainSqlIndex, "MaintainSqlIndex", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, "MaintainSQLIndex", null, null, emitAsAttribute: true, null, prependObjectName: false, "Key"));
			instance.Add("MAINTAINSIFTINDEX", new PropertyTypeInfo(PropertyKind.MaintainSiftIndex, "MaintainSiftIndex", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, "MaintainSIFTIndex", null, null, emitAsAttribute: true, null, prependObjectName: false, "Key"));
			instance.Add("CLUSTERED", new PropertyTypeInfo(PropertyKind.Clustered, "Clustered", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "MaintainSqlIndex(true)" }, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Key"));
			instance.Add("SQLINDEX", new PropertyTypeInfo(PropertyKind.SqlIndex, "SqlIndex", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, "Field", prependObjectName: false, "Key"));
			instance.Add("UNIQUE", new PropertyTypeInfo(PropertyKind.Unique, "Unique", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Key", VersionCompatibility.Parse("3.0")));
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "No,Pending,Removed", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Field_Table_Key"));
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "ObsoleteState(Pending,Removed)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_Table_Key"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupKeyProperty(string name)
	{
		PropertyTypeInfo value = null;
		KeyProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("SOURCETABLE", new PropertyTypeInfo(PropertyKind.SourceTable, "SourceTable", "String", "ObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("SOURCETABLETEMPORARY", new PropertyTypeInfo(PropertyKind.SourceTableTemporary, "SourceTableTemporary", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "SourceTable" }, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("SOURCETABLEVIEW", new PropertyTypeInfo(PropertyKind.SourceTableView, "SourceTableView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, new List<string> { "SourceTable" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("DATACAPTIONFIELDS", new PropertyTypeInfo(PropertyKind.DataCaptionFields, "DataCaptionFields", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, new List<string> { "SourceTable" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, "Field", prependObjectName: false, "Page_RequestPage"));
			instance.Add("CARDPAGEID", new PropertyTypeInfo(PropertyKind.CardPageId, "CardPageId", "String", "ObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, "CardFormID", null, null, emitAsAttribute: true, "Page", prependObjectName: false, "Page_RequestPage"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("INSERTALLOWED", new PropertyTypeInfo(PropertyKind.InsertAllowed, "InsertAllowed", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("MODIFYALLOWED", new PropertyTypeInfo(PropertyKind.ModifyAllowed, "ModifyAllowed", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("DELETEALLOWED", new PropertyTypeInfo(PropertyKind.DeleteAllowed, "DeleteAllowed", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("AUTOSPLITKEY", new PropertyTypeInfo(PropertyKind.AutoSplitKey, "AutoSplitKey", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("SAVEVALUES", new PropertyTypeInfo(PropertyKind.SaveValues, "SaveValues", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("LINKSALLOWED", new PropertyTypeInfo(PropertyKind.LinksAllowed, "LinksAllowed", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("MULTIPLENEWLINES", new PropertyTypeInfo(PropertyKind.MultipleNewLines, "MultipleNewLines", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("POPULATEALLFIELDS", new PropertyTypeInfo(PropertyKind.PopulateAllFields, "PopulateAllFields", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("SHOWFILTER", new PropertyTypeInfo(PropertyKind.ShowFilter, "ShowFilter", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("INSTRUCTIONALTEXTML", new PropertyTypeInfo(PropertyKind.InstructionalTextML, "InstructionalTextML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("INSTRUCTIONALTEXT", new PropertyTypeInfo(PropertyKind.InstructionalText, "InstructionalText", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("DATACAPTIONEXPRESSION", new PropertyTypeInfo(PropertyKind.DataCaptionExpression, "DataCaptionExpression", "String", "TextExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, "DataCaptionExpr", null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("HELPLINK", new PropertyTypeInfo(PropertyKind.HelpLink, "HelpLink", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("CONTEXTSENSITIVEHELPPAGE", new PropertyTypeInfo(PropertyKind.ContextSensitiveHelpPage, "ContextSensitiveHelpPage", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Extensions, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage", VersionCompatibility.Parse("3.0")));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page"));
			instance.Add("PAGETYPE", new EnumPropertyTypeInfo(PropertyKind.PageType, "PageType", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Card,List,RoleCenter,CardPart,ListPart,Document,Worksheet,ListPlus,ConfirmationDialog,NavigatePage,StandardDialog,API,ReportPreview,ReportProcessingOnly,XmlPort,HeadlinePart", null, "Card", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Page"));
			instance.Add("DELAYEDINSERT", new PropertyTypeInfo(PropertyKind.DelayedInsert, "DelayedInsert", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page"));
			instance.Add("REFRESHONACTIVATE", new PropertyTypeInfo(PropertyKind.RefreshOnActivate, "RefreshOnActivate", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page"));
			instance.Add("PROMOTEDACTIONCATEGORIESML", new PropertyTypeInfo(PropertyKind.PromotedActionCategoriesML, "PromotedActionCategoriesML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page"));
			instance.Add("PROMOTEDACTIONCATEGORIES", new PropertyTypeInfo(PropertyKind.PromotedActionCategories, "PromotedActionCategories", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page"));
			instance.Add("ODATAKEYFIELDS", new PropertyTypeInfo(PropertyKind.ODataKeyFields, "ODataKeyFields", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, new List<string> { "SourceTable" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, "Field", prependObjectName: false, "Page"));
			instance.Add("APIVERSION", new PropertyTypeInfo(PropertyKind.APIVersion, "APIVersion", "String", "CommaSeparatedStrings", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedStringsPropertyValue(pti, ref equals);
			}, new List<string> { "PageType(API)" }, null, null, "beta", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page"));
			instance.Add("ENTITYNAME", new PropertyTypeInfo(PropertyKind.EntityName, "EntityName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "PageType(API)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page"));
			instance.Add("ENTITYSETNAME", new PropertyTypeInfo(PropertyKind.EntitySetName, "EntitySetName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "PageType(API)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page"));
			instance.Add("APIGROUP", new PropertyTypeInfo(PropertyKind.APIGroup, "APIGroup", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "PageType(API)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page"));
			instance.Add("APIPUBLISHER", new PropertyTypeInfo(PropertyKind.APIPublisher, "APIPublisher", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "PageType(API)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page"));
			instance.Add("CHANGETRACKINGALLOWED", new PropertyTypeInfo(PropertyKind.ChangeTrackingAllowed, "ChangeTrackingAllowed", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "PageType(API)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page", VersionCompatibility.Parse("2.0")));
			instance.Add("QUERYCATEGORY", new PropertyTypeInfo(PropertyKind.QueryCategory, "QueryCategory", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, "", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page", VersionCompatibility.Parse("3.0")));
			instance.Add("ADDITIONALSEARCHTERMSML", new PropertyTypeInfo(PropertyKind.AdditionalSearchTermsML, "AdditionalSearchTermsML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page", VersionCompatibility.Parse("3.0")));
			instance.Add("ADDITIONALSEARCHTERMS", new PropertyTypeInfo(PropertyKind.AdditionalSearchTerms, "AdditionalSearchTerms", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page", VersionCompatibility.Parse("3.0")));
			instance.Add("USAGECATEGORY", new EnumPropertyTypeInfo(PropertyKind.UsageCategory, "UsageCategory", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "None,Lists,Tasks,ReportsAndAnalysis,Documents,History,Administration", null, "None", isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Page_Report"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, new List<string> { "UsageCategory(Lists,Tasks,ReportsAndAnalysis,Documents,History,Administration)" }, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, "Page_Report"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, new List<string> { "UsageCategory(Lists,Tasks,ReportsAndAnalysis,Documents,History,Administration)" }, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_Report"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageActionProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("IMAGE", new PropertyTypeInfo(PropertyKind.Image, "Image", "String", "Image", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseImagePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("RUNPAGEMODE", new EnumPropertyTypeInfo(PropertyKind.RunPageMode, "RunPageMode", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "View,Edit,Create", null, "Edit", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "PageAction"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, "PageAction"));
			instance.Add("PROMOTED", new PropertyTypeInfo(PropertyKind.Promoted, "Promoted", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("PROMOTEDISBIG", new PropertyTypeInfo(PropertyKind.PromotedIsBig, "PromotedIsBig", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "Promoted(true)" }, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("PROMOTEDONLY", new PropertyTypeInfo(PropertyKind.PromotedOnly, "PromotedOnly", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "Promoted(true)" }, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("PROMOTEDCATEGORY", new EnumPropertyTypeInfo(PropertyKind.PromotedCategory, "PromotedCategory", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, new List<string> { "Promoted(true)" }, null, null, "New,Process,Report,Category4,Category5,Category6,Category7,Category8,Category9,Category10,Category11,Category12,Category13,Category14,Category15,Category16,Category17,Category18,Category19,Category20", null, "New", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "PageAction"));
			instance.Add("SCOPE", new EnumPropertyTypeInfo(PropertyKind.Scope, "Scope", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Page,Repeater", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: true, "PageAction"));
			instance.Add("ELLIPSIS", new PropertyTypeInfo(PropertyKind.Ellipsis, "Ellipsis", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("SHORTCUTKEY", new PropertyTypeInfo(PropertyKind.ShortcutKey, "ShortcutKey", "String", "Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, "ShortCutKey", null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("RUNOBJECT", new PropertyTypeInfo(PropertyKind.RunObject, "RunObject", "String", "QualifiedObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseQualifiedObjectReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("RUNPAGEVIEW", new PropertyTypeInfo(PropertyKind.RunPageView, "RunPageView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, new List<string> { "RunObject" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "PageAction"));
			instance.Add("RUNPAGELINK", new PropertyTypeInfo(PropertyKind.RunPageLink, "RunPageLink", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, new List<string> { "RunObject" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "PageAction"));
			instance.Add("RUNPAGEONREC", new PropertyTypeInfo(PropertyKind.RunPageOnRec, "RunPageOnRec", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("INFOOTERBAR", new PropertyTypeInfo(PropertyKind.InFooterBar, "InFooterBar", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageAction"));
			instance.Add("GESTURE", new EnumPropertyTypeInfo(PropertyKind.Gesture, "Gesture", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "None,LeftSwipe,RightSwipe,ContextMenu", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "PageAction"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageActionProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageActionProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageActionAreaProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageActionArea"));
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageActionArea"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageActionAreaProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageActionAreaProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageActionGroupProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageActionGroup"));
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageActionGroup"));
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageActionGroup"));
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageActionGroup"));
			instance.Add("IMAGE", new PropertyTypeInfo(PropertyKind.Image, "Image", "String", "Image", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseImagePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageActionGroup"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageActionGroupProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageActionGroupProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageAreaProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageAreaProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageAreaProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageFieldProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("AUTOFORMATTYPE", new PropertyTypeInfo(PropertyKind.AutoFormatType, "AutoFormatType", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("AUTOFORMATEXPRESSION", new PropertyTypeInfo(PropertyKind.AutoFormatExpression, "AutoFormatExpression", "String", "TextExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("BLANKNUMBERS", new EnumPropertyTypeInfo(PropertyKind.BlankNumbers, "BlankNumbers", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Date,DateTime,Decimal,Duration,Enum,Integer,Option,Time)" }, null, "DontBlank,BlankNeg,BlankNegAndZero,BlankZero,BlankZeroAndPos,BlankPos", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Field_PageField"));
			instance.Add("BLANKZERO", new PropertyTypeInfo(PropertyKind.BlankZero, "BlankZero", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Integer,Decimal,Duration,Enum,Option)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("SIGNDISPLACEMENT", new PropertyTypeInfo(PropertyKind.SignDisplacement, "SignDisplacement", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)" }, null, "0", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("MINVALUE", new PropertyTypeInfo(PropertyKind.MinValue, "MinValue", "String", "Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("MAXVALUE", new PropertyTypeInfo(PropertyKind.MaxValue, "MaxValue", "String", "Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("NOTBLANK", new PropertyTypeInfo(PropertyKind.NotBlank, "NotBlank", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("NUMERIC", new PropertyTypeInfo(PropertyKind.Numeric, "Numeric", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, new List<string> { "Type(Code,Text)" }, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("CHARALLOWED", new PropertyTypeInfo(PropertyKind.CharAllowed, "CharAllowed", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(Char,Code,Label,String,Text,TextConst)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, CharAllowedPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("DATEFORMULA", new PropertyTypeInfo(PropertyKind.DateFormula, "DateFormula", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, new List<string> { "Type(Char,Code,Label,String,Text,TextConst)" }, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("VALUESALLOWED", new PropertyTypeInfo(PropertyKind.ValuesAllowed, "ValuesAllowed", "String", "CommaSeparatedIdentifierOrLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedIdentifierOrLiteralPropertyValue(pti, ref equals);
			}, null, new List<string> { "Type(Boolean,Enum,Option,Integer,BigInteger,Decimal,Code,Text)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("OPTIONCAPTIONML", new PropertyTypeInfo(PropertyKind.OptionCaptionML, "OptionCaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, new List<string> { "Type(Option)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("OPTIONCAPTION", new PropertyTypeInfo(PropertyKind.OptionCaption, "OptionCaption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, new List<string> { "Type(Option)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("CLOSINGDATES", new PropertyTypeInfo(PropertyKind.ClosingDates, "ClosingDates", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, new List<string> { "Type(Date)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("DECIMALPLACES", new PropertyTypeInfo(PropertyKind.DecimalPlaces, "DecimalPlaces", "String", "DecimalPlaces", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseDecimalPlacesPropertyValue(pti);
			}, null, new List<string> { "Type(Decimal)" }, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, DecimalPlacesPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("EXTENDEDDATATYPE", new EnumPropertyTypeInfo(PropertyKind.ExtendedDatatype, "ExtendedDatatype", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, "None,PhoneNo,URL,EMail,Ratio,Masked,Person", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Field_PageField"));
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Customizations, WidthPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_PageField"));
			instance.Add("STYLE", new EnumPropertyTypeInfo(PropertyKind.Style, "Style", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "None,Standard,StandardAccent,Strong,StrongAccent,Attention,AttentionAccent,Favorable,Unfavorable,Ambiguous,Subordinate", null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, "PageLabel_PageField"));
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("STYLEEXPR", new PropertyTypeInfo(PropertyKind.StyleExpr, "StyleExpr", "String", "StyleExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStyleExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("ROWSPAN", new PropertyTypeInfo(PropertyKind.RowSpan, "RowSpan", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("COLUMNSPAN", new PropertyTypeInfo(PropertyKind.ColumnSpan, "ColumnSpan", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("CAPTIONCLASS", new PropertyTypeInfo(PropertyKind.CaptionClass, "CaptionClass", "String", "TextExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Blob,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("HIDEVALUE", new PropertyTypeInfo(PropertyKind.HideValue, "HideValue", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("SHOWMANDATORY", new PropertyTypeInfo(PropertyKind.ShowMandatory, "ShowMandatory", "Boolean", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("MULTILINE", new PropertyTypeInfo(PropertyKind.MultiLine, "MultiLine", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("SHOWCAPTION", new PropertyTypeInfo(PropertyKind.ShowCaption, "ShowCaption", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("LOOKUPPAGEID", new PropertyTypeInfo(PropertyKind.LookupPageId, "LookupPageId", "String", "ObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, "LookupFormID", null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("DRILLDOWNPAGEID", new PropertyTypeInfo(PropertyKind.DrillDownPageId, "DrillDownPageId", "String", "ObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, "DrillDownFormID", null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("IMPORTANCE", new EnumPropertyTypeInfo(PropertyKind.Importance, "Importance", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Standard,Promoted,Optional,Additional", null, "Standard", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "PageField"));
			instance.Add("TITLE", new PropertyTypeInfo(PropertyKind.Title, "Title", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: true, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("QUICKENTRY", new PropertyTypeInfo(PropertyKind.QuickEntry, "QuickEntry", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("LOOKUP", new PropertyTypeInfo(PropertyKind.Lookup, "Lookup", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("DRILLDOWN", new PropertyTypeInfo(PropertyKind.DrillDown, "DrillDown", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("ASSISTEDIT", new PropertyTypeInfo(PropertyKind.AssistEdit, "AssistEdit", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("IMAGE", new PropertyTypeInfo(PropertyKind.Image, "Image", "String", "Image", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseImagePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("ODATAEDMTYPE", new PropertyTypeInfo(PropertyKind.ODataEDMType, "ODataEDMType", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageField"));
			instance.Add("TABLERELATION", new PropertyTypeInfo(PropertyKind.TableRelation, "TableRelation", "String", "TableRelation", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableRelationPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "PageField"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageFieldProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageFieldProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageGroupProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageGroup"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageGroup"));
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageGroup"));
			instance.Add("INSTRUCTIONALTEXTML", new PropertyTypeInfo(PropertyKind.InstructionalTextML, "InstructionalTextML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageGroup"));
			instance.Add("INSTRUCTIONALTEXT", new PropertyTypeInfo(PropertyKind.InstructionalText, "InstructionalText", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageGroup"));
			instance.Add("GRIDLAYOUT", new EnumPropertyTypeInfo(PropertyKind.GridLayout, "GridLayout", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Rows,Columns", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, GridLayoutPropertyValueValidator.ValidatePropertyValue, "Layout", null, null, emitAsAttribute: true, prependObjectName: false, "PageGroup"));
			instance.Add("CUEGROUPLAYOUT", new EnumPropertyTypeInfo(PropertyKind.CuegroupLayout, "CuegroupLayout", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Wide", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, CuegroupLayoutPropertyValueValidator.ValidatePropertyValue, "Layout", null, null, emitAsAttribute: true, prependObjectName: false, "PageGroup"));
			instance.Add("INDENTATIONCOLUMN", new PropertyTypeInfo(PropertyKind.IndentationColumn, "IndentationColumn", "String", "IntegerExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseIntegerExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, "IndentationColumnName", null, null, emitAsAttribute: true, null, prependObjectName: false, "PageGroup"));
			instance.Add("INDENTATIONCONTROLS", new PropertyTypeInfo(PropertyKind.IndentationControls, "IndentationControls", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, "Control", prependObjectName: false, "PageGroup"));
			instance.Add("FREEZECOLUMN", new PropertyTypeInfo(PropertyKind.FreezeColumn, "FreezeColumn", "String", "PageFieldReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePageFieldReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, "FreezeColumnID", null, null, emitAsAttribute: true, null, prependObjectName: false, "PageGroup"));
			instance.Add("SHOWASTREE", new PropertyTypeInfo(PropertyKind.ShowAsTree, "ShowAsTree", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageGroup"));
			instance.Add("SHOWCAPTION", new PropertyTypeInfo(PropertyKind.ShowCaption, "ShowCaption", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageGroup"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageGroupProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageGroupProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageLabelProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("STYLE", new EnumPropertyTypeInfo(PropertyKind.Style, "Style", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "None,Standard,StandardAccent,Strong,StrongAccent,Attention,AttentionAccent,Favorable,Unfavorable,Ambiguous,Subordinate", null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, "PageLabel_PageField"));
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("STYLEEXPR", new PropertyTypeInfo(PropertyKind.StyleExpr, "StyleExpr", "String", "StyleExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStyleExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("ROWSPAN", new PropertyTypeInfo(PropertyKind.RowSpan, "RowSpan", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("COLUMNSPAN", new PropertyTypeInfo(PropertyKind.ColumnSpan, "ColumnSpan", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel_PageField"));
			instance.Add("CAPTIONCLASS", new PropertyTypeInfo(PropertyKind.CaptionClass, "CaptionClass", "String", "TextExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel", VersionCompatibility.Parse("3.2")));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel", VersionCompatibility.Parse("3.2")));
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel", VersionCompatibility.Parse("3.2")));
			instance.Add("HIDEVALUE", new PropertyTypeInfo(PropertyKind.HideValue, "HideValue", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel", VersionCompatibility.Parse("3.2")));
			instance.Add("IMPORTANCE", new EnumPropertyTypeInfo(PropertyKind.Importance, "Importance", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Standard,Promoted,Optional,Additional", null, "Standard", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, prependObjectName: true, "PageLabel", VersionCompatibility.Parse("3.2")));
			instance.Add("MULTILINE", new PropertyTypeInfo(PropertyKind.MultiLine, "MultiLine", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel", VersionCompatibility.Parse("3.2")));
			instance.Add("SHOWCAPTION", new PropertyTypeInfo(PropertyKind.ShowCaption, "ShowCaption", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageLabel", VersionCompatibility.Parse("3.2")));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageLabelProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageLabelProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPagePartProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("SUBPAGEVIEW", new PropertyTypeInfo(PropertyKind.SubPageView, "SubPageView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("SUBPAGELINK", new PropertyTypeInfo(PropertyKind.SubPageLink, "SubPageLink", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("SHOWFILTER", new PropertyTypeInfo(PropertyKind.ShowFilter, "ShowFilter", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("UPDATEPROPAGATION", new EnumPropertyTypeInfo(PropertyKind.UpdatePropagation, "UpdatePropagation", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "SubPart,Both", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("PROVIDER", new PropertyTypeInfo(PropertyKind.Provider, "Provider", "String", "PageFieldReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePageFieldReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, "ProviderID", null, null, emitAsAttribute: true, "Control", prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ENTITYNAME", new PropertyTypeInfo(PropertyKind.EntityName, "EntityName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ENTITYSETNAME", new PropertyTypeInfo(PropertyKind.EntitySetName, "EntitySetName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPagePartProperty(string name)
	{
		PropertyTypeInfo value = null;
		PagePartProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageSystemPartProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("SUBPAGEVIEW", new PropertyTypeInfo(PropertyKind.SubPageView, "SubPageView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("SUBPAGELINK", new PropertyTypeInfo(PropertyKind.SubPageLink, "SubPageLink", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("SHOWFILTER", new PropertyTypeInfo(PropertyKind.ShowFilter, "ShowFilter", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("UPDATEPROPAGATION", new EnumPropertyTypeInfo(PropertyKind.UpdatePropagation, "UpdatePropagation", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "SubPart,Both", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("PROVIDER", new PropertyTypeInfo(PropertyKind.Provider, "Provider", "String", "PageFieldReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePageFieldReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, "ProviderID", null, null, emitAsAttribute: true, "Control", prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ENTITYNAME", new PropertyTypeInfo(PropertyKind.EntityName, "EntityName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ENTITYSETNAME", new PropertyTypeInfo(PropertyKind.EntitySetName, "EntitySetName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageSystemPartProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageSystemPartProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageChartPartProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("SUBPAGEVIEW", new PropertyTypeInfo(PropertyKind.SubPageView, "SubPageView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("SUBPAGELINK", new PropertyTypeInfo(PropertyKind.SubPageLink, "SubPageLink", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("SHOWFILTER", new PropertyTypeInfo(PropertyKind.ShowFilter, "ShowFilter", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("UPDATEPROPAGATION", new EnumPropertyTypeInfo(PropertyKind.UpdatePropagation, "UpdatePropagation", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "SubPart,Both", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("PROVIDER", new PropertyTypeInfo(PropertyKind.Provider, "Provider", "String", "PageFieldReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePageFieldReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, "ProviderID", null, null, emitAsAttribute: true, "Control", prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ENTITYNAME", new PropertyTypeInfo(PropertyKind.EntityName, "EntityName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ENTITYSETNAME", new PropertyTypeInfo(PropertyKind.EntitySetName, "EntitySetName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PagePart_PageSystemPart_PageChartPart"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageChartPartProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageChartPartProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetQueryProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort_Report_ReportColumn_Query_QueryColumn_QueryFilter"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort_Report_ReportColumn_Query_QueryColumn_QueryFilter"));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Query"));
			instance.Add("ORDERBY", new PropertyTypeInfo(PropertyKind.OrderBy, "OrderBy", "String", "OrderBy", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseOrderByPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Query"));
			instance.Add("TOPNUMBEROFROWS", new PropertyTypeInfo(PropertyKind.TopNumberOfRows, "TopNumberOfRows", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, "0", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Query"));
			instance.Add("READSTATE", new EnumPropertyTypeInfo(PropertyKind.ReadState, "ReadState", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "ReadUncommitted,ReadShared,ReadExclusive", null, "ReadUncommitted", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "Query"));
			instance.Add("QUERYTYPE", new EnumPropertyTypeInfo(PropertyKind.QueryType, "QueryType", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Normal,API", null, "Normal", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "Query"));
			instance.Add("APIVERSION", new PropertyTypeInfo(PropertyKind.APIVersion, "APIVersion", "String", "CommaSeparatedStrings", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedStringsPropertyValue(pti, ref equals);
			}, new List<string> { "QueryType(API)" }, null, null, "beta", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Query"));
			instance.Add("ENTITYNAME", new PropertyTypeInfo(PropertyKind.EntityName, "EntityName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "QueryType(API)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Query"));
			instance.Add("ENTITYSETNAME", new PropertyTypeInfo(PropertyKind.EntitySetName, "EntitySetName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "QueryType(API)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Query"));
			instance.Add("APIGROUP", new PropertyTypeInfo(PropertyKind.APIGroup, "APIGroup", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "QueryType(API)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Query"));
			instance.Add("APIPUBLISHER", new PropertyTypeInfo(PropertyKind.APIPublisher, "APIPublisher", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "QueryType(API)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Query"));
			instance.Add("QUERYCATEGORY", new PropertyTypeInfo(PropertyKind.QueryCategory, "QueryCategory", "String", "CommaSeparatedStrings", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedStringsPropertyValue(pti, ref equals);
			}, null, null, null, "", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Query", VersionCompatibility.Parse("3.0")));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupQueryProperty(string name)
	{
		PropertyTypeInfo value = null;
		QueryProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetQueryColumnProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort_Report_ReportColumn_Query_QueryColumn_QueryFilter"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort_Report_ReportColumn_Query_QueryColumn_QueryFilter"));
			instance.Add("COLUMNFILTER", new PropertyTypeInfo(PropertyKind.ColumnFilter, "ColumnFilter", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "QueryColumn_QueryFilter"));
			instance.Add("METHOD", new EnumPropertyTypeInfo(PropertyKind.Method, "Method", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Day,Month,Year,Sum,Count,Average,Min,Max", null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: true, "QueryColumn"));
			instance.Add("REVERSESIGN", new PropertyTypeInfo(PropertyKind.ReverseSign, "ReverseSign", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "QueryColumn"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupQueryColumnProperty(string name)
	{
		PropertyTypeInfo value = null;
		QueryColumnProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetQueryDataItemProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("DATAITEMLINK", new PropertyTypeInfo(PropertyKind.DataItemLink, "DataItemLink", "String", "QueryDataItemLink", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseQueryDataItemLinkPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "QueryDataItem"));
			instance.Add("SQLJOINTYPE", new EnumPropertyTypeInfo(PropertyKind.SqlJoinType, "SqlJoinType", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "LeftOuterJoin,InnerJoin,RightOuterJoin,FullOuterJoin,CrossJoin", null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, "DataItemLinkType", null, null, emitAsAttribute: false, prependObjectName: false, "QueryDataItem"));
			instance.Add("DATAITEMTABLEFILTER", new PropertyTypeInfo(PropertyKind.DataItemTableFilter, "DataItemTableFilter", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "QueryDataItem"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupQueryDataItemProperty(string name)
	{
		PropertyTypeInfo value = null;
		QueryDataItemProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetQueryFilterProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort_Report_ReportColumn_Query_QueryColumn_QueryFilter"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort_Report_ReportColumn_Query_QueryColumn_QueryFilter"));
			instance.Add("COLUMNFILTER", new PropertyTypeInfo(PropertyKind.ColumnFilter, "ColumnFilter", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "QueryColumn_QueryFilter"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupQueryFilterProperty(string name)
	{
		PropertyTypeInfo value = null;
		QueryFilterProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetReportProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort_Report_ReportColumn_Query_QueryColumn_QueryFilter"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort_Report_ReportColumn_Query_QueryColumn_QueryFilter"));
			instance.Add("TRANSACTIONTYPE", new EnumPropertyTypeInfo(PropertyKind.TransactionType, "TransactionType", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "UpdateNoLocks,Update,Snapshot,Browse,Report", null, "UpdateNoLocks", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPort_Report"));
			instance.Add("USEREQUESTPAGE", new PropertyTypeInfo(PropertyKind.UseRequestPage, "UseRequestPage", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Report"));
			instance.Add("USESYSTEMPRINTER", new PropertyTypeInfo(PropertyKind.UseSystemPrinter, "UseSystemPrinter", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Report"));
			instance.Add("ENABLEEXTERNALIMAGES", new PropertyTypeInfo(PropertyKind.EnableExternalImages, "EnableExternalImages", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Report"));
			instance.Add("ENABLEHYPERLINKS", new PropertyTypeInfo(PropertyKind.EnableHyperlinks, "EnableHyperlinks", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Report"));
			instance.Add("ENABLEEXTERNALASSEMBLIES", new PropertyTypeInfo(PropertyKind.EnableExternalAssemblies, "EnableExternalAssemblies", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, "Internal", emitAsAttribute: false, null, prependObjectName: false, "Report"));
			instance.Add("PROCESSINGONLY", new PropertyTypeInfo(PropertyKind.ProcessingOnly, "ProcessingOnly", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Report"));
			instance.Add("SHOWPRINTSTATUS", new PropertyTypeInfo(PropertyKind.ShowPrintStatus, "ShowPrintStatus", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Report"));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Report"));
			instance.Add("PAPERSOURCEFIRSTPAGE", new EnumPropertyTypeInfo(PropertyKind.PaperSourceFirstPage, "PaperSourceFirstPage", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Upper,Lower,Middle,Manual,Envelope,ManualFeed,AutomaticFeed,TractorFeed,SmallFormat,LargeFormat,LargeCapacity,Cassette,FormSource,Custom1,Custom2,Custom3,Custom4,Custom5,Custom6,Custom7,Custom8,Custom9,Custom10,Custom11,Custom12,Custom13,Custom14,Custom15,Custom16", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "Report"));
			instance.Add("PAPERSOURCEDEFAULTPAGE", new EnumPropertyTypeInfo(PropertyKind.PaperSourceDefaultPage, "PaperSourceDefaultPage", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Upper,Lower,Middle,Manual,Envelope,ManualFeed,AutomaticFeed,TractorFeed,SmallFormat,LargeFormat,LargeCapacity,Cassette,FormSource,Custom1,Custom2,Custom3,Custom4,Custom5,Custom6,Custom7,Custom8,Custom9,Custom10,Custom11,Custom12,Custom13,Custom14,Custom15,Custom16", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "Report"));
			instance.Add("PAPERSOURCELASTPAGE", new EnumPropertyTypeInfo(PropertyKind.PaperSourceLastPage, "PaperSourceLastPage", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Upper,Lower,Middle,Manual,Envelope,ManualFeed,AutomaticFeed,TractorFeed,SmallFormat,LargeFormat,LargeCapacity,Cassette,FormSource,Custom1,Custom2,Custom3,Custom4,Custom5,Custom6,Custom7,Custom8,Custom9,Custom10,Custom11,Custom12,Custom13,Custom14,Custom15,Custom16", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "Report"));
			instance.Add("DEFAULTLAYOUT", new EnumPropertyTypeInfo(PropertyKind.DefaultLayout, "DefaultLayout", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "RDLC,Word", null, "RDLC", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "Report"));
			instance.Add("WORDMERGEDATAITEM", new PropertyTypeInfo(PropertyKind.WordMergeDataItem, "WordMergeDataItem", "String", "MemberReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMemberReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Report"));
			instance.Add("PDFFONTEMBEDDING", new EnumPropertyTypeInfo(PropertyKind.PdfFontEmbedding, "PdfFontEmbedding", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Default,Yes,No", null, "Default", isObsolete: false, generateMetadata: true, ModificationKind.None, null, "PDFFontEmbedding", null, null, emitAsAttribute: false, prependObjectName: false, "Report"));
			instance.Add("RDLCLAYOUT", new PropertyTypeInfo(PropertyKind.RDLCLayout, "RDLCLayout", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Report"));
			instance.Add("WORDLAYOUT", new PropertyTypeInfo(PropertyKind.WordLayout, "WordLayout", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Report"));
			instance.Add("PREVIEWMODE", new EnumPropertyTypeInfo(PropertyKind.PreviewMode, "PreviewMode", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Normal,PrintLayout", null, "Normal", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "Report"));
			instance.Add("ADDITIONALSEARCHTERMSML", new PropertyTypeInfo(PropertyKind.AdditionalSearchTermsML, "AdditionalSearchTermsML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Report", VersionCompatibility.Parse("3.0")));
			instance.Add("ADDITIONALSEARCHTERMS", new PropertyTypeInfo(PropertyKind.AdditionalSearchTerms, "AdditionalSearchTerms", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Report", VersionCompatibility.Parse("3.0")));
			instance.Add("USAGECATEGORY", new EnumPropertyTypeInfo(PropertyKind.UsageCategory, "UsageCategory", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "None,Lists,Tasks,ReportsAndAnalysis,Documents,History,Administration", null, "None", isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Page_Report"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, new List<string> { "UsageCategory(Lists,Tasks,ReportsAndAnalysis,Documents,History,Administration)" }, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, "Page_Report"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, new List<string> { "UsageCategory(Lists,Tasks,ReportsAndAnalysis,Documents,History,Administration)" }, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_Report"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupReportProperty(string name)
	{
		PropertyTypeInfo value = null;
		ReportProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetReportDataItemProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("DATAITEMTABLEVIEW", new PropertyTypeInfo(PropertyKind.DataItemTableView, "DataItemTableView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "ReportDataItem"));
			instance.Add("DATAITEMLINKREFERENCE", new PropertyTypeInfo(PropertyKind.DataItemLinkReference, "DataItemLinkReference", "String", "MemberReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMemberReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "ReportDataItem"));
			instance.Add("DATAITEMLINK", new PropertyTypeInfo(PropertyKind.DataItemLink, "DataItemLink", "String", "ReportDataItemLink", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseReportDataItemLinkPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "ReportDataItem"));
			instance.Add("REQUESTFILTERHEADINGML", new PropertyTypeInfo(PropertyKind.RequestFilterHeadingML, "RequestFilterHeadingML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ReportDataItem"));
			instance.Add("REQUESTFILTERHEADING", new PropertyTypeInfo(PropertyKind.RequestFilterHeading, "RequestFilterHeading", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ReportDataItem"));
			instance.Add("REQUESTFILTERFIELDS", new PropertyTypeInfo(PropertyKind.RequestFilterFields, "RequestFilterFields", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, "ReqFilterFields", null, null, emitAsAttribute: true, "Field", prependObjectName: false, "ReportDataItem"));
			instance.Add("CALCFIELDS", new PropertyTypeInfo(PropertyKind.CalcFields, "CalcFields", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, "Field", prependObjectName: false, "ReportDataItem"));
			instance.Add("MAXITERATION", new PropertyTypeInfo(PropertyKind.MaxIteration, "MaxIteration", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, "0", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "ReportDataItem"));
			instance.Add("PRINTONLYIFDETAIL", new PropertyTypeInfo(PropertyKind.PrintOnlyIfDetail, "PrintOnlyIfDetail", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "ReportDataItem"));
			instance.Add("USETEMPORARY", new PropertyTypeInfo(PropertyKind.UseTemporary, "UseTemporary", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "ReportDataItem"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupReportDataItemProperty(string name)
	{
		PropertyTypeInfo value = null;
		ReportDataItemProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetReportColumnProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort_Report_ReportColumn_Query_QueryColumn_QueryFilter"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort_Report_ReportColumn_Query_QueryColumn_QueryFilter"));
			instance.Add("OPTIONCAPTIONML", new PropertyTypeInfo(PropertyKind.OptionCaptionML, "OptionCaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, new List<string> { "Type(Option)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "ReportColumn"));
			instance.Add("OPTIONCAPTION", new PropertyTypeInfo(PropertyKind.OptionCaption, "OptionCaption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, new List<string> { "Type(Option)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ReportColumn"));
			instance.Add("OPTIONMEMBERS", new PropertyTypeInfo(PropertyKind.OptionMembers, "OptionMembers", "String", "OptionValues", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseOptionValuesPropertyValue(pti, ref equals);
			}, null, new List<string> { "Type(Option)" }, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, "OptionString", null, null, emitAsAttribute: false, null, prependObjectName: false, "ReportColumn"));
			instance.Add("DECIMALPLACES", new PropertyTypeInfo(PropertyKind.DecimalPlaces, "DecimalPlaces", "String", "DecimalPlaces", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseDecimalPlacesPropertyValue(pti);
			}, null, new List<string> { "Type(Decimal)" }, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, DecimalPlacesPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "ReportColumn"));
			instance.Add("AUTOFORMATTYPE", new PropertyTypeInfo(PropertyKind.AutoFormatType, "AutoFormatType", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "ReportColumn"));
			instance.Add("AUTOFORMATEXPRESSION", new PropertyTypeInfo(PropertyKind.AutoFormatExpression, "AutoFormatExpression", "String", "TextExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			}, null, new List<string> { "Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)" }, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "ReportColumn"));
			instance.Add("AUTOCALCFIELD", new PropertyTypeInfo(PropertyKind.AutoCalcField, "AutoCalcField", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "ReportColumn"));
			instance.Add("INCLUDECAPTION", new PropertyTypeInfo(PropertyKind.IncludeCaption, "IncludeCaption", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "ReportColumn"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupReportColumnProperty(string name)
	{
		PropertyTypeInfo value = null;
		ReportColumnProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetRequestPageProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("SOURCETABLE", new PropertyTypeInfo(PropertyKind.SourceTable, "SourceTable", "String", "ObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("SOURCETABLETEMPORARY", new PropertyTypeInfo(PropertyKind.SourceTableTemporary, "SourceTableTemporary", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "SourceTable" }, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("SOURCETABLEVIEW", new PropertyTypeInfo(PropertyKind.SourceTableView, "SourceTableView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, new List<string> { "SourceTable" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("DATACAPTIONFIELDS", new PropertyTypeInfo(PropertyKind.DataCaptionFields, "DataCaptionFields", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, new List<string> { "SourceTable" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, "Field", prependObjectName: false, "Page_RequestPage"));
			instance.Add("CARDPAGEID", new PropertyTypeInfo(PropertyKind.CardPageId, "CardPageId", "String", "ObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, "CardFormID", null, null, emitAsAttribute: true, "Page", prependObjectName: false, "Page_RequestPage"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("INSERTALLOWED", new PropertyTypeInfo(PropertyKind.InsertAllowed, "InsertAllowed", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("MODIFYALLOWED", new PropertyTypeInfo(PropertyKind.ModifyAllowed, "ModifyAllowed", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("DELETEALLOWED", new PropertyTypeInfo(PropertyKind.DeleteAllowed, "DeleteAllowed", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("AUTOSPLITKEY", new PropertyTypeInfo(PropertyKind.AutoSplitKey, "AutoSplitKey", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("SAVEVALUES", new PropertyTypeInfo(PropertyKind.SaveValues, "SaveValues", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("LINKSALLOWED", new PropertyTypeInfo(PropertyKind.LinksAllowed, "LinksAllowed", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("MULTIPLENEWLINES", new PropertyTypeInfo(PropertyKind.MultipleNewLines, "MultipleNewLines", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("POPULATEALLFIELDS", new PropertyTypeInfo(PropertyKind.PopulateAllFields, "PopulateAllFields", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("SHOWFILTER", new PropertyTypeInfo(PropertyKind.ShowFilter, "ShowFilter", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("INSTRUCTIONALTEXTML", new PropertyTypeInfo(PropertyKind.InstructionalTextML, "InstructionalTextML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("INSTRUCTIONALTEXT", new PropertyTypeInfo(PropertyKind.InstructionalText, "InstructionalText", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("DATACAPTIONEXPRESSION", new PropertyTypeInfo(PropertyKind.DataCaptionExpression, "DataCaptionExpression", "String", "TextExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, "DataCaptionExpr", null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("HELPLINK", new PropertyTypeInfo(PropertyKind.HelpLink, "HelpLink", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage"));
			instance.Add("CONTEXTSENSITIVEHELPPAGE", new PropertyTypeInfo(PropertyKind.ContextSensitiveHelpPage, "ContextSensitiveHelpPage", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Extensions, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Page_RequestPage", VersionCompatibility.Parse("3.0")));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "RequestPage"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupRequestPageProperty(string name)
	{
		PropertyTypeInfo value = null;
		RequestPageProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetTableProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("DATACLASSIFICATION", new EnumPropertyTypeInfo(PropertyKind.DataClassification, "DataClassification", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "CustomerContent,EndUserIdentifiableInformation,AccountData,EndUserPseudonymousIdentifiers,OrganizationIdentifiableInformation,SystemMetadata,ToBeClassified", null, "CustomerContent", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: true, "Table"));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table"));
			instance.Add("DATAPERCOMPANY", new PropertyTypeInfo(PropertyKind.DataPerCompany, "DataPerCompany", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "TableType(Normal)" }, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table"));
			instance.Add("LOOKUPPAGEID", new PropertyTypeInfo(PropertyKind.LookupPageId, "LookupPageId", "String", "ObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, "LookupFormID", null, null, emitAsAttribute: true, null, prependObjectName: false, "Table"));
			instance.Add("DRILLDOWNPAGEID", new PropertyTypeInfo(PropertyKind.DrillDownPageId, "DrillDownPageId", "String", "ObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, "DrillDownFormID", null, null, emitAsAttribute: true, null, prependObjectName: false, "Table"));
			instance.Add("DATACAPTIONFIELDS", new PropertyTypeInfo(PropertyKind.DataCaptionFields, "DataCaptionFields", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, "Field", prependObjectName: false, "Table"));
			instance.Add("PASTEISVALID", new PropertyTypeInfo(PropertyKind.PasteIsValid, "PasteIsValid", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table"));
			instance.Add("LINKEDOBJECT", new PropertyTypeInfo(PropertyKind.LinkedObject, "LinkedObject", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table"));
			instance.Add("LINKEDINTRANSACTION", new PropertyTypeInfo(PropertyKind.LinkedInTransaction, "LinkedInTransaction", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "LinkedObject(true)" }, null, null, "true", isObsolete: true, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table"));
			instance.Add("TABLETYPE", new EnumPropertyTypeInfo(PropertyKind.TableType, "TableType", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Normal,CRM,ExternalSQL,Exchange,MicrosoftGraph", "Extension,Extension,Internal,Internal,Internal", "Normal", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Table"));
			instance.Add("COMPRESSIONTYPE", new EnumPropertyTypeInfo(PropertyKind.CompressionType, "CompressionType", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, new List<string> { "TableType(Normal)" }, null, null, "Unspecified,None,Row,Page", null, "Unspecified", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Table", VersionCompatibility.Parse("3.0")));
			instance.Add("EXTERNALNAME", new PropertyTypeInfo(PropertyKind.ExternalName, "ExternalName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "TableType(CRM,ExternalSQL,Exchange,MicrosoftGraph)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table"));
			instance.Add("EXTERNALSCHEMA", new PropertyTypeInfo(PropertyKind.ExternalSchema, "ExternalSchema", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "TableType(CRM,ExternalSQL)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table"));
			instance.Add("SCOPE", new EnumPropertyTypeInfo(PropertyKind.Scope, "Scope", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Personalization,Extension,Internal", null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: true, "Table"));
			instance.Add("REPLICATEDATA", new PropertyTypeInfo(PropertyKind.ReplicateData, "ReplicateData", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table", VersionCompatibility.Parse("2.0")));
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "No,Pending,Removed", null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, prependObjectName: false, "Field_Table_Key"));
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "ObsoleteState(Pending,Removed)" }, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Field_Table_Key"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupTableProperty(string name)
	{
		PropertyTypeInfo value = null;
		TableProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetXmlPortProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort_Report_ReportColumn_Query_QueryColumn_QueryFilter"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort_Report_ReportColumn_Query_QueryColumn_QueryFilter"));
			instance.Add("TRANSACTIONTYPE", new EnumPropertyTypeInfo(PropertyKind.TransactionType, "TransactionType", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "UpdateNoLocks,Update,Snapshot,Browse,Report", null, "UpdateNoLocks", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPort_Report"));
			instance.Add("DEFAULTFIELDSVALIDATION", new PropertyTypeInfo(PropertyKind.DefaultFieldsValidation, "DefaultFieldsValidation", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort"));
			instance.Add("DEFAULTNAMESPACE", new PropertyTypeInfo(PropertyKind.DefaultNamespace, "DefaultNamespace", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "Format(Xml)" }, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "XmlPort"));
			instance.Add("DIRECTION", new EnumPropertyTypeInfo(PropertyKind.Direction, "Direction", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Import,Export,Both", null, "Both", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPort"));
			instance.Add("ENCODING", new EnumPropertyTypeInfo(PropertyKind.Encoding, "Encoding", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, new List<string> { "Format(Xml)" }, null, null, "UTF8,UTF16,ISO88592", null, "UTF16", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPort"));
			instance.Add("FIELDDELIMITER", new PropertyTypeInfo(PropertyKind.FieldDelimiter, "FieldDelimiter", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "Format(VariableText)" }, null, null, "\"", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort"));
			instance.Add("FIELDSEPARATOR", new PropertyTypeInfo(PropertyKind.FieldSeparator, "FieldSeparator", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "Format(VariableText)" }, null, null, ",", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort"));
			instance.Add("FILENAME", new PropertyTypeInfo(PropertyKind.FileName, "FileName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, "", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort"));
			instance.Add("FORMAT", new EnumPropertyTypeInfo(PropertyKind.Format, "Format", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Xml,VariableText,FixedText", null, "Xml", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPort"));
			instance.Add("FORMATEVALUATE", new EnumPropertyTypeInfo(PropertyKind.FormatEvaluate, "FormatEvaluate", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Legacy,Xml", null, "Legacy", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPort"));
			instance.Add("INLINESCHEMA", new PropertyTypeInfo(PropertyKind.InlineSchema, "InlineSchema", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "Format(Xml)" }, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort"));
			instance.Add("NAMESPACES", new PropertyTypeInfo(PropertyKind.Namespaces, "Namespaces", "String", "CommaSeparatedIdentifierEqualsStringList", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedIdentifierEqualsStringListPropertyValue(pti, ref equals);
			}, new List<string> { "Format(Xml)" }, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, NamespacesPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "XmlPort"));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort"));
			instance.Add("PRESERVEWHITESPACE", new PropertyTypeInfo(PropertyKind.PreserveWhiteSpace, "PreserveWhiteSpace", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort"));
			instance.Add("RECORDSEPARATOR", new PropertyTypeInfo(PropertyKind.RecordSeparator, "RecordSeparator", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "Format(VariableText,FixedText)" }, null, null, "<NewLine>", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort"));
			instance.Add("TABLESEPARATOR", new PropertyTypeInfo(PropertyKind.TableSeparator, "TableSeparator", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, new List<string> { "Format(VariableText,FixedText)" }, null, null, "<NewLine><NewLine>", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort"));
			instance.Add("TEXTENCODING", new EnumPropertyTypeInfo(PropertyKind.TextEncoding, "TextEncoding", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, new List<string> { "Format(VariableText,FixedText)" }, null, null, "MSDOS,UTF8,UTF16,WINDOWS", null, "MSDOS", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPort"));
			instance.Add("USEDEFAULTNAMESPACE", new PropertyTypeInfo(PropertyKind.UseDefaultNamespace, "UseDefaultNamespace", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "Format(Xml)" }, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort"));
			instance.Add("USELAX", new PropertyTypeInfo(PropertyKind.UseLax, "UseLax", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, new List<string> { "Format(Xml)" }, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort"));
			instance.Add("USEREQUESTPAGE", new PropertyTypeInfo(PropertyKind.UseRequestPage, "UseRequestPage", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, "UseRequestForm", null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPort"));
			instance.Add("XMLVERSIONNO", new EnumPropertyTypeInfo(PropertyKind.XmlVersionNo, "XmlVersionNo", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, new List<string> { "Format(Xml)" }, null, null, "V10,V11", null, "V10", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPort"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupXmlPortProperty(string name)
	{
		PropertyTypeInfo value = null;
		XmlPortProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetXmlPortTextElementProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("TEXTTYPE", new EnumPropertyTypeInfo(PropertyKind.TextType, "TextType", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Text,BigText", null, "Text", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPortTextAttribute_XmlPortTextElement"));
			instance.Add("MAXOCCURS", new EnumPropertyTypeInfo(PropertyKind.MaxOccurs, "MaxOccurs", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Once,Unbounded", null, "Unbounded", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement"));
			instance.Add("MINOCCURS", new EnumPropertyTypeInfo(PropertyKind.MinOccurs, "MinOccurs", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Zero,Once", null, "Once", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement"));
			instance.Add("XMLNAME", new PropertyTypeInfo(PropertyKind.XmlName, "XmlName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("NAMESPACEPREFIX", new PropertyTypeInfo(PropertyKind.NamespacePrefix, "NamespacePrefix", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("UNBOUND", new PropertyTypeInfo(PropertyKind.Unbound, "Unbound", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, "0", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupXmlPortTextElementProperty(string name)
	{
		PropertyTypeInfo value = null;
		XmlPortTextElementProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetXmlPortFieldElementProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("AUTOCALCFIELD", new PropertyTypeInfo(PropertyKind.AutoCalcField, "AutoCalcField", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortFieldAttribute_XmlPortFieldElement"));
			instance.Add("FIELDVALIDATE", new EnumPropertyTypeInfo(PropertyKind.FieldValidate, "FieldValidate", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Yes,No,Undefined", null, "Undefined", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPortFieldAttribute_XmlPortFieldElement"));
			instance.Add("MAXOCCURS", new EnumPropertyTypeInfo(PropertyKind.MaxOccurs, "MaxOccurs", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Once,Unbounded", null, "Unbounded", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement"));
			instance.Add("MINOCCURS", new EnumPropertyTypeInfo(PropertyKind.MinOccurs, "MinOccurs", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Zero,Once", null, "Once", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement"));
			instance.Add("XMLNAME", new PropertyTypeInfo(PropertyKind.XmlName, "XmlName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("NAMESPACEPREFIX", new PropertyTypeInfo(PropertyKind.NamespacePrefix, "NamespacePrefix", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("UNBOUND", new PropertyTypeInfo(PropertyKind.Unbound, "Unbound", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, "0", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupXmlPortFieldElementProperty(string name)
	{
		PropertyTypeInfo value = null;
		XmlPortFieldElementProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetXmlPortTableElementProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("LINKTABLE", new PropertyTypeInfo(PropertyKind.LinkTable, "LinkTable", "String", "MemberReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMemberReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTableElement"));
			instance.Add("LINKTABLEFORCEINSERT", new PropertyTypeInfo(PropertyKind.LinkTableForceInsert, "LinkTableForceInsert", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTableElement"));
			instance.Add("LINKFIELDS", new PropertyTypeInfo(PropertyKind.LinkFields, "LinkFields", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, new List<string> { "LinkTable" }, null, null, "", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTableElement"));
			instance.Add("REQUESTFILTERFIELDS", new PropertyTypeInfo(PropertyKind.RequestFilterFields, "RequestFilterFields", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, "", isObsolete: false, generateMetadata: true, ModificationKind.None, null, "ReqFilterFields", null, null, emitAsAttribute: false, "Field", prependObjectName: false, "XmlPortTableElement"));
			instance.Add("REQUESTFILTERHEADINGML", new PropertyTypeInfo(PropertyKind.RequestFilterHeadingML, "RequestFilterHeadingML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, "ReqFilterHeadingML", null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTableElement"));
			instance.Add("REQUESTFILTERHEADING", new PropertyTypeInfo(PropertyKind.RequestFilterHeading, "RequestFilterHeading", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, "ReqFilterHeadingML", null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTableElement"));
			instance.Add("SOURCETABLEVIEW", new PropertyTypeInfo(PropertyKind.SourceTableView, "SourceTableView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, null, null, null, "", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTableElement"));
			instance.Add("USETEMPORARY", new PropertyTypeInfo(PropertyKind.UseTemporary, "UseTemporary", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, "Temporary", null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTableElement"));
			instance.Add("AUTOREPLACE", new PropertyTypeInfo(PropertyKind.AutoReplace, "AutoReplace", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTableElement"));
			instance.Add("AUTOSAVE", new PropertyTypeInfo(PropertyKind.AutoSave, "AutoSave", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTableElement"));
			instance.Add("AUTOUPDATE", new PropertyTypeInfo(PropertyKind.AutoUpdate, "AutoUpdate", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTableElement"));
			instance.Add("CALCFIELDS", new PropertyTypeInfo(PropertyKind.CalcFields, "CalcFields", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, "", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, "Field", prependObjectName: false, "XmlPortTableElement"));
			instance.Add("MAXOCCURS", new EnumPropertyTypeInfo(PropertyKind.MaxOccurs, "MaxOccurs", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Once,Unbounded", null, "Unbounded", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement"));
			instance.Add("MINOCCURS", new EnumPropertyTypeInfo(PropertyKind.MinOccurs, "MinOccurs", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Zero,Once", null, "Once", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement"));
			instance.Add("XMLNAME", new PropertyTypeInfo(PropertyKind.XmlName, "XmlName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("NAMESPACEPREFIX", new PropertyTypeInfo(PropertyKind.NamespacePrefix, "NamespacePrefix", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("UNBOUND", new PropertyTypeInfo(PropertyKind.Unbound, "Unbound", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, "0", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupXmlPortTableElementProperty(string name)
	{
		PropertyTypeInfo value = null;
		XmlPortTableElementProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetXmlPortFieldAttributeProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("AUTOCALCFIELD", new PropertyTypeInfo(PropertyKind.AutoCalcField, "AutoCalcField", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortFieldAttribute_XmlPortFieldElement"));
			instance.Add("FIELDVALIDATE", new EnumPropertyTypeInfo(PropertyKind.FieldValidate, "FieldValidate", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Yes,No,Undefined", null, "Undefined", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPortFieldAttribute_XmlPortFieldElement"));
			instance.Add("OCCURRENCE", new EnumPropertyTypeInfo(PropertyKind.Occurrence, "Occurrence", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Required,Optional", null, "Required", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("XMLNAME", new PropertyTypeInfo(PropertyKind.XmlName, "XmlName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("NAMESPACEPREFIX", new PropertyTypeInfo(PropertyKind.NamespacePrefix, "NamespacePrefix", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("UNBOUND", new PropertyTypeInfo(PropertyKind.Unbound, "Unbound", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, "0", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupXmlPortFieldAttributeProperty(string name)
	{
		PropertyTypeInfo value = null;
		XmlPortFieldAttributeProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetXmlPortTextAttributeProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("TEXTTYPE", new EnumPropertyTypeInfo(PropertyKind.TextType, "TextType", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Text,BigText", null, "Text", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPortTextAttribute_XmlPortTextElement"));
			instance.Add("OCCURRENCE", new EnumPropertyTypeInfo(PropertyKind.Occurrence, "Occurrence", "Enum", "EnumLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			}, null, null, null, "Required,Optional", null, "Required", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, prependObjectName: false, "XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("XMLNAME", new PropertyTypeInfo(PropertyKind.XmlName, "XmlName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("NAMESPACEPREFIX", new PropertyTypeInfo(PropertyKind.NamespacePrefix, "NamespacePrefix", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("UNBOUND", new PropertyTypeInfo(PropertyKind.Unbound, "Unbound", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, "0", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupXmlPortTextAttributeProperty(string name)
	{
		PropertyTypeInfo value = null;
		XmlPortTextAttributeProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetFieldGroupProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupFieldGroupProperty(string name)
	{
		PropertyTypeInfo value = null;
		FieldGroupProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageActionSeparatorProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionGroup_PageActionSeparator"));
			instance.Add("ISHEADER", new PropertyTypeInfo(PropertyKind.IsHeader, "IsHeader", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageActionSeparator"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageActionSeparatorProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageActionSeparatorProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetEnumValueProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "EnumValue", VersionCompatibility.Parse("2.3")));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "EnumValue", VersionCompatibility.Parse("2.0")));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupEnumValueProperty(string name)
	{
		PropertyTypeInfo value = null;
		EnumValueProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetDotNetAssemblyProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("VERSION", new PropertyTypeInfo(PropertyKind.Version, "Version", "String", "Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "DotNetAssembly"));
			instance.Add("CULTURE", new PropertyTypeInfo(PropertyKind.Culture, "Culture", "String", "Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "DotNetAssembly"));
			instance.Add("PUBLICKEYTOKEN", new PropertyTypeInfo(PropertyKind.PublicKeyToken, "PublicKeyToken", "String", "Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "DotNetAssembly"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupDotNetAssemblyProperty(string name)
	{
		PropertyTypeInfo value = null;
		DotNetAssemblyProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetDotNetTypeDeclarationProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("ISCONTROLADDIN", new PropertyTypeInfo(PropertyKind.IsControlAddIn, "IsControlAddIn", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "DotNetTypeDeclaration", VersionCompatibility.Parse("2.0")));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupDotNetTypeDeclarationProperty(string name)
	{
		PropertyTypeInfo value = null;
		DotNetTypeDeclarationProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageViewProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageView", VersionCompatibility.Parse("3.0")));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageView", VersionCompatibility.Parse("3.0")));
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageView", VersionCompatibility.Parse("3.0")));
			instance.Add("FILTERS", new PropertyTypeInfo(PropertyKind.Filters, "Filters", "String", "Filters", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseFiltersPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "PageView", VersionCompatibility.Parse("3.0")));
			instance.Add("ORDERBY", new PropertyTypeInfo(PropertyKind.OrderBy, "OrderBy", "String", "OrderBy", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseOrderByPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, "PageView", VersionCompatibility.Parse("3.0")));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageViewProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageViewProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetProfileProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Profile"));
			instance.Add("ROLECENTER", new PropertyTypeInfo(PropertyKind.RoleCenter, "RoleCenter", "String", "ObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "Profile"));
			instance.Add("CUSTOMIZATIONS", new PropertyTypeInfo(PropertyKind.Customizations, "Customizations", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.None, null, "PageExtensions", null, null, emitAsAttribute: true, "PageCustomization", prependObjectName: false, "Profile"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupProfileProperty(string name)
	{
		PropertyTypeInfo value = null;
		ProfileProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageCustomizationObjectProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageCustomizationObjectProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageCustomizationObjectProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetControlAddInObjectProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("SCRIPTS", new PropertyTypeInfo(PropertyKind.Scripts, "Scripts", "String", "CommaSeparatedStrings", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedStringsPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("STYLESHEETS", new PropertyTypeInfo(PropertyKind.StyleSheets, "StyleSheets", "String", "CommaSeparatedStrings", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedStringsPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("IMAGES", new PropertyTypeInfo(PropertyKind.Images, "Images", "String", "CommaSeparatedStrings", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedStringsPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("STARTUPSCRIPT", new PropertyTypeInfo(PropertyKind.StartupScript, "StartupScript", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("RECREATESCRIPT", new PropertyTypeInfo(PropertyKind.RecreateScript, "RecreateScript", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("REFRESHSCRIPT", new PropertyTypeInfo(PropertyKind.RefreshScript, "RefreshScript", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, isObsolete: false, generateMetadata: false, ModificationKind.None, EmptyPropertyValueValidator.ValidatePropertyValue, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("REQUESTEDHEIGHT", new PropertyTypeInfo(PropertyKind.RequestedHeight, "RequestedHeight", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, "0", isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("REQUESTEDWIDTH", new PropertyTypeInfo(PropertyKind.RequestedWidth, "RequestedWidth", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, "0", isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("MINIMUMHEIGHT", new PropertyTypeInfo(PropertyKind.MinimumHeight, "MinimumHeight", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, "0", isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("MINIMUMWIDTH", new PropertyTypeInfo(PropertyKind.MinimumWidth, "MinimumWidth", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, "0", isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("MAXIMUMHEIGHT", new PropertyTypeInfo(PropertyKind.MaximumHeight, "MaximumHeight", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, "0", isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("MAXIMUMWIDTH", new PropertyTypeInfo(PropertyKind.MaximumWidth, "MaximumWidth", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, "0", isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("VERTICALSHRINK", new PropertyTypeInfo(PropertyKind.VerticalShrink, "VerticalShrink", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("HORIZONTALSHRINK", new PropertyTypeInfo(PropertyKind.HorizontalShrink, "HorizontalShrink", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("VERTICALSTRETCH", new PropertyTypeInfo(PropertyKind.VerticalStretch, "VerticalStretch", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			instance.Add("HORIZONTALSTRETCH", new PropertyTypeInfo(PropertyKind.HorizontalStretch, "HorizontalStretch", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: false, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "ControlAddInObject"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupControlAddInObjectProperty(string name)
	{
		PropertyTypeInfo value = null;
		ControlAddInObjectProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageUserControlProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			}, null, null, null, "true", isObsolete: false, generateMetadata: true, ModificationKind.Customizations, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "PageUserControl"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, isObsolete: false, generateMetadata: true, ModificationKind.Extensions, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, "PageUserControl"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageUserControlProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageUserControlProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetEnumTypeProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("EXTENSIBLE", new PropertyTypeInfo(PropertyKind.Extensible, "Extensible", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, "false", isObsolete: false, generateMetadata: true, ModificationKind.None, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, "EnumType", VersionCompatibility.Parse("2.0")));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupEnumTypeProperty(string name)
	{
		PropertyTypeInfo value = null;
		EnumTypeProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	internal ObjectParser(Lexer lexer, SyntaxNode oldTree, IEnumerable<TextChangeRange> changes, CancellationToken cancellationToken = default(CancellationToken))
		: base(lexer, LexerMode.Object, oldTree, changes, cancellationToken)
	{
	}

	internal InternalObjectCompilationUnitSyntax ParseObjects()
	{
		InternalSyntaxListBuilder<InternalObjectSyntax> objects = Pool.Allocate<InternalObjectSyntax>();
		try
		{
			InternalSyntaxListBuilder initialBadNodes = null;
			while (!IsEndOfFile)
			{
				InternalObjectSyntax node = null;
				switch (CurrentToken.Kind)
				{
				case SyntaxKind.CodeunitKeyword:
					node = ParseCodeunit();
					break;
				case SyntaxKind.TableKeyword:
					node = ParseTable();
					break;
				case SyntaxKind.TableExtensionKeyword:
					node = ParseTableExtension();
					break;
				case SyntaxKind.PageKeyword:
					node = ParsePage();
					break;
				case SyntaxKind.PageExtensionKeyword:
					node = ParsePageExtension();
					break;
				case SyntaxKind.ReportKeyword:
					node = ParseReport();
					break;
				case SyntaxKind.ProfileKeyword:
					node = ParseProfile();
					break;
				case SyntaxKind.PageCustomizationKeyword:
					node = ParsePageCustomization();
					break;
				case SyntaxKind.XmlPortKeyword:
					node = ParseXmlPort();
					break;
				case SyntaxKind.QueryKeyword:
					node = ParseQuery();
					break;
				case SyntaxKind.ControlAddInKeyword:
					node = ParseControlAddIn();
					break;
				case SyntaxKind.DotNetKeyword:
					node = ParseDotNet();
					break;
				case SyntaxKind.EnumKeyword:
					node = ParseEnum();
					break;
				case SyntaxKind.EnumExtensionKeyword:
					node = ParseEnumExtension();
					break;
				default:
					SkipBadApplicationObjectTokens(ref objects, ref initialBadNodes);
					break;
				}
				objects.Add(node);
			}
			InternalObjectCompilationUnitSyntax internalObjectCompilationUnitSyntax = ContextAwareSyntaxFactory.ObjectCompilationUnit(objects, (CurrentToken.Kind == SyntaxKind.EndOfFileToken) ? CurrentToken : null);
			if (initialBadNodes != null)
			{
				internalObjectCompilationUnitSyntax = AddLeadingSkippedSyntax(internalObjectCompilationUnitSyntax, initialBadNodes.ToListNode());
			}
			return internalObjectCompilationUnitSyntax;
		}
		finally
		{
			Pool.Free(objects);
		}
	}

	private InternalSyntaxToken ParseCloseBraceToken(InternalSyntaxToken openBraceToken)
	{
		if (openBraceToken.IsMissing)
		{
			InternalSyntaxToken node = InternalSyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken);
			return WithAdditionalDiagnostics(node, GetExpectedTokenError(SyntaxKind.CloseBraceToken, CurrentToken.Kind));
		}
		return EatToken(SyntaxKind.CloseBraceToken);
	}

	private InternalPropertyListSyntax ParsePropertyList(Func<string, PropertyTypeInfo> lookupProperty, bool asExtension = false, bool deferErrorChecking = false)
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.PropertyList)
		{
			return (InternalPropertyListSyntax)EatNode();
		}
		InternalSyntaxListBuilder<InternalPropertySyntaxOrEmpty> internalSyntaxListBuilder = Pool.Allocate<InternalPropertySyntaxOrEmpty>();
		try
		{
			if (lookupProperty != null)
			{
				while (!IsEndOfFile && (CurrentToken.Kind == SyntaxKind.IdentifierToken || CurrentToken.Kind == SyntaxKind.SemicolonToken))
				{
					if (IsIncremental && CurrentNodeKind == SyntaxKind.Property)
					{
						InternalPropertySyntax node = (InternalPropertySyntax)EatNode();
						internalSyntaxListBuilder.Add(node);
						continue;
					}
					InternalPropertySyntaxOrEmpty node2;
					if (CurrentToken.Kind == SyntaxKind.SemicolonToken)
					{
						node2 = ContextAwareSyntaxFactory.EmptyProperty(EatToken(SyntaxKind.SemicolonToken));
					}
					else
					{
						InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxKind.IdentifierToken);
						InternalPropertyNameSyntax internalPropertyNameSyntax = ContextAwareSyntaxFactory.PropertyName(internalSyntaxToken);
						string text = internalPropertyNameSyntax.Identifier.Text;
						PropertyTypeInfo propertyTypeInfo = lookupProperty(internalSyntaxToken.Text.ToUpperInvariant());
						if (!deferErrorChecking)
						{
							if (propertyTypeInfo == null)
							{
								internalPropertyNameSyntax = AddError(internalPropertyNameSyntax, ErrorCode.ERR_PropertyInfoNotAvailable, text);
							}
							else if (asExtension && propertyTypeInfo.Modification == ModificationKind.None)
							{
								internalPropertyNameSyntax = AddError(internalPropertyNameSyntax, ErrorCode.ERR_NonCustomizableProperty, text);
							}
						}
						InternalSyntaxToken equals = null;
						InternalPropertyValueSyntax previousToken = null;
						if (CurrentToken.Kind == SyntaxKind.EqualsToken && propertyTypeInfo != null)
						{
							previousToken = ParseInLexerMode(LexerMode.Property, () =>
							{
								equals = EatToken(SyntaxKind.EqualsToken);
								return propertyTypeInfo.ParseValue(this, ref equals);
							});
							DebugAssertHelper.Assert(previousToken != null, "value should never be null here. If it is we have a bug.");
							if (previousToken != null && previousToken.ContainsDiagnostics)
							{
								SkipBadPropertyValue(ref previousToken);
							}
						}
						else
						{
							equals = EatToken(SyntaxKind.EqualsToken);
							SkipBadPropertyValue(ref equals);
						}
						node2 = ContextAwareSyntaxFactory.Property(internalPropertyNameSyntax, equals, previousToken, EatToken(SyntaxKind.SemicolonToken));
						if (propertyTypeInfo != null && propertyTypeInfo.IsObsolete)
						{
							node2 = AddError(node2, ErrorCode.WRN_PropertyIsObsolete, text);
						}
					}
					internalSyntaxListBuilder.Add(node2);
				}
			}
			return ContextAwareSyntaxFactory.PropertyList(internalSyntaxListBuilder.ToList());
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalSyntaxList<InternalTriggerDeclarationSyntax> ParseTriggerList(Func<string, TriggerTypeInfo> getTriggerInfo = null)
	{
		InternalSyntaxListBuilder<InternalTriggerDeclarationSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalTriggerDeclarationSyntax>();
		try
		{
			while (true)
			{
				SyntaxKind kind = CurrentToken.Kind;
				if (kind != SyntaxKind.TriggerKeyword)
				{
					break;
				}
				internalSyntaxListBuilder.Add(ParseTrigger(getTriggerInfo, default));
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalSyntaxList<InternalMemberSyntax> ParseMemberList(Func<string, TriggerTypeInfo> getTriggerInfo = null)
	{
		InternalSyntaxListBuilder<InternalMemberSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalMemberSyntax>();
		try
		{
			InternalSyntaxList<InternalMemberAttributeSyntax> attributeList = default;
			while (true)
			{
				switch (CurrentToken.Kind)
				{
				case SyntaxKind.VarKeyword:
					internalSyntaxListBuilder.Add(ParseVarSection(ref attributeList));
					continue;
				case SyntaxKind.OpenBracketToken:
					attributeList = ParseInLexerMode(LexerMode.Code, ParseMemberAttributeList);
					continue;
				case SyntaxKind.ProcedureKeyword:
				case SyntaxKind.LocalKeyword:
					internalSyntaxListBuilder.Add(ParseMethod(attributeList));
					attributeList = default;
					continue;
				case SyntaxKind.TriggerKeyword:
					internalSyntaxListBuilder.Add(ParseTriggerOrEventTrigger(getTriggerInfo, attributeList));
					attributeList = default;
					continue;
				case SyntaxKind.EventKeyword:
					internalSyntaxListBuilder.Add(ParseEvent(attributeList));
					attributeList = default;
					continue;
				}
				if (attributeList.Count > 0)
				{
					internalSyntaxListBuilder.Add(ParseMethod(attributeList));
				}
				return internalSyntaxListBuilder.ToList();
			}
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalVarSectionSyntax ParseVarSection(ref InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxKind.VarKeyword);
		if (attributeList.Count > 0)
		{
			internalSyntaxToken = AddLeadingSkippedSyntax(internalSyntaxToken, attributeList.Node);
			if (!attributeList.Node.ContainsDiagnostics)
			{
				internalSyntaxToken = AddError(internalSyntaxToken, ErrorCode.ERR_AttributeContext);
			}
			attributeList = default;
		}
		return ContextAwareSyntaxFactory.VarSection(internalSyntaxToken, ParseVariableList(ref attributeList));
	}

	private InternalSyntaxList<InternalVariableDeclarationSyntax> ParseVariableList(ref InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		InternalSyntaxListBuilder<InternalVariableDeclarationSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalVariableDeclarationSyntax>();
		try
		{
			while (CurrentToken.Kind.IsAllowedVariableName() || CurrentToken.Kind == SyntaxKind.OpenBracketToken)
			{
				if (CurrentToken.Kind == SyntaxKind.OpenBracketToken)
				{
					attributeList = ParseInLexerMode(LexerMode.Code, ParseMemberAttributeList);
				}
				if (CurrentToken.Kind.IsAllowedVariableName())
				{
					InternalVariableDeclarationSyntax previousNode = ParseVariable(attributeList);
					SkipBadVariableDefinitionTokens(ref previousNode);
					internalSyntaxListBuilder.Add(previousNode);
					attributeList = default;
				}
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalVariableDeclarationSyntax ParseVariable(InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.VariableDeclaration)
		{
			return (InternalVariableDeclarationSyntax)EatNode();
		}
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
		InternalTypeReferenceBaseSyntax type = ParseType();
		type = CheckTypeReferenceSyntax(type);
		return ContextAwareSyntaxFactory.VariableDeclaration(attributeList, name, colonToken, type, EatToken(SyntaxKind.SemicolonToken));
	}

	private InternalTypeReferenceBaseSyntax ParseType()
	{
		InternalArraySyntax array = ParseArray();
		InternalDataTypeSyntax internalDataTypeSyntax = ParseDatatype();
		switch ((internalDataTypeSyntax.TypeName != null) ? NavTypeExtensions.GetNavTypeKind(internalDataTypeSyntax.TypeName.ValueText, throwIfUnknown: false) : NavTypeKind.None)
		{
		case NavTypeKind.Record:
		{
			InternalSyntaxToken temporary = ((CurrentToken.Kind == SyntaxKind.TemporaryKeyword) ? EatToken() : null);
			return ContextAwareSyntaxFactory.RecordTypeReference(array, internalDataTypeSyntax, temporary);
		}
		case NavTypeKind.DotNet:
			return ContextAwareSyntaxFactory.DotNetTypeReference(array, internalDataTypeSyntax);
		default:
			return ContextAwareSyntaxFactory.SimpleTypeReference(array, internalDataTypeSyntax);
		}
	}

	private InternalArraySyntax ParseArray()
	{
		if (CurrentToken.Kind != SyntaxKind.ArrayKeyword)
		{
			return null;
		}
		InternalSyntaxToken arrayKeyword = EatToken(SyntaxKind.ArrayKeyword);
		InternalBracketedDimensionListSyntax internalBracketedDimensionListSyntax = ((CurrentToken.Kind == SyntaxKind.OpenBracketToken) ? ParseBracketedDimensionList() : ContextAwareSyntaxFactory.BracketedDimensionList(EatToken(SyntaxKind.OpenBracketToken), default, EatToken(SyntaxKind.CloseBracketToken)));
		if (internalBracketedDimensionListSyntax.Dimensions.Count == 0)
		{
			internalBracketedDimensionListSyntax = AddError(internalBracketedDimensionListSyntax, ErrorCode.ERR_ArrayMustHaveAtLeastOneDimension);
		}
		else if (internalBracketedDimensionListSyntax.Dimensions.Count > 10)
		{
			internalBracketedDimensionListSyntax = AddError(internalBracketedDimensionListSyntax, ErrorCode.ERR_ArrayTooManyDimensions, 10);
		}
		return ContextAwareSyntaxFactory.Array(arrayKeyword, internalBracketedDimensionListSyntax, EatToken(SyntaxKind.OfKeyword));
	}

	private InternalBracketedDimensionListSyntax ParseBracketedDimensionList()
	{
		InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenBracketToken);
		InternalSeparatedSyntaxList<InternalDimensionSyntax> dimensions = ParseSeparatedList(ref startToken, SyntaxKind.Int32LiteralToken, (InternalSyntaxToken token) => token.Kind == SyntaxKind.Int32LiteralToken, SyntaxKind.CommaToken, SyntaxKind.CloseBracketToken, SkipBadArrayDimensionSpecifierTokens, ParseDimension);
		InternalSyntaxToken closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
		return ContextAwareSyntaxFactory.BracketedDimensionList(startToken, dimensions, closeBracketToken);
	}

	private PostSkipAction SkipBadArrayDimensionSpecifierTokens(ref InternalSyntaxToken openBracket, InternalSeparatedSyntaxListBuilder<InternalDimensionSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref openBracket, list, (ParserBase p) => p.CurrentToken.Kind != expected, (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.CloseBracketToken || p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.IsTerminator(), expected);
	}

	private InternalDimensionSyntax ParseDimension()
	{
		if (CurrentToken.Kind != SyntaxKind.CloseBracketToken)
		{
			return ContextAwareSyntaxFactory.Dimension(EatToken(SyntaxKind.Int32LiteralToken));
		}
		return ContextAwareSyntaxFactory.Dimension(CreateMissingToken(SyntaxKind.Int32LiteralToken, CurrentToken.Kind, reportError: true));
	}

	private InternalDataTypeSyntax ParseDatatype()
	{
		if (SyntaxFacts.IsOptionType(CurrentToken))
		{
			return ParseOptionDataType();
		}
		InternalSyntaxToken internalSyntaxToken = (CurrentToken.Kind.IsAllowedVariableType() ? EatToken() : EatToken(SyntaxKind.IdentifierToken));
		if (internalSyntaxToken.IsMissing)
		{
			return ContextAwareSyntaxFactory.SimpleNamedDataType(internalSyntaxToken);
		}
		NavTypeKind navTypeKind = NavTypeExtensions.GetNavTypeKind(internalSyntaxToken.ValueText, throwIfUnknown: false);
		switch (navTypeKind)
		{
		case NavTypeKind.TextConst:
			return ParseTextConstDataType(internalSyntaxToken);
		case NavTypeKind.Label:
			return ParseLabelDataType(internalSyntaxToken);
		case NavTypeKind.Enum:
			return ParseEnumDataType(internalSyntaxToken);
		default:
			if (navTypeKind.HasLength())
			{
				InternalSyntaxToken openBracketToken = null;
				InternalSyntaxToken length = null;
				InternalSyntaxToken closeBracketToken = null;
				if (navTypeKind.LengthRequired() || CurrentToken.Kind == SyntaxKind.OpenBracketToken)
				{
					openBracketToken = EatToken(SyntaxKind.OpenBracketToken);
					length = EatToken(SyntaxKind.Int32LiteralToken);
					closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
				}
				return ContextAwareSyntaxFactory.LengthDataType(internalSyntaxToken, openBracketToken, length, closeBracketToken);
			}
			if (navTypeKind.HasSubType())
			{
				InternalObjectNameOrIdSyntax subtype = ParseObjectReferenceSyntax();
				return ContextAwareSyntaxFactory.SubtypedDataType(internalSyntaxToken, subtype);
			}
			if (navTypeKind.IsGenericType())
			{
				InternalSyntaxToken ofKeyword = EatToken(SyntaxKind.OfKeyword);
				InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenBracketToken);
				InternalSeparatedSyntaxList<InternalDataTypeSyntax> typeArguments = ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, SyntaxKind.CloseBracketToken, SkipBadCommaSeparatedToken, ParseDatatype);
				InternalSyntaxToken closeBracketToken2 = EatToken(SyntaxKind.CloseBracketToken);
				return ContextAwareSyntaxFactory.GenericNamedDataType(internalSyntaxToken, ofKeyword, startToken, typeArguments, closeBracketToken2);
			}
			return ContextAwareSyntaxFactory.SimpleNamedDataType(internalSyntaxToken);
		}
	}

	private InternalDataTypeSyntax ParseOptionDataType(bool skipParsingOptionValues = false)
	{
		InternalSyntaxToken startToken = EatToken(SyntaxKind.IdentifierToken);
		DebugAssertHelper.Assert(SyntaxFacts.IsOptionType(startToken));
		InternalOptionValuesSyntax optionValues = null;
		if (!skipParsingOptionValues)
		{
			optionValues = ParseOptionMembersSyntax(ref startToken);
		}
		return ContextAwareSyntaxFactory.OptionDataType(startToken, optionValues);
	}

	private InternalOptionValuesSyntax ParseOptionMembersSyntax(ref InternalSyntaxToken startToken)
	{
		InternalSeparatedSyntaxList<InternalIdentifierNameOrEmptySyntax> options = ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsTokenIdentifier() || token.Kind == SyntaxKind.CommaToken, SyntaxKind.CommaToken, SyntaxKind.SemicolonToken, SkipBadCommaSeparatedToken, ParseIdentifierNameOrEmptySyntax, shouldParseMemberIfLastElementIsSeparator: true);
		return ContextAwareSyntaxFactory.OptionValues(options);
	}

	private InternalDataTypeSyntax ParseTextConstDataType(InternalSyntaxToken textConstType)
	{
		InternalCommaSeparatedIdentifierEqualsStringListSyntax multilanguage = ParseCommaSeparatedIdentifierEqualsStringList(ref textConstType);
		return ContextAwareSyntaxFactory.TextConstDataType(textConstType, multilanguage);
	}

	private InternalDataTypeSyntax ParseLabelDataType(InternalSyntaxToken labelType)
	{
		return ContextAwareSyntaxFactory.LabelDataType(labelType, ParseLabel());
	}

	private InternalDataTypeSyntax ParseEnumDataType(InternalSyntaxToken enumType)
	{
		InternalEnumDataTypeSyntax node = ContextAwareSyntaxFactory.EnumDataType(enumType, ParseIdentifierName());
		return CheckFeatureAvailability(node, Feature.Enum);
	}

	private InternalLabelSyntax ParseLabel()
	{
		InternalStringLiteralValueSyntax labelText = ParseStringLiteralValue();
		if (CurrentToken.IsKind(SyntaxKind.CommaToken))
		{
			InternalSyntaxToken startToken = EatToken(SyntaxKind.CommaToken);
			InternalCommaSeparatedIdentifierEqualsLiteralListSyntax properties = ParseCommaSeparatedIdentifierEqualsLiteralList(ref startToken);
			return ContextAwareSyntaxFactory.Label(labelText, startToken, properties);
		}
		return ContextAwareSyntaxFactory.Label(labelText, null, null);
	}

	private InternalCommaSeparatedIdentifierEqualsLiteralListSyntax ParseCommaSeparatedIdentifierEqualsLiteralList(ref InternalSyntaxToken startToken, SyntaxKind closeTokenKind = SyntaxKind.SemicolonToken)
	{
		InternalSeparatedSyntaxList<InternalIdentifierEqualsLiteralSyntax> values = ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, closeTokenKind, SkipBadCommaSeparatedToken, ParseIdentifierEqualsLiteral);
		return ContextAwareSyntaxFactory.CommaSeparatedIdentifierEqualsLiteralList(values);
	}

	private InternalIdentifierEqualsStringSyntax ParseIdentifierEqualsString()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.IdentifierEqualsString)
		{
			return (InternalIdentifierEqualsStringSyntax)EatNode();
		}
		InternalSyntaxToken identifier = ParseIdentifierToken();
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalStringLiteralValueSyntax stringLiteral = ParseStringLiteralValue();
		return ContextAwareSyntaxFactory.IdentifierEqualsString(identifier, equalsToken, stringLiteral);
	}

	private InternalIdentifierEqualsLiteralSyntax ParseIdentifierEqualsLiteral()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.IdentifierEqualsLiteral)
		{
			return (InternalIdentifierEqualsLiteralSyntax)EatNode();
		}
		InternalSyntaxToken identifier = ParseIdentifierToken();
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalLiteralValueSyntax literal = ParseLiteralValue();
		return ContextAwareSyntaxFactory.IdentifierEqualsLiteral(identifier, equalsToken, literal);
	}

	private InternalTriggerDeclarationSyntax ParseTrigger(Func<string, TriggerTypeInfo> getTriggerInfo, InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.TriggerDeclaration)
		{
			return (InternalTriggerDeclarationSyntax)EatNode();
		}
		InternalSyntaxToken triggerKeyword = EatToken(SyntaxKind.TriggerKeyword);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		return (InternalTriggerDeclarationSyntax)ParseTrigger(attributeList, triggerKeyword, name, getTriggerInfo);
	}

	private InternalMethodOrTriggerDeclarationSyntax ParseTriggerOrEventTrigger(Func<string, TriggerTypeInfo> getTriggerInfo, InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		if (IsIncremental && (CurrentNodeKind == SyntaxKind.TriggerDeclaration || CurrentNodeKind == SyntaxKind.EventTriggerDeclaration))
		{
			return (InternalMethodOrTriggerDeclarationSyntax)EatNode();
		}
		InternalSyntaxToken triggerKeyword = EatToken(SyntaxKind.TriggerKeyword);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		if (CurrentToken.Kind == SyntaxKind.ColonColonToken)
		{
			return ParseEventTrigger(attributeList, triggerKeyword, name);
		}
		return ParseTrigger(attributeList, triggerKeyword, name, getTriggerInfo);
	}

	private InternalMethodOrTriggerDeclarationSyntax ParseEventTrigger(InternalSyntaxList<InternalMemberAttributeSyntax> attributeList, InternalSyntaxToken triggerKeyword, InternalIdentifierNameSyntax name)
	{
		InternalSyntaxToken colonColonToken = EatToken(SyntaxKind.ColonColonToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalParameterListSyntax parameterList = ParseParameterList();
		InternalReturnValueSyntax returnValue = ParseReturnValue();
		InternalSyntaxToken semicolonToken = EatTokenIfKind(SyntaxKind.SemicolonToken);
		InternalVarSectionSyntax variables = ParseVarSectionAndSkipOrphanedAttributes();
		InternalBlockSyntax body = ParseInLexerMode(LexerMode.Code, () => ParseBlock(semicolonRequired: true));
		return ContextAwareSyntaxFactory.EventTriggerDeclaration(attributeList, triggerKeyword, name, colonColonToken, name2, parameterList, returnValue, semicolonToken, variables, body);
	}

	private InternalMethodOrTriggerDeclarationSyntax ParseTrigger(InternalSyntaxList<InternalMemberAttributeSyntax> attributeList, InternalSyntaxToken triggerKeyword, InternalIdentifierNameSyntax name, Func<string, TriggerTypeInfo> getTriggerInfo)
	{
		TriggerTypeInfo triggerTypeInfo = getTriggerInfo?.Invoke(name.Identifier.ValueText);
		if (triggerTypeInfo == null && getTriggerInfo != null)
		{
			name = AddError(name, ErrorCode.ERR_UnknownTrigger, name.Identifier.ValueText);
		}
		InternalParameterListSyntax parameterList = ParseParameterList();
		InternalReturnValueSyntax returnValue = ParseReturnValue();
		InternalSyntaxToken semicolonToken = EatTokenIfKind(SyntaxKind.SemicolonToken);
		InternalVarSectionSyntax variables = ParseVarSectionAndSkipOrphanedAttributes();
		InternalBlockSyntax body = ParseInLexerMode(LexerMode.Code, () => ParseBlock(semicolonRequired: true));
		InternalTriggerDeclarationSyntax previousNode = ContextAwareSyntaxFactory.TriggerDeclaration(attributeList, triggerKeyword, name, parameterList, returnValue, semicolonToken, variables, body);
		if (triggerTypeInfo != null)
		{
			previousNode = previousNode.WithAnnotations(new SyntaxAnnotation[1] { triggerTypeInfo.TriggerAnnotation });
		}
		SkipBadMethodDefinitionTokens(ref previousNode);
		return previousNode;
	}

	private InternalMethodDeclarationSyntax ParseMethod(InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.MethodDeclaration)
		{
			return (InternalMethodDeclarationSyntax)EatNode();
		}
		InternalSyntaxToken localKeyword = ((CurrentToken.Kind == SyntaxKind.LocalKeyword) ? EatToken() : null);
		InternalSyntaxToken procedureKeyword = EatToken(SyntaxKind.ProcedureKeyword);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalParameterListSyntax parameterList = ParseParameterList();
		InternalReturnValueSyntax returnValue = ParseReturnValue();
		InternalSyntaxToken semicolonToken = EatTokenIfKind(SyntaxKind.SemicolonToken);
		InternalSyntaxNode internalSyntaxNode = ParseVarSectionAndSkipOrphanedAttributes();
		InternalBlockSyntax body = ((CurrentToken.Kind == SyntaxKind.BeginKeyword) ? ParseInLexerMode(LexerMode.Code, () => ParseBlock(semicolonRequired: true)) : null);
		InternalMethodDeclarationSyntax previousNode = ContextAwareSyntaxFactory.MethodDeclaration(attributeList, localKeyword, procedureKeyword, name, parameterList, returnValue, semicolonToken, (InternalVarSectionSyntax)internalSyntaxNode, body);
		SkipBadMethodDefinitionTokens(ref previousNode);
		return previousNode;
	}

	private InternalEventDeclarationSyntax ParseEvent(InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.EventDeclaration)
		{
			return (InternalEventDeclarationSyntax)EatNode();
		}
		InternalSyntaxToken eventKeyword = EatToken(SyntaxKind.EventKeyword);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalParameterListSyntax parameterList = ParseParameterList();
		InternalSyntaxToken semicolonToken = EatTokenIfKind(SyntaxKind.SemicolonToken);
		InternalEventDeclarationSyntax previousNode = ContextAwareSyntaxFactory.EventDeclaration(attributeList, eventKeyword, name, parameterList, semicolonToken);
		SkipBadMethodDefinitionTokens(ref previousNode);
		return previousNode;
	}

	private InternalVarSectionSyntax ParseVarSectionAndSkipOrphanedAttributes()
	{
		InternalSyntaxNode lastGoodNode = null;
		if (CurrentToken.Kind == SyntaxKind.VarKeyword)
		{
			InternalSyntaxList<InternalMemberAttributeSyntax> attributeList = default;
			lastGoodNode = ParseVarSection(ref attributeList);
			SkipOrphanedAttributes(ref lastGoodNode, attributeList);
		}
		return (InternalVarSectionSyntax)lastGoodNode;
	}

	private InternalParameterListSyntax ParseParameterList()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.ParameterList)
		{
			return (InternalParameterListSyntax)EatNode();
		}
		InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSeparatedSyntaxList<InternalParameterSyntax> parameters = ParseParameters(ref startToken);
		return ContextAwareSyntaxFactory.ParameterList(startToken, parameters, EatToken(SyntaxKind.CloseParenToken));
	}

	private InternalSeparatedSyntaxList<InternalParameterSyntax> ParseParameters(ref InternalSyntaxToken startToken)
	{
		return ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.IsPossibleParameter(), SyntaxKind.SemicolonToken, SyntaxKind.CloseParenToken, SkipBadParameterTokens, ParseParameter);
	}

	private InternalParameterSyntax ParseParameter()
	{
		InternalSyntaxToken varKeyword = ((CurrentToken.Kind == SyntaxKind.VarKeyword) ? EatToken() : null);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
		InternalTypeReferenceBaseSyntax type = ParseType();
		type = CheckTypeReferenceSyntax(type);
		return ContextAwareSyntaxFactory.Parameter(varKeyword, name, colonToken, type);
	}

	private InternalReturnValueSyntax ParseReturnValue()
	{
		if (!CurrentToken.Kind.IsAllowedVariableName() && CurrentToken.Kind != SyntaxKind.ColonToken)
		{
			return null;
		}
		InternalIdentifierNameSyntax name = ((CurrentToken.Kind == SyntaxKind.ColonToken) ? null : ParseIdentifierName());
		InternalSyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
		InternalDataTypeSyntax dataType = ParseDatatype();
		return ContextAwareSyntaxFactory.ReturnValue(name, colonToken, dataType);
	}

	private PostSkipAction SkipBadParameterTokens(ref InternalSyntaxToken openParen, InternalSeparatedSyntaxListBuilder<InternalParameterSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref openParen, list, (ParserBase p) => (expected == SyntaxKind.SemicolonToken && p.CurrentToken.Kind != SyntaxKind.SemicolonToken) || (expected != SyntaxKind.SemicolonToken && !CurrentToken.IsPossibleParameter()), (ParserBase p) => p.CurrentToken.Kind.IsPossibleMember() || p.IsTerminator() || p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken || p.CurrentToken.Kind == SyntaxKind.BeginKeyword, expected);
	}

	private void SkipBadApplicationObjectTokens(ref InternalSyntaxListBuilder<InternalObjectSyntax> objects, ref InternalSyntaxListBuilder initialBadNodes)
	{
		InternalSyntaxListBuilder<InternalSyntaxToken> internalSyntaxListBuilder = Pool.Allocate<InternalSyntaxToken>();
		try
		{
			while (!IsEndOfFile && !CurrentToken.Kind.IsObjectKeyword())
			{
				InternalSyntaxToken node = EatToken();
				if (internalSyntaxListBuilder.Count == 0)
				{
					node = AddError(node, ErrorCode.ERR_ExpectedApplicationObjectKeyword, SyntaxFacts.SupportedApplicationObjectsString);
				}
				internalSyntaxListBuilder.Add(node);
			}
			if (objects.Count > 0)
			{
				objects[objects.Count - 1] = AddTrailingSkippedSyntax(objects[objects.Count - 1], internalSyntaxListBuilder.ToListNode());
				return;
			}
			initialBadNodes = new InternalSyntaxListBuilder(internalSyntaxListBuilder.Count);
			initialBadNodes.AddRange(internalSyntaxListBuilder.ToList());
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private void SkipBadMethodDefinitionTokens<T>(ref T previousNode) where T : InternalMemberSyntax
	{
		Func<ParserBase, InternalSyntaxToken, InternalSyntaxToken> errorFunction = null;
		if (!previousNode.ContainsDiagnostics)
		{
			errorFunction = (ParserBase p, InternalSyntaxToken token) => p.AddError(token, ErrorCode.ERR_InvalidValueInContext, token.ToFullString());
		}
		SkipBadTokens((ParserBase p) => p.IsEndOfFile || p.CurrentToken.Kind.IsAllowedVariableName() || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken || p.CurrentToken.Kind == SyntaxKind.VarKeyword || p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.CurrentToken.Kind.IsMemberStart() || p.CurrentToken.Kind.IsObjectKeyword(), errorFunction, out var trailingTrivia);
		if (trailingTrivia != null)
		{
			previousNode = AddTrailingSkippedSyntax(previousNode, trailingTrivia);
		}
	}

	private void SkipBadVariableDefinitionTokens(ref InternalVariableDeclarationSyntax previousNode)
	{
		Func<ParserBase, InternalSyntaxToken, InternalSyntaxToken> errorFunction = null;
		if (!previousNode.ContainsDiagnostics)
		{
			errorFunction = (ParserBase p, InternalSyntaxToken token) => p.AddError(token, ErrorCode.ERR_InvalidValueInContext, token.ToFullString());
		}
		SkipBadTokens((ParserBase p) => p.IsEndOfFile || p.CurrentToken.Kind.IsAllowedVariableName() || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken || p.CurrentToken.Kind == SyntaxKind.VarKeyword || p.CurrentToken.Kind == SyntaxKind.BeginKeyword || p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.CurrentToken.Kind.IsMemberStart(), errorFunction, out var trailingTrivia);
		if (trailingTrivia != null)
		{
			previousNode = AddTrailingSkippedSyntax(previousNode, trailingTrivia);
		}
	}

	private void SkipBadPropertyValue<TNode>(ref TNode previousToken) where TNode : InternalSyntaxNode
	{
		InternalSyntaxListBuilder<InternalSyntaxToken> internalSyntaxListBuilder = Pool.Allocate<InternalSyntaxToken>();
		try
		{
			while (!IsEndOfFile && CurrentToken.Kind != SyntaxKind.SemicolonToken && CurrentToken.Kind != SyntaxKind.CloseBraceToken && !CurrentToken.Kind.IsMemberStart())
			{
				InternalSyntaxToken node = EatToken();
				internalSyntaxListBuilder.Add(node);
			}
			if (internalSyntaxListBuilder.Count > 0)
			{
				previousToken = AddTrailingSkippedSyntax(previousToken, internalSyntaxListBuilder.ToListNode());
			}
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private T ParseInLexerMode<T>(LexerMode mode, Func<T> func)
	{
		LexerMode lexerMode = LexerMode;
		try
		{
			LexerMode = mode;
			return func();
		}
		finally
		{
			LexerMode = lexerMode;
		}
	}

	private InternalObjectNameOrIdSyntax ParseObjectReferenceSyntax()
	{
		if (CurrentToken.Kind == SyntaxKind.Int32LiteralToken)
		{
			return ContextAwareSyntaxFactory.ObjectNameOrId(ParseObjectIdSyntax());
		}
		return ContextAwareSyntaxFactory.ObjectNameOrId(ParseIdentifierName());
	}

	private InternalObjectIdSyntax ParseObjectIdSyntax()
	{
		return ContextAwareSyntaxFactory.ObjectId(EatToken(SyntaxKind.Int32LiteralToken));
	}

	private InternalObjectNameReferenceSyntax ParseObjectNameReferenceSyntax()
	{
		return ContextAwareSyntaxFactory.ObjectNameReference(ParseIdentifierName());
	}

	private InternalTypeReferenceBaseSyntax CheckTypeReferenceSyntax(InternalTypeReferenceBaseSyntax type)
	{
		switch (type.Kind)
		{
		case SyntaxKind.DotNetTypeReference:
			type = CheckFeatureAvailability(type, Feature.DotNet);
			break;
		case SyntaxKind.SimpleTypeReference:
			if (type.DataType.Kind == SyntaxKind.EnumDataType)
			{
				type = CheckFeatureAvailability(type, Feature.Enum);
			}
			break;
		}
		return type;
	}
}
