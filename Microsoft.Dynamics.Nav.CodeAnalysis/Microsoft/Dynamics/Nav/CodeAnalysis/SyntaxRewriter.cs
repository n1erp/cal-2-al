using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
public abstract class SyntaxRewriter : SyntaxVisitor<SyntaxNode>
{
	public virtual bool VisitIntoStructuredTrivia { get; }

	public override SyntaxNode VisitObjectCompilationUnit(ObjectCompilationUnitSyntax node)
	{
		SyntaxList<ObjectSyntax> objects = VisitList(node.Objects);
		SyntaxToken endOfFileToken = VisitToken(node.EndOfFileToken);
		return node.Update(objects, endOfFileToken);
	}

	public override SyntaxNode VisitPropertyList(PropertyListSyntax node)
	{
		SyntaxList<PropertySyntaxOrEmpty> properties = VisitList(node.Properties);
		return node.Update(properties);
	}

	public override SyntaxNode VisitProfile(ProfileSyntax node)
	{
		SyntaxToken profileToken = VisitToken(node.ProfileToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(profileToken, name, openBraceToken, propertyList, members, closeBraceToken);
	}

	public override SyntaxNode VisitEnumType(EnumTypeSyntax node)
	{
		SyntaxToken enumToken = VisitToken(node.EnumToken);
		ObjectIdSyntax objectId = (ObjectIdSyntax)Visit(node.ObjectId);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<EnumValueSyntax> values = VisitList(node.Values);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(enumToken, objectId, name, openBraceToken, propertyList, values, members, closeBraceToken);
	}

	public override SyntaxNode VisitEnumValue(EnumValueSyntax node)
	{
		SyntaxToken enumValueToken = VisitToken(node.EnumValueToken);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		SyntaxToken id = VisitToken(node.Id);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(enumValueToken, openParenthesisToken, id, semicolonToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitEnumExtensionType(EnumExtensionTypeSyntax node)
	{
		SyntaxToken enumExtensionKeyword = VisitToken(node.EnumExtensionKeyword);
		ObjectIdSyntax objectId = (ObjectIdSyntax)Visit(node.ObjectId);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken extendsKeyword = VisitToken(node.ExtendsKeyword);
		ObjectNameOrIdSyntax baseObject = (ObjectNameOrIdSyntax)Visit(node.BaseObject);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<EnumValueSyntax> values = VisitList(node.Values);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(enumExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, values, members, closeBraceToken);
	}

	public override SyntaxNode VisitDotNetPackage(DotNetPackageSyntax node)
	{
		SyntaxToken dotNetKeyword = VisitToken(node.DotNetKeyword);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<DotNetAssemblySyntax> assemblies = VisitList(node.Assemblies);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(dotNetKeyword, name, openBraceToken, propertyList, assemblies, members, closeBraceToken);
	}

	public override SyntaxNode VisitDotNetAssembly(DotNetAssemblySyntax node)
	{
		SyntaxToken assemblyKeyword = VisitToken(node.AssemblyKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		NameSyntax assemblyName = (NameSyntax)Visit(node.AssemblyName);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<DotNetTypeDeclarationSyntax> typeDeclarations = VisitList(node.TypeDeclarations);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(assemblyKeyword, openParenthesisToken, assemblyName, closeParenthesisToken, openBraceToken, propertyList, typeDeclarations, closeBraceToken);
	}

	public override SyntaxNode VisitDotNetTypeDeclaration(DotNetTypeDeclarationSyntax node)
	{
		SyntaxToken type = VisitToken(node.Type);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		NameSyntax typeName = (NameSyntax)Visit(node.TypeName);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		IdentifierNameSyntax typeAlias = (IdentifierNameSyntax)Visit(node.TypeAlias);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(type, openParenthesisToken, typeName, semicolonToken, typeAlias, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitCodeunit(CodeunitSyntax node)
	{
		SyntaxToken codeunitKeyword = VisitToken(node.CodeunitKeyword);
		ObjectIdSyntax objectId = (ObjectIdSyntax)Visit(node.ObjectId);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(codeunitKeyword, objectId, name, openBraceToken, propertyList, members, closeBraceToken);
	}

	public override SyntaxNode VisitTable(TableSyntax node)
	{
		SyntaxToken tableKeyword = VisitToken(node.TableKeyword);
		ObjectIdSyntax objectId = (ObjectIdSyntax)Visit(node.ObjectId);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		FieldListSyntax fields = (FieldListSyntax)Visit(node.Fields);
		KeyListSyntax keys = (KeyListSyntax)Visit(node.Keys);
		FieldGroupListSyntax fieldGroups = (FieldGroupListSyntax)Visit(node.FieldGroups);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(tableKeyword, objectId, name, openBraceToken, propertyList, fields, keys, fieldGroups, members, closeBraceToken);
	}

	public override SyntaxNode VisitFieldList(FieldListSyntax node)
	{
		SyntaxToken fieldsKeyword = VisitToken(node.FieldsKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<FieldSyntax> fields = VisitList(node.Fields);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(fieldsKeyword, openBraceToken, fields, closeBraceToken);
	}

	public override SyntaxNode VisitFieldExtensionList(FieldExtensionListSyntax node)
	{
		SyntaxToken fieldsKeyword = VisitToken(node.FieldsKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<FieldBaseSyntax> fields = VisitList(node.Fields);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(fieldsKeyword, openBraceToken, fields, closeBraceToken);
	}

	public override SyntaxNode VisitField(FieldSyntax node)
	{
		SyntaxToken fieldKeyword = VisitToken(node.FieldKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		SyntaxToken no = VisitToken(node.No);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken1);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken2 = VisitToken(node.SemicolonToken2);
		DataTypeSyntax type = (DataTypeSyntax)Visit(node.Type);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<TriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(fieldKeyword, openParenthesisToken, no, semicolonToken, name, semicolonToken2, type, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override SyntaxNode VisitFieldModification(FieldModificationSyntax node)
	{
		SyntaxToken fieldKeyword = VisitToken(node.FieldKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<TriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(fieldKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override SyntaxNode VisitKeyList(KeyListSyntax node)
	{
		SyntaxToken keysKeyword = VisitToken(node.KeysKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<KeySyntax> keys = VisitList(node.Keys);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(keysKeyword, openBraceToken, keys, closeBraceToken);
	}

	public override SyntaxNode VisitKey(KeySyntax node)
	{
		SyntaxToken keyKeyword = VisitToken(node.KeyKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		SyntaxToken id = VisitToken(node.Id);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken1);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken2 = VisitToken(node.SemicolonToken2);
		SeparatedSyntaxList<IdentifierNameSyntax> fields = VisitList(node.Fields);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(keyKeyword, openParenthesisToken, id, semicolonToken, name, semicolonToken2, fields, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitFieldGroupList(FieldGroupListSyntax node)
	{
		SyntaxToken fieldGroupsKeyword = VisitToken(node.FieldGroupsKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<FieldGroupSyntax> fieldGroups = VisitList(node.FieldGroups);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(fieldGroupsKeyword, openBraceToken, fieldGroups, closeBraceToken);
	}

	public override SyntaxNode VisitFieldGroupExtensionList(FieldGroupExtensionListSyntax node)
	{
		SyntaxToken fieldGroupsKeyword = VisitToken(node.FieldGroupsKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<FieldGroupChangeBaseSyntax> changes = VisitList(node.Changes);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(fieldGroupsKeyword, openBraceToken, changes, closeBraceToken);
	}

	public override SyntaxNode VisitFieldGroupAddChange(FieldGroupAddChangeSyntax node)
	{
		SyntaxToken changeKeyword = VisitToken(node.ChangeKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax anchor = (IdentifierNameSyntax)Visit(node.Anchor);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken1);
		SeparatedSyntaxList<IdentifierNameSyntax> fields = VisitList(node.Fields);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, semicolonToken, fields, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitFieldGroup(FieldGroupSyntax node)
	{
		SyntaxToken fieldGroupKeyword = VisitToken(node.FieldGroupKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		SyntaxToken id = VisitToken(node.Id);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken1);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken2 = VisitToken(node.SemicolonToken2);
		SeparatedSyntaxList<IdentifierNameSyntax> fields = VisitList(node.Fields);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(fieldGroupKeyword, openParenthesisToken, id, semicolonToken, name, semicolonToken2, fields, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitPage(PageSyntax node)
	{
		SyntaxToken pageKeyword = VisitToken(node.PageKeyword);
		ObjectIdSyntax objectId = (ObjectIdSyntax)Visit(node.ObjectId);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		PageLayoutSyntax layout = (PageLayoutSyntax)Visit(node.Layout);
		PageActionListSyntax actions = (PageActionListSyntax)Visit(node.Actions);
		PageViewListSyntax views = (PageViewListSyntax)Visit(node.Views);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(pageKeyword, objectId, name, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken);
	}

	public override SyntaxNode VisitPageLayout(PageLayoutSyntax node)
	{
		SyntaxToken layoutKeyword = VisitToken(node.LayoutKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<PageAreaSyntax> areas = VisitList(node.Areas);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(layoutKeyword, openBraceToken, areas, closeBraceToken);
	}

	public override SyntaxNode VisitPageField(PageFieldSyntax node)
	{
		SyntaxToken controlKeyword = VisitToken(node.ControlKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<TriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, semicolonToken, expression, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override SyntaxNode VisitPageLabel(PageLabelSyntax node)
	{
		SyntaxToken controlKeyword = VisitToken(node.ControlKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitPageGroup(PageGroupSyntax node)
	{
		SyntaxToken controlKeyword = VisitToken(node.ControlKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<ControlBaseSyntax> controls = VisitList(node.Controls);
		GroupActionListSyntax actionList = (GroupActionListSyntax)Visit(node.ActionList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, controls, actionList, closeBraceToken);
	}

	public override SyntaxNode VisitPagePart(PagePartSyntax node)
	{
		SyntaxToken controlKeyword = VisitToken(node.ControlKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		ObjectNameOrIdSyntax partName = (ObjectNameOrIdSyntax)Visit(node.PartName);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, semicolonToken, partName, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitPageSystemPart(PageSystemPartSyntax node)
	{
		SyntaxToken controlKeyword = VisitToken(node.ControlKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		IdentifierNameSyntax systemPartType = (IdentifierNameSyntax)Visit(node.SystemPartType);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, semicolonToken, systemPartType, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitPageChartPart(PageChartPartSyntax node)
	{
		SyntaxToken controlKeyword = VisitToken(node.ControlKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		IdentifierNameSyntax chartPartType = (IdentifierNameSyntax)Visit(node.ChartPartType);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, semicolonToken, chartPartType, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitPageUserControl(PageUserControlSyntax node)
	{
		SyntaxToken controlKeyword = VisitToken(node.ControlKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		ObjectNameReferenceSyntax controlAddIn = (ObjectNameReferenceSyntax)Visit(node.ControlAddIn);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<TriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, semicolonToken, controlAddIn, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override SyntaxNode VisitPageArea(PageAreaSyntax node)
	{
		SyntaxToken controlKeyword = VisitToken(node.ControlKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<ControlBaseSyntax> controls = VisitList(node.Controls);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, controls, closeBraceToken);
	}

	public override SyntaxNode VisitPageActionList(PageActionListSyntax node)
	{
		SyntaxToken actionsKeyword = VisitToken(node.ActionsKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<PageActionAreaSyntax> areas = VisitList(node.Areas);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(actionsKeyword, openBraceToken, areas, closeBraceToken);
	}

	public override SyntaxNode VisitPageViewList(PageViewListSyntax node)
	{
		SyntaxToken viewsKeyword = VisitToken(node.ViewsKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<PageViewSyntax> views = VisitList(node.Views);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(viewsKeyword, openBraceToken, views, closeBraceToken);
	}

	public override SyntaxNode VisitPageView(PageViewSyntax node)
	{
		SyntaxToken viewKeyword = VisitToken(node.ViewKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		PageExtensionLayoutSyntax layout = (PageExtensionLayoutSyntax)Visit(node.Layout);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(viewKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, layout, closeBraceToken);
	}

	public override SyntaxNode VisitGroupActionList(GroupActionListSyntax node)
	{
		SyntaxToken actionsKeyword = VisitToken(node.ActionsKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<ActionBaseSyntax> actions = VisitList(node.Actions);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(actionsKeyword, openBraceToken, actions, closeBraceToken);
	}

	public override SyntaxNode VisitPageActionGroup(PageActionGroupSyntax node)
	{
		SyntaxToken actionKeyword = VisitToken(node.ActionKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<ActionBaseSyntax> actions = VisitList(node.Actions);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, actions, closeBraceToken);
	}

	public override SyntaxNode VisitPageActionArea(PageActionAreaSyntax node)
	{
		SyntaxToken actionKeyword = VisitToken(node.ActionKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<ActionBaseSyntax> actions = VisitList(node.Actions);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, actions, closeBraceToken);
	}

	public override SyntaxNode VisitPageAction(PageActionSyntax node)
	{
		SyntaxToken actionKeyword = VisitToken(node.ActionKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<TriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override SyntaxNode VisitPageActionSeparator(PageActionSeparatorSyntax node)
	{
		SyntaxToken actionKeyword = VisitToken(node.ActionKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitPageExtension(PageExtensionSyntax node)
	{
		SyntaxToken pageExtensionKeyword = VisitToken(node.PageExtensionKeyword);
		ObjectIdSyntax objectId = (ObjectIdSyntax)Visit(node.ObjectId);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken extendsKeyword = VisitToken(node.ExtendsKeyword);
		ObjectNameOrIdSyntax baseObject = (ObjectNameOrIdSyntax)Visit(node.BaseObject);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		PageExtensionLayoutSyntax layout = (PageExtensionLayoutSyntax)Visit(node.Layout);
		PageExtensionActionListSyntax actions = (PageExtensionActionListSyntax)Visit(node.Actions);
		PageExtensionViewListSyntax views = (PageExtensionViewListSyntax)Visit(node.Views);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(pageExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken);
	}

	public override SyntaxNode VisitPageCustomization(PageCustomizationSyntax node)
	{
		SyntaxToken pageCustomizationKeyword = VisitToken(node.PageCustomizationKeyword);
		ObjectIdSyntax objectId = (ObjectIdSyntax)Visit(node.ObjectId);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken customizesKeyword = VisitToken(node.CustomizesKeyword);
		ObjectNameOrIdSyntax baseObject = (ObjectNameOrIdSyntax)Visit(node.BaseObject);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		PageExtensionLayoutSyntax layout = (PageExtensionLayoutSyntax)Visit(node.Layout);
		PageExtensionActionListSyntax actions = (PageExtensionActionListSyntax)Visit(node.Actions);
		PageExtensionViewListSyntax views = (PageExtensionViewListSyntax)Visit(node.Views);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(pageCustomizationKeyword, objectId, name, customizesKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, members, closeBraceToken);
	}

	public override SyntaxNode VisitTableExtension(TableExtensionSyntax node)
	{
		SyntaxToken tableExtensionKeyword = VisitToken(node.TableExtensionKeyword);
		ObjectIdSyntax objectId = (ObjectIdSyntax)Visit(node.ObjectId);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken extendsKeyword = VisitToken(node.ExtendsKeyword);
		ObjectNameOrIdSyntax baseObject = (ObjectNameOrIdSyntax)Visit(node.BaseObject);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		FieldExtensionListSyntax fields = (FieldExtensionListSyntax)Visit(node.Fields);
		KeyListSyntax keys = (KeyListSyntax)Visit(node.Keys);
		FieldGroupExtensionListSyntax fieldGroups = (FieldGroupExtensionListSyntax)Visit(node.FieldGroups);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(tableExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, fields, keys, fieldGroups, members, closeBraceToken);
	}

	public override SyntaxNode VisitPageExtensionLayout(PageExtensionLayoutSyntax node)
	{
		SyntaxToken layoutKeyword = VisitToken(node.LayoutKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<ControlChangeBaseSyntax> changes = VisitList(node.Changes);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(layoutKeyword, openBraceToken, changes, closeBraceToken);
	}

	public override SyntaxNode VisitControlAddChange(ControlAddChangeSyntax node)
	{
		SyntaxToken changeKeyword = VisitToken(node.ChangeKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax anchor = (IdentifierNameSyntax)Visit(node.Anchor);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<ControlBaseSyntax> controls = VisitList(node.Controls);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, controls, closeBraceToken);
	}

	public override SyntaxNode VisitControlMoveChange(ControlMoveChangeSyntax node)
	{
		SyntaxToken changeKeyword = VisitToken(node.ChangeKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax anchor = (IdentifierNameSyntax)Visit(node.Anchor);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		SeparatedSyntaxList<IdentifierNameSyntax> controls = VisitList(node.Controls);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, semicolonToken, controls, closeParenthesisToken);
	}

	public override SyntaxNode VisitControlModifyChange(ControlModifyChangeSyntax node)
	{
		SyntaxToken changeKeyword = VisitToken(node.ChangeKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<TriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override SyntaxNode VisitPageExtensionViewList(PageExtensionViewListSyntax node)
	{
		SyntaxToken viewsKeyword = VisitToken(node.ViewsKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<ViewChangeBaseSyntax> changes = VisitList(node.Changes);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(viewsKeyword, openBraceToken, changes, closeBraceToken);
	}

	public override SyntaxNode VisitViewAddChange(ViewAddChangeSyntax node)
	{
		SyntaxToken changeKeyword = VisitToken(node.ChangeKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax anchor = (IdentifierNameSyntax)Visit(node.Anchor);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<PageViewSyntax> views = VisitList(node.Views);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, views, closeBraceToken);
	}

	public override SyntaxNode VisitViewMoveChange(ViewMoveChangeSyntax node)
	{
		SyntaxToken changeKeyword = VisitToken(node.ChangeKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax anchor = (IdentifierNameSyntax)Visit(node.Anchor);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		SeparatedSyntaxList<IdentifierNameSyntax> views = VisitList(node.Views);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, semicolonToken, views, closeParenthesisToken);
	}

	public override SyntaxNode VisitViewModifyChange(ViewModifyChangeSyntax node)
	{
		SyntaxToken changeKeyword = VisitToken(node.ChangeKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitPageExtensionActionList(PageExtensionActionListSyntax node)
	{
		SyntaxToken actionsKeyword = VisitToken(node.ActionsKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<ActionChangeBaseSyntax> changes = VisitList(node.Changes);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(actionsKeyword, openBraceToken, changes, closeBraceToken);
	}

	public override SyntaxNode VisitActionAddChange(ActionAddChangeSyntax node)
	{
		SyntaxToken changeKeyword = VisitToken(node.ChangeKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax anchor = (IdentifierNameSyntax)Visit(node.Anchor);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<ActionBaseSyntax> actions = VisitList(node.Actions);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, actions, closeBraceToken);
	}

	public override SyntaxNode VisitActionMoveChange(ActionMoveChangeSyntax node)
	{
		SyntaxToken changeKeyword = VisitToken(node.ChangeKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax anchor = (IdentifierNameSyntax)Visit(node.Anchor);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		SeparatedSyntaxList<IdentifierNameSyntax> actions = VisitList(node.Actions);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(changeKeyword, openParenthesisToken, anchor, semicolonToken, actions, closeParenthesisToken);
	}

	public override SyntaxNode VisitActionModifyChange(ActionModifyChangeSyntax node)
	{
		SyntaxToken changeKeyword = VisitToken(node.ChangeKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<TriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	public override SyntaxNode VisitQuery(QuerySyntax node)
	{
		SyntaxToken queryKeyword = VisitToken(node.QueryKeyword);
		ObjectIdSyntax objectId = (ObjectIdSyntax)Visit(node.ObjectId);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		QueryElementsSyntax elements = (QueryElementsSyntax)Visit(node.Elements);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(queryKeyword, objectId, name, openBraceToken, propertyList, elements, members, closeBraceToken);
	}

	public override SyntaxNode VisitQueryElements(QueryElementsSyntax node)
	{
		SyntaxToken queryElementsKeyword = VisitToken(node.QueryElementsKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<QueryDataItemSyntax> dataItems = VisitList(node.DataItems);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(queryElementsKeyword, openBraceToken, dataItems, closeBraceToken);
	}

	public override SyntaxNode VisitQueryDataItem(QueryDataItemSyntax node)
	{
		SyntaxToken dataItemKeyword = VisitToken(node.DataItemKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		ObjectNameOrIdSyntax dataItemTable = (ObjectNameOrIdSyntax)Visit(node.DataItemTable);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<QueryDataItemElementSyntax> elements = VisitList(node.Elements);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(dataItemKeyword, openParenthesisToken, name, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements, closeBraceToken);
	}

	public override SyntaxNode VisitQueryColumn(QueryColumnSyntax node)
	{
		SyntaxToken columnKeyword = VisitToken(node.ColumnKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		IdentifierNameSyntax relatedField = (IdentifierNameSyntax)Visit(node.RelatedField);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(columnKeyword, openParenthesisToken, name, semicolonToken, relatedField, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitQueryFilter(QueryFilterSyntax node)
	{
		SyntaxToken filterKeyword = VisitToken(node.FilterKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		IdentifierNameSyntax relatedField = (IdentifierNameSyntax)Visit(node.RelatedField);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(filterKeyword, openParenthesisToken, name, semicolonToken, relatedField, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitReport(ReportSyntax node)
	{
		SyntaxToken reportKeyword = VisitToken(node.ReportKeyword);
		ObjectIdSyntax objectId = (ObjectIdSyntax)Visit(node.ObjectId);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		ReportDataSetSectionSyntax dataSet = (ReportDataSetSectionSyntax)Visit(node.DataSet);
		RequestPageSyntax requestPage = (RequestPageSyntax)Visit(node.RequestPage);
		ReportLabelsSectionSyntax labels = (ReportLabelsSectionSyntax)Visit(node.Labels);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(reportKeyword, objectId, name, openBraceToken, propertyList, dataSet, requestPage, labels, members, closeBraceToken);
	}

	public override SyntaxNode VisitReportDataSetSection(ReportDataSetSectionSyntax node)
	{
		SyntaxToken dataSetKeyword = VisitToken(node.DataSetKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<ReportDataItemSyntax> dataItems = VisitList(node.DataItems);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(dataSetKeyword, openBraceToken, dataItems, closeBraceToken);
	}

	public override SyntaxNode VisitReportDataItem(ReportDataItemSyntax node)
	{
		SyntaxToken dataItemKeyword = VisitToken(node.DataItemKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		ObjectNameOrIdSyntax dataItemTable = (ObjectNameOrIdSyntax)Visit(node.DataItemTable);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<ReportDataItemElementSyntax> elements = VisitList(node.Elements);
		SyntaxList<TriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(dataItemKeyword, openParenthesisToken, name, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements, triggers, closeBraceToken);
	}

	public override SyntaxNode VisitReportColumn(ReportColumnSyntax node)
	{
		SyntaxToken columnKeyword = VisitToken(node.ColumnKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		CodeExpressionSyntax sourceExpression = (CodeExpressionSyntax)Visit(node.SourceExpression);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(columnKeyword, openParenthesisToken, name, semicolonToken, sourceExpression, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	public override SyntaxNode VisitReportLabelsSection(ReportLabelsSectionSyntax node)
	{
		SyntaxToken labelsKeyword = VisitToken(node.LabelsKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<ReportLabelBaseSyntax> labels = VisitList(node.Labels);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(labelsKeyword, openBraceToken, labels, closeBraceToken);
	}

	public override SyntaxNode VisitReportLabelMultilanguage(ReportLabelMultilanguageSyntax node)
	{
		SyntaxToken labelKeyword = VisitToken(node.LabelKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		CommaSeparatedIdentifierEqualsStringListSyntax caption = (CommaSeparatedIdentifierEqualsStringListSyntax)Visit(node.Caption);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(labelKeyword, openParenthesisToken, name, semicolonToken, caption, closeParenthesisToken);
	}

	public override SyntaxNode VisitReportLabel(ReportLabelSyntax node)
	{
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		LabelSyntax label = (LabelSyntax)Visit(node.Label);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(name, equalsToken, label, semicolonToken);
	}

	public override SyntaxNode VisitRequestPage(RequestPageSyntax node)
	{
		SyntaxToken requestPageKeyword = VisitToken(node.RequestPageKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		PageLayoutSyntax layout = (PageLayoutSyntax)Visit(node.Layout);
		PageActionListSyntax actions = (PageActionListSyntax)Visit(node.Actions);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(requestPageKeyword, openBraceToken, name, propertyList, layout, actions, members, closeBraceToken);
	}

	public override SyntaxNode VisitXmlPort(XmlPortSyntax node)
	{
		SyntaxToken xmlPortKeyword = VisitToken(node.XmlPortKeyword);
		ObjectIdSyntax objectId = (ObjectIdSyntax)Visit(node.ObjectId);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		XmlPortSchemaSyntax xmlPortSchema = (XmlPortSchemaSyntax)Visit(node.XmlPortSchema);
		RequestPageSyntax xmlPortRequestPage = (RequestPageSyntax)Visit(node.XmlPortRequestPage);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(xmlPortKeyword, objectId, name, openBraceToken, propertyList, xmlPortSchema, xmlPortRequestPage, members, closeBraceToken);
	}

	public override SyntaxNode VisitXmlPortSchema(XmlPortSchemaSyntax node)
	{
		SyntaxToken xmlPortSchemaKeyword = VisitToken(node.XmlPortSchemaKeyword);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		SyntaxList<XmlPortNodeSyntax> xmlPortSchema = VisitList(node.XmlPortSchema);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(xmlPortSchemaKeyword, openBraceToken, xmlPortSchema, closeBraceToken);
	}

	public override SyntaxNode VisitXmlPortTableElement(XmlPortTableElementSyntax node)
	{
		SyntaxToken xmlPortTableElementKeyword = VisitToken(node.XmlPortTableElementKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		ObjectNameOrIdSyntax sourceTable = (ObjectNameOrIdSyntax)Visit(node.SourceTable);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<XmlPortNodeSyntax> schema = VisitList(node.Schema);
		SyntaxList<TriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(xmlPortTableElementKeyword, openParenthesisToken, name, semicolonToken, sourceTable, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
	}

	public override SyntaxNode VisitXmlPortFieldElement(XmlPortFieldElementSyntax node)
	{
		SyntaxToken xmlPortFieldElementKeyword = VisitToken(node.XmlPortFieldElementKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		CodeExpressionSyntax sourceField = (CodeExpressionSyntax)Visit(node.SourceField);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<XmlPortNodeSyntax> schema = VisitList(node.Schema);
		SyntaxList<TriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(xmlPortFieldElementKeyword, openParenthesisToken, name, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
	}

	public override SyntaxNode VisitXmlPortFieldAttribute(XmlPortFieldAttributeSyntax node)
	{
		SyntaxToken xmlPortFieldAttributeKeyword = VisitToken(node.XmlPortFieldAttributeKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		CodeExpressionSyntax sourceField = (CodeExpressionSyntax)Visit(node.SourceField);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<XmlPortNodeSyntax> schema = VisitList(node.Schema);
		SyntaxList<TriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(xmlPortFieldAttributeKeyword, openParenthesisToken, name, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
	}

	public override SyntaxNode VisitXmlPortTextElement(XmlPortTextElementSyntax node)
	{
		SyntaxToken xmlPortTextElementKeyword = VisitToken(node.XmlPortTextElementKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<XmlPortNodeSyntax> schema = VisitList(node.Schema);
		SyntaxList<TriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(xmlPortTextElementKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
	}

	public override SyntaxNode VisitXmlPortTextAttribute(XmlPortTextAttributeSyntax node)
	{
		SyntaxToken xmlPortTextAttributeKeyword = VisitToken(node.XmlPortTextAttributeKeyword);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<XmlPortNodeSyntax> schema = VisitList(node.Schema);
		SyntaxList<TriggerDeclarationSyntax> triggers = VisitList(node.Triggers);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(xmlPortTextAttributeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
	}

	public override SyntaxNode VisitControlAddIn(ControlAddInSyntax node)
	{
		SyntaxToken controlAddInKeyword = VisitToken(node.ControlAddInKeyword);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken openBraceToken = VisitToken(node.OpenBraceToken);
		PropertyListSyntax propertyList = (PropertyListSyntax)Visit(node.PropertyList);
		SyntaxList<MemberSyntax> members = VisitList(node.Members);
		SyntaxToken closeBraceToken = VisitToken(node.CloseBraceToken);
		return node.Update(controlAddInKeyword, name, openBraceToken, propertyList, members, closeBraceToken);
	}

	public override SyntaxNode VisitMemberAttribute(MemberAttributeSyntax node)
	{
		SyntaxToken openBracketToken = VisitToken(node.OpenBracketToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		AttributeArgumentListSyntax argumentList = (AttributeArgumentListSyntax)Visit(node.ArgumentList);
		SyntaxToken closeBracketToken = VisitToken(node.CloseBracketToken);
		return node.Update(openBracketToken, name, argumentList, closeBracketToken);
	}

	public override SyntaxNode VisitAttributeArgumentList(AttributeArgumentListSyntax node)
	{
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		SeparatedSyntaxList<AttributeArgumentSyntax> arguments = VisitList(node.Arguments);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(openParenthesisToken, arguments, closeParenthesisToken);
	}

	public override SyntaxNode VisitInvalidAttributeArgument(InvalidAttributeArgumentSyntax node)
	{
		SyntaxNode invalidToken = Visit(node.InvalidToken);
		return node.Update(invalidToken);
	}

	public override SyntaxNode VisitLiteralAttributeArgument(LiteralAttributeArgumentSyntax node)
	{
		LiteralExpressionSyntax literal = (LiteralExpressionSyntax)Visit(node.Literal);
		return node.Update(literal);
	}

	public override SyntaxNode VisitMethodReferenceAttributeArgument(MethodReferenceAttributeArgumentSyntax node)
	{
		MemberAccessExpressionSyntax methodReference = (MemberAccessExpressionSyntax)Visit(node.MethodReference);
		return node.Update(methodReference);
	}

	public override SyntaxNode VisitOptionAccessAttributeArgument(OptionAccessAttributeArgumentSyntax node)
	{
		OptionAccessExpressionSyntax optionAccess = (OptionAccessExpressionSyntax)Visit(node.OptionAccess);
		return node.Update(optionAccess);
	}

	public override SyntaxNode VisitIdentifierNameOrEmpty(IdentifierNameOrEmptySyntax node)
	{
		IdentifierNameSyntax identifierName = (IdentifierNameSyntax)Visit(node.IdentifierName);
		SyntaxToken empty = VisitToken(node.Empty);
		return node.Update(identifierName, empty);
	}

	public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
	{
		LiteralValueSyntax literal = (LiteralValueSyntax)Visit(node.Literal);
		return node.Update(literal);
	}

	public override SyntaxNode VisitOptionValues(OptionValuesSyntax node)
	{
		SeparatedSyntaxList<IdentifierNameOrEmptySyntax> options = VisitList(node.Options);
		return node.Update(options);
	}

	public override SyntaxNode VisitInt32SignedLiteralValue(Int32SignedLiteralValueSyntax node)
	{
		SyntaxToken sign = VisitToken(node.Sign);
		SyntaxToken number = VisitToken(node.Number);
		return node.Update(sign, number);
	}

	public override SyntaxNode VisitInt64SignedLiteralValue(Int64SignedLiteralValueSyntax node)
	{
		SyntaxToken sign = VisitToken(node.Sign);
		SyntaxToken number = VisitToken(node.Number);
		return node.Update(sign, number);
	}

	public override SyntaxNode VisitDecimalSignedLiteralValue(DecimalSignedLiteralValueSyntax node)
	{
		SyntaxToken sign = VisitToken(node.Sign);
		SyntaxToken number = VisitToken(node.Number);
		return node.Update(sign, number);
	}

	public override SyntaxNode VisitBooleanLiteralValue(BooleanLiteralValueSyntax node)
	{
		SyntaxToken value = VisitToken(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitStringLiteralValue(StringLiteralValueSyntax node)
	{
		SyntaxToken value = VisitToken(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitDateLiteralValue(DateLiteralValueSyntax node)
	{
		SyntaxToken value = VisitToken(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitTimeLiteralValue(TimeLiteralValueSyntax node)
	{
		SyntaxToken value = VisitToken(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitDateTimeLiteralValue(DateTimeLiteralValueSyntax node)
	{
		SyntaxToken value = VisitToken(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitInvalidPropertyValue(InvalidPropertyValueSyntax node)
	{
		SyntaxToken invalidToken = VisitToken(node.InvalidToken);
		return node.Update(invalidToken);
	}

	public override SyntaxNode VisitDecimalPlacesPropertyValue(DecimalPlacesPropertyValueSyntax node)
	{
		DecimalPlacesSyntax value = (DecimalPlacesSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitOptionValuePropertyValue(OptionValuePropertyValueSyntax node)
	{
		IdentifierNameSyntax value = (IdentifierNameSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitImagePropertyValue(ImagePropertyValueSyntax node)
	{
		IdentifierNameSyntax value = (IdentifierNameSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitPageFieldReferencePropertyValue(PageFieldReferencePropertyValueSyntax node)
	{
		IdentifierNameSyntax value = (IdentifierNameSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitMultilanguagePropertyValue(MultilanguagePropertyValueSyntax node)
	{
		CommaSeparatedIdentifierEqualsStringListSyntax value = (CommaSeparatedIdentifierEqualsStringListSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitLabelPropertyValue(LabelPropertyValueSyntax node)
	{
		LabelSyntax value = (LabelSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitMemberReferencePropertyValue(MemberReferencePropertyValueSyntax node)
	{
		IdentifierNameSyntax value = (IdentifierNameSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitPermissionPropertyValue(PermissionPropertyValueSyntax node)
	{
		SeparatedSyntaxList<PermissionSyntax> permissionProperties = VisitList(node.PermissionProperties);
		return node.Update(permissionProperties);
	}

	public override SyntaxNode VisitQualifiedObjectReferencePropertyValue(QualifiedObjectReferencePropertyValueSyntax node)
	{
		SyntaxToken objectType = VisitToken(node.ObjectType);
		ObjectNameOrIdSyntax objectNameOrId = (ObjectNameOrIdSyntax)Visit(node.ObjectNameOrId);
		return node.Update(objectType, objectNameOrId);
	}

	public override SyntaxNode VisitObjectReferencePropertyValue(ObjectReferencePropertyValueSyntax node)
	{
		ObjectNameOrIdSyntax objectNameOrId = (ObjectNameOrIdSyntax)Visit(node.ObjectNameOrId);
		return node.Update(objectNameOrId);
	}

	public override SyntaxNode VisitPermission(PermissionSyntax node)
	{
		SyntaxToken objectType = VisitToken(node.ObjectType);
		ObjectNameOrIdSyntax objectReference = (ObjectNameOrIdSyntax)Visit(node.ObjectReference);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		SyntaxToken permissions = VisitToken(node.Permissions);
		return node.Update(objectType, objectReference, equalsToken, permissions);
	}

	public override SyntaxNode VisitEnumPropertyValue(EnumPropertyValueSyntax node)
	{
		IdentifierNameSyntax value = (IdentifierNameSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitIdentifierOrLiteralPropertyValue(IdentifierOrLiteralPropertyValueSyntax node)
	{
		IdentifierOrLiteralExpressionSyntax identifier = (IdentifierOrLiteralExpressionSyntax)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override SyntaxNode VisitIdentifierOrLiteralExpression(IdentifierOrLiteralExpressionSyntax node)
	{
		CodeExpressionSyntax identifier = (CodeExpressionSyntax)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override SyntaxNode VisitObjectNameReference(ObjectNameReferenceSyntax node)
	{
		CodeExpressionSyntax identifier = (CodeExpressionSyntax)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override SyntaxNode VisitObjectNameOrId(ObjectNameOrIdSyntax node)
	{
		CodeExpressionSyntax identifier = (CodeExpressionSyntax)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override SyntaxNode VisitObjectId(ObjectIdSyntax node)
	{
		SyntaxToken value = VisitToken(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitDecimalPlaces(DecimalPlacesSyntax node)
	{
		SyntaxToken minimumToken = VisitToken(node.MinimumToken);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		SyntaxToken maximumToken = VisitToken(node.MaximumToken);
		return node.Update(minimumToken, colonToken, maximumToken);
	}

	public override SyntaxNode VisitMemberAccessOrIdentifierPropertyValue(MemberAccessOrIdentifierPropertyValueSyntax node)
	{
		CodeExpressionSyntax identifier = (CodeExpressionSyntax)Visit(node.Identifier);
		return node.Update(identifier);
	}

	public override SyntaxNode VisitPropertyName(PropertyNameSyntax node)
	{
		SyntaxToken identifier = VisitToken(node.Identifier);
		return node.Update(identifier);
	}

	public override SyntaxNode VisitCommaSeparatedPropertyValue(CommaSeparatedPropertyValueSyntax node)
	{
		SeparatedSyntaxList<IdentifierNameSyntax> values = VisitList(node.Values);
		return node.Update(values);
	}

	public override SyntaxNode VisitCommaSeparatedStringsPropertyValue(CommaSeparatedStringsPropertyValueSyntax node)
	{
		SeparatedSyntaxList<StringLiteralValueSyntax> values = VisitList(node.Values);
		return node.Update(values);
	}

	public override SyntaxNode VisitCommaSeparatedIdentifierEqualsStringList(CommaSeparatedIdentifierEqualsStringListSyntax node)
	{
		SeparatedSyntaxList<IdentifierEqualsStringSyntax> values = VisitList(node.Values);
		return node.Update(values);
	}

	public override SyntaxNode VisitCommaSeparatedIdentifierEqualsLiteralList(CommaSeparatedIdentifierEqualsLiteralListSyntax node)
	{
		SeparatedSyntaxList<IdentifierEqualsLiteralSyntax> values = VisitList(node.Values);
		return node.Update(values);
	}

	public override SyntaxNode VisitCommaSeparatedIdentifierEqualsStringListPropertyValue(CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax node)
	{
		CommaSeparatedIdentifierEqualsStringListSyntax list = (CommaSeparatedIdentifierEqualsStringListSyntax)Visit(node.List);
		return node.Update(list);
	}

	public override SyntaxNode VisitCommaSeparatedIdentifierOrLiteralPropertyValue(CommaSeparatedIdentifierOrLiteralPropertyValueSyntax node)
	{
		SeparatedSyntaxList<IdentifierOrLiteralExpressionSyntax> values = VisitList(node.Values);
		return node.Update(values);
	}

	public override SyntaxNode VisitLabel(LabelSyntax node)
	{
		StringLiteralValueSyntax labelText = (StringLiteralValueSyntax)Visit(node.LabelText);
		SyntaxToken commaToken = VisitToken(node.CommaToken);
		CommaSeparatedIdentifierEqualsLiteralListSyntax properties = (CommaSeparatedIdentifierEqualsLiteralListSyntax)Visit(node.Properties);
		return node.Update(labelText, commaToken, properties);
	}

	public override SyntaxNode VisitOptionValuesPropertyValue(OptionValuesPropertyValueSyntax node)
	{
		OptionValuesSyntax value = (OptionValuesSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitBooleanPropertyValue(BooleanPropertyValueSyntax node)
	{
		BooleanLiteralValueSyntax value = (BooleanLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitInt32PropertyValue(Int32PropertyValueSyntax node)
	{
		Int32SignedLiteralValueSyntax value = (Int32SignedLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitInt64PropertyValue(Int64PropertyValueSyntax node)
	{
		Int64SignedLiteralValueSyntax value = (Int64SignedLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitDecimalPropertyValue(DecimalPropertyValueSyntax node)
	{
		DecimalSignedLiteralValueSyntax value = (DecimalSignedLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitTimePropertyValue(TimePropertyValueSyntax node)
	{
		TimeLiteralValueSyntax value = (TimeLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitDatePropertyValue(DatePropertyValueSyntax node)
	{
		DateLiteralValueSyntax value = (DateLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitDateTimePropertyValue(DateTimePropertyValueSyntax node)
	{
		DateTimeLiteralValueSyntax value = (DateTimeLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitStringPropertyValue(StringPropertyValueSyntax node)
	{
		StringLiteralValueSyntax value = (StringLiteralValueSyntax)Visit(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitStyleExpressionPropertyValue(StyleExpressionPropertyValueSyntax node)
	{
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		return node.Update(expression);
	}

	public override SyntaxNode VisitBooleanExpressionPropertyValue(BooleanExpressionPropertyValueSyntax node)
	{
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		return node.Update(expression);
	}

	public override SyntaxNode VisitTextExpressionPropertyValue(TextExpressionPropertyValueSyntax node)
	{
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		return node.Update(expression);
	}

	public override SyntaxNode VisitIntegerExpressionPropertyValue(IntegerExpressionPropertyValueSyntax node)
	{
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		return node.Update(expression);
	}

	public override SyntaxNode VisitClientSideBooleanExpressionPropertyValue(ClientSideBooleanExpressionPropertyValueSyntax node)
	{
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		return node.Update(expression);
	}

	public override SyntaxNode VisitTableFilterPropertyValue(TableFilterPropertyValueSyntax node)
	{
		SeparatedSyntaxList<PropertyExpressionSyntax> conditions = VisitList(node.Conditions);
		return node.Update(conditions);
	}

	public override SyntaxNode VisitOrderByPropertyValue(OrderByPropertyValueSyntax node)
	{
		SeparatedSyntaxList<OrderByExpressionSyntax> order = VisitList(node.Order);
		return node.Update(order);
	}

	public override SyntaxNode VisitOrderByExpression(OrderByExpressionSyntax node)
	{
		SyntaxToken orderSpecifier = VisitToken(node.OrderSpecifier);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		SeparatedSyntaxList<IdentifierNameSyntax> sortingFields = VisitList(node.SortingFields);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(orderSpecifier, openParenthesisToken, sortingFields, closeParenthesisToken);
	}

	public override SyntaxNode VisitQueryDataItemLinkPropertyValue(QueryDataItemLinkPropertyValueSyntax node)
	{
		SeparatedSyntaxList<QueryDataItemLinkExpressionSyntax> joins = VisitList(node.Joins);
		return node.Update(joins);
	}

	public override SyntaxNode VisitQueryDataItemLinkExpression(QueryDataItemLinkExpressionSyntax node)
	{
		IdentifierNameSyntax sourceField = (IdentifierNameSyntax)Visit(node.SourceField);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		MemberAccessExpressionSyntax relatedField = (MemberAccessExpressionSyntax)Visit(node.RelatedField);
		return node.Update(sourceField, equalsToken, relatedField);
	}

	public override SyntaxNode VisitReportDataItemLinkPropertyValue(ReportDataItemLinkPropertyValueSyntax node)
	{
		SeparatedSyntaxList<ReportDataItemLinkExpressionSyntax> joins = VisitList(node.Joins);
		return node.Update(joins);
	}

	public override SyntaxNode VisitReportDataItemLinkExpression(ReportDataItemLinkExpressionSyntax node)
	{
		IdentifierNameSyntax sourceField = (IdentifierNameSyntax)Visit(node.SourceField);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		SyntaxToken fieldKeywordToken = VisitToken(node.FieldKeywordToken);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax relatedField = (IdentifierNameSyntax)Visit(node.RelatedField);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(sourceField, equalsToken, fieldKeywordToken, openParenthesisToken, relatedField, closeParenthesisToken);
	}

	public override SyntaxNode VisitTableViewPropertyValue(TableViewPropertyValueSyntax node)
	{
		SortingSyntax sorting = (SortingSyntax)Visit(node.Sorting);
		OrderSyntax order = (OrderSyntax)Visit(node.Order);
		WhereExpressionSyntax tableFilter = (WhereExpressionSyntax)Visit(node.TableFilter);
		return node.Update(sorting, order, tableFilter);
	}

	public override SyntaxNode VisitFiltersPropertyValue(FiltersPropertyValueSyntax node)
	{
		WhereExpressionSyntax tableFilter = (WhereExpressionSyntax)Visit(node.TableFilter);
		return node.Update(tableFilter);
	}

	public override SyntaxNode VisitSorting(SortingSyntax node)
	{
		SyntaxToken sortingToken = VisitToken(node.SortingToken);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		SeparatedSyntaxList<IdentifierNameSyntax> sortingFields = VisitList(node.SortingFields);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(sortingToken, openParenthesisToken, sortingFields, closeParenthesisToken);
	}

	public override SyntaxNode VisitOrder(OrderSyntax node)
	{
		SyntaxToken orderToken = VisitToken(node.OrderToken);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		SyntaxToken orderBy = VisitToken(node.OrderBy);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(orderToken, openParenthesisToken, orderBy, closeParenthesisToken);
	}

	public override SyntaxNode VisitTableRelationPropertyValue(TableRelationPropertyValueSyntax node)
	{
		IfTableRelationSyntax ifExpression = (IfTableRelationSyntax)Visit(node.IfExpression);
		MemberAccessOrIdentifierPropertyValueSyntax relatedTableField = (MemberAccessOrIdentifierPropertyValueSyntax)Visit(node.RelatedTableField);
		WhereExpressionSyntax tableFilter = (WhereExpressionSyntax)Visit(node.TableFilter);
		ElseTableRelationSyntax elseExpression = (ElseTableRelationSyntax)Visit(node.ElseExpression);
		return node.Update(ifExpression, relatedTableField, tableFilter, elseExpression);
	}

	public override SyntaxNode VisitIfTableRelation(IfTableRelationSyntax node)
	{
		SyntaxToken ifKeywordToken = VisitToken(node.IfKeywordToken);
		TableFilterExpressionSyntax ifTableRelationCondition = (TableFilterExpressionSyntax)Visit(node.IfTableRelationCondition);
		return node.Update(ifKeywordToken, ifTableRelationCondition);
	}

	public override SyntaxNode VisitTableFilterExpression(TableFilterExpressionSyntax node)
	{
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		SeparatedSyntaxList<PropertyExpressionSyntax> conditions = VisitList(node.Conditions);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(openParenthesisToken, conditions, closeParenthesisToken);
	}

	public override SyntaxNode VisitElseTableRelation(ElseTableRelationSyntax node)
	{
		SyntaxToken elseKeywordToken = VisitToken(node.ElseKeywordToken);
		TableRelationPropertyValueSyntax elseTableRelationCondition = (TableRelationPropertyValueSyntax)Visit(node.ElseTableRelationCondition);
		return node.Update(elseKeywordToken, elseTableRelationCondition);
	}

	public override SyntaxNode VisitWhereExpression(WhereExpressionSyntax node)
	{
		SyntaxToken whereKeywordToken = VisitToken(node.WhereKeywordToken);
		TableFilterExpressionSyntax filter = (TableFilterExpressionSyntax)Visit(node.Filter);
		return node.Update(whereKeywordToken, filter);
	}

	public override SyntaxNode VisitConstExpression(ConstExpressionSyntax node)
	{
		IdentifierNameSyntax leftHandSide = (IdentifierNameSyntax)Visit(node.LeftHandSide);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		SyntaxToken constKeywordToken = VisitToken(node.ConstKeywordToken);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierOrLiteralExpressionSyntax identifier = (IdentifierOrLiteralExpressionSyntax)Visit(node.Identifier);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(leftHandSide, equalsToken, constKeywordToken, openParenthesisToken, identifier, closeParenthesisToken);
	}

	public override SyntaxNode VisitSimpleFieldExpression(SimpleFieldExpressionSyntax node)
	{
		IdentifierNameSyntax leftHandSide = (IdentifierNameSyntax)Visit(node.LeftHandSide);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		SyntaxToken fieldKeywordToken = VisitToken(node.FieldKeywordToken);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax identifier = (IdentifierNameSyntax)Visit(node.Identifier);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(leftHandSide, equalsToken, fieldKeywordToken, openParenthesisToken, identifier, closeParenthesisToken);
	}

	public override SyntaxNode VisitInvalidPropertyExpression(InvalidPropertyExpressionSyntax node)
	{
		IdentifierNameSyntax leftHandSide = (IdentifierNameSyntax)Visit(node.LeftHandSide);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		SyntaxToken constOrFieldOrFilterKeywordToken = VisitToken(node.ConstOrFieldOrFilterKeywordToken);
		SyntaxToken invalidExpression = VisitToken(node.InvalidExpression);
		return node.Update(leftHandSide, equalsToken, constOrFieldOrFilterKeywordToken, invalidExpression);
	}

	public override SyntaxNode VisitTableCalculationFormula(TableCalculationFormulaSyntax node)
	{
		SyntaxToken sign = VisitToken(node.Sign);
		SyntaxToken formulaKeywordToken = VisitToken(node.FormulaKeywordToken);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		IdentifierNameSyntax table = (IdentifierNameSyntax)Visit(node.Table);
		WhereExpressionSyntax whereExpression = (WhereExpressionSyntax)Visit(node.WhereExpression);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(sign, formulaKeywordToken, openParenthesisToken, table, whereExpression, closeParenthesisToken);
	}

	public override SyntaxNode VisitFieldCalculationFormula(FieldCalculationFormulaSyntax node)
	{
		SyntaxToken sign = VisitToken(node.Sign);
		SyntaxToken formulaKeywordToken = VisitToken(node.FormulaKeywordToken);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		MemberAccessExpressionSyntax field = (MemberAccessExpressionSyntax)Visit(node.Field);
		WhereExpressionSyntax whereExpression = (WhereExpressionSyntax)Visit(node.WhereExpression);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(sign, formulaKeywordToken, openParenthesisToken, field, whereExpression, closeParenthesisToken);
	}

	public override SyntaxNode VisitFieldFilterExpression(FieldFilterExpressionSyntax node)
	{
		IdentifierNameSyntax leftHandSide = (IdentifierNameSyntax)Visit(node.LeftHandSide);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		SyntaxToken fieldFormulaKeywordToken = VisitToken(node.FieldFormulaKeywordToken);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		SyntaxToken filterFormulaKeywordToken = VisitToken(node.FilterFormulaKeywordToken);
		SyntaxToken openParenthesisToken2 = VisitToken(node.OpenParenthesisToken1);
		IdentifierNameSyntax identifier = (IdentifierNameSyntax)Visit(node.Identifier);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken1);
		SyntaxToken closeParenthesisToken2 = VisitToken(node.CloseParenthesisToken);
		return node.Update(leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, filterFormulaKeywordToken, openParenthesisToken2, identifier, closeParenthesisToken, closeParenthesisToken2);
	}

	public override SyntaxNode VisitFieldUpperLimitExpression(FieldUpperLimitExpressionSyntax node)
	{
		IdentifierNameSyntax leftHandSide = (IdentifierNameSyntax)Visit(node.LeftHandSide);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		SyntaxToken fieldFormulaKeywordToken = VisitToken(node.FieldFormulaKeywordToken);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		SyntaxToken upperLimitFormulaKeywordToken = VisitToken(node.UpperLimitFormulaKeywordToken);
		SyntaxToken openParenthesisToken2 = VisitToken(node.OpenParenthesisToken1);
		IdentifierNameSyntax identifier = (IdentifierNameSyntax)Visit(node.Identifier);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken1);
		SyntaxToken closeParenthesisToken2 = VisitToken(node.CloseParenthesisToken);
		return node.Update(leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, upperLimitFormulaKeywordToken, openParenthesisToken2, identifier, closeParenthesisToken, closeParenthesisToken2);
	}

	public override SyntaxNode VisitFieldUpperLimitFilterExpression(FieldUpperLimitFilterExpressionSyntax node)
	{
		IdentifierNameSyntax leftHandSide = (IdentifierNameSyntax)Visit(node.LeftHandSide);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		SyntaxToken fieldFormulaKeywordToken = VisitToken(node.FieldFormulaKeywordToken);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken1);
		SyntaxToken upperLimitFormulaKeywordToken = VisitToken(node.UpperLimitFormulaKeywordToken);
		SyntaxToken openParenthesisToken2 = VisitToken(node.OpenParenthesisToken2);
		SyntaxToken filterFormulaKeywordToken = VisitToken(node.FilterFormulaKeywordToken);
		SyntaxToken openParenthesisToken3 = VisitToken(node.OpenParenthesisToken3);
		IdentifierNameSyntax identifier = (IdentifierNameSyntax)Visit(node.Identifier);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken3);
		SyntaxToken closeParenthesisToken2 = VisitToken(node.CloseParenthesisToken2);
		SyntaxToken closeParenthesisToken3 = VisitToken(node.CloseParenthesisToken1);
		return node.Update(leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, upperLimitFormulaKeywordToken, openParenthesisToken2, filterFormulaKeywordToken, openParenthesisToken3, identifier, closeParenthesisToken, closeParenthesisToken2, closeParenthesisToken3);
	}

	public override SyntaxNode VisitFilterExpression(FilterExpressionSyntax node)
	{
		IdentifierNameSyntax leftHandSide = (IdentifierNameSyntax)Visit(node.LeftHandSide);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		SyntaxToken filterFormulaKeywordToken = VisitToken(node.FilterFormulaKeywordToken);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		FilterExpressionValueSyntax filter = (FilterExpressionValueSyntax)Visit(node.Filter);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(leftHandSide, equalsToken, filterFormulaKeywordToken, openParenthesisToken, filter, closeParenthesisToken);
	}

	public override SyntaxNode VisitParenthesizedFilterExpressionValue(ParenthesizedFilterExpressionValueSyntax node)
	{
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		FilterExpressionValueSyntax expression = (FilterExpressionValueSyntax)Visit(node.Expression);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(openParenthesisToken, expression, closeParenthesisToken);
	}

	public override SyntaxNode VisitUnaryFilterExpressionValue(UnaryFilterExpressionValueSyntax node)
	{
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		IdentifierOrLiteralExpressionSyntax identifier = (IdentifierOrLiteralExpressionSyntax)Visit(node.Identifier);
		return node.Update(operatorToken, identifier);
	}

	public override SyntaxNode VisitBinaryFilterExpressionValue(BinaryFilterExpressionValueSyntax node)
	{
		SyntaxNode left = Visit(node.Left);
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		SyntaxNode right = Visit(node.Right);
		return node.Update(left, operatorToken, right);
	}

	public override SyntaxNode VisitRangeFromFilterExpressionValue(RangeFromFilterExpressionValueSyntax node)
	{
		IdentifierOrLiteralExpressionSyntax left = (IdentifierOrLiteralExpressionSyntax)Visit(node.Left);
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		return node.Update(left, operatorToken);
	}

	public override SyntaxNode VisitInvalidFilterExpressionValue(InvalidFilterExpressionValueSyntax node)
	{
		SyntaxToken invalidToken = VisitToken(node.InvalidToken);
		return node.Update(invalidToken);
	}

	public override SyntaxNode VisitEmptyProperty(EmptyPropertySyntax node)
	{
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(semicolonToken);
	}

	public override SyntaxNode VisitProperty(PropertySyntax node)
	{
		PropertyNameSyntax name = (PropertyNameSyntax)Visit(node.Name);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		PropertyValueSyntax value = (PropertyValueSyntax)Visit(node.Value);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(name, equalsToken, value, semicolonToken);
	}

	public override SyntaxNode VisitVarSection(VarSectionSyntax node)
	{
		SyntaxToken varKeyword = VisitToken(node.VarKeyword);
		SyntaxList<VariableDeclarationSyntax> variables = VisitList(node.Variables);
		return node.Update(varKeyword, variables);
	}

	public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
	{
		SyntaxList<MemberAttributeSyntax> attributes = VisitList(node.Attributes);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		TypeReferenceBaseSyntax type = (TypeReferenceBaseSyntax)Visit(node.Type);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributes, name, colonToken, type, semicolonToken);
	}

	public override SyntaxNode VisitParameter(ParameterSyntax node)
	{
		SyntaxToken varKeyword = VisitToken(node.VarKeyword);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		TypeReferenceBaseSyntax type = (TypeReferenceBaseSyntax)Visit(node.Type);
		return node.Update(varKeyword, name, colonToken, type);
	}

	public override SyntaxNode VisitReturnValue(ReturnValueSyntax node)
	{
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		DataTypeSyntax dataType = (DataTypeSyntax)Visit(node.DataType);
		return node.Update(name, colonToken, dataType);
	}

	public override SyntaxNode VisitSimpleTypeReference(SimpleTypeReferenceSyntax node)
	{
		ArraySyntax array = (ArraySyntax)Visit(node.Array);
		DataTypeSyntax dataType = (DataTypeSyntax)Visit(node.DataType);
		return node.Update(array, dataType);
	}

	public override SyntaxNode VisitRecordTypeReference(RecordTypeReferenceSyntax node)
	{
		ArraySyntax array = (ArraySyntax)Visit(node.Array);
		DataTypeSyntax dataType = (DataTypeSyntax)Visit(node.DataType);
		SyntaxToken temporary = VisitToken(node.Temporary);
		return node.Update(array, dataType, temporary);
	}

	public override SyntaxNode VisitDotNetTypeReference(DotNetTypeReferenceSyntax node)
	{
		ArraySyntax array = (ArraySyntax)Visit(node.Array);
		DataTypeSyntax dataType = (DataTypeSyntax)Visit(node.DataType);
		return node.Update(array, dataType);
	}

	public override SyntaxNode VisitArray(ArraySyntax node)
	{
		SyntaxToken arrayKeyword = VisitToken(node.ArrayKeyword);
		BracketedDimensionListSyntax dimensionList = (BracketedDimensionListSyntax)Visit(node.DimensionList);
		SyntaxToken ofKeyword = VisitToken(node.OfKeyword);
		return node.Update(arrayKeyword, dimensionList, ofKeyword);
	}

	public override SyntaxNode VisitBracketedDimensionList(BracketedDimensionListSyntax node)
	{
		SyntaxToken openBracketToken = VisitToken(node.OpenBracketToken);
		SeparatedSyntaxList<DimensionSyntax> dimensions = VisitList(node.Dimensions);
		SyntaxToken closeBracketToken = VisitToken(node.CloseBracketToken);
		return node.Update(openBracketToken, dimensions, closeBracketToken);
	}

	public override SyntaxNode VisitDimension(DimensionSyntax node)
	{
		SyntaxToken value = VisitToken(node.Value);
		return node.Update(value);
	}

	public override SyntaxNode VisitOptionDataType(OptionDataTypeSyntax node)
	{
		SyntaxToken typeName = VisitToken(node.TypeName);
		OptionValuesSyntax optionValues = (OptionValuesSyntax)Visit(node.OptionValues);
		return node.Update(typeName, optionValues);
	}

	public override SyntaxNode VisitSimpleNamedDataType(SimpleNamedDataTypeSyntax node)
	{
		SyntaxToken typeName = VisitToken(node.TypeName);
		return node.Update(typeName);
	}

	public override SyntaxNode VisitGenericNamedDataType(GenericNamedDataTypeSyntax node)
	{
		SyntaxToken typeName = VisitToken(node.TypeName);
		SyntaxToken ofKeyword = VisitToken(node.OfKeyword);
		SyntaxToken openBracketToken = VisitToken(node.OpenBracketToken);
		SeparatedSyntaxList<DataTypeSyntax> typeArguments = VisitList(node.TypeArguments);
		SyntaxToken closeBracketToken = VisitToken(node.CloseBracketToken);
		return node.Update(typeName, ofKeyword, openBracketToken, typeArguments, closeBracketToken);
	}

	public override SyntaxNode VisitTextConstDataType(TextConstDataTypeSyntax node)
	{
		SyntaxToken typeName = VisitToken(node.TypeName);
		CommaSeparatedIdentifierEqualsStringListSyntax multilanguage = (CommaSeparatedIdentifierEqualsStringListSyntax)Visit(node.Multilanguage);
		return node.Update(typeName, multilanguage);
	}

	public override SyntaxNode VisitLabelDataType(LabelDataTypeSyntax node)
	{
		SyntaxToken typeName = VisitToken(node.TypeName);
		LabelSyntax label = (LabelSyntax)Visit(node.Label);
		return node.Update(typeName, label);
	}

	public override SyntaxNode VisitDotNetDataType(DotNetDataTypeSyntax node)
	{
		SyntaxToken typeName = VisitToken(node.TypeName);
		return node.Update(typeName);
	}

	public override SyntaxNode VisitLengthDataType(LengthDataTypeSyntax node)
	{
		SyntaxToken typeName = VisitToken(node.TypeName);
		SyntaxToken openBracketToken = VisitToken(node.OpenBracketToken);
		SyntaxToken length = VisitToken(node.Length);
		SyntaxToken closeBracketToken = VisitToken(node.CloseBracketToken);
		return node.Update(typeName, openBracketToken, length, closeBracketToken);
	}

	public override SyntaxNode VisitSubtypedDataType(SubtypedDataTypeSyntax node)
	{
		SyntaxToken typeName = VisitToken(node.TypeName);
		ObjectNameOrIdSyntax subtype = (ObjectNameOrIdSyntax)Visit(node.Subtype);
		return node.Update(typeName, subtype);
	}

	public override SyntaxNode VisitEnumDataType(EnumDataTypeSyntax node)
	{
		SyntaxToken typeName = VisitToken(node.TypeName);
		IdentifierNameSyntax enumTypeName = (IdentifierNameSyntax)Visit(node.EnumTypeName);
		return node.Update(typeName, enumTypeName);
	}

	public override SyntaxNode VisitParameterList(ParameterListSyntax node)
	{
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		SeparatedSyntaxList<ParameterSyntax> parameters = VisitList(node.Parameters);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(openParenthesisToken, parameters, closeParenthesisToken);
	}

	public override SyntaxNode VisitTriggerDeclaration(TriggerDeclarationSyntax node)
	{
		SyntaxList<MemberAttributeSyntax> attributes = VisitList(node.Attributes);
		SyntaxToken triggerKeyword = VisitToken(node.TriggerKeyword);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		ReturnValueSyntax returnValue = (ReturnValueSyntax)Visit(node.ReturnValue);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		VarSectionSyntax variables = (VarSectionSyntax)Visit(node.Variables);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		return node.Update(attributes, triggerKeyword, name, parameterList, returnValue, semicolonToken, variables, body);
	}

	public override SyntaxNode VisitEventTriggerDeclaration(EventTriggerDeclarationSyntax node)
	{
		SyntaxList<MemberAttributeSyntax> attributes = VisitList(node.Attributes);
		SyntaxToken triggerKeyword = VisitToken(node.TriggerKeyword);
		IdentifierNameSyntax publisher = (IdentifierNameSyntax)Visit(node.Publisher);
		SyntaxToken colonColonToken = VisitToken(node.ColonColonToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		ReturnValueSyntax returnValue = (ReturnValueSyntax)Visit(node.ReturnValue);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		VarSectionSyntax variables = (VarSectionSyntax)Visit(node.Variables);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		return node.Update(attributes, triggerKeyword, publisher, colonColonToken, name, parameterList, returnValue, semicolonToken, variables, body);
	}

	public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
	{
		SyntaxList<MemberAttributeSyntax> attributes = VisitList(node.Attributes);
		SyntaxToken localKeyword = VisitToken(node.LocalKeyword);
		SyntaxToken procedureKeyword = VisitToken(node.ProcedureKeyword);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		ReturnValueSyntax returnValue = (ReturnValueSyntax)Visit(node.ReturnValue);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		VarSectionSyntax variables = (VarSectionSyntax)Visit(node.Variables);
		BlockSyntax body = (BlockSyntax)Visit(node.Body);
		return node.Update(attributes, localKeyword, procedureKeyword, name, parameterList, returnValue, semicolonToken, variables, body);
	}

	public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
	{
		SyntaxList<MemberAttributeSyntax> attributes = VisitList(node.Attributes);
		SyntaxToken eventKeyword = VisitToken(node.EventKeyword);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		ParameterListSyntax parameterList = (ParameterListSyntax)Visit(node.ParameterList);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(attributes, eventKeyword, name, parameterList, semicolonToken);
	}

	public override SyntaxNode VisitBlock(BlockSyntax node)
	{
		SyntaxToken beginKeywordToken = VisitToken(node.BeginKeywordToken);
		SyntaxList<StatementSyntax> statements = VisitList(node.Statements);
		SyntaxToken endKeywordToken = VisitToken(node.EndKeywordToken);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(beginKeywordToken, statements, endKeywordToken, semicolonToken);
	}

	public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
	{
		SyntaxToken ifKeywordToken = VisitToken(node.IfKeywordToken);
		CodeExpressionSyntax condition = (CodeExpressionSyntax)Visit(node.Condition);
		SyntaxToken thenKeywordToken = VisitToken(node.ThenKeywordToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		SyntaxToken elseKeywordToken = VisitToken(node.ElseKeywordToken);
		StatementSyntax elseStatement = (StatementSyntax)Visit(node.ElseStatement);
		return node.Update(ifKeywordToken, condition, thenKeywordToken, statement, elseKeywordToken, elseStatement);
	}

	public override SyntaxNode VisitOrphanedElseStatement(OrphanedElseStatementSyntax node)
	{
		SyntaxToken elseKeywordToken = VisitToken(node.ElseKeywordToken);
		StatementSyntax elseStatement = (StatementSyntax)Visit(node.ElseStatement);
		return node.Update(elseKeywordToken, elseStatement);
	}

	public override SyntaxNode VisitAssignmentStatement(AssignmentStatementSyntax node)
	{
		CodeExpressionSyntax target = (CodeExpressionSyntax)Visit(node.Target);
		SyntaxToken assignmentToken = VisitToken(node.AssignmentToken);
		CodeExpressionSyntax source = (CodeExpressionSyntax)Visit(node.Source);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(target, assignmentToken, source, semicolonToken);
	}

	public override SyntaxNode VisitCompoundAssignmentStatement(CompoundAssignmentStatementSyntax node)
	{
		CodeExpressionSyntax target = (CodeExpressionSyntax)Visit(node.Target);
		SyntaxToken assignmentToken = VisitToken(node.AssignmentToken);
		CodeExpressionSyntax source = (CodeExpressionSyntax)Visit(node.Source);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(target, assignmentToken, source, semicolonToken);
	}

	public override SyntaxNode VisitForStatement(ForStatementSyntax node)
	{
		SyntaxToken forKeywordToken = VisitToken(node.ForKeywordToken);
		CodeExpressionSyntax loopVariable = (CodeExpressionSyntax)Visit(node.LoopVariable);
		SyntaxToken assignToken = VisitToken(node.AssignToken);
		CodeExpressionSyntax initialValue = (CodeExpressionSyntax)Visit(node.InitialValue);
		SyntaxToken operatorKeywordToken = VisitToken(node.OperatorKeywordToken);
		CodeExpressionSyntax endValue = (CodeExpressionSyntax)Visit(node.EndValue);
		SyntaxToken doKeywordToken = VisitToken(node.DoKeywordToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(forKeywordToken, loopVariable, assignToken, initialValue, operatorKeywordToken, endValue, doKeywordToken, statement);
	}

	public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
	{
		SyntaxToken whileKeywordToken = VisitToken(node.WhileKeywordToken);
		CodeExpressionSyntax condition = (CodeExpressionSyntax)Visit(node.Condition);
		SyntaxToken doKeywordToken = VisitToken(node.DoKeywordToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(whileKeywordToken, condition, doKeywordToken, statement);
	}

	public override SyntaxNode VisitWithStatement(WithStatementSyntax node)
	{
		SyntaxToken withKeywordToken = VisitToken(node.WithKeywordToken);
		CodeExpressionSyntax withId = (CodeExpressionSyntax)Visit(node.WithId);
		SyntaxToken doKeywordToken = VisitToken(node.DoKeywordToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(withKeywordToken, withId, doKeywordToken, statement);
	}

	public override SyntaxNode VisitExitStatement(ExitStatementSyntax node)
	{
		SyntaxToken exitKeywordToken = VisitToken(node.ExitKeywordToken);
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		CodeExpressionSyntax exitValue = (CodeExpressionSyntax)Visit(node.ExitValue);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(exitKeywordToken, openParenthesisToken, exitValue, closeParenthesisToken, semicolonToken);
	}

	public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
	{
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(expression, semicolonToken);
	}

	public override SyntaxNode VisitRepeatStatement(RepeatStatementSyntax node)
	{
		SyntaxToken repeatKeywordToken = VisitToken(node.RepeatKeywordToken);
		SyntaxList<StatementSyntax> statements = VisitList(node.Statements);
		SyntaxToken untilKeywordToken = VisitToken(node.UntilKeywordToken);
		CodeExpressionSyntax condition = (CodeExpressionSyntax)Visit(node.Condition);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(repeatKeywordToken, statements, untilKeywordToken, condition, semicolonToken);
	}

	public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
	{
		SyntaxToken forEachKeywordToken = VisitToken(node.ForEachKeywordToken);
		IdentifierNameSyntax iterationVariable = (IdentifierNameSyntax)Visit(node.IterationVariable);
		SyntaxToken inKeywordToken = VisitToken(node.InKeywordToken);
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		SyntaxToken doKeywordToken = VisitToken(node.DoKeywordToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(forEachKeywordToken, iterationVariable, inKeywordToken, expression, doKeywordToken, statement);
	}

	public override SyntaxNode VisitBreakStatement(BreakStatementSyntax node)
	{
		SyntaxToken breakKeyword = VisitToken(node.BreakKeyword);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(breakKeyword, semicolonToken);
	}

	public override SyntaxNode VisitAssertErrorStatement(AssertErrorStatementSyntax node)
	{
		SyntaxToken assertErrorKeywordToken = VisitToken(node.AssertErrorKeywordToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(assertErrorKeywordToken, statement);
	}

	public override SyntaxNode VisitCaseStatement(CaseStatementSyntax node)
	{
		SyntaxToken caseKeywordToken = VisitToken(node.CaseKeywordToken);
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		SyntaxToken ofKeywordToken = VisitToken(node.OfKeywordToken);
		SyntaxList<CaseLineSyntax> caseLines = VisitList(node.CaseLines);
		CaseElseSyntax caseElse = (CaseElseSyntax)Visit(node.CaseElse);
		SyntaxToken endKeywordToken = VisitToken(node.EndKeywordToken);
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(caseKeywordToken, expression, ofKeywordToken, caseLines, caseElse, endKeywordToken, semicolonToken);
	}

	public override SyntaxNode VisitCaseLine(CaseLineSyntax node)
	{
		SeparatedSyntaxList<CodeExpressionSyntax> expressions = VisitList(node.Expressions);
		SyntaxToken colonToken = VisitToken(node.ColonToken);
		StatementSyntax statement = (StatementSyntax)Visit(node.Statement);
		return node.Update(expressions, colonToken, statement);
	}

	public override SyntaxNode VisitCaseElse(CaseElseSyntax node)
	{
		SyntaxToken elseKeywordToken = VisitToken(node.ElseKeywordToken);
		SyntaxList<StatementSyntax> elseStatements = VisitList(node.ElseStatements);
		return node.Update(elseKeywordToken, elseStatements);
	}

	public override SyntaxNode VisitEmptyStatement(EmptyStatementSyntax node)
	{
		SyntaxToken semicolonToken = VisitToken(node.SemicolonToken);
		return node.Update(semicolonToken);
	}

	public override SyntaxNode VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
	{
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(openParenthesisToken, expression, closeParenthesisToken);
	}

	public override SyntaxNode VisitElementAccessExpression(ElementAccessExpressionSyntax node)
	{
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		BracketedArgumentListSyntax argumentList = (BracketedArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(expression, argumentList);
	}

	public override SyntaxNode VisitBracketedArgumentList(BracketedArgumentListSyntax node)
	{
		SyntaxToken openBracketToken = VisitToken(node.OpenBracketToken);
		SeparatedSyntaxList<CodeExpressionSyntax> arguments = VisitList(node.Arguments);
		SyntaxToken closeBracketToken = VisitToken(node.CloseBracketToken);
		return node.Update(openBracketToken, arguments, closeBracketToken);
	}

	public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
	{
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		ArgumentListSyntax argumentList = (ArgumentListSyntax)Visit(node.ArgumentList);
		return node.Update(expression, argumentList);
	}

	public override SyntaxNode VisitArgumentList(ArgumentListSyntax node)
	{
		SyntaxToken openParenthesisToken = VisitToken(node.OpenParenthesisToken);
		SeparatedSyntaxList<CodeExpressionSyntax> arguments = VisitList(node.Arguments);
		SyntaxToken closeParenthesisToken = VisitToken(node.CloseParenthesisToken);
		return node.Update(openParenthesisToken, arguments, closeParenthesisToken);
	}

	public override SyntaxNode VisitUnaryExpression(UnaryExpressionSyntax node)
	{
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		return node.Update(operatorToken, expression);
	}

	public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
	{
		CodeExpressionSyntax left = (CodeExpressionSyntax)Visit(node.Left);
		SyntaxToken operatorToken = VisitToken(node.OperatorToken);
		CodeExpressionSyntax right = (CodeExpressionSyntax)Visit(node.Right);
		return node.Update(left, operatorToken, right);
	}

	public override SyntaxNode VisitInListExpression(InListExpressionSyntax node)
	{
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		SyntaxToken inKeywordToken = VisitToken(node.InKeywordToken);
		BracketedArgumentListSyntax listExpression = (BracketedArgumentListSyntax)Visit(node.ListExpression);
		return node.Update(expression, inKeywordToken, listExpression);
	}

	public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
	{
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		SyntaxToken dotToken = VisitToken(node.DotToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		return node.Update(expression, dotToken, name);
	}

	public override SyntaxNode VisitOptionAccessExpression(OptionAccessExpressionSyntax node)
	{
		CodeExpressionSyntax expression = (CodeExpressionSyntax)Visit(node.Expression);
		SyntaxToken colonColonToken = VisitToken(node.ColonColonToken);
		IdentifierNameSyntax name = (IdentifierNameSyntax)Visit(node.Name);
		return node.Update(expression, colonColonToken, name);
	}

	public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
	{
		SyntaxToken identifier = VisitToken(node.Identifier);
		return node.Update(identifier);
	}

	public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node)
	{
		NameSyntax left = (NameSyntax)Visit(node.Left);
		SyntaxToken dotToken = VisitToken(node.DotToken);
		SimpleNameSyntax right = (SimpleNameSyntax)Visit(node.Right);
		return node.Update(left, dotToken, right);
	}

	public override SyntaxNode VisitIdentifierEqualsString(IdentifierEqualsStringSyntax node)
	{
		SyntaxToken identifier = VisitToken(node.Identifier);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		StringLiteralValueSyntax stringLiteral = (StringLiteralValueSyntax)Visit(node.StringLiteral);
		return node.Update(identifier, equalsToken, stringLiteral);
	}

	public override SyntaxNode VisitIdentifierEqualsLiteral(IdentifierEqualsLiteralSyntax node)
	{
		SyntaxToken identifier = VisitToken(node.Identifier);
		SyntaxToken equalsToken = VisitToken(node.EqualsToken);
		LiteralValueSyntax literal = (LiteralValueSyntax)Visit(node.Literal);
		return node.Update(identifier, equalsToken, literal);
	}

	public override SyntaxNode VisitSkippedTokensTrivia(SkippedTokensTriviaSyntax node)
	{
		SyntaxNode tokens = Visit(node.Tokens);
		return node.Update(tokens);
	}

	protected SyntaxRewriter(bool visitIntoStructuredTrivia = false)
	{
		VisitIntoStructuredTrivia = visitIntoStructuredTrivia;
	}

	public virtual SyntaxToken VisitToken(SyntaxToken token)
	{
		InternalSyntaxNode node = token.Node;
		if (node == null)
		{
			return token;
		}
		InternalSyntaxNode leadingTrivia = node.GetLeadingTrivia();
		InternalSyntaxNode trailingTrivia = node.GetTrailingTrivia();
		DebugAssertHelper.Assert(leadingTrivia == null || !leadingTrivia.IsList || leadingTrivia.SlotCount > 0);
		DebugAssertHelper.Assert(trailingTrivia == null || !trailingTrivia.IsList || trailingTrivia.SlotCount > 0);
		if (leadingTrivia != null)
		{
			SyntaxTriviaList trivia = VisitList(new SyntaxTriviaList(token, leadingTrivia));
			if (trailingTrivia != null)
			{
				int index = ((!leadingTrivia.IsList) ? 1 : leadingTrivia.SlotCount);
				SyntaxTriviaList trivia2 = VisitList(new SyntaxTriviaList(token, trailingTrivia, token.Position + node.FullWidth - trailingTrivia.FullWidth, index));
				if (trivia.Node != leadingTrivia)
				{
					token = token.WithLeadingTrivia(trivia);
				}
				if (trivia2.Node == trailingTrivia)
				{
					return token;
				}
				return token.WithTrailingTrivia(trivia2);
			}
			if (trivia.Node == leadingTrivia)
			{
				return token;
			}
			return token.WithLeadingTrivia(trivia);
		}
		if (trailingTrivia != null)
		{
			SyntaxTriviaList trivia3 = VisitList(new SyntaxTriviaList(token, trailingTrivia, token.Position + node.FullWidth - trailingTrivia.FullWidth));
			if (trivia3.Node == trailingTrivia)
			{
				return token;
			}
			return token.WithTrailingTrivia(trivia3);
		}
		return token;
	}

	public virtual SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
	{
		return trivia;
	}

	public virtual SyntaxList<TNode> VisitList<TNode>(SyntaxList<TNode> list) where TNode : SyntaxNode
	{
		SyntaxListBuilder syntaxListBuilder = null;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			TNode val = list[i];
			TNode val2 = VisitListElement(val);
			if (val != val2 && syntaxListBuilder == null)
			{
				syntaxListBuilder = new SyntaxListBuilder(count);
				syntaxListBuilder.AddRange(list, 0, i);
			}
			if (syntaxListBuilder != null && val2 != null && !val2.IsKind(SyntaxKind.None))
			{
				syntaxListBuilder.Add(val2);
			}
		}
		if (syntaxListBuilder != null)
		{
			return syntaxListBuilder.ToList();
		}
		return list;
	}

	public virtual TNode VisitListElement<TNode>(TNode node) where TNode : SyntaxNode
	{
		return (TNode)Visit(node);
	}

	public virtual SeparatedSyntaxList<TNode> VisitList<TNode>(SeparatedSyntaxList<TNode> list) where TNode : SyntaxNode
	{
		int count = list.Count;
		int separatorCount = list.SeparatorCount;
		SeparatedSyntaxListBuilder<TNode> separatedSyntaxListBuilder = default;
		int i;
		for (i = 0; i < separatorCount; i++)
		{
			TNode val = list[i];
			TNode val2 = VisitListElement(val);
			SyntaxToken separator = list.GetSeparator(i);
			SyntaxToken syntaxToken = VisitListSeparator(separator);
			if (separatedSyntaxListBuilder.IsNull && (val != val2 || separator != syntaxToken))
			{
				separatedSyntaxListBuilder = new SeparatedSyntaxListBuilder<TNode>(count);
				separatedSyntaxListBuilder.AddRange(list, i);
			}
			if (separatedSyntaxListBuilder.IsNull)
			{
				continue;
			}
			if (val2 != null)
			{
				separatedSyntaxListBuilder.Add(val2);
				if (syntaxToken.Kind == SyntaxKind.None)
				{
					throw new InvalidOperationException("CSharpResources.SeparatorIsExpected");
				}
				separatedSyntaxListBuilder.AddSeparator(syntaxToken);
			}
			else if (val2 == null)
			{
				throw new InvalidOperationException("CSharpResources.ElementIsExpected");
			}
		}
		if (i < count)
		{
			TNode val3 = list[i];
			TNode val4 = VisitListElement(val3);
			if (separatedSyntaxListBuilder.IsNull && val3 != val4)
			{
				separatedSyntaxListBuilder = new SeparatedSyntaxListBuilder<TNode>(count);
				separatedSyntaxListBuilder.AddRange(list, i);
			}
			if (!separatedSyntaxListBuilder.IsNull && val4 != null)
			{
				separatedSyntaxListBuilder.Add(val4);
			}
		}
		if (!separatedSyntaxListBuilder.IsNull)
		{
			return separatedSyntaxListBuilder.ToList();
		}
		return list;
	}

	public virtual SyntaxToken VisitListSeparator(SyntaxToken separator)
	{
		return VisitToken(separator);
	}

	public virtual SyntaxTokenList VisitList(SyntaxTokenList list)
	{
		SyntaxTokenListBuilder syntaxTokenListBuilder = null;
		int count = list.Count;
		int num = -1;
		foreach (SyntaxToken item in list)
		{
			num++;
			SyntaxToken syntaxToken = VisitToken(item);
			if (item != syntaxToken && syntaxTokenListBuilder == null)
			{
				syntaxTokenListBuilder = new SyntaxTokenListBuilder(count);
				syntaxTokenListBuilder.Add(list, 0, num);
			}
			if (syntaxTokenListBuilder != null && syntaxToken.Kind != SyntaxKind.None)
			{
				syntaxTokenListBuilder.Add(syntaxToken);
			}
		}
		return syntaxTokenListBuilder?.ToList() ?? list;
	}

	public virtual SyntaxTriviaList VisitList(SyntaxTriviaList list)
	{
		int count = list.Count;
		if (count != 0)
		{
			SyntaxTriviaListBuilder syntaxTriviaListBuilder = null;
			int num = -1;
			foreach (SyntaxTrivia item in list)
			{
				num++;
				SyntaxTrivia syntaxTrivia = VisitListElement(item);
				if (syntaxTrivia != item && syntaxTriviaListBuilder == null)
				{
					syntaxTriviaListBuilder = new SyntaxTriviaListBuilder(count);
					syntaxTriviaListBuilder.Add(list, 0, num);
				}
				if (syntaxTriviaListBuilder != null && syntaxTrivia.Kind != SyntaxKind.None)
				{
					syntaxTriviaListBuilder.Add(syntaxTrivia);
				}
			}
			if (syntaxTriviaListBuilder != null)
			{
				return syntaxTriviaListBuilder.ToList();
			}
		}
		return list;
	}

	public virtual SyntaxTrivia VisitListElement(SyntaxTrivia element)
	{
		return VisitTrivia(element);
	}
}
