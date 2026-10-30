using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ContextAwareSyntaxFactory
{
	internal LexerMode LexerMode { get; set; }

	public ContextAwareSyntaxFactory()
	{
		LexerMode = LexerMode.Default;
	}

	public InternalObjectCompilationUnitSyntax ObjectCompilationUnit(InternalSyntaxList<InternalObjectSyntax> objects, InternalSyntaxToken endOfFileToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(227, objects.Node, endOfFileToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalObjectCompilationUnitSyntax)internalSyntaxNode;
		}
		InternalObjectCompilationUnitSyntax internalObjectCompilationUnitSyntax = new InternalObjectCompilationUnitSyntax(SyntaxKind.CompilationUnit, objects.Node, endOfFileToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalObjectCompilationUnitSyntax, hash);
		}
		return internalObjectCompilationUnitSyntax;
	}

	public InternalPropertyListSyntax PropertyList(InternalSyntaxList<InternalPropertySyntaxOrEmpty> properties)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(228, properties.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalPropertyListSyntax)internalSyntaxNode;
		}
		InternalPropertyListSyntax internalPropertyListSyntax = new InternalPropertyListSyntax(SyntaxKind.PropertyList, properties.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalPropertyListSyntax, hash);
		}
		return internalPropertyListSyntax;
	}

	public InternalProfileSyntax Profile(InternalSyntaxToken profileToken, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalProfileSyntax(SyntaxKind.ProfileObject, profileToken, name, openBraceToken, propertyList, members.Node, closeBraceToken, LexerMode);
	}

	public InternalEnumTypeSyntax EnumType(InternalSyntaxToken enumToken, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalEnumValueSyntax> values, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalEnumTypeSyntax(SyntaxKind.EnumType, enumToken, objectId, name, openBraceToken, propertyList, values.Node, members.Node, closeBraceToken, LexerMode);
	}

	public InternalEnumValueSyntax EnumValue(InternalSyntaxToken enumValueToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalEnumValueSyntax(SyntaxKind.EnumValue, enumValueToken, openParenthesisToken, id, semicolonToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalEnumExtensionTypeSyntax EnumExtensionType(InternalSyntaxToken enumExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalEnumValueSyntax> values, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalEnumExtensionTypeSyntax(SyntaxKind.EnumExtensionType, enumExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, values.Node, members.Node, closeBraceToken, LexerMode);
	}

	public InternalDotNetPackageSyntax DotNetPackage(InternalSyntaxToken dotNetKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalDotNetAssemblySyntax> assemblies, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalDotNetPackageSyntax(SyntaxKind.DotNetPackage, dotNetKeyword, name, openBraceToken, propertyList, assemblies.Node, members.Node, closeBraceToken, LexerMode);
	}

	public InternalDotNetAssemblySyntax DotNetAssembly(InternalSyntaxToken assemblyKeyword, InternalSyntaxToken openParenthesisToken, InternalNameSyntax assemblyName, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalDotNetTypeDeclarationSyntax> typeDeclarations, InternalSyntaxToken closeBraceToken)
	{
		return new InternalDotNetAssemblySyntax(SyntaxKind.DotNetAssembly, assemblyKeyword, openParenthesisToken, assemblyName, closeParenthesisToken, openBraceToken, propertyList, typeDeclarations.Node, closeBraceToken, LexerMode);
	}

	public InternalDotNetTypeDeclarationSyntax DotNetTypeDeclaration(InternalSyntaxToken type, InternalSyntaxToken openParenthesisToken, InternalNameSyntax typeName, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax typeAlias, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalDotNetTypeDeclarationSyntax(SyntaxKind.DotNetTypeDeclaration, type, openParenthesisToken, typeName, semicolonToken, typeAlias, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalCodeunitSyntax Codeunit(InternalSyntaxToken codeunitKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalCodeunitSyntax(SyntaxKind.CodeunitObject, codeunitKeyword, objectId, name, openBraceToken, propertyList, members.Node, closeBraceToken, LexerMode);
	}

	public InternalTableSyntax Table(InternalSyntaxToken tableKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalFieldListSyntax fields, InternalKeyListSyntax keys, InternalFieldGroupListSyntax fieldGroups, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalTableSyntax(SyntaxKind.TableObject, tableKeyword, objectId, name, openBraceToken, propertyList, fields, keys, fieldGroups, members.Node, closeBraceToken, LexerMode);
	}

	public InternalFieldListSyntax FieldList(InternalSyntaxToken fieldsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalFieldSyntax> fields, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldListSyntax(SyntaxKind.FieldList, fieldsKeyword, openBraceToken, fields.Node, closeBraceToken, LexerMode);
	}

	public InternalFieldExtensionListSyntax FieldExtensionList(InternalSyntaxToken fieldsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalFieldBaseSyntax> fields, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldExtensionListSyntax(SyntaxKind.FieldExtensionList, fieldsKeyword, openBraceToken, fields.Node, closeBraceToken, LexerMode);
	}

	public InternalFieldSyntax Field(InternalSyntaxToken fieldKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken no, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalDataTypeSyntax type, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldSyntax(SyntaxKind.Field, fieldKeyword, openParenthesisToken, no, semicolonToken1, name, semicolonToken2, type, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken, LexerMode);
	}

	public InternalFieldModificationSyntax FieldModification(InternalSyntaxToken fieldKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldModificationSyntax(SyntaxKind.FieldModification, fieldKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken, LexerMode);
	}

	public InternalKeyListSyntax KeyList(InternalSyntaxToken keysKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalKeySyntax> keys, InternalSyntaxToken closeBraceToken)
	{
		return new InternalKeyListSyntax(SyntaxKind.KeyList, keysKeyword, openBraceToken, keys.Node, closeBraceToken, LexerMode);
	}

	public InternalKeySyntax Key(InternalSyntaxToken keyKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalKeySyntax(SyntaxKind.Key, keyKeyword, openParenthesisToken, id, semicolonToken1, name, semicolonToken2, fields.Node, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalFieldGroupListSyntax FieldGroupList(InternalSyntaxToken fieldGroupsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalFieldGroupSyntax> fieldGroups, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldGroupListSyntax(SyntaxKind.FieldGroupList, fieldGroupsKeyword, openBraceToken, fieldGroups.Node, closeBraceToken, LexerMode);
	}

	public InternalFieldGroupExtensionListSyntax FieldGroupExtensionList(InternalSyntaxToken fieldGroupsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalFieldGroupChangeBaseSyntax> changes, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldGroupExtensionListSyntax(SyntaxKind.FieldGroupExtensionList, fieldGroupsKeyword, openBraceToken, changes.Node, closeBraceToken, LexerMode);
	}

	public InternalFieldGroupAddChangeSyntax FieldGroupAddChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken1, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldGroupAddChangeSyntax(SyntaxKind.FieldGroupAddChange, changeKeyword, openParenthesisToken, anchor, semicolonToken1, fields.Node, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalFieldGroupSyntax FieldGroup(InternalSyntaxToken fieldGroupKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalFieldGroupSyntax(SyntaxKind.FieldGroup, fieldGroupKeyword, openParenthesisToken, id, semicolonToken1, name, semicolonToken2, fields.Node, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalPageSyntax Page(InternalSyntaxToken pageKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageLayoutSyntax layout, InternalPageActionListSyntax actions, InternalPageViewListSyntax views, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageSyntax(SyntaxKind.PageObject, pageKeyword, objectId, name, openBraceToken, propertyList, layout, actions, views, members.Node, closeBraceToken, LexerMode);
	}

	public InternalPageLayoutSyntax PageLayout(InternalSyntaxToken layoutKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalPageAreaSyntax> areas, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageLayoutSyntax(SyntaxKind.PageLayout, layoutKeyword, openBraceToken, areas.Node, closeBraceToken, LexerMode);
	}

	public InternalPageFieldSyntax PageField(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageFieldSyntax(SyntaxKind.PageField, controlKeyword, openParenthesisToken, name, semicolonToken, expression, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken, LexerMode);
	}

	public InternalPageLabelSyntax PageLabel(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageLabelSyntax(SyntaxKind.PageLabel, controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalPageGroupSyntax PageGroup(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalControlBaseSyntax> controls, InternalGroupActionListSyntax actionList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageGroupSyntax(SyntaxKind.PageGroup, controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, controls.Node, actionList, closeBraceToken, LexerMode);
	}

	public InternalPagePartSyntax PagePart(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax partName, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPagePartSyntax(SyntaxKind.PagePart, controlKeyword, openParenthesisToken, name, semicolonToken, partName, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalPageSystemPartSyntax PageSystemPart(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax systemPartType, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageSystemPartSyntax(SyntaxKind.PageSystemPart, controlKeyword, openParenthesisToken, name, semicolonToken, systemPartType, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalPageChartPartSyntax PageChartPart(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax chartPartType, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageChartPartSyntax(SyntaxKind.PageChartPart, controlKeyword, openParenthesisToken, name, semicolonToken, chartPartType, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalPageUserControlSyntax PageUserControl(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameReferenceSyntax controlAddIn, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageUserControlSyntax(SyntaxKind.PageUserControl, controlKeyword, openParenthesisToken, name, semicolonToken, controlAddIn, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken, LexerMode);
	}

	public InternalPageAreaSyntax PageArea(InternalSyntaxToken controlKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalControlBaseSyntax> controls, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageAreaSyntax(SyntaxKind.PageArea, controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, controls.Node, closeBraceToken, LexerMode);
	}

	public InternalPageActionListSyntax PageActionList(InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalPageActionAreaSyntax> areas, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageActionListSyntax(SyntaxKind.PageActionList, actionsKeyword, openBraceToken, areas.Node, closeBraceToken, LexerMode);
	}

	public InternalPageViewListSyntax PageViewList(InternalSyntaxToken viewsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalPageViewSyntax> views, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageViewListSyntax(SyntaxKind.PageViewList, viewsKeyword, openBraceToken, views.Node, closeBraceToken, LexerMode);
	}

	public InternalPageViewSyntax PageView(InternalSyntaxToken viewKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageExtensionLayoutSyntax layout, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageViewSyntax(SyntaxKind.PageView, viewKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, layout, closeBraceToken, LexerMode);
	}

	public InternalGroupActionListSyntax GroupActionList(InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalActionBaseSyntax> actions, InternalSyntaxToken closeBraceToken)
	{
		return new InternalGroupActionListSyntax(SyntaxKind.GroupActionList, actionsKeyword, openBraceToken, actions.Node, closeBraceToken, LexerMode);
	}

	public InternalPageActionGroupSyntax PageActionGroup(InternalSyntaxToken actionKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalActionBaseSyntax> actions, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageActionGroupSyntax(SyntaxKind.PageActionGroup, actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, actions.Node, closeBraceToken, LexerMode);
	}

	public InternalPageActionAreaSyntax PageActionArea(InternalSyntaxToken actionKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalActionBaseSyntax> actions, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageActionAreaSyntax(SyntaxKind.PageActionArea, actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, actions.Node, closeBraceToken, LexerMode);
	}

	public InternalPageActionSyntax PageAction(InternalSyntaxToken actionKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageActionSyntax(SyntaxKind.PageAction, actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken, LexerMode);
	}

	public InternalPageActionSeparatorSyntax PageActionSeparator(InternalSyntaxToken actionKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageActionSeparatorSyntax(SyntaxKind.PageActionSeparator, actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalPageExtensionSyntax PageExtension(InternalSyntaxToken pageExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageExtensionLayoutSyntax layout, InternalPageExtensionActionListSyntax actions, InternalPageExtensionViewListSyntax views, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageExtensionSyntax(SyntaxKind.PageExtensionObject, pageExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, members.Node, closeBraceToken, LexerMode);
	}

	public InternalPageCustomizationSyntax PageCustomization(InternalSyntaxToken pageCustomizationKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken customizesKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalPageExtensionLayoutSyntax layout, InternalPageExtensionActionListSyntax actions, InternalPageExtensionViewListSyntax views, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageCustomizationSyntax(SyntaxKind.PageCustomizationObject, pageCustomizationKeyword, objectId, name, customizesKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, members.Node, closeBraceToken, LexerMode);
	}

	public InternalTableExtensionSyntax TableExtension(InternalSyntaxToken tableExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalFieldExtensionListSyntax fields, InternalKeyListSyntax keys, InternalFieldGroupExtensionListSyntax fieldGroups, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalTableExtensionSyntax(SyntaxKind.TableExtensionObject, tableExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, fields, keys, fieldGroups, members.Node, closeBraceToken, LexerMode);
	}

	public InternalPageExtensionLayoutSyntax PageExtensionLayout(InternalSyntaxToken layoutKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalControlChangeBaseSyntax> changes, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageExtensionLayoutSyntax(SyntaxKind.PageExtensionLayout, layoutKeyword, openBraceToken, changes.Node, closeBraceToken, LexerMode);
	}

	public InternalControlAddChangeSyntax ControlAddChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalControlBaseSyntax> controls, InternalSyntaxToken closeBraceToken)
	{
		return new InternalControlAddChangeSyntax(SyntaxKind.ControlAddChange, changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, controls.Node, closeBraceToken, LexerMode);
	}

	public InternalControlMoveChangeSyntax ControlMoveChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> controls, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalControlMoveChangeSyntax(SyntaxKind.ControlMoveChange, changeKeyword, openParenthesisToken, anchor, semicolonToken, controls.Node, closeParenthesisToken, LexerMode);
	}

	public InternalControlModifyChangeSyntax ControlModifyChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalControlModifyChangeSyntax(SyntaxKind.ControlModifyChange, changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken, LexerMode);
	}

	public InternalPageExtensionViewListSyntax PageExtensionViewList(InternalSyntaxToken viewsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalViewChangeBaseSyntax> changes, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageExtensionViewListSyntax(SyntaxKind.PageExtensionViewList, viewsKeyword, openBraceToken, changes.Node, closeBraceToken, LexerMode);
	}

	public InternalViewAddChangeSyntax ViewAddChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalPageViewSyntax> views, InternalSyntaxToken closeBraceToken)
	{
		return new InternalViewAddChangeSyntax(SyntaxKind.ViewAddChange, changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, views.Node, closeBraceToken, LexerMode);
	}

	public InternalViewMoveChangeSyntax ViewMoveChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> views, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalViewMoveChangeSyntax(SyntaxKind.ViewMoveChange, changeKeyword, openParenthesisToken, anchor, semicolonToken, views.Node, closeParenthesisToken, LexerMode);
	}

	public InternalViewModifyChangeSyntax ViewModifyChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalViewModifyChangeSyntax(SyntaxKind.ViewModifyChange, changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalPageExtensionActionListSyntax PageExtensionActionList(InternalSyntaxToken actionsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalActionChangeBaseSyntax> changes, InternalSyntaxToken closeBraceToken)
	{
		return new InternalPageExtensionActionListSyntax(SyntaxKind.PageExtensionActionList, actionsKeyword, openBraceToken, changes.Node, closeBraceToken, LexerMode);
	}

	public InternalActionAddChangeSyntax ActionAddChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalActionBaseSyntax> actions, InternalSyntaxToken closeBraceToken)
	{
		return new InternalActionAddChangeSyntax(SyntaxKind.ActionAddChange, changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, actions.Node, closeBraceToken, LexerMode);
	}

	public InternalActionMoveChangeSyntax ActionMoveChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> actions, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalActionMoveChangeSyntax(SyntaxKind.ActionMoveChange, changeKeyword, openParenthesisToken, anchor, semicolonToken, actions.Node, closeParenthesisToken, LexerMode);
	}

	public InternalActionModifyChangeSyntax ActionModifyChange(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalActionModifyChangeSyntax(SyntaxKind.ActionModifyChange, changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers.Node, closeBraceToken, LexerMode);
	}

	public InternalQuerySyntax Query(InternalSyntaxToken queryKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalQueryElementsSyntax elements, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalQuerySyntax(SyntaxKind.QueryObject, queryKeyword, objectId, name, openBraceToken, propertyList, elements, members.Node, closeBraceToken, LexerMode);
	}

	public InternalQueryElementsSyntax QueryElements(InternalSyntaxToken queryElementsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalQueryDataItemSyntax> dataItems, InternalSyntaxToken closeBraceToken)
	{
		return new InternalQueryElementsSyntax(SyntaxKind.QueryElements, queryElementsKeyword, openBraceToken, dataItems.Node, closeBraceToken, LexerMode);
	}

	public InternalQueryDataItemSyntax QueryDataItem(InternalSyntaxToken dataItemKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax dataItemTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalQueryDataItemElementSyntax> elements, InternalSyntaxToken closeBraceToken)
	{
		return new InternalQueryDataItemSyntax(SyntaxKind.QueryDataItem, dataItemKeyword, openParenthesisToken, name, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements.Node, closeBraceToken, LexerMode);
	}

	public InternalQueryColumnSyntax QueryColumn(InternalSyntaxToken columnKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalQueryColumnSyntax(SyntaxKind.QueryColumn, columnKeyword, openParenthesisToken, name, semicolonToken, relatedField, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalQueryFilterSyntax QueryFilter(InternalSyntaxToken filterKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalQueryFilterSyntax(SyntaxKind.QueryFilter, filterKeyword, openParenthesisToken, name, semicolonToken, relatedField, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalReportSyntax Report(InternalSyntaxToken reportKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalReportDataSetSectionSyntax dataSet, InternalRequestPageSyntax requestPage, InternalReportLabelsSectionSyntax labels, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalReportSyntax(SyntaxKind.ReportObject, reportKeyword, objectId, name, openBraceToken, propertyList, dataSet, requestPage, labels, members.Node, closeBraceToken, LexerMode);
	}

	public InternalReportDataSetSectionSyntax ReportDataSetSection(InternalSyntaxToken dataSetKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalReportDataItemSyntax> dataItems, InternalSyntaxToken closeBraceToken)
	{
		return new InternalReportDataSetSectionSyntax(SyntaxKind.ReportDataSetSection, dataSetKeyword, openBraceToken, dataItems.Node, closeBraceToken, LexerMode);
	}

	public InternalReportDataItemSyntax ReportDataItem(InternalSyntaxToken dataItemKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax dataItemTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalReportDataItemElementSyntax> elements, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalReportDataItemSyntax(SyntaxKind.ReportDataItem, dataItemKeyword, openParenthesisToken, name, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements.Node, triggers.Node, closeBraceToken, LexerMode);
	}

	public InternalReportColumnSyntax ReportColumn(InternalSyntaxToken columnKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceExpression, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		return new InternalReportColumnSyntax(SyntaxKind.ReportColumn, columnKeyword, openParenthesisToken, name, semicolonToken, sourceExpression, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken, LexerMode);
	}

	public InternalReportLabelsSectionSyntax ReportLabelsSection(InternalSyntaxToken labelsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalReportLabelBaseSyntax> labels, InternalSyntaxToken closeBraceToken)
	{
		return new InternalReportLabelsSectionSyntax(SyntaxKind.ReportLabelsSection, labelsKeyword, openBraceToken, labels.Node, closeBraceToken, LexerMode);
	}

	public InternalReportLabelMultilanguageSyntax ReportLabelMultilanguage(InternalSyntaxToken labelKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCommaSeparatedIdentifierEqualsStringListSyntax caption, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalReportLabelMultilanguageSyntax(SyntaxKind.ReportLabelMultilanguage, labelKeyword, openParenthesisToken, name, semicolonToken, caption, closeParenthesisToken, LexerMode);
	}

	public InternalReportLabelSyntax ReportLabel(InternalIdentifierNameSyntax name, InternalSyntaxToken equalsToken, InternalLabelSyntax label, InternalSyntaxToken semicolonToken)
	{
		return new InternalReportLabelSyntax(SyntaxKind.ReportLabel, name, equalsToken, label, semicolonToken, LexerMode);
	}

	public InternalRequestPageSyntax RequestPage(InternalSyntaxToken requestPageKeyword, InternalSyntaxToken openBraceToken, InternalIdentifierNameSyntax name, InternalPropertyListSyntax propertyList, InternalPageLayoutSyntax layout, InternalPageActionListSyntax actions, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalRequestPageSyntax(SyntaxKind.RequestPage, requestPageKeyword, openBraceToken, name, propertyList, layout, actions, members.Node, closeBraceToken, LexerMode);
	}

	public InternalXmlPortSyntax XmlPort(InternalSyntaxToken xmlPortKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalXmlPortSchemaSyntax xmlPortSchema, InternalRequestPageSyntax xmlPortRequestPage, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortSyntax(SyntaxKind.XmlPortObject, xmlPortKeyword, objectId, name, openBraceToken, propertyList, xmlPortSchema, xmlPortRequestPage, members.Node, closeBraceToken, LexerMode);
	}

	public InternalXmlPortSchemaSyntax XmlPortSchema(InternalSyntaxToken xmlPortSchemaKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalXmlPortNodeSyntax> xmlPortSchema, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortSchemaSyntax(SyntaxKind.XmlPortSchema, xmlPortSchemaKeyword, openBraceToken, xmlPortSchema.Node, closeBraceToken, LexerMode);
	}

	public InternalXmlPortTableElementSyntax XmlPortTableElement(InternalSyntaxToken xmlPortTableElementKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax sourceTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalXmlPortNodeSyntax> schema, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortTableElementSyntax(SyntaxKind.XmlPortTableElement, xmlPortTableElementKeyword, openParenthesisToken, name, semicolonToken, sourceTable, closeParenthesisToken, openBraceToken, propertyList, schema.Node, triggers.Node, closeBraceToken, LexerMode);
	}

	public InternalXmlPortFieldElementSyntax XmlPortFieldElement(InternalSyntaxToken xmlPortFieldElementKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalXmlPortNodeSyntax> schema, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortFieldElementSyntax(SyntaxKind.XmlPortFieldElement, xmlPortFieldElementKeyword, openParenthesisToken, name, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema.Node, triggers.Node, closeBraceToken, LexerMode);
	}

	public InternalXmlPortFieldAttributeSyntax XmlPortFieldAttribute(InternalSyntaxToken xmlPortFieldAttributeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceField, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalXmlPortNodeSyntax> schema, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortFieldAttributeSyntax(SyntaxKind.XmlPortFieldAttribute, xmlPortFieldAttributeKeyword, openParenthesisToken, name, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema.Node, triggers.Node, closeBraceToken, LexerMode);
	}

	public InternalXmlPortTextElementSyntax XmlPortTextElement(InternalSyntaxToken xmlPortTextElementKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalXmlPortNodeSyntax> schema, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortTextElementSyntax(SyntaxKind.XmlPortTextElement, xmlPortTextElementKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, schema.Node, triggers.Node, closeBraceToken, LexerMode);
	}

	public InternalXmlPortTextAttributeSyntax XmlPortTextAttribute(InternalSyntaxToken xmlPortTextAttributeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalXmlPortNodeSyntax> schema, InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers, InternalSyntaxToken closeBraceToken)
	{
		return new InternalXmlPortTextAttributeSyntax(SyntaxKind.XmlPortTextAttribute, xmlPortTextAttributeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, schema.Node, triggers.Node, closeBraceToken, LexerMode);
	}

	public InternalControlAddInSyntax ControlAddIn(InternalSyntaxToken controlAddInKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		return new InternalControlAddInSyntax(SyntaxKind.ControlAddInObject, controlAddInKeyword, name, openBraceToken, propertyList, members.Node, closeBraceToken, LexerMode);
	}

	public InternalMemberAttributeSyntax MemberAttribute(InternalSyntaxToken openBracketToken, InternalIdentifierNameSyntax name, InternalAttributeArgumentListSyntax argumentList, InternalSyntaxToken closeBracketToken)
	{
		return new InternalMemberAttributeSyntax(SyntaxKind.MemberAttribute, openBracketToken, name, argumentList, closeBracketToken, LexerMode);
	}

	public InternalAttributeArgumentListSyntax AttributeArgumentList(InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalAttributeArgumentSyntax> arguments, InternalSyntaxToken closeParenthesisToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(423, openParenthesisToken, arguments.Node, closeParenthesisToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalAttributeArgumentListSyntax)internalSyntaxNode;
		}
		InternalAttributeArgumentListSyntax internalAttributeArgumentListSyntax = new InternalAttributeArgumentListSyntax(SyntaxKind.AttributeArgumentList, openParenthesisToken, arguments.Node, closeParenthesisToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalAttributeArgumentListSyntax, hash);
		}
		return internalAttributeArgumentListSyntax;
	}

	public InternalInvalidAttributeArgumentSyntax InvalidAttributeArgument(InternalSyntaxNode invalidToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(427, invalidToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInvalidAttributeArgumentSyntax)internalSyntaxNode;
		}
		InternalInvalidAttributeArgumentSyntax internalInvalidAttributeArgumentSyntax = new InternalInvalidAttributeArgumentSyntax(SyntaxKind.InvalidAttributeArgument, invalidToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInvalidAttributeArgumentSyntax, hash);
		}
		return internalInvalidAttributeArgumentSyntax;
	}

	public InternalLiteralAttributeArgumentSyntax LiteralAttributeArgument(InternalLiteralExpressionSyntax literal)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(424, literal, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalLiteralAttributeArgumentSyntax)internalSyntaxNode;
		}
		InternalLiteralAttributeArgumentSyntax internalLiteralAttributeArgumentSyntax = new InternalLiteralAttributeArgumentSyntax(SyntaxKind.LiteralAttributeArgument, literal, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalLiteralAttributeArgumentSyntax, hash);
		}
		return internalLiteralAttributeArgumentSyntax;
	}

	public InternalMethodReferenceAttributeArgumentSyntax MethodReferenceAttributeArgument(InternalMemberAccessExpressionSyntax methodReference)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(425, methodReference, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalMethodReferenceAttributeArgumentSyntax)internalSyntaxNode;
		}
		InternalMethodReferenceAttributeArgumentSyntax internalMethodReferenceAttributeArgumentSyntax = new InternalMethodReferenceAttributeArgumentSyntax(SyntaxKind.MethodReferenceAttributeArgument, methodReference, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalMethodReferenceAttributeArgumentSyntax, hash);
		}
		return internalMethodReferenceAttributeArgumentSyntax;
	}

	public InternalOptionAccessAttributeArgumentSyntax OptionAccessAttributeArgument(InternalOptionAccessExpressionSyntax optionAccess)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(426, optionAccess, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOptionAccessAttributeArgumentSyntax)internalSyntaxNode;
		}
		InternalOptionAccessAttributeArgumentSyntax internalOptionAccessAttributeArgumentSyntax = new InternalOptionAccessAttributeArgumentSyntax(SyntaxKind.OptionAccessAttributeArgument, optionAccess, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOptionAccessAttributeArgumentSyntax, hash);
		}
		return internalOptionAccessAttributeArgumentSyntax;
	}

	public InternalIdentifierNameOrEmptySyntax IdentifierNameOrEmpty(InternalIdentifierNameSyntax identifierName, InternalSyntaxToken empty)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(222, identifierName, empty, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIdentifierNameOrEmptySyntax)internalSyntaxNode;
		}
		InternalIdentifierNameOrEmptySyntax internalIdentifierNameOrEmptySyntax = new InternalIdentifierNameOrEmptySyntax(SyntaxKind.IdentifierNameOrEmpty, identifierName, empty, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIdentifierNameOrEmptySyntax, hash);
		}
		return internalIdentifierNameOrEmptySyntax;
	}

	public InternalLiteralExpressionSyntax LiteralExpression(InternalLiteralValueSyntax literal)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(218, literal, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalLiteralExpressionSyntax)internalSyntaxNode;
		}
		InternalLiteralExpressionSyntax internalLiteralExpressionSyntax = new InternalLiteralExpressionSyntax(SyntaxKind.LiteralExpression, literal, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalLiteralExpressionSyntax, hash);
		}
		return internalLiteralExpressionSyntax;
	}

	public InternalOptionValuesSyntax OptionValues(InternalSeparatedSyntaxList<InternalIdentifierNameOrEmptySyntax> options)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(229, options.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOptionValuesSyntax)internalSyntaxNode;
		}
		InternalOptionValuesSyntax internalOptionValuesSyntax = new InternalOptionValuesSyntax(SyntaxKind.OptionValues, options.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOptionValuesSyntax, hash);
		}
		return internalOptionValuesSyntax;
	}

	public InternalInt32SignedLiteralValueSyntax Int32SignedLiteralValue(InternalSyntaxToken sign, InternalSyntaxToken number)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(170, sign, number, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInt32SignedLiteralValueSyntax)internalSyntaxNode;
		}
		InternalInt32SignedLiteralValueSyntax internalInt32SignedLiteralValueSyntax = new InternalInt32SignedLiteralValueSyntax(SyntaxKind.Int32SignedLiteralValue, sign, number, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInt32SignedLiteralValueSyntax, hash);
		}
		return internalInt32SignedLiteralValueSyntax;
	}

	public InternalInt64SignedLiteralValueSyntax Int64SignedLiteralValue(InternalSyntaxToken sign, InternalSyntaxToken number)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(171, sign, number, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInt64SignedLiteralValueSyntax)internalSyntaxNode;
		}
		InternalInt64SignedLiteralValueSyntax internalInt64SignedLiteralValueSyntax = new InternalInt64SignedLiteralValueSyntax(SyntaxKind.Int64SignedLiteralValue, sign, number, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInt64SignedLiteralValueSyntax, hash);
		}
		return internalInt64SignedLiteralValueSyntax;
	}

	public InternalDecimalSignedLiteralValueSyntax DecimalSignedLiteralValue(InternalSyntaxToken sign, InternalSyntaxToken number)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(172, sign, number, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDecimalSignedLiteralValueSyntax)internalSyntaxNode;
		}
		InternalDecimalSignedLiteralValueSyntax internalDecimalSignedLiteralValueSyntax = new InternalDecimalSignedLiteralValueSyntax(SyntaxKind.DecimalSignedLiteralValue, sign, number, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDecimalSignedLiteralValueSyntax, hash);
		}
		return internalDecimalSignedLiteralValueSyntax;
	}

	public InternalBooleanLiteralValueSyntax BooleanLiteralValue(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(169, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBooleanLiteralValueSyntax)internalSyntaxNode;
		}
		InternalBooleanLiteralValueSyntax internalBooleanLiteralValueSyntax = new InternalBooleanLiteralValueSyntax(SyntaxKind.BooleanLiteralValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBooleanLiteralValueSyntax, hash);
		}
		return internalBooleanLiteralValueSyntax;
	}

	public InternalStringLiteralValueSyntax StringLiteralValue(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(176, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalStringLiteralValueSyntax)internalSyntaxNode;
		}
		InternalStringLiteralValueSyntax internalStringLiteralValueSyntax = new InternalStringLiteralValueSyntax(SyntaxKind.StringLiteralValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalStringLiteralValueSyntax, hash);
		}
		return internalStringLiteralValueSyntax;
	}

	public InternalDateLiteralValueSyntax DateLiteralValue(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(173, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDateLiteralValueSyntax)internalSyntaxNode;
		}
		InternalDateLiteralValueSyntax internalDateLiteralValueSyntax = new InternalDateLiteralValueSyntax(SyntaxKind.DateLiteralValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDateLiteralValueSyntax, hash);
		}
		return internalDateLiteralValueSyntax;
	}

	public InternalTimeLiteralValueSyntax TimeLiteralValue(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(174, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTimeLiteralValueSyntax)internalSyntaxNode;
		}
		InternalTimeLiteralValueSyntax internalTimeLiteralValueSyntax = new InternalTimeLiteralValueSyntax(SyntaxKind.TimeLiteralValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTimeLiteralValueSyntax, hash);
		}
		return internalTimeLiteralValueSyntax;
	}

	public InternalDateTimeLiteralValueSyntax DateTimeLiteralValue(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(175, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDateTimeLiteralValueSyntax)internalSyntaxNode;
		}
		InternalDateTimeLiteralValueSyntax internalDateTimeLiteralValueSyntax = new InternalDateTimeLiteralValueSyntax(SyntaxKind.DateTimeLiteralValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDateTimeLiteralValueSyntax, hash);
		}
		return internalDateTimeLiteralValueSyntax;
	}

	public InternalInvalidPropertyValueSyntax InvalidPropertyValue(InternalSyntaxToken invalidToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(324, invalidToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInvalidPropertyValueSyntax)internalSyntaxNode;
		}
		InternalInvalidPropertyValueSyntax internalInvalidPropertyValueSyntax = new InternalInvalidPropertyValueSyntax(SyntaxKind.InvalidPropertyValue, invalidToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInvalidPropertyValueSyntax, hash);
		}
		return internalInvalidPropertyValueSyntax;
	}

	public InternalDecimalPlacesPropertyValueSyntax DecimalPlacesPropertyValue(InternalDecimalPlacesSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(384, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDecimalPlacesPropertyValueSyntax)internalSyntaxNode;
		}
		InternalDecimalPlacesPropertyValueSyntax internalDecimalPlacesPropertyValueSyntax = new InternalDecimalPlacesPropertyValueSyntax(SyntaxKind.DecimalPlacesPropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDecimalPlacesPropertyValueSyntax, hash);
		}
		return internalDecimalPlacesPropertyValueSyntax;
	}

	public InternalOptionValuePropertyValueSyntax OptionValuePropertyValue(InternalIdentifierNameSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(337, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOptionValuePropertyValueSyntax)internalSyntaxNode;
		}
		InternalOptionValuePropertyValueSyntax internalOptionValuePropertyValueSyntax = new InternalOptionValuePropertyValueSyntax(SyntaxKind.OptionValuePropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOptionValuePropertyValueSyntax, hash);
		}
		return internalOptionValuePropertyValueSyntax;
	}

	public InternalImagePropertyValueSyntax ImagePropertyValue(InternalIdentifierNameSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(336, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalImagePropertyValueSyntax)internalSyntaxNode;
		}
		InternalImagePropertyValueSyntax internalImagePropertyValueSyntax = new InternalImagePropertyValueSyntax(SyntaxKind.ImagePropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalImagePropertyValueSyntax, hash);
		}
		return internalImagePropertyValueSyntax;
	}

	public InternalPageFieldReferencePropertyValueSyntax PageFieldReferencePropertyValue(InternalIdentifierNameSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(340, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalPageFieldReferencePropertyValueSyntax)internalSyntaxNode;
		}
		InternalPageFieldReferencePropertyValueSyntax internalPageFieldReferencePropertyValueSyntax = new InternalPageFieldReferencePropertyValueSyntax(SyntaxKind.PageFieldReferencePropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalPageFieldReferencePropertyValueSyntax, hash);
		}
		return internalPageFieldReferencePropertyValueSyntax;
	}

	public InternalMultilanguagePropertyValueSyntax MultilanguagePropertyValue(InternalCommaSeparatedIdentifierEqualsStringListSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(332, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalMultilanguagePropertyValueSyntax)internalSyntaxNode;
		}
		InternalMultilanguagePropertyValueSyntax internalMultilanguagePropertyValueSyntax = new InternalMultilanguagePropertyValueSyntax(SyntaxKind.MultilanguagePropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalMultilanguagePropertyValueSyntax, hash);
		}
		return internalMultilanguagePropertyValueSyntax;
	}

	public InternalLabelPropertyValueSyntax LabelPropertyValue(InternalLabelSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(333, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalLabelPropertyValueSyntax)internalSyntaxNode;
		}
		InternalLabelPropertyValueSyntax internalLabelPropertyValueSyntax = new InternalLabelPropertyValueSyntax(SyntaxKind.LabelPropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalLabelPropertyValueSyntax, hash);
		}
		return internalLabelPropertyValueSyntax;
	}

	public InternalMemberReferencePropertyValueSyntax MemberReferencePropertyValue(InternalIdentifierNameSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(391, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalMemberReferencePropertyValueSyntax)internalSyntaxNode;
		}
		InternalMemberReferencePropertyValueSyntax internalMemberReferencePropertyValueSyntax = new InternalMemberReferencePropertyValueSyntax(SyntaxKind.MemberReferencePropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalMemberReferencePropertyValueSyntax, hash);
		}
		return internalMemberReferencePropertyValueSyntax;
	}

	public InternalPermissionPropertyValueSyntax PermissionPropertyValue(InternalSeparatedSyntaxList<InternalPermissionSyntax> permissionProperties)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(366, permissionProperties.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalPermissionPropertyValueSyntax)internalSyntaxNode;
		}
		InternalPermissionPropertyValueSyntax internalPermissionPropertyValueSyntax = new InternalPermissionPropertyValueSyntax(SyntaxKind.PermissionPropertyValue, permissionProperties.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalPermissionPropertyValueSyntax, hash);
		}
		return internalPermissionPropertyValueSyntax;
	}

	public InternalQualifiedObjectReferencePropertyValueSyntax QualifiedObjectReferencePropertyValue(InternalSyntaxToken objectType, InternalObjectNameOrIdSyntax objectNameOrId)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(377, objectType, objectNameOrId, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalQualifiedObjectReferencePropertyValueSyntax)internalSyntaxNode;
		}
		InternalQualifiedObjectReferencePropertyValueSyntax internalQualifiedObjectReferencePropertyValueSyntax = new InternalQualifiedObjectReferencePropertyValueSyntax(SyntaxKind.QualifiedObjectReferencePropertyValue, objectType, objectNameOrId, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalQualifiedObjectReferencePropertyValueSyntax, hash);
		}
		return internalQualifiedObjectReferencePropertyValueSyntax;
	}

	public InternalObjectReferencePropertyValueSyntax ObjectReferencePropertyValue(InternalObjectNameOrIdSyntax objectNameOrId)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(378, objectNameOrId, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalObjectReferencePropertyValueSyntax)internalSyntaxNode;
		}
		InternalObjectReferencePropertyValueSyntax internalObjectReferencePropertyValueSyntax = new InternalObjectReferencePropertyValueSyntax(SyntaxKind.ObjectReferencePropertyValue, objectNameOrId, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalObjectReferencePropertyValueSyntax, hash);
		}
		return internalObjectReferencePropertyValueSyntax;
	}

	public InternalPermissionSyntax Permission(InternalSyntaxToken objectType, InternalObjectNameOrIdSyntax objectReference, InternalSyntaxToken equalsToken, InternalSyntaxToken permissions)
	{
		return new InternalPermissionSyntax(SyntaxKind.PermissionValue, objectType, objectReference, equalsToken, permissions, LexerMode);
	}

	public InternalEnumPropertyValueSyntax EnumPropertyValue(InternalIdentifierNameSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(334, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalEnumPropertyValueSyntax)internalSyntaxNode;
		}
		InternalEnumPropertyValueSyntax internalEnumPropertyValueSyntax = new InternalEnumPropertyValueSyntax(SyntaxKind.EnumPropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalEnumPropertyValueSyntax, hash);
		}
		return internalEnumPropertyValueSyntax;
	}

	public InternalIdentifierOrLiteralPropertyValueSyntax IdentifierOrLiteralPropertyValue(InternalIdentifierOrLiteralExpressionSyntax identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(338, identifier, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIdentifierOrLiteralPropertyValueSyntax)internalSyntaxNode;
		}
		InternalIdentifierOrLiteralPropertyValueSyntax internalIdentifierOrLiteralPropertyValueSyntax = new InternalIdentifierOrLiteralPropertyValueSyntax(SyntaxKind.IdentifierOrLiteralPropertyValue, identifier, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIdentifierOrLiteralPropertyValueSyntax, hash);
		}
		return internalIdentifierOrLiteralPropertyValueSyntax;
	}

	public InternalIdentifierOrLiteralExpressionSyntax IdentifierOrLiteralExpression(InternalCodeExpressionSyntax identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(219, identifier, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIdentifierOrLiteralExpressionSyntax)internalSyntaxNode;
		}
		InternalIdentifierOrLiteralExpressionSyntax internalIdentifierOrLiteralExpressionSyntax = new InternalIdentifierOrLiteralExpressionSyntax(SyntaxKind.IdentifierOrLiteralExpression, identifier, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIdentifierOrLiteralExpressionSyntax, hash);
		}
		return internalIdentifierOrLiteralExpressionSyntax;
	}

	public InternalObjectNameReferenceSyntax ObjectNameReference(InternalCodeExpressionSyntax identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(232, identifier, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalObjectNameReferenceSyntax)internalSyntaxNode;
		}
		InternalObjectNameReferenceSyntax internalObjectNameReferenceSyntax = new InternalObjectNameReferenceSyntax(SyntaxKind.ObjectNameReference, identifier, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalObjectNameReferenceSyntax, hash);
		}
		return internalObjectNameReferenceSyntax;
	}

	public InternalObjectNameOrIdSyntax ObjectNameOrId(InternalCodeExpressionSyntax identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(230, identifier, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalObjectNameOrIdSyntax)internalSyntaxNode;
		}
		InternalObjectNameOrIdSyntax internalObjectNameOrIdSyntax = new InternalObjectNameOrIdSyntax(SyntaxKind.ObjectReference, identifier, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalObjectNameOrIdSyntax, hash);
		}
		return internalObjectNameOrIdSyntax;
	}

	public InternalObjectIdSyntax ObjectId(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(231, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalObjectIdSyntax)internalSyntaxNode;
		}
		InternalObjectIdSyntax internalObjectIdSyntax = new InternalObjectIdSyntax(SyntaxKind.ObjectId, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalObjectIdSyntax, hash);
		}
		return internalObjectIdSyntax;
	}

	public InternalDecimalPlacesSyntax DecimalPlaces(InternalSyntaxToken minimumToken, InternalSyntaxToken colonToken, InternalSyntaxToken maximumToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(364, minimumToken, colonToken, maximumToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDecimalPlacesSyntax)internalSyntaxNode;
		}
		InternalDecimalPlacesSyntax internalDecimalPlacesSyntax = new InternalDecimalPlacesSyntax(SyntaxKind.DecimalPlaces, minimumToken, colonToken, maximumToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDecimalPlacesSyntax, hash);
		}
		return internalDecimalPlacesSyntax;
	}

	public InternalMemberAccessOrIdentifierPropertyValueSyntax MemberAccessOrIdentifierPropertyValue(InternalCodeExpressionSyntax identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(339, identifier, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalMemberAccessOrIdentifierPropertyValueSyntax)internalSyntaxNode;
		}
		InternalMemberAccessOrIdentifierPropertyValueSyntax internalMemberAccessOrIdentifierPropertyValueSyntax = new InternalMemberAccessOrIdentifierPropertyValueSyntax(SyntaxKind.MemberAccessOrIdentifierPropertyValue, identifier, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalMemberAccessOrIdentifierPropertyValueSyntax, hash);
		}
		return internalMemberAccessOrIdentifierPropertyValueSyntax;
	}

	public InternalPropertyNameSyntax PropertyName(InternalSyntaxToken identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(323, identifier, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalPropertyNameSyntax)internalSyntaxNode;
		}
		InternalPropertyNameSyntax internalPropertyNameSyntax = new InternalPropertyNameSyntax(SyntaxKind.PropertyName, identifier, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalPropertyNameSyntax, hash);
		}
		return internalPropertyNameSyntax;
	}

	public InternalCommaSeparatedPropertyValueSyntax CommaSeparatedPropertyValue(InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> values)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(369, values.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCommaSeparatedPropertyValueSyntax)internalSyntaxNode;
		}
		InternalCommaSeparatedPropertyValueSyntax internalCommaSeparatedPropertyValueSyntax = new InternalCommaSeparatedPropertyValueSyntax(SyntaxKind.CommaSeparatedPropertyValue, values.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCommaSeparatedPropertyValueSyntax, hash);
		}
		return internalCommaSeparatedPropertyValueSyntax;
	}

	public InternalCommaSeparatedStringsPropertyValueSyntax CommaSeparatedStringsPropertyValue(InternalSeparatedSyntaxList<InternalStringLiteralValueSyntax> values)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(370, values.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCommaSeparatedStringsPropertyValueSyntax)internalSyntaxNode;
		}
		InternalCommaSeparatedStringsPropertyValueSyntax internalCommaSeparatedStringsPropertyValueSyntax = new InternalCommaSeparatedStringsPropertyValueSyntax(SyntaxKind.CommaSeparatedStringsPropertyValue, values.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCommaSeparatedStringsPropertyValueSyntax, hash);
		}
		return internalCommaSeparatedStringsPropertyValueSyntax;
	}

	public InternalCommaSeparatedIdentifierEqualsStringListSyntax CommaSeparatedIdentifierEqualsStringList(InternalSeparatedSyntaxList<InternalIdentifierEqualsStringSyntax> values)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(371, values.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCommaSeparatedIdentifierEqualsStringListSyntax)internalSyntaxNode;
		}
		InternalCommaSeparatedIdentifierEqualsStringListSyntax internalCommaSeparatedIdentifierEqualsStringListSyntax = new InternalCommaSeparatedIdentifierEqualsStringListSyntax(SyntaxKind.CommaSeparatedIdentifierEqualsStringList, values.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCommaSeparatedIdentifierEqualsStringListSyntax, hash);
		}
		return internalCommaSeparatedIdentifierEqualsStringListSyntax;
	}

	public InternalCommaSeparatedIdentifierEqualsLiteralListSyntax CommaSeparatedIdentifierEqualsLiteralList(InternalSeparatedSyntaxList<InternalIdentifierEqualsLiteralSyntax> values)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(372, values.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCommaSeparatedIdentifierEqualsLiteralListSyntax)internalSyntaxNode;
		}
		InternalCommaSeparatedIdentifierEqualsLiteralListSyntax internalCommaSeparatedIdentifierEqualsLiteralListSyntax = new InternalCommaSeparatedIdentifierEqualsLiteralListSyntax(SyntaxKind.CommaSeparatedIdentifierEqualsLiteralList, values.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCommaSeparatedIdentifierEqualsLiteralListSyntax, hash);
		}
		return internalCommaSeparatedIdentifierEqualsLiteralListSyntax;
	}

	public InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax CommaSeparatedIdentifierEqualsStringListPropertyValue(InternalCommaSeparatedIdentifierEqualsStringListSyntax list)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(373, list, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax)internalSyntaxNode;
		}
		InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax internalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax = new InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax(SyntaxKind.CommaSeparatedIdentifierEqualsStringListPropertyValue, list, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax, hash);
		}
		return internalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax;
	}

	public InternalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax CommaSeparatedIdentifierOrLiteralPropertyValue(InternalSeparatedSyntaxList<InternalIdentifierOrLiteralExpressionSyntax> values)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(374, values.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax)internalSyntaxNode;
		}
		InternalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax internalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax = new InternalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax(SyntaxKind.CommaSeparatedIdentifierOrLiteralPropertyValue, values.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax, hash);
		}
		return internalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax;
	}

	public InternalLabelSyntax Label(InternalStringLiteralValueSyntax labelText, InternalSyntaxToken commaToken, InternalCommaSeparatedIdentifierEqualsLiteralListSyntax properties)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(313, labelText, commaToken, properties, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalLabelSyntax)internalSyntaxNode;
		}
		InternalLabelSyntax internalLabelSyntax = new InternalLabelSyntax(SyntaxKind.Label, labelText, commaToken, properties, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalLabelSyntax, hash);
		}
		return internalLabelSyntax;
	}

	public InternalOptionValuesPropertyValueSyntax OptionValuesPropertyValue(InternalOptionValuesSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(365, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOptionValuesPropertyValueSyntax)internalSyntaxNode;
		}
		InternalOptionValuesPropertyValueSyntax internalOptionValuesPropertyValueSyntax = new InternalOptionValuesPropertyValueSyntax(SyntaxKind.OptionValuesPropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOptionValuesPropertyValueSyntax, hash);
		}
		return internalOptionValuesPropertyValueSyntax;
	}

	public InternalBooleanPropertyValueSyntax BooleanPropertyValue(InternalBooleanLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(335, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBooleanPropertyValueSyntax)internalSyntaxNode;
		}
		InternalBooleanPropertyValueSyntax internalBooleanPropertyValueSyntax = new InternalBooleanPropertyValueSyntax(SyntaxKind.BooleanPropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBooleanPropertyValueSyntax, hash);
		}
		return internalBooleanPropertyValueSyntax;
	}

	public InternalInt32PropertyValueSyntax Int32PropertyValue(InternalInt32SignedLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(325, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInt32PropertyValueSyntax)internalSyntaxNode;
		}
		InternalInt32PropertyValueSyntax internalInt32PropertyValueSyntax = new InternalInt32PropertyValueSyntax(SyntaxKind.Int32PropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInt32PropertyValueSyntax, hash);
		}
		return internalInt32PropertyValueSyntax;
	}

	public InternalInt64PropertyValueSyntax Int64PropertyValue(InternalInt64SignedLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(326, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInt64PropertyValueSyntax)internalSyntaxNode;
		}
		InternalInt64PropertyValueSyntax internalInt64PropertyValueSyntax = new InternalInt64PropertyValueSyntax(SyntaxKind.Int64PropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInt64PropertyValueSyntax, hash);
		}
		return internalInt64PropertyValueSyntax;
	}

	public InternalDecimalPropertyValueSyntax DecimalPropertyValue(InternalDecimalSignedLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(327, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDecimalPropertyValueSyntax)internalSyntaxNode;
		}
		InternalDecimalPropertyValueSyntax internalDecimalPropertyValueSyntax = new InternalDecimalPropertyValueSyntax(SyntaxKind.DecimalPropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDecimalPropertyValueSyntax, hash);
		}
		return internalDecimalPropertyValueSyntax;
	}

	public InternalTimePropertyValueSyntax TimePropertyValue(InternalTimeLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(329, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTimePropertyValueSyntax)internalSyntaxNode;
		}
		InternalTimePropertyValueSyntax internalTimePropertyValueSyntax = new InternalTimePropertyValueSyntax(SyntaxKind.TimePropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTimePropertyValueSyntax, hash);
		}
		return internalTimePropertyValueSyntax;
	}

	public InternalDatePropertyValueSyntax DatePropertyValue(InternalDateLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(330, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDatePropertyValueSyntax)internalSyntaxNode;
		}
		InternalDatePropertyValueSyntax internalDatePropertyValueSyntax = new InternalDatePropertyValueSyntax(SyntaxKind.DatePropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDatePropertyValueSyntax, hash);
		}
		return internalDatePropertyValueSyntax;
	}

	public InternalDateTimePropertyValueSyntax DateTimePropertyValue(InternalDateTimeLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(331, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDateTimePropertyValueSyntax)internalSyntaxNode;
		}
		InternalDateTimePropertyValueSyntax internalDateTimePropertyValueSyntax = new InternalDateTimePropertyValueSyntax(SyntaxKind.DateTimePropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDateTimePropertyValueSyntax, hash);
		}
		return internalDateTimePropertyValueSyntax;
	}

	public InternalStringPropertyValueSyntax StringPropertyValue(InternalStringLiteralValueSyntax value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(328, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalStringPropertyValueSyntax)internalSyntaxNode;
		}
		InternalStringPropertyValueSyntax internalStringPropertyValueSyntax = new InternalStringPropertyValueSyntax(SyntaxKind.StringPropertyValue, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalStringPropertyValueSyntax, hash);
		}
		return internalStringPropertyValueSyntax;
	}

	public InternalStyleExpressionPropertyValueSyntax StyleExpressionPropertyValue(InternalCodeExpressionSyntax expression)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(390, expression, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalStyleExpressionPropertyValueSyntax)internalSyntaxNode;
		}
		InternalStyleExpressionPropertyValueSyntax internalStyleExpressionPropertyValueSyntax = new InternalStyleExpressionPropertyValueSyntax(SyntaxKind.StyleExpressionPropertyValue, expression, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalStyleExpressionPropertyValueSyntax, hash);
		}
		return internalStyleExpressionPropertyValueSyntax;
	}

	public InternalBooleanExpressionPropertyValueSyntax BooleanExpressionPropertyValue(InternalCodeExpressionSyntax expression)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(380, expression, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBooleanExpressionPropertyValueSyntax)internalSyntaxNode;
		}
		InternalBooleanExpressionPropertyValueSyntax internalBooleanExpressionPropertyValueSyntax = new InternalBooleanExpressionPropertyValueSyntax(SyntaxKind.BooleanExpressionPropertyValue, expression, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBooleanExpressionPropertyValueSyntax, hash);
		}
		return internalBooleanExpressionPropertyValueSyntax;
	}

	public InternalTextExpressionPropertyValueSyntax TextExpressionPropertyValue(InternalCodeExpressionSyntax expression)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(383, expression, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTextExpressionPropertyValueSyntax)internalSyntaxNode;
		}
		InternalTextExpressionPropertyValueSyntax internalTextExpressionPropertyValueSyntax = new InternalTextExpressionPropertyValueSyntax(SyntaxKind.TextExpressionPropertyValue, expression, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTextExpressionPropertyValueSyntax, hash);
		}
		return internalTextExpressionPropertyValueSyntax;
	}

	public InternalIntegerExpressionPropertyValueSyntax IntegerExpressionPropertyValue(InternalCodeExpressionSyntax expression)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(382, expression, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIntegerExpressionPropertyValueSyntax)internalSyntaxNode;
		}
		InternalIntegerExpressionPropertyValueSyntax internalIntegerExpressionPropertyValueSyntax = new InternalIntegerExpressionPropertyValueSyntax(SyntaxKind.IntegerExpressionPropertyValue, expression, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIntegerExpressionPropertyValueSyntax, hash);
		}
		return internalIntegerExpressionPropertyValueSyntax;
	}

	public InternalClientSideBooleanExpressionPropertyValueSyntax ClientSideBooleanExpressionPropertyValue(InternalCodeExpressionSyntax expression)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(381, expression, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalClientSideBooleanExpressionPropertyValueSyntax)internalSyntaxNode;
		}
		InternalClientSideBooleanExpressionPropertyValueSyntax internalClientSideBooleanExpressionPropertyValueSyntax = new InternalClientSideBooleanExpressionPropertyValueSyntax(SyntaxKind.ClientSideBooleanExpressionPropertyValue, expression, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalClientSideBooleanExpressionPropertyValueSyntax, hash);
		}
		return internalClientSideBooleanExpressionPropertyValueSyntax;
	}

	public InternalTableFilterPropertyValueSyntax TableFilterPropertyValue(InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> conditions)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(385, conditions.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTableFilterPropertyValueSyntax)internalSyntaxNode;
		}
		InternalTableFilterPropertyValueSyntax internalTableFilterPropertyValueSyntax = new InternalTableFilterPropertyValueSyntax(SyntaxKind.TableFilterPropertyValue, conditions.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTableFilterPropertyValueSyntax, hash);
		}
		return internalTableFilterPropertyValueSyntax;
	}

	public InternalOrderByPropertyValueSyntax OrderByPropertyValue(InternalSeparatedSyntaxList<InternalOrderByExpressionSyntax> order)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(393, order.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOrderByPropertyValueSyntax)internalSyntaxNode;
		}
		InternalOrderByPropertyValueSyntax internalOrderByPropertyValueSyntax = new InternalOrderByPropertyValueSyntax(SyntaxKind.OrderByPropertyValue, order.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOrderByPropertyValueSyntax, hash);
		}
		return internalOrderByPropertyValueSyntax;
	}

	public InternalOrderByExpressionSyntax OrderByExpression(InternalSyntaxToken orderSpecifier, InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> sortingFields, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalOrderByExpressionSyntax(SyntaxKind.OrderByExpression, orderSpecifier, openParenthesisToken, sortingFields.Node, closeParenthesisToken, LexerMode);
	}

	public InternalQueryDataItemLinkPropertyValueSyntax QueryDataItemLinkPropertyValue(InternalSeparatedSyntaxList<InternalQueryDataItemLinkExpressionSyntax> joins)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(395, joins.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalQueryDataItemLinkPropertyValueSyntax)internalSyntaxNode;
		}
		InternalQueryDataItemLinkPropertyValueSyntax internalQueryDataItemLinkPropertyValueSyntax = new InternalQueryDataItemLinkPropertyValueSyntax(SyntaxKind.QueryDataItemLinkPropertyValue, joins.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalQueryDataItemLinkPropertyValueSyntax, hash);
		}
		return internalQueryDataItemLinkPropertyValueSyntax;
	}

	public InternalQueryDataItemLinkExpressionSyntax QueryDataItemLinkExpression(InternalIdentifierNameSyntax sourceField, InternalSyntaxToken equalsToken, InternalMemberAccessExpressionSyntax relatedField)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(396, sourceField, equalsToken, relatedField, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalQueryDataItemLinkExpressionSyntax)internalSyntaxNode;
		}
		InternalQueryDataItemLinkExpressionSyntax internalQueryDataItemLinkExpressionSyntax = new InternalQueryDataItemLinkExpressionSyntax(SyntaxKind.QueryDataItemLinkExpression, sourceField, equalsToken, relatedField, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalQueryDataItemLinkExpressionSyntax, hash);
		}
		return internalQueryDataItemLinkExpressionSyntax;
	}

	public InternalReportDataItemLinkPropertyValueSyntax ReportDataItemLinkPropertyValue(InternalSeparatedSyntaxList<InternalReportDataItemLinkExpressionSyntax> joins)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(397, joins.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalReportDataItemLinkPropertyValueSyntax)internalSyntaxNode;
		}
		InternalReportDataItemLinkPropertyValueSyntax internalReportDataItemLinkPropertyValueSyntax = new InternalReportDataItemLinkPropertyValueSyntax(SyntaxKind.ReportDataItemLinkPropertyValue, joins.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalReportDataItemLinkPropertyValueSyntax, hash);
		}
		return internalReportDataItemLinkPropertyValueSyntax;
	}

	public InternalReportDataItemLinkExpressionSyntax ReportDataItemLinkExpression(InternalIdentifierNameSyntax sourceField, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalReportDataItemLinkExpressionSyntax(SyntaxKind.ReportDataItemLinkExpression, sourceField, equalsToken, fieldKeywordToken, openParenthesisToken, relatedField, closeParenthesisToken, LexerMode);
	}

	public InternalTableViewPropertyValueSyntax TableViewPropertyValue(InternalSortingSyntax sorting, InternalOrderSyntax order, InternalWhereExpressionSyntax tableFilter)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(386, sorting, order, tableFilter, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTableViewPropertyValueSyntax)internalSyntaxNode;
		}
		InternalTableViewPropertyValueSyntax internalTableViewPropertyValueSyntax = new InternalTableViewPropertyValueSyntax(SyntaxKind.TableViewPropertyValue, sorting, order, tableFilter, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTableViewPropertyValueSyntax, hash);
		}
		return internalTableViewPropertyValueSyntax;
	}

	public InternalFiltersPropertyValueSyntax FiltersPropertyValue(InternalWhereExpressionSyntax tableFilter)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(387, tableFilter, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalFiltersPropertyValueSyntax)internalSyntaxNode;
		}
		InternalFiltersPropertyValueSyntax internalFiltersPropertyValueSyntax = new InternalFiltersPropertyValueSyntax(SyntaxKind.FiltersPropertyValue, tableFilter, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalFiltersPropertyValueSyntax, hash);
		}
		return internalFiltersPropertyValueSyntax;
	}

	public InternalSortingSyntax Sorting(InternalSyntaxToken sortingToken, InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> sortingFields, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalSortingSyntax(SyntaxKind.SortingExpression, sortingToken, openParenthesisToken, sortingFields.Node, closeParenthesisToken, LexerMode);
	}

	public InternalOrderSyntax Order(InternalSyntaxToken orderToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken orderBy, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalOrderSyntax(SyntaxKind.OrderExpression, orderToken, openParenthesisToken, orderBy, closeParenthesisToken, LexerMode);
	}

	public InternalTableRelationPropertyValueSyntax TableRelationPropertyValue(InternalIfTableRelationSyntax ifExpression, InternalMemberAccessOrIdentifierPropertyValueSyntax relatedTableField, InternalWhereExpressionSyntax tableFilter, InternalElseTableRelationSyntax elseExpression)
	{
		return new InternalTableRelationPropertyValueSyntax(SyntaxKind.TableRelationStatement, ifExpression, relatedTableField, tableFilter, elseExpression, LexerMode);
	}

	public InternalIfTableRelationSyntax IfTableRelation(InternalSyntaxToken ifKeywordToken, InternalTableFilterExpressionSyntax ifTableRelationCondition)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(353, ifKeywordToken, ifTableRelationCondition, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIfTableRelationSyntax)internalSyntaxNode;
		}
		InternalIfTableRelationSyntax internalIfTableRelationSyntax = new InternalIfTableRelationSyntax(SyntaxKind.IfTableRelationExpression, ifKeywordToken, ifTableRelationCondition, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIfTableRelationSyntax, hash);
		}
		return internalIfTableRelationSyntax;
	}

	public InternalTableFilterExpressionSyntax TableFilterExpression(InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> conditions, InternalSyntaxToken closeParenthesisToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(344, openParenthesisToken, conditions.Node, closeParenthesisToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTableFilterExpressionSyntax)internalSyntaxNode;
		}
		InternalTableFilterExpressionSyntax internalTableFilterExpressionSyntax = new InternalTableFilterExpressionSyntax(SyntaxKind.TableFilterExpression, openParenthesisToken, conditions.Node, closeParenthesisToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTableFilterExpressionSyntax, hash);
		}
		return internalTableFilterExpressionSyntax;
	}

	public InternalElseTableRelationSyntax ElseTableRelation(InternalSyntaxToken elseKeywordToken, InternalTableRelationPropertyValueSyntax elseTableRelationCondition)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(354, elseKeywordToken, elseTableRelationCondition, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalElseTableRelationSyntax)internalSyntaxNode;
		}
		InternalElseTableRelationSyntax internalElseTableRelationSyntax = new InternalElseTableRelationSyntax(SyntaxKind.ElseTableRelationExpression, elseKeywordToken, elseTableRelationCondition, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalElseTableRelationSyntax, hash);
		}
		return internalElseTableRelationSyntax;
	}

	public InternalWhereExpressionSyntax WhereExpression(InternalSyntaxToken whereKeywordToken, InternalTableFilterExpressionSyntax filter)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(363, whereKeywordToken, filter, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalWhereExpressionSyntax)internalSyntaxNode;
		}
		InternalWhereExpressionSyntax internalWhereExpressionSyntax = new InternalWhereExpressionSyntax(SyntaxKind.WhereExpression, whereKeywordToken, filter, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalWhereExpressionSyntax, hash);
		}
		return internalWhereExpressionSyntax;
	}

	public InternalConstExpressionSyntax ConstExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken constKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierOrLiteralExpressionSyntax identifier, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalConstExpressionSyntax(SyntaxKind.ConstExpression, leftHandSide, equalsToken, constKeywordToken, openParenthesisToken, identifier, closeParenthesisToken, LexerMode);
	}

	public InternalSimpleFieldExpressionSyntax SimpleFieldExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalSimpleFieldExpressionSyntax(SyntaxKind.SimpleFieldExpression, leftHandSide, equalsToken, fieldKeywordToken, openParenthesisToken, identifier, closeParenthesisToken, LexerMode);
	}

	public InternalInvalidPropertyExpressionSyntax InvalidPropertyExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken constOrFieldOrFilterKeywordToken, InternalSyntaxToken invalidExpression)
	{
		return new InternalInvalidPropertyExpressionSyntax(SyntaxKind.InvalidPropertyExpression, leftHandSide, equalsToken, constOrFieldOrFilterKeywordToken, invalidExpression, LexerMode);
	}

	public InternalTableCalculationFormulaSyntax TableCalculationFormula(SyntaxKind kind, InternalSyntaxToken sign, InternalSyntaxToken formulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax table, InternalWhereExpressionSyntax whereExpression, InternalSyntaxToken closeParenthesisToken)
	{
		if ((uint)(kind - 355) > 1u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		return new InternalTableCalculationFormulaSyntax(kind, sign, formulaKeywordToken, openParenthesisToken, table, whereExpression, closeParenthesisToken, LexerMode);
	}

	public InternalFieldCalculationFormulaSyntax FieldCalculationFormula(SyntaxKind kind, InternalSyntaxToken sign, InternalSyntaxToken formulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalMemberAccessExpressionSyntax field, InternalWhereExpressionSyntax whereExpression, InternalSyntaxToken closeParenthesisToken)
	{
		if ((uint)(kind - 357) > 4u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		return new InternalFieldCalculationFormulaSyntax(kind, sign, formulaKeywordToken, openParenthesisToken, field, whereExpression, closeParenthesisToken, LexerMode);
	}

	public InternalFieldFilterExpressionSyntax FieldFilterExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken1, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalFieldFilterExpressionSyntax(SyntaxKind.FieldFilterExpression, leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, filterFormulaKeywordToken, openParenthesisToken1, identifier, closeParenthesisToken1, closeParenthesisToken, LexerMode);
	}

	public InternalFieldUpperLimitExpressionSyntax FieldUpperLimitExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken upperLimitFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken1, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalFieldUpperLimitExpressionSyntax(SyntaxKind.FieldUpperLimitExpression, leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, upperLimitFormulaKeywordToken, openParenthesisToken1, identifier, closeParenthesisToken1, closeParenthesisToken, LexerMode);
	}

	public InternalFieldUpperLimitFilterExpressionSyntax FieldUpperLimitFilterExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalSyntaxToken upperLimitFormulaKeywordToken, InternalSyntaxToken openParenthesisToken2, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken3, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken3, InternalSyntaxToken closeParenthesisToken2, InternalSyntaxToken closeParenthesisToken1)
	{
		return new InternalFieldUpperLimitFilterExpressionSyntax(SyntaxKind.FieldUpperLimitFilterExpression, leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken1, upperLimitFormulaKeywordToken, openParenthesisToken2, filterFormulaKeywordToken, openParenthesisToken3, identifier, closeParenthesisToken3, closeParenthesisToken2, closeParenthesisToken1, LexerMode);
	}

	public InternalFilterExpressionSyntax FilterExpression(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalFilterExpressionValueSyntax filter, InternalSyntaxToken closeParenthesisToken)
	{
		return new InternalFilterExpressionSyntax(SyntaxKind.FilterExpression, leftHandSide, equalsToken, filterFormulaKeywordToken, openParenthesisToken, filter, closeParenthesisToken, LexerMode);
	}

	public InternalParenthesizedFilterExpressionValueSyntax ParenthesizedFilterExpressionValue(InternalSyntaxToken openParenthesisToken, InternalFilterExpressionValueSyntax expression, InternalSyntaxToken closeParenthesisToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(399, openParenthesisToken, expression, closeParenthesisToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalParenthesizedFilterExpressionValueSyntax)internalSyntaxNode;
		}
		InternalParenthesizedFilterExpressionValueSyntax internalParenthesizedFilterExpressionValueSyntax = new InternalParenthesizedFilterExpressionValueSyntax(SyntaxKind.ParenthesizedFilterExpressionValue, openParenthesisToken, expression, closeParenthesisToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalParenthesizedFilterExpressionValueSyntax, hash);
		}
		return internalParenthesizedFilterExpressionValueSyntax;
	}

	public InternalUnaryFilterExpressionValueSyntax UnaryFilterExpressionValue(SyntaxKind kind, InternalSyntaxToken operatorToken, InternalIdentifierOrLiteralExpressionSyntax identifier)
	{
		if ((uint)(kind - 400) > 5u && kind != SyntaxKind.RangeToFilterExpression)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode((int)kind, operatorToken, identifier, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalUnaryFilterExpressionValueSyntax)internalSyntaxNode;
		}
		InternalUnaryFilterExpressionValueSyntax internalUnaryFilterExpressionValueSyntax = new InternalUnaryFilterExpressionValueSyntax(kind, operatorToken, identifier, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalUnaryFilterExpressionValueSyntax, hash);
		}
		return internalUnaryFilterExpressionValueSyntax;
	}

	public InternalBinaryFilterExpressionValueSyntax BinaryFilterExpressionValue(SyntaxKind kind, InternalSyntaxNode left, InternalSyntaxToken operatorToken, InternalSyntaxNode right)
	{
		if ((uint)(kind - 406) > 2u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode((int)kind, left, operatorToken, right, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBinaryFilterExpressionValueSyntax)internalSyntaxNode;
		}
		InternalBinaryFilterExpressionValueSyntax internalBinaryFilterExpressionValueSyntax = new InternalBinaryFilterExpressionValueSyntax(kind, left, operatorToken, right, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBinaryFilterExpressionValueSyntax, hash);
		}
		return internalBinaryFilterExpressionValueSyntax;
	}

	public InternalRangeFromFilterExpressionValueSyntax RangeFromFilterExpressionValue(InternalIdentifierOrLiteralExpressionSyntax left, InternalSyntaxToken operatorToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(409, left, operatorToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalRangeFromFilterExpressionValueSyntax)internalSyntaxNode;
		}
		InternalRangeFromFilterExpressionValueSyntax internalRangeFromFilterExpressionValueSyntax = new InternalRangeFromFilterExpressionValueSyntax(SyntaxKind.RangeFromFilterExpression, left, operatorToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalRangeFromFilterExpressionValueSyntax, hash);
		}
		return internalRangeFromFilterExpressionValueSyntax;
	}

	public InternalInvalidFilterExpressionValueSyntax InvalidFilterExpressionValue(InternalSyntaxToken invalidToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(351, invalidToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInvalidFilterExpressionValueSyntax)internalSyntaxNode;
		}
		InternalInvalidFilterExpressionValueSyntax internalInvalidFilterExpressionValueSyntax = new InternalInvalidFilterExpressionValueSyntax(SyntaxKind.InvalidFilterExpressionValue, invalidToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInvalidFilterExpressionValueSyntax, hash);
		}
		return internalInvalidFilterExpressionValueSyntax;
	}

	public InternalEmptyPropertySyntax EmptyProperty(InternalSyntaxToken semicolonToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(322, semicolonToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalEmptyPropertySyntax)internalSyntaxNode;
		}
		InternalEmptyPropertySyntax internalEmptyPropertySyntax = new InternalEmptyPropertySyntax(SyntaxKind.EmptyProperty, semicolonToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalEmptyPropertySyntax, hash);
		}
		return internalEmptyPropertySyntax;
	}

	public InternalPropertySyntax Property(InternalPropertyNameSyntax name, InternalSyntaxToken equalsToken, InternalPropertyValueSyntax value, InternalSyntaxToken semicolonToken)
	{
		return new InternalPropertySyntax(SyntaxKind.Property, name, equalsToken, value, semicolonToken, LexerMode);
	}

	public InternalVarSectionSyntax VarSection(InternalSyntaxToken varKeyword, InternalSyntaxList<InternalVariableDeclarationSyntax> variables)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(235, varKeyword, variables.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalVarSectionSyntax)internalSyntaxNode;
		}
		InternalVarSectionSyntax internalVarSectionSyntax = new InternalVarSectionSyntax(SyntaxKind.VarSection, varKeyword, variables.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalVarSectionSyntax, hash);
		}
		return internalVarSectionSyntax;
	}

	public InternalVariableDeclarationSyntax VariableDeclaration(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalTypeReferenceBaseSyntax type, InternalSyntaxToken semicolonToken)
	{
		return new InternalVariableDeclarationSyntax(SyntaxKind.VariableDeclaration, attributes.Node, name, colonToken, type, semicolonToken, LexerMode);
	}

	public InternalParameterSyntax Parameter(InternalSyntaxToken varKeyword, InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalTypeReferenceBaseSyntax type)
	{
		return new InternalParameterSyntax(SyntaxKind.Parameter, varKeyword, name, colonToken, type, LexerMode);
	}

	public InternalReturnValueSyntax ReturnValue(InternalIdentifierNameSyntax name, InternalSyntaxToken colonToken, InternalDataTypeSyntax dataType)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(242, name, colonToken, dataType, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalReturnValueSyntax)internalSyntaxNode;
		}
		InternalReturnValueSyntax internalReturnValueSyntax = new InternalReturnValueSyntax(SyntaxKind.ReturnValue, name, colonToken, dataType, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalReturnValueSyntax, hash);
		}
		return internalReturnValueSyntax;
	}

	public InternalSimpleTypeReferenceSyntax SimpleTypeReference(InternalArraySyntax array, InternalDataTypeSyntax dataType)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(243, array, dataType, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalSimpleTypeReferenceSyntax)internalSyntaxNode;
		}
		InternalSimpleTypeReferenceSyntax internalSimpleTypeReferenceSyntax = new InternalSimpleTypeReferenceSyntax(SyntaxKind.SimpleTypeReference, array, dataType, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalSimpleTypeReferenceSyntax, hash);
		}
		return internalSimpleTypeReferenceSyntax;
	}

	public InternalRecordTypeReferenceSyntax RecordTypeReference(InternalArraySyntax array, InternalDataTypeSyntax dataType, InternalSyntaxToken temporary)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(244, array, dataType, temporary, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalRecordTypeReferenceSyntax)internalSyntaxNode;
		}
		InternalRecordTypeReferenceSyntax internalRecordTypeReferenceSyntax = new InternalRecordTypeReferenceSyntax(SyntaxKind.RecordTypeReference, array, dataType, temporary, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalRecordTypeReferenceSyntax, hash);
		}
		return internalRecordTypeReferenceSyntax;
	}

	public InternalDotNetTypeReferenceSyntax DotNetTypeReference(InternalArraySyntax array, InternalDataTypeSyntax dataType)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(245, array, dataType, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDotNetTypeReferenceSyntax)internalSyntaxNode;
		}
		InternalDotNetTypeReferenceSyntax internalDotNetTypeReferenceSyntax = new InternalDotNetTypeReferenceSyntax(SyntaxKind.DotNetTypeReference, array, dataType, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDotNetTypeReferenceSyntax, hash);
		}
		return internalDotNetTypeReferenceSyntax;
	}

	public InternalArraySyntax Array(InternalSyntaxToken arrayKeyword, InternalBracketedDimensionListSyntax dimensionList, InternalSyntaxToken ofKeyword)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(255, arrayKeyword, dimensionList, ofKeyword, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalArraySyntax)internalSyntaxNode;
		}
		InternalArraySyntax internalArraySyntax = new InternalArraySyntax(SyntaxKind.Array, arrayKeyword, dimensionList, ofKeyword, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalArraySyntax, hash);
		}
		return internalArraySyntax;
	}

	public InternalBracketedDimensionListSyntax BracketedDimensionList(InternalSyntaxToken openBracketToken, InternalSeparatedSyntaxList<InternalDimensionSyntax> dimensions, InternalSyntaxToken closeBracketToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(256, openBracketToken, dimensions.Node, closeBracketToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBracketedDimensionListSyntax)internalSyntaxNode;
		}
		InternalBracketedDimensionListSyntax internalBracketedDimensionListSyntax = new InternalBracketedDimensionListSyntax(SyntaxKind.BracketedDimensionList, openBracketToken, dimensions.Node, closeBracketToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBracketedDimensionListSyntax, hash);
		}
		return internalBracketedDimensionListSyntax;
	}

	public InternalDimensionSyntax Dimension(InternalSyntaxToken value)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(257, value, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDimensionSyntax)internalSyntaxNode;
		}
		InternalDimensionSyntax internalDimensionSyntax = new InternalDimensionSyntax(SyntaxKind.Dimension, value, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDimensionSyntax, hash);
		}
		return internalDimensionSyntax;
	}

	public InternalOptionDataTypeSyntax OptionDataType(InternalSyntaxToken typeName, InternalOptionValuesSyntax optionValues)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(248, typeName, optionValues, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOptionDataTypeSyntax)internalSyntaxNode;
		}
		InternalOptionDataTypeSyntax internalOptionDataTypeSyntax = new InternalOptionDataTypeSyntax(SyntaxKind.OptionDataType, typeName, optionValues, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOptionDataTypeSyntax, hash);
		}
		return internalOptionDataTypeSyntax;
	}

	public InternalSimpleNamedDataTypeSyntax SimpleNamedDataType(InternalSyntaxToken typeName)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(246, typeName, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalSimpleNamedDataTypeSyntax)internalSyntaxNode;
		}
		InternalSimpleNamedDataTypeSyntax internalSimpleNamedDataTypeSyntax = new InternalSimpleNamedDataTypeSyntax(SyntaxKind.DataType, typeName, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalSimpleNamedDataTypeSyntax, hash);
		}
		return internalSimpleNamedDataTypeSyntax;
	}

	public InternalGenericNamedDataTypeSyntax GenericNamedDataType(InternalSyntaxToken typeName, InternalSyntaxToken ofKeyword, InternalSyntaxToken openBracketToken, InternalSeparatedSyntaxList<InternalDataTypeSyntax> typeArguments, InternalSyntaxToken closeBracketToken)
	{
		return new InternalGenericNamedDataTypeSyntax(SyntaxKind.GenericDataType, typeName, ofKeyword, openBracketToken, typeArguments.Node, closeBracketToken, LexerMode);
	}

	public InternalTextConstDataTypeSyntax TextConstDataType(InternalSyntaxToken typeName, InternalCommaSeparatedIdentifierEqualsStringListSyntax multilanguage)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(249, typeName, multilanguage, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalTextConstDataTypeSyntax)internalSyntaxNode;
		}
		InternalTextConstDataTypeSyntax internalTextConstDataTypeSyntax = new InternalTextConstDataTypeSyntax(SyntaxKind.TextConstDataType, typeName, multilanguage, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalTextConstDataTypeSyntax, hash);
		}
		return internalTextConstDataTypeSyntax;
	}

	public InternalLabelDataTypeSyntax LabelDataType(InternalSyntaxToken typeName, InternalLabelSyntax label)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(250, typeName, label, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalLabelDataTypeSyntax)internalSyntaxNode;
		}
		InternalLabelDataTypeSyntax internalLabelDataTypeSyntax = new InternalLabelDataTypeSyntax(SyntaxKind.LabelDataType, typeName, label, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalLabelDataTypeSyntax, hash);
		}
		return internalLabelDataTypeSyntax;
	}

	public InternalDotNetDataTypeSyntax DotNetDataType(InternalSyntaxToken typeName)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(251, typeName, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalDotNetDataTypeSyntax)internalSyntaxNode;
		}
		InternalDotNetDataTypeSyntax internalDotNetDataTypeSyntax = new InternalDotNetDataTypeSyntax(SyntaxKind.DotNetDataType, typeName, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalDotNetDataTypeSyntax, hash);
		}
		return internalDotNetDataTypeSyntax;
	}

	public InternalLengthDataTypeSyntax LengthDataType(InternalSyntaxToken typeName, InternalSyntaxToken openBracketToken, InternalSyntaxToken length, InternalSyntaxToken closeBracketToken)
	{
		return new InternalLengthDataTypeSyntax(SyntaxKind.LengthDataType, typeName, openBracketToken, length, closeBracketToken, LexerMode);
	}

	public InternalSubtypedDataTypeSyntax SubtypedDataType(InternalSyntaxToken typeName, InternalObjectNameOrIdSyntax subtype)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(253, typeName, subtype, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalSubtypedDataTypeSyntax)internalSyntaxNode;
		}
		InternalSubtypedDataTypeSyntax internalSubtypedDataTypeSyntax = new InternalSubtypedDataTypeSyntax(SyntaxKind.SubtypedDataType, typeName, subtype, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalSubtypedDataTypeSyntax, hash);
		}
		return internalSubtypedDataTypeSyntax;
	}

	public InternalEnumDataTypeSyntax EnumDataType(InternalSyntaxToken typeName, InternalIdentifierNameSyntax enumTypeName)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(254, typeName, enumTypeName, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalEnumDataTypeSyntax)internalSyntaxNode;
		}
		InternalEnumDataTypeSyntax internalEnumDataTypeSyntax = new InternalEnumDataTypeSyntax(SyntaxKind.EnumDataType, typeName, enumTypeName, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalEnumDataTypeSyntax, hash);
		}
		return internalEnumDataTypeSyntax;
	}

	public InternalParameterListSyntax ParameterList(InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalParameterSyntax> parameters, InternalSyntaxToken closeParenthesisToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(233, openParenthesisToken, parameters.Node, closeParenthesisToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalParameterListSyntax)internalSyntaxNode;
		}
		InternalParameterListSyntax internalParameterListSyntax = new InternalParameterListSyntax(SyntaxKind.ParameterList, openParenthesisToken, parameters.Node, closeParenthesisToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalParameterListSyntax, hash);
		}
		return internalParameterListSyntax;
	}

	public InternalTriggerDeclarationSyntax TriggerDeclaration(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalSyntaxToken triggerKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body)
	{
		return new InternalTriggerDeclarationSyntax(SyntaxKind.TriggerDeclaration, attributes.Node, triggerKeyword, name, parameterList, returnValue, semicolonToken, variables, body, LexerMode);
	}

	public InternalEventTriggerDeclarationSyntax EventTriggerDeclaration(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalSyntaxToken triggerKeyword, InternalIdentifierNameSyntax publisher, InternalSyntaxToken colonColonToken, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body)
	{
		return new InternalEventTriggerDeclarationSyntax(SyntaxKind.EventTriggerDeclaration, attributes.Node, triggerKeyword, publisher, colonColonToken, name, parameterList, returnValue, semicolonToken, variables, body, LexerMode);
	}

	public InternalMethodDeclarationSyntax MethodDeclaration(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalSyntaxToken localKeyword, InternalSyntaxToken procedureKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalReturnValueSyntax returnValue, InternalSyntaxToken semicolonToken, InternalVarSectionSyntax variables, InternalBlockSyntax body)
	{
		return new InternalMethodDeclarationSyntax(SyntaxKind.MethodDeclaration, attributes.Node, localKeyword, procedureKeyword, name, parameterList, returnValue, semicolonToken, variables, body, LexerMode);
	}

	public InternalEventDeclarationSyntax EventDeclaration(InternalSyntaxList<InternalMemberAttributeSyntax> attributes, InternalSyntaxToken eventKeyword, InternalIdentifierNameSyntax name, InternalParameterListSyntax parameterList, InternalSyntaxToken semicolonToken)
	{
		return new InternalEventDeclarationSyntax(SyntaxKind.EventDeclaration, attributes.Node, eventKeyword, name, parameterList, semicolonToken, LexerMode);
	}

	public InternalBlockSyntax Block(InternalSyntaxToken beginKeywordToken, InternalSyntaxList<InternalStatementSyntax> statements, InternalSyntaxToken endKeywordToken, InternalSyntaxToken semicolonToken)
	{
		return new InternalBlockSyntax(SyntaxKind.Block, beginKeywordToken, statements.Node, endKeywordToken, semicolonToken, LexerMode);
	}

	public InternalIfStatementSyntax IfStatement(InternalSyntaxToken ifKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken thenKeywordToken, InternalStatementSyntax statement, InternalSyntaxToken elseKeywordToken, InternalStatementSyntax elseStatement)
	{
		return new InternalIfStatementSyntax(SyntaxKind.IfStatement, ifKeywordToken, condition, thenKeywordToken, statement, elseKeywordToken, elseStatement, LexerMode);
	}

	public InternalOrphanedElseStatementSyntax OrphanedElseStatement(InternalSyntaxToken elseKeywordToken, InternalStatementSyntax elseStatement)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(180, elseKeywordToken, elseStatement, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOrphanedElseStatementSyntax)internalSyntaxNode;
		}
		InternalOrphanedElseStatementSyntax internalOrphanedElseStatementSyntax = new InternalOrphanedElseStatementSyntax(SyntaxKind.OrphanedElseStatement, elseKeywordToken, elseStatement, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOrphanedElseStatementSyntax, hash);
		}
		return internalOrphanedElseStatementSyntax;
	}

	public InternalAssignmentStatementSyntax AssignmentStatement(InternalCodeExpressionSyntax target, InternalSyntaxToken assignmentToken, InternalCodeExpressionSyntax source, InternalSyntaxToken semicolonToken)
	{
		return new InternalAssignmentStatementSyntax(SyntaxKind.AssignmentStatement, target, assignmentToken, source, semicolonToken, LexerMode);
	}

	public InternalCompoundAssignmentStatementSyntax CompoundAssignmentStatement(InternalCodeExpressionSyntax target, InternalSyntaxToken assignmentToken, InternalCodeExpressionSyntax source, InternalSyntaxToken semicolonToken)
	{
		return new InternalCompoundAssignmentStatementSyntax(SyntaxKind.CompoundAssignmentStatement, target, assignmentToken, source, semicolonToken, LexerMode);
	}

	public InternalForStatementSyntax ForStatement(InternalSyntaxToken forKeywordToken, InternalCodeExpressionSyntax loopVariable, InternalSyntaxToken assignToken, InternalCodeExpressionSyntax initialValue, InternalSyntaxToken operatorKeywordToken, InternalCodeExpressionSyntax endValue, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
	{
		return new InternalForStatementSyntax(SyntaxKind.ForStatement, forKeywordToken, loopVariable, assignToken, initialValue, operatorKeywordToken, endValue, doKeywordToken, statement, LexerMode);
	}

	public InternalWhileStatementSyntax WhileStatement(InternalSyntaxToken whileKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
	{
		return new InternalWhileStatementSyntax(SyntaxKind.WhileStatement, whileKeywordToken, condition, doKeywordToken, statement, LexerMode);
	}

	public InternalWithStatementSyntax WithStatement(InternalSyntaxToken withKeywordToken, InternalCodeExpressionSyntax withId, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
	{
		return new InternalWithStatementSyntax(SyntaxKind.WithStatement, withKeywordToken, withId, doKeywordToken, statement, LexerMode);
	}

	public InternalExitStatementSyntax ExitStatement(InternalSyntaxToken exitKeywordToken, InternalSyntaxToken openParenthesisToken, InternalCodeExpressionSyntax exitValue, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken semicolonToken)
	{
		return new InternalExitStatementSyntax(SyntaxKind.ExitStatement, exitKeywordToken, openParenthesisToken, exitValue, closeParenthesisToken, semicolonToken, LexerMode);
	}

	public InternalExpressionStatementSyntax ExpressionStatement(InternalCodeExpressionSyntax expression, InternalSyntaxToken semicolonToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(190, expression, semicolonToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalExpressionStatementSyntax)internalSyntaxNode;
		}
		InternalExpressionStatementSyntax internalExpressionStatementSyntax = new InternalExpressionStatementSyntax(SyntaxKind.ExpressionStatement, expression, semicolonToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalExpressionStatementSyntax, hash);
		}
		return internalExpressionStatementSyntax;
	}

	public InternalRepeatStatementSyntax RepeatStatement(InternalSyntaxToken repeatKeywordToken, InternalSyntaxList<InternalStatementSyntax> statements, InternalSyntaxToken untilKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken semicolonToken)
	{
		return new InternalRepeatStatementSyntax(SyntaxKind.RepeatStatement, repeatKeywordToken, statements.Node, untilKeywordToken, condition, semicolonToken, LexerMode);
	}

	public InternalForEachStatementSyntax ForEachStatement(InternalSyntaxToken forEachKeywordToken, InternalIdentifierNameSyntax iterationVariable, InternalSyntaxToken inKeywordToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
	{
		return new InternalForEachStatementSyntax(SyntaxKind.ForEachStatement, forEachKeywordToken, iterationVariable, inKeywordToken, expression, doKeywordToken, statement, LexerMode);
	}

	public InternalBreakStatementSyntax BreakStatement(InternalSyntaxToken breakKeyword, InternalSyntaxToken semicolonToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(192, breakKeyword, semicolonToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBreakStatementSyntax)internalSyntaxNode;
		}
		InternalBreakStatementSyntax internalBreakStatementSyntax = new InternalBreakStatementSyntax(SyntaxKind.BreakStatement, breakKeyword, semicolonToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBreakStatementSyntax, hash);
		}
		return internalBreakStatementSyntax;
	}

	public InternalAssertErrorStatementSyntax AssertErrorStatement(InternalSyntaxToken assertErrorKeywordToken, InternalStatementSyntax statement)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(191, assertErrorKeywordToken, statement, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalAssertErrorStatementSyntax)internalSyntaxNode;
		}
		InternalAssertErrorStatementSyntax internalAssertErrorStatementSyntax = new InternalAssertErrorStatementSyntax(SyntaxKind.AssertErrorStatement, assertErrorKeywordToken, statement, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalAssertErrorStatementSyntax, hash);
		}
		return internalAssertErrorStatementSyntax;
	}

	public InternalCaseStatementSyntax CaseStatement(InternalSyntaxToken caseKeywordToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken ofKeywordToken, InternalSyntaxList<InternalCaseLineSyntax> caseLines, InternalCaseElseSyntax caseElse, InternalSyntaxToken endKeywordToken, InternalSyntaxToken semicolonToken)
	{
		return new InternalCaseStatementSyntax(SyntaxKind.CaseStatement, caseKeywordToken, expression, ofKeywordToken, caseLines.Node, caseElse, endKeywordToken, semicolonToken, LexerMode);
	}

	public InternalCaseLineSyntax CaseLine(InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> expressions, InternalSyntaxToken colonToken, InternalStatementSyntax statement)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(225, expressions.Node, colonToken, statement, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCaseLineSyntax)internalSyntaxNode;
		}
		InternalCaseLineSyntax internalCaseLineSyntax = new InternalCaseLineSyntax(SyntaxKind.CaseLine, expressions.Node, colonToken, statement, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCaseLineSyntax, hash);
		}
		return internalCaseLineSyntax;
	}

	public InternalCaseElseSyntax CaseElse(InternalSyntaxToken elseKeywordToken, InternalSyntaxList<InternalStatementSyntax> elseStatements)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(226, elseKeywordToken, elseStatements.Node, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalCaseElseSyntax)internalSyntaxNode;
		}
		InternalCaseElseSyntax internalCaseElseSyntax = new InternalCaseElseSyntax(SyntaxKind.CaseElse, elseKeywordToken, elseStatements.Node, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalCaseElseSyntax, hash);
		}
		return internalCaseElseSyntax;
	}

	public InternalEmptyStatementSyntax EmptyStatement(InternalSyntaxToken semicolonToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(189, semicolonToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalEmptyStatementSyntax)internalSyntaxNode;
		}
		InternalEmptyStatementSyntax internalEmptyStatementSyntax = new InternalEmptyStatementSyntax(SyntaxKind.EmptyStatement, semicolonToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalEmptyStatementSyntax, hash);
		}
		return internalEmptyStatementSyntax;
	}

	public InternalParenthesizedExpressionSyntax ParenthesizedExpression(InternalSyntaxToken openParenthesisToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken closeParenthesisToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(193, openParenthesisToken, expression, closeParenthesisToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalParenthesizedExpressionSyntax)internalSyntaxNode;
		}
		InternalParenthesizedExpressionSyntax internalParenthesizedExpressionSyntax = new InternalParenthesizedExpressionSyntax(SyntaxKind.ParenthesizedExpression, openParenthesisToken, expression, closeParenthesisToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalParenthesizedExpressionSyntax, hash);
		}
		return internalParenthesizedExpressionSyntax;
	}

	public InternalElementAccessExpressionSyntax ElementAccessExpression(InternalCodeExpressionSyntax expression, InternalBracketedArgumentListSyntax argumentList)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(213, expression, argumentList, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalElementAccessExpressionSyntax)internalSyntaxNode;
		}
		InternalElementAccessExpressionSyntax internalElementAccessExpressionSyntax = new InternalElementAccessExpressionSyntax(SyntaxKind.ArrayIndexExpression, expression, argumentList, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalElementAccessExpressionSyntax, hash);
		}
		return internalElementAccessExpressionSyntax;
	}

	public InternalBracketedArgumentListSyntax BracketedArgumentList(InternalSyntaxToken openBracketToken, InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> arguments, InternalSyntaxToken closeBracketToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(224, openBracketToken, arguments.Node, closeBracketToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBracketedArgumentListSyntax)internalSyntaxNode;
		}
		InternalBracketedArgumentListSyntax internalBracketedArgumentListSyntax = new InternalBracketedArgumentListSyntax(SyntaxKind.BracketedArgumentList, openBracketToken, arguments.Node, closeBracketToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBracketedArgumentListSyntax, hash);
		}
		return internalBracketedArgumentListSyntax;
	}

	public InternalInvocationExpressionSyntax InvocationExpression(InternalCodeExpressionSyntax expression, InternalArgumentListSyntax argumentList)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(214, expression, argumentList, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInvocationExpressionSyntax)internalSyntaxNode;
		}
		InternalInvocationExpressionSyntax internalInvocationExpressionSyntax = new InternalInvocationExpressionSyntax(SyntaxKind.InvocationExpression, expression, argumentList, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInvocationExpressionSyntax, hash);
		}
		return internalInvocationExpressionSyntax;
	}

	public InternalArgumentListSyntax ArgumentList(InternalSyntaxToken openParenthesisToken, InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> arguments, InternalSyntaxToken closeParenthesisToken)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(223, openParenthesisToken, arguments.Node, closeParenthesisToken, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalArgumentListSyntax)internalSyntaxNode;
		}
		InternalArgumentListSyntax internalArgumentListSyntax = new InternalArgumentListSyntax(SyntaxKind.ArgumentList, openParenthesisToken, arguments.Node, closeParenthesisToken, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalArgumentListSyntax, hash);
		}
		return internalArgumentListSyntax;
	}

	public InternalUnaryExpressionSyntax UnaryExpression(SyntaxKind kind, InternalSyntaxToken operatorToken, InternalCodeExpressionSyntax expression)
	{
		if ((uint)(kind - 210) > 2u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode((int)kind, operatorToken, expression, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalUnaryExpressionSyntax)internalSyntaxNode;
		}
		InternalUnaryExpressionSyntax internalUnaryExpressionSyntax = new InternalUnaryExpressionSyntax(kind, operatorToken, expression, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalUnaryExpressionSyntax, hash);
		}
		return internalUnaryExpressionSyntax;
	}

	public InternalBinaryExpressionSyntax BinaryExpression(SyntaxKind kind, InternalCodeExpressionSyntax left, InternalSyntaxToken operatorToken, InternalCodeExpressionSyntax right)
	{
		if ((uint)(kind - 194) > 15u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode((int)kind, left, operatorToken, right, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalBinaryExpressionSyntax)internalSyntaxNode;
		}
		InternalBinaryExpressionSyntax internalBinaryExpressionSyntax = new InternalBinaryExpressionSyntax(kind, left, operatorToken, right, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalBinaryExpressionSyntax, hash);
		}
		return internalBinaryExpressionSyntax;
	}

	public InternalInListExpressionSyntax InListExpression(InternalCodeExpressionSyntax expression, InternalSyntaxToken inKeywordToken, InternalBracketedArgumentListSyntax listExpression)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(217, expression, inKeywordToken, listExpression, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalInListExpressionSyntax)internalSyntaxNode;
		}
		InternalInListExpressionSyntax internalInListExpressionSyntax = new InternalInListExpressionSyntax(SyntaxKind.InListExpression, expression, inKeywordToken, listExpression, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalInListExpressionSyntax, hash);
		}
		return internalInListExpressionSyntax;
	}

	public InternalMemberAccessExpressionSyntax MemberAccessExpression(InternalCodeExpressionSyntax expression, InternalSyntaxToken dotToken, InternalIdentifierNameSyntax name)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(215, expression, dotToken, name, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalMemberAccessExpressionSyntax)internalSyntaxNode;
		}
		InternalMemberAccessExpressionSyntax internalMemberAccessExpressionSyntax = new InternalMemberAccessExpressionSyntax(SyntaxKind.MemberAccessExpression, expression, dotToken, name, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalMemberAccessExpressionSyntax, hash);
		}
		return internalMemberAccessExpressionSyntax;
	}

	public InternalOptionAccessExpressionSyntax OptionAccessExpression(InternalCodeExpressionSyntax expression, InternalSyntaxToken colonColonToken, InternalIdentifierNameSyntax name)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(216, expression, colonColonToken, name, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalOptionAccessExpressionSyntax)internalSyntaxNode;
		}
		InternalOptionAccessExpressionSyntax internalOptionAccessExpressionSyntax = new InternalOptionAccessExpressionSyntax(SyntaxKind.OptionAccessExpression, expression, colonColonToken, name, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalOptionAccessExpressionSyntax, hash);
		}
		return internalOptionAccessExpressionSyntax;
	}

	public InternalIdentifierNameSyntax IdentifierName(InternalSyntaxToken identifier)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(220, identifier, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIdentifierNameSyntax)internalSyntaxNode;
		}
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = new InternalIdentifierNameSyntax(SyntaxKind.IdentifierName, identifier, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIdentifierNameSyntax, hash);
		}
		return internalIdentifierNameSyntax;
	}

	public InternalQualifiedNameSyntax QualifiedName(InternalNameSyntax left, InternalSyntaxToken dotToken, InternalSimpleNameSyntax right)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(221, left, dotToken, right, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalQualifiedNameSyntax)internalSyntaxNode;
		}
		InternalQualifiedNameSyntax internalQualifiedNameSyntax = new InternalQualifiedNameSyntax(SyntaxKind.QualifiedName, left, dotToken, right, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalQualifiedNameSyntax, hash);
		}
		return internalQualifiedNameSyntax;
	}

	public InternalIdentifierEqualsStringSyntax IdentifierEqualsString(InternalSyntaxToken identifier, InternalSyntaxToken equalsToken, InternalStringLiteralValueSyntax stringLiteral)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(375, identifier, equalsToken, stringLiteral, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIdentifierEqualsStringSyntax)internalSyntaxNode;
		}
		InternalIdentifierEqualsStringSyntax internalIdentifierEqualsStringSyntax = new InternalIdentifierEqualsStringSyntax(SyntaxKind.IdentifierEqualsString, identifier, equalsToken, stringLiteral, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIdentifierEqualsStringSyntax, hash);
		}
		return internalIdentifierEqualsStringSyntax;
	}

	public InternalIdentifierEqualsLiteralSyntax IdentifierEqualsLiteral(InternalSyntaxToken identifier, InternalSyntaxToken equalsToken, InternalLiteralValueSyntax literal)
	{
		InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(376, identifier, equalsToken, literal, LexerMode, out var hash);
		if (internalSyntaxNode != null)
		{
			return (InternalIdentifierEqualsLiteralSyntax)internalSyntaxNode;
		}
		InternalIdentifierEqualsLiteralSyntax internalIdentifierEqualsLiteralSyntax = new InternalIdentifierEqualsLiteralSyntax(SyntaxKind.IdentifierEqualsLiteral, identifier, equalsToken, literal, LexerMode);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(internalIdentifierEqualsLiteralSyntax, hash);
		}
		return internalIdentifierEqualsLiteralSyntax;
	}

	public InternalSkippedTokensTriviaSyntax SkippedTokensTrivia(InternalSyntaxNode tokens)
	{
		return new InternalSkippedTokensTriviaSyntax(SyntaxKind.SkippedTokensTrivia, tokens, LexerMode);
	}
}
