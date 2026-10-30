using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal class InternalSyntaxRewriter : InternalSyntaxVisitor<InternalSyntaxNode>
{
	protected readonly bool VisitIntoStructuredTrivia;

	public override InternalSyntaxNode VisitObjectCompilationUnit(InternalObjectCompilationUnitSyntax node)
	{
		InternalSyntaxList<InternalObjectSyntax> objects = VisitList(node.Objects);
		InternalSyntaxToken endOfFileToken = (InternalSyntaxToken)Visit(node.EndOfFileToken);
		return node.Update(objects, endOfFileToken);
	}

	public override InternalSyntaxNode VisitPropertyList(InternalPropertyListSyntax node)
	{
		InternalSyntaxList<InternalPropertySyntaxOrEmpty> properties = VisitList(node.Properties);
		return node.Update(properties);
	}

	public override InternalSyntaxNode VisitProfile(InternalProfileSyntax node)
	{
		InternalSyntaxToken profileToken = (InternalSyntaxToken)Visit(node.ProfileToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(profileToken, name, openBraceToken, propertyList, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitEnumType(InternalEnumTypeSyntax node)
	{
		InternalSyntaxToken enumToken = (InternalSyntaxToken)Visit(node.EnumToken);
		InternalObjectIdSyntax objectId = (InternalObjectIdSyntax)Visit(node.ObjectId);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalEnumValueSyntax> values = VisitList(node.Values);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(enumToken, objectId, name, openBraceToken, propertyList, values, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitEnumValue(InternalEnumValueSyntax node)
	{
		InternalSyntaxToken enumValueToken = (InternalSyntaxToken)Visit(node.EnumValueToken);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalSyntaxToken id = (InternalSyntaxToken)Visit(node.Id);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(enumValueToken, openParenthesisToken, id, semicolonToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitEnumExtensionType(InternalEnumExtensionTypeSyntax node)
	{
		InternalSyntaxToken enumExtensionKeyword = (InternalSyntaxToken)Visit(node.EnumExtensionKeyword);
		InternalObjectIdSyntax objectId = (InternalObjectIdSyntax)Visit(node.ObjectId);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken extendsKeyword = (InternalSyntaxToken)Visit(node.ExtendsKeyword);
		InternalObjectNameOrIdSyntax baseObject = (InternalObjectNameOrIdSyntax)Visit(node.BaseObject);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalEnumValueSyntax> values = VisitList(node.Values);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(enumExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, values, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitDotNetPackage(InternalDotNetPackageSyntax node)
	{
		InternalSyntaxToken dotNetKeyword = (InternalSyntaxToken)Visit(node.DotNetKeyword);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalDotNetAssemblySyntax> assemblies = VisitList(node.Assemblies);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(dotNetKeyword, name, openBraceToken, propertyList, assemblies, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitDotNetAssembly(InternalDotNetAssemblySyntax node)
	{
		InternalSyntaxToken assemblyKeyword = (InternalSyntaxToken)Visit(node.AssemblyKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalNameSyntax assemblyName = (InternalNameSyntax)Visit(node.AssemblyName);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalDotNetTypeDeclarationSyntax> typeDeclarations = VisitList(node.TypeDeclarations);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(assemblyKeyword, openParenthesisToken, assemblyName, closeParenthesisToken, openBraceToken, propertyList, typeDeclarations, closeBraceToken);
	}

	public override InternalSyntaxNode VisitDotNetTypeDeclaration(InternalDotNetTypeDeclarationSyntax node)
	{
		InternalSyntaxToken type = (InternalSyntaxToken)Visit(node.Type);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalNameSyntax typeName = (InternalNameSyntax)Visit(node.TypeName);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalIdentifierNameSyntax typeAlias = (InternalIdentifierNameSyntax)Visit(node.TypeAlias);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(type, openParenthesisToken, typeName, semicolonToken, typeAlias, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitCodeunit(InternalCodeunitSyntax node)
	{
		InternalSyntaxToken codeunitKeyword = (InternalSyntaxToken)Visit(node.CodeunitKeyword);
		InternalObjectIdSyntax objectId = (InternalObjectIdSyntax)Visit(node.ObjectId);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(codeunitKeyword, objectId, name, openBraceToken, propertyList, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitTable(InternalTableSyntax node)
	{
		InternalSyntaxToken tableKeyword = (InternalSyntaxToken)Visit(node.TableKeyword);
		InternalObjectIdSyntax objectId = (InternalObjectIdSyntax)Visit(node.ObjectId);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalFieldListSyntax fields = (InternalFieldListSyntax)Visit(node.Fields);
		InternalKeyListSyntax keys = (InternalKeyListSyntax)Visit(node.Keys);
		InternalFieldGroupListSyntax fieldGroups = (InternalFieldGroupListSyntax)Visit(node.FieldGroups);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(tableKeyword, objectId, name, openBraceToken, propertyList, fields, keys, fieldGroups, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitFieldList(InternalFieldListSyntax node)
	{
		InternalSyntaxToken fieldsKeyword = (InternalSyntaxToken)Visit(node.FieldsKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalFieldSyntax> fields = VisitList(node.Fields);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(fieldsKeyword, openBraceToken, fields, closeBraceToken);
	}

	public override InternalSyntaxNode VisitFieldExtensionList(InternalFieldExtensionListSyntax node)
	{
		InternalSyntaxToken fieldsKeyword = (InternalSyntaxToken)Visit(node.FieldsKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalFieldBaseSyntax> fields = VisitList(node.Fields);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(fieldsKeyword, openBraceToken, fields, closeBraceToken);
	}

	public override InternalSyntaxNode VisitField(InternalFieldSyntax node)
	{
		InternalSyntaxToken fieldKeyword = (InternalSyntaxToken)Visit(node.FieldKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalSyntaxToken no = (InternalSyntaxToken)Visit(node.No);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken1);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken2 = (InternalSyntaxToken)Visit(node.SemicolonToken2);
		InternalDataTypeSyntax type = (InternalDataTypeSyntax)Visit(node.Type);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(fieldKeyword, openParenthesisToken, no, semicolonToken, name, semicolonToken2, type, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override InternalSyntaxNode VisitFieldModification(InternalFieldModificationSyntax node)
	{
		InternalSyntaxToken fieldKeyword = (InternalSyntaxToken)Visit(node.FieldKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(fieldKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override InternalSyntaxNode VisitKeyList(InternalKeyListSyntax node)
	{
		InternalSyntaxToken keysKeyword = (InternalSyntaxToken)Visit(node.KeysKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalKeySyntax> keys = VisitList(node.Keys);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(keysKeyword, openBraceToken, keys, closeBraceToken);
	}

	public override InternalSyntaxNode VisitKey(InternalKeySyntax node)
	{
		InternalSyntaxToken keyKeyword = (InternalSyntaxToken)Visit(node.KeyKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalSyntaxToken id = (InternalSyntaxToken)Visit(node.Id);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken1);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken2 = (InternalSyntaxToken)Visit(node.SemicolonToken2);
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields = VisitList(node.Fields);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(keyKeyword, openParenthesisToken, id, semicolonToken, name, semicolonToken2, fields, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitFieldGroupList(InternalFieldGroupListSyntax node)
	{
		InternalSyntaxToken fieldGroupsKeyword = (InternalSyntaxToken)Visit(node.FieldGroupsKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalFieldGroupSyntax> fieldGroups = VisitList(node.FieldGroups);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(fieldGroupsKeyword, openBraceToken, fieldGroups, closeBraceToken);
	}

	public override InternalSyntaxNode VisitFieldGroupExtensionList(InternalFieldGroupExtensionListSyntax node)
	{
		InternalSyntaxToken fieldGroupsKeyword = (InternalSyntaxToken)Visit(node.FieldGroupsKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalFieldGroupChangeBaseSyntax> changes = VisitList(node.Changes);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(fieldGroupsKeyword, openBraceToken, changes, closeBraceToken);
	}

	public override InternalSyntaxNode VisitFieldGroupAddChange(InternalFieldGroupAddChangeSyntax node)
	{
		InternalSyntaxToken changeKeyword = (InternalSyntaxToken)Visit(node.ChangeKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax anchor = (InternalIdentifierNameSyntax)Visit(node.Anchor);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken1);
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields = VisitList(node.Fields);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, semicolonToken, fields, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitFieldGroup(InternalFieldGroupSyntax node)
	{
		InternalSyntaxToken fieldGroupKeyword = (InternalSyntaxToken)Visit(node.FieldGroupKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalSyntaxToken id = (InternalSyntaxToken)Visit(node.Id);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken1);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken2 = (InternalSyntaxToken)Visit(node.SemicolonToken2);
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields = VisitList(node.Fields);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(fieldGroupKeyword, openParenthesisToken, id, semicolonToken, name, semicolonToken2, fields, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPage(InternalPageSyntax node)
	{
		InternalSyntaxToken pageKeyword = (InternalSyntaxToken)Visit(node.PageKeyword);
		InternalObjectIdSyntax objectId = (InternalObjectIdSyntax)Visit(node.ObjectId);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalPageLayoutSyntax layout = (InternalPageLayoutSyntax)Visit(node.Layout);
		InternalPageActionListSyntax actions = (InternalPageActionListSyntax)Visit(node.Actions);
		InternalPageViewListSyntax views = (InternalPageViewListSyntax)Visit(node.Views);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(pageKeyword, objectId, name, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageLayout(InternalPageLayoutSyntax node)
	{
		InternalSyntaxToken layoutKeyword = (InternalSyntaxToken)Visit(node.LayoutKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalPageAreaSyntax> areas = VisitList(node.Areas);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(layoutKeyword, openBraceToken, areas, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageField(InternalPageFieldSyntax node)
	{
		InternalSyntaxToken controlKeyword = (InternalSyntaxToken)Visit(node.ControlKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, semicolonToken, expression, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageLabel(InternalPageLabelSyntax node)
	{
		InternalSyntaxToken controlKeyword = (InternalSyntaxToken)Visit(node.ControlKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageGroup(InternalPageGroupSyntax node)
	{
		InternalSyntaxToken controlKeyword = (InternalSyntaxToken)Visit(node.ControlKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalControlBaseSyntax> controls = VisitList(node.Controls);
		InternalGroupActionListSyntax actionList = (InternalGroupActionListSyntax)Visit(node.ActionList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, controls, actionList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPagePart(InternalPagePartSyntax node)
	{
		InternalSyntaxToken controlKeyword = (InternalSyntaxToken)Visit(node.ControlKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalObjectNameOrIdSyntax partName = (InternalObjectNameOrIdSyntax)Visit(node.PartName);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, semicolonToken, partName, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageSystemPart(InternalPageSystemPartSyntax node)
	{
		InternalSyntaxToken controlKeyword = (InternalSyntaxToken)Visit(node.ControlKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalIdentifierNameSyntax systemPartType = (InternalIdentifierNameSyntax)Visit(node.SystemPartType);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, semicolonToken, systemPartType, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageChartPart(InternalPageChartPartSyntax node)
	{
		InternalSyntaxToken controlKeyword = (InternalSyntaxToken)Visit(node.ControlKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalIdentifierNameSyntax chartPartType = (InternalIdentifierNameSyntax)Visit(node.ChartPartType);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, semicolonToken, chartPartType, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageUserControl(InternalPageUserControlSyntax node)
	{
		InternalSyntaxToken controlKeyword = (InternalSyntaxToken)Visit(node.ControlKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalObjectNameReferenceSyntax controlAddIn = (InternalObjectNameReferenceSyntax)Visit(node.ControlAddIn);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, semicolonToken, controlAddIn, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageArea(InternalPageAreaSyntax node)
	{
		InternalSyntaxToken controlKeyword = (InternalSyntaxToken)Visit(node.ControlKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalControlBaseSyntax> controls = VisitList(node.Controls);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, controls, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageActionList(InternalPageActionListSyntax node)
	{
		InternalSyntaxToken actionsKeyword = (InternalSyntaxToken)Visit(node.ActionsKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalPageActionAreaSyntax> areas = VisitList(node.Areas);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(actionsKeyword, openBraceToken, areas, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageViewList(InternalPageViewListSyntax node)
	{
		InternalSyntaxToken viewsKeyword = (InternalSyntaxToken)Visit(node.ViewsKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalPageViewSyntax> views = VisitList(node.Views);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(viewsKeyword, openBraceToken, views, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageView(InternalPageViewSyntax node)
	{
		InternalSyntaxToken viewKeyword = (InternalSyntaxToken)Visit(node.ViewKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalPageExtensionLayoutSyntax layout = (InternalPageExtensionLayoutSyntax)Visit(node.Layout);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(viewKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, layout, closeBraceToken);
	}

	public override InternalSyntaxNode VisitGroupActionList(InternalGroupActionListSyntax node)
	{
		InternalSyntaxToken actionsKeyword = (InternalSyntaxToken)Visit(node.ActionsKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalActionBaseSyntax> actions = VisitList(node.Actions);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(actionsKeyword, openBraceToken, actions, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageActionGroup(InternalPageActionGroupSyntax node)
	{
		InternalSyntaxToken actionKeyword = (InternalSyntaxToken)Visit(node.ActionKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalActionBaseSyntax> actions = VisitList(node.Actions);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, actions, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageActionArea(InternalPageActionAreaSyntax node)
	{
		InternalSyntaxToken actionKeyword = (InternalSyntaxToken)Visit(node.ActionKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalActionBaseSyntax> actions = VisitList(node.Actions);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, actions, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageAction(InternalPageActionSyntax node)
	{
		InternalSyntaxToken actionKeyword = (InternalSyntaxToken)Visit(node.ActionKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageActionSeparator(InternalPageActionSeparatorSyntax node)
	{
		InternalSyntaxToken actionKeyword = (InternalSyntaxToken)Visit(node.ActionKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageExtension(InternalPageExtensionSyntax node)
	{
		InternalSyntaxToken pageExtensionKeyword = (InternalSyntaxToken)Visit(node.PageExtensionKeyword);
		InternalObjectIdSyntax objectId = (InternalObjectIdSyntax)Visit(node.ObjectId);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken extendsKeyword = (InternalSyntaxToken)Visit(node.ExtendsKeyword);
		InternalObjectNameOrIdSyntax baseObject = (InternalObjectNameOrIdSyntax)Visit(node.BaseObject);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalPageExtensionLayoutSyntax layout = (InternalPageExtensionLayoutSyntax)Visit(node.Layout);
		InternalPageExtensionActionListSyntax actions = (InternalPageExtensionActionListSyntax)Visit(node.Actions);
		InternalPageExtensionViewListSyntax views = (InternalPageExtensionViewListSyntax)Visit(node.Views);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(pageExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageCustomization(InternalPageCustomizationSyntax node)
	{
		InternalSyntaxToken pageCustomizationKeyword = (InternalSyntaxToken)Visit(node.PageCustomizationKeyword);
		InternalObjectIdSyntax objectId = (InternalObjectIdSyntax)Visit(node.ObjectId);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken customizesKeyword = (InternalSyntaxToken)Visit(node.CustomizesKeyword);
		InternalObjectNameOrIdSyntax baseObject = (InternalObjectNameOrIdSyntax)Visit(node.BaseObject);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalPageExtensionLayoutSyntax layout = (InternalPageExtensionLayoutSyntax)Visit(node.Layout);
		InternalPageExtensionActionListSyntax actions = (InternalPageExtensionActionListSyntax)Visit(node.Actions);
		InternalPageExtensionViewListSyntax views = (InternalPageExtensionViewListSyntax)Visit(node.Views);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(pageCustomizationKeyword, objectId, name, customizesKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitTableExtension(InternalTableExtensionSyntax node)
	{
		InternalSyntaxToken tableExtensionKeyword = (InternalSyntaxToken)Visit(node.TableExtensionKeyword);
		InternalObjectIdSyntax objectId = (InternalObjectIdSyntax)Visit(node.ObjectId);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken extendsKeyword = (InternalSyntaxToken)Visit(node.ExtendsKeyword);
		InternalObjectNameOrIdSyntax baseObject = (InternalObjectNameOrIdSyntax)Visit(node.BaseObject);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalFieldExtensionListSyntax fields = (InternalFieldExtensionListSyntax)Visit(node.Fields);
		InternalKeyListSyntax keys = (InternalKeyListSyntax)Visit(node.Keys);
		InternalFieldGroupExtensionListSyntax fieldGroups = (InternalFieldGroupExtensionListSyntax)Visit(node.FieldGroups);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(tableExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, fields, keys, fieldGroups, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageExtensionLayout(InternalPageExtensionLayoutSyntax node)
	{
		InternalSyntaxToken layoutKeyword = (InternalSyntaxToken)Visit(node.LayoutKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalControlChangeBaseSyntax> changes = VisitList(node.Changes);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(layoutKeyword, openBraceToken, changes, closeBraceToken);
	}

	public override InternalSyntaxNode VisitControlAddChange(InternalControlAddChangeSyntax node)
	{
		InternalSyntaxToken changeKeyword = (InternalSyntaxToken)Visit(node.ChangeKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax anchor = (InternalIdentifierNameSyntax)Visit(node.Anchor);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalControlBaseSyntax> controls = VisitList(node.Controls);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, controls, closeBraceToken);
	}

	public override InternalSyntaxNode VisitControlMoveChange(InternalControlMoveChangeSyntax node)
	{
		InternalSyntaxToken changeKeyword = (InternalSyntaxToken)Visit(node.ChangeKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax anchor = (InternalIdentifierNameSyntax)Visit(node.Anchor);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> controls = VisitList(node.Controls);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, semicolonToken, controls, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitControlModifyChange(InternalControlModifyChangeSyntax node)
	{
		InternalSyntaxToken changeKeyword = (InternalSyntaxToken)Visit(node.ChangeKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageExtensionViewList(InternalPageExtensionViewListSyntax node)
	{
		InternalSyntaxToken viewsKeyword = (InternalSyntaxToken)Visit(node.ViewsKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalViewChangeBaseSyntax> changes = VisitList(node.Changes);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(viewsKeyword, openBraceToken, changes, closeBraceToken);
	}

	public override InternalSyntaxNode VisitViewAddChange(InternalViewAddChangeSyntax node)
	{
		InternalSyntaxToken changeKeyword = (InternalSyntaxToken)Visit(node.ChangeKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax anchor = (InternalIdentifierNameSyntax)Visit(node.Anchor);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalPageViewSyntax> views = VisitList(node.Views);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, views, closeBraceToken);
	}

	public override InternalSyntaxNode VisitViewMoveChange(InternalViewMoveChangeSyntax node)
	{
		InternalSyntaxToken changeKeyword = (InternalSyntaxToken)Visit(node.ChangeKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax anchor = (InternalIdentifierNameSyntax)Visit(node.Anchor);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> views = VisitList(node.Views);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, semicolonToken, views, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitViewModifyChange(InternalViewModifyChangeSyntax node)
	{
		InternalSyntaxToken changeKeyword = (InternalSyntaxToken)Visit(node.ChangeKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitPageExtensionActionList(InternalPageExtensionActionListSyntax node)
	{
		InternalSyntaxToken actionsKeyword = (InternalSyntaxToken)Visit(node.ActionsKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalActionChangeBaseSyntax> changes = VisitList(node.Changes);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(actionsKeyword, openBraceToken, changes, closeBraceToken);
	}

	public override InternalSyntaxNode VisitActionAddChange(InternalActionAddChangeSyntax node)
	{
		InternalSyntaxToken changeKeyword = (InternalSyntaxToken)Visit(node.ChangeKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax anchor = (InternalIdentifierNameSyntax)Visit(node.Anchor);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalActionBaseSyntax> actions = VisitList(node.Actions);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, actions, closeBraceToken);
	}

	public override InternalSyntaxNode VisitActionMoveChange(InternalActionMoveChangeSyntax node)
	{
		InternalSyntaxToken changeKeyword = (InternalSyntaxToken)Visit(node.ChangeKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax anchor = (InternalIdentifierNameSyntax)Visit(node.Anchor);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> actions = VisitList(node.Actions);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, semicolonToken, actions, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitActionModifyChange(InternalActionModifyChangeSyntax node)
	{
		InternalSyntaxToken changeKeyword = (InternalSyntaxToken)Visit(node.ChangeKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override InternalSyntaxNode VisitQuery(InternalQuerySyntax node)
	{
		InternalSyntaxToken queryKeyword = (InternalSyntaxToken)Visit(node.QueryKeyword);
		InternalObjectIdSyntax objectId = (InternalObjectIdSyntax)Visit(node.ObjectId);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalQueryElementsSyntax elements = (InternalQueryElementsSyntax)Visit(node.Elements);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(queryKeyword, objectId, name, openBraceToken, propertyList, elements, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitQueryElements(InternalQueryElementsSyntax node)
	{
		InternalSyntaxToken queryElementsKeyword = (InternalSyntaxToken)Visit(node.QueryElementsKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalQueryDataItemSyntax> dataItems = VisitList(node.DataItems);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(queryElementsKeyword, openBraceToken, dataItems, closeBraceToken);
	}

	public override InternalSyntaxNode VisitQueryDataItem(InternalQueryDataItemSyntax node)
	{
		InternalSyntaxToken dataItemKeyword = (InternalSyntaxToken)Visit(node.DataItemKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalObjectNameOrIdSyntax dataItemTable = (InternalObjectNameOrIdSyntax)Visit(node.DataItemTable);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalQueryDataItemElementSyntax> elements = VisitList(node.Elements);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(dataItemKeyword, openParenthesisToken, name, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements, closeBraceToken);
	}

	public override InternalSyntaxNode VisitQueryColumn(InternalQueryColumnSyntax node)
	{
		InternalSyntaxToken columnKeyword = (InternalSyntaxToken)Visit(node.ColumnKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalIdentifierNameSyntax relatedField = (InternalIdentifierNameSyntax)Visit(node.RelatedField);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(columnKeyword, openParenthesisToken, name, semicolonToken, relatedField, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitQueryFilter(InternalQueryFilterSyntax node)
	{
		InternalSyntaxToken filterKeyword = (InternalSyntaxToken)Visit(node.FilterKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalIdentifierNameSyntax relatedField = (InternalIdentifierNameSyntax)Visit(node.RelatedField);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(filterKeyword, openParenthesisToken, name, semicolonToken, relatedField, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitReport(InternalReportSyntax node)
	{
		InternalSyntaxToken reportKeyword = (InternalSyntaxToken)Visit(node.ReportKeyword);
		InternalObjectIdSyntax objectId = (InternalObjectIdSyntax)Visit(node.ObjectId);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalReportDataSetSectionSyntax dataSet = (InternalReportDataSetSectionSyntax)Visit(node.DataSet);
		InternalRequestPageSyntax requestPage = (InternalRequestPageSyntax)Visit(node.RequestPage);
		InternalReportLabelsSectionSyntax labels = (InternalReportLabelsSectionSyntax)Visit(node.Labels);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(reportKeyword, objectId, name, openBraceToken, propertyList, dataSet, requestPage, labels, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitReportDataSetSection(InternalReportDataSetSectionSyntax node)
	{
		InternalSyntaxToken dataSetKeyword = (InternalSyntaxToken)Visit(node.DataSetKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalReportDataItemSyntax> dataItems = VisitList(node.DataItems);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(dataSetKeyword, openBraceToken, dataItems, closeBraceToken);
	}

	public override InternalSyntaxNode VisitReportDataItem(InternalReportDataItemSyntax node)
	{
		InternalSyntaxToken dataItemKeyword = (InternalSyntaxToken)Visit(node.DataItemKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalObjectNameOrIdSyntax dataItemTable = (InternalObjectNameOrIdSyntax)Visit(node.DataItemTable);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalReportDataItemElementSyntax> elements = VisitList(node.Elements);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(dataItemKeyword, openParenthesisToken, name, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements, triggers, closeBraceToken);
	}

	public override InternalSyntaxNode VisitReportColumn(InternalReportColumnSyntax node)
	{
		InternalSyntaxToken columnKeyword = (InternalSyntaxToken)Visit(node.ColumnKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalCodeExpressionSyntax sourceExpression = (InternalCodeExpressionSyntax)Visit(node.SourceExpression);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(columnKeyword, openParenthesisToken, name, semicolonToken, sourceExpression, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override InternalSyntaxNode VisitReportLabelsSection(InternalReportLabelsSectionSyntax node)
	{
		InternalSyntaxToken labelsKeyword = (InternalSyntaxToken)Visit(node.LabelsKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalReportLabelBaseSyntax> labels = VisitList(node.Labels);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(labelsKeyword, openBraceToken, labels, closeBraceToken);
	}

	public override InternalSyntaxNode VisitReportLabelMultilanguage(InternalReportLabelMultilanguageSyntax node)
	{
		InternalSyntaxToken labelKeyword = (InternalSyntaxToken)Visit(node.LabelKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalCommaSeparatedIdentifierEqualsStringListSyntax caption = (InternalCommaSeparatedIdentifierEqualsStringListSyntax)Visit(node.Caption);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(labelKeyword, openParenthesisToken, name, semicolonToken, caption, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitReportLabel(InternalReportLabelSyntax node)
	{
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalLabelSyntax label = (InternalLabelSyntax)Visit(node.Label);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(name, equalsToken, label, semicolonToken);
	}

	public override InternalSyntaxNode VisitRequestPage(InternalRequestPageSyntax node)
	{
		InternalSyntaxToken requestPageKeyword = (InternalSyntaxToken)Visit(node.RequestPageKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalPageLayoutSyntax layout = (InternalPageLayoutSyntax)Visit(node.Layout);
		InternalPageActionListSyntax actions = (InternalPageActionListSyntax)Visit(node.Actions);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(requestPageKeyword, openBraceToken, name, propertyList, layout, actions, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitXmlPort(InternalXmlPortSyntax node)
	{
		InternalSyntaxToken xmlPortKeyword = (InternalSyntaxToken)Visit(node.XmlPortKeyword);
		InternalObjectIdSyntax objectId = (InternalObjectIdSyntax)Visit(node.ObjectId);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalXmlPortSchemaSyntax xmlPortSchema = (InternalXmlPortSchemaSyntax)Visit(node.XmlPortSchema);
		InternalRequestPageSyntax xmlPortRequestPage = (InternalRequestPageSyntax)Visit(node.XmlPortRequestPage);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(xmlPortKeyword, objectId, name, openBraceToken, propertyList, xmlPortSchema, xmlPortRequestPage, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitXmlPortSchema(InternalXmlPortSchemaSyntax node)
	{
		InternalSyntaxToken xmlPortSchemaKeyword = (InternalSyntaxToken)Visit(node.XmlPortSchemaKeyword);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalSyntaxList<InternalXmlPortNodeSyntax> xmlPortSchema = VisitList(node.XmlPortSchema);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(xmlPortSchemaKeyword, openBraceToken, xmlPortSchema, closeBraceToken);
	}

	public override InternalSyntaxNode VisitXmlPortTableElement(InternalXmlPortTableElementSyntax node)
	{
		InternalSyntaxToken xmlPortTableElementKeyword = (InternalSyntaxToken)Visit(node.XmlPortTableElementKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalObjectNameOrIdSyntax sourceTable = (InternalObjectNameOrIdSyntax)Visit(node.SourceTable);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = VisitList(node.Schema);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(xmlPortTableElementKeyword, openParenthesisToken, name, semicolonToken, sourceTable, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
	}

	public override InternalSyntaxNode VisitXmlPortFieldElement(InternalXmlPortFieldElementSyntax node)
	{
		InternalSyntaxToken xmlPortFieldElementKeyword = (InternalSyntaxToken)Visit(node.XmlPortFieldElementKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalCodeExpressionSyntax sourceField = (InternalCodeExpressionSyntax)Visit(node.SourceField);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = VisitList(node.Schema);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(xmlPortFieldElementKeyword, openParenthesisToken, name, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
	}

	public override InternalSyntaxNode VisitXmlPortFieldAttribute(InternalXmlPortFieldAttributeSyntax node)
	{
		InternalSyntaxToken xmlPortFieldAttributeKeyword = (InternalSyntaxToken)Visit(node.XmlPortFieldAttributeKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalCodeExpressionSyntax sourceField = (InternalCodeExpressionSyntax)Visit(node.SourceField);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = VisitList(node.Schema);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(xmlPortFieldAttributeKeyword, openParenthesisToken, name, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
	}

	public override InternalSyntaxNode VisitXmlPortTextElement(InternalXmlPortTextElementSyntax node)
	{
		InternalSyntaxToken xmlPortTextElementKeyword = (InternalSyntaxToken)Visit(node.XmlPortTextElementKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = VisitList(node.Schema);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(xmlPortTextElementKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
	}

	public override InternalSyntaxNode VisitXmlPortTextAttribute(InternalXmlPortTextAttributeSyntax node)
	{
		InternalSyntaxToken xmlPortTextAttributeKeyword = (InternalSyntaxToken)Visit(node.XmlPortTextAttributeKeyword);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = VisitList(node.Schema);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(xmlPortTextAttributeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
	}

	public override InternalSyntaxNode VisitControlAddIn(InternalControlAddInSyntax node)
	{
		InternalSyntaxToken controlAddInKeyword = (InternalSyntaxToken)Visit(node.ControlAddInKeyword);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken openBraceToken = (InternalSyntaxToken)Visit(node.OpenBraceToken);
		InternalPropertyListSyntax propertyList = (InternalPropertyListSyntax)Visit(node.PropertyList);
		InternalSyntaxList<InternalMemberSyntax> members = VisitList(node.Members);
		InternalSyntaxToken closeBraceToken = (InternalSyntaxToken)Visit(node.CloseBraceToken);
		return node.Update(controlAddInKeyword, name, openBraceToken, propertyList, members, closeBraceToken);
	}

	public override InternalSyntaxNode VisitMemberAttribute(InternalMemberAttributeSyntax node)
	{
		InternalSyntaxToken openBracketToken = (InternalSyntaxToken)Visit(node.OpenBracketToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalAttributeArgumentListSyntax argumentList = (InternalAttributeArgumentListSyntax)Visit(node.ArgumentList);
		InternalSyntaxToken closeBracketToken = (InternalSyntaxToken)Visit(node.CloseBracketToken);
		return node.Update(openBracketToken, name, argumentList, closeBracketToken);
	}

	public override InternalSyntaxNode VisitAttributeArgumentList(InternalAttributeArgumentListSyntax node)
	{
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalSeparatedSyntaxList<InternalAttributeArgumentSyntax> arguments = VisitList(node.Arguments);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(openParenthesisToken, arguments, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitInvalidAttributeArgument(InternalInvalidAttributeArgumentSyntax node)
	{
		InternalSyntaxNode invalidToken = Visit(node.InvalidToken);
		return node.Update(invalidToken);
	}

	public override InternalSyntaxNode VisitLiteralAttributeArgument(InternalLiteralAttributeArgumentSyntax node)
	{
		InternalLiteralExpressionSyntax literal = (InternalLiteralExpressionSyntax)Visit(node.Literal);
		return node.Update(literal);
	}

	public override InternalSyntaxNode VisitMethodReferenceAttributeArgument(InternalMethodReferenceAttributeArgumentSyntax node)
	{
		InternalMemberAccessExpressionSyntax methodReference = (InternalMemberAccessExpressionSyntax)Visit(node.MethodReference);
		return node.Update(methodReference);
	}

	public override InternalSyntaxNode VisitOptionAccessAttributeArgument(InternalOptionAccessAttributeArgumentSyntax node)
	{
		InternalOptionAccessExpressionSyntax optionAccess = (InternalOptionAccessExpressionSyntax)Visit(node.OptionAccess);
		return node.Update(optionAccess);
	}

	public override InternalSyntaxNode VisitIdentifierNameOrEmpty(InternalIdentifierNameOrEmptySyntax node)
	{
		InternalIdentifierNameSyntax identifierName = (InternalIdentifierNameSyntax)Visit(node.IdentifierName);
		InternalSyntaxToken empty = (InternalSyntaxToken)Visit(node.Empty);
		return node.Update(identifierName, empty);
	}

	public override InternalSyntaxNode VisitLiteralExpression(InternalLiteralExpressionSyntax node)
	{
		InternalLiteralValueSyntax literal = (InternalLiteralValueSyntax)Visit(node.Literal);
		return node.Update(literal);
	}

	public override InternalSyntaxNode VisitOptionValues(InternalOptionValuesSyntax node)
	{
		InternalSeparatedSyntaxList<InternalIdentifierNameOrEmptySyntax> options = VisitList(node.Options);
		return node.Update(options);
	}

	public override InternalSyntaxNode VisitInt32SignedLiteralValue(InternalInt32SignedLiteralValueSyntax node)
	{
		InternalSyntaxToken sign = (InternalSyntaxToken)Visit(node.Sign);
		InternalSyntaxToken number = (InternalSyntaxToken)Visit(node.Number);
		return node.Update(sign, number);
	}

	public override InternalSyntaxNode VisitInt64SignedLiteralValue(InternalInt64SignedLiteralValueSyntax node)
	{
		InternalSyntaxToken sign = (InternalSyntaxToken)Visit(node.Sign);
		InternalSyntaxToken number = (InternalSyntaxToken)Visit(node.Number);
		return node.Update(sign, number);
	}

	public override InternalSyntaxNode VisitDecimalSignedLiteralValue(InternalDecimalSignedLiteralValueSyntax node)
	{
		InternalSyntaxToken sign = (InternalSyntaxToken)Visit(node.Sign);
		InternalSyntaxToken number = (InternalSyntaxToken)Visit(node.Number);
		return node.Update(sign, number);
	}

	public override InternalSyntaxNode VisitBooleanLiteralValue(InternalBooleanLiteralValueSyntax node)
	{
		InternalSyntaxToken value = (InternalSyntaxToken)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitStringLiteralValue(InternalStringLiteralValueSyntax node)
	{
		InternalSyntaxToken value = (InternalSyntaxToken)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitDateLiteralValue(InternalDateLiteralValueSyntax node)
	{
		InternalSyntaxToken value = (InternalSyntaxToken)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitTimeLiteralValue(InternalTimeLiteralValueSyntax node)
	{
		InternalSyntaxToken value = (InternalSyntaxToken)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitDateTimeLiteralValue(InternalDateTimeLiteralValueSyntax node)
	{
		InternalSyntaxToken value = (InternalSyntaxToken)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitInvalidPropertyValue(InternalInvalidPropertyValueSyntax node)
	{
		InternalSyntaxToken invalidToken = (InternalSyntaxToken)Visit(node.InvalidToken);
		return node.Update(invalidToken);
	}

	public override InternalSyntaxNode VisitDecimalPlacesPropertyValue(InternalDecimalPlacesPropertyValueSyntax node)
	{
		InternalDecimalPlacesSyntax value = (InternalDecimalPlacesSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitOptionValuePropertyValue(InternalOptionValuePropertyValueSyntax node)
	{
		InternalIdentifierNameSyntax value = (InternalIdentifierNameSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitImagePropertyValue(InternalImagePropertyValueSyntax node)
	{
		InternalIdentifierNameSyntax value = (InternalIdentifierNameSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitPageFieldReferencePropertyValue(InternalPageFieldReferencePropertyValueSyntax node)
	{
		InternalIdentifierNameSyntax value = (InternalIdentifierNameSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitMultilanguagePropertyValue(InternalMultilanguagePropertyValueSyntax node)
	{
		InternalCommaSeparatedIdentifierEqualsStringListSyntax value = (InternalCommaSeparatedIdentifierEqualsStringListSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitLabelPropertyValue(InternalLabelPropertyValueSyntax node)
	{
		InternalLabelSyntax value = (InternalLabelSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitMemberReferencePropertyValue(InternalMemberReferencePropertyValueSyntax node)
	{
		InternalIdentifierNameSyntax value = (InternalIdentifierNameSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitPermissionPropertyValue(InternalPermissionPropertyValueSyntax node)
	{
		InternalSeparatedSyntaxList<InternalPermissionSyntax> permissionProperties = VisitList(node.PermissionProperties);
		return node.Update(permissionProperties);
	}

	public override InternalSyntaxNode VisitQualifiedObjectReferencePropertyValue(InternalQualifiedObjectReferencePropertyValueSyntax node)
	{
		InternalSyntaxToken objectType = (InternalSyntaxToken)Visit(node.ObjectType);
		InternalObjectNameOrIdSyntax objectNameOrId = (InternalObjectNameOrIdSyntax)Visit(node.ObjectNameOrId);
		return node.Update(objectType, objectNameOrId);
	}

	public override InternalSyntaxNode VisitObjectReferencePropertyValue(InternalObjectReferencePropertyValueSyntax node)
	{
		InternalObjectNameOrIdSyntax objectNameOrId = (InternalObjectNameOrIdSyntax)Visit(node.ObjectNameOrId);
		return node.Update(objectNameOrId);
	}

	public override InternalSyntaxNode VisitPermission(InternalPermissionSyntax node)
	{
		InternalSyntaxToken objectType = (InternalSyntaxToken)Visit(node.ObjectType);
		InternalObjectNameOrIdSyntax objectReference = (InternalObjectNameOrIdSyntax)Visit(node.ObjectReference);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalSyntaxToken permissions = (InternalSyntaxToken)Visit(node.Permissions);
		return node.Update(objectType, objectReference, equalsToken, permissions);
	}

	public override InternalSyntaxNode VisitEnumPropertyValue(InternalEnumPropertyValueSyntax node)
	{
		InternalIdentifierNameSyntax value = (InternalIdentifierNameSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitIdentifierOrLiteralPropertyValue(InternalIdentifierOrLiteralPropertyValueSyntax node)
	{
		InternalIdentifierOrLiteralExpressionSyntax identifier = (InternalIdentifierOrLiteralExpressionSyntax)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override InternalSyntaxNode VisitIdentifierOrLiteralExpression(InternalIdentifierOrLiteralExpressionSyntax node)
	{
		InternalCodeExpressionSyntax identifier = (InternalCodeExpressionSyntax)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override InternalSyntaxNode VisitObjectNameReference(InternalObjectNameReferenceSyntax node)
	{
		InternalCodeExpressionSyntax identifier = (InternalCodeExpressionSyntax)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override InternalSyntaxNode VisitObjectNameOrId(InternalObjectNameOrIdSyntax node)
	{
		InternalCodeExpressionSyntax identifier = (InternalCodeExpressionSyntax)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override InternalSyntaxNode VisitObjectId(InternalObjectIdSyntax node)
	{
		InternalSyntaxToken value = (InternalSyntaxToken)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitDecimalPlaces(InternalDecimalPlacesSyntax node)
	{
		InternalSyntaxToken minimumToken = (InternalSyntaxToken)Visit(node.MinimumToken);
		InternalSyntaxToken colonToken = (InternalSyntaxToken)Visit(node.ColonToken);
		InternalSyntaxToken maximumToken = (InternalSyntaxToken)Visit(node.MaximumToken);
		return node.Update(minimumToken, colonToken, maximumToken);
	}

	public override InternalSyntaxNode VisitMemberAccessOrIdentifierPropertyValue(InternalMemberAccessOrIdentifierPropertyValueSyntax node)
	{
		InternalCodeExpressionSyntax identifier = (InternalCodeExpressionSyntax)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override InternalSyntaxNode VisitPropertyName(InternalPropertyNameSyntax node)
	{
		InternalSyntaxToken identifier = (InternalSyntaxToken)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override InternalSyntaxNode VisitCommaSeparatedPropertyValue(InternalCommaSeparatedPropertyValueSyntax node)
	{
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> values = VisitList(node.Values);
		return node.Update(values);
	}

	public override InternalSyntaxNode VisitCommaSeparatedStringsPropertyValue(InternalCommaSeparatedStringsPropertyValueSyntax node)
	{
		InternalSeparatedSyntaxList<InternalStringLiteralValueSyntax> values = VisitList(node.Values);
		return node.Update(values);
	}

	public override InternalSyntaxNode VisitCommaSeparatedIdentifierEqualsStringList(InternalCommaSeparatedIdentifierEqualsStringListSyntax node)
	{
		InternalSeparatedSyntaxList<InternalIdentifierEqualsStringSyntax> values = VisitList(node.Values);
		return node.Update(values);
	}

	public override InternalSyntaxNode VisitCommaSeparatedIdentifierEqualsLiteralList(InternalCommaSeparatedIdentifierEqualsLiteralListSyntax node)
	{
		InternalSeparatedSyntaxList<InternalIdentifierEqualsLiteralSyntax> values = VisitList(node.Values);
		return node.Update(values);
	}

	public override InternalSyntaxNode VisitCommaSeparatedIdentifierEqualsStringListPropertyValue(InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax node)
	{
		InternalCommaSeparatedIdentifierEqualsStringListSyntax list = (InternalCommaSeparatedIdentifierEqualsStringListSyntax)Visit(node.List);
		return node.Update(list);
	}

	public override InternalSyntaxNode VisitCommaSeparatedIdentifierOrLiteralPropertyValue(InternalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax node)
	{
		InternalSeparatedSyntaxList<InternalIdentifierOrLiteralExpressionSyntax> values = VisitList(node.Values);
		return node.Update(values);
	}

	public override InternalSyntaxNode VisitLabel(InternalLabelSyntax node)
	{
		InternalStringLiteralValueSyntax labelText = (InternalStringLiteralValueSyntax)Visit(node.LabelText);
		InternalSyntaxToken commaToken = (InternalSyntaxToken)Visit(node.CommaToken);
		InternalCommaSeparatedIdentifierEqualsLiteralListSyntax properties = (InternalCommaSeparatedIdentifierEqualsLiteralListSyntax)Visit(node.Properties);
		return node.Update(labelText, commaToken, properties);
	}

	public override InternalSyntaxNode VisitOptionValuesPropertyValue(InternalOptionValuesPropertyValueSyntax node)
	{
		InternalOptionValuesSyntax value = (InternalOptionValuesSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitBooleanPropertyValue(InternalBooleanPropertyValueSyntax node)
	{
		InternalBooleanLiteralValueSyntax value = (InternalBooleanLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitInt32PropertyValue(InternalInt32PropertyValueSyntax node)
	{
		InternalInt32SignedLiteralValueSyntax value = (InternalInt32SignedLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitInt64PropertyValue(InternalInt64PropertyValueSyntax node)
	{
		InternalInt64SignedLiteralValueSyntax value = (InternalInt64SignedLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitDecimalPropertyValue(InternalDecimalPropertyValueSyntax node)
	{
		InternalDecimalSignedLiteralValueSyntax value = (InternalDecimalSignedLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitTimePropertyValue(InternalTimePropertyValueSyntax node)
	{
		InternalTimeLiteralValueSyntax value = (InternalTimeLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitDatePropertyValue(InternalDatePropertyValueSyntax node)
	{
		InternalDateLiteralValueSyntax value = (InternalDateLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitDateTimePropertyValue(InternalDateTimePropertyValueSyntax node)
	{
		InternalDateTimeLiteralValueSyntax value = (InternalDateTimeLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitStringPropertyValue(InternalStringPropertyValueSyntax node)
	{
		InternalStringLiteralValueSyntax value = (InternalStringLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitStyleExpressionPropertyValue(InternalStyleExpressionPropertyValueSyntax node)
	{
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		return node.Update(expression);
	}

	public override InternalSyntaxNode VisitBooleanExpressionPropertyValue(InternalBooleanExpressionPropertyValueSyntax node)
	{
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		return node.Update(expression);
	}

	public override InternalSyntaxNode VisitTextExpressionPropertyValue(InternalTextExpressionPropertyValueSyntax node)
	{
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		return node.Update(expression);
	}

	public override InternalSyntaxNode VisitIntegerExpressionPropertyValue(InternalIntegerExpressionPropertyValueSyntax node)
	{
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		return node.Update(expression);
	}

	public override InternalSyntaxNode VisitClientSideBooleanExpressionPropertyValue(InternalClientSideBooleanExpressionPropertyValueSyntax node)
	{
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		return node.Update(expression);
	}

	public override InternalSyntaxNode VisitTableFilterPropertyValue(InternalTableFilterPropertyValueSyntax node)
	{
		InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> conditions = VisitList(node.Conditions);
		return node.Update(conditions);
	}

	public override InternalSyntaxNode VisitOrderByPropertyValue(InternalOrderByPropertyValueSyntax node)
	{
		InternalSeparatedSyntaxList<InternalOrderByExpressionSyntax> order = VisitList(node.Order);
		return node.Update(order);
	}

	public override InternalSyntaxNode VisitOrderByExpression(InternalOrderByExpressionSyntax node)
	{
		InternalSyntaxToken orderSpecifier = (InternalSyntaxToken)Visit(node.OrderSpecifier);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> sortingFields = VisitList(node.SortingFields);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(orderSpecifier, openParenthesisToken, sortingFields, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitQueryDataItemLinkPropertyValue(InternalQueryDataItemLinkPropertyValueSyntax node)
	{
		InternalSeparatedSyntaxList<InternalQueryDataItemLinkExpressionSyntax> joins = VisitList(node.Joins);
		return node.Update(joins);
	}

	public override InternalSyntaxNode VisitQueryDataItemLinkExpression(InternalQueryDataItemLinkExpressionSyntax node)
	{
		InternalIdentifierNameSyntax sourceField = (InternalIdentifierNameSyntax)Visit(node.SourceField);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalMemberAccessExpressionSyntax relatedField = (InternalMemberAccessExpressionSyntax)Visit(node.RelatedField);
		return node.Update(sourceField, equalsToken, relatedField);
	}

	public override InternalSyntaxNode VisitReportDataItemLinkPropertyValue(InternalReportDataItemLinkPropertyValueSyntax node)
	{
		InternalSeparatedSyntaxList<InternalReportDataItemLinkExpressionSyntax> joins = VisitList(node.Joins);
		return node.Update(joins);
	}

	public override InternalSyntaxNode VisitReportDataItemLinkExpression(InternalReportDataItemLinkExpressionSyntax node)
	{
		InternalIdentifierNameSyntax sourceField = (InternalIdentifierNameSyntax)Visit(node.SourceField);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalSyntaxToken fieldKeywordToken = (InternalSyntaxToken)Visit(node.FieldKeywordToken);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax relatedField = (InternalIdentifierNameSyntax)Visit(node.RelatedField);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(sourceField, equalsToken, fieldKeywordToken, openParenthesisToken, relatedField, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitTableViewPropertyValue(InternalTableViewPropertyValueSyntax node)
	{
		InternalSortingSyntax sorting = (InternalSortingSyntax)Visit(node.Sorting);
		InternalOrderSyntax order = (InternalOrderSyntax)Visit(node.Order);
		InternalWhereExpressionSyntax tableFilter = (InternalWhereExpressionSyntax)Visit(node.TableFilter);
		return node.Update(sorting, order, tableFilter);
	}

	public override InternalSyntaxNode VisitFiltersPropertyValue(InternalFiltersPropertyValueSyntax node)
	{
		InternalWhereExpressionSyntax tableFilter = (InternalWhereExpressionSyntax)Visit(node.TableFilter);
		return node.Update(tableFilter);
	}

	public override InternalSyntaxNode VisitSorting(InternalSortingSyntax node)
	{
		InternalSyntaxToken sortingToken = (InternalSyntaxToken)Visit(node.SortingToken);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> sortingFields = VisitList(node.SortingFields);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(sortingToken, openParenthesisToken, sortingFields, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitOrder(InternalOrderSyntax node)
	{
		InternalSyntaxToken orderToken = (InternalSyntaxToken)Visit(node.OrderToken);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalSyntaxToken orderBy = (InternalSyntaxToken)Visit(node.OrderBy);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(orderToken, openParenthesisToken, orderBy, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitTableRelationPropertyValue(InternalTableRelationPropertyValueSyntax node)
	{
		InternalIfTableRelationSyntax ifExpression = (InternalIfTableRelationSyntax)Visit(node.IfExpression);
		InternalMemberAccessOrIdentifierPropertyValueSyntax relatedTableField = (InternalMemberAccessOrIdentifierPropertyValueSyntax)Visit(node.RelatedTableField);
		InternalWhereExpressionSyntax tableFilter = (InternalWhereExpressionSyntax)Visit(node.TableFilter);
		InternalElseTableRelationSyntax elseExpression = (InternalElseTableRelationSyntax)Visit(node.ElseExpression);
		return node.Update(ifExpression, relatedTableField, tableFilter, elseExpression);
	}

	public override InternalSyntaxNode VisitIfTableRelation(InternalIfTableRelationSyntax node)
	{
		InternalSyntaxToken ifKeywordToken = (InternalSyntaxToken)Visit(node.IfKeywordToken);
		InternalTableFilterExpressionSyntax ifTableRelationCondition = (InternalTableFilterExpressionSyntax)Visit(node.IfTableRelationCondition);
		return node.Update(ifKeywordToken, ifTableRelationCondition);
	}

	public override InternalSyntaxNode VisitTableFilterExpression(InternalTableFilterExpressionSyntax node)
	{
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> conditions = VisitList(node.Conditions);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(openParenthesisToken, conditions, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitElseTableRelation(InternalElseTableRelationSyntax node)
	{
		InternalSyntaxToken elseKeywordToken = (InternalSyntaxToken)Visit(node.ElseKeywordToken);
		InternalTableRelationPropertyValueSyntax elseTableRelationCondition = (InternalTableRelationPropertyValueSyntax)Visit(node.ElseTableRelationCondition);
		return node.Update(elseKeywordToken, elseTableRelationCondition);
	}

	public override InternalSyntaxNode VisitWhereExpression(InternalWhereExpressionSyntax node)
	{
		InternalSyntaxToken whereKeywordToken = (InternalSyntaxToken)Visit(node.WhereKeywordToken);
		InternalTableFilterExpressionSyntax filter = (InternalTableFilterExpressionSyntax)Visit(node.Filter);
		return node.Update(whereKeywordToken, filter);
	}

	public override InternalSyntaxNode VisitConstExpression(InternalConstExpressionSyntax node)
	{
		InternalIdentifierNameSyntax leftHandSide = (InternalIdentifierNameSyntax)Visit(node.LeftHandSide);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalSyntaxToken constKeywordToken = (InternalSyntaxToken)Visit(node.ConstKeywordToken);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierOrLiteralExpressionSyntax identifier = (InternalIdentifierOrLiteralExpressionSyntax)Visit(node.Identifier);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(leftHandSide, equalsToken, constKeywordToken, openParenthesisToken, identifier, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitSimpleFieldExpression(InternalSimpleFieldExpressionSyntax node)
	{
		InternalIdentifierNameSyntax leftHandSide = (InternalIdentifierNameSyntax)Visit(node.LeftHandSide);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalSyntaxToken fieldKeywordToken = (InternalSyntaxToken)Visit(node.FieldKeywordToken);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax identifier = (InternalIdentifierNameSyntax)Visit(node.Identifier);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(leftHandSide, equalsToken, fieldKeywordToken, openParenthesisToken, identifier, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitInvalidPropertyExpression(InternalInvalidPropertyExpressionSyntax node)
	{
		InternalIdentifierNameSyntax leftHandSide = (InternalIdentifierNameSyntax)Visit(node.LeftHandSide);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalSyntaxToken constOrFieldOrFilterKeywordToken = (InternalSyntaxToken)Visit(node.ConstOrFieldOrFilterKeywordToken);
		InternalSyntaxToken invalidExpression = (InternalSyntaxToken)Visit(node.InvalidExpression);
		return node.Update(leftHandSide, equalsToken, constOrFieldOrFilterKeywordToken, invalidExpression);
	}

	public override InternalSyntaxNode VisitTableCalculationFormula(InternalTableCalculationFormulaSyntax node)
	{
		InternalSyntaxToken sign = (InternalSyntaxToken)Visit(node.Sign);
		InternalSyntaxToken formulaKeywordToken = (InternalSyntaxToken)Visit(node.FormulaKeywordToken);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalIdentifierNameSyntax table = (InternalIdentifierNameSyntax)Visit(node.Table);
		InternalWhereExpressionSyntax whereExpression = (InternalWhereExpressionSyntax)Visit(node.WhereExpression);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(sign, formulaKeywordToken, openParenthesisToken, table, whereExpression, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitFieldCalculationFormula(InternalFieldCalculationFormulaSyntax node)
	{
		InternalSyntaxToken sign = (InternalSyntaxToken)Visit(node.Sign);
		InternalSyntaxToken formulaKeywordToken = (InternalSyntaxToken)Visit(node.FormulaKeywordToken);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalMemberAccessExpressionSyntax field = (InternalMemberAccessExpressionSyntax)Visit(node.Field);
		InternalWhereExpressionSyntax whereExpression = (InternalWhereExpressionSyntax)Visit(node.WhereExpression);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(sign, formulaKeywordToken, openParenthesisToken, field, whereExpression, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitFieldFilterExpression(InternalFieldFilterExpressionSyntax node)
	{
		InternalIdentifierNameSyntax leftHandSide = (InternalIdentifierNameSyntax)Visit(node.LeftHandSide);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalSyntaxToken fieldFormulaKeywordToken = (InternalSyntaxToken)Visit(node.FieldFormulaKeywordToken);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalSyntaxToken filterFormulaKeywordToken = (InternalSyntaxToken)Visit(node.FilterFormulaKeywordToken);
		InternalSyntaxToken openParenthesisToken2 = (InternalSyntaxToken)Visit(node.OpenParenthesisToken1);
		InternalIdentifierNameSyntax identifier = (InternalIdentifierNameSyntax)Visit(node.Identifier);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken1);
		InternalSyntaxToken closeParenthesisToken2 = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, filterFormulaKeywordToken, openParenthesisToken2, identifier, closeParenthesisToken, closeParenthesisToken2);
	}

	public override InternalSyntaxNode VisitFieldUpperLimitExpression(InternalFieldUpperLimitExpressionSyntax node)
	{
		InternalIdentifierNameSyntax leftHandSide = (InternalIdentifierNameSyntax)Visit(node.LeftHandSide);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalSyntaxToken fieldFormulaKeywordToken = (InternalSyntaxToken)Visit(node.FieldFormulaKeywordToken);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalSyntaxToken upperLimitFormulaKeywordToken = (InternalSyntaxToken)Visit(node.UpperLimitFormulaKeywordToken);
		InternalSyntaxToken openParenthesisToken2 = (InternalSyntaxToken)Visit(node.OpenParenthesisToken1);
		InternalIdentifierNameSyntax identifier = (InternalIdentifierNameSyntax)Visit(node.Identifier);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken1);
		InternalSyntaxToken closeParenthesisToken2 = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, upperLimitFormulaKeywordToken, openParenthesisToken2, identifier, closeParenthesisToken, closeParenthesisToken2);
	}

	public override InternalSyntaxNode VisitFieldUpperLimitFilterExpression(InternalFieldUpperLimitFilterExpressionSyntax node)
	{
		InternalIdentifierNameSyntax leftHandSide = (InternalIdentifierNameSyntax)Visit(node.LeftHandSide);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalSyntaxToken fieldFormulaKeywordToken = (InternalSyntaxToken)Visit(node.FieldFormulaKeywordToken);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken1);
		InternalSyntaxToken upperLimitFormulaKeywordToken = (InternalSyntaxToken)Visit(node.UpperLimitFormulaKeywordToken);
		InternalSyntaxToken openParenthesisToken2 = (InternalSyntaxToken)Visit(node.OpenParenthesisToken2);
		InternalSyntaxToken filterFormulaKeywordToken = (InternalSyntaxToken)Visit(node.FilterFormulaKeywordToken);
		InternalSyntaxToken openParenthesisToken3 = (InternalSyntaxToken)Visit(node.OpenParenthesisToken3);
		InternalIdentifierNameSyntax identifier = (InternalIdentifierNameSyntax)Visit(node.Identifier);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken3);
		InternalSyntaxToken closeParenthesisToken2 = (InternalSyntaxToken)Visit(node.CloseParenthesisToken2);
		InternalSyntaxToken closeParenthesisToken3 = (InternalSyntaxToken)Visit(node.CloseParenthesisToken1);
		return node.Update(leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, upperLimitFormulaKeywordToken, openParenthesisToken2, filterFormulaKeywordToken, openParenthesisToken3, identifier, closeParenthesisToken, closeParenthesisToken2, closeParenthesisToken3);
	}

	public override InternalSyntaxNode VisitFilterExpression(InternalFilterExpressionSyntax node)
	{
		InternalIdentifierNameSyntax leftHandSide = (InternalIdentifierNameSyntax)Visit(node.LeftHandSide);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalSyntaxToken filterFormulaKeywordToken = (InternalSyntaxToken)Visit(node.FilterFormulaKeywordToken);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalFilterExpressionValueSyntax filter = (InternalFilterExpressionValueSyntax)Visit(node.Filter);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(leftHandSide, equalsToken, filterFormulaKeywordToken, openParenthesisToken, filter, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitParenthesizedFilterExpressionValue(InternalParenthesizedFilterExpressionValueSyntax node)
	{
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalFilterExpressionValueSyntax expression = (InternalFilterExpressionValueSyntax)Visit(node.Expression);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(openParenthesisToken, expression, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitUnaryFilterExpressionValue(InternalUnaryFilterExpressionValueSyntax node)
	{
		InternalSyntaxToken operatorToken = (InternalSyntaxToken)Visit(node.OperatorToken);
		InternalIdentifierOrLiteralExpressionSyntax identifier = (InternalIdentifierOrLiteralExpressionSyntax)Visit(node.Identifier);
		return node.Update(operatorToken, identifier);
	}

	public override InternalSyntaxNode VisitBinaryFilterExpressionValue(InternalBinaryFilterExpressionValueSyntax node)
	{
		InternalSyntaxNode left = Visit(node.Left);
		InternalSyntaxToken operatorToken = (InternalSyntaxToken)Visit(node.OperatorToken);
		InternalSyntaxNode right = Visit(node.Right);
		return node.Update(left, operatorToken, right);
	}

	public override InternalSyntaxNode VisitRangeFromFilterExpressionValue(InternalRangeFromFilterExpressionValueSyntax node)
	{
		InternalIdentifierOrLiteralExpressionSyntax left = (InternalIdentifierOrLiteralExpressionSyntax)Visit(node.Left);
		InternalSyntaxToken operatorToken = (InternalSyntaxToken)Visit(node.OperatorToken);
		return node.Update(left, operatorToken);
	}

	public override InternalSyntaxNode VisitInvalidFilterExpressionValue(InternalInvalidFilterExpressionValueSyntax node)
	{
		InternalSyntaxToken invalidToken = (InternalSyntaxToken)Visit(node.InvalidToken);
		return node.Update(invalidToken);
	}

	public override InternalSyntaxNode VisitEmptyProperty(InternalEmptyPropertySyntax node)
	{
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(semicolonToken);
	}

	public override InternalSyntaxNode VisitProperty(InternalPropertySyntax node)
	{
		InternalPropertyNameSyntax name = (InternalPropertyNameSyntax)Visit(node.Name);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalPropertyValueSyntax value = (InternalPropertyValueSyntax)Visit(node.Value);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(name, equalsToken, value, semicolonToken);
	}

	public override InternalSyntaxNode VisitVarSection(InternalVarSectionSyntax node)
	{
		InternalSyntaxToken varKeyword = (InternalSyntaxToken)Visit(node.VarKeyword);
		InternalSyntaxList<InternalVariableDeclarationSyntax> variables = VisitList(node.Variables);
		return node.Update(varKeyword, variables);
	}

	public override InternalSyntaxNode VisitVariableDeclaration(InternalVariableDeclarationSyntax node)
	{
		InternalSyntaxList<InternalMemberAttributeSyntax> attributes = VisitList(node.Attributes);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken colonToken = (InternalSyntaxToken)Visit(node.ColonToken);
		InternalTypeReferenceBaseSyntax type = (InternalTypeReferenceBaseSyntax)Visit(node.Type);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributes, name, colonToken, type, semicolonToken);
	}

	public override InternalSyntaxNode VisitParameter(InternalParameterSyntax node)
	{
		InternalSyntaxToken varKeyword = (InternalSyntaxToken)Visit(node.VarKeyword);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken colonToken = (InternalSyntaxToken)Visit(node.ColonToken);
		InternalTypeReferenceBaseSyntax type = (InternalTypeReferenceBaseSyntax)Visit(node.Type);
		return node.Update(varKeyword, name, colonToken, type);
	}

	public override InternalSyntaxNode VisitReturnValue(InternalReturnValueSyntax node)
	{
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalSyntaxToken colonToken = (InternalSyntaxToken)Visit(node.ColonToken);
		InternalDataTypeSyntax dataType = (InternalDataTypeSyntax)Visit(node.DataType);
		return node.Update(name, colonToken, dataType);
	}

	public override InternalSyntaxNode VisitSimpleTypeReference(InternalSimpleTypeReferenceSyntax node)
	{
		InternalArraySyntax array = (InternalArraySyntax)Visit(node.Array);
		InternalDataTypeSyntax dataType = (InternalDataTypeSyntax)Visit(node.DataType);
		return node.Update(array, dataType);
	}

	public override InternalSyntaxNode VisitRecordTypeReference(InternalRecordTypeReferenceSyntax node)
	{
		InternalArraySyntax array = (InternalArraySyntax)Visit(node.Array);
		InternalDataTypeSyntax dataType = (InternalDataTypeSyntax)Visit(node.DataType);
		InternalSyntaxToken temporary = (InternalSyntaxToken)Visit(node.Temporary);
		return node.Update(array, dataType, temporary);
	}

	public override InternalSyntaxNode VisitDotNetTypeReference(InternalDotNetTypeReferenceSyntax node)
	{
		InternalArraySyntax array = (InternalArraySyntax)Visit(node.Array);
		InternalDataTypeSyntax dataType = (InternalDataTypeSyntax)Visit(node.DataType);
		return node.Update(array, dataType);
	}

	public override InternalSyntaxNode VisitArray(InternalArraySyntax node)
	{
		InternalSyntaxToken arrayKeyword = (InternalSyntaxToken)Visit(node.ArrayKeyword);
		InternalBracketedDimensionListSyntax dimensionList = (InternalBracketedDimensionListSyntax)Visit(node.DimensionList);
		InternalSyntaxToken ofKeyword = (InternalSyntaxToken)Visit(node.OfKeyword);
		return node.Update(arrayKeyword, dimensionList, ofKeyword);
	}

	public override InternalSyntaxNode VisitBracketedDimensionList(InternalBracketedDimensionListSyntax node)
	{
		InternalSyntaxToken openBracketToken = (InternalSyntaxToken)Visit(node.OpenBracketToken);
		InternalSeparatedSyntaxList<InternalDimensionSyntax> dimensions = VisitList(node.Dimensions);
		InternalSyntaxToken closeBracketToken = (InternalSyntaxToken)Visit(node.CloseBracketToken);
		return node.Update(openBracketToken, dimensions, closeBracketToken);
	}

	public override InternalSyntaxNode VisitDimension(InternalDimensionSyntax node)
	{
		InternalSyntaxToken value = (InternalSyntaxToken)Visit(node.Value);
		return node.Update(value);
	}

	public override InternalSyntaxNode VisitOptionDataType(InternalOptionDataTypeSyntax node)
	{
		InternalSyntaxToken typeName = (InternalSyntaxToken)Visit(node.TypeName);
		InternalOptionValuesSyntax optionValues = (InternalOptionValuesSyntax)Visit(node.OptionValues);
		return node.Update(typeName, optionValues);
	}

	public override InternalSyntaxNode VisitSimpleNamedDataType(InternalSimpleNamedDataTypeSyntax node)
	{
		InternalSyntaxToken typeName = (InternalSyntaxToken)Visit(node.TypeName);
		return node.Update(typeName);
	}

	public override InternalSyntaxNode VisitGenericNamedDataType(InternalGenericNamedDataTypeSyntax node)
	{
		InternalSyntaxToken typeName = (InternalSyntaxToken)Visit(node.TypeName);
		InternalSyntaxToken ofKeyword = (InternalSyntaxToken)Visit(node.OfKeyword);
		InternalSyntaxToken openBracketToken = (InternalSyntaxToken)Visit(node.OpenBracketToken);
		InternalSeparatedSyntaxList<InternalDataTypeSyntax> typeArguments = VisitList(node.TypeArguments);
		InternalSyntaxToken closeBracketToken = (InternalSyntaxToken)Visit(node.CloseBracketToken);
		return node.Update(typeName, ofKeyword, openBracketToken, typeArguments, closeBracketToken);
	}

	public override InternalSyntaxNode VisitTextConstDataType(InternalTextConstDataTypeSyntax node)
	{
		InternalSyntaxToken typeName = (InternalSyntaxToken)Visit(node.TypeName);
		InternalCommaSeparatedIdentifierEqualsStringListSyntax multilanguage = (InternalCommaSeparatedIdentifierEqualsStringListSyntax)Visit(node.Multilanguage);
		return node.Update(typeName, multilanguage);
	}

	public override InternalSyntaxNode VisitLabelDataType(InternalLabelDataTypeSyntax node)
	{
		InternalSyntaxToken typeName = (InternalSyntaxToken)Visit(node.TypeName);
		InternalLabelSyntax label = (InternalLabelSyntax)Visit(node.Label);
		return node.Update(typeName, label);
	}

	public override InternalSyntaxNode VisitDotNetDataType(InternalDotNetDataTypeSyntax node)
	{
		InternalSyntaxToken typeName = (InternalSyntaxToken)Visit(node.TypeName);
		return node.Update(typeName);
	}

	public override InternalSyntaxNode VisitLengthDataType(InternalLengthDataTypeSyntax node)
	{
		InternalSyntaxToken typeName = (InternalSyntaxToken)Visit(node.TypeName);
		InternalSyntaxToken openBracketToken = (InternalSyntaxToken)Visit(node.OpenBracketToken);
		InternalSyntaxToken length = (InternalSyntaxToken)Visit(node.Length);
		InternalSyntaxToken closeBracketToken = (InternalSyntaxToken)Visit(node.CloseBracketToken);
		return node.Update(typeName, openBracketToken, length, closeBracketToken);
	}

	public override InternalSyntaxNode VisitSubtypedDataType(InternalSubtypedDataTypeSyntax node)
	{
		InternalSyntaxToken typeName = (InternalSyntaxToken)Visit(node.TypeName);
		InternalObjectNameOrIdSyntax subtype = (InternalObjectNameOrIdSyntax)Visit(node.Subtype);
		return node.Update(typeName, subtype);
	}

	public override InternalSyntaxNode VisitEnumDataType(InternalEnumDataTypeSyntax node)
	{
		InternalSyntaxToken typeName = (InternalSyntaxToken)Visit(node.TypeName);
		InternalIdentifierNameSyntax enumTypeName = (InternalIdentifierNameSyntax)Visit(node.EnumTypeName);
		return node.Update(typeName, enumTypeName);
	}

	public override InternalSyntaxNode VisitParameterList(InternalParameterListSyntax node)
	{
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalSeparatedSyntaxList<InternalParameterSyntax> parameters = VisitList(node.Parameters);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(openParenthesisToken, parameters, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitTriggerDeclaration(InternalTriggerDeclarationSyntax node)
	{
		InternalSyntaxList<InternalMemberAttributeSyntax> attributes = VisitList(node.Attributes);
		InternalSyntaxToken triggerKeyword = (InternalSyntaxToken)Visit(node.TriggerKeyword);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalParameterListSyntax parameterList = (InternalParameterListSyntax)Visit(node.ParameterList);
		InternalReturnValueSyntax returnValue = (InternalReturnValueSyntax)Visit(node.ReturnValue);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalVarSectionSyntax variables = (InternalVarSectionSyntax)Visit(node.Variables);
		InternalBlockSyntax body = (InternalBlockSyntax)Visit(node.Body);
		return node.Update(attributes, triggerKeyword, name, parameterList, returnValue, semicolonToken, variables, body);
	}

	public override InternalSyntaxNode VisitEventTriggerDeclaration(InternalEventTriggerDeclarationSyntax node)
	{
		InternalSyntaxList<InternalMemberAttributeSyntax> attributes = VisitList(node.Attributes);
		InternalSyntaxToken triggerKeyword = (InternalSyntaxToken)Visit(node.TriggerKeyword);
		InternalIdentifierNameSyntax publisher = (InternalIdentifierNameSyntax)Visit(node.Publisher);
		InternalSyntaxToken colonColonToken = (InternalSyntaxToken)Visit(node.ColonColonToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalParameterListSyntax parameterList = (InternalParameterListSyntax)Visit(node.ParameterList);
		InternalReturnValueSyntax returnValue = (InternalReturnValueSyntax)Visit(node.ReturnValue);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalVarSectionSyntax variables = (InternalVarSectionSyntax)Visit(node.Variables);
		InternalBlockSyntax body = (InternalBlockSyntax)Visit(node.Body);
		return node.Update(attributes, triggerKeyword, publisher, colonColonToken, name, parameterList, returnValue, semicolonToken, variables, body);
	}

	public override InternalSyntaxNode VisitMethodDeclaration(InternalMethodDeclarationSyntax node)
	{
		InternalSyntaxList<InternalMemberAttributeSyntax> attributes = VisitList(node.Attributes);
		InternalSyntaxToken localKeyword = (InternalSyntaxToken)Visit(node.LocalKeyword);
		InternalSyntaxToken procedureKeyword = (InternalSyntaxToken)Visit(node.ProcedureKeyword);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalParameterListSyntax parameterList = (InternalParameterListSyntax)Visit(node.ParameterList);
		InternalReturnValueSyntax returnValue = (InternalReturnValueSyntax)Visit(node.ReturnValue);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		InternalVarSectionSyntax variables = (InternalVarSectionSyntax)Visit(node.Variables);
		InternalBlockSyntax body = (InternalBlockSyntax)Visit(node.Body);
		return node.Update(attributes, localKeyword, procedureKeyword, name, parameterList, returnValue, semicolonToken, variables, body);
	}

	public override InternalSyntaxNode VisitEventDeclaration(InternalEventDeclarationSyntax node)
	{
		InternalSyntaxList<InternalMemberAttributeSyntax> attributes = VisitList(node.Attributes);
		InternalSyntaxToken eventKeyword = (InternalSyntaxToken)Visit(node.EventKeyword);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		InternalParameterListSyntax parameterList = (InternalParameterListSyntax)Visit(node.ParameterList);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(attributes, eventKeyword, name, parameterList, semicolonToken);
	}

	public override InternalSyntaxNode VisitBlock(InternalBlockSyntax node)
	{
		InternalSyntaxToken beginKeywordToken = (InternalSyntaxToken)Visit(node.BeginKeywordToken);
		InternalSyntaxList<InternalStatementSyntax> statements = VisitList(node.Statements);
		InternalSyntaxToken endKeywordToken = (InternalSyntaxToken)Visit(node.EndKeywordToken);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(beginKeywordToken, statements, endKeywordToken, semicolonToken);
	}

	public override InternalSyntaxNode VisitIfStatement(InternalIfStatementSyntax node)
	{
		InternalSyntaxToken ifKeywordToken = (InternalSyntaxToken)Visit(node.IfKeywordToken);
		InternalCodeExpressionSyntax condition = (InternalCodeExpressionSyntax)Visit(node.Condition);
		InternalSyntaxToken thenKeywordToken = (InternalSyntaxToken)Visit(node.ThenKeywordToken);
		InternalStatementSyntax statement = (InternalStatementSyntax)Visit(node.Statement);
		InternalSyntaxToken elseKeywordToken = (InternalSyntaxToken)Visit(node.ElseKeywordToken);
		InternalStatementSyntax elseStatement = (InternalStatementSyntax)Visit(node.ElseStatement);
		return node.Update(ifKeywordToken, condition, thenKeywordToken, statement, elseKeywordToken, elseStatement);
	}

	public override InternalSyntaxNode VisitOrphanedElseStatement(InternalOrphanedElseStatementSyntax node)
	{
		InternalSyntaxToken elseKeywordToken = (InternalSyntaxToken)Visit(node.ElseKeywordToken);
		InternalStatementSyntax elseStatement = (InternalStatementSyntax)Visit(node.ElseStatement);
		return node.Update(elseKeywordToken, elseStatement);
	}

	public override InternalSyntaxNode VisitAssignmentStatement(InternalAssignmentStatementSyntax node)
	{
		InternalCodeExpressionSyntax target = (InternalCodeExpressionSyntax)Visit(node.Target);
		InternalSyntaxToken assignmentToken = (InternalSyntaxToken)Visit(node.AssignmentToken);
		InternalCodeExpressionSyntax source = (InternalCodeExpressionSyntax)Visit(node.Source);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(target, assignmentToken, source, semicolonToken);
	}

	public override InternalSyntaxNode VisitCompoundAssignmentStatement(InternalCompoundAssignmentStatementSyntax node)
	{
		InternalCodeExpressionSyntax target = (InternalCodeExpressionSyntax)Visit(node.Target);
		InternalSyntaxToken assignmentToken = (InternalSyntaxToken)Visit(node.AssignmentToken);
		InternalCodeExpressionSyntax source = (InternalCodeExpressionSyntax)Visit(node.Source);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(target, assignmentToken, source, semicolonToken);
	}

	public override InternalSyntaxNode VisitForStatement(InternalForStatementSyntax node)
	{
		InternalSyntaxToken forKeywordToken = (InternalSyntaxToken)Visit(node.ForKeywordToken);
		InternalCodeExpressionSyntax loopVariable = (InternalCodeExpressionSyntax)Visit(node.LoopVariable);
		InternalSyntaxToken assignToken = (InternalSyntaxToken)Visit(node.AssignToken);
		InternalCodeExpressionSyntax initialValue = (InternalCodeExpressionSyntax)Visit(node.InitialValue);
		InternalSyntaxToken operatorKeywordToken = (InternalSyntaxToken)Visit(node.OperatorKeywordToken);
		InternalCodeExpressionSyntax endValue = (InternalCodeExpressionSyntax)Visit(node.EndValue);
		InternalSyntaxToken doKeywordToken = (InternalSyntaxToken)Visit(node.DoKeywordToken);
		InternalStatementSyntax statement = (InternalStatementSyntax)Visit(node.Statement);
		return node.Update(forKeywordToken, loopVariable, assignToken, initialValue, operatorKeywordToken, endValue, doKeywordToken, statement);
	}

	public override InternalSyntaxNode VisitWhileStatement(InternalWhileStatementSyntax node)
	{
		InternalSyntaxToken whileKeywordToken = (InternalSyntaxToken)Visit(node.WhileKeywordToken);
		InternalCodeExpressionSyntax condition = (InternalCodeExpressionSyntax)Visit(node.Condition);
		InternalSyntaxToken doKeywordToken = (InternalSyntaxToken)Visit(node.DoKeywordToken);
		InternalStatementSyntax statement = (InternalStatementSyntax)Visit(node.Statement);
		return node.Update(whileKeywordToken, condition, doKeywordToken, statement);
	}

	public override InternalSyntaxNode VisitWithStatement(InternalWithStatementSyntax node)
	{
		InternalSyntaxToken withKeywordToken = (InternalSyntaxToken)Visit(node.WithKeywordToken);
		InternalCodeExpressionSyntax withId = (InternalCodeExpressionSyntax)Visit(node.WithId);
		InternalSyntaxToken doKeywordToken = (InternalSyntaxToken)Visit(node.DoKeywordToken);
		InternalStatementSyntax statement = (InternalStatementSyntax)Visit(node.Statement);
		return node.Update(withKeywordToken, withId, doKeywordToken, statement);
	}

	public override InternalSyntaxNode VisitExitStatement(InternalExitStatementSyntax node)
	{
		InternalSyntaxToken exitKeywordToken = (InternalSyntaxToken)Visit(node.ExitKeywordToken);
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalCodeExpressionSyntax exitValue = (InternalCodeExpressionSyntax)Visit(node.ExitValue);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(exitKeywordToken, openParenthesisToken, exitValue, closeParenthesisToken, semicolonToken);
	}

	public override InternalSyntaxNode VisitExpressionStatement(InternalExpressionStatementSyntax node)
	{
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(expression, semicolonToken);
	}

	public override InternalSyntaxNode VisitRepeatStatement(InternalRepeatStatementSyntax node)
	{
		InternalSyntaxToken repeatKeywordToken = (InternalSyntaxToken)Visit(node.RepeatKeywordToken);
		InternalSyntaxList<InternalStatementSyntax> statements = VisitList(node.Statements);
		InternalSyntaxToken untilKeywordToken = (InternalSyntaxToken)Visit(node.UntilKeywordToken);
		InternalCodeExpressionSyntax condition = (InternalCodeExpressionSyntax)Visit(node.Condition);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(repeatKeywordToken, statements, untilKeywordToken, condition, semicolonToken);
	}

	public override InternalSyntaxNode VisitForEachStatement(InternalForEachStatementSyntax node)
	{
		InternalSyntaxToken forEachKeywordToken = (InternalSyntaxToken)Visit(node.ForEachKeywordToken);
		InternalIdentifierNameSyntax iterationVariable = (InternalIdentifierNameSyntax)Visit(node.IterationVariable);
		InternalSyntaxToken inKeywordToken = (InternalSyntaxToken)Visit(node.InKeywordToken);
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		InternalSyntaxToken doKeywordToken = (InternalSyntaxToken)Visit(node.DoKeywordToken);
		InternalStatementSyntax statement = (InternalStatementSyntax)Visit(node.Statement);
		return node.Update(forEachKeywordToken, iterationVariable, inKeywordToken, expression, doKeywordToken, statement);
	}

	public override InternalSyntaxNode VisitBreakStatement(InternalBreakStatementSyntax node)
	{
		InternalSyntaxToken breakKeyword = (InternalSyntaxToken)Visit(node.BreakKeyword);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(breakKeyword, semicolonToken);
	}

	public override InternalSyntaxNode VisitAssertErrorStatement(InternalAssertErrorStatementSyntax node)
	{
		InternalSyntaxToken assertErrorKeywordToken = (InternalSyntaxToken)Visit(node.AssertErrorKeywordToken);
		InternalStatementSyntax statement = (InternalStatementSyntax)Visit(node.Statement);
		return node.Update(assertErrorKeywordToken, statement);
	}

	public override InternalSyntaxNode VisitCaseStatement(InternalCaseStatementSyntax node)
	{
		InternalSyntaxToken caseKeywordToken = (InternalSyntaxToken)Visit(node.CaseKeywordToken);
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		InternalSyntaxToken ofKeywordToken = (InternalSyntaxToken)Visit(node.OfKeywordToken);
		InternalSyntaxList<InternalCaseLineSyntax> caseLines = VisitList(node.CaseLines);
		InternalCaseElseSyntax caseElse = (InternalCaseElseSyntax)Visit(node.CaseElse);
		InternalSyntaxToken endKeywordToken = (InternalSyntaxToken)Visit(node.EndKeywordToken);
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(caseKeywordToken, expression, ofKeywordToken, caseLines, caseElse, endKeywordToken, semicolonToken);
	}

	public override InternalSyntaxNode VisitCaseLine(InternalCaseLineSyntax node)
	{
		InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> expressions = VisitList(node.Expressions);
		InternalSyntaxToken colonToken = (InternalSyntaxToken)Visit(node.ColonToken);
		InternalStatementSyntax statement = (InternalStatementSyntax)Visit(node.Statement);
		return node.Update(expressions, colonToken, statement);
	}

	public override InternalSyntaxNode VisitCaseElse(InternalCaseElseSyntax node)
	{
		InternalSyntaxToken elseKeywordToken = (InternalSyntaxToken)Visit(node.ElseKeywordToken);
		InternalSyntaxList<InternalStatementSyntax> elseStatements = VisitList(node.ElseStatements);
		return node.Update(elseKeywordToken, elseStatements);
	}

	public override InternalSyntaxNode VisitEmptyStatement(InternalEmptyStatementSyntax node)
	{
		InternalSyntaxToken semicolonToken = (InternalSyntaxToken)Visit(node.SemicolonToken);
		return node.Update(semicolonToken);
	}

	public override InternalSyntaxNode VisitParenthesizedExpression(InternalParenthesizedExpressionSyntax node)
	{
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(openParenthesisToken, expression, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitElementAccessExpression(InternalElementAccessExpressionSyntax node)
	{
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		InternalBracketedArgumentListSyntax argumentList = (InternalBracketedArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(expression, argumentList);
	}

	public override InternalSyntaxNode VisitBracketedArgumentList(InternalBracketedArgumentListSyntax node)
	{
		InternalSyntaxToken openBracketToken = (InternalSyntaxToken)Visit(node.OpenBracketToken);
		InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> arguments = VisitList(node.Arguments);
		InternalSyntaxToken closeBracketToken = (InternalSyntaxToken)Visit(node.CloseBracketToken);
		return node.Update(openBracketToken, arguments, closeBracketToken);
	}

	public override InternalSyntaxNode VisitInvocationExpression(InternalInvocationExpressionSyntax node)
	{
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		InternalArgumentListSyntax argumentList = (InternalArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(expression, argumentList);
	}

	public override InternalSyntaxNode VisitArgumentList(InternalArgumentListSyntax node)
	{
		InternalSyntaxToken openParenthesisToken = (InternalSyntaxToken)Visit(node.OpenParenthesisToken);
		InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> arguments = VisitList(node.Arguments);
		InternalSyntaxToken closeParenthesisToken = (InternalSyntaxToken)Visit(node.CloseParenthesisToken);
		return node.Update(openParenthesisToken, arguments, closeParenthesisToken);
	}

	public override InternalSyntaxNode VisitUnaryExpression(InternalUnaryExpressionSyntax node)
	{
		InternalSyntaxToken operatorToken = (InternalSyntaxToken)Visit(node.OperatorToken);
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		return node.Update(operatorToken, expression);
	}

	public override InternalSyntaxNode VisitBinaryExpression(InternalBinaryExpressionSyntax node)
	{
		InternalCodeExpressionSyntax left = (InternalCodeExpressionSyntax)Visit(node.Left);
		InternalSyntaxToken operatorToken = (InternalSyntaxToken)Visit(node.OperatorToken);
		InternalCodeExpressionSyntax right = (InternalCodeExpressionSyntax)Visit(node.Right);
		return node.Update(left, operatorToken, right);
	}

	public override InternalSyntaxNode VisitInListExpression(InternalInListExpressionSyntax node)
	{
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		InternalSyntaxToken inKeywordToken = (InternalSyntaxToken)Visit(node.InKeywordToken);
		InternalBracketedArgumentListSyntax listExpression = (InternalBracketedArgumentListSyntax)Visit(node.ListExpression);
		return node.Update(expression, inKeywordToken, listExpression);
	}

	public override InternalSyntaxNode VisitMemberAccessExpression(InternalMemberAccessExpressionSyntax node)
	{
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		InternalSyntaxToken dotToken = (InternalSyntaxToken)Visit(node.DotToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		return node.Update(expression, dotToken, name);
	}

	public override InternalSyntaxNode VisitOptionAccessExpression(InternalOptionAccessExpressionSyntax node)
	{
		InternalCodeExpressionSyntax expression = (InternalCodeExpressionSyntax)Visit(node.Expression);
		InternalSyntaxToken colonColonToken = (InternalSyntaxToken)Visit(node.ColonColonToken);
		InternalIdentifierNameSyntax name = (InternalIdentifierNameSyntax)Visit(node.Name);
		return node.Update(expression, colonColonToken, name);
	}

	public override InternalSyntaxNode VisitIdentifierName(InternalIdentifierNameSyntax node)
	{
		InternalSyntaxToken identifier = (InternalSyntaxToken)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override InternalSyntaxNode VisitQualifiedName(InternalQualifiedNameSyntax node)
	{
		InternalNameSyntax left = (InternalNameSyntax)Visit(node.Left);
		InternalSyntaxToken dotToken = (InternalSyntaxToken)Visit(node.DotToken);
		InternalSimpleNameSyntax right = (InternalSimpleNameSyntax)Visit(node.Right);
		return node.Update(left, dotToken, right);
	}

	public override InternalSyntaxNode VisitIdentifierEqualsString(InternalIdentifierEqualsStringSyntax node)
	{
		InternalSyntaxToken identifier = (InternalSyntaxToken)Visit(node.Identifier);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalStringLiteralValueSyntax stringLiteral = (InternalStringLiteralValueSyntax)Visit(node.StringLiteral);
		return node.Update(identifier, equalsToken, stringLiteral);
	}

	public override InternalSyntaxNode VisitIdentifierEqualsLiteral(InternalIdentifierEqualsLiteralSyntax node)
	{
		InternalSyntaxToken identifier = (InternalSyntaxToken)Visit(node.Identifier);
		InternalSyntaxToken equalsToken = (InternalSyntaxToken)Visit(node.EqualsToken);
		InternalLiteralValueSyntax literal = (InternalLiteralValueSyntax)Visit(node.Literal);
		return node.Update(identifier, equalsToken, literal);
	}

	public override InternalSyntaxNode VisitSkippedTokensTrivia(InternalSkippedTokensTriviaSyntax node)
	{
		InternalSyntaxNode tokens = Visit(node.Tokens);
		return node.Update(tokens);
	}

	public InternalSyntaxRewriter(bool visitIntoStructuredTrivia = false)
	{
		VisitIntoStructuredTrivia = visitIntoStructuredTrivia;
	}

	public override InternalSyntaxNode VisitToken(InternalSyntaxToken token)
	{
		InternalSyntaxList<InternalSyntaxNode> internalSyntaxList = VisitList(token.LeadingTrivia);
		InternalSyntaxList<InternalSyntaxNode> internalSyntaxList2 = VisitList(token.TrailingTrivia);
		if (internalSyntaxList != token.LeadingTrivia || internalSyntaxList2 != token.TrailingTrivia)
		{
			if (internalSyntaxList != token.LeadingTrivia)
			{
				token = (InternalSyntaxToken)token.WithLeadingTrivia(internalSyntaxList.Node);
			}
			if (internalSyntaxList2 != token.TrailingTrivia)
			{
				token = (InternalSyntaxToken)token.WithTrailingTrivia(internalSyntaxList2.Node);
			}
		}
		return token;
	}

	public InternalSyntaxList<TNode> VisitList<TNode>(InternalSyntaxList<TNode> list) where TNode : InternalSyntaxNode
	{
		InternalSyntaxListBuilder internalSyntaxListBuilder = null;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			TNode val = list[i];
			TNode val2 = (TNode)Visit(val);
			if (val != val2 && internalSyntaxListBuilder == null)
			{
				internalSyntaxListBuilder = new InternalSyntaxListBuilder(count);
				internalSyntaxListBuilder.AddRange(list, 0, i);
			}
			if (internalSyntaxListBuilder != null)
			{
				DebugAssertHelper.Assert(val2 != null && val2.Kind != SyntaxKind.None, "Cannot remove node using Syntax.InternalSyntax.SyntaxRewriter.");
				internalSyntaxListBuilder.Add(val2);
			}
		}
		if (internalSyntaxListBuilder != null)
		{
			return internalSyntaxListBuilder.ToList();
		}
		return list;
	}

	public InternalSeparatedSyntaxList<TNode> VisitList<TNode>(InternalSeparatedSyntaxList<TNode> list) where TNode : InternalSyntaxNode
	{
		InternalSyntaxList<InternalSyntaxNode> withSeparators = list.GetWithSeparators();
		InternalSyntaxList<InternalSyntaxNode> internalSyntaxList = VisitList(withSeparators);
		if (internalSyntaxList != withSeparators)
		{
			return internalSyntaxList.AsSeparatedList<TNode>();
		}
		return list;
	}
}
