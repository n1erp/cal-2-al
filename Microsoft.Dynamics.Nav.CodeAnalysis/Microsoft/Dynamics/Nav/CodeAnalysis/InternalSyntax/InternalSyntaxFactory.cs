using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal static class InternalSyntaxFactory
{
	private static readonly InternalSyntaxTrivia space;

	private static readonly InternalSyntaxTrivia carriageReturnLineFeed;

	private static readonly InternalSyntaxTrivia lineFeed;

	private static readonly InternalSyntaxTrivia carriageReturn;

	private static readonly InternalSyntaxTrivia elasticSpace;

	private static readonly InternalSyntaxTrivia elasticCarriageReturnLineFeed;

	private static readonly InternalSyntaxTrivia elasticLineFeed;

	private static readonly InternalSyntaxTrivia elasticCarriageReturn;

	internal static readonly InternalSyntaxTrivia ElasticZeroSpace;

	internal static readonly InternalSyntaxToken BeginKeyword;

	internal static readonly InternalSyntaxToken EndKeyword;

	internal static readonly InternalSyntaxToken IfKeyword;

	internal static readonly InternalSyntaxToken ThenKeyword;

	internal static readonly InternalSyntaxToken ElseKeyword;

	internal static readonly InternalSyntaxToken WhileKeyword;

	internal static readonly InternalSyntaxToken DoKeyword;

	internal static readonly InternalSyntaxToken RepeatKeyword;

	internal static readonly InternalSyntaxToken UntilKeyword;

	internal static readonly InternalSyntaxToken WithKeyword;

	internal static readonly InternalSyntaxToken CaseKeyword;

	internal static readonly InternalSyntaxToken OfKeyword;

	internal static readonly InternalSyntaxToken ForeachKeyword;

	internal static readonly InternalSyntaxToken InKeyword;

	internal static readonly InternalSyntaxToken AssertErrorKeyword;

	internal static readonly InternalSyntaxToken ForKeyword;

	internal static readonly InternalSyntaxToken ExitKeyword;

	internal static readonly InternalSyntaxToken VarKeyword;

	internal static readonly InternalSyntaxToken TriggerKeyword;

	internal static readonly InternalSyntaxToken ArrayKeyword;

	internal static readonly InternalSyntaxToken TemporaryKeyword;

	internal static readonly InternalSyntaxToken LocalKeyword;

	internal static readonly InternalSyntaxToken ProcedureKeyword;

	internal static readonly InternalSyntaxToken FieldsKeyword;

	internal static readonly InternalSyntaxToken FieldKeyword;

	internal static readonly InternalSyntaxToken AssemblyKeyword;

	internal static readonly InternalSyntaxToken TypeKeyword;

	internal static readonly InternalSyntaxToken CodeunitKeyword;

	internal static readonly InternalSyntaxToken DotNetKeyword;

	internal static readonly InternalSyntaxToken TableKeyword;

	internal static readonly InternalSyntaxToken OpenParenToken;

	internal static readonly InternalSyntaxToken CloseParenToken;

	internal static readonly InternalSyntaxToken OpenBracketToken;

	internal static readonly InternalSyntaxToken CloseBracketToken;

	internal static readonly InternalSyntaxToken ColonToken;

	internal static readonly InternalSyntaxToken ColonColonToken;

	internal static readonly InternalSyntaxToken DotToken;

	internal static readonly InternalSyntaxToken SemicolonToken;

	private static readonly ArrayElement<InternalSyntaxTrivia>[] spaceCache;

	public static InternalSyntaxTrivia Space => space;

	public static InternalSyntaxTrivia CarriageReturnLineFeed => carriageReturnLineFeed;

	public static InternalSyntaxTrivia CarriageReturn => carriageReturn;

	public static InternalSyntaxTrivia LineFeed => lineFeed;

	public static InternalSyntaxTrivia ElasticSpace => elasticSpace;

	public static InternalSyntaxTrivia ElasticCarriageReturnLineFeed => elasticCarriageReturnLineFeed;

	public static InternalSyntaxTrivia ElasticCarriageReturn => elasticCarriageReturn;

	public static InternalSyntaxTrivia ElasticLineFeed => elasticLineFeed;

	public static InternalObjectCompilationUnitSyntax ObjectCompilationUnit(InternalSyntaxList<InternalObjectSyntax> objects, InternalSyntaxToken endOfFileToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(227, objects.Node, endOfFileToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalObjectCompilationUnitSyntax)internalSyntaxNode;
		}
		InternalObjectCompilationUnitSyntax internalObjectCompilationUnitSyntax = new InternalObjectCompilationUnitSyntax(SyntaxKind.CompilationUnit, objects.Node, endOfFileToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalObjectCompilationUnitSyntax, hash);
		}
		return internalObjectCompilationUnitSyntax;
	}

	public static InternalPropertyListSyntax PropertyList(InternalSyntaxList<InternalPropertySyntaxOrEmpty> properties)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(228, properties.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalPropertyListSyntax)internalSyntaxNode;
		}
		InternalPropertyListSyntax internalPropertyListSyntax = new InternalPropertyListSyntax(SyntaxKind.PropertyList, properties.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalPropertyListSyntax, hash);
		}
		return internalPropertyListSyntax;
	}

	public static InternalProfileSyntax Profile(InternalSyntaxToken profileToken, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalProfileSyntax(SyntaxKind.ProfileObject, profileToken, name, openBraceToken, propertyList, members.Node, closeBraceToken);
	}

	public static InternalEnumTypeSyntax EnumType(InternalSyntaxToken enumToken, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalEnumValueSyntax> values, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalEnumTypeSyntax(SyntaxKind.EnumType, enumToken, objectId, name, openBraceToken, propertyList, values.Node, members.Node, closeBraceToken);
	}

	public static InternalEnumValueSyntax EnumValue(InternalSyntaxToken enumValueToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalEnumValueSyntax(SyntaxKind.EnumValue, enumValueToken, openParenthesisToken, id, semicolonToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalEnumExtensionTypeSyntax EnumExtensionType(InternalSyntaxToken enumExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalEnumValueSyntax> values, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalEnumExtensionTypeSyntax(SyntaxKind.EnumExtensionType, enumExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, values.Node, members.Node, closeBraceToken);
	}

	public static InternalDotNetPackageSyntax DotNetPackage(InternalSyntaxToken dotNetKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalDotNetAssemblySyntax> assemblies, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalDotNetPackageSyntax(SyntaxKind.DotNetPackage, dotNetKeyword, name, openBraceToken, propertyList, assemblies.Node, members.Node, closeBraceToken);
	}

	public static InternalDotNetAssemblySyntax DotNetAssembly(InternalSyntaxToken assemblyKeyword, InternalSyntaxToken openParenthesisToken, InternalNameSyntax assemblyName, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalDotNetTypeDeclarationSyntax> typeDeclarations, InternalSyntaxToken closeBraceToken)
	{
		return new InternalDotNetAssemblySyntax(SyntaxKind.DotNetAssembly, assemblyKeyword, openParenthesisToken, assemblyName, closeParenthesisToken, openBraceToken, propertyList, typeDeclarations.Node, closeBraceToken);
	}

	public static InternalDotNetTypeDeclarationSyntax DotNetTypeDeclaration(InternalSyntaxToken type, InternalSyntaxToken openParenthesisToken, InternalNameSyntax typeName, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax typeAlias, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalDotNetTypeDeclarationSyntax(SyntaxKind.DotNetTypeDeclaration, type, openParenthesisToken, typeName, semicolonToken, typeAlias, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalCodeunitSyntax Codeunit(InternalSyntaxToken codeunitKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalCodeunitSyntax(SyntaxKind.CodeunitObject, codeunitKeyword, objectId, name, openBraceToken, propertyList, members.Node, closeBraceToken);
	}

	public static InternalTableSyntax Table(InternalSyntaxToken tableKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalFieldListSyntax fields, InternalKeyListSyntax keys, InternalFieldGroupListSyntax fieldGroups, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalTableSyntax(SyntaxKind.TableObject, tableKeyword, objectId, name, openBraceToken, propertyList, fields, keys, fieldGroups, members.Node, closeBraceToken);
	}

	public static InternalFieldListSyntax FieldList(InternalSyntaxToken fieldsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalFieldSyntax> fields, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldListSyntax(SyntaxKind.FieldList, fieldsKeyword, openBraceToken, fields.Node, closeBraceToken);
	}

	public static InternalFieldExtensionListSyntax FieldExtensionList(InternalSyntaxToken fieldsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalFieldBaseSyntax> fields, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldExtensionListSyntax(SyntaxKind.FieldExtensionList, fieldsKeyword, openBraceToken, fields.Node, closeBraceToken);
	}

	public static InternalFieldSyntax Field(InternalSyntaxToken fieldKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken no, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalDataTypeSyntax type, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldSyntax(SyntaxKind.Field, fieldKeyword, openParenthesisToken, no, semicolonToken1, name, semicolonToken2, type, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken);
	}

	public static InternalFieldModificationSyntax FieldModification(InternalSyntaxToken fieldKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldModificationSyntax(SyntaxKind.FieldModification, fieldKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken);
	}

	public static InternalKeyListSyntax KeyList(InternalSyntaxToken keysKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalKeySyntax> keys, InternalSyntaxToken closeBraceToken)
	{
		return new InternalKeyListSyntax(SyntaxKind.KeyList, keysKeyword, openBraceToken, keys.Node, closeBraceToken);
	}

	public static InternalKeySyntax Key(InternalSyntaxToken keyKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalKeySyntax(SyntaxKind.Key, keyKeyword, openParenthesisToken, id, semicolonToken1, name, semicolonToken2, fields.Node, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalFieldGroupListSyntax FieldGroupList(InternalSyntaxToken fieldGroupsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalFieldGroupSyntax> fieldGroups, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldGroupListSyntax(SyntaxKind.FieldGroupList, fieldGroupsKeyword, openBraceToken, fieldGroups.Node, closeBraceToken);
	}

	public static InternalFieldGroupExtensionListSyntax FieldGroupExtensionList(InternalSyntaxToken fieldGroupsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalFieldGroupChangeBaseSyntax> changes, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldGroupExtensionListSyntax(SyntaxKind.FieldGroupExtensionList, fieldGroupsKeyword, openBraceToken, changes.Node, closeBraceToken);
	}

	public static InternalFieldGroupAddChangeSyntax FieldGroupAddChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken1, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldGroupAddChangeSyntax(SyntaxKind.FieldGroupAddChange, changeKeyword, openParenthesisToken, anchor, semicolonToken1, fields.Node, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalFieldGroupSyntax FieldGroup(InternalSyntaxToken fieldGroupKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldGroupSyntax(SyntaxKind.FieldGroup, fieldGroupKeyword, openParenthesisToken, id, semicolonToken1, name, semicolonToken2, fields.Node, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalPageSyntax Page(InternalSyntaxToken pageKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageLayoutSyntax layout, InternalPageActionListSyntax actions, InternalPageViewListSyntax views, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageSyntax(SyntaxKind.PageObject, pageKeyword, objectId, name, openBraceToken, propertyList, layout, actions, views, members.Node, closeBraceToken);
	}

	public static InternalPageLayoutSyntax PageLayout(InternalSyntaxToken layoutKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalPageAreaSyntax> areas, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageLayoutSyntax(SyntaxKind.PageLayout, layoutKeyword, openBraceToken, areas.Node, closeBraceToken);
	}

	public static InternalPageFieldSyntax PageField(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageFieldSyntax(SyntaxKind.PageField, controlKeyword, openParenthesisToken, name, semicolonToken, expression, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken);
	}

	public static InternalPageLabelSyntax PageLabel(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageLabelSyntax(SyntaxKind.PageLabel, controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalPageGroupSyntax PageGroup(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalControlBaseSyntax> controls, InternalGroupActionListSyntax actionList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageGroupSyntax(SyntaxKind.PageGroup, controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, controls.Node, actionList, closeBraceToken);
	}

	public static InternalPagePartSyntax PagePart(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax partName, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPagePartSyntax(SyntaxKind.PagePart, controlKeyword, openParenthesisToken, name, semicolonToken, partName, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalPageSystemPartSyntax PageSystemPart(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax systemPartType, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageSystemPartSyntax(SyntaxKind.PageSystemPart, controlKeyword, openParenthesisToken, name, semicolonToken, systemPartType, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalPageChartPartSyntax PageChartPart(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax chartPartType, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageChartPartSyntax(SyntaxKind.PageChartPart, controlKeyword, openParenthesisToken, name, semicolonToken, chartPartType, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalPageUserControlSyntax PageUserControl(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameReferenceSyntax controlAddIn, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageUserControlSyntax(SyntaxKind.PageUserControl, controlKeyword, openParenthesisToken, name, semicolonToken, controlAddIn, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken);
	}

	public static InternalPageAreaSyntax PageArea(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalControlBaseSyntax> controls, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageAreaSyntax(SyntaxKind.PageArea, controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, controls.Node, closeBraceToken);
	}

	public static InternalPageActionListSyntax PageActionList(InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalPageActionAreaSyntax> areas, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageActionListSyntax(SyntaxKind.PageActionList, actionsKeyword, openBraceToken, areas.Node, closeBraceToken);
	}

	public static InternalPageViewListSyntax PageViewList(InternalSyntaxToken viewsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalPageViewSyntax> views, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageViewListSyntax(SyntaxKind.PageViewList, viewsKeyword, openBraceToken, views.Node, closeBraceToken);
	}

	public static InternalPageViewSyntax PageView(InternalSyntaxToken viewKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageExtensionLayoutSyntax layout, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageViewSyntax(SyntaxKind.PageView, viewKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, layout, closeBraceToken);
	}

	public static InternalGroupActionListSyntax GroupActionList(InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalActionBaseSyntax> actions, InternalSyntaxToken closeBraceToken)
	{
		return new InternalGroupActionListSyntax(SyntaxKind.GroupActionList, actionsKeyword, openBraceToken, actions.Node, closeBraceToken);
	}

	public static InternalPageActionGroupSyntax PageActionGroup(InternalSyntaxToken actionKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalActionBaseSyntax> actions, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageActionGroupSyntax(SyntaxKind.PageActionGroup, actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, actions.Node, closeBraceToken);
	}

	public static InternalPageActionAreaSyntax PageActionArea(InternalSyntaxToken actionKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalActionBaseSyntax> actions, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageActionAreaSyntax(SyntaxKind.PageActionArea, actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, actions.Node, closeBraceToken);
	}

	public static InternalPageActionSyntax PageAction(InternalSyntaxToken actionKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageActionSyntax(SyntaxKind.PageAction, actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken);
	}

	public static InternalPageActionSeparatorSyntax PageActionSeparator(InternalSyntaxToken actionKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageActionSeparatorSyntax(SyntaxKind.PageActionSeparator, actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalPageExtensionSyntax PageExtension(InternalSyntaxToken pageExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageExtensionLayoutSyntax layout, InternalPageExtensionActionListSyntax actions, InternalPageExtensionViewListSyntax views, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageExtensionSyntax(SyntaxKind.PageExtensionObject, pageExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, members.Node, closeBraceToken);
	}

	public static InternalPageCustomizationSyntax PageCustomization(InternalSyntaxToken pageCustomizationKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken customizesKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageExtensionLayoutSyntax layout, InternalPageExtensionActionListSyntax actions, InternalPageExtensionViewListSyntax views, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageCustomizationSyntax(SyntaxKind.PageCustomizationObject, pageCustomizationKeyword, objectId, name, customizesKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, members.Node, closeBraceToken);
	}

	public static InternalTableExtensionSyntax TableExtension(InternalSyntaxToken tableExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalFieldExtensionListSyntax fields, InternalKeyListSyntax keys, InternalFieldGroupExtensionListSyntax fieldGroups, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalTableExtensionSyntax(SyntaxKind.TableExtensionObject, tableExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, fields, keys, fieldGroups, members.Node, closeBraceToken);
	}

	public static InternalPageExtensionLayoutSyntax PageExtensionLayout(InternalSyntaxToken layoutKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalControlChangeBaseSyntax> changes, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageExtensionLayoutSyntax(SyntaxKind.PageExtensionLayout, layoutKeyword, openBraceToken, changes.Node, closeBraceToken);
	}

	public static InternalControlAddChangeSyntax ControlAddChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalControlBaseSyntax> controls, InternalSyntaxToken closeBraceToken)
	{
		return new InternalControlAddChangeSyntax(SyntaxKind.ControlAddChange, changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, controls.Node, closeBraceToken);
	}

	public static InternalControlMoveChangeSyntax ControlMoveChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> controls, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalControlMoveChangeSyntax(SyntaxKind.ControlMoveChange, changeKeyword, openParenthesisToken, anchor, semicolonToken, controls.Node, closeParenthesisToken);
	}

	public static InternalControlModifyChangeSyntax ControlModifyChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalControlModifyChangeSyntax(SyntaxKind.ControlModifyChange, changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken);
	}

	public static InternalPageExtensionViewListSyntax PageExtensionViewList(InternalSyntaxToken viewsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalViewChangeBaseSyntax> changes, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageExtensionViewListSyntax(SyntaxKind.PageExtensionViewList, viewsKeyword, openBraceToken, changes.Node, closeBraceToken);
	}

	public static InternalViewAddChangeSyntax ViewAddChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalPageViewSyntax> views, InternalSyntaxToken closeBraceToken)
	{
		return new InternalViewAddChangeSyntax(SyntaxKind.ViewAddChange, changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, views.Node, closeBraceToken);
	}

	public static InternalViewMoveChangeSyntax ViewMoveChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> views, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalViewMoveChangeSyntax(SyntaxKind.ViewMoveChange, changeKeyword, openParenthesisToken, anchor, semicolonToken, views.Node, closeParenthesisToken);
	}

	public static InternalViewModifyChangeSyntax ViewModifyChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalViewModifyChangeSyntax(SyntaxKind.ViewModifyChange, changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalPageExtensionActionListSyntax PageExtensionActionList(InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalActionChangeBaseSyntax> changes, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageExtensionActionListSyntax(SyntaxKind.PageExtensionActionList, actionsKeyword, openBraceToken, changes.Node, closeBraceToken);
	}

	public static InternalActionAddChangeSyntax ActionAddChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalActionBaseSyntax> actions, InternalSyntaxToken closeBraceToken)
	{
		return new InternalActionAddChangeSyntax(SyntaxKind.ActionAddChange, changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, actions.Node, closeBraceToken);
	}

	public static InternalActionMoveChangeSyntax ActionMoveChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> actions, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalActionMoveChangeSyntax(SyntaxKind.ActionMoveChange, changeKeyword, openParenthesisToken, anchor, semicolonToken, actions.Node, closeParenthesisToken);
	}

	public static InternalActionModifyChangeSyntax ActionModifyChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalActionModifyChangeSyntax(SyntaxKind.ActionModifyChange, changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken);
	}

	public static InternalQuerySyntax Query(InternalSyntaxToken queryKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalQueryElementsSyntax elements, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalQuerySyntax(SyntaxKind.QueryObject, queryKeyword, objectId, name, openBraceToken, propertyList, elements, members.Node, closeBraceToken);
	}

	public static InternalQueryElementsSyntax QueryElements(InternalSyntaxToken queryElementsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalQueryDataItemSyntax> dataItems, InternalSyntaxToken closeBraceToken)
	{
		return new InternalQueryElementsSyntax(SyntaxKind.QueryElements, queryElementsKeyword, openBraceToken, dataItems.Node, closeBraceToken);
	}

	public static InternalQueryDataItemSyntax QueryDataItem(InternalSyntaxToken dataItemKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax dataItemTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalQueryDataItemElementSyntax> elements, InternalSyntaxToken closeBraceToken)
	{
		return new InternalQueryDataItemSyntax(SyntaxKind.QueryDataItem, dataItemKeyword, openParenthesisToken, name, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements.Node, closeBraceToken);
	}

	public static InternalQueryColumnSyntax QueryColumn(InternalSyntaxToken columnKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalQueryColumnSyntax(SyntaxKind.QueryColumn, columnKeyword, openParenthesisToken, name, semicolonToken, relatedField, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalQueryFilterSyntax QueryFilter(InternalSyntaxToken filterKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalQueryFilterSyntax(SyntaxKind.QueryFilter, filterKeyword, openParenthesisToken, name, semicolonToken, relatedField, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalReportSyntax Report(InternalSyntaxToken reportKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalReportDataSetSectionSyntax dataSet, InternalRequestPageSyntax requestPage, InternalReportLabelsSectionSyntax labels, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalReportSyntax(SyntaxKind.ReportObject, reportKeyword, objectId, name, openBraceToken, propertyList, dataSet, requestPage, labels, members.Node, closeBraceToken);
	}

	public static InternalReportDataSetSectionSyntax ReportDataSetSection(InternalSyntaxToken dataSetKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalReportDataItemSyntax> dataItems, InternalSyntaxToken closeBraceToken)
	{
		return new InternalReportDataSetSectionSyntax(SyntaxKind.ReportDataSetSection, dataSetKeyword, openBraceToken, dataItems.Node, closeBraceToken);
	}

	public static InternalReportDataItemSyntax ReportDataItem(InternalSyntaxToken dataItemKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax dataItemTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalReportDataItemElementSyntax> elements, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalReportDataItemSyntax(SyntaxKind.ReportDataItem, dataItemKeyword, openParenthesisToken, name, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements.Node, triggers.Node, closeBraceToken);
	}

	public static InternalReportColumnSyntax ReportColumn(InternalSyntaxToken columnKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceExpression, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalReportColumnSyntax(SyntaxKind.ReportColumn, columnKeyword, openParenthesisToken, name, semicolonToken, sourceExpression, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public static InternalReportLabelsSectionSyntax ReportLabelsSection(InternalSyntaxToken labelsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalReportLabelBaseSyntax> labels, InternalSyntaxToken closeBraceToken)
	{
		return new InternalReportLabelsSectionSyntax(SyntaxKind.ReportLabelsSection, labelsKeyword, openBraceToken, labels.Node, closeBraceToken);
	}

	public static InternalReportLabelMultilanguageSyntax ReportLabelMultilanguage(InternalSyntaxToken labelKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCommaSeparatedIdentifierEqualsStringListSyntax caption, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalReportLabelMultilanguageSyntax(SyntaxKind.ReportLabelMultilanguage, labelKeyword, openParenthesisToken, name, semicolonToken, caption, closeParenthesisToken);
	}

	public static InternalReportLabelSyntax ReportLabel(InternalIdentifierNameSyntax name, InternalSyntaxToken equalsToken, InternalLabelSyntax label, InternalSyntaxToken semicolonToken)
	{
		return new InternalReportLabelSyntax(SyntaxKind.ReportLabel, name, equalsToken, label, semicolonToken);
	}

	public static InternalRequestPageSyntax RequestPage(InternalSyntaxToken requestPageKeyword, InternalSyntaxToken openBraceToken, InternalIdentifierNameSyntax name, InternalPropertyListSyntax propertyList, InternalPageLayoutSyntax layout, InternalPageActionListSyntax actions, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalRequestPageSyntax(SyntaxKind.RequestPage, requestPageKeyword, openBraceToken, name, propertyList, layout, actions, members.Node, closeBraceToken);
	}

	public static InternalXmlPortSyntax XmlPort(InternalSyntaxToken xmlPortKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalXmlPortSchemaSyntax xmlPortSchema, InternalRequestPageSyntax xmlPortRequestPage, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortSyntax(SyntaxKind.XmlPortObject, xmlPortKeyword, objectId, name, openBraceToken, propertyList, xmlPortSchema, xmlPortRequestPage, members.Node, closeBraceToken);
	}

	public static InternalXmlPortSchemaSyntax XmlPortSchema(InternalSyntaxToken xmlPortSchemaKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalXmlPortNodeSyntax> xmlPortSchema, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortSchemaSyntax(SyntaxKind.XmlPortSchema, xmlPortSchemaKeyword, openBraceToken, xmlPortSchema.Node, closeBraceToken);
	}

	public static InternalXmlPortTableElementSyntax XmlPortTableElement(InternalSyntaxToken xmlPortTableElementKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax sourceTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalXmlPortNodeSyntax> schema, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortTableElementSyntax(SyntaxKind.XmlPortTableElement, xmlPortTableElementKeyword, openParenthesisToken, name, semicolonToken, sourceTable, closeParenthesisToken, openBraceToken, propertyList, schema.Node, triggers.Node, closeBraceToken);
	}

	public static InternalXmlPortFieldElementSyntax XmlPortFieldElement(InternalSyntaxToken xmlPortFieldElementKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalXmlPortNodeSyntax> schema, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortFieldElementSyntax(SyntaxKind.XmlPortFieldElement, xmlPortFieldElementKeyword, openParenthesisToken, name, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema.Node, triggers.Node, closeBraceToken);
	}

	public static InternalXmlPortFieldAttributeSyntax XmlPortFieldAttribute(InternalSyntaxToken xmlPortFieldAttributeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalXmlPortNodeSyntax> schema, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortFieldAttributeSyntax(SyntaxKind.XmlPortFieldAttribute, xmlPortFieldAttributeKeyword, openParenthesisToken, name, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema.Node, triggers.Node, closeBraceToken);
	}

	public static InternalXmlPortTextElementSyntax XmlPortTextElement(InternalSyntaxToken xmlPortTextElementKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalXmlPortNodeSyntax> schema, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortTextElementSyntax(SyntaxKind.XmlPortTextElement, xmlPortTextElementKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, schema.Node, triggers.Node, closeBraceToken);
	}

	public static InternalXmlPortTextAttributeSyntax XmlPortTextAttribute(InternalSyntaxToken xmlPortTextAttributeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalXmlPortNodeSyntax> schema, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortTextAttributeSyntax(SyntaxKind.XmlPortTextAttribute, xmlPortTextAttributeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, schema.Node, triggers.Node, closeBraceToken);
	}

	public static InternalControlAddInSyntax ControlAddIn(InternalSyntaxToken controlAddInKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalControlAddInSyntax(SyntaxKind.ControlAddInObject, controlAddInKeyword, name, openBraceToken, propertyList, members.Node, closeBraceToken);
	}

	public static InternalMemberAttributeSyntax MemberAttribute(InternalSyntaxToken openBracketToken, InternalIdentifierNameSyntax name, InternalAttributeArgumentListSyntax argumentList, InternalSyntaxToken closeBracketToken)
	{
		return new InternalMemberAttributeSyntax(SyntaxKind.MemberAttribute, openBracketToken, name, argumentList, closeBracketToken);
	}

	public static InternalAttributeArgumentListSyntax AttributeArgumentList(InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalAttributeArgumentSyntax> arguments, InternalSyntaxToken closeParenthesisToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(423, openParenthesisToken, arguments.Node, closeParenthesisToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalAttributeArgumentListSyntax)internalSyntaxNode;
		}
		InternalAttributeArgumentListSyntax internalAttributeArgumentListSyntax = new InternalAttributeArgumentListSyntax(SyntaxKind.AttributeArgumentList, openParenthesisToken, arguments.Node, closeParenthesisToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalAttributeArgumentListSyntax, hash);
		}
		return internalAttributeArgumentListSyntax;
	}

	public static InternalInvalidAttributeArgumentSyntax InvalidAttributeArgument(InternalSyntaxNode invalidToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(427, invalidToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInvalidAttributeArgumentSyntax)internalSyntaxNode;
		}
		InternalInvalidAttributeArgumentSyntax internalInvalidAttributeArgumentSyntax = new InternalInvalidAttributeArgumentSyntax(SyntaxKind.InvalidAttributeArgument, invalidToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInvalidAttributeArgumentSyntax, hash);
		}
		return internalInvalidAttributeArgumentSyntax;
	}

	public static InternalLiteralAttributeArgumentSyntax LiteralAttributeArgument(InternalLiteralExpressionSyntax literal)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(424, literal, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalLiteralAttributeArgumentSyntax)internalSyntaxNode;
		}
		InternalLiteralAttributeArgumentSyntax internalLiteralAttributeArgumentSyntax = new InternalLiteralAttributeArgumentSyntax(SyntaxKind.LiteralAttributeArgument, literal);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalLiteralAttributeArgumentSyntax, hash);
		}
		return internalLiteralAttributeArgumentSyntax;
	}

	public static InternalMethodReferenceAttributeArgumentSyntax MethodReferenceAttributeArgument(InternalMemberAccessExpressionSyntax methodReference)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(425, methodReference, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalMethodReferenceAttributeArgumentSyntax)internalSyntaxNode;
		}
		InternalMethodReferenceAttributeArgumentSyntax internalMethodReferenceAttributeArgumentSyntax = new InternalMethodReferenceAttributeArgumentSyntax(SyntaxKind.MethodReferenceAttributeArgument, methodReference);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalMethodReferenceAttributeArgumentSyntax, hash);
		}
		return internalMethodReferenceAttributeArgumentSyntax;
	}

	public static InternalOptionAccessAttributeArgumentSyntax OptionAccessAttributeArgument(InternalOptionAccessExpressionSyntax optionAccess)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(426, optionAccess, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOptionAccessAttributeArgumentSyntax)internalSyntaxNode;
		}
		InternalOptionAccessAttributeArgumentSyntax internalOptionAccessAttributeArgumentSyntax = new InternalOptionAccessAttributeArgumentSyntax(SyntaxKind.OptionAccessAttributeArgument, optionAccess);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOptionAccessAttributeArgumentSyntax, hash);
		}
		return internalOptionAccessAttributeArgumentSyntax;
	}

	public static InternalIdentifierNameOrEmptySyntax IdentifierNameOrEmpty(InternalIdentifierNameSyntax identifierName, InternalSyntaxToken empty)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(222, identifierName, empty, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIdentifierNameOrEmptySyntax)internalSyntaxNode;
		}
		InternalIdentifierNameOrEmptySyntax internalIdentifierNameOrEmptySyntax = new InternalIdentifierNameOrEmptySyntax(SyntaxKind.IdentifierNameOrEmpty, identifierName, empty);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIdentifierNameOrEmptySyntax, hash);
		}
		return internalIdentifierNameOrEmptySyntax;
	}

	public static InternalLiteralExpressionSyntax LiteralExpression(InternalLiteralValueSyntax literal)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(218, literal, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalLiteralExpressionSyntax)internalSyntaxNode;
		}
		InternalLiteralExpressionSyntax internalLiteralExpressionSyntax = new InternalLiteralExpressionSyntax(SyntaxKind.LiteralExpression, literal);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalLiteralExpressionSyntax, hash);
		}
		return internalLiteralExpressionSyntax;
	}

	public static InternalOptionValuesSyntax OptionValues(InternalSeparatedSyntaxList<InternalIdentifierNameOrEmptySyntax> options)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(229, options.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOptionValuesSyntax)internalSyntaxNode;
		}
		InternalOptionValuesSyntax internalOptionValuesSyntax = new InternalOptionValuesSyntax(SyntaxKind.OptionValues, options.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOptionValuesSyntax, hash);
		}
		return internalOptionValuesSyntax;
	}

	public static InternalInt32SignedLiteralValueSyntax Int32SignedLiteralValue(InternalSyntaxToken sign, InternalSyntaxToken number)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(170, sign, number, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInt32SignedLiteralValueSyntax)internalSyntaxNode;
		}
		InternalInt32SignedLiteralValueSyntax internalInt32SignedLiteralValueSyntax = new InternalInt32SignedLiteralValueSyntax(SyntaxKind.Int32SignedLiteralValue, sign, number);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInt32SignedLiteralValueSyntax, hash);
		}
		return internalInt32SignedLiteralValueSyntax;
	}

	public static InternalInt64SignedLiteralValueSyntax Int64SignedLiteralValue(InternalSyntaxToken sign, InternalSyntaxToken number)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(171, sign, number, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInt64SignedLiteralValueSyntax)internalSyntaxNode;
		}
		InternalInt64SignedLiteralValueSyntax internalInt64SignedLiteralValueSyntax = new InternalInt64SignedLiteralValueSyntax(SyntaxKind.Int64SignedLiteralValue, sign, number);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInt64SignedLiteralValueSyntax, hash);
		}
		return internalInt64SignedLiteralValueSyntax;
	}

	public static InternalDecimalSignedLiteralValueSyntax DecimalSignedLiteralValue(InternalSyntaxToken sign, InternalSyntaxToken number)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(172, sign, number, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDecimalSignedLiteralValueSyntax)internalSyntaxNode;
		}
		InternalDecimalSignedLiteralValueSyntax internalDecimalSignedLiteralValueSyntax = new InternalDecimalSignedLiteralValueSyntax(SyntaxKind.DecimalSignedLiteralValue, sign, number);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDecimalSignedLiteralValueSyntax, hash);
		}
		return internalDecimalSignedLiteralValueSyntax;
	}

	public static InternalBooleanLiteralValueSyntax BooleanLiteralValue(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(169, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBooleanLiteralValueSyntax)internalSyntaxNode;
		}
		InternalBooleanLiteralValueSyntax internalBooleanLiteralValueSyntax = new InternalBooleanLiteralValueSyntax(SyntaxKind.BooleanLiteralValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBooleanLiteralValueSyntax, hash);
		}
		return internalBooleanLiteralValueSyntax;
	}

	public static InternalStringLiteralValueSyntax StringLiteralValue(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(176, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalStringLiteralValueSyntax)internalSyntaxNode;
		}
		InternalStringLiteralValueSyntax internalStringLiteralValueSyntax = new InternalStringLiteralValueSyntax(SyntaxKind.StringLiteralValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalStringLiteralValueSyntax, hash);
		}
		return internalStringLiteralValueSyntax;
	}

	public static InternalDateLiteralValueSyntax DateLiteralValue(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(173, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDateLiteralValueSyntax)internalSyntaxNode;
		}
		InternalDateLiteralValueSyntax internalDateLiteralValueSyntax = new InternalDateLiteralValueSyntax(SyntaxKind.DateLiteralValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDateLiteralValueSyntax, hash);
		}
		return internalDateLiteralValueSyntax;
	}

	public static InternalTimeLiteralValueSyntax TimeLiteralValue(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(174, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTimeLiteralValueSyntax)internalSyntaxNode;
		}
		InternalTimeLiteralValueSyntax internalTimeLiteralValueSyntax = new InternalTimeLiteralValueSyntax(SyntaxKind.TimeLiteralValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTimeLiteralValueSyntax, hash);
		}
		return internalTimeLiteralValueSyntax;
	}

	public static InternalDateTimeLiteralValueSyntax DateTimeLiteralValue(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(175, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDateTimeLiteralValueSyntax)internalSyntaxNode;
		}
		InternalDateTimeLiteralValueSyntax internalDateTimeLiteralValueSyntax = new InternalDateTimeLiteralValueSyntax(SyntaxKind.DateTimeLiteralValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDateTimeLiteralValueSyntax, hash);
		}
		return internalDateTimeLiteralValueSyntax;
	}

	public static InternalInvalidPropertyValueSyntax InvalidPropertyValue(InternalSyntaxToken invalidToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(324, invalidToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInvalidPropertyValueSyntax)internalSyntaxNode;
		}
		InternalInvalidPropertyValueSyntax internalInvalidPropertyValueSyntax = new InternalInvalidPropertyValueSyntax(SyntaxKind.InvalidPropertyValue, invalidToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInvalidPropertyValueSyntax, hash);
		}
		return internalInvalidPropertyValueSyntax;
	}

	public static InternalDecimalPlacesPropertyValueSyntax DecimalPlacesPropertyValue(InternalDecimalPlacesSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(384, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDecimalPlacesPropertyValueSyntax)internalSyntaxNode;
		}
		InternalDecimalPlacesPropertyValueSyntax internalDecimalPlacesPropertyValueSyntax = new InternalDecimalPlacesPropertyValueSyntax(SyntaxKind.DecimalPlacesPropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDecimalPlacesPropertyValueSyntax, hash);
		}
		return internalDecimalPlacesPropertyValueSyntax;
	}

	public static InternalOptionValuePropertyValueSyntax OptionValuePropertyValue(InternalIdentifierNameSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(337, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOptionValuePropertyValueSyntax)internalSyntaxNode;
		}
		InternalOptionValuePropertyValueSyntax internalOptionValuePropertyValueSyntax = new InternalOptionValuePropertyValueSyntax(SyntaxKind.OptionValuePropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOptionValuePropertyValueSyntax, hash);
		}
		return internalOptionValuePropertyValueSyntax;
	}

	public static InternalImagePropertyValueSyntax ImagePropertyValue(InternalIdentifierNameSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(336, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalImagePropertyValueSyntax)internalSyntaxNode;
		}
		InternalImagePropertyValueSyntax internalImagePropertyValueSyntax = new InternalImagePropertyValueSyntax(SyntaxKind.ImagePropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalImagePropertyValueSyntax, hash);
		}
		return internalImagePropertyValueSyntax;
	}

	public static InternalPageFieldReferencePropertyValueSyntax PageFieldReferencePropertyValue(InternalIdentifierNameSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(340, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalPageFieldReferencePropertyValueSyntax)internalSyntaxNode;
		}
		InternalPageFieldReferencePropertyValueSyntax internalPageFieldReferencePropertyValueSyntax = new InternalPageFieldReferencePropertyValueSyntax(SyntaxKind.PageFieldReferencePropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalPageFieldReferencePropertyValueSyntax, hash);
		}
		return internalPageFieldReferencePropertyValueSyntax;
	}

	public static InternalMultilanguagePropertyValueSyntax MultilanguagePropertyValue(InternalCommaSeparatedIdentifierEqualsStringListSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(332, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalMultilanguagePropertyValueSyntax)internalSyntaxNode;
		}
		InternalMultilanguagePropertyValueSyntax internalMultilanguagePropertyValueSyntax = new InternalMultilanguagePropertyValueSyntax(SyntaxKind.MultilanguagePropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalMultilanguagePropertyValueSyntax, hash);
		}
		return internalMultilanguagePropertyValueSyntax;
	}

	public static InternalLabelPropertyValueSyntax LabelPropertyValue(InternalLabelSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(333, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalLabelPropertyValueSyntax)internalSyntaxNode;
		}
		InternalLabelPropertyValueSyntax internalLabelPropertyValueSyntax = new InternalLabelPropertyValueSyntax(SyntaxKind.LabelPropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalLabelPropertyValueSyntax, hash);
		}
		return internalLabelPropertyValueSyntax;
	}

	public static InternalMemberReferencePropertyValueSyntax MemberReferencePropertyValue(InternalIdentifierNameSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(391, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalMemberReferencePropertyValueSyntax)internalSyntaxNode;
		}
		InternalMemberReferencePropertyValueSyntax internalMemberReferencePropertyValueSyntax = new InternalMemberReferencePropertyValueSyntax(SyntaxKind.MemberReferencePropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalMemberReferencePropertyValueSyntax, hash);
		}
		return internalMemberReferencePropertyValueSyntax;
	}

	public static InternalPermissionPropertyValueSyntax PermissionPropertyValue(InternalSeparatedSyntaxList<InternalPermissionSyntax> permissionProperties)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(366, permissionProperties.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalPermissionPropertyValueSyntax)internalSyntaxNode;
		}
		InternalPermissionPropertyValueSyntax internalPermissionPropertyValueSyntax = new InternalPermissionPropertyValueSyntax(SyntaxKind.PermissionPropertyValue, permissionProperties.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalPermissionPropertyValueSyntax, hash);
		}
		return internalPermissionPropertyValueSyntax;
	}

	public static InternalQualifiedObjectReferencePropertyValueSyntax QualifiedObjectReferencePropertyValue(InternalSyntaxToken objectType, InternalObjectNameOrIdSyntax objectNameOrId)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(377, objectType, objectNameOrId, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalQualifiedObjectReferencePropertyValueSyntax)internalSyntaxNode;
		}
		InternalQualifiedObjectReferencePropertyValueSyntax internalQualifiedObjectReferencePropertyValueSyntax = new InternalQualifiedObjectReferencePropertyValueSyntax(SyntaxKind.QualifiedObjectReferencePropertyValue, objectType, objectNameOrId);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalQualifiedObjectReferencePropertyValueSyntax, hash);
		}
		return internalQualifiedObjectReferencePropertyValueSyntax;
	}

	public static InternalObjectReferencePropertyValueSyntax ObjectReferencePropertyValue(InternalObjectNameOrIdSyntax objectNameOrId)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(378, objectNameOrId, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalObjectReferencePropertyValueSyntax)internalSyntaxNode;
		}
		InternalObjectReferencePropertyValueSyntax internalObjectReferencePropertyValueSyntax = new InternalObjectReferencePropertyValueSyntax(SyntaxKind.ObjectReferencePropertyValue, objectNameOrId);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalObjectReferencePropertyValueSyntax, hash);
		}
		return internalObjectReferencePropertyValueSyntax;
	}

	public static InternalPermissionSyntax Permission(InternalSyntaxToken objectType, InternalObjectNameOrIdSyntax objectReference, InternalSyntaxToken equalsToken, InternalSyntaxToken permissions)
	{
		return new InternalPermissionSyntax(SyntaxKind.PermissionValue, objectType, objectReference, equalsToken, permissions);
	}

	public static InternalEnumPropertyValueSyntax EnumPropertyValue(InternalIdentifierNameSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(334, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalEnumPropertyValueSyntax)internalSyntaxNode;
		}
		InternalEnumPropertyValueSyntax internalEnumPropertyValueSyntax = new InternalEnumPropertyValueSyntax(SyntaxKind.EnumPropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalEnumPropertyValueSyntax, hash);
		}
		return internalEnumPropertyValueSyntax;
	}

	public static InternalIdentifierOrLiteralPropertyValueSyntax IdentifierOrLiteralPropertyValue(InternalIdentifierOrLiteralExpressionSyntax identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(338, identifier, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIdentifierOrLiteralPropertyValueSyntax)internalSyntaxNode;
		}
		InternalIdentifierOrLiteralPropertyValueSyntax internalIdentifierOrLiteralPropertyValueSyntax = new InternalIdentifierOrLiteralPropertyValueSyntax(SyntaxKind.IdentifierOrLiteralPropertyValue, identifier);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIdentifierOrLiteralPropertyValueSyntax, hash);
		}
		return internalIdentifierOrLiteralPropertyValueSyntax;
	}

	public static InternalIdentifierOrLiteralExpressionSyntax IdentifierOrLiteralExpression(InternalCodeExpressionSyntax identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(219, identifier, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIdentifierOrLiteralExpressionSyntax)internalSyntaxNode;
		}
		InternalIdentifierOrLiteralExpressionSyntax internalIdentifierOrLiteralExpressionSyntax = new InternalIdentifierOrLiteralExpressionSyntax(SyntaxKind.IdentifierOrLiteralExpression, identifier);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIdentifierOrLiteralExpressionSyntax, hash);
		}
		return internalIdentifierOrLiteralExpressionSyntax;
	}

	public static InternalObjectNameReferenceSyntax ObjectNameReference(InternalCodeExpressionSyntax identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(232, identifier, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalObjectNameReferenceSyntax)internalSyntaxNode;
		}
		InternalObjectNameReferenceSyntax internalObjectNameReferenceSyntax = new InternalObjectNameReferenceSyntax(SyntaxKind.ObjectNameReference, identifier);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalObjectNameReferenceSyntax, hash);
		}
		return internalObjectNameReferenceSyntax;
	}

	public static InternalObjectNameOrIdSyntax ObjectNameOrId(InternalCodeExpressionSyntax identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(230, identifier, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalObjectNameOrIdSyntax)internalSyntaxNode;
		}
		InternalObjectNameOrIdSyntax internalObjectNameOrIdSyntax = new InternalObjectNameOrIdSyntax(SyntaxKind.ObjectReference, identifier);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalObjectNameOrIdSyntax, hash);
		}
		return internalObjectNameOrIdSyntax;
	}

	public static InternalObjectIdSyntax ObjectId(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(231, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalObjectIdSyntax)internalSyntaxNode;
		}
		InternalObjectIdSyntax internalObjectIdSyntax = new InternalObjectIdSyntax(SyntaxKind.ObjectId, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalObjectIdSyntax, hash);
		}
		return internalObjectIdSyntax;
	}

	public static InternalDecimalPlacesSyntax DecimalPlaces(InternalSyntaxToken minimumToken, InternalSyntaxToken colonToken, InternalSyntaxToken maximumToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(364, minimumToken, colonToken, maximumToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDecimalPlacesSyntax)internalSyntaxNode;
		}
		InternalDecimalPlacesSyntax internalDecimalPlacesSyntax = new InternalDecimalPlacesSyntax(SyntaxKind.DecimalPlaces, minimumToken, colonToken, maximumToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDecimalPlacesSyntax, hash);
		}
		return internalDecimalPlacesSyntax;
	}

	public static InternalMemberAccessOrIdentifierPropertyValueSyntax MemberAccessOrIdentifierPropertyValue(InternalCodeExpressionSyntax identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(339, identifier, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalMemberAccessOrIdentifierPropertyValueSyntax)internalSyntaxNode;
		}
		InternalMemberAccessOrIdentifierPropertyValueSyntax internalMemberAccessOrIdentifierPropertyValueSyntax = new InternalMemberAccessOrIdentifierPropertyValueSyntax(SyntaxKind.MemberAccessOrIdentifierPropertyValue, identifier);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalMemberAccessOrIdentifierPropertyValueSyntax, hash);
		}
		return internalMemberAccessOrIdentifierPropertyValueSyntax;
	}

	public static InternalPropertyNameSyntax PropertyName(InternalSyntaxToken identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(323, identifier, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalPropertyNameSyntax)internalSyntaxNode;
		}
		InternalPropertyNameSyntax internalPropertyNameSyntax = new InternalPropertyNameSyntax(SyntaxKind.PropertyName, identifier);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalPropertyNameSyntax, hash);
		}
		return internalPropertyNameSyntax;
	}

	public static InternalCommaSeparatedPropertyValueSyntax CommaSeparatedPropertyValue(InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> values)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(369, values.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCommaSeparatedPropertyValueSyntax)internalSyntaxNode;
		}
		InternalCommaSeparatedPropertyValueSyntax internalCommaSeparatedPropertyValueSyntax = new InternalCommaSeparatedPropertyValueSyntax(SyntaxKind.CommaSeparatedPropertyValue, values.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCommaSeparatedPropertyValueSyntax, hash);
		}
		return internalCommaSeparatedPropertyValueSyntax;
	}

	public static InternalCommaSeparatedStringsPropertyValueSyntax CommaSeparatedStringsPropertyValue(InternalSeparatedSyntaxList<InternalStringLiteralValueSyntax> values)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(370, values.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCommaSeparatedStringsPropertyValueSyntax)internalSyntaxNode;
		}
		InternalCommaSeparatedStringsPropertyValueSyntax internalCommaSeparatedStringsPropertyValueSyntax = new InternalCommaSeparatedStringsPropertyValueSyntax(SyntaxKind.CommaSeparatedStringsPropertyValue, values.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCommaSeparatedStringsPropertyValueSyntax, hash);
		}
		return internalCommaSeparatedStringsPropertyValueSyntax;
	}

	public static InternalCommaSeparatedIdentifierEqualsStringListSyntax CommaSeparatedIdentifierEqualsStringList(InternalSeparatedSyntaxList<InternalIdentifierEqualsStringSyntax> values)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(371, values.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCommaSeparatedIdentifierEqualsStringListSyntax)internalSyntaxNode;
		}
		InternalCommaSeparatedIdentifierEqualsStringListSyntax internalCommaSeparatedIdentifierEqualsStringListSyntax = new InternalCommaSeparatedIdentifierEqualsStringListSyntax(SyntaxKind.CommaSeparatedIdentifierEqualsStringList, values.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCommaSeparatedIdentifierEqualsStringListSyntax, hash);
		}
		return internalCommaSeparatedIdentifierEqualsStringListSyntax;
	}

	public static InternalCommaSeparatedIdentifierEqualsLiteralListSyntax CommaSeparatedIdentifierEqualsLiteralList(InternalSeparatedSyntaxList<InternalIdentifierEqualsLiteralSyntax> values)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(372, values.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCommaSeparatedIdentifierEqualsLiteralListSyntax)internalSyntaxNode;
		}
		InternalCommaSeparatedIdentifierEqualsLiteralListSyntax internalCommaSeparatedIdentifierEqualsLiteralListSyntax = new InternalCommaSeparatedIdentifierEqualsLiteralListSyntax(SyntaxKind.CommaSeparatedIdentifierEqualsLiteralList, values.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCommaSeparatedIdentifierEqualsLiteralListSyntax, hash);
		}
		return internalCommaSeparatedIdentifierEqualsLiteralListSyntax;
	}

	public static InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax CommaSeparatedIdentifierEqualsStringListPropertyValue(InternalCommaSeparatedIdentifierEqualsStringListSyntax list)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(373, list, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax)internalSyntaxNode;
		}
		InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax internalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax = new InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax(SyntaxKind.CommaSeparatedIdentifierEqualsStringListPropertyValue, list);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax, hash);
		}
		return internalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax;
	}

	public static InternalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax CommaSeparatedIdentifierOrLiteralPropertyValue(InternalSeparatedSyntaxList<InternalIdentifierOrLiteralExpressionSyntax> values)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(374, values.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax)internalSyntaxNode;
		}
		InternalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax internalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax = new InternalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax(SyntaxKind.CommaSeparatedIdentifierOrLiteralPropertyValue, values.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax, hash);
		}
		return internalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax;
	}

	public static InternalLabelSyntax Label(InternalStringLiteralValueSyntax labelText, InternalSyntaxToken commaToken, InternalCommaSeparatedIdentifierEqualsLiteralListSyntax properties)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(313, labelText, commaToken, properties, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalLabelSyntax)internalSyntaxNode;
		}
		InternalLabelSyntax internalLabelSyntax = new InternalLabelSyntax(SyntaxKind.Label, labelText, commaToken, properties);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalLabelSyntax, hash);
		}
		return internalLabelSyntax;
	}

	public static InternalOptionValuesPropertyValueSyntax OptionValuesPropertyValue(InternalOptionValuesSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(365, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOptionValuesPropertyValueSyntax)internalSyntaxNode;
		}
		InternalOptionValuesPropertyValueSyntax internalOptionValuesPropertyValueSyntax = new InternalOptionValuesPropertyValueSyntax(SyntaxKind.OptionValuesPropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOptionValuesPropertyValueSyntax, hash);
		}
		return internalOptionValuesPropertyValueSyntax;
	}

	public static InternalBooleanPropertyValueSyntax BooleanPropertyValue(InternalBooleanLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(335, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBooleanPropertyValueSyntax)internalSyntaxNode;
		}
		InternalBooleanPropertyValueSyntax internalBooleanPropertyValueSyntax = new InternalBooleanPropertyValueSyntax(SyntaxKind.BooleanPropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBooleanPropertyValueSyntax, hash);
		}
		return internalBooleanPropertyValueSyntax;
	}

	public static InternalInt32PropertyValueSyntax Int32PropertyValue(InternalInt32SignedLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(325, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInt32PropertyValueSyntax)internalSyntaxNode;
		}
		InternalInt32PropertyValueSyntax internalInt32PropertyValueSyntax = new InternalInt32PropertyValueSyntax(SyntaxKind.Int32PropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInt32PropertyValueSyntax, hash);
		}
		return internalInt32PropertyValueSyntax;
	}

	public static InternalInt64PropertyValueSyntax Int64PropertyValue(InternalInt64SignedLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(326, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInt64PropertyValueSyntax)internalSyntaxNode;
		}
		InternalInt64PropertyValueSyntax internalInt64PropertyValueSyntax = new InternalInt64PropertyValueSyntax(SyntaxKind.Int64PropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInt64PropertyValueSyntax, hash);
		}
		return internalInt64PropertyValueSyntax;
	}

	public static InternalDecimalPropertyValueSyntax DecimalPropertyValue(InternalDecimalSignedLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(327, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDecimalPropertyValueSyntax)internalSyntaxNode;
		}
		InternalDecimalPropertyValueSyntax internalDecimalPropertyValueSyntax = new InternalDecimalPropertyValueSyntax(SyntaxKind.DecimalPropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDecimalPropertyValueSyntax, hash);
		}
		return internalDecimalPropertyValueSyntax;
	}

	public static InternalTimePropertyValueSyntax TimePropertyValue(InternalTimeLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(329, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTimePropertyValueSyntax)internalSyntaxNode;
		}
		InternalTimePropertyValueSyntax internalTimePropertyValueSyntax = new InternalTimePropertyValueSyntax(SyntaxKind.TimePropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTimePropertyValueSyntax, hash);
		}
		return internalTimePropertyValueSyntax;
	}

	public static InternalDatePropertyValueSyntax DatePropertyValue(InternalDateLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(330, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDatePropertyValueSyntax)internalSyntaxNode;
		}
		InternalDatePropertyValueSyntax internalDatePropertyValueSyntax = new InternalDatePropertyValueSyntax(SyntaxKind.DatePropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDatePropertyValueSyntax, hash);
		}
		return internalDatePropertyValueSyntax;
	}

	public static InternalDateTimePropertyValueSyntax DateTimePropertyValue(InternalDateTimeLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(331, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDateTimePropertyValueSyntax)internalSyntaxNode;
		}
		InternalDateTimePropertyValueSyntax internalDateTimePropertyValueSyntax = new InternalDateTimePropertyValueSyntax(SyntaxKind.DateTimePropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDateTimePropertyValueSyntax, hash);
		}
		return internalDateTimePropertyValueSyntax;
	}

	public static InternalStringPropertyValueSyntax StringPropertyValue(InternalStringLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(328, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalStringPropertyValueSyntax)internalSyntaxNode;
		}
		InternalStringPropertyValueSyntax internalStringPropertyValueSyntax = new InternalStringPropertyValueSyntax(SyntaxKind.StringPropertyValue, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalStringPropertyValueSyntax, hash);
		}
		return internalStringPropertyValueSyntax;
	}

	public static InternalStyleExpressionPropertyValueSyntax StyleExpressionPropertyValue(InternalCodeExpressionSyntax expression)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(390, expression, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalStyleExpressionPropertyValueSyntax)internalSyntaxNode;
		}
		InternalStyleExpressionPropertyValueSyntax internalStyleExpressionPropertyValueSyntax = new InternalStyleExpressionPropertyValueSyntax(SyntaxKind.StyleExpressionPropertyValue, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalStyleExpressionPropertyValueSyntax, hash);
		}
		return internalStyleExpressionPropertyValueSyntax;
	}

	public static InternalBooleanExpressionPropertyValueSyntax BooleanExpressionPropertyValue(InternalCodeExpressionSyntax expression)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(380, expression, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBooleanExpressionPropertyValueSyntax)internalSyntaxNode;
		}
		InternalBooleanExpressionPropertyValueSyntax internalBooleanExpressionPropertyValueSyntax = new InternalBooleanExpressionPropertyValueSyntax(SyntaxKind.BooleanExpressionPropertyValue, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBooleanExpressionPropertyValueSyntax, hash);
		}
		return internalBooleanExpressionPropertyValueSyntax;
	}

	public static InternalTextExpressionPropertyValueSyntax TextExpressionPropertyValue(InternalCodeExpressionSyntax expression)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(383, expression, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTextExpressionPropertyValueSyntax)internalSyntaxNode;
		}
		InternalTextExpressionPropertyValueSyntax internalTextExpressionPropertyValueSyntax = new InternalTextExpressionPropertyValueSyntax(SyntaxKind.TextExpressionPropertyValue, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTextExpressionPropertyValueSyntax, hash);
		}
		return internalTextExpressionPropertyValueSyntax;
	}

	public static InternalIntegerExpressionPropertyValueSyntax IntegerExpressionPropertyValue(InternalCodeExpressionSyntax expression)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(382, expression, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIntegerExpressionPropertyValueSyntax)internalSyntaxNode;
		}
		InternalIntegerExpressionPropertyValueSyntax internalIntegerExpressionPropertyValueSyntax = new InternalIntegerExpressionPropertyValueSyntax(SyntaxKind.IntegerExpressionPropertyValue, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIntegerExpressionPropertyValueSyntax, hash);
		}
		return internalIntegerExpressionPropertyValueSyntax;
	}

	public static InternalClientSideBooleanExpressionPropertyValueSyntax ClientSideBooleanExpressionPropertyValue(InternalCodeExpressionSyntax expression)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(381, expression, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalClientSideBooleanExpressionPropertyValueSyntax)internalSyntaxNode;
		}
		InternalClientSideBooleanExpressionPropertyValueSyntax internalClientSideBooleanExpressionPropertyValueSyntax = new InternalClientSideBooleanExpressionPropertyValueSyntax(SyntaxKind.ClientSideBooleanExpressionPropertyValue, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalClientSideBooleanExpressionPropertyValueSyntax, hash);
		}
		return internalClientSideBooleanExpressionPropertyValueSyntax;
	}

	public static InternalTableFilterPropertyValueSyntax TableFilterPropertyValue(InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> conditions)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(385, conditions.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTableFilterPropertyValueSyntax)internalSyntaxNode;
		}
		InternalTableFilterPropertyValueSyntax internalTableFilterPropertyValueSyntax = new InternalTableFilterPropertyValueSyntax(SyntaxKind.TableFilterPropertyValue, conditions.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTableFilterPropertyValueSyntax, hash);
		}
		return internalTableFilterPropertyValueSyntax;
	}

	public static InternalOrderByPropertyValueSyntax OrderByPropertyValue(InternalSeparatedSyntaxList<InternalOrderByExpressionSyntax> order)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(393, order.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOrderByPropertyValueSyntax)internalSyntaxNode;
		}
		InternalOrderByPropertyValueSyntax internalOrderByPropertyValueSyntax = new InternalOrderByPropertyValueSyntax(SyntaxKind.OrderByPropertyValue, order.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOrderByPropertyValueSyntax, hash);
		}
		return internalOrderByPropertyValueSyntax;
	}

	public static InternalOrderByExpressionSyntax OrderByExpression(InternalSyntaxToken orderSpecifier, InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> sortingFields, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalOrderByExpressionSyntax(SyntaxKind.OrderByExpression, orderSpecifier, openParenthesisToken, sortingFields.Node, closeParenthesisToken);
	}

	public static InternalQueryDataItemLinkPropertyValueSyntax QueryDataItemLinkPropertyValue(InternalSeparatedSyntaxList<InternalQueryDataItemLinkExpressionSyntax> joins)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(395, joins.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalQueryDataItemLinkPropertyValueSyntax)internalSyntaxNode;
		}
		InternalQueryDataItemLinkPropertyValueSyntax internalQueryDataItemLinkPropertyValueSyntax = new InternalQueryDataItemLinkPropertyValueSyntax(SyntaxKind.QueryDataItemLinkPropertyValue, joins.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalQueryDataItemLinkPropertyValueSyntax, hash);
		}
		return internalQueryDataItemLinkPropertyValueSyntax;
	}

	public static InternalQueryDataItemLinkExpressionSyntax QueryDataItemLinkExpression(InternalIdentifierNameSyntax sourceField, InternalSyntaxToken equalsToken, InternalMemberAccessExpressionSyntax relatedField)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(396, sourceField, equalsToken, relatedField, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalQueryDataItemLinkExpressionSyntax)internalSyntaxNode;
		}
		InternalQueryDataItemLinkExpressionSyntax internalQueryDataItemLinkExpressionSyntax = new InternalQueryDataItemLinkExpressionSyntax(SyntaxKind.QueryDataItemLinkExpression, sourceField, equalsToken, relatedField);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalQueryDataItemLinkExpressionSyntax, hash);
		}
		return internalQueryDataItemLinkExpressionSyntax;
	}

	public static InternalReportDataItemLinkPropertyValueSyntax ReportDataItemLinkPropertyValue(InternalSeparatedSyntaxList<InternalReportDataItemLinkExpressionSyntax> joins)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(397, joins.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalReportDataItemLinkPropertyValueSyntax)internalSyntaxNode;
		}
		InternalReportDataItemLinkPropertyValueSyntax internalReportDataItemLinkPropertyValueSyntax = new InternalReportDataItemLinkPropertyValueSyntax(SyntaxKind.ReportDataItemLinkPropertyValue, joins.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalReportDataItemLinkPropertyValueSyntax, hash);
		}
		return internalReportDataItemLinkPropertyValueSyntax;
	}

	public static InternalReportDataItemLinkExpressionSyntax ReportDataItemLinkExpression(InternalIdentifierNameSyntax sourceField, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalReportDataItemLinkExpressionSyntax(SyntaxKind.ReportDataItemLinkExpression, sourceField, equalsToken, fieldKeywordToken, openParenthesisToken, relatedField, closeParenthesisToken);
	}

	public static InternalTableViewPropertyValueSyntax TableViewPropertyValue(InternalSortingSyntax sorting, InternalOrderSyntax order, InternalWhereExpressionSyntax tableFilter)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(386, sorting, order, tableFilter, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTableViewPropertyValueSyntax)internalSyntaxNode;
		}
		InternalTableViewPropertyValueSyntax internalTableViewPropertyValueSyntax = new InternalTableViewPropertyValueSyntax(SyntaxKind.TableViewPropertyValue, sorting, order, tableFilter);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTableViewPropertyValueSyntax, hash);
		}
		return internalTableViewPropertyValueSyntax;
	}

	public static InternalFiltersPropertyValueSyntax FiltersPropertyValue(InternalWhereExpressionSyntax tableFilter)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(387, tableFilter, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalFiltersPropertyValueSyntax)internalSyntaxNode;
		}
		InternalFiltersPropertyValueSyntax internalFiltersPropertyValueSyntax = new InternalFiltersPropertyValueSyntax(SyntaxKind.FiltersPropertyValue, tableFilter);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalFiltersPropertyValueSyntax, hash);
		}
		return internalFiltersPropertyValueSyntax;
	}

	public static InternalSortingSyntax Sorting(InternalSyntaxToken sortingToken, InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> sortingFields, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalSortingSyntax(SyntaxKind.SortingExpression, sortingToken, openParenthesisToken, sortingFields.Node, closeParenthesisToken);
	}

	public static InternalOrderSyntax Order(InternalSyntaxToken orderToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken orderBy, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalOrderSyntax(SyntaxKind.OrderExpression, orderToken, openParenthesisToken, orderBy, closeParenthesisToken);
	}

	public static InternalTableRelationPropertyValueSyntax TableRelationPropertyValue(InternalIfTableRelationSyntax ifExpression, InternalMemberAccessOrIdentifierPropertyValueSyntax relatedTableField, InternalWhereExpressionSyntax tableFilter, InternalElseTableRelationSyntax elseExpression)
	{
		return new InternalTableRelationPropertyValueSyntax(SyntaxKind.TableRelationStatement, ifExpression, relatedTableField, tableFilter, elseExpression);
	}

	public static InternalIfTableRelationSyntax IfTableRelation(InternalSyntaxToken ifKeywordToken, InternalTableFilterExpressionSyntax ifTableRelationCondition)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(353, ifKeywordToken, ifTableRelationCondition, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIfTableRelationSyntax)internalSyntaxNode;
		}
		InternalIfTableRelationSyntax internalIfTableRelationSyntax = new InternalIfTableRelationSyntax(SyntaxKind.IfTableRelationExpression, ifKeywordToken, ifTableRelationCondition);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIfTableRelationSyntax, hash);
		}
		return internalIfTableRelationSyntax;
	}

	public static InternalTableFilterExpressionSyntax TableFilterExpression(InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> conditions, InternalSyntaxToken closeParenthesisToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(344, openParenthesisToken, conditions.Node, closeParenthesisToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTableFilterExpressionSyntax)internalSyntaxNode;
		}
		InternalTableFilterExpressionSyntax internalTableFilterExpressionSyntax = new InternalTableFilterExpressionSyntax(SyntaxKind.TableFilterExpression, openParenthesisToken, conditions.Node, closeParenthesisToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTableFilterExpressionSyntax, hash);
		}
		return internalTableFilterExpressionSyntax;
	}

	public static InternalElseTableRelationSyntax ElseTableRelation(InternalSyntaxToken elseKeywordToken, InternalTableRelationPropertyValueSyntax elseTableRelationCondition)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(354, elseKeywordToken, elseTableRelationCondition, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalElseTableRelationSyntax)internalSyntaxNode;
		}
		InternalElseTableRelationSyntax internalElseTableRelationSyntax = new InternalElseTableRelationSyntax(SyntaxKind.ElseTableRelationExpression, elseKeywordToken, elseTableRelationCondition);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalElseTableRelationSyntax, hash);
		}
		return internalElseTableRelationSyntax;
	}

	public static InternalWhereExpressionSyntax WhereExpression(InternalSyntaxToken whereKeywordToken, InternalTableFilterExpressionSyntax filter)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(363, whereKeywordToken, filter, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalWhereExpressionSyntax)internalSyntaxNode;
		}
		InternalWhereExpressionSyntax internalWhereExpressionSyntax = new InternalWhereExpressionSyntax(SyntaxKind.WhereExpression, whereKeywordToken, filter);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalWhereExpressionSyntax, hash);
		}
		return internalWhereExpressionSyntax;
	}

	public static InternalConstExpressionSyntax ConstExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken constKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierOrLiteralExpressionSyntax identifier, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalConstExpressionSyntax(SyntaxKind.ConstExpression, leftHandSide, equalsToken, constKeywordToken, openParenthesisToken, identifier, closeParenthesisToken);
	}

	public static InternalSimpleFieldExpressionSyntax SimpleFieldExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalSimpleFieldExpressionSyntax(SyntaxKind.SimpleFieldExpression, leftHandSide, equalsToken, fieldKeywordToken, openParenthesisToken, identifier, closeParenthesisToken);
	}

	public static InternalInvalidPropertyExpressionSyntax InvalidPropertyExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken constOrFieldOrFilterKeywordToken, InternalSyntaxToken invalidExpression)
	{
		return new InternalInvalidPropertyExpressionSyntax(SyntaxKind.InvalidPropertyExpression, leftHandSide, equalsToken, constOrFieldOrFilterKeywordToken, invalidExpression);
	}

	public static InternalTableCalculationFormulaSyntax TableCalculationFormula(SyntaxKind kind, InternalSyntaxToken sign, InternalSyntaxToken formulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax table, InternalWhereExpressionSyntax whereExpression, InternalSyntaxToken closeParenthesisToken)
	{
		if ((uint)(kind - 355) > 1u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		return new InternalTableCalculationFormulaSyntax(kind, sign, formulaKeywordToken, openParenthesisToken, table, whereExpression, closeParenthesisToken);
	}

	public static InternalFieldCalculationFormulaSyntax FieldCalculationFormula(SyntaxKind kind, InternalSyntaxToken sign, InternalSyntaxToken formulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalMemberAccessExpressionSyntax field, InternalWhereExpressionSyntax whereExpression, InternalSyntaxToken closeParenthesisToken)
	{
		if ((uint)(kind - 357) > 4u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		return new InternalFieldCalculationFormulaSyntax(kind, sign, formulaKeywordToken, openParenthesisToken, field, whereExpression, closeParenthesisToken);
	}

	public static InternalFieldFilterExpressionSyntax FieldFilterExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken1, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalFieldFilterExpressionSyntax(SyntaxKind.FieldFilterExpression, leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, filterFormulaKeywordToken, openParenthesisToken1, identifier, closeParenthesisToken1, closeParenthesisToken);
	}

	public static InternalFieldUpperLimitExpressionSyntax FieldUpperLimitExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken upperLimitFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken1, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalFieldUpperLimitExpressionSyntax(SyntaxKind.FieldUpperLimitExpression, leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, upperLimitFormulaKeywordToken, openParenthesisToken1, identifier, closeParenthesisToken1, closeParenthesisToken);
	}

	public static InternalFieldUpperLimitFilterExpressionSyntax FieldUpperLimitFilterExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalSyntaxToken upperLimitFormulaKeywordToken, InternalSyntaxToken openParenthesisToken2, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken3, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken3, InternalSyntaxToken closeParenthesisToken2, InternalSyntaxToken closeParenthesisToken1)
	{
		return new InternalFieldUpperLimitFilterExpressionSyntax(SyntaxKind.FieldUpperLimitFilterExpression, leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken1, upperLimitFormulaKeywordToken, openParenthesisToken2, filterFormulaKeywordToken, openParenthesisToken3, identifier, closeParenthesisToken3, closeParenthesisToken2, closeParenthesisToken1);
	}

	public static InternalFilterExpressionSyntax FilterExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalFilterExpressionValueSyntax filter, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalFilterExpressionSyntax(SyntaxKind.FilterExpression, leftHandSide, equalsToken, filterFormulaKeywordToken, openParenthesisToken, filter, closeParenthesisToken);
	}

	public static InternalParenthesizedFilterExpressionValueSyntax ParenthesizedFilterExpressionValue(InternalSyntaxToken openParenthesisToken, InternalFilterExpressionValueSyntax expression, InternalSyntaxToken closeParenthesisToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(399, openParenthesisToken, expression, closeParenthesisToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalParenthesizedFilterExpressionValueSyntax)internalSyntaxNode;
		}
		InternalParenthesizedFilterExpressionValueSyntax internalParenthesizedFilterExpressionValueSyntax = new InternalParenthesizedFilterExpressionValueSyntax(SyntaxKind.ParenthesizedFilterExpressionValue, openParenthesisToken, expression, closeParenthesisToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalParenthesizedFilterExpressionValueSyntax, hash);
		}
		return internalParenthesizedFilterExpressionValueSyntax;
	}

	public static InternalUnaryFilterExpressionValueSyntax UnaryFilterExpressionValue(SyntaxKind kind, InternalSyntaxToken operatorToken, InternalIdentifierOrLiteralExpressionSyntax identifier)
	{
		if ((uint)(kind - 400) > 5u && kind != SyntaxKind.RangeToFilterExpression)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode((int)kind, operatorToken, identifier, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalUnaryFilterExpressionValueSyntax)internalSyntaxNode;
		}
		InternalUnaryFilterExpressionValueSyntax internalUnaryFilterExpressionValueSyntax = new InternalUnaryFilterExpressionValueSyntax(kind, operatorToken, identifier);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalUnaryFilterExpressionValueSyntax, hash);
		}
		return internalUnaryFilterExpressionValueSyntax;
	}

	public static InternalBinaryFilterExpressionValueSyntax BinaryFilterExpressionValue(SyntaxKind kind, InternalSyntaxNode left, InternalSyntaxToken operatorToken, InternalSyntaxNode right)
	{
		if ((uint)(kind - 406) > 2u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode((int)kind, left, operatorToken, right, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBinaryFilterExpressionValueSyntax)internalSyntaxNode;
		}
		InternalBinaryFilterExpressionValueSyntax internalBinaryFilterExpressionValueSyntax = new InternalBinaryFilterExpressionValueSyntax(kind, left, operatorToken, right);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBinaryFilterExpressionValueSyntax, hash);
		}
		return internalBinaryFilterExpressionValueSyntax;
	}

	public static InternalRangeFromFilterExpressionValueSyntax RangeFromFilterExpressionValue(InternalIdentifierOrLiteralExpressionSyntax left, InternalSyntaxToken operatorToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(409, left, operatorToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalRangeFromFilterExpressionValueSyntax)internalSyntaxNode;
		}
		InternalRangeFromFilterExpressionValueSyntax internalRangeFromFilterExpressionValueSyntax = new InternalRangeFromFilterExpressionValueSyntax(SyntaxKind.RangeFromFilterExpression, left, operatorToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalRangeFromFilterExpressionValueSyntax, hash);
		}
		return internalRangeFromFilterExpressionValueSyntax;
	}

	public static InternalInvalidFilterExpressionValueSyntax InvalidFilterExpressionValue(InternalSyntaxToken invalidToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(351, invalidToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInvalidFilterExpressionValueSyntax)internalSyntaxNode;
		}
		InternalInvalidFilterExpressionValueSyntax internalInvalidFilterExpressionValueSyntax = new InternalInvalidFilterExpressionValueSyntax(SyntaxKind.InvalidFilterExpressionValue, invalidToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInvalidFilterExpressionValueSyntax, hash);
		}
		return internalInvalidFilterExpressionValueSyntax;
	}

	public static InternalEmptyPropertySyntax EmptyProperty(InternalSyntaxToken semicolonToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(322, semicolonToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalEmptyPropertySyntax)internalSyntaxNode;
		}
		InternalEmptyPropertySyntax internalEmptyPropertySyntax = new InternalEmptyPropertySyntax(SyntaxKind.EmptyProperty, semicolonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalEmptyPropertySyntax, hash);
		}
		return internalEmptyPropertySyntax;
	}

	public static InternalPropertySyntax Property(InternalPropertyNameSyntax name, InternalSyntaxToken equalsToken, InternalPropertyValueSyntax value, InternalSyntaxToken semicolonToken)
	{
		return new InternalPropertySyntax(SyntaxKind.Property, name, equalsToken, value, semicolonToken);
	}

	public static InternalVarSectionSyntax VarSection(InternalSyntaxToken varKeyword, InternalSyntaxList<InternalVariableDeclarationSyntax> variables)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(235, varKeyword, variables.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalVarSectionSyntax)internalSyntaxNode;
		}
		InternalVarSectionSyntax internalVarSectionSyntax = new InternalVarSectionSyntax(SyntaxKind.VarSection, varKeyword, variables.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalVarSectionSyntax, hash);
		}
		return internalVarSectionSyntax;
	}

	public static InternalVariableDeclarationSyntax VariableDeclaration(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalTypeReferenceBaseSyntax type, InternalSyntaxToken semicolonToken)
	{
		return new InternalVariableDeclarationSyntax(SyntaxKind.VariableDeclaration, attributes.Node, name, colonToken, type, semicolonToken);
	}

	public static InternalParameterSyntax Parameter(InternalSyntaxToken varKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalTypeReferenceBaseSyntax type)
	{
		return new InternalParameterSyntax(SyntaxKind.Parameter, varKeyword, name, colonToken, type);
	}

	public static InternalReturnValueSyntax ReturnValue(InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalDataTypeSyntax dataType)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(242, name, colonToken, dataType, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalReturnValueSyntax)internalSyntaxNode;
		}
		InternalReturnValueSyntax internalReturnValueSyntax = new InternalReturnValueSyntax(SyntaxKind.ReturnValue, name, colonToken, dataType);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalReturnValueSyntax, hash);
		}
		return internalReturnValueSyntax;
	}

	public static InternalSimpleTypeReferenceSyntax SimpleTypeReference(InternalArraySyntax array, InternalDataTypeSyntax dataType)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(243, array, dataType, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalSimpleTypeReferenceSyntax)internalSyntaxNode;
		}
		InternalSimpleTypeReferenceSyntax internalSimpleTypeReferenceSyntax = new InternalSimpleTypeReferenceSyntax(SyntaxKind.SimpleTypeReference, array, dataType);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalSimpleTypeReferenceSyntax, hash);
		}
		return internalSimpleTypeReferenceSyntax;
	}

	public static InternalRecordTypeReferenceSyntax RecordTypeReference(InternalArraySyntax array, InternalDataTypeSyntax dataType, InternalSyntaxToken temporary)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(244, array, dataType, temporary, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalRecordTypeReferenceSyntax)internalSyntaxNode;
		}
		InternalRecordTypeReferenceSyntax internalRecordTypeReferenceSyntax = new InternalRecordTypeReferenceSyntax(SyntaxKind.RecordTypeReference, array, dataType, temporary);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalRecordTypeReferenceSyntax, hash);
		}
		return internalRecordTypeReferenceSyntax;
	}

	public static InternalDotNetTypeReferenceSyntax DotNetTypeReference(InternalArraySyntax array, InternalDataTypeSyntax dataType)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(245, array, dataType, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDotNetTypeReferenceSyntax)internalSyntaxNode;
		}
		InternalDotNetTypeReferenceSyntax internalDotNetTypeReferenceSyntax = new InternalDotNetTypeReferenceSyntax(SyntaxKind.DotNetTypeReference, array, dataType);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDotNetTypeReferenceSyntax, hash);
		}
		return internalDotNetTypeReferenceSyntax;
	}

	public static InternalArraySyntax Array(InternalSyntaxToken arrayKeyword, InternalBracketedDimensionListSyntax dimensionList, InternalSyntaxToken ofKeyword)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(255, arrayKeyword, dimensionList, ofKeyword, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalArraySyntax)internalSyntaxNode;
		}
		InternalArraySyntax internalArraySyntax = new InternalArraySyntax(SyntaxKind.Array, arrayKeyword, dimensionList, ofKeyword);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalArraySyntax, hash);
		}
		return internalArraySyntax;
	}

	public static InternalBracketedDimensionListSyntax BracketedDimensionList(InternalSyntaxToken openBracketToken, InternalSeparatedSyntaxList<InternalDimensionSyntax> dimensions, InternalSyntaxToken closeBracketToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(256, openBracketToken, dimensions.Node, closeBracketToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBracketedDimensionListSyntax)internalSyntaxNode;
		}
		InternalBracketedDimensionListSyntax internalBracketedDimensionListSyntax = new InternalBracketedDimensionListSyntax(SyntaxKind.BracketedDimensionList, openBracketToken, dimensions.Node, closeBracketToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBracketedDimensionListSyntax, hash);
		}
		return internalBracketedDimensionListSyntax;
	}

	public static InternalDimensionSyntax Dimension(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(257, value, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDimensionSyntax)internalSyntaxNode;
		}
		InternalDimensionSyntax internalDimensionSyntax = new InternalDimensionSyntax(SyntaxKind.Dimension, value);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDimensionSyntax, hash);
		}
		return internalDimensionSyntax;
	}

	public static InternalOptionDataTypeSyntax OptionDataType(InternalSyntaxToken typeName, InternalOptionValuesSyntax optionValues)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(248, typeName, optionValues, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOptionDataTypeSyntax)internalSyntaxNode;
		}
		InternalOptionDataTypeSyntax internalOptionDataTypeSyntax = new InternalOptionDataTypeSyntax(SyntaxKind.OptionDataType, typeName, optionValues);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOptionDataTypeSyntax, hash);
		}
		return internalOptionDataTypeSyntax;
	}

	public static InternalSimpleNamedDataTypeSyntax SimpleNamedDataType(InternalSyntaxToken typeName)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(246, typeName, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalSimpleNamedDataTypeSyntax)internalSyntaxNode;
		}
		InternalSimpleNamedDataTypeSyntax internalSimpleNamedDataTypeSyntax = new InternalSimpleNamedDataTypeSyntax(SyntaxKind.DataType, typeName);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalSimpleNamedDataTypeSyntax, hash);
		}
		return internalSimpleNamedDataTypeSyntax;
	}

	public static InternalGenericNamedDataTypeSyntax GenericNamedDataType(InternalSyntaxToken typeName, InternalSyntaxToken ofKeyword, InternalSyntaxToken openBracketToken, InternalSeparatedSyntaxList<InternalDataTypeSyntax> typeArguments, InternalSyntaxToken closeBracketToken)
	{
		return new InternalGenericNamedDataTypeSyntax(SyntaxKind.GenericDataType, typeName, ofKeyword, openBracketToken, typeArguments.Node, closeBracketToken);
	}

	public static InternalTextConstDataTypeSyntax TextConstDataType(InternalSyntaxToken typeName, InternalCommaSeparatedIdentifierEqualsStringListSyntax multilanguage)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(249, typeName, multilanguage, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTextConstDataTypeSyntax)internalSyntaxNode;
		}
		InternalTextConstDataTypeSyntax internalTextConstDataTypeSyntax = new InternalTextConstDataTypeSyntax(SyntaxKind.TextConstDataType, typeName, multilanguage);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTextConstDataTypeSyntax, hash);
		}
		return internalTextConstDataTypeSyntax;
	}

	public static InternalLabelDataTypeSyntax LabelDataType(InternalSyntaxToken typeName, InternalLabelSyntax label)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(250, typeName, label, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalLabelDataTypeSyntax)internalSyntaxNode;
		}
		InternalLabelDataTypeSyntax internalLabelDataTypeSyntax = new InternalLabelDataTypeSyntax(SyntaxKind.LabelDataType, typeName, label);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalLabelDataTypeSyntax, hash);
		}
		return internalLabelDataTypeSyntax;
	}

	public static InternalDotNetDataTypeSyntax DotNetDataType(InternalSyntaxToken typeName)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(251, typeName, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDotNetDataTypeSyntax)internalSyntaxNode;
		}
		InternalDotNetDataTypeSyntax internalDotNetDataTypeSyntax = new InternalDotNetDataTypeSyntax(SyntaxKind.DotNetDataType, typeName);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDotNetDataTypeSyntax, hash);
		}
		return internalDotNetDataTypeSyntax;
	}

	public static InternalLengthDataTypeSyntax LengthDataType(InternalSyntaxToken typeName, InternalSyntaxToken openBracketToken, InternalSyntaxToken length, InternalSyntaxToken closeBracketToken)
	{
		return new InternalLengthDataTypeSyntax(SyntaxKind.LengthDataType, typeName, openBracketToken, length, closeBracketToken);
	}

	public static InternalSubtypedDataTypeSyntax SubtypedDataType(InternalSyntaxToken typeName, InternalObjectNameOrIdSyntax subtype)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(253, typeName, subtype, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalSubtypedDataTypeSyntax)internalSyntaxNode;
		}
		InternalSubtypedDataTypeSyntax internalSubtypedDataTypeSyntax = new InternalSubtypedDataTypeSyntax(SyntaxKind.SubtypedDataType, typeName, subtype);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalSubtypedDataTypeSyntax, hash);
		}
		return internalSubtypedDataTypeSyntax;
	}

	public static InternalEnumDataTypeSyntax EnumDataType(InternalSyntaxToken typeName, InternalIdentifierNameSyntax enumTypeName)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(254, typeName, enumTypeName, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalEnumDataTypeSyntax)internalSyntaxNode;
		}
		InternalEnumDataTypeSyntax internalEnumDataTypeSyntax = new InternalEnumDataTypeSyntax(SyntaxKind.EnumDataType, typeName, enumTypeName);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalEnumDataTypeSyntax, hash);
		}
		return internalEnumDataTypeSyntax;
	}

	public static InternalParameterListSyntax ParameterList(InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalParameterSyntax> parameters, InternalSyntaxToken closeParenthesisToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(233, openParenthesisToken, parameters.Node, closeParenthesisToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalParameterListSyntax)internalSyntaxNode;
		}
		InternalParameterListSyntax internalParameterListSyntax = new InternalParameterListSyntax(SyntaxKind.ParameterList, openParenthesisToken, parameters.Node, closeParenthesisToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalParameterListSyntax, hash);
		}
		return internalParameterListSyntax;
	}

	public static InternalTriggerDeclarationSyntax TriggerDeclaration(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalSyntaxToken triggerKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body)
	{
		return new InternalTriggerDeclarationSyntax(SyntaxKind.TriggerDeclaration, attributes.Node, triggerKeyword, name, parameterList, returnValue, semicolonToken, variables, body);
	}

	public static InternalEventTriggerDeclarationSyntax EventTriggerDeclaration(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalSyntaxToken triggerKeyword, InternalIdentifierNameSyntax publisher, InternalSyntaxToken colonColonToken, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body)
	{
		return new InternalEventTriggerDeclarationSyntax(SyntaxKind.EventTriggerDeclaration, attributes.Node, triggerKeyword, publisher, colonColonToken, name, parameterList, returnValue, semicolonToken, variables, body);
	}

	public static InternalMethodDeclarationSyntax MethodDeclaration(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalSyntaxToken localKeyword, InternalSyntaxToken procedureKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body)
	{
		return new InternalMethodDeclarationSyntax(SyntaxKind.MethodDeclaration, attributes.Node, localKeyword, procedureKeyword, name, parameterList, returnValue, semicolonToken, variables, body);
	}

	public static InternalEventDeclarationSyntax EventDeclaration(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalSyntaxToken eventKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalSyntaxToken semicolonToken)
	{
		return new InternalEventDeclarationSyntax(SyntaxKind.EventDeclaration, attributes.Node, eventKeyword, name, parameterList, semicolonToken);
	}

	public static InternalBlockSyntax Block(InternalSyntaxToken beginKeywordToken, InternalSyntaxList<InternalStatementSyntax> statements, InternalSyntaxToken endKeywordToken, InternalSyntaxToken semicolonToken)
	{
		return new InternalBlockSyntax(SyntaxKind.Block, beginKeywordToken, statements.Node, endKeywordToken, semicolonToken);
	}

	public static InternalIfStatementSyntax IfStatement(InternalSyntaxToken ifKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken thenKeywordToken, InternalStatementSyntax statement, InternalSyntaxToken elseKeywordToken, InternalStatementSyntax elseStatement)
	{
		return new InternalIfStatementSyntax(SyntaxKind.IfStatement, ifKeywordToken, condition, thenKeywordToken, statement, elseKeywordToken, elseStatement);
	}

	public static InternalOrphanedElseStatementSyntax OrphanedElseStatement(InternalSyntaxToken elseKeywordToken, InternalStatementSyntax elseStatement)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(180, elseKeywordToken, elseStatement, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOrphanedElseStatementSyntax)internalSyntaxNode;
		}
		InternalOrphanedElseStatementSyntax internalOrphanedElseStatementSyntax = new InternalOrphanedElseStatementSyntax(SyntaxKind.OrphanedElseStatement, elseKeywordToken, elseStatement);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOrphanedElseStatementSyntax, hash);
		}
		return internalOrphanedElseStatementSyntax;
	}

	public static InternalAssignmentStatementSyntax AssignmentStatement(InternalCodeExpressionSyntax target, InternalSyntaxToken assignmentToken, InternalCodeExpressionSyntax source, InternalSyntaxToken semicolonToken)
	{
		return new InternalAssignmentStatementSyntax(SyntaxKind.AssignmentStatement, target, assignmentToken, source, semicolonToken);
	}

	public static InternalCompoundAssignmentStatementSyntax CompoundAssignmentStatement(InternalCodeExpressionSyntax target, InternalSyntaxToken assignmentToken, InternalCodeExpressionSyntax source, InternalSyntaxToken semicolonToken)
	{
		return new InternalCompoundAssignmentStatementSyntax(SyntaxKind.CompoundAssignmentStatement, target, assignmentToken, source, semicolonToken);
	}

	public static InternalForStatementSyntax ForStatement(InternalSyntaxToken forKeywordToken, InternalCodeExpressionSyntax loopVariable, InternalSyntaxToken assignToken, InternalCodeExpressionSyntax initialValue, InternalSyntaxToken operatorKeywordToken, InternalCodeExpressionSyntax endValue, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
	{
		return new InternalForStatementSyntax(SyntaxKind.ForStatement, forKeywordToken, loopVariable, assignToken, initialValue, operatorKeywordToken, endValue, doKeywordToken, statement);
	}

	public static InternalWhileStatementSyntax WhileStatement(InternalSyntaxToken whileKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
	{
		return new InternalWhileStatementSyntax(SyntaxKind.WhileStatement, whileKeywordToken, condition, doKeywordToken, statement);
	}

	public static InternalWithStatementSyntax WithStatement(InternalSyntaxToken withKeywordToken, InternalCodeExpressionSyntax withId, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
	{
		return new InternalWithStatementSyntax(SyntaxKind.WithStatement, withKeywordToken, withId, doKeywordToken, statement);
	}

	public static InternalExitStatementSyntax ExitStatement(InternalSyntaxToken exitKeywordToken, InternalSyntaxToken openParenthesisToken, InternalCodeExpressionSyntax exitValue, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken semicolonToken)
	{
		return new InternalExitStatementSyntax(SyntaxKind.ExitStatement, exitKeywordToken, openParenthesisToken, exitValue, closeParenthesisToken, semicolonToken);
	}

	public static InternalExpressionStatementSyntax ExpressionStatement(InternalCodeExpressionSyntax expression, InternalSyntaxToken semicolonToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(190, expression, semicolonToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalExpressionStatementSyntax)internalSyntaxNode;
		}
		InternalExpressionStatementSyntax internalExpressionStatementSyntax = new InternalExpressionStatementSyntax(SyntaxKind.ExpressionStatement, expression, semicolonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalExpressionStatementSyntax, hash);
		}
		return internalExpressionStatementSyntax;
	}

	public static InternalRepeatStatementSyntax RepeatStatement(InternalSyntaxToken repeatKeywordToken, InternalSyntaxList<InternalStatementSyntax> statements, InternalSyntaxToken untilKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken semicolonToken)
	{
		return new InternalRepeatStatementSyntax(SyntaxKind.RepeatStatement, repeatKeywordToken, statements.Node, untilKeywordToken, condition, semicolonToken);
	}

	public static InternalForEachStatementSyntax ForEachStatement(InternalSyntaxToken forEachKeywordToken, InternalIdentifierNameSyntax iterationVariable, InternalSyntaxToken inKeywordToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
	{
		return new InternalForEachStatementSyntax(SyntaxKind.ForEachStatement, forEachKeywordToken, iterationVariable, inKeywordToken, expression, doKeywordToken, statement);
	}

	public static InternalBreakStatementSyntax BreakStatement(InternalSyntaxToken breakKeyword, InternalSyntaxToken semicolonToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(192, breakKeyword, semicolonToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBreakStatementSyntax)internalSyntaxNode;
		}
		InternalBreakStatementSyntax internalBreakStatementSyntax = new InternalBreakStatementSyntax(SyntaxKind.BreakStatement, breakKeyword, semicolonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBreakStatementSyntax, hash);
		}
		return internalBreakStatementSyntax;
	}

	public static InternalAssertErrorStatementSyntax AssertErrorStatement(InternalSyntaxToken assertErrorKeywordToken, InternalStatementSyntax statement)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(191, assertErrorKeywordToken, statement, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalAssertErrorStatementSyntax)internalSyntaxNode;
		}
		InternalAssertErrorStatementSyntax internalAssertErrorStatementSyntax = new InternalAssertErrorStatementSyntax(SyntaxKind.AssertErrorStatement, assertErrorKeywordToken, statement);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalAssertErrorStatementSyntax, hash);
		}
		return internalAssertErrorStatementSyntax;
	}

	public static InternalCaseStatementSyntax CaseStatement(InternalSyntaxToken caseKeywordToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken ofKeywordToken, InternalSyntaxList<InternalCaseLineSyntax> caseLines, InternalCaseElseSyntax caseElse, InternalSyntaxToken endKeywordToken, InternalSyntaxToken semicolonToken)
	{
		return new InternalCaseStatementSyntax(SyntaxKind.CaseStatement, caseKeywordToken, expression, ofKeywordToken, caseLines.Node, caseElse, endKeywordToken, semicolonToken);
	}

	public static InternalCaseLineSyntax CaseLine(InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> expressions, InternalSyntaxToken colonToken, InternalStatementSyntax statement)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(225, expressions.Node, colonToken, statement, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCaseLineSyntax)internalSyntaxNode;
		}
		InternalCaseLineSyntax internalCaseLineSyntax = new InternalCaseLineSyntax(SyntaxKind.CaseLine, expressions.Node, colonToken, statement);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCaseLineSyntax, hash);
		}
		return internalCaseLineSyntax;
	}

	public static InternalCaseElseSyntax CaseElse(InternalSyntaxToken elseKeywordToken, InternalSyntaxList<InternalStatementSyntax> elseStatements)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(226, elseKeywordToken, elseStatements.Node, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCaseElseSyntax)internalSyntaxNode;
		}
		InternalCaseElseSyntax internalCaseElseSyntax = new InternalCaseElseSyntax(SyntaxKind.CaseElse, elseKeywordToken, elseStatements.Node);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCaseElseSyntax, hash);
		}
		return internalCaseElseSyntax;
	}

	public static InternalEmptyStatementSyntax EmptyStatement(InternalSyntaxToken semicolonToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(189, semicolonToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalEmptyStatementSyntax)internalSyntaxNode;
		}
		InternalEmptyStatementSyntax internalEmptyStatementSyntax = new InternalEmptyStatementSyntax(SyntaxKind.EmptyStatement, semicolonToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalEmptyStatementSyntax, hash);
		}
		return internalEmptyStatementSyntax;
	}

	public static InternalParenthesizedExpressionSyntax ParenthesizedExpression(InternalSyntaxToken openParenthesisToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken closeParenthesisToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(193, openParenthesisToken, expression, closeParenthesisToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalParenthesizedExpressionSyntax)internalSyntaxNode;
		}
		InternalParenthesizedExpressionSyntax internalParenthesizedExpressionSyntax = new InternalParenthesizedExpressionSyntax(SyntaxKind.ParenthesizedExpression, openParenthesisToken, expression, closeParenthesisToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalParenthesizedExpressionSyntax, hash);
		}
		return internalParenthesizedExpressionSyntax;
	}

	public static InternalElementAccessExpressionSyntax ElementAccessExpression(InternalCodeExpressionSyntax expression, InternalBracketedArgumentListSyntax argumentList)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(213, expression, argumentList, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalElementAccessExpressionSyntax)internalSyntaxNode;
		}
		InternalElementAccessExpressionSyntax internalElementAccessExpressionSyntax = new InternalElementAccessExpressionSyntax(SyntaxKind.ArrayIndexExpression, expression, argumentList);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalElementAccessExpressionSyntax, hash);
		}
		return internalElementAccessExpressionSyntax;
	}

	public static InternalBracketedArgumentListSyntax BracketedArgumentList(InternalSyntaxToken openBracketToken, InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> arguments, InternalSyntaxToken closeBracketToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(224, openBracketToken, arguments.Node, closeBracketToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBracketedArgumentListSyntax)internalSyntaxNode;
		}
		InternalBracketedArgumentListSyntax internalBracketedArgumentListSyntax = new InternalBracketedArgumentListSyntax(SyntaxKind.BracketedArgumentList, openBracketToken, arguments.Node, closeBracketToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBracketedArgumentListSyntax, hash);
		}
		return internalBracketedArgumentListSyntax;
	}

	public static InternalInvocationExpressionSyntax InvocationExpression(InternalCodeExpressionSyntax expression, InternalArgumentListSyntax argumentList)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(214, expression, argumentList, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInvocationExpressionSyntax)internalSyntaxNode;
		}
		InternalInvocationExpressionSyntax internalInvocationExpressionSyntax = new InternalInvocationExpressionSyntax(SyntaxKind.InvocationExpression, expression, argumentList);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInvocationExpressionSyntax, hash);
		}
		return internalInvocationExpressionSyntax;
	}

	public static InternalArgumentListSyntax ArgumentList(InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> arguments, InternalSyntaxToken closeParenthesisToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(223, openParenthesisToken, arguments.Node, closeParenthesisToken, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalArgumentListSyntax)internalSyntaxNode;
		}
		InternalArgumentListSyntax internalArgumentListSyntax = new InternalArgumentListSyntax(SyntaxKind.ArgumentList, openParenthesisToken, arguments.Node, closeParenthesisToken);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalArgumentListSyntax, hash);
		}
		return internalArgumentListSyntax;
	}

	public static InternalUnaryExpressionSyntax UnaryExpression(SyntaxKind kind, InternalSyntaxToken operatorToken, InternalCodeExpressionSyntax expression)
	{
		if ((uint)(kind - 210) > 2u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode((int)kind, operatorToken, expression, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalUnaryExpressionSyntax)internalSyntaxNode;
		}
		InternalUnaryExpressionSyntax internalUnaryExpressionSyntax = new InternalUnaryExpressionSyntax(kind, operatorToken, expression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalUnaryExpressionSyntax, hash);
		}
		return internalUnaryExpressionSyntax;
	}

	public static InternalBinaryExpressionSyntax BinaryExpression(SyntaxKind kind, InternalCodeExpressionSyntax left, InternalSyntaxToken operatorToken, InternalCodeExpressionSyntax right)
	{
		if ((uint)(kind - 194) > 15u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode((int)kind, left, operatorToken, right, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBinaryExpressionSyntax)internalSyntaxNode;
		}
		InternalBinaryExpressionSyntax internalBinaryExpressionSyntax = new InternalBinaryExpressionSyntax(kind, left, operatorToken, right);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBinaryExpressionSyntax, hash);
		}
		return internalBinaryExpressionSyntax;
	}

	public static InternalInListExpressionSyntax InListExpression(InternalCodeExpressionSyntax expression, InternalSyntaxToken inKeywordToken, InternalBracketedArgumentListSyntax listExpression)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(217, expression, inKeywordToken, listExpression, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInListExpressionSyntax)internalSyntaxNode;
		}
		InternalInListExpressionSyntax internalInListExpressionSyntax = new InternalInListExpressionSyntax(SyntaxKind.InListExpression, expression, inKeywordToken, listExpression);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInListExpressionSyntax, hash);
		}
		return internalInListExpressionSyntax;
	}

	public static InternalMemberAccessExpressionSyntax MemberAccessExpression(InternalCodeExpressionSyntax expression, InternalSyntaxToken dotToken, InternalIdentifierNameSyntax name)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(215, expression, dotToken, name, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalMemberAccessExpressionSyntax)internalSyntaxNode;
		}
		InternalMemberAccessExpressionSyntax internalMemberAccessExpressionSyntax = new InternalMemberAccessExpressionSyntax(SyntaxKind.MemberAccessExpression, expression, dotToken, name);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalMemberAccessExpressionSyntax, hash);
		}
		return internalMemberAccessExpressionSyntax;
	}

	public static InternalOptionAccessExpressionSyntax OptionAccessExpression(InternalCodeExpressionSyntax expression, InternalSyntaxToken colonColonToken, InternalIdentifierNameSyntax name)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(216, expression, colonColonToken, name, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOptionAccessExpressionSyntax)internalSyntaxNode;
		}
		InternalOptionAccessExpressionSyntax internalOptionAccessExpressionSyntax = new InternalOptionAccessExpressionSyntax(SyntaxKind.OptionAccessExpression, expression, colonColonToken, name);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOptionAccessExpressionSyntax, hash);
		}
		return internalOptionAccessExpressionSyntax;
	}

	public static InternalIdentifierNameSyntax IdentifierName(InternalSyntaxToken identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(220, identifier, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIdentifierNameSyntax)internalSyntaxNode;
		}
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = new InternalIdentifierNameSyntax(SyntaxKind.IdentifierName, identifier);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIdentifierNameSyntax, hash);
		}
		return internalIdentifierNameSyntax;
	}

	public static InternalQualifiedNameSyntax QualifiedName(InternalNameSyntax left, InternalSyntaxToken dotToken, InternalSimpleNameSyntax right)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(221, left, dotToken, right, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalQualifiedNameSyntax)internalSyntaxNode;
		}
		InternalQualifiedNameSyntax internalQualifiedNameSyntax = new InternalQualifiedNameSyntax(SyntaxKind.QualifiedName, left, dotToken, right);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalQualifiedNameSyntax, hash);
		}
		return internalQualifiedNameSyntax;
	}

	public static InternalIdentifierEqualsStringSyntax IdentifierEqualsString(InternalSyntaxToken identifier, InternalSyntaxToken equalsToken, InternalStringLiteralValueSyntax stringLiteral)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(375, identifier, equalsToken, stringLiteral, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIdentifierEqualsStringSyntax)internalSyntaxNode;
		}
		InternalIdentifierEqualsStringSyntax internalIdentifierEqualsStringSyntax = new InternalIdentifierEqualsStringSyntax(SyntaxKind.IdentifierEqualsString, identifier, equalsToken, stringLiteral);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIdentifierEqualsStringSyntax, hash);
		}
		return internalIdentifierEqualsStringSyntax;
	}

	public static InternalIdentifierEqualsLiteralSyntax IdentifierEqualsLiteral(InternalSyntaxToken identifier, InternalSyntaxToken equalsToken, InternalLiteralValueSyntax literal)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(376, identifier, equalsToken, literal, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIdentifierEqualsLiteralSyntax)internalSyntaxNode;
		}
		InternalIdentifierEqualsLiteralSyntax internalIdentifierEqualsLiteralSyntax = new InternalIdentifierEqualsLiteralSyntax(SyntaxKind.IdentifierEqualsLiteral, identifier, equalsToken, literal);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIdentifierEqualsLiteralSyntax, hash);
		}
		return internalIdentifierEqualsLiteralSyntax;
	}

	public static InternalSkippedTokensTriviaSyntax SkippedTokensTrivia(InternalSyntaxNode tokens)
	{
		return new InternalSkippedTokensTriviaSyntax(SyntaxKind.SkippedTokensTrivia, tokens);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506: AvoidExcessiveClassCoupling")]
	internal static IEnumerable<Type> GetNodeTypes()
	{
		return new Type[223]
		{
			typeof(InternalObjectCompilationUnitSyntax),
			typeof(InternalPropertyListSyntax),
			typeof(InternalProfileSyntax),
			typeof(InternalEnumTypeSyntax),
			typeof(InternalEnumValueSyntax),
			typeof(InternalEnumExtensionTypeSyntax),
			typeof(InternalDotNetPackageSyntax),
			typeof(InternalDotNetAssemblySyntax),
			typeof(InternalDotNetTypeDeclarationSyntax),
			typeof(InternalCodeunitSyntax),
			typeof(InternalTableSyntax),
			typeof(InternalFieldListSyntax),
			typeof(InternalFieldExtensionListSyntax),
			typeof(InternalFieldSyntax),
			typeof(InternalFieldModificationSyntax),
			typeof(InternalKeyListSyntax),
			typeof(InternalKeySyntax),
			typeof(InternalFieldGroupListSyntax),
			typeof(InternalFieldGroupExtensionListSyntax),
			typeof(InternalFieldGroupAddChangeSyntax),
			typeof(InternalFieldGroupSyntax),
			typeof(InternalPageSyntax),
			typeof(InternalPageLayoutSyntax),
			typeof(InternalPageFieldSyntax),
			typeof(InternalPageLabelSyntax),
			typeof(InternalPageGroupSyntax),
			typeof(InternalPagePartSyntax),
			typeof(InternalPageSystemPartSyntax),
			typeof(InternalPageChartPartSyntax),
			typeof(InternalPageUserControlSyntax),
			typeof(InternalPageAreaSyntax),
			typeof(InternalPageActionListSyntax),
			typeof(InternalPageViewListSyntax),
			typeof(InternalPageViewSyntax),
			typeof(InternalGroupActionListSyntax),
			typeof(InternalPageActionGroupSyntax),
			typeof(InternalPageActionAreaSyntax),
			typeof(InternalPageActionSyntax),
			typeof(InternalPageActionSeparatorSyntax),
			typeof(InternalPageExtensionSyntax),
			typeof(InternalPageCustomizationSyntax),
			typeof(InternalTableExtensionSyntax),
			typeof(InternalPageExtensionLayoutSyntax),
			typeof(InternalControlAddChangeSyntax),
			typeof(InternalControlMoveChangeSyntax),
			typeof(InternalControlModifyChangeSyntax),
			typeof(InternalPageExtensionViewListSyntax),
			typeof(InternalViewAddChangeSyntax),
			typeof(InternalViewMoveChangeSyntax),
			typeof(InternalViewModifyChangeSyntax),
			typeof(InternalPageExtensionActionListSyntax),
			typeof(InternalActionAddChangeSyntax),
			typeof(InternalActionMoveChangeSyntax),
			typeof(InternalActionModifyChangeSyntax),
			typeof(InternalQuerySyntax),
			typeof(InternalQueryElementsSyntax),
			typeof(InternalQueryDataItemSyntax),
			typeof(InternalQueryColumnSyntax),
			typeof(InternalQueryFilterSyntax),
			typeof(InternalReportSyntax),
			typeof(InternalReportDataSetSectionSyntax),
			typeof(InternalReportDataItemSyntax),
			typeof(InternalReportColumnSyntax),
			typeof(InternalReportLabelsSectionSyntax),
			typeof(InternalReportLabelMultilanguageSyntax),
			typeof(InternalReportLabelSyntax),
			typeof(InternalRequestPageSyntax),
			typeof(InternalXmlPortSyntax),
			typeof(InternalXmlPortSchemaSyntax),
			typeof(InternalXmlPortTableElementSyntax),
			typeof(InternalXmlPortFieldElementSyntax),
			typeof(InternalXmlPortFieldAttributeSyntax),
			typeof(InternalXmlPortTextElementSyntax),
			typeof(InternalXmlPortTextAttributeSyntax),
			typeof(InternalControlAddInSyntax),
			typeof(InternalMemberAttributeSyntax),
			typeof(InternalAttributeArgumentListSyntax),
			typeof(InternalInvalidAttributeArgumentSyntax),
			typeof(InternalLiteralAttributeArgumentSyntax),
			typeof(InternalMethodReferenceAttributeArgumentSyntax),
			typeof(InternalOptionAccessAttributeArgumentSyntax),
			typeof(InternalIdentifierNameOrEmptySyntax),
			typeof(InternalLiteralExpressionSyntax),
			typeof(InternalOptionValuesSyntax),
			typeof(InternalInt32SignedLiteralValueSyntax),
			typeof(InternalInt64SignedLiteralValueSyntax),
			typeof(InternalDecimalSignedLiteralValueSyntax),
			typeof(InternalBooleanLiteralValueSyntax),
			typeof(InternalStringLiteralValueSyntax),
			typeof(InternalDateLiteralValueSyntax),
			typeof(InternalTimeLiteralValueSyntax),
			typeof(InternalDateTimeLiteralValueSyntax),
			typeof(InternalInvalidPropertyValueSyntax),
			typeof(InternalDecimalPlacesPropertyValueSyntax),
			typeof(InternalOptionValuePropertyValueSyntax),
			typeof(InternalImagePropertyValueSyntax),
			typeof(InternalPageFieldReferencePropertyValueSyntax),
			typeof(InternalMultilanguagePropertyValueSyntax),
			typeof(InternalLabelPropertyValueSyntax),
			typeof(InternalMemberReferencePropertyValueSyntax),
			typeof(InternalPermissionPropertyValueSyntax),
			typeof(InternalQualifiedObjectReferencePropertyValueSyntax),
			typeof(InternalObjectReferencePropertyValueSyntax),
			typeof(InternalPermissionSyntax),
			typeof(InternalEnumPropertyValueSyntax),
			typeof(InternalIdentifierOrLiteralPropertyValueSyntax),
			typeof(InternalIdentifierOrLiteralExpressionSyntax),
			typeof(InternalObjectNameReferenceSyntax),
			typeof(InternalObjectNameOrIdSyntax),
			typeof(InternalObjectIdSyntax),
			typeof(InternalDecimalPlacesSyntax),
			typeof(InternalMemberAccessOrIdentifierPropertyValueSyntax),
			typeof(InternalPropertyNameSyntax),
			typeof(InternalCommaSeparatedPropertyValueSyntax),
			typeof(InternalCommaSeparatedStringsPropertyValueSyntax),
			typeof(InternalCommaSeparatedIdentifierEqualsStringListSyntax),
			typeof(InternalCommaSeparatedIdentifierEqualsLiteralListSyntax),
			typeof(InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax),
			typeof(InternalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax),
			typeof(InternalLabelSyntax),
			typeof(InternalOptionValuesPropertyValueSyntax),
			typeof(InternalBooleanPropertyValueSyntax),
			typeof(InternalInt32PropertyValueSyntax),
			typeof(InternalInt64PropertyValueSyntax),
			typeof(InternalDecimalPropertyValueSyntax),
			typeof(InternalTimePropertyValueSyntax),
			typeof(InternalDatePropertyValueSyntax),
			typeof(InternalDateTimePropertyValueSyntax),
			typeof(InternalStringPropertyValueSyntax),
			typeof(InternalStyleExpressionPropertyValueSyntax),
			typeof(InternalBooleanExpressionPropertyValueSyntax),
			typeof(InternalTextExpressionPropertyValueSyntax),
			typeof(InternalIntegerExpressionPropertyValueSyntax),
			typeof(InternalClientSideBooleanExpressionPropertyValueSyntax),
			typeof(InternalTableFilterPropertyValueSyntax),
			typeof(InternalOrderByPropertyValueSyntax),
			typeof(InternalOrderByExpressionSyntax),
			typeof(InternalQueryDataItemLinkPropertyValueSyntax),
			typeof(InternalQueryDataItemLinkExpressionSyntax),
			typeof(InternalReportDataItemLinkPropertyValueSyntax),
			typeof(InternalReportDataItemLinkExpressionSyntax),
			typeof(InternalTableViewPropertyValueSyntax),
			typeof(InternalFiltersPropertyValueSyntax),
			typeof(InternalSortingSyntax),
			typeof(InternalOrderSyntax),
			typeof(InternalTableRelationPropertyValueSyntax),
			typeof(InternalIfTableRelationSyntax),
			typeof(InternalTableFilterExpressionSyntax),
			typeof(InternalElseTableRelationSyntax),
			typeof(InternalWhereExpressionSyntax),
			typeof(InternalConstExpressionSyntax),
			typeof(InternalSimpleFieldExpressionSyntax),
			typeof(InternalInvalidPropertyExpressionSyntax),
			typeof(InternalTableCalculationFormulaSyntax),
			typeof(InternalFieldCalculationFormulaSyntax),
			typeof(InternalFieldFilterExpressionSyntax),
			typeof(InternalFieldUpperLimitExpressionSyntax),
			typeof(InternalFieldUpperLimitFilterExpressionSyntax),
			typeof(InternalFilterExpressionSyntax),
			typeof(InternalParenthesizedFilterExpressionValueSyntax),
			typeof(InternalUnaryFilterExpressionValueSyntax),
			typeof(InternalBinaryFilterExpressionValueSyntax),
			typeof(InternalRangeFromFilterExpressionValueSyntax),
			typeof(InternalInvalidFilterExpressionValueSyntax),
			typeof(InternalEmptyPropertySyntax),
			typeof(InternalPropertySyntax),
			typeof(InternalVarSectionSyntax),
			typeof(InternalVariableDeclarationSyntax),
			typeof(InternalParameterSyntax),
			typeof(InternalReturnValueSyntax),
			typeof(InternalSimpleTypeReferenceSyntax),
			typeof(InternalRecordTypeReferenceSyntax),
			typeof(InternalDotNetTypeReferenceSyntax),
			typeof(InternalArraySyntax),
			typeof(InternalBracketedDimensionListSyntax),
			typeof(InternalDimensionSyntax),
			typeof(InternalOptionDataTypeSyntax),
			typeof(InternalSimpleNamedDataTypeSyntax),
			typeof(InternalGenericNamedDataTypeSyntax),
			typeof(InternalTextConstDataTypeSyntax),
			typeof(InternalLabelDataTypeSyntax),
			typeof(InternalDotNetDataTypeSyntax),
			typeof(InternalLengthDataTypeSyntax),
			typeof(InternalSubtypedDataTypeSyntax),
			typeof(InternalEnumDataTypeSyntax),
			typeof(InternalParameterListSyntax),
			typeof(InternalTriggerDeclarationSyntax),
			typeof(InternalEventTriggerDeclarationSyntax),
			typeof(InternalMethodDeclarationSyntax),
			typeof(InternalEventDeclarationSyntax),
			typeof(InternalBlockSyntax),
			typeof(InternalIfStatementSyntax),
			typeof(InternalOrphanedElseStatementSyntax),
			typeof(InternalAssignmentStatementSyntax),
			typeof(InternalCompoundAssignmentStatementSyntax),
			typeof(InternalForStatementSyntax),
			typeof(InternalWhileStatementSyntax),
			typeof(InternalWithStatementSyntax),
			typeof(InternalExitStatementSyntax),
			typeof(InternalExpressionStatementSyntax),
			typeof(InternalRepeatStatementSyntax),
			typeof(InternalForEachStatementSyntax),
			typeof(InternalBreakStatementSyntax),
			typeof(InternalAssertErrorStatementSyntax),
			typeof(InternalCaseStatementSyntax),
			typeof(InternalCaseLineSyntax),
			typeof(InternalCaseElseSyntax),
			typeof(InternalEmptyStatementSyntax),
			typeof(InternalParenthesizedExpressionSyntax),
			typeof(InternalElementAccessExpressionSyntax),
			typeof(InternalBracketedArgumentListSyntax),
			typeof(InternalInvocationExpressionSyntax),
			typeof(InternalArgumentListSyntax),
			typeof(InternalUnaryExpressionSyntax),
			typeof(InternalBinaryExpressionSyntax),
			typeof(InternalInListExpressionSyntax),
			typeof(InternalMemberAccessExpressionSyntax),
			typeof(InternalOptionAccessExpressionSyntax),
			typeof(InternalIdentifierNameSyntax),
			typeof(InternalQualifiedNameSyntax),
			typeof(InternalIdentifierEqualsStringSyntax),
			typeof(InternalIdentifierEqualsLiteralSyntax),
			typeof(InternalSkippedTokensTriviaSyntax)
		};
	}

	public static InternalSyntaxToken Token(SyntaxKind kind)
	{
		return InternalSyntaxToken.Create(kind);
	}

	internal static InternalSyntaxToken Token(InternalSyntaxNode leading, SyntaxKind kind, InternalSyntaxNode trailing, LexerMode mode = LexerMode.Default)
	{
		return InternalSyntaxToken.Create(kind, leading, trailing, mode);
	}

	public static InternalSyntaxTrivia Comment(SyntaxKind kind, string comment)
	{
		DebugAssertHelper.Assert(kind == SyntaxKind.LineCommentTrivia || kind == SyntaxKind.CommentTrivia);
		return new InternalSyntaxTrivia(kind, comment);
	}

	static InternalSyntaxFactory()
	{
		space = Whitespace(" ");
		carriageReturnLineFeed = EndOfLine("\r\n");
		lineFeed = EndOfLine("\n");
		carriageReturn = EndOfLine("\r");
		elasticSpace = Whitespace(" ", elastic: true);
		elasticCarriageReturnLineFeed = EndOfLine("\r\n", elastic: true);
		elasticLineFeed = EndOfLine("\n", elastic: true);
		elasticCarriageReturn = EndOfLine("\r", elastic: true);
		ElasticZeroSpace = Whitespace(string.Empty, elastic: true);
		BeginKeyword = Keyword(SyntaxKind.BeginKeyword);
		EndKeyword = Keyword(SyntaxKind.EndKeyword);
		IfKeyword = Keyword(SyntaxKind.IfKeyword);
		ThenKeyword = Keyword(SyntaxKind.ThenKeyword);
		ElseKeyword = Keyword(SyntaxKind.ElseKeyword);
		WhileKeyword = Keyword(SyntaxKind.WhileKeyword);
		DoKeyword = Keyword(SyntaxKind.DoKeyword);
		RepeatKeyword = Keyword(SyntaxKind.RepeatKeyword);
		UntilKeyword = Keyword(SyntaxKind.UntilKeyword);
		WithKeyword = Keyword(SyntaxKind.WithKeyword);
		CaseKeyword = Keyword(SyntaxKind.CaseKeyword);
		OfKeyword = Keyword(SyntaxKind.OfKeyword);
		ForeachKeyword = Keyword(SyntaxKind.ForEachKeyword);
		InKeyword = Keyword(SyntaxKind.InKeyword);
		AssertErrorKeyword = Keyword(SyntaxKind.AssertErrorKeyword);
		ForKeyword = Keyword(SyntaxKind.ForKeyword);
		ExitKeyword = Keyword(SyntaxKind.ExitKeyword);
		VarKeyword = Keyword(SyntaxKind.VarKeyword);
		TriggerKeyword = Keyword(SyntaxKind.TriggerKeyword);
		ArrayKeyword = Keyword(SyntaxKind.ArrayKeyword);
		TemporaryKeyword = Keyword(SyntaxKind.TemporaryKeyword);
		LocalKeyword = Keyword(SyntaxKind.LocalKeyword);
		ProcedureKeyword = Keyword(SyntaxKind.ProcedureKeyword);
		FieldsKeyword = Keyword(SyntaxKind.FieldsKeyword);
		FieldKeyword = Keyword(SyntaxKind.FieldKeyword);
		AssemblyKeyword = Keyword(SyntaxKind.AssemblyKeyword);
		TypeKeyword = Keyword(SyntaxKind.TypeKeyword);
		CodeunitKeyword = Keyword(SyntaxKind.CodeunitKeyword);
		DotNetKeyword = Keyword(SyntaxKind.DotNetKeyword);
		TableKeyword = Keyword(SyntaxKind.TableKeyword);
		OpenParenToken = Token(SyntaxKind.OpenParenToken);
		CloseParenToken = Token(SyntaxKind.CloseParenToken);
		OpenBracketToken = Token(SyntaxKind.OpenBracketToken);
		CloseBracketToken = Token(SyntaxKind.CloseBracketToken);
		ColonToken = Token(SyntaxKind.ColonToken);
		ColonColonToken = Token(SyntaxKind.ColonColonToken);
		DotToken = Token(SyntaxKind.DotToken);
		SemicolonToken = Token(SyntaxKind.SemicolonToken);
		spaceCache = new ArrayElement<InternalSyntaxTrivia>[301];
		for (int i = 0; i < spaceCache.Length; i++)
		{
			spaceCache[i].Value = ((i == 1) ? Space : new InternalSyntaxTrivia(SyntaxKind.WhiteSpaceTrivia, new string(' ', i)));
		}
	}

	public static InternalSyntaxTrivia Spaces(int length)
	{
		if (length == 1)
		{
			return Space;
		}
		if (length < spaceCache.Length)
		{
			return spaceCache[length];
		}
		return new InternalSyntaxTrivia(SyntaxKind.WhiteSpaceTrivia, new string(' ', length));
	}

	internal static InternalSyntaxTrivia EndOfLine(string text, bool elastic = false)
	{
		InternalSyntaxTrivia internalSyntaxTrivia = null;
		switch (text)
		{
		case "\r":
			internalSyntaxTrivia = (elastic ? ElasticCarriageReturn : CarriageReturn);
			break;
		case "\n":
			internalSyntaxTrivia = (elastic ? ElasticLineFeed : LineFeed);
			break;
		case "\r\n":
			internalSyntaxTrivia = (elastic ? ElasticCarriageReturnLineFeed : CarriageReturnLineFeed);
			break;
		}
		if (internalSyntaxTrivia != null)
		{
			return internalSyntaxTrivia;
		}
		internalSyntaxTrivia = InternalSyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, text);
		if (!elastic)
		{
			return internalSyntaxTrivia;
		}
		return internalSyntaxTrivia.WithAnnotations<InternalSyntaxTrivia>(new SyntaxAnnotation[1] { SyntaxAnnotation.ElasticAnnotation });
	}

	public static InternalSyntaxTrivia Whitespace(string value, bool elastic = false)
	{
		InternalSyntaxTrivia internalSyntaxTrivia = InternalSyntaxTrivia.Create(SyntaxKind.WhiteSpaceTrivia, value);
		if (!elastic)
		{
			return internalSyntaxTrivia;
		}
		return internalSyntaxTrivia.WithAnnotations<InternalSyntaxTrivia>(new SyntaxAnnotation[1] { SyntaxAnnotation.ElasticAnnotation });
	}

	internal static InternalSyntaxToken MissingToken(SyntaxKind kind)
	{
		return InternalSyntaxToken.CreateMissing(kind, null, null);
	}

	internal static InternalSyntaxToken MissingToken(InternalSyntaxNode leading, SyntaxKind kind, InternalSyntaxNode trailing)
	{
		return InternalSyntaxToken.CreateMissing(kind, leading, trailing);
	}

	internal static InternalSyntaxToken Identifier(InternalSyntaxNode leading, string text, InternalSyntaxNode trailing, LexerMode mode = LexerMode.Default)
	{
		return InternalSyntaxToken.Identifier(leading, text, trailing, mode);
	}

	internal static InternalSyntaxToken Identifier(InternalSyntaxToken identifierToken)
	{
		return Identifier(identifierToken.GetLeadingTrivia(), identifierToken.GetValueText(), identifierToken.GetTrailingTrivia());
	}

	internal static InternalSyntaxToken Identifier(string text)
	{
		return InternalSyntaxToken.Identifier(text);
	}

	internal static InternalSyntaxToken Keyword(SyntaxKind kind)
	{
		return InternalSyntaxToken.Keyword(kind);
	}

	internal static InternalSyntaxToken Keyword(SyntaxKind kind, InternalSyntaxNode leading, string text, InternalSyntaxNode trailing, LexerMode mode = LexerMode.Default)
	{
		return InternalSyntaxToken.Keyword(kind, leading, text, trailing, mode);
	}

	internal static InternalSyntaxToken Literal(InternalSyntaxNode leading, string text, string value, InternalSyntaxNode trailing, LexerMode mode = LexerMode.Default)
	{
		return InternalSyntaxToken.WithValue(SyntaxKind.StringLiteralToken, text, value, leading, trailing, mode);
	}

	internal static InternalSyntaxToken Literal(InternalSyntaxNode leading, string text, int value, InternalSyntaxNode trailing, LexerMode mode = LexerMode.Default)
	{
		return InternalSyntaxToken.WithValue(SyntaxKind.Int32LiteralToken, text, value, leading, trailing, mode);
	}

	internal static InternalSyntaxToken Literal(InternalSyntaxNode leading, string text, long value, InternalSyntaxNode trailing, LexerMode mode = LexerMode.Default)
	{
		return InternalSyntaxToken.WithValue(SyntaxKind.Int64LiteralToken, text, value, leading, trailing, mode);
	}

	internal static InternalSyntaxToken Literal(InternalSyntaxNode leading, string text, decimal value, InternalSyntaxNode trailing, LexerMode mode = LexerMode.Default)
	{
		return InternalSyntaxToken.WithValue(SyntaxKind.DecimalLiteralToken, text, value, leading, trailing, mode);
	}

	internal static InternalSyntaxToken Literal(SyntaxKind kind, InternalSyntaxNode leading, string text, int value, InternalSyntaxNode trailing, LexerMode mode = LexerMode.Default)
	{
		return InternalSyntaxToken.WithValue(kind, text, value, leading, trailing, mode);
	}

	internal static InternalSyntaxToken Literal(SyntaxKind kind, InternalSyntaxNode leading, string text, DateTime value, InternalSyntaxNode trailing, LexerMode mode = LexerMode.Default)
	{
		return InternalSyntaxToken.WithValue(kind, text, value, leading, trailing, mode);
	}

	internal static InternalSyntaxToken Literal(SyntaxKind kind, InternalSyntaxNode leading, string text, bool value, InternalSyntaxNode trailing, LexerMode mode = LexerMode.Default)
	{
		return InternalSyntaxToken.WithValue(kind, text, value, leading, trailing, mode);
	}

	internal static InternalSyntaxToken BadToken(InternalSyntaxNode leading, string text, InternalSyntaxNode trailing)
	{
		return InternalSyntaxToken.WithValue(SyntaxKind.BadToken, text, text, leading, trailing);
	}
}
