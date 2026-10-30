using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
public static class SyntaxFactory
{
	public static readonly SyntaxTrivia CarriageReturnLinefeed = InternalSyntaxFactory.CarriageReturnLineFeed;

	public static readonly SyntaxTrivia Linefeed = InternalSyntaxFactory.LineFeed;

	public static readonly SyntaxTrivia CarriageReturn = InternalSyntaxFactory.CarriageReturn;

	public static readonly SyntaxTrivia Space = InternalSyntaxFactory.Space;

	public static readonly SyntaxTrivia ElasticCarriageReturnLinefeed = InternalSyntaxFactory.ElasticCarriageReturnLineFeed;

	public static readonly SyntaxTrivia ElasticLinefeed = InternalSyntaxFactory.ElasticLineFeed;

	public static readonly SyntaxTrivia ElasticCarriageReturn = InternalSyntaxFactory.ElasticCarriageReturn;

	public static readonly SyntaxTrivia ElasticSpace = InternalSyntaxFactory.ElasticSpace;

	public static readonly SyntaxTrivia ElasticMarker = InternalSyntaxFactory.ElasticZeroSpace;

	public static readonly SyntaxToken BeginKeyword = Token(SyntaxKind.BeginKeyword);

	public static readonly SyntaxToken EndKeyword = Token(SyntaxKind.EndKeyword);

	public static readonly SyntaxToken IfKeyword = Token(SyntaxKind.IfKeyword);

	public static readonly SyntaxToken ThenKeyword = Token(SyntaxKind.ThenKeyword);

	public static readonly SyntaxToken ElseKeyword = Token(SyntaxKind.ElseKeyword);

	public static readonly SyntaxToken WithKeyword = Token(SyntaxKind.WithKeyword);

	public static readonly SyntaxToken WhileKeyword = Token(SyntaxKind.WhileKeyword);

	public static readonly SyntaxToken DoKeyword = Token(SyntaxKind.DoKeyword);

	public static readonly SyntaxToken RepeatKeyword = Token(SyntaxKind.RepeatKeyword);

	public static readonly SyntaxToken UntilKeyword = Token(SyntaxKind.UntilKeyword);

	public static readonly SyntaxToken ForKeyword = Token(SyntaxKind.ForKeyword);

	public static readonly SyntaxToken AssertErrorKeyword = Token(SyntaxKind.AssertErrorKeyword);

	public static readonly SyntaxToken CaseKeyword = Token(SyntaxKind.CaseKeyword);

	public static readonly SyntaxToken OfKeyword = Token(SyntaxKind.OfKeyword);

	public static readonly SyntaxToken ForEachKeyword = Token(SyntaxKind.ForEachKeyword);

	public static readonly SyntaxToken InKeyword = Token(SyntaxKind.InKeyword);

	public static readonly SyntaxToken ExitKeyword = Token(SyntaxKind.ExitKeyword);

	public static readonly SyntaxToken TriggerKeyword = Token(SyntaxKind.TriggerKeyword);

	public static readonly SyntaxToken AddLastKeyword = Token(SyntaxKind.AddLastKeyword);

	public static readonly SyntaxToken AddFirstKeyword = Token(SyntaxKind.AddFirstKeyword);

	public static readonly SyntaxToken PageKeyword = Token(SyntaxKind.PageKeyword);

	public static readonly SyntaxToken TableKeyword = Token(SyntaxKind.TableKeyword);

	public static readonly SyntaxToken CodeunitKeyword = Token(SyntaxKind.CodeunitKeyword);

	public static readonly SyntaxToken DotNetKeyword = Token(SyntaxKind.DotNetKeyword);

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ObjectCompilationUnitSyntax ObjectCompilationUnit(SyntaxList<ObjectSyntax> objects, SyntaxToken endOfFileToken)
	{
		SyntaxKind kind = endOfFileToken.Kind;
		if (kind != SyntaxKind.EndOfFileToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "endOfFileToken");
		}
		return (ObjectCompilationUnitSyntax)InternalSyntaxFactory.ObjectCompilationUnit(objects.Node.ToInternalList<InternalObjectSyntax>(), (InternalSyntaxToken)endOfFileToken.Node).CreateRed();
	}

	public static ObjectCompilationUnitSyntax ObjectCompilationUnit(SyntaxList<ObjectSyntax> objects = default(SyntaxList<ObjectSyntax>))
	{
		return ObjectCompilationUnit(objects, Token(SyntaxKind.EndOfFileToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PropertyListSyntax PropertyList(SyntaxList<PropertySyntaxOrEmpty> properties)
	{
		return (PropertyListSyntax)InternalSyntaxFactory.PropertyList(properties.Node.ToInternalList<InternalPropertySyntaxOrEmpty>()).CreateRed();
	}

	public static PropertyListSyntax PropertyList()
	{
		return PropertyList(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ProfileSyntax Profile(SyntaxToken profileToken, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = profileToken.Kind;
		if (kind != SyntaxKind.ProfileKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "profileToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (ProfileSyntax)InternalSyntaxFactory.Profile((InternalSyntaxToken)profileToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static ProfileSyntax Profile(IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<MemberSyntax> members)
	{
		return Profile(Token(SyntaxKind.ProfileKeyword), name, Token(SyntaxKind.OpenBraceToken), propertyList, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static ProfileSyntax Profile(IdentifierNameSyntax name)
	{
		return Profile(Token(SyntaxKind.ProfileKeyword), name, Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static ProfileSyntax Profile(string name)
	{
		return Profile(Token(SyntaxKind.ProfileKeyword), IdentifierName(name), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static EnumTypeSyntax EnumType(SyntaxToken enumToken, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<EnumValueSyntax> values, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = enumToken.Kind;
		if (kind != SyntaxKind.EnumKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "enumToken");
		}
		if (objectId == null)
		{
			throw new ArgumentNullException("objectId");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (EnumTypeSyntax)InternalSyntaxFactory.EnumType((InternalSyntaxToken)enumToken.Node, (objectId == null) ? null : ((InternalObjectIdSyntax)objectId.Node), (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), values.Node.ToInternalList<InternalEnumValueSyntax>(), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static EnumTypeSyntax EnumType(ObjectIdSyntax objectId, IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<EnumValueSyntax> values, SyntaxList<MemberSyntax> members)
	{
		return EnumType(Token(SyntaxKind.EnumKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), propertyList, values, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static EnumTypeSyntax EnumType(ObjectIdSyntax objectId, IdentifierNameSyntax name)
	{
		return EnumType(Token(SyntaxKind.EnumKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), null, default, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static EnumTypeSyntax EnumType(ObjectIdSyntax objectId, string name)
	{
		return EnumType(Token(SyntaxKind.EnumKeyword), objectId, IdentifierName(name), Token(SyntaxKind.OpenBraceToken), null, default, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static EnumValueSyntax EnumValue(SyntaxToken enumValueToken, SyntaxToken openParenthesisToken, SyntaxToken id, SyntaxToken semicolonToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = enumValueToken.Kind;
		if (kind != SyntaxKind.EnumValueKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "enumValueToken");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = id.Kind;
		if (kind != SyntaxKind.Int32LiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "id");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (EnumValueSyntax)InternalSyntaxFactory.EnumValue((InternalSyntaxToken)enumValueToken.Node, (InternalSyntaxToken)openParenthesisToken.Node, (InternalSyntaxToken)id.Node, (InternalSyntaxToken)semicolonToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static EnumValueSyntax EnumValue(SyntaxToken id, IdentifierNameSyntax name, PropertyListSyntax propertyList)
	{
		return EnumValue(Token(SyntaxKind.EnumValueKeyword), Token(SyntaxKind.OpenParenToken), id, Token(SyntaxKind.SemicolonToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static EnumValueSyntax EnumValue(SyntaxToken id, IdentifierNameSyntax name)
	{
		return EnumValue(Token(SyntaxKind.EnumValueKeyword), Token(SyntaxKind.OpenParenToken), id, Token(SyntaxKind.SemicolonToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), null, Token(SyntaxKind.CloseBraceToken));
	}

	public static EnumValueSyntax EnumValue(SyntaxToken id, string name)
	{
		return EnumValue(Token(SyntaxKind.EnumValueKeyword), Token(SyntaxKind.OpenParenToken), id, Token(SyntaxKind.SemicolonToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), null, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static EnumExtensionTypeSyntax EnumExtensionType(SyntaxToken enumExtensionKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken extendsKeyword, ObjectNameOrIdSyntax baseObject, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<EnumValueSyntax> values, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = enumExtensionKeyword.Kind;
		if (kind != SyntaxKind.EnumExtensionKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "enumExtensionKeyword");
		}
		if (objectId == null)
		{
			throw new ArgumentNullException("objectId");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = extendsKeyword.Kind;
		if (kind != SyntaxKind.ExtendsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "extendsKeyword");
		}
		if (baseObject == null)
		{
			throw new ArgumentNullException("baseObject");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (EnumExtensionTypeSyntax)InternalSyntaxFactory.EnumExtensionType((InternalSyntaxToken)enumExtensionKeyword.Node, (objectId == null) ? null : ((InternalObjectIdSyntax)objectId.Node), (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)extendsKeyword.Node, (baseObject == null) ? null : ((InternalObjectNameOrIdSyntax)baseObject.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), values.Node.ToInternalList<InternalEnumValueSyntax>(), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static EnumExtensionTypeSyntax EnumExtensionType(ObjectIdSyntax objectId, IdentifierNameSyntax name, ObjectNameOrIdSyntax baseObject, PropertyListSyntax propertyList, SyntaxList<EnumValueSyntax> values, SyntaxList<MemberSyntax> members)
	{
		return EnumExtensionType(Token(SyntaxKind.EnumExtensionKeyword), objectId, name, Token(SyntaxKind.ExtendsKeyword), baseObject, Token(SyntaxKind.OpenBraceToken), propertyList, values, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static EnumExtensionTypeSyntax EnumExtensionType(ObjectIdSyntax objectId, IdentifierNameSyntax name, ObjectNameOrIdSyntax baseObject)
	{
		return EnumExtensionType(Token(SyntaxKind.EnumExtensionKeyword), objectId, name, Token(SyntaxKind.ExtendsKeyword), baseObject, Token(SyntaxKind.OpenBraceToken), null, default, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static EnumExtensionTypeSyntax EnumExtensionType(ObjectIdSyntax objectId, string name, ObjectNameOrIdSyntax baseObject)
	{
		return EnumExtensionType(Token(SyntaxKind.EnumExtensionKeyword), objectId, IdentifierName(name), Token(SyntaxKind.ExtendsKeyword), baseObject, Token(SyntaxKind.OpenBraceToken), null, default, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DotNetPackageSyntax DotNetPackage(SyntaxToken dotNetKeyword, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<DotNetAssemblySyntax> assemblies, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = dotNetKeyword.Kind;
		if (kind != SyntaxKind.DotNetKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "dotNetKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (DotNetPackageSyntax)InternalSyntaxFactory.DotNetPackage((InternalSyntaxToken)dotNetKeyword.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), assemblies.Node.ToInternalList<InternalDotNetAssemblySyntax>(), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static DotNetPackageSyntax DotNetPackage(IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<DotNetAssemblySyntax> assemblies, SyntaxList<MemberSyntax> members)
	{
		return DotNetPackage(Token(SyntaxKind.DotNetKeyword), name, Token(SyntaxKind.OpenBraceToken), propertyList, assemblies, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static DotNetPackageSyntax DotNetPackage()
	{
		return DotNetPackage(Token(SyntaxKind.DotNetKeyword), null, Token(SyntaxKind.OpenBraceToken), PropertyList(), default, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DotNetAssemblySyntax DotNetAssembly(SyntaxToken assemblyKeyword, SyntaxToken openParenthesisToken, NameSyntax assemblyName, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<DotNetTypeDeclarationSyntax> typeDeclarations, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = assemblyKeyword.Kind;
		if (kind != SyntaxKind.AssemblyKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "assemblyKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (assemblyName == null)
		{
			throw new ArgumentNullException("assemblyName");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (DotNetAssemblySyntax)InternalSyntaxFactory.DotNetAssembly((InternalSyntaxToken)assemblyKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (assemblyName == null) ? null : ((InternalNameSyntax)assemblyName.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), typeDeclarations.Node.ToInternalList<InternalDotNetTypeDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static DotNetAssemblySyntax DotNetAssembly(NameSyntax assemblyName, PropertyListSyntax propertyList, SyntaxList<DotNetTypeDeclarationSyntax> typeDeclarations)
	{
		return DotNetAssembly(Token(SyntaxKind.AssemblyKeyword), Token(SyntaxKind.OpenParenToken), assemblyName, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, typeDeclarations, Token(SyntaxKind.CloseBraceToken));
	}

	public static DotNetAssemblySyntax DotNetAssembly(NameSyntax assemblyName)
	{
		return DotNetAssembly(Token(SyntaxKind.AssemblyKeyword), Token(SyntaxKind.OpenParenToken), assemblyName, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DotNetTypeDeclarationSyntax DotNetTypeDeclaration(SyntaxToken type, SyntaxToken openParenthesisToken, NameSyntax typeName, SyntaxToken semicolonToken, IdentifierNameSyntax typeAlias, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = type.Kind;
		if (kind != SyntaxKind.TypeKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "type");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (DotNetTypeDeclarationSyntax)InternalSyntaxFactory.DotNetTypeDeclaration((InternalSyntaxToken)type.Node, (InternalSyntaxToken)openParenthesisToken.Node, (typeName == null) ? null : ((InternalNameSyntax)typeName.Node), (InternalSyntaxToken)semicolonToken.Node, (typeAlias == null) ? null : ((InternalIdentifierNameSyntax)typeAlias.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static DotNetTypeDeclarationSyntax DotNetTypeDeclaration(NameSyntax typeName, IdentifierNameSyntax typeAlias, PropertyListSyntax propertyList)
	{
		return DotNetTypeDeclaration(Token(SyntaxKind.TypeKeyword), Token(SyntaxKind.OpenParenToken), typeName, default, typeAlias, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static DotNetTypeDeclarationSyntax DotNetTypeDeclaration(NameSyntax typeName)
	{
		return DotNetTypeDeclaration(Token(SyntaxKind.TypeKeyword), Token(SyntaxKind.OpenParenToken), typeName, default, null, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static CodeunitSyntax Codeunit(SyntaxToken codeunitKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = codeunitKeyword.Kind;
		if (kind != SyntaxKind.CodeunitKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "codeunitKeyword");
		}
		if (objectId == null)
		{
			throw new ArgumentNullException("objectId");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (CodeunitSyntax)InternalSyntaxFactory.Codeunit((InternalSyntaxToken)codeunitKeyword.Node, (objectId == null) ? null : ((InternalObjectIdSyntax)objectId.Node), (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static CodeunitSyntax Codeunit(ObjectIdSyntax objectId, IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<MemberSyntax> members)
	{
		return Codeunit(Token(SyntaxKind.CodeunitKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), propertyList, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static CodeunitSyntax Codeunit(ObjectIdSyntax objectId, IdentifierNameSyntax name)
	{
		return Codeunit(Token(SyntaxKind.CodeunitKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static CodeunitSyntax Codeunit(ObjectIdSyntax objectId, string name)
	{
		return Codeunit(Token(SyntaxKind.CodeunitKeyword), objectId, IdentifierName(name), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static TableSyntax Table(SyntaxToken tableKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, FieldListSyntax fields, KeyListSyntax keys, FieldGroupListSyntax fieldGroups, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = tableKeyword.Kind;
		if (kind != SyntaxKind.TableKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "tableKeyword");
		}
		if (objectId == null)
		{
			throw new ArgumentNullException("objectId");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		if (fields == null)
		{
			throw new ArgumentNullException("fields");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (TableSyntax)InternalSyntaxFactory.Table((InternalSyntaxToken)tableKeyword.Node, (objectId == null) ? null : ((InternalObjectIdSyntax)objectId.Node), (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (fields == null) ? null : ((InternalFieldListSyntax)fields.Node), (keys == null) ? null : ((InternalKeyListSyntax)keys.Node), (fieldGroups == null) ? null : ((InternalFieldGroupListSyntax)fieldGroups.Node), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static TableSyntax Table(ObjectIdSyntax objectId, IdentifierNameSyntax name, PropertyListSyntax propertyList, FieldListSyntax fields, KeyListSyntax keys, FieldGroupListSyntax fieldGroups, SyntaxList<MemberSyntax> members)
	{
		return Table(Token(SyntaxKind.TableKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), propertyList, fields, keys, fieldGroups, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static TableSyntax Table(ObjectIdSyntax objectId, IdentifierNameSyntax name)
	{
		return Table(Token(SyntaxKind.TableKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), PropertyList(), FieldList(), null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static TableSyntax Table(ObjectIdSyntax objectId, string name)
	{
		return Table(Token(SyntaxKind.TableKeyword), objectId, IdentifierName(name), Token(SyntaxKind.OpenBraceToken), PropertyList(), FieldList(), null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FieldListSyntax FieldList(SyntaxToken fieldsKeyword, SyntaxToken openBraceToken, SyntaxList<FieldSyntax> fields, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = fieldsKeyword.Kind;
		if (kind != SyntaxKind.FieldsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "fieldsKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (FieldListSyntax)InternalSyntaxFactory.FieldList((InternalSyntaxToken)fieldsKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, fields.Node.ToInternalList<InternalFieldSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static FieldListSyntax FieldList(SyntaxList<FieldSyntax> fields = default(SyntaxList<FieldSyntax>))
	{
		return FieldList(Token(SyntaxKind.FieldsKeyword), Token(SyntaxKind.OpenBraceToken), fields, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FieldExtensionListSyntax FieldExtensionList(SyntaxToken fieldsKeyword, SyntaxToken openBraceToken, SyntaxList<FieldBaseSyntax> fields, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = fieldsKeyword.Kind;
		if (kind != SyntaxKind.FieldsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "fieldsKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (FieldExtensionListSyntax)InternalSyntaxFactory.FieldExtensionList((InternalSyntaxToken)fieldsKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, fields.Node.ToInternalList<InternalFieldBaseSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static FieldExtensionListSyntax FieldExtensionList(SyntaxList<FieldBaseSyntax> fields = default(SyntaxList<FieldBaseSyntax>))
	{
		return FieldExtensionList(Token(SyntaxKind.FieldsKeyword), Token(SyntaxKind.OpenBraceToken), fields, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FieldSyntax Field(SyntaxToken fieldKeyword, SyntaxToken openParenthesisToken, SyntaxToken no, SyntaxToken semicolonToken1, IdentifierNameSyntax name, SyntaxToken semicolonToken2, DataTypeSyntax type, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = fieldKeyword.Kind;
		if (kind != SyntaxKind.FieldKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "fieldKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = no.Kind;
		if (kind != SyntaxKind.Int32LiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "no");
		}
		kind = semicolonToken1.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken1");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken2.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken2");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (FieldSyntax)InternalSyntaxFactory.Field((InternalSyntaxToken)fieldKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (InternalSyntaxToken)no.Node, (InternalSyntaxToken)semicolonToken1.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken2.Node, (type == null) ? null : ((InternalDataTypeSyntax)type.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), triggers.Node.ToInternalList<InternalTriggerDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static FieldSyntax Field(SyntaxToken no, IdentifierNameSyntax name, DataTypeSyntax type, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return Field(Token(SyntaxKind.FieldKeyword), Token(SyntaxKind.OpenParenToken), no, Token(SyntaxKind.SemicolonToken), name, Token(SyntaxKind.SemicolonToken), type, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, triggers, Token(SyntaxKind.CloseBraceToken));
	}

	public static FieldSyntax Field(SyntaxToken no, IdentifierNameSyntax name, DataTypeSyntax type)
	{
		return Field(Token(SyntaxKind.FieldKeyword), Token(SyntaxKind.OpenParenToken), no, Token(SyntaxKind.SemicolonToken), name, Token(SyntaxKind.SemicolonToken), type, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static FieldSyntax Field(SyntaxToken no, string name, DataTypeSyntax type)
	{
		return Field(Token(SyntaxKind.FieldKeyword), Token(SyntaxKind.OpenParenToken), no, Token(SyntaxKind.SemicolonToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), type, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FieldModificationSyntax FieldModification(SyntaxToken fieldKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = fieldKeyword.Kind;
		if (kind != SyntaxKind.ModifyKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "fieldKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (FieldModificationSyntax)InternalSyntaxFactory.FieldModification((InternalSyntaxToken)fieldKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), triggers.Node.ToInternalList<InternalTriggerDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static FieldModificationSyntax FieldModification(IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return FieldModification(Token(SyntaxKind.ModifyKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, triggers, Token(SyntaxKind.CloseBraceToken));
	}

	public static FieldModificationSyntax FieldModification(IdentifierNameSyntax name)
	{
		return FieldModification(Token(SyntaxKind.ModifyKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static FieldModificationSyntax FieldModification(string name)
	{
		return FieldModification(Token(SyntaxKind.ModifyKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static KeyListSyntax KeyList(SyntaxToken keysKeyword, SyntaxToken openBraceToken, SyntaxList<KeySyntax> keys, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = keysKeyword.Kind;
		if (kind != SyntaxKind.KeysKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "keysKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (KeyListSyntax)InternalSyntaxFactory.KeyList((InternalSyntaxToken)keysKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, keys.Node.ToInternalList<InternalKeySyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static KeyListSyntax KeyList(SyntaxList<KeySyntax> keys = default(SyntaxList<KeySyntax>))
	{
		return KeyList(Token(SyntaxKind.KeysKeyword), Token(SyntaxKind.OpenBraceToken), keys, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static KeySyntax Key(SyntaxToken keyKeyword, SyntaxToken openParenthesisToken, SyntaxToken id, SyntaxToken semicolonToken1, IdentifierNameSyntax name, SyntaxToken semicolonToken2, SeparatedSyntaxList<IdentifierNameSyntax> fields, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = keyKeyword.Kind;
		if (kind != SyntaxKind.KeyKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "keyKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = id.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.Int32LiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "id");
		}
		kind = semicolonToken1.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken1");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken2.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken2");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (KeySyntax)InternalSyntaxFactory.Key((InternalSyntaxToken)keyKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (InternalSyntaxToken)id.Node, (InternalSyntaxToken)semicolonToken1.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken2.Node, fields.Node.ToInternalSeparatedList<InternalIdentifierNameSyntax>(), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static KeySyntax Key(SyntaxToken id, IdentifierNameSyntax name, SeparatedSyntaxList<IdentifierNameSyntax> fields, PropertyListSyntax propertyList)
	{
		return Key(Token(SyntaxKind.KeyKeyword), Token(SyntaxKind.OpenParenToken), id, default, name, Token(SyntaxKind.SemicolonToken), fields, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static KeySyntax Key(IdentifierNameSyntax name)
	{
		return Key(Token(SyntaxKind.KeyKeyword), Token(SyntaxKind.OpenParenToken), default, default, name, Token(SyntaxKind.SemicolonToken), default, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	public static KeySyntax Key(string name)
	{
		return Key(Token(SyntaxKind.KeyKeyword), Token(SyntaxKind.OpenParenToken), default, default, IdentifierName(name), Token(SyntaxKind.SemicolonToken), default, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FieldGroupListSyntax FieldGroupList(SyntaxToken fieldGroupsKeyword, SyntaxToken openBraceToken, SyntaxList<FieldGroupSyntax> fieldGroups, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = fieldGroupsKeyword.Kind;
		if (kind != SyntaxKind.FieldGroupsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "fieldGroupsKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (FieldGroupListSyntax)InternalSyntaxFactory.FieldGroupList((InternalSyntaxToken)fieldGroupsKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, fieldGroups.Node.ToInternalList<InternalFieldGroupSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static FieldGroupListSyntax FieldGroupList(SyntaxList<FieldGroupSyntax> fieldGroups = default(SyntaxList<FieldGroupSyntax>))
	{
		return FieldGroupList(Token(SyntaxKind.FieldGroupsKeyword), Token(SyntaxKind.OpenBraceToken), fieldGroups, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FieldGroupExtensionListSyntax FieldGroupExtensionList(SyntaxToken fieldGroupsKeyword, SyntaxToken openBraceToken, SyntaxList<FieldGroupChangeBaseSyntax> changes, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = fieldGroupsKeyword.Kind;
		if (kind != SyntaxKind.FieldGroupsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "fieldGroupsKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (FieldGroupExtensionListSyntax)InternalSyntaxFactory.FieldGroupExtensionList((InternalSyntaxToken)fieldGroupsKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, changes.Node.ToInternalList<InternalFieldGroupChangeBaseSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static FieldGroupExtensionListSyntax FieldGroupExtensionList(SyntaxList<FieldGroupChangeBaseSyntax> changes = default(SyntaxList<FieldGroupChangeBaseSyntax>))
	{
		return FieldGroupExtensionList(Token(SyntaxKind.FieldGroupsKeyword), Token(SyntaxKind.OpenBraceToken), changes, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FieldGroupAddChangeSyntax FieldGroupAddChange(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken semicolonToken1, SeparatedSyntaxList<IdentifierNameSyntax> fields, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = changeKeyword.Kind;
		if (kind != SyntaxKind.AddLastKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "changeKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (anchor == null)
		{
			throw new ArgumentNullException("anchor");
		}
		kind = semicolonToken1.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken1");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (FieldGroupAddChangeSyntax)InternalSyntaxFactory.FieldGroupAddChange((InternalSyntaxToken)changeKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (anchor == null) ? null : ((InternalIdentifierNameSyntax)anchor.Node), (InternalSyntaxToken)semicolonToken1.Node, fields.Node.ToInternalSeparatedList<InternalIdentifierNameSyntax>(), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static FieldGroupAddChangeSyntax FieldGroupAddChange(IdentifierNameSyntax anchor, SeparatedSyntaxList<IdentifierNameSyntax> fields, PropertyListSyntax propertyList)
	{
		return FieldGroupAddChange(Token(SyntaxKind.AddLastKeyword), Token(SyntaxKind.OpenParenToken), anchor, Token(SyntaxKind.SemicolonToken), fields, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static FieldGroupAddChangeSyntax FieldGroupAddChange(IdentifierNameSyntax anchor)
	{
		return FieldGroupAddChange(Token(SyntaxKind.AddLastKeyword), Token(SyntaxKind.OpenParenToken), anchor, Token(SyntaxKind.SemicolonToken), default, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	public static FieldGroupAddChangeSyntax FieldGroupAddChange(string anchor)
	{
		return FieldGroupAddChange(Token(SyntaxKind.AddLastKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(anchor), Token(SyntaxKind.SemicolonToken), default, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FieldGroupSyntax FieldGroup(SyntaxToken fieldGroupKeyword, SyntaxToken openParenthesisToken, SyntaxToken id, SyntaxToken semicolonToken1, IdentifierNameSyntax name, SyntaxToken semicolonToken2, SeparatedSyntaxList<IdentifierNameSyntax> fields, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = fieldGroupKeyword.Kind;
		if (kind != SyntaxKind.FieldGroupKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "fieldGroupKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = id.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.Int32LiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "id");
		}
		kind = semicolonToken1.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken1");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken2.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken2");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (FieldGroupSyntax)InternalSyntaxFactory.FieldGroup((InternalSyntaxToken)fieldGroupKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (InternalSyntaxToken)id.Node, (InternalSyntaxToken)semicolonToken1.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken2.Node, fields.Node.ToInternalSeparatedList<InternalIdentifierNameSyntax>(), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static FieldGroupSyntax FieldGroup(SyntaxToken id, IdentifierNameSyntax name, SeparatedSyntaxList<IdentifierNameSyntax> fields, PropertyListSyntax propertyList)
	{
		return FieldGroup(Token(SyntaxKind.FieldGroupKeyword), Token(SyntaxKind.OpenParenToken), id, default, name, Token(SyntaxKind.SemicolonToken), fields, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static FieldGroupSyntax FieldGroup(IdentifierNameSyntax name)
	{
		return FieldGroup(Token(SyntaxKind.FieldGroupKeyword), Token(SyntaxKind.OpenParenToken), default, default, name, Token(SyntaxKind.SemicolonToken), default, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	public static FieldGroupSyntax FieldGroup(string name)
	{
		return FieldGroup(Token(SyntaxKind.FieldGroupKeyword), Token(SyntaxKind.OpenParenToken), default, default, IdentifierName(name), Token(SyntaxKind.SemicolonToken), default, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageSyntax Page(SyntaxToken pageKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, PageLayoutSyntax layout, PageActionListSyntax actions, PageViewListSyntax views, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = pageKeyword.Kind;
		if (kind != SyntaxKind.PageKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "pageKeyword");
		}
		if (objectId == null)
		{
			throw new ArgumentNullException("objectId");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageSyntax)InternalSyntaxFactory.Page((InternalSyntaxToken)pageKeyword.Node, (objectId == null) ? null : ((InternalObjectIdSyntax)objectId.Node), (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (layout == null) ? null : ((InternalPageLayoutSyntax)layout.Node), (actions == null) ? null : ((InternalPageActionListSyntax)actions.Node), (views == null) ? null : ((InternalPageViewListSyntax)views.Node), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageSyntax Page(ObjectIdSyntax objectId, IdentifierNameSyntax name, PropertyListSyntax propertyList, PageLayoutSyntax layout, PageActionListSyntax actions, PageViewListSyntax views, SyntaxList<MemberSyntax> members)
	{
		return Page(Token(SyntaxKind.PageKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), propertyList, layout, actions, views, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageSyntax Page(ObjectIdSyntax objectId, IdentifierNameSyntax name)
	{
		return Page(Token(SyntaxKind.PageKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), PropertyList(), null, null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageSyntax Page(ObjectIdSyntax objectId, string name)
	{
		return Page(Token(SyntaxKind.PageKeyword), objectId, IdentifierName(name), Token(SyntaxKind.OpenBraceToken), PropertyList(), null, null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageLayoutSyntax PageLayout(SyntaxToken layoutKeyword, SyntaxToken openBraceToken, SyntaxList<PageAreaSyntax> areas, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = layoutKeyword.Kind;
		if (kind != SyntaxKind.PageLayoutKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "layoutKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageLayoutSyntax)InternalSyntaxFactory.PageLayout((InternalSyntaxToken)layoutKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, areas.Node.ToInternalList<InternalPageAreaSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageLayoutSyntax PageLayout(SyntaxList<PageAreaSyntax> areas = default(SyntaxList<PageAreaSyntax>))
	{
		return PageLayout(Token(SyntaxKind.PageLayoutKeyword), Token(SyntaxKind.OpenBraceToken), areas, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageFieldSyntax PageField(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, CodeExpressionSyntax expression, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = controlKeyword.Kind;
		if (kind != SyntaxKind.FieldKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "controlKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageFieldSyntax)InternalSyntaxFactory.PageField((InternalSyntaxToken)controlKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), triggers.Node.ToInternalList<InternalTriggerDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageFieldSyntax PageField(IdentifierNameSyntax name, CodeExpressionSyntax expression, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return PageField(Token(SyntaxKind.FieldKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), expression, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, triggers, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageFieldSyntax PageField(IdentifierNameSyntax name, CodeExpressionSyntax expression)
	{
		return PageField(Token(SyntaxKind.FieldKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), expression, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageFieldSyntax PageField(string name, CodeExpressionSyntax expression)
	{
		return PageField(Token(SyntaxKind.FieldKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), expression, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageLabelSyntax PageLabel(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = controlKeyword.Kind;
		if (kind != SyntaxKind.LabelKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "controlKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageLabelSyntax)InternalSyntaxFactory.PageLabel((InternalSyntaxToken)controlKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageLabelSyntax PageLabel(IdentifierNameSyntax name, PropertyListSyntax propertyList)
	{
		return PageLabel(Token(SyntaxKind.LabelKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageLabelSyntax PageLabel(IdentifierNameSyntax name)
	{
		return PageLabel(Token(SyntaxKind.LabelKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	public static PageLabelSyntax PageLabel(string name)
	{
		return PageLabel(Token(SyntaxKind.LabelKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageGroupSyntax PageGroup(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<ControlBaseSyntax> controls, GroupActionListSyntax actionList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = controlKeyword.Kind;
		if ((uint)(kind - 101) > 4u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "controlKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageGroupSyntax)InternalSyntaxFactory.PageGroup((InternalSyntaxToken)controlKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), controls.Node.ToInternalList<InternalControlBaseSyntax>(), (actionList == null) ? null : ((InternalGroupActionListSyntax)actionList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageGroupSyntax PageGroup(SyntaxToken controlKeyword, IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<ControlBaseSyntax> controls, GroupActionListSyntax actionList)
	{
		return PageGroup(controlKeyword, Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, controls, actionList, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageGroupSyntax PageGroup(SyntaxToken controlKeyword, IdentifierNameSyntax name)
	{
		return PageGroup(controlKeyword, Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, null, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageGroupSyntax PageGroup(SyntaxToken controlKeyword, string name)
	{
		return PageGroup(controlKeyword, Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, null, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PagePartSyntax PagePart(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, ObjectNameOrIdSyntax partName, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = controlKeyword.Kind;
		if (kind != SyntaxKind.PagePartKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "controlKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (partName == null)
		{
			throw new ArgumentNullException("partName");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PagePartSyntax)InternalSyntaxFactory.PagePart((InternalSyntaxToken)controlKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (partName == null) ? null : ((InternalObjectNameOrIdSyntax)partName.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PagePartSyntax PagePart(IdentifierNameSyntax name, ObjectNameOrIdSyntax partName, PropertyListSyntax propertyList)
	{
		return PagePart(Token(SyntaxKind.PagePartKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), partName, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static PagePartSyntax PagePart(IdentifierNameSyntax name, ObjectNameOrIdSyntax partName)
	{
		return PagePart(Token(SyntaxKind.PagePartKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), partName, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	public static PagePartSyntax PagePart(string name, ObjectNameOrIdSyntax partName)
	{
		return PagePart(Token(SyntaxKind.PagePartKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), partName, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageSystemPartSyntax PageSystemPart(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, IdentifierNameSyntax systemPartType, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = controlKeyword.Kind;
		if (kind != SyntaxKind.PageSystemPartKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "controlKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (systemPartType == null)
		{
			throw new ArgumentNullException("systemPartType");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageSystemPartSyntax)InternalSyntaxFactory.PageSystemPart((InternalSyntaxToken)controlKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (systemPartType == null) ? null : ((InternalIdentifierNameSyntax)systemPartType.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageSystemPartSyntax PageSystemPart(IdentifierNameSyntax name, IdentifierNameSyntax systemPartType, PropertyListSyntax propertyList)
	{
		return PageSystemPart(Token(SyntaxKind.PageSystemPartKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), systemPartType, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageSystemPartSyntax PageSystemPart(IdentifierNameSyntax name, IdentifierNameSyntax systemPartType)
	{
		return PageSystemPart(Token(SyntaxKind.PageSystemPartKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), systemPartType, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	public static PageSystemPartSyntax PageSystemPart(string name, string systemPartType)
	{
		return PageSystemPart(Token(SyntaxKind.PageSystemPartKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), IdentifierName(systemPartType), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageChartPartSyntax PageChartPart(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, IdentifierNameSyntax chartPartType, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = controlKeyword.Kind;
		if (kind != SyntaxKind.PageChartPartKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "controlKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (chartPartType == null)
		{
			throw new ArgumentNullException("chartPartType");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageChartPartSyntax)InternalSyntaxFactory.PageChartPart((InternalSyntaxToken)controlKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (chartPartType == null) ? null : ((InternalIdentifierNameSyntax)chartPartType.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageChartPartSyntax PageChartPart(IdentifierNameSyntax name, IdentifierNameSyntax chartPartType, PropertyListSyntax propertyList)
	{
		return PageChartPart(Token(SyntaxKind.PageChartPartKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), chartPartType, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageChartPartSyntax PageChartPart(IdentifierNameSyntax name, IdentifierNameSyntax chartPartType)
	{
		return PageChartPart(Token(SyntaxKind.PageChartPartKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), chartPartType, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	public static PageChartPartSyntax PageChartPart(string name, string chartPartType)
	{
		return PageChartPart(Token(SyntaxKind.PageChartPartKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), IdentifierName(chartPartType), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageUserControlSyntax PageUserControl(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, ObjectNameReferenceSyntax controlAddIn, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = controlKeyword.Kind;
		if (kind != SyntaxKind.PageUserControlKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "controlKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (controlAddIn == null)
		{
			throw new ArgumentNullException("controlAddIn");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageUserControlSyntax)InternalSyntaxFactory.PageUserControl((InternalSyntaxToken)controlKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (controlAddIn == null) ? null : ((InternalObjectNameReferenceSyntax)controlAddIn.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), triggers.Node.ToInternalList<InternalTriggerDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageUserControlSyntax PageUserControl(IdentifierNameSyntax name, ObjectNameReferenceSyntax controlAddIn, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return PageUserControl(Token(SyntaxKind.PageUserControlKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), controlAddIn, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, triggers, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageUserControlSyntax PageUserControl(IdentifierNameSyntax name, ObjectNameReferenceSyntax controlAddIn)
	{
		return PageUserControl(Token(SyntaxKind.PageUserControlKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), controlAddIn, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageUserControlSyntax PageUserControl(string name, ObjectNameReferenceSyntax controlAddIn)
	{
		return PageUserControl(Token(SyntaxKind.PageUserControlKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), controlAddIn, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageAreaSyntax PageArea(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<ControlBaseSyntax> controls, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = controlKeyword.Kind;
		if (kind != SyntaxKind.PageAreaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "controlKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageAreaSyntax)InternalSyntaxFactory.PageArea((InternalSyntaxToken)controlKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), controls.Node.ToInternalList<InternalControlBaseSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageAreaSyntax PageArea(IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<ControlBaseSyntax> controls)
	{
		return PageArea(Token(SyntaxKind.PageAreaKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, controls, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageAreaSyntax PageArea(IdentifierNameSyntax name)
	{
		return PageArea(Token(SyntaxKind.PageAreaKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageAreaSyntax PageArea(string name)
	{
		return PageArea(Token(SyntaxKind.PageAreaKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageActionListSyntax PageActionList(SyntaxToken actionsKeyword, SyntaxToken openBraceToken, SyntaxList<PageActionAreaSyntax> areas, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = actionsKeyword.Kind;
		if (kind != SyntaxKind.ActionsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "actionsKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageActionListSyntax)InternalSyntaxFactory.PageActionList((InternalSyntaxToken)actionsKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, areas.Node.ToInternalList<InternalPageActionAreaSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageActionListSyntax PageActionList(SyntaxList<PageActionAreaSyntax> areas = default(SyntaxList<PageActionAreaSyntax>))
	{
		return PageActionList(Token(SyntaxKind.ActionsKeyword), Token(SyntaxKind.OpenBraceToken), areas, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageViewListSyntax PageViewList(SyntaxToken viewsKeyword, SyntaxToken openBraceToken, SyntaxList<PageViewSyntax> views, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = viewsKeyword.Kind;
		if (kind != SyntaxKind.ViewsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "viewsKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageViewListSyntax)InternalSyntaxFactory.PageViewList((InternalSyntaxToken)viewsKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, views.Node.ToInternalList<InternalPageViewSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageViewListSyntax PageViewList(SyntaxList<PageViewSyntax> views = default(SyntaxList<PageViewSyntax>))
	{
		return PageViewList(Token(SyntaxKind.ViewsKeyword), Token(SyntaxKind.OpenBraceToken), views, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageViewSyntax PageView(SyntaxToken viewKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, PageExtensionLayoutSyntax layout, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = viewKeyword.Kind;
		if (kind != SyntaxKind.ViewKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "viewKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageViewSyntax)InternalSyntaxFactory.PageView((InternalSyntaxToken)viewKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (layout == null) ? null : ((InternalPageExtensionLayoutSyntax)layout.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageViewSyntax PageView(IdentifierNameSyntax name, PropertyListSyntax propertyList, PageExtensionLayoutSyntax layout)
	{
		return PageView(Token(SyntaxKind.ViewKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, layout, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageViewSyntax PageView(IdentifierNameSyntax name)
	{
		return PageView(Token(SyntaxKind.ViewKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), null, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageViewSyntax PageView(string name)
	{
		return PageView(Token(SyntaxKind.ViewKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), null, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static GroupActionListSyntax GroupActionList(SyntaxToken actionsKeyword, SyntaxToken openBraceToken, SyntaxList<ActionBaseSyntax> actions, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = actionsKeyword.Kind;
		if (kind != SyntaxKind.ActionsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "actionsKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (GroupActionListSyntax)InternalSyntaxFactory.GroupActionList((InternalSyntaxToken)actionsKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, actions.Node.ToInternalList<InternalActionBaseSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static GroupActionListSyntax GroupActionList(SyntaxList<ActionBaseSyntax> actions = default(SyntaxList<ActionBaseSyntax>))
	{
		return GroupActionList(Token(SyntaxKind.ActionsKeyword), Token(SyntaxKind.OpenBraceToken), actions, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageActionGroupSyntax PageActionGroup(SyntaxToken actionKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<ActionBaseSyntax> actions, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = actionKeyword.Kind;
		if (kind != SyntaxKind.PageGroupKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "actionKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageActionGroupSyntax)InternalSyntaxFactory.PageActionGroup((InternalSyntaxToken)actionKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), actions.Node.ToInternalList<InternalActionBaseSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageActionGroupSyntax PageActionGroup(IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<ActionBaseSyntax> actions)
	{
		return PageActionGroup(Token(SyntaxKind.PageGroupKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, actions, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageActionGroupSyntax PageActionGroup(IdentifierNameSyntax name)
	{
		return PageActionGroup(Token(SyntaxKind.PageGroupKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageActionGroupSyntax PageActionGroup(string name)
	{
		return PageActionGroup(Token(SyntaxKind.PageGroupKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageActionAreaSyntax PageActionArea(SyntaxToken actionKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<ActionBaseSyntax> actions, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = actionKeyword.Kind;
		if (kind != SyntaxKind.PageAreaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "actionKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageActionAreaSyntax)InternalSyntaxFactory.PageActionArea((InternalSyntaxToken)actionKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), actions.Node.ToInternalList<InternalActionBaseSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageActionAreaSyntax PageActionArea(IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<ActionBaseSyntax> actions)
	{
		return PageActionArea(Token(SyntaxKind.PageAreaKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, actions, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageActionAreaSyntax PageActionArea(IdentifierNameSyntax name)
	{
		return PageActionArea(Token(SyntaxKind.PageAreaKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageActionAreaSyntax PageActionArea(string name)
	{
		return PageActionArea(Token(SyntaxKind.PageAreaKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageActionSyntax PageAction(SyntaxToken actionKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = actionKeyword.Kind;
		if (kind != SyntaxKind.ActionKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "actionKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageActionSyntax)InternalSyntaxFactory.PageAction((InternalSyntaxToken)actionKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), triggers.Node.ToInternalList<InternalTriggerDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageActionSyntax PageAction(IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return PageAction(Token(SyntaxKind.ActionKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, triggers, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageActionSyntax PageAction(IdentifierNameSyntax name)
	{
		return PageAction(Token(SyntaxKind.ActionKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageActionSyntax PageAction(string name)
	{
		return PageAction(Token(SyntaxKind.ActionKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageActionSeparatorSyntax PageActionSeparator(SyntaxToken actionKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = actionKeyword.Kind;
		if (kind != SyntaxKind.SeparatorKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "actionKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageActionSeparatorSyntax)InternalSyntaxFactory.PageActionSeparator((InternalSyntaxToken)actionKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageActionSeparatorSyntax PageActionSeparator(IdentifierNameSyntax name, PropertyListSyntax propertyList)
	{
		return PageActionSeparator(Token(SyntaxKind.SeparatorKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageActionSeparatorSyntax PageActionSeparator(IdentifierNameSyntax name)
	{
		return PageActionSeparator(Token(SyntaxKind.SeparatorKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	public static PageActionSeparatorSyntax PageActionSeparator(string name)
	{
		return PageActionSeparator(Token(SyntaxKind.SeparatorKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageExtensionSyntax PageExtension(SyntaxToken pageExtensionKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken extendsKeyword, ObjectNameOrIdSyntax baseObject, SyntaxToken openBraceToken, PropertyListSyntax propertyList, PageExtensionLayoutSyntax layout, PageExtensionActionListSyntax actions, PageExtensionViewListSyntax views, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = pageExtensionKeyword.Kind;
		if (kind != SyntaxKind.PageExtensionKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "pageExtensionKeyword");
		}
		if (objectId == null)
		{
			throw new ArgumentNullException("objectId");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = extendsKeyword.Kind;
		if (kind != SyntaxKind.ExtendsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "extendsKeyword");
		}
		if (baseObject == null)
		{
			throw new ArgumentNullException("baseObject");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageExtensionSyntax)InternalSyntaxFactory.PageExtension((InternalSyntaxToken)pageExtensionKeyword.Node, (objectId == null) ? null : ((InternalObjectIdSyntax)objectId.Node), (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)extendsKeyword.Node, (baseObject == null) ? null : ((InternalObjectNameOrIdSyntax)baseObject.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (layout == null) ? null : ((InternalPageExtensionLayoutSyntax)layout.Node), (actions == null) ? null : ((InternalPageExtensionActionListSyntax)actions.Node), (views == null) ? null : ((InternalPageExtensionViewListSyntax)views.Node), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageExtensionSyntax PageExtension(ObjectIdSyntax objectId, IdentifierNameSyntax name, ObjectNameOrIdSyntax baseObject, PropertyListSyntax propertyList, PageExtensionLayoutSyntax layout, PageExtensionActionListSyntax actions, PageExtensionViewListSyntax views, SyntaxList<MemberSyntax> members)
	{
		return PageExtension(Token(SyntaxKind.PageExtensionKeyword), objectId, name, Token(SyntaxKind.ExtendsKeyword), baseObject, Token(SyntaxKind.OpenBraceToken), propertyList, layout, actions, views, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageExtensionSyntax PageExtension(ObjectIdSyntax objectId, IdentifierNameSyntax name, ObjectNameOrIdSyntax baseObject)
	{
		return PageExtension(Token(SyntaxKind.PageExtensionKeyword), objectId, name, Token(SyntaxKind.ExtendsKeyword), baseObject, Token(SyntaxKind.OpenBraceToken), PropertyList(), null, null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageExtensionSyntax PageExtension(ObjectIdSyntax objectId, string name, ObjectNameOrIdSyntax baseObject)
	{
		return PageExtension(Token(SyntaxKind.PageExtensionKeyword), objectId, IdentifierName(name), Token(SyntaxKind.ExtendsKeyword), baseObject, Token(SyntaxKind.OpenBraceToken), PropertyList(), null, null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageCustomizationSyntax PageCustomization(SyntaxToken pageCustomizationKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken customizesKeyword, ObjectNameOrIdSyntax baseObject, SyntaxToken openBraceToken, PropertyListSyntax propertyList, PageExtensionLayoutSyntax layout, PageExtensionActionListSyntax actions, PageExtensionViewListSyntax views, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = pageCustomizationKeyword.Kind;
		if (kind != SyntaxKind.PageCustomizationKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "pageCustomizationKeyword");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = customizesKeyword.Kind;
		if (kind != SyntaxKind.CustomizesKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "customizesKeyword");
		}
		if (baseObject == null)
		{
			throw new ArgumentNullException("baseObject");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageCustomizationSyntax)InternalSyntaxFactory.PageCustomization((InternalSyntaxToken)pageCustomizationKeyword.Node, (objectId == null) ? null : ((InternalObjectIdSyntax)objectId.Node), (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)customizesKeyword.Node, (baseObject == null) ? null : ((InternalObjectNameOrIdSyntax)baseObject.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (layout == null) ? null : ((InternalPageExtensionLayoutSyntax)layout.Node), (actions == null) ? null : ((InternalPageExtensionActionListSyntax)actions.Node), (views == null) ? null : ((InternalPageExtensionViewListSyntax)views.Node), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageCustomizationSyntax PageCustomization(ObjectIdSyntax objectId, IdentifierNameSyntax name, ObjectNameOrIdSyntax baseObject, PropertyListSyntax propertyList, PageExtensionLayoutSyntax layout, PageExtensionActionListSyntax actions, PageExtensionViewListSyntax views, SyntaxList<MemberSyntax> members)
	{
		return PageCustomization(Token(SyntaxKind.PageCustomizationKeyword), objectId, name, Token(SyntaxKind.CustomizesKeyword), baseObject, Token(SyntaxKind.OpenBraceToken), propertyList, layout, actions, views, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageCustomizationSyntax PageCustomization(IdentifierNameSyntax name, ObjectNameOrIdSyntax baseObject)
	{
		return PageCustomization(Token(SyntaxKind.PageCustomizationKeyword), null, name, Token(SyntaxKind.CustomizesKeyword), baseObject, Token(SyntaxKind.OpenBraceToken), null, null, null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static PageCustomizationSyntax PageCustomization(string name, ObjectNameOrIdSyntax baseObject)
	{
		return PageCustomization(Token(SyntaxKind.PageCustomizationKeyword), null, IdentifierName(name), Token(SyntaxKind.CustomizesKeyword), baseObject, Token(SyntaxKind.OpenBraceToken), null, null, null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static TableExtensionSyntax TableExtension(SyntaxToken tableExtensionKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken extendsKeyword, ObjectNameOrIdSyntax baseObject, SyntaxToken openBraceToken, PropertyListSyntax propertyList, FieldExtensionListSyntax fields, KeyListSyntax keys, FieldGroupExtensionListSyntax fieldGroups, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = tableExtensionKeyword.Kind;
		if (kind != SyntaxKind.TableExtensionKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "tableExtensionKeyword");
		}
		if (objectId == null)
		{
			throw new ArgumentNullException("objectId");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = extendsKeyword.Kind;
		if (kind != SyntaxKind.ExtendsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "extendsKeyword");
		}
		if (baseObject == null)
		{
			throw new ArgumentNullException("baseObject");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (TableExtensionSyntax)InternalSyntaxFactory.TableExtension((InternalSyntaxToken)tableExtensionKeyword.Node, (objectId == null) ? null : ((InternalObjectIdSyntax)objectId.Node), (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)extendsKeyword.Node, (baseObject == null) ? null : ((InternalObjectNameOrIdSyntax)baseObject.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (fields == null) ? null : ((InternalFieldExtensionListSyntax)fields.Node), (keys == null) ? null : ((InternalKeyListSyntax)keys.Node), (fieldGroups == null) ? null : ((InternalFieldGroupExtensionListSyntax)fieldGroups.Node), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static TableExtensionSyntax TableExtension(ObjectIdSyntax objectId, IdentifierNameSyntax name, ObjectNameOrIdSyntax baseObject, PropertyListSyntax propertyList, FieldExtensionListSyntax fields, KeyListSyntax keys, FieldGroupExtensionListSyntax fieldGroups, SyntaxList<MemberSyntax> members)
	{
		return TableExtension(Token(SyntaxKind.TableExtensionKeyword), objectId, name, Token(SyntaxKind.ExtendsKeyword), baseObject, Token(SyntaxKind.OpenBraceToken), propertyList, fields, keys, fieldGroups, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static TableExtensionSyntax TableExtension(ObjectIdSyntax objectId, IdentifierNameSyntax name, ObjectNameOrIdSyntax baseObject)
	{
		return TableExtension(Token(SyntaxKind.TableExtensionKeyword), objectId, name, Token(SyntaxKind.ExtendsKeyword), baseObject, Token(SyntaxKind.OpenBraceToken), PropertyList(), null, null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static TableExtensionSyntax TableExtension(ObjectIdSyntax objectId, string name, ObjectNameOrIdSyntax baseObject)
	{
		return TableExtension(Token(SyntaxKind.TableExtensionKeyword), objectId, IdentifierName(name), Token(SyntaxKind.ExtendsKeyword), baseObject, Token(SyntaxKind.OpenBraceToken), PropertyList(), null, null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageExtensionLayoutSyntax PageExtensionLayout(SyntaxToken layoutKeyword, SyntaxToken openBraceToken, SyntaxList<ControlChangeBaseSyntax> changes, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = layoutKeyword.Kind;
		if (kind != SyntaxKind.PageLayoutKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "layoutKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageExtensionLayoutSyntax)InternalSyntaxFactory.PageExtensionLayout((InternalSyntaxToken)layoutKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, changes.Node.ToInternalList<InternalControlChangeBaseSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageExtensionLayoutSyntax PageExtensionLayout(SyntaxList<ControlChangeBaseSyntax> changes = default(SyntaxList<ControlChangeBaseSyntax>))
	{
		return PageExtensionLayout(Token(SyntaxKind.PageLayoutKeyword), Token(SyntaxKind.OpenBraceToken), changes, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ControlAddChangeSyntax ControlAddChange(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, SyntaxList<ControlBaseSyntax> controls, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = changeKeyword.Kind;
		if ((uint)(kind - 116) > 3u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "changeKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (anchor == null)
		{
			throw new ArgumentNullException("anchor");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (ControlAddChangeSyntax)InternalSyntaxFactory.ControlAddChange((InternalSyntaxToken)changeKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (anchor == null) ? null : ((InternalIdentifierNameSyntax)anchor.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, controls.Node.ToInternalList<InternalControlBaseSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static ControlAddChangeSyntax ControlAddChange(SyntaxToken changeKeyword, IdentifierNameSyntax anchor, SyntaxList<ControlBaseSyntax> controls)
	{
		return ControlAddChange(changeKeyword, Token(SyntaxKind.OpenParenToken), anchor, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), controls, Token(SyntaxKind.CloseBraceToken));
	}

	public static ControlAddChangeSyntax ControlAddChange(SyntaxToken changeKeyword, IdentifierNameSyntax anchor)
	{
		return ControlAddChange(changeKeyword, Token(SyntaxKind.OpenParenToken), anchor, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static ControlAddChangeSyntax ControlAddChange(SyntaxToken changeKeyword, string anchor)
	{
		return ControlAddChange(changeKeyword, Token(SyntaxKind.OpenParenToken), IdentifierName(anchor), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ControlMoveChangeSyntax ControlMoveChange(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken semicolonToken, SeparatedSyntaxList<IdentifierNameSyntax> controls, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = changeKeyword.Kind;
		if ((uint)(kind - 120) > 3u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "changeKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (anchor == null)
		{
			throw new ArgumentNullException("anchor");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (ControlMoveChangeSyntax)InternalSyntaxFactory.ControlMoveChange((InternalSyntaxToken)changeKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (anchor == null) ? null : ((InternalIdentifierNameSyntax)anchor.Node), (InternalSyntaxToken)semicolonToken.Node, controls.Node.ToInternalSeparatedList<InternalIdentifierNameSyntax>(), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static ControlMoveChangeSyntax ControlMoveChange(SyntaxToken changeKeyword, IdentifierNameSyntax anchor, SeparatedSyntaxList<IdentifierNameSyntax> controls)
	{
		return ControlMoveChange(changeKeyword, Token(SyntaxKind.OpenParenToken), anchor, Token(SyntaxKind.SemicolonToken), controls, Token(SyntaxKind.CloseParenToken));
	}

	public static ControlMoveChangeSyntax ControlMoveChange(SyntaxToken changeKeyword, IdentifierNameSyntax anchor)
	{
		return ControlMoveChange(changeKeyword, Token(SyntaxKind.OpenParenToken), anchor, Token(SyntaxKind.SemicolonToken), default, Token(SyntaxKind.CloseParenToken));
	}

	public static ControlMoveChangeSyntax ControlMoveChange(SyntaxToken changeKeyword, string anchor)
	{
		return ControlMoveChange(changeKeyword, Token(SyntaxKind.OpenParenToken), IdentifierName(anchor), Token(SyntaxKind.SemicolonToken), default, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ControlModifyChangeSyntax ControlModifyChange(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = changeKeyword.Kind;
		if (kind != SyntaxKind.ModifyKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "changeKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (ControlModifyChangeSyntax)InternalSyntaxFactory.ControlModifyChange((InternalSyntaxToken)changeKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), triggers.Node.ToInternalList<InternalTriggerDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static ControlModifyChangeSyntax ControlModifyChange(IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return ControlModifyChange(Token(SyntaxKind.ModifyKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, triggers, Token(SyntaxKind.CloseBraceToken));
	}

	public static ControlModifyChangeSyntax ControlModifyChange(IdentifierNameSyntax name)
	{
		return ControlModifyChange(Token(SyntaxKind.ModifyKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static ControlModifyChangeSyntax ControlModifyChange(string name)
	{
		return ControlModifyChange(Token(SyntaxKind.ModifyKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageExtensionViewListSyntax PageExtensionViewList(SyntaxToken viewsKeyword, SyntaxToken openBraceToken, SyntaxList<ViewChangeBaseSyntax> changes, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = viewsKeyword.Kind;
		if (kind != SyntaxKind.ViewsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "viewsKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageExtensionViewListSyntax)InternalSyntaxFactory.PageExtensionViewList((InternalSyntaxToken)viewsKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, changes.Node.ToInternalList<InternalViewChangeBaseSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageExtensionViewListSyntax PageExtensionViewList(SyntaxList<ViewChangeBaseSyntax> changes = default(SyntaxList<ViewChangeBaseSyntax>))
	{
		return PageExtensionViewList(Token(SyntaxKind.ViewsKeyword), Token(SyntaxKind.OpenBraceToken), changes, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ViewAddChangeSyntax ViewAddChange(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, SyntaxList<PageViewSyntax> views, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = changeKeyword.Kind;
		if ((uint)(kind - 116) > 3u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "changeKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (ViewAddChangeSyntax)InternalSyntaxFactory.ViewAddChange((InternalSyntaxToken)changeKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (anchor == null) ? null : ((InternalIdentifierNameSyntax)anchor.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, views.Node.ToInternalList<InternalPageViewSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static ViewAddChangeSyntax ViewAddChange(SyntaxToken changeKeyword, IdentifierNameSyntax anchor, SyntaxList<PageViewSyntax> views)
	{
		return ViewAddChange(changeKeyword, default, anchor, default, Token(SyntaxKind.OpenBraceToken), views, Token(SyntaxKind.CloseBraceToken));
	}

	public static ViewAddChangeSyntax ViewAddChange(SyntaxToken changeKeyword)
	{
		return ViewAddChange(changeKeyword, default, null, default, Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ViewMoveChangeSyntax ViewMoveChange(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken semicolonToken, SeparatedSyntaxList<IdentifierNameSyntax> views, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = changeKeyword.Kind;
		if ((uint)(kind - 120) > 3u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "changeKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (ViewMoveChangeSyntax)InternalSyntaxFactory.ViewMoveChange((InternalSyntaxToken)changeKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (anchor == null) ? null : ((InternalIdentifierNameSyntax)anchor.Node), (InternalSyntaxToken)semicolonToken.Node, views.Node.ToInternalSeparatedList<InternalIdentifierNameSyntax>(), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static ViewMoveChangeSyntax ViewMoveChange(SyntaxToken changeKeyword, IdentifierNameSyntax anchor, SeparatedSyntaxList<IdentifierNameSyntax> views)
	{
		return ViewMoveChange(changeKeyword, Token(SyntaxKind.OpenParenToken), anchor, default, views, Token(SyntaxKind.CloseParenToken));
	}

	public static ViewMoveChangeSyntax ViewMoveChange(SyntaxToken changeKeyword)
	{
		return ViewMoveChange(changeKeyword, Token(SyntaxKind.OpenParenToken), null, default, default, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ViewModifyChangeSyntax ViewModifyChange(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = changeKeyword.Kind;
		if (kind != SyntaxKind.ModifyKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "changeKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (ViewModifyChangeSyntax)InternalSyntaxFactory.ViewModifyChange((InternalSyntaxToken)changeKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static ViewModifyChangeSyntax ViewModifyChange(IdentifierNameSyntax name, PropertyListSyntax propertyList)
	{
		return ViewModifyChange(Token(SyntaxKind.ModifyKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static ViewModifyChangeSyntax ViewModifyChange(IdentifierNameSyntax name)
	{
		return ViewModifyChange(Token(SyntaxKind.ModifyKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	public static ViewModifyChangeSyntax ViewModifyChange(string name)
	{
		return ViewModifyChange(Token(SyntaxKind.ModifyKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageExtensionActionListSyntax PageExtensionActionList(SyntaxToken actionsKeyword, SyntaxToken openBraceToken, SyntaxList<ActionChangeBaseSyntax> changes, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = actionsKeyword.Kind;
		if (kind != SyntaxKind.ActionsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "actionsKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (PageExtensionActionListSyntax)InternalSyntaxFactory.PageExtensionActionList((InternalSyntaxToken)actionsKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, changes.Node.ToInternalList<InternalActionChangeBaseSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static PageExtensionActionListSyntax PageExtensionActionList(SyntaxList<ActionChangeBaseSyntax> changes = default(SyntaxList<ActionChangeBaseSyntax>))
	{
		return PageExtensionActionList(Token(SyntaxKind.ActionsKeyword), Token(SyntaxKind.OpenBraceToken), changes, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ActionAddChangeSyntax ActionAddChange(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, SyntaxList<ActionBaseSyntax> actions, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = changeKeyword.Kind;
		if ((uint)(kind - 116) > 3u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "changeKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (anchor == null)
		{
			throw new ArgumentNullException("anchor");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (ActionAddChangeSyntax)InternalSyntaxFactory.ActionAddChange((InternalSyntaxToken)changeKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (anchor == null) ? null : ((InternalIdentifierNameSyntax)anchor.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, actions.Node.ToInternalList<InternalActionBaseSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static ActionAddChangeSyntax ActionAddChange(SyntaxToken changeKeyword, IdentifierNameSyntax anchor, SyntaxList<ActionBaseSyntax> actions)
	{
		return ActionAddChange(changeKeyword, Token(SyntaxKind.OpenParenToken), anchor, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), actions, Token(SyntaxKind.CloseBraceToken));
	}

	public static ActionAddChangeSyntax ActionAddChange(SyntaxToken changeKeyword, IdentifierNameSyntax anchor)
	{
		return ActionAddChange(changeKeyword, Token(SyntaxKind.OpenParenToken), anchor, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static ActionAddChangeSyntax ActionAddChange(SyntaxToken changeKeyword, string anchor)
	{
		return ActionAddChange(changeKeyword, Token(SyntaxKind.OpenParenToken), IdentifierName(anchor), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ActionMoveChangeSyntax ActionMoveChange(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken semicolonToken, SeparatedSyntaxList<IdentifierNameSyntax> actions, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = changeKeyword.Kind;
		if ((uint)(kind - 120) > 3u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "changeKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (anchor == null)
		{
			throw new ArgumentNullException("anchor");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (ActionMoveChangeSyntax)InternalSyntaxFactory.ActionMoveChange((InternalSyntaxToken)changeKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (anchor == null) ? null : ((InternalIdentifierNameSyntax)anchor.Node), (InternalSyntaxToken)semicolonToken.Node, actions.Node.ToInternalSeparatedList<InternalIdentifierNameSyntax>(), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static ActionMoveChangeSyntax ActionMoveChange(SyntaxToken changeKeyword, IdentifierNameSyntax anchor, SeparatedSyntaxList<IdentifierNameSyntax> actions)
	{
		return ActionMoveChange(changeKeyword, Token(SyntaxKind.OpenParenToken), anchor, Token(SyntaxKind.SemicolonToken), actions, Token(SyntaxKind.CloseParenToken));
	}

	public static ActionMoveChangeSyntax ActionMoveChange(SyntaxToken changeKeyword, IdentifierNameSyntax anchor)
	{
		return ActionMoveChange(changeKeyword, Token(SyntaxKind.OpenParenToken), anchor, Token(SyntaxKind.SemicolonToken), default, Token(SyntaxKind.CloseParenToken));
	}

	public static ActionMoveChangeSyntax ActionMoveChange(SyntaxToken changeKeyword, string anchor)
	{
		return ActionMoveChange(changeKeyword, Token(SyntaxKind.OpenParenToken), IdentifierName(anchor), Token(SyntaxKind.SemicolonToken), default, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ActionModifyChangeSyntax ActionModifyChange(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = changeKeyword.Kind;
		if (kind != SyntaxKind.ModifyKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "changeKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (ActionModifyChangeSyntax)InternalSyntaxFactory.ActionModifyChange((InternalSyntaxToken)changeKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), triggers.Node.ToInternalList<InternalTriggerDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static ActionModifyChangeSyntax ActionModifyChange(IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return ActionModifyChange(Token(SyntaxKind.ModifyKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, triggers, Token(SyntaxKind.CloseBraceToken));
	}

	public static ActionModifyChangeSyntax ActionModifyChange(IdentifierNameSyntax name)
	{
		return ActionModifyChange(Token(SyntaxKind.ModifyKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static ActionModifyChangeSyntax ActionModifyChange(string name)
	{
		return ActionModifyChange(Token(SyntaxKind.ModifyKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static QuerySyntax Query(SyntaxToken queryKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, QueryElementsSyntax elements, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = queryKeyword.Kind;
		if (kind != SyntaxKind.QueryKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "queryKeyword");
		}
		if (objectId == null)
		{
			throw new ArgumentNullException("objectId");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (QuerySyntax)InternalSyntaxFactory.Query((InternalSyntaxToken)queryKeyword.Node, (objectId == null) ? null : ((InternalObjectIdSyntax)objectId.Node), (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (elements == null) ? null : ((InternalQueryElementsSyntax)elements.Node), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static QuerySyntax Query(ObjectIdSyntax objectId, IdentifierNameSyntax name, PropertyListSyntax propertyList, QueryElementsSyntax elements, SyntaxList<MemberSyntax> members)
	{
		return Query(Token(SyntaxKind.QueryKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), propertyList, elements, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static QuerySyntax Query(ObjectIdSyntax objectId, IdentifierNameSyntax name)
	{
		return Query(Token(SyntaxKind.QueryKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), PropertyList(), null, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static QuerySyntax Query(ObjectIdSyntax objectId, string name)
	{
		return Query(Token(SyntaxKind.QueryKeyword), objectId, IdentifierName(name), Token(SyntaxKind.OpenBraceToken), PropertyList(), null, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static QueryElementsSyntax QueryElements(SyntaxToken queryElementsKeyword, SyntaxToken openBraceToken, SyntaxList<QueryDataItemSyntax> dataItems, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = queryElementsKeyword.Kind;
		if (kind != SyntaxKind.QueryElementsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "queryElementsKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (QueryElementsSyntax)InternalSyntaxFactory.QueryElements((InternalSyntaxToken)queryElementsKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, dataItems.Node.ToInternalList<InternalQueryDataItemSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static QueryElementsSyntax QueryElements(SyntaxList<QueryDataItemSyntax> dataItems = default(SyntaxList<QueryDataItemSyntax>))
	{
		return QueryElements(Token(SyntaxKind.QueryElementsKeyword), Token(SyntaxKind.OpenBraceToken), dataItems, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static QueryDataItemSyntax QueryDataItem(SyntaxToken dataItemKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, ObjectNameOrIdSyntax dataItemTable, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<QueryDataItemElementSyntax> elements, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = dataItemKeyword.Kind;
		if (kind != SyntaxKind.DataItemKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "dataItemKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (dataItemTable == null)
		{
			throw new ArgumentNullException("dataItemTable");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (QueryDataItemSyntax)InternalSyntaxFactory.QueryDataItem((InternalSyntaxToken)dataItemKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (dataItemTable == null) ? null : ((InternalObjectNameOrIdSyntax)dataItemTable.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), elements.Node.ToInternalList<InternalQueryDataItemElementSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static QueryDataItemSyntax QueryDataItem(IdentifierNameSyntax name, ObjectNameOrIdSyntax dataItemTable, PropertyListSyntax propertyList, SyntaxList<QueryDataItemElementSyntax> elements)
	{
		return QueryDataItem(Token(SyntaxKind.DataItemKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), dataItemTable, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, elements, Token(SyntaxKind.CloseBraceToken));
	}

	public static QueryDataItemSyntax QueryDataItem(IdentifierNameSyntax name, ObjectNameOrIdSyntax dataItemTable)
	{
		return QueryDataItem(Token(SyntaxKind.DataItemKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), dataItemTable, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static QueryDataItemSyntax QueryDataItem(string name, ObjectNameOrIdSyntax dataItemTable)
	{
		return QueryDataItem(Token(SyntaxKind.DataItemKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), dataItemTable, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static QueryColumnSyntax QueryColumn(SyntaxToken columnKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, IdentifierNameSyntax relatedField, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = columnKeyword.Kind;
		if (kind != SyntaxKind.ColumnKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "columnKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (QueryColumnSyntax)InternalSyntaxFactory.QueryColumn((InternalSyntaxToken)columnKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (relatedField == null) ? null : ((InternalIdentifierNameSyntax)relatedField.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static QueryColumnSyntax QueryColumn(IdentifierNameSyntax name, IdentifierNameSyntax relatedField, PropertyListSyntax propertyList)
	{
		return QueryColumn(Token(SyntaxKind.ColumnKeyword), Token(SyntaxKind.OpenParenToken), name, default, relatedField, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static QueryColumnSyntax QueryColumn(IdentifierNameSyntax name)
	{
		return QueryColumn(Token(SyntaxKind.ColumnKeyword), Token(SyntaxKind.OpenParenToken), name, default, null, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	public static QueryColumnSyntax QueryColumn(string name)
	{
		return QueryColumn(Token(SyntaxKind.ColumnKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), default, null, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static QueryFilterSyntax QueryFilter(SyntaxToken filterKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, IdentifierNameSyntax relatedField, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = filterKeyword.Kind;
		if (kind != SyntaxKind.FilterKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "filterKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (relatedField == null)
		{
			throw new ArgumentNullException("relatedField");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (QueryFilterSyntax)InternalSyntaxFactory.QueryFilter((InternalSyntaxToken)filterKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (relatedField == null) ? null : ((InternalIdentifierNameSyntax)relatedField.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static QueryFilterSyntax QueryFilter(IdentifierNameSyntax name, IdentifierNameSyntax relatedField, PropertyListSyntax propertyList)
	{
		return QueryFilter(Token(SyntaxKind.FilterKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), relatedField, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static QueryFilterSyntax QueryFilter(IdentifierNameSyntax name, IdentifierNameSyntax relatedField)
	{
		return QueryFilter(Token(SyntaxKind.FilterKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), relatedField, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	public static QueryFilterSyntax QueryFilter(string name, string relatedField)
	{
		return QueryFilter(Token(SyntaxKind.FilterKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), IdentifierName(relatedField), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ReportSyntax Report(SyntaxToken reportKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, ReportDataSetSectionSyntax dataSet, RequestPageSyntax requestPage, ReportLabelsSectionSyntax labels, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = reportKeyword.Kind;
		if (kind != SyntaxKind.ReportKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "reportKeyword");
		}
		if (objectId == null)
		{
			throw new ArgumentNullException("objectId");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (ReportSyntax)InternalSyntaxFactory.Report((InternalSyntaxToken)reportKeyword.Node, (objectId == null) ? null : ((InternalObjectIdSyntax)objectId.Node), (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (dataSet == null) ? null : ((InternalReportDataSetSectionSyntax)dataSet.Node), (requestPage == null) ? null : ((InternalRequestPageSyntax)requestPage.Node), (labels == null) ? null : ((InternalReportLabelsSectionSyntax)labels.Node), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static ReportSyntax Report(ObjectIdSyntax objectId, IdentifierNameSyntax name, PropertyListSyntax propertyList, ReportDataSetSectionSyntax dataSet, RequestPageSyntax requestPage, ReportLabelsSectionSyntax labels, SyntaxList<MemberSyntax> members)
	{
		return Report(Token(SyntaxKind.ReportKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), propertyList, dataSet, requestPage, labels, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static ReportSyntax Report(ObjectIdSyntax objectId, IdentifierNameSyntax name)
	{
		return Report(Token(SyntaxKind.ReportKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), PropertyList(), null, null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static ReportSyntax Report(ObjectIdSyntax objectId, string name)
	{
		return Report(Token(SyntaxKind.ReportKeyword), objectId, IdentifierName(name), Token(SyntaxKind.OpenBraceToken), PropertyList(), null, null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ReportDataSetSectionSyntax ReportDataSetSection(SyntaxToken dataSetKeyword, SyntaxToken openBraceToken, SyntaxList<ReportDataItemSyntax> dataItems, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = dataSetKeyword.Kind;
		if (kind != SyntaxKind.DataSetKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "dataSetKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (ReportDataSetSectionSyntax)InternalSyntaxFactory.ReportDataSetSection((InternalSyntaxToken)dataSetKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, dataItems.Node.ToInternalList<InternalReportDataItemSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static ReportDataSetSectionSyntax ReportDataSetSection(SyntaxList<ReportDataItemSyntax> dataItems = default(SyntaxList<ReportDataItemSyntax>))
	{
		return ReportDataSetSection(Token(SyntaxKind.DataSetKeyword), Token(SyntaxKind.OpenBraceToken), dataItems, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ReportDataItemSyntax ReportDataItem(SyntaxToken dataItemKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, ObjectNameOrIdSyntax dataItemTable, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<ReportDataItemElementSyntax> elements, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = dataItemKeyword.Kind;
		if (kind != SyntaxKind.DataItemKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "dataItemKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (dataItemTable == null)
		{
			throw new ArgumentNullException("dataItemTable");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (ReportDataItemSyntax)InternalSyntaxFactory.ReportDataItem((InternalSyntaxToken)dataItemKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (dataItemTable == null) ? null : ((InternalObjectNameOrIdSyntax)dataItemTable.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), elements.Node.ToInternalList<InternalReportDataItemElementSyntax>(), triggers.Node.ToInternalList<InternalTriggerDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static ReportDataItemSyntax ReportDataItem(IdentifierNameSyntax name, ObjectNameOrIdSyntax dataItemTable, PropertyListSyntax propertyList, SyntaxList<ReportDataItemElementSyntax> elements, SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return ReportDataItem(Token(SyntaxKind.DataItemKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), dataItemTable, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, elements, triggers, Token(SyntaxKind.CloseBraceToken));
	}

	public static ReportDataItemSyntax ReportDataItem(IdentifierNameSyntax name, ObjectNameOrIdSyntax dataItemTable)
	{
		return ReportDataItem(Token(SyntaxKind.DataItemKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), dataItemTable, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static ReportDataItemSyntax ReportDataItem(string name, ObjectNameOrIdSyntax dataItemTable)
	{
		return ReportDataItem(Token(SyntaxKind.DataItemKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), dataItemTable, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ReportColumnSyntax ReportColumn(SyntaxToken columnKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, CodeExpressionSyntax sourceExpression, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = columnKeyword.Kind;
		if (kind != SyntaxKind.ColumnKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "columnKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (sourceExpression == null)
		{
			throw new ArgumentNullException("sourceExpression");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (ReportColumnSyntax)InternalSyntaxFactory.ReportColumn((InternalSyntaxToken)columnKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (sourceExpression == null) ? null : ((InternalCodeExpressionSyntax)sourceExpression.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static ReportColumnSyntax ReportColumn(IdentifierNameSyntax name, CodeExpressionSyntax sourceExpression, PropertyListSyntax propertyList)
	{
		return ReportColumn(Token(SyntaxKind.ColumnKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), sourceExpression, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, Token(SyntaxKind.CloseBraceToken));
	}

	public static ReportColumnSyntax ReportColumn(IdentifierNameSyntax name, CodeExpressionSyntax sourceExpression)
	{
		return ReportColumn(Token(SyntaxKind.ColumnKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), sourceExpression, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	public static ReportColumnSyntax ReportColumn(string name, CodeExpressionSyntax sourceExpression)
	{
		return ReportColumn(Token(SyntaxKind.ColumnKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), sourceExpression, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ReportLabelsSectionSyntax ReportLabelsSection(SyntaxToken labelsKeyword, SyntaxToken openBraceToken, SyntaxList<ReportLabelBaseSyntax> labels, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = labelsKeyword.Kind;
		if (kind != SyntaxKind.LabelsKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "labelsKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (ReportLabelsSectionSyntax)InternalSyntaxFactory.ReportLabelsSection((InternalSyntaxToken)labelsKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, labels.Node.ToInternalList<InternalReportLabelBaseSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static ReportLabelsSectionSyntax ReportLabelsSection(SyntaxList<ReportLabelBaseSyntax> labels = default(SyntaxList<ReportLabelBaseSyntax>))
	{
		return ReportLabelsSection(Token(SyntaxKind.LabelsKeyword), Token(SyntaxKind.OpenBraceToken), labels, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ReportLabelMultilanguageSyntax ReportLabelMultilanguage(SyntaxToken labelKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, CommaSeparatedIdentifierEqualsStringListSyntax caption, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = labelKeyword.Kind;
		if (kind != SyntaxKind.LabelKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "labelKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (caption == null)
		{
			throw new ArgumentNullException("caption");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (ReportLabelMultilanguageSyntax)InternalSyntaxFactory.ReportLabelMultilanguage((InternalSyntaxToken)labelKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (caption == null) ? null : ((InternalCommaSeparatedIdentifierEqualsStringListSyntax)caption.Node), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static ReportLabelMultilanguageSyntax ReportLabelMultilanguage(IdentifierNameSyntax name, CommaSeparatedIdentifierEqualsStringListSyntax caption)
	{
		return ReportLabelMultilanguage(Token(SyntaxKind.LabelKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), caption, Token(SyntaxKind.CloseParenToken));
	}

	public static ReportLabelMultilanguageSyntax ReportLabelMultilanguage(IdentifierNameSyntax name)
	{
		return ReportLabelMultilanguage(Token(SyntaxKind.LabelKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), CommaSeparatedIdentifierEqualsStringList(), Token(SyntaxKind.CloseParenToken));
	}

	public static ReportLabelMultilanguageSyntax ReportLabelMultilanguage(string name)
	{
		return ReportLabelMultilanguage(Token(SyntaxKind.LabelKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), CommaSeparatedIdentifierEqualsStringList(), Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ReportLabelSyntax ReportLabel(IdentifierNameSyntax name, SyntaxToken equalsToken, LabelSyntax label, SyntaxToken semicolonToken)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		SyntaxKind kind = equalsToken.Kind;
		if (kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		if (label == null)
		{
			throw new ArgumentNullException("label");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (ReportLabelSyntax)InternalSyntaxFactory.ReportLabel((name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)equalsToken.Node, (label == null) ? null : ((InternalLabelSyntax)label.Node), (InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static ReportLabelSyntax ReportLabel(IdentifierNameSyntax name, LabelSyntax label)
	{
		return ReportLabel(name, Token(SyntaxKind.EqualsToken), label, Token(SyntaxKind.SemicolonToken));
	}

	public static ReportLabelSyntax ReportLabel(string name, LabelSyntax label)
	{
		return ReportLabel(IdentifierName(name), Token(SyntaxKind.EqualsToken), label, Token(SyntaxKind.SemicolonToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static RequestPageSyntax RequestPage(SyntaxToken requestPageKeyword, SyntaxToken openBraceToken, IdentifierNameSyntax name, PropertyListSyntax propertyList, PageLayoutSyntax layout, PageActionListSyntax actions, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = requestPageKeyword.Kind;
		if (kind != SyntaxKind.RequestPageKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "requestPageKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (RequestPageSyntax)InternalSyntaxFactory.RequestPage((InternalSyntaxToken)requestPageKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (layout == null) ? null : ((InternalPageLayoutSyntax)layout.Node), (actions == null) ? null : ((InternalPageActionListSyntax)actions.Node), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static RequestPageSyntax RequestPage(IdentifierNameSyntax name, PropertyListSyntax propertyList, PageLayoutSyntax layout, PageActionListSyntax actions, SyntaxList<MemberSyntax> members)
	{
		return RequestPage(Token(SyntaxKind.RequestPageKeyword), Token(SyntaxKind.OpenBraceToken), name, propertyList, layout, actions, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static RequestPageSyntax RequestPage(SyntaxList<MemberSyntax> members = default(SyntaxList<MemberSyntax>))
	{
		return RequestPage(Token(SyntaxKind.RequestPageKeyword), Token(SyntaxKind.OpenBraceToken), null, PropertyList(), null, null, members, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static XmlPortSyntax XmlPort(SyntaxToken xmlPortKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, XmlPortSchemaSyntax xmlPortSchema, RequestPageSyntax xmlPortRequestPage, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = xmlPortKeyword.Kind;
		if (kind != SyntaxKind.XmlPortKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "xmlPortKeyword");
		}
		if (objectId == null)
		{
			throw new ArgumentNullException("objectId");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (XmlPortSyntax)InternalSyntaxFactory.XmlPort((InternalSyntaxToken)xmlPortKeyword.Node, (objectId == null) ? null : ((InternalObjectIdSyntax)objectId.Node), (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), (xmlPortSchema == null) ? null : ((InternalXmlPortSchemaSyntax)xmlPortSchema.Node), (xmlPortRequestPage == null) ? null : ((InternalRequestPageSyntax)xmlPortRequestPage.Node), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static XmlPortSyntax XmlPort(ObjectIdSyntax objectId, IdentifierNameSyntax name, PropertyListSyntax propertyList, XmlPortSchemaSyntax xmlPortSchema, RequestPageSyntax xmlPortRequestPage, SyntaxList<MemberSyntax> members)
	{
		return XmlPort(Token(SyntaxKind.XmlPortKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), propertyList, xmlPortSchema, xmlPortRequestPage, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static XmlPortSyntax XmlPort(ObjectIdSyntax objectId, IdentifierNameSyntax name)
	{
		return XmlPort(Token(SyntaxKind.XmlPortKeyword), objectId, name, Token(SyntaxKind.OpenBraceToken), PropertyList(), null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static XmlPortSyntax XmlPort(ObjectIdSyntax objectId, string name)
	{
		return XmlPort(Token(SyntaxKind.XmlPortKeyword), objectId, IdentifierName(name), Token(SyntaxKind.OpenBraceToken), PropertyList(), null, null, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static XmlPortSchemaSyntax XmlPortSchema(SyntaxToken xmlPortSchemaKeyword, SyntaxToken openBraceToken, SyntaxList<XmlPortNodeSyntax> xmlPortSchema, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = xmlPortSchemaKeyword.Kind;
		if (kind != SyntaxKind.XmlPortSchemaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "xmlPortSchemaKeyword");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (XmlPortSchemaSyntax)InternalSyntaxFactory.XmlPortSchema((InternalSyntaxToken)xmlPortSchemaKeyword.Node, (InternalSyntaxToken)openBraceToken.Node, xmlPortSchema.Node.ToInternalList<InternalXmlPortNodeSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static XmlPortSchemaSyntax XmlPortSchema(SyntaxList<XmlPortNodeSyntax> xmlPortSchema = default(SyntaxList<XmlPortNodeSyntax>))
	{
		return XmlPortSchema(Token(SyntaxKind.XmlPortSchemaKeyword), Token(SyntaxKind.OpenBraceToken), xmlPortSchema, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static XmlPortTableElementSyntax XmlPortTableElement(SyntaxToken xmlPortTableElementKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, ObjectNameOrIdSyntax sourceTable, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = xmlPortTableElementKeyword.Kind;
		if (kind != SyntaxKind.XmlPortTableElementKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "xmlPortTableElementKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (sourceTable == null)
		{
			throw new ArgumentNullException("sourceTable");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (XmlPortTableElementSyntax)InternalSyntaxFactory.XmlPortTableElement((InternalSyntaxToken)xmlPortTableElementKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (sourceTable == null) ? null : ((InternalObjectNameOrIdSyntax)sourceTable.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), schema.Node.ToInternalList<InternalXmlPortNodeSyntax>(), triggers.Node.ToInternalList<InternalTriggerDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static XmlPortTableElementSyntax XmlPortTableElement(IdentifierNameSyntax name, ObjectNameOrIdSyntax sourceTable, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return XmlPortTableElement(Token(SyntaxKind.XmlPortTableElementKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), sourceTable, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, schema, triggers, Token(SyntaxKind.CloseBraceToken));
	}

	public static XmlPortTableElementSyntax XmlPortTableElement(IdentifierNameSyntax name, ObjectNameOrIdSyntax sourceTable)
	{
		return XmlPortTableElement(Token(SyntaxKind.XmlPortTableElementKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), sourceTable, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static XmlPortTableElementSyntax XmlPortTableElement(string name, ObjectNameOrIdSyntax sourceTable)
	{
		return XmlPortTableElement(Token(SyntaxKind.XmlPortTableElementKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), sourceTable, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static XmlPortFieldElementSyntax XmlPortFieldElement(SyntaxToken xmlPortFieldElementKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, CodeExpressionSyntax sourceField, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = xmlPortFieldElementKeyword.Kind;
		if (kind != SyntaxKind.XmlPortFieldElementKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "xmlPortFieldElementKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (sourceField == null)
		{
			throw new ArgumentNullException("sourceField");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (XmlPortFieldElementSyntax)InternalSyntaxFactory.XmlPortFieldElement((InternalSyntaxToken)xmlPortFieldElementKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (sourceField == null) ? null : ((InternalCodeExpressionSyntax)sourceField.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), schema.Node.ToInternalList<InternalXmlPortNodeSyntax>(), triggers.Node.ToInternalList<InternalTriggerDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static XmlPortFieldElementSyntax XmlPortFieldElement(IdentifierNameSyntax name, CodeExpressionSyntax sourceField, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return XmlPortFieldElement(Token(SyntaxKind.XmlPortFieldElementKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), sourceField, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, schema, triggers, Token(SyntaxKind.CloseBraceToken));
	}

	public static XmlPortFieldElementSyntax XmlPortFieldElement(IdentifierNameSyntax name, CodeExpressionSyntax sourceField)
	{
		return XmlPortFieldElement(Token(SyntaxKind.XmlPortFieldElementKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), sourceField, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static XmlPortFieldElementSyntax XmlPortFieldElement(string name, CodeExpressionSyntax sourceField)
	{
		return XmlPortFieldElement(Token(SyntaxKind.XmlPortFieldElementKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), sourceField, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static XmlPortFieldAttributeSyntax XmlPortFieldAttribute(SyntaxToken xmlPortFieldAttributeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, CodeExpressionSyntax sourceField, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = xmlPortFieldAttributeKeyword.Kind;
		if (kind != SyntaxKind.XmlPortFieldAttributeKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "xmlPortFieldAttributeKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (sourceField == null)
		{
			throw new ArgumentNullException("sourceField");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (XmlPortFieldAttributeSyntax)InternalSyntaxFactory.XmlPortFieldAttribute((InternalSyntaxToken)xmlPortFieldAttributeKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)semicolonToken.Node, (sourceField == null) ? null : ((InternalCodeExpressionSyntax)sourceField.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), schema.Node.ToInternalList<InternalXmlPortNodeSyntax>(), triggers.Node.ToInternalList<InternalTriggerDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static XmlPortFieldAttributeSyntax XmlPortFieldAttribute(IdentifierNameSyntax name, CodeExpressionSyntax sourceField, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return XmlPortFieldAttribute(Token(SyntaxKind.XmlPortFieldAttributeKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), sourceField, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, schema, triggers, Token(SyntaxKind.CloseBraceToken));
	}

	public static XmlPortFieldAttributeSyntax XmlPortFieldAttribute(IdentifierNameSyntax name, CodeExpressionSyntax sourceField)
	{
		return XmlPortFieldAttribute(Token(SyntaxKind.XmlPortFieldAttributeKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.SemicolonToken), sourceField, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static XmlPortFieldAttributeSyntax XmlPortFieldAttribute(string name, CodeExpressionSyntax sourceField)
	{
		return XmlPortFieldAttribute(Token(SyntaxKind.XmlPortFieldAttributeKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.SemicolonToken), sourceField, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static XmlPortTextElementSyntax XmlPortTextElement(SyntaxToken xmlPortTextElementKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = xmlPortTextElementKeyword.Kind;
		if (kind != SyntaxKind.XmlPortTextElementKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "xmlPortTextElementKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (XmlPortTextElementSyntax)InternalSyntaxFactory.XmlPortTextElement((InternalSyntaxToken)xmlPortTextElementKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), schema.Node.ToInternalList<InternalXmlPortNodeSyntax>(), triggers.Node.ToInternalList<InternalTriggerDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static XmlPortTextElementSyntax XmlPortTextElement(IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return XmlPortTextElement(Token(SyntaxKind.XmlPortTextElementKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, schema, triggers, Token(SyntaxKind.CloseBraceToken));
	}

	public static XmlPortTextElementSyntax XmlPortTextElement(IdentifierNameSyntax name)
	{
		return XmlPortTextElement(Token(SyntaxKind.XmlPortTextElementKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static XmlPortTextElementSyntax XmlPortTextElement(string name)
	{
		return XmlPortTextElement(Token(SyntaxKind.XmlPortTextElementKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static XmlPortTextAttributeSyntax XmlPortTextAttribute(SyntaxToken xmlPortTextAttributeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = xmlPortTextAttributeKeyword.Kind;
		if (kind != SyntaxKind.XmlPortTextAttributeKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "xmlPortTextAttributeKeyword");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (XmlPortTextAttributeSyntax)InternalSyntaxFactory.XmlPortTextAttribute((InternalSyntaxToken)xmlPortTextAttributeKeyword.Node, (InternalSyntaxToken)openParenthesisToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), schema.Node.ToInternalList<InternalXmlPortNodeSyntax>(), triggers.Node.ToInternalList<InternalTriggerDeclarationSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static XmlPortTextAttributeSyntax XmlPortTextAttribute(IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return XmlPortTextAttribute(Token(SyntaxKind.XmlPortTextAttributeKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), propertyList, schema, triggers, Token(SyntaxKind.CloseBraceToken));
	}

	public static XmlPortTextAttributeSyntax XmlPortTextAttribute(IdentifierNameSyntax name)
	{
		return XmlPortTextAttribute(Token(SyntaxKind.XmlPortTextAttributeKeyword), Token(SyntaxKind.OpenParenToken), name, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, default, Token(SyntaxKind.CloseBraceToken));
	}

	public static XmlPortTextAttributeSyntax XmlPortTextAttribute(string name)
	{
		return XmlPortTextAttribute(Token(SyntaxKind.XmlPortTextAttributeKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(name), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ControlAddInSyntax ControlAddIn(SyntaxToken controlAddInKeyword, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		SyntaxKind kind = controlAddInKeyword.Kind;
		if (kind != SyntaxKind.ControlAddInKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "controlAddInKeyword");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = openBraceToken.Kind;
		if (kind != SyntaxKind.OpenBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBraceToken");
		}
		if (propertyList == null)
		{
			throw new ArgumentNullException("propertyList");
		}
		kind = closeBraceToken.Kind;
		if (kind != SyntaxKind.CloseBraceToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBraceToken");
		}
		return (ControlAddInSyntax)InternalSyntaxFactory.ControlAddIn((InternalSyntaxToken)controlAddInKeyword.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)openBraceToken.Node, (propertyList == null) ? null : ((InternalPropertyListSyntax)propertyList.Node), members.Node.ToInternalList<InternalMemberSyntax>(), (InternalSyntaxToken)closeBraceToken.Node).CreateRed();
	}

	public static ControlAddInSyntax ControlAddIn(IdentifierNameSyntax name, PropertyListSyntax propertyList, SyntaxList<MemberSyntax> members)
	{
		return ControlAddIn(Token(SyntaxKind.ControlAddInKeyword), name, Token(SyntaxKind.OpenBraceToken), propertyList, members, Token(SyntaxKind.CloseBraceToken));
	}

	public static ControlAddInSyntax ControlAddIn(IdentifierNameSyntax name)
	{
		return ControlAddIn(Token(SyntaxKind.ControlAddInKeyword), name, Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	public static ControlAddInSyntax ControlAddIn(string name)
	{
		return ControlAddIn(Token(SyntaxKind.ControlAddInKeyword), IdentifierName(name), Token(SyntaxKind.OpenBraceToken), PropertyList(), default, Token(SyntaxKind.CloseBraceToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static MemberAttributeSyntax MemberAttribute(SyntaxToken openBracketToken, IdentifierNameSyntax name, AttributeArgumentListSyntax argumentList, SyntaxToken closeBracketToken)
	{
		SyntaxKind kind = openBracketToken.Kind;
		if (kind != SyntaxKind.OpenBracketToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBracketToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = closeBracketToken.Kind;
		if (kind != SyntaxKind.CloseBracketToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBracketToken");
		}
		return (MemberAttributeSyntax)InternalSyntaxFactory.MemberAttribute((InternalSyntaxToken)openBracketToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (argumentList == null) ? null : ((InternalAttributeArgumentListSyntax)argumentList.Node), (InternalSyntaxToken)closeBracketToken.Node).CreateRed();
	}

	public static MemberAttributeSyntax MemberAttribute(IdentifierNameSyntax name, AttributeArgumentListSyntax argumentList)
	{
		return MemberAttribute(Token(SyntaxKind.OpenBracketToken), name, argumentList, Token(SyntaxKind.CloseBracketToken));
	}

	public static MemberAttributeSyntax MemberAttribute(IdentifierNameSyntax name)
	{
		return MemberAttribute(Token(SyntaxKind.OpenBracketToken), name, null, Token(SyntaxKind.CloseBracketToken));
	}

	public static MemberAttributeSyntax MemberAttribute(string name)
	{
		return MemberAttribute(Token(SyntaxKind.OpenBracketToken), IdentifierName(name), null, Token(SyntaxKind.CloseBracketToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static AttributeArgumentListSyntax AttributeArgumentList(SyntaxToken openParenthesisToken, SeparatedSyntaxList<AttributeArgumentSyntax> arguments, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (AttributeArgumentListSyntax)InternalSyntaxFactory.AttributeArgumentList((InternalSyntaxToken)openParenthesisToken.Node, arguments.Node.ToInternalSeparatedList<InternalAttributeArgumentSyntax>(), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static AttributeArgumentListSyntax AttributeArgumentList(SeparatedSyntaxList<AttributeArgumentSyntax> arguments = default(SeparatedSyntaxList<AttributeArgumentSyntax>))
	{
		return AttributeArgumentList(Token(SyntaxKind.OpenParenToken), arguments, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static InvalidAttributeArgumentSyntax InvalidAttributeArgument(SyntaxNode invalidToken)
	{
		return (InvalidAttributeArgumentSyntax)InternalSyntaxFactory.InvalidAttributeArgument(invalidToken?.Node).CreateRed();
	}

	public static InvalidAttributeArgumentSyntax InvalidAttributeArgument()
	{
		return InvalidAttributeArgument(null);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static LiteralAttributeArgumentSyntax LiteralAttributeArgument(LiteralExpressionSyntax literal)
	{
		if (literal == null)
		{
			throw new ArgumentNullException("literal");
		}
		return (LiteralAttributeArgumentSyntax)InternalSyntaxFactory.LiteralAttributeArgument((literal == null) ? null : ((InternalLiteralExpressionSyntax)literal.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static MethodReferenceAttributeArgumentSyntax MethodReferenceAttributeArgument(MemberAccessExpressionSyntax methodReference)
	{
		if (methodReference == null)
		{
			throw new ArgumentNullException("methodReference");
		}
		return (MethodReferenceAttributeArgumentSyntax)InternalSyntaxFactory.MethodReferenceAttributeArgument((methodReference == null) ? null : ((InternalMemberAccessExpressionSyntax)methodReference.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static OptionAccessAttributeArgumentSyntax OptionAccessAttributeArgument(OptionAccessExpressionSyntax optionAccess)
	{
		if (optionAccess == null)
		{
			throw new ArgumentNullException("optionAccess");
		}
		return (OptionAccessAttributeArgumentSyntax)InternalSyntaxFactory.OptionAccessAttributeArgument((optionAccess == null) ? null : ((InternalOptionAccessExpressionSyntax)optionAccess.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static IdentifierNameOrEmptySyntax IdentifierNameOrEmpty(IdentifierNameSyntax identifierName, SyntaxToken empty)
	{
		SyntaxKind kind = empty.Kind;
		if ((uint)kind > 1u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "empty");
		}
		return (IdentifierNameOrEmptySyntax)InternalSyntaxFactory.IdentifierNameOrEmpty((identifierName == null) ? null : ((InternalIdentifierNameSyntax)identifierName.Node), (InternalSyntaxToken)empty.Node).CreateRed();
	}

	public static IdentifierNameOrEmptySyntax IdentifierNameOrEmpty(IdentifierNameSyntax identifierName = null)
	{
		return IdentifierNameOrEmpty(identifierName, default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static LiteralExpressionSyntax LiteralExpression(LiteralValueSyntax literal)
	{
		if (literal == null)
		{
			throw new ArgumentNullException("literal");
		}
		return (LiteralExpressionSyntax)InternalSyntaxFactory.LiteralExpression((literal == null) ? null : ((InternalLiteralValueSyntax)literal.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static OptionValuesSyntax OptionValues(SeparatedSyntaxList<IdentifierNameOrEmptySyntax> options)
	{
		return (OptionValuesSyntax)InternalSyntaxFactory.OptionValues(options.Node.ToInternalSeparatedList<InternalIdentifierNameOrEmptySyntax>()).CreateRed();
	}

	public static OptionValuesSyntax OptionValues()
	{
		return OptionValues(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static Int32SignedLiteralValueSyntax Int32SignedLiteralValue(SyntaxToken sign, SyntaxToken number)
	{
		SyntaxKind kind = sign.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.MinusToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "sign");
		}
		kind = number.Kind;
		if (kind != SyntaxKind.Int32LiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "number");
		}
		return (Int32SignedLiteralValueSyntax)InternalSyntaxFactory.Int32SignedLiteralValue((InternalSyntaxToken)sign.Node, (InternalSyntaxToken)number.Node).CreateRed();
	}

	public static Int32SignedLiteralValueSyntax Int32SignedLiteralValue(SyntaxToken number)
	{
		return Int32SignedLiteralValue(default, number);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static Int64SignedLiteralValueSyntax Int64SignedLiteralValue(SyntaxToken sign, SyntaxToken number)
	{
		SyntaxKind kind = sign.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.MinusToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "sign");
		}
		kind = number.Kind;
		if (kind != SyntaxKind.Int64LiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "number");
		}
		return (Int64SignedLiteralValueSyntax)InternalSyntaxFactory.Int64SignedLiteralValue((InternalSyntaxToken)sign.Node, (InternalSyntaxToken)number.Node).CreateRed();
	}

	public static Int64SignedLiteralValueSyntax Int64SignedLiteralValue(SyntaxToken number)
	{
		return Int64SignedLiteralValue(default, number);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DecimalSignedLiteralValueSyntax DecimalSignedLiteralValue(SyntaxToken sign, SyntaxToken number)
	{
		SyntaxKind kind = sign.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.MinusToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "sign");
		}
		kind = number.Kind;
		if (kind != SyntaxKind.DecimalLiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "number");
		}
		return (DecimalSignedLiteralValueSyntax)InternalSyntaxFactory.DecimalSignedLiteralValue((InternalSyntaxToken)sign.Node, (InternalSyntaxToken)number.Node).CreateRed();
	}

	public static DecimalSignedLiteralValueSyntax DecimalSignedLiteralValue(SyntaxToken number)
	{
		return DecimalSignedLiteralValue(default, number);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static BooleanLiteralValueSyntax BooleanLiteralValue(SyntaxToken value)
	{
		SyntaxKind kind = value.Kind;
		if ((uint)(kind - 2) > 1u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "value");
		}
		return (BooleanLiteralValueSyntax)InternalSyntaxFactory.BooleanLiteralValue((InternalSyntaxToken)value.Node).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static StringLiteralValueSyntax StringLiteralValue(SyntaxToken value)
	{
		SyntaxKind kind = value.Kind;
		if (kind != SyntaxKind.StringLiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "value");
		}
		return (StringLiteralValueSyntax)InternalSyntaxFactory.StringLiteralValue((InternalSyntaxToken)value.Node).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DateLiteralValueSyntax DateLiteralValue(SyntaxToken value)
	{
		SyntaxKind kind = value.Kind;
		if (kind != SyntaxKind.DateLiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "value");
		}
		return (DateLiteralValueSyntax)InternalSyntaxFactory.DateLiteralValue((InternalSyntaxToken)value.Node).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static TimeLiteralValueSyntax TimeLiteralValue(SyntaxToken value)
	{
		SyntaxKind kind = value.Kind;
		if (kind != SyntaxKind.TimeLiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "value");
		}
		return (TimeLiteralValueSyntax)InternalSyntaxFactory.TimeLiteralValue((InternalSyntaxToken)value.Node).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DateTimeLiteralValueSyntax DateTimeLiteralValue(SyntaxToken value)
	{
		SyntaxKind kind = value.Kind;
		if (kind != SyntaxKind.DateTimeLiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "value");
		}
		return (DateTimeLiteralValueSyntax)InternalSyntaxFactory.DateTimeLiteralValue((InternalSyntaxToken)value.Node).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static InvalidPropertyValueSyntax InvalidPropertyValue(SyntaxToken invalidToken)
	{
		return (InvalidPropertyValueSyntax)InternalSyntaxFactory.InvalidPropertyValue((InternalSyntaxToken)invalidToken.Node).CreateRed();
	}

	public static InvalidPropertyValueSyntax InvalidPropertyValue()
	{
		return InvalidPropertyValue(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DecimalPlacesPropertyValueSyntax DecimalPlacesPropertyValue(DecimalPlacesSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (DecimalPlacesPropertyValueSyntax)InternalSyntaxFactory.DecimalPlacesPropertyValue((value == null) ? null : ((InternalDecimalPlacesSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static OptionValuePropertyValueSyntax OptionValuePropertyValue(IdentifierNameSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (OptionValuePropertyValueSyntax)InternalSyntaxFactory.OptionValuePropertyValue((value == null) ? null : ((InternalIdentifierNameSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ImagePropertyValueSyntax ImagePropertyValue(IdentifierNameSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (ImagePropertyValueSyntax)InternalSyntaxFactory.ImagePropertyValue((value == null) ? null : ((InternalIdentifierNameSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PageFieldReferencePropertyValueSyntax PageFieldReferencePropertyValue(IdentifierNameSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (PageFieldReferencePropertyValueSyntax)InternalSyntaxFactory.PageFieldReferencePropertyValue((value == null) ? null : ((InternalIdentifierNameSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static MultilanguagePropertyValueSyntax MultilanguagePropertyValue(CommaSeparatedIdentifierEqualsStringListSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (MultilanguagePropertyValueSyntax)InternalSyntaxFactory.MultilanguagePropertyValue((value == null) ? null : ((InternalCommaSeparatedIdentifierEqualsStringListSyntax)value.Node)).CreateRed();
	}

	public static MultilanguagePropertyValueSyntax MultilanguagePropertyValue()
	{
		return MultilanguagePropertyValue(CommaSeparatedIdentifierEqualsStringList());
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static LabelPropertyValueSyntax LabelPropertyValue(LabelSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (LabelPropertyValueSyntax)InternalSyntaxFactory.LabelPropertyValue((value == null) ? null : ((InternalLabelSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static MemberReferencePropertyValueSyntax MemberReferencePropertyValue(IdentifierNameSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (MemberReferencePropertyValueSyntax)InternalSyntaxFactory.MemberReferencePropertyValue((value == null) ? null : ((InternalIdentifierNameSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PermissionPropertyValueSyntax PermissionPropertyValue(SeparatedSyntaxList<PermissionSyntax> permissionProperties)
	{
		return (PermissionPropertyValueSyntax)InternalSyntaxFactory.PermissionPropertyValue(permissionProperties.Node.ToInternalSeparatedList<InternalPermissionSyntax>()).CreateRed();
	}

	public static PermissionPropertyValueSyntax PermissionPropertyValue()
	{
		return PermissionPropertyValue(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static QualifiedObjectReferencePropertyValueSyntax QualifiedObjectReferencePropertyValue(SyntaxToken objectType, ObjectNameOrIdSyntax objectNameOrId)
	{
		SyntaxKind kind = objectType.Kind;
		if ((uint)(kind - 77) > 1u && (uint)(kind - 81) > 3u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "objectType");
		}
		if (objectNameOrId == null)
		{
			throw new ArgumentNullException("objectNameOrId");
		}
		return (QualifiedObjectReferencePropertyValueSyntax)InternalSyntaxFactory.QualifiedObjectReferencePropertyValue((InternalSyntaxToken)objectType.Node, (objectNameOrId == null) ? null : ((InternalObjectNameOrIdSyntax)objectNameOrId.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ObjectReferencePropertyValueSyntax ObjectReferencePropertyValue(ObjectNameOrIdSyntax objectNameOrId)
	{
		if (objectNameOrId == null)
		{
			throw new ArgumentNullException("objectNameOrId");
		}
		return (ObjectReferencePropertyValueSyntax)InternalSyntaxFactory.ObjectReferencePropertyValue((objectNameOrId == null) ? null : ((InternalObjectNameOrIdSyntax)objectNameOrId.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PermissionSyntax Permission(SyntaxToken objectType, ObjectNameOrIdSyntax objectReference, SyntaxToken equalsToken, SyntaxToken permissions)
	{
		SyntaxKind kind = objectType.Kind;
		if ((uint)(kind - 77) > 7u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "objectType");
		}
		if (objectReference == null)
		{
			throw new ArgumentNullException("objectReference");
		}
		kind = equalsToken.Kind;
		if (kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		kind = permissions.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "permissions");
		}
		return (PermissionSyntax)InternalSyntaxFactory.Permission((InternalSyntaxToken)objectType.Node, (objectReference == null) ? null : ((InternalObjectNameOrIdSyntax)objectReference.Node), (InternalSyntaxToken)equalsToken.Node, (InternalSyntaxToken)permissions.Node).CreateRed();
	}

	public static PermissionSyntax Permission(SyntaxToken objectType, ObjectNameOrIdSyntax objectReference, SyntaxToken permissions)
	{
		return Permission(objectType, objectReference, Token(SyntaxKind.EqualsToken), permissions);
	}

	public static PermissionSyntax Permission(SyntaxToken objectType, ObjectNameOrIdSyntax objectReference)
	{
		return Permission(objectType, objectReference, Token(SyntaxKind.EqualsToken), default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static EnumPropertyValueSyntax EnumPropertyValue(IdentifierNameSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (EnumPropertyValueSyntax)InternalSyntaxFactory.EnumPropertyValue((value == null) ? null : ((InternalIdentifierNameSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static IdentifierOrLiteralPropertyValueSyntax IdentifierOrLiteralPropertyValue(IdentifierOrLiteralExpressionSyntax identifier)
	{
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		return (IdentifierOrLiteralPropertyValueSyntax)InternalSyntaxFactory.IdentifierOrLiteralPropertyValue((identifier == null) ? null : ((InternalIdentifierOrLiteralExpressionSyntax)identifier.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static IdentifierOrLiteralExpressionSyntax IdentifierOrLiteralExpression(CodeExpressionSyntax identifier)
	{
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		return (IdentifierOrLiteralExpressionSyntax)InternalSyntaxFactory.IdentifierOrLiteralExpression((identifier == null) ? null : ((InternalCodeExpressionSyntax)identifier.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ObjectNameReferenceSyntax ObjectNameReference(CodeExpressionSyntax identifier)
	{
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		return (ObjectNameReferenceSyntax)InternalSyntaxFactory.ObjectNameReference((identifier == null) ? null : ((InternalCodeExpressionSyntax)identifier.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ObjectNameOrIdSyntax ObjectNameOrId(CodeExpressionSyntax identifier)
	{
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		return (ObjectNameOrIdSyntax)InternalSyntaxFactory.ObjectNameOrId((identifier == null) ? null : ((InternalCodeExpressionSyntax)identifier.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ObjectIdSyntax ObjectId(SyntaxToken value)
	{
		SyntaxKind kind = value.Kind;
		if (kind != SyntaxKind.Int32LiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "value");
		}
		return (ObjectIdSyntax)InternalSyntaxFactory.ObjectId((InternalSyntaxToken)value.Node).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DecimalPlacesSyntax DecimalPlaces(SyntaxToken minimumToken, SyntaxToken colonToken, SyntaxToken maximumToken)
	{
		SyntaxKind kind = minimumToken.Kind;
		if (kind != SyntaxKind.Int32LiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "minimumToken");
		}
		kind = colonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.ColonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "colonToken");
		}
		kind = maximumToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.Int32LiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "maximumToken");
		}
		return (DecimalPlacesSyntax)InternalSyntaxFactory.DecimalPlaces((InternalSyntaxToken)minimumToken.Node, (InternalSyntaxToken)colonToken.Node, (InternalSyntaxToken)maximumToken.Node).CreateRed();
	}

	public static DecimalPlacesSyntax DecimalPlaces(SyntaxToken minimumToken, SyntaxToken maximumToken)
	{
		return DecimalPlaces(minimumToken, default, maximumToken);
	}

	public static DecimalPlacesSyntax DecimalPlaces(SyntaxToken minimumToken)
	{
		return DecimalPlaces(minimumToken, default, default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static MemberAccessOrIdentifierPropertyValueSyntax MemberAccessOrIdentifierPropertyValue(CodeExpressionSyntax identifier)
	{
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		return (MemberAccessOrIdentifierPropertyValueSyntax)InternalSyntaxFactory.MemberAccessOrIdentifierPropertyValue((identifier == null) ? null : ((InternalCodeExpressionSyntax)identifier.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PropertyNameSyntax PropertyName(SyntaxToken identifier)
	{
		SyntaxKind kind = identifier.Kind;
		if (kind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "identifier");
		}
		return (PropertyNameSyntax)InternalSyntaxFactory.PropertyName((InternalSyntaxToken)identifier.Node).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static CommaSeparatedPropertyValueSyntax CommaSeparatedPropertyValue(SeparatedSyntaxList<IdentifierNameSyntax> values)
	{
		return (CommaSeparatedPropertyValueSyntax)InternalSyntaxFactory.CommaSeparatedPropertyValue(values.Node.ToInternalSeparatedList<InternalIdentifierNameSyntax>()).CreateRed();
	}

	public static CommaSeparatedPropertyValueSyntax CommaSeparatedPropertyValue()
	{
		return CommaSeparatedPropertyValue(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static CommaSeparatedStringsPropertyValueSyntax CommaSeparatedStringsPropertyValue(SeparatedSyntaxList<StringLiteralValueSyntax> values)
	{
		return (CommaSeparatedStringsPropertyValueSyntax)InternalSyntaxFactory.CommaSeparatedStringsPropertyValue(values.Node.ToInternalSeparatedList<InternalStringLiteralValueSyntax>()).CreateRed();
	}

	public static CommaSeparatedStringsPropertyValueSyntax CommaSeparatedStringsPropertyValue()
	{
		return CommaSeparatedStringsPropertyValue(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static CommaSeparatedIdentifierEqualsStringListSyntax CommaSeparatedIdentifierEqualsStringList(SeparatedSyntaxList<IdentifierEqualsStringSyntax> values)
	{
		return (CommaSeparatedIdentifierEqualsStringListSyntax)InternalSyntaxFactory.CommaSeparatedIdentifierEqualsStringList(values.Node.ToInternalSeparatedList<InternalIdentifierEqualsStringSyntax>()).CreateRed();
	}

	public static CommaSeparatedIdentifierEqualsStringListSyntax CommaSeparatedIdentifierEqualsStringList()
	{
		return CommaSeparatedIdentifierEqualsStringList(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static CommaSeparatedIdentifierEqualsLiteralListSyntax CommaSeparatedIdentifierEqualsLiteralList(SeparatedSyntaxList<IdentifierEqualsLiteralSyntax> values)
	{
		return (CommaSeparatedIdentifierEqualsLiteralListSyntax)InternalSyntaxFactory.CommaSeparatedIdentifierEqualsLiteralList(values.Node.ToInternalSeparatedList<InternalIdentifierEqualsLiteralSyntax>()).CreateRed();
	}

	public static CommaSeparatedIdentifierEqualsLiteralListSyntax CommaSeparatedIdentifierEqualsLiteralList()
	{
		return CommaSeparatedIdentifierEqualsLiteralList(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax CommaSeparatedIdentifierEqualsStringListPropertyValue(CommaSeparatedIdentifierEqualsStringListSyntax list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return (CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax)InternalSyntaxFactory.CommaSeparatedIdentifierEqualsStringListPropertyValue((list == null) ? null : ((InternalCommaSeparatedIdentifierEqualsStringListSyntax)list.Node)).CreateRed();
	}

	public static CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax CommaSeparatedIdentifierEqualsStringListPropertyValue()
	{
		return CommaSeparatedIdentifierEqualsStringListPropertyValue(CommaSeparatedIdentifierEqualsStringList());
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static CommaSeparatedIdentifierOrLiteralPropertyValueSyntax CommaSeparatedIdentifierOrLiteralPropertyValue(SeparatedSyntaxList<IdentifierOrLiteralExpressionSyntax> values)
	{
		return (CommaSeparatedIdentifierOrLiteralPropertyValueSyntax)InternalSyntaxFactory.CommaSeparatedIdentifierOrLiteralPropertyValue(values.Node.ToInternalSeparatedList<InternalIdentifierOrLiteralExpressionSyntax>()).CreateRed();
	}

	public static CommaSeparatedIdentifierOrLiteralPropertyValueSyntax CommaSeparatedIdentifierOrLiteralPropertyValue()
	{
		return CommaSeparatedIdentifierOrLiteralPropertyValue(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static LabelSyntax Label(StringLiteralValueSyntax labelText, SyntaxToken commaToken, CommaSeparatedIdentifierEqualsLiteralListSyntax properties)
	{
		if (labelText == null)
		{
			throw new ArgumentNullException("labelText");
		}
		SyntaxKind kind = commaToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.CommaToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "commaToken");
		}
		return (LabelSyntax)InternalSyntaxFactory.Label((labelText == null) ? null : ((InternalStringLiteralValueSyntax)labelText.Node), (InternalSyntaxToken)commaToken.Node, (properties == null) ? null : ((InternalCommaSeparatedIdentifierEqualsLiteralListSyntax)properties.Node)).CreateRed();
	}

	public static LabelSyntax Label(StringLiteralValueSyntax labelText, CommaSeparatedIdentifierEqualsLiteralListSyntax properties)
	{
		return Label(labelText, default, properties);
	}

	public static LabelSyntax Label(StringLiteralValueSyntax labelText)
	{
		return Label(labelText, default, null);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static OptionValuesPropertyValueSyntax OptionValuesPropertyValue(OptionValuesSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (OptionValuesPropertyValueSyntax)InternalSyntaxFactory.OptionValuesPropertyValue((value == null) ? null : ((InternalOptionValuesSyntax)value.Node)).CreateRed();
	}

	public static OptionValuesPropertyValueSyntax OptionValuesPropertyValue()
	{
		return OptionValuesPropertyValue(OptionValues());
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static BooleanPropertyValueSyntax BooleanPropertyValue(BooleanLiteralValueSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (BooleanPropertyValueSyntax)InternalSyntaxFactory.BooleanPropertyValue((value == null) ? null : ((InternalBooleanLiteralValueSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static Int32PropertyValueSyntax Int32PropertyValue(Int32SignedLiteralValueSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (Int32PropertyValueSyntax)InternalSyntaxFactory.Int32PropertyValue((value == null) ? null : ((InternalInt32SignedLiteralValueSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static Int64PropertyValueSyntax Int64PropertyValue(Int64SignedLiteralValueSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (Int64PropertyValueSyntax)InternalSyntaxFactory.Int64PropertyValue((value == null) ? null : ((InternalInt64SignedLiteralValueSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DecimalPropertyValueSyntax DecimalPropertyValue(DecimalSignedLiteralValueSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (DecimalPropertyValueSyntax)InternalSyntaxFactory.DecimalPropertyValue((value == null) ? null : ((InternalDecimalSignedLiteralValueSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static TimePropertyValueSyntax TimePropertyValue(TimeLiteralValueSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (TimePropertyValueSyntax)InternalSyntaxFactory.TimePropertyValue((value == null) ? null : ((InternalTimeLiteralValueSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DatePropertyValueSyntax DatePropertyValue(DateLiteralValueSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (DatePropertyValueSyntax)InternalSyntaxFactory.DatePropertyValue((value == null) ? null : ((InternalDateLiteralValueSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DateTimePropertyValueSyntax DateTimePropertyValue(DateTimeLiteralValueSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (DateTimePropertyValueSyntax)InternalSyntaxFactory.DateTimePropertyValue((value == null) ? null : ((InternalDateTimeLiteralValueSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static StringPropertyValueSyntax StringPropertyValue(StringLiteralValueSyntax value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return (StringPropertyValueSyntax)InternalSyntaxFactory.StringPropertyValue((value == null) ? null : ((InternalStringLiteralValueSyntax)value.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static StyleExpressionPropertyValueSyntax StyleExpressionPropertyValue(CodeExpressionSyntax expression)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (StyleExpressionPropertyValueSyntax)InternalSyntaxFactory.StyleExpressionPropertyValue((expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static BooleanExpressionPropertyValueSyntax BooleanExpressionPropertyValue(CodeExpressionSyntax expression)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (BooleanExpressionPropertyValueSyntax)InternalSyntaxFactory.BooleanExpressionPropertyValue((expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static TextExpressionPropertyValueSyntax TextExpressionPropertyValue(CodeExpressionSyntax expression)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (TextExpressionPropertyValueSyntax)InternalSyntaxFactory.TextExpressionPropertyValue((expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static IntegerExpressionPropertyValueSyntax IntegerExpressionPropertyValue(CodeExpressionSyntax expression)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (IntegerExpressionPropertyValueSyntax)InternalSyntaxFactory.IntegerExpressionPropertyValue((expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ClientSideBooleanExpressionPropertyValueSyntax ClientSideBooleanExpressionPropertyValue(CodeExpressionSyntax expression)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (ClientSideBooleanExpressionPropertyValueSyntax)InternalSyntaxFactory.ClientSideBooleanExpressionPropertyValue((expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static TableFilterPropertyValueSyntax TableFilterPropertyValue(SeparatedSyntaxList<PropertyExpressionSyntax> conditions)
	{
		return (TableFilterPropertyValueSyntax)InternalSyntaxFactory.TableFilterPropertyValue(conditions.Node.ToInternalSeparatedList<InternalPropertyExpressionSyntax>()).CreateRed();
	}

	public static TableFilterPropertyValueSyntax TableFilterPropertyValue()
	{
		return TableFilterPropertyValue(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static OrderByPropertyValueSyntax OrderByPropertyValue(SeparatedSyntaxList<OrderByExpressionSyntax> order)
	{
		return (OrderByPropertyValueSyntax)InternalSyntaxFactory.OrderByPropertyValue(order.Node.ToInternalSeparatedList<InternalOrderByExpressionSyntax>()).CreateRed();
	}

	public static OrderByPropertyValueSyntax OrderByPropertyValue()
	{
		return OrderByPropertyValue(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static OrderByExpressionSyntax OrderByExpression(SyntaxToken orderSpecifier, SyntaxToken openParenthesisToken, SeparatedSyntaxList<IdentifierNameSyntax> sortingFields, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = orderSpecifier.Kind;
		if ((uint)(kind - 160) > 1u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "orderSpecifier");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (OrderByExpressionSyntax)InternalSyntaxFactory.OrderByExpression((InternalSyntaxToken)orderSpecifier.Node, (InternalSyntaxToken)openParenthesisToken.Node, sortingFields.Node.ToInternalSeparatedList<InternalIdentifierNameSyntax>(), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static OrderByExpressionSyntax OrderByExpression(SyntaxToken orderSpecifier, SeparatedSyntaxList<IdentifierNameSyntax> sortingFields)
	{
		return OrderByExpression(orderSpecifier, Token(SyntaxKind.OpenParenToken), sortingFields, Token(SyntaxKind.CloseParenToken));
	}

	public static OrderByExpressionSyntax OrderByExpression(SyntaxToken orderSpecifier)
	{
		return OrderByExpression(orderSpecifier, Token(SyntaxKind.OpenParenToken), default, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static QueryDataItemLinkPropertyValueSyntax QueryDataItemLinkPropertyValue(SeparatedSyntaxList<QueryDataItemLinkExpressionSyntax> joins)
	{
		return (QueryDataItemLinkPropertyValueSyntax)InternalSyntaxFactory.QueryDataItemLinkPropertyValue(joins.Node.ToInternalSeparatedList<InternalQueryDataItemLinkExpressionSyntax>()).CreateRed();
	}

	public static QueryDataItemLinkPropertyValueSyntax QueryDataItemLinkPropertyValue()
	{
		return QueryDataItemLinkPropertyValue(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static QueryDataItemLinkExpressionSyntax QueryDataItemLinkExpression(IdentifierNameSyntax sourceField, SyntaxToken equalsToken, MemberAccessExpressionSyntax relatedField)
	{
		if (sourceField == null)
		{
			throw new ArgumentNullException("sourceField");
		}
		SyntaxKind kind = equalsToken.Kind;
		if (kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		if (relatedField == null)
		{
			throw new ArgumentNullException("relatedField");
		}
		return (QueryDataItemLinkExpressionSyntax)InternalSyntaxFactory.QueryDataItemLinkExpression((sourceField == null) ? null : ((InternalIdentifierNameSyntax)sourceField.Node), (InternalSyntaxToken)equalsToken.Node, (relatedField == null) ? null : ((InternalMemberAccessExpressionSyntax)relatedField.Node)).CreateRed();
	}

	public static QueryDataItemLinkExpressionSyntax QueryDataItemLinkExpression(IdentifierNameSyntax sourceField, MemberAccessExpressionSyntax relatedField)
	{
		return QueryDataItemLinkExpression(sourceField, Token(SyntaxKind.EqualsToken), relatedField);
	}

	public static QueryDataItemLinkExpressionSyntax QueryDataItemLinkExpression(string sourceField, MemberAccessExpressionSyntax relatedField)
	{
		return QueryDataItemLinkExpression(IdentifierName(sourceField), Token(SyntaxKind.EqualsToken), relatedField);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ReportDataItemLinkPropertyValueSyntax ReportDataItemLinkPropertyValue(SeparatedSyntaxList<ReportDataItemLinkExpressionSyntax> joins)
	{
		return (ReportDataItemLinkPropertyValueSyntax)InternalSyntaxFactory.ReportDataItemLinkPropertyValue(joins.Node.ToInternalSeparatedList<InternalReportDataItemLinkExpressionSyntax>()).CreateRed();
	}

	public static ReportDataItemLinkPropertyValueSyntax ReportDataItemLinkPropertyValue()
	{
		return ReportDataItemLinkPropertyValue(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ReportDataItemLinkExpressionSyntax ReportDataItemLinkExpression(IdentifierNameSyntax sourceField, SyntaxToken equalsToken, SyntaxToken fieldKeywordToken, SyntaxToken openParenthesisToken, IdentifierNameSyntax relatedField, SyntaxToken closeParenthesisToken)
	{
		if (sourceField == null)
		{
			throw new ArgumentNullException("sourceField");
		}
		SyntaxKind kind = equalsToken.Kind;
		if (kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		kind = fieldKeywordToken.Kind;
		if (kind != SyntaxKind.FieldFormulaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "fieldKeywordToken");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (relatedField == null)
		{
			throw new ArgumentNullException("relatedField");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (ReportDataItemLinkExpressionSyntax)InternalSyntaxFactory.ReportDataItemLinkExpression((sourceField == null) ? null : ((InternalIdentifierNameSyntax)sourceField.Node), (InternalSyntaxToken)equalsToken.Node, (InternalSyntaxToken)fieldKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken.Node, (relatedField == null) ? null : ((InternalIdentifierNameSyntax)relatedField.Node), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static ReportDataItemLinkExpressionSyntax ReportDataItemLinkExpression(IdentifierNameSyntax sourceField, IdentifierNameSyntax relatedField)
	{
		return ReportDataItemLinkExpression(sourceField, Token(SyntaxKind.EqualsToken), Token(SyntaxKind.FieldFormulaKeyword), Token(SyntaxKind.OpenParenToken), relatedField, Token(SyntaxKind.CloseParenToken));
	}

	public static ReportDataItemLinkExpressionSyntax ReportDataItemLinkExpression(string sourceField, string relatedField)
	{
		return ReportDataItemLinkExpression(IdentifierName(sourceField), Token(SyntaxKind.EqualsToken), Token(SyntaxKind.FieldFormulaKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(relatedField), Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static TableViewPropertyValueSyntax TableViewPropertyValue(SortingSyntax sorting, OrderSyntax order, WhereExpressionSyntax tableFilter)
	{
		return (TableViewPropertyValueSyntax)InternalSyntaxFactory.TableViewPropertyValue((sorting == null) ? null : ((InternalSortingSyntax)sorting.Node), (order == null) ? null : ((InternalOrderSyntax)order.Node), (tableFilter == null) ? null : ((InternalWhereExpressionSyntax)tableFilter.Node)).CreateRed();
	}

	public static TableViewPropertyValueSyntax TableViewPropertyValue()
	{
		return TableViewPropertyValue(null, null, null);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FiltersPropertyValueSyntax FiltersPropertyValue(WhereExpressionSyntax tableFilter)
	{
		if (tableFilter == null)
		{
			throw new ArgumentNullException("tableFilter");
		}
		return (FiltersPropertyValueSyntax)InternalSyntaxFactory.FiltersPropertyValue((tableFilter == null) ? null : ((InternalWhereExpressionSyntax)tableFilter.Node)).CreateRed();
	}

	public static FiltersPropertyValueSyntax FiltersPropertyValue()
	{
		return FiltersPropertyValue(WhereExpression());
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static SortingSyntax Sorting(SyntaxToken sortingToken, SyntaxToken openParenthesisToken, SeparatedSyntaxList<IdentifierNameSyntax> sortingFields, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = sortingToken.Kind;
		if (kind != SyntaxKind.SortingKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "sortingToken");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (SortingSyntax)InternalSyntaxFactory.Sorting((InternalSyntaxToken)sortingToken.Node, (InternalSyntaxToken)openParenthesisToken.Node, sortingFields.Node.ToInternalSeparatedList<InternalIdentifierNameSyntax>(), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static SortingSyntax Sorting(SeparatedSyntaxList<IdentifierNameSyntax> sortingFields = default(SeparatedSyntaxList<IdentifierNameSyntax>))
	{
		return Sorting(Token(SyntaxKind.SortingKeyword), Token(SyntaxKind.OpenParenToken), sortingFields, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static OrderSyntax Order(SyntaxToken orderToken, SyntaxToken openParenthesisToken, SyntaxToken orderBy, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = orderToken.Kind;
		if (kind != SyntaxKind.OrderKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "orderToken");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = orderBy.Kind;
		if ((uint)(kind - 160) > 1u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "orderBy");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (OrderSyntax)InternalSyntaxFactory.Order((InternalSyntaxToken)orderToken.Node, (InternalSyntaxToken)openParenthesisToken.Node, (InternalSyntaxToken)orderBy.Node, (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static OrderSyntax Order(SyntaxToken orderBy)
	{
		return Order(Token(SyntaxKind.OrderKeyword), Token(SyntaxKind.OpenParenToken), orderBy, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static TableRelationPropertyValueSyntax TableRelationPropertyValue(IfTableRelationSyntax ifExpression, MemberAccessOrIdentifierPropertyValueSyntax relatedTableField, WhereExpressionSyntax tableFilter, ElseTableRelationSyntax elseExpression)
	{
		if (relatedTableField == null)
		{
			throw new ArgumentNullException("relatedTableField");
		}
		return (TableRelationPropertyValueSyntax)InternalSyntaxFactory.TableRelationPropertyValue((ifExpression == null) ? null : ((InternalIfTableRelationSyntax)ifExpression.Node), (relatedTableField == null) ? null : ((InternalMemberAccessOrIdentifierPropertyValueSyntax)relatedTableField.Node), (tableFilter == null) ? null : ((InternalWhereExpressionSyntax)tableFilter.Node), (elseExpression == null) ? null : ((InternalElseTableRelationSyntax)elseExpression.Node)).CreateRed();
	}

	public static TableRelationPropertyValueSyntax TableRelationPropertyValue(MemberAccessOrIdentifierPropertyValueSyntax relatedTableField)
	{
		return TableRelationPropertyValue(null, relatedTableField, null, null);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static IfTableRelationSyntax IfTableRelation(SyntaxToken ifKeywordToken, TableFilterExpressionSyntax ifTableRelationCondition)
	{
		SyntaxKind kind = ifKeywordToken.Kind;
		if (kind != SyntaxKind.IfKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "ifKeywordToken");
		}
		if (ifTableRelationCondition == null)
		{
			throw new ArgumentNullException("ifTableRelationCondition");
		}
		return (IfTableRelationSyntax)InternalSyntaxFactory.IfTableRelation((InternalSyntaxToken)ifKeywordToken.Node, (ifTableRelationCondition == null) ? null : ((InternalTableFilterExpressionSyntax)ifTableRelationCondition.Node)).CreateRed();
	}

	public static IfTableRelationSyntax IfTableRelation(TableFilterExpressionSyntax ifTableRelationCondition = null)
	{
		return IfTableRelation(Token(SyntaxKind.IfKeyword), ifTableRelationCondition ?? TableFilterExpression());
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static TableFilterExpressionSyntax TableFilterExpression(SyntaxToken openParenthesisToken, SeparatedSyntaxList<PropertyExpressionSyntax> conditions, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (TableFilterExpressionSyntax)InternalSyntaxFactory.TableFilterExpression((InternalSyntaxToken)openParenthesisToken.Node, conditions.Node.ToInternalSeparatedList<InternalPropertyExpressionSyntax>(), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static TableFilterExpressionSyntax TableFilterExpression(SeparatedSyntaxList<PropertyExpressionSyntax> conditions = default(SeparatedSyntaxList<PropertyExpressionSyntax>))
	{
		return TableFilterExpression(Token(SyntaxKind.OpenParenToken), conditions, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ElseTableRelationSyntax ElseTableRelation(SyntaxToken elseKeywordToken, TableRelationPropertyValueSyntax elseTableRelationCondition)
	{
		SyntaxKind kind = elseKeywordToken.Kind;
		if (kind != SyntaxKind.ElseKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "elseKeywordToken");
		}
		if (elseTableRelationCondition == null)
		{
			throw new ArgumentNullException("elseTableRelationCondition");
		}
		return (ElseTableRelationSyntax)InternalSyntaxFactory.ElseTableRelation((InternalSyntaxToken)elseKeywordToken.Node, (elseTableRelationCondition == null) ? null : ((InternalTableRelationPropertyValueSyntax)elseTableRelationCondition.Node)).CreateRed();
	}

	public static ElseTableRelationSyntax ElseTableRelation(TableRelationPropertyValueSyntax elseTableRelationCondition)
	{
		return ElseTableRelation(Token(SyntaxKind.ElseKeyword), elseTableRelationCondition);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static WhereExpressionSyntax WhereExpression(SyntaxToken whereKeywordToken, TableFilterExpressionSyntax filter)
	{
		SyntaxKind kind = whereKeywordToken.Kind;
		if (kind != SyntaxKind.WhereFormulaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "whereKeywordToken");
		}
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		return (WhereExpressionSyntax)InternalSyntaxFactory.WhereExpression((InternalSyntaxToken)whereKeywordToken.Node, (filter == null) ? null : ((InternalTableFilterExpressionSyntax)filter.Node)).CreateRed();
	}

	public static WhereExpressionSyntax WhereExpression(TableFilterExpressionSyntax filter = null)
	{
		return WhereExpression(Token(SyntaxKind.WhereFormulaKeyword), filter ?? TableFilterExpression());
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ConstExpressionSyntax ConstExpression(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken constKeywordToken, SyntaxToken openParenthesisToken, IdentifierOrLiteralExpressionSyntax identifier, SyntaxToken closeParenthesisToken)
	{
		if (leftHandSide == null)
		{
			throw new ArgumentNullException("leftHandSide");
		}
		SyntaxKind kind = equalsToken.Kind;
		if (kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		kind = constKeywordToken.Kind;
		if (kind != SyntaxKind.ConstFormulaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "constKeywordToken");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (ConstExpressionSyntax)InternalSyntaxFactory.ConstExpression((leftHandSide == null) ? null : ((InternalIdentifierNameSyntax)leftHandSide.Node), (InternalSyntaxToken)equalsToken.Node, (InternalSyntaxToken)constKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken.Node, (identifier == null) ? null : ((InternalIdentifierOrLiteralExpressionSyntax)identifier.Node), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static ConstExpressionSyntax ConstExpression(IdentifierNameSyntax leftHandSide, IdentifierOrLiteralExpressionSyntax identifier)
	{
		return ConstExpression(leftHandSide, Token(SyntaxKind.EqualsToken), Token(SyntaxKind.ConstFormulaKeyword), Token(SyntaxKind.OpenParenToken), identifier, Token(SyntaxKind.CloseParenToken));
	}

	public static ConstExpressionSyntax ConstExpression(IdentifierNameSyntax leftHandSide)
	{
		return ConstExpression(leftHandSide, Token(SyntaxKind.EqualsToken), Token(SyntaxKind.ConstFormulaKeyword), Token(SyntaxKind.OpenParenToken), null, Token(SyntaxKind.CloseParenToken));
	}

	public static ConstExpressionSyntax ConstExpression(string leftHandSide)
	{
		return ConstExpression(IdentifierName(leftHandSide), Token(SyntaxKind.EqualsToken), Token(SyntaxKind.ConstFormulaKeyword), Token(SyntaxKind.OpenParenToken), null, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static SimpleFieldExpressionSyntax SimpleFieldExpression(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken fieldKeywordToken, SyntaxToken openParenthesisToken, IdentifierNameSyntax identifier, SyntaxToken closeParenthesisToken)
	{
		if (leftHandSide == null)
		{
			throw new ArgumentNullException("leftHandSide");
		}
		SyntaxKind kind = equalsToken.Kind;
		if (kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		kind = fieldKeywordToken.Kind;
		if (kind != SyntaxKind.FieldFormulaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "fieldKeywordToken");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (SimpleFieldExpressionSyntax)InternalSyntaxFactory.SimpleFieldExpression((leftHandSide == null) ? null : ((InternalIdentifierNameSyntax)leftHandSide.Node), (InternalSyntaxToken)equalsToken.Node, (InternalSyntaxToken)fieldKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken.Node, (identifier == null) ? null : ((InternalIdentifierNameSyntax)identifier.Node), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static SimpleFieldExpressionSyntax SimpleFieldExpression(IdentifierNameSyntax leftHandSide, IdentifierNameSyntax identifier)
	{
		return SimpleFieldExpression(leftHandSide, Token(SyntaxKind.EqualsToken), Token(SyntaxKind.FieldFormulaKeyword), Token(SyntaxKind.OpenParenToken), identifier, Token(SyntaxKind.CloseParenToken));
	}

	public static SimpleFieldExpressionSyntax SimpleFieldExpression(string leftHandSide, string identifier)
	{
		return SimpleFieldExpression(IdentifierName(leftHandSide), Token(SyntaxKind.EqualsToken), Token(SyntaxKind.FieldFormulaKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(identifier), Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static InvalidPropertyExpressionSyntax InvalidPropertyExpression(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken constOrFieldOrFilterKeywordToken, SyntaxToken invalidExpression)
	{
		if (leftHandSide == null)
		{
			throw new ArgumentNullException("leftHandSide");
		}
		SyntaxKind kind = equalsToken.Kind;
		if (kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		kind = constOrFieldOrFilterKeywordToken.Kind;
		if (kind != SyntaxKind.None && (uint)(kind - 147) > 2u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "constOrFieldOrFilterKeywordToken");
		}
		return (InvalidPropertyExpressionSyntax)InternalSyntaxFactory.InvalidPropertyExpression((leftHandSide == null) ? null : ((InternalIdentifierNameSyntax)leftHandSide.Node), (InternalSyntaxToken)equalsToken.Node, (InternalSyntaxToken)constOrFieldOrFilterKeywordToken.Node, (InternalSyntaxToken)invalidExpression.Node).CreateRed();
	}

	public static InvalidPropertyExpressionSyntax InvalidPropertyExpression(IdentifierNameSyntax leftHandSide, SyntaxToken constOrFieldOrFilterKeywordToken, SyntaxToken invalidExpression)
	{
		return InvalidPropertyExpression(leftHandSide, Token(SyntaxKind.EqualsToken), constOrFieldOrFilterKeywordToken, invalidExpression);
	}

	public static InvalidPropertyExpressionSyntax InvalidPropertyExpression(IdentifierNameSyntax leftHandSide)
	{
		return InvalidPropertyExpression(leftHandSide, Token(SyntaxKind.EqualsToken), default, default);
	}

	public static InvalidPropertyExpressionSyntax InvalidPropertyExpression(string leftHandSide)
	{
		return InvalidPropertyExpression(IdentifierName(leftHandSide), Token(SyntaxKind.EqualsToken), default, default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static TableCalculationFormulaSyntax TableCalculationFormula(SyntaxKind kind, SyntaxToken sign, SyntaxToken formulaKeywordToken, SyntaxToken openParenthesisToken, IdentifierNameSyntax table, WhereExpressionSyntax whereExpression, SyntaxToken closeParenthesisToken)
	{
		if ((uint)(kind - 355) > 1u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		SyntaxKind kind2 = sign.Kind;
		if (kind2 != SyntaxKind.None && kind2 != SyntaxKind.MinusToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "sign");
		}
		kind2 = formulaKeywordToken.Kind;
		if ((uint)(kind2 - 151) > 1u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "formulaKeywordToken");
		}
		kind2 = openParenthesisToken.Kind;
		if (kind2 != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (table == null)
		{
			throw new ArgumentNullException("table");
		}
		kind2 = closeParenthesisToken.Kind;
		if (kind2 != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (TableCalculationFormulaSyntax)InternalSyntaxFactory.TableCalculationFormula(kind, (InternalSyntaxToken)sign.Node, (InternalSyntaxToken)formulaKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken.Node, (table == null) ? null : ((InternalIdentifierNameSyntax)table.Node), (whereExpression == null) ? null : ((InternalWhereExpressionSyntax)whereExpression.Node), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static TableCalculationFormulaSyntax TableCalculationFormula(SyntaxKind kind, IdentifierNameSyntax table, WhereExpressionSyntax whereExpression)
	{
		return TableCalculationFormula(kind, default, Token(GetTableCalculationFormulaFormulaKeywordTokenKind(kind)), Token(SyntaxKind.OpenParenToken), table, whereExpression, Token(SyntaxKind.CloseParenToken));
	}

	public static TableCalculationFormulaSyntax TableCalculationFormula(SyntaxKind kind, IdentifierNameSyntax table)
	{
		return TableCalculationFormula(kind, default, Token(GetTableCalculationFormulaFormulaKeywordTokenKind(kind)), Token(SyntaxKind.OpenParenToken), table, null, Token(SyntaxKind.CloseParenToken));
	}

	public static TableCalculationFormulaSyntax TableCalculationFormula(SyntaxKind kind, string table)
	{
		return TableCalculationFormula(kind, default, Token(GetTableCalculationFormulaFormulaKeywordTokenKind(kind)), Token(SyntaxKind.OpenParenToken), IdentifierName(table), null, Token(SyntaxKind.CloseParenToken));
	}

	private static SyntaxKind GetTableCalculationFormulaFormulaKeywordTokenKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.ExistCalculationFormulaStatement => SyntaxKind.ExistCalculationFormulaKeyword, 
			SyntaxKind.CountCalculationFormulaStatement => SyntaxKind.CountCalculationFormulaKeyword, 
			_ => throw new ArgumentOutOfRangeException("kind"), 
		};
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FieldCalculationFormulaSyntax FieldCalculationFormula(SyntaxKind kind, SyntaxToken sign, SyntaxToken formulaKeywordToken, SyntaxToken openParenthesisToken, MemberAccessExpressionSyntax field, WhereExpressionSyntax whereExpression, SyntaxToken closeParenthesisToken)
	{
		if ((uint)(kind - 357) > 4u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		SyntaxKind kind2 = sign.Kind;
		if (kind2 != SyntaxKind.None && kind2 != SyntaxKind.MinusToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "sign");
		}
		kind2 = formulaKeywordToken.Kind;
		if ((uint)(kind2 - 153) > 4u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "formulaKeywordToken");
		}
		kind2 = openParenthesisToken.Kind;
		if (kind2 != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		kind2 = closeParenthesisToken.Kind;
		if (kind2 != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (FieldCalculationFormulaSyntax)InternalSyntaxFactory.FieldCalculationFormula(kind, (InternalSyntaxToken)sign.Node, (InternalSyntaxToken)formulaKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken.Node, (field == null) ? null : ((InternalMemberAccessExpressionSyntax)field.Node), (whereExpression == null) ? null : ((InternalWhereExpressionSyntax)whereExpression.Node), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static FieldCalculationFormulaSyntax FieldCalculationFormula(SyntaxKind kind, MemberAccessExpressionSyntax field, WhereExpressionSyntax whereExpression)
	{
		return FieldCalculationFormula(kind, default, Token(GetFieldCalculationFormulaFormulaKeywordTokenKind(kind)), Token(SyntaxKind.OpenParenToken), field, whereExpression, Token(SyntaxKind.CloseParenToken));
	}

	public static FieldCalculationFormulaSyntax FieldCalculationFormula(SyntaxKind kind, MemberAccessExpressionSyntax field)
	{
		return FieldCalculationFormula(kind, default, Token(GetFieldCalculationFormulaFormulaKeywordTokenKind(kind)), Token(SyntaxKind.OpenParenToken), field, null, Token(SyntaxKind.CloseParenToken));
	}

	private static SyntaxKind GetFieldCalculationFormulaFormulaKeywordTokenKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.AverageCalculationFormulaStatement => SyntaxKind.SumCalculationFormulaKeyword, 
			SyntaxKind.SumCalculationFormulaStatement => SyntaxKind.AverageCalculationFormulaKeyword, 
			SyntaxKind.LookupCalculationFormulaStatement => SyntaxKind.MinCalculationFormulaKeyword, 
			SyntaxKind.MinCalculationFormulaStatement => SyntaxKind.MaxCalculationFormulaKeyword, 
			SyntaxKind.MaxCalculationFormulaStatement => SyntaxKind.LookupCalculationFormulaKeyword, 
			_ => throw new ArgumentOutOfRangeException("kind"), 
		};
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FieldFilterExpressionSyntax FieldFilterExpression(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken fieldFormulaKeywordToken, SyntaxToken openParenthesisToken, SyntaxToken filterFormulaKeywordToken, SyntaxToken openParenthesisToken1, IdentifierNameSyntax identifier, SyntaxToken closeParenthesisToken1, SyntaxToken closeParenthesisToken)
	{
		if (leftHandSide == null)
		{
			throw new ArgumentNullException("leftHandSide");
		}
		SyntaxKind kind = equalsToken.Kind;
		if (kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		kind = fieldFormulaKeywordToken.Kind;
		if (kind != SyntaxKind.FieldFormulaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "fieldFormulaKeywordToken");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = filterFormulaKeywordToken.Kind;
		if (kind != SyntaxKind.FilterFormulaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "filterFormulaKeywordToken");
		}
		kind = openParenthesisToken1.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken1");
		}
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		kind = closeParenthesisToken1.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken1");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (FieldFilterExpressionSyntax)InternalSyntaxFactory.FieldFilterExpression((leftHandSide == null) ? null : ((InternalIdentifierNameSyntax)leftHandSide.Node), (InternalSyntaxToken)equalsToken.Node, (InternalSyntaxToken)fieldFormulaKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken.Node, (InternalSyntaxToken)filterFormulaKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken1.Node, (identifier == null) ? null : ((InternalIdentifierNameSyntax)identifier.Node), (InternalSyntaxToken)closeParenthesisToken1.Node, (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static FieldFilterExpressionSyntax FieldFilterExpression(IdentifierNameSyntax leftHandSide, IdentifierNameSyntax identifier)
	{
		return FieldFilterExpression(leftHandSide, Token(SyntaxKind.EqualsToken), Token(SyntaxKind.FieldFormulaKeyword), Token(SyntaxKind.OpenParenToken), Token(SyntaxKind.FilterFormulaKeyword), Token(SyntaxKind.OpenParenToken), identifier, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.CloseParenToken));
	}

	public static FieldFilterExpressionSyntax FieldFilterExpression(string leftHandSide, string identifier)
	{
		return FieldFilterExpression(IdentifierName(leftHandSide), Token(SyntaxKind.EqualsToken), Token(SyntaxKind.FieldFormulaKeyword), Token(SyntaxKind.OpenParenToken), Token(SyntaxKind.FilterFormulaKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(identifier), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FieldUpperLimitExpressionSyntax FieldUpperLimitExpression(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken fieldFormulaKeywordToken, SyntaxToken openParenthesisToken, SyntaxToken upperLimitFormulaKeywordToken, SyntaxToken openParenthesisToken1, IdentifierNameSyntax identifier, SyntaxToken closeParenthesisToken1, SyntaxToken closeParenthesisToken)
	{
		if (leftHandSide == null)
		{
			throw new ArgumentNullException("leftHandSide");
		}
		SyntaxKind kind = equalsToken.Kind;
		if (kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		kind = fieldFormulaKeywordToken.Kind;
		if (kind != SyntaxKind.FieldFormulaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "fieldFormulaKeywordToken");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = upperLimitFormulaKeywordToken.Kind;
		if (kind != SyntaxKind.UpperLimitFormulaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "upperLimitFormulaKeywordToken");
		}
		kind = openParenthesisToken1.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken1");
		}
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		kind = closeParenthesisToken1.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken1");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (FieldUpperLimitExpressionSyntax)InternalSyntaxFactory.FieldUpperLimitExpression((leftHandSide == null) ? null : ((InternalIdentifierNameSyntax)leftHandSide.Node), (InternalSyntaxToken)equalsToken.Node, (InternalSyntaxToken)fieldFormulaKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken.Node, (InternalSyntaxToken)upperLimitFormulaKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken1.Node, (identifier == null) ? null : ((InternalIdentifierNameSyntax)identifier.Node), (InternalSyntaxToken)closeParenthesisToken1.Node, (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static FieldUpperLimitExpressionSyntax FieldUpperLimitExpression(IdentifierNameSyntax leftHandSide, IdentifierNameSyntax identifier)
	{
		return FieldUpperLimitExpression(leftHandSide, Token(SyntaxKind.EqualsToken), Token(SyntaxKind.FieldFormulaKeyword), Token(SyntaxKind.OpenParenToken), Token(SyntaxKind.UpperLimitFormulaKeyword), Token(SyntaxKind.OpenParenToken), identifier, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.CloseParenToken));
	}

	public static FieldUpperLimitExpressionSyntax FieldUpperLimitExpression(string leftHandSide, string identifier)
	{
		return FieldUpperLimitExpression(IdentifierName(leftHandSide), Token(SyntaxKind.EqualsToken), Token(SyntaxKind.FieldFormulaKeyword), Token(SyntaxKind.OpenParenToken), Token(SyntaxKind.UpperLimitFormulaKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(identifier), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FieldUpperLimitFilterExpressionSyntax FieldUpperLimitFilterExpression(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken fieldFormulaKeywordToken, SyntaxToken openParenthesisToken1, SyntaxToken upperLimitFormulaKeywordToken, SyntaxToken openParenthesisToken2, SyntaxToken filterFormulaKeywordToken, SyntaxToken openParenthesisToken3, IdentifierNameSyntax identifier, SyntaxToken closeParenthesisToken3, SyntaxToken closeParenthesisToken2, SyntaxToken closeParenthesisToken1)
	{
		if (leftHandSide == null)
		{
			throw new ArgumentNullException("leftHandSide");
		}
		SyntaxKind kind = equalsToken.Kind;
		if (kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		kind = fieldFormulaKeywordToken.Kind;
		if (kind != SyntaxKind.FieldFormulaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "fieldFormulaKeywordToken");
		}
		kind = openParenthesisToken1.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken1");
		}
		kind = upperLimitFormulaKeywordToken.Kind;
		if (kind != SyntaxKind.UpperLimitFormulaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "upperLimitFormulaKeywordToken");
		}
		kind = openParenthesisToken2.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken2");
		}
		kind = filterFormulaKeywordToken.Kind;
		if (kind != SyntaxKind.FilterFormulaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "filterFormulaKeywordToken");
		}
		kind = openParenthesisToken3.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken3");
		}
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		kind = closeParenthesisToken3.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken3");
		}
		kind = closeParenthesisToken2.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken2");
		}
		kind = closeParenthesisToken1.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken1");
		}
		return (FieldUpperLimitFilterExpressionSyntax)InternalSyntaxFactory.FieldUpperLimitFilterExpression((leftHandSide == null) ? null : ((InternalIdentifierNameSyntax)leftHandSide.Node), (InternalSyntaxToken)equalsToken.Node, (InternalSyntaxToken)fieldFormulaKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken1.Node, (InternalSyntaxToken)upperLimitFormulaKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken2.Node, (InternalSyntaxToken)filterFormulaKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken3.Node, (identifier == null) ? null : ((InternalIdentifierNameSyntax)identifier.Node), (InternalSyntaxToken)closeParenthesisToken3.Node, (InternalSyntaxToken)closeParenthesisToken2.Node, (InternalSyntaxToken)closeParenthesisToken1.Node).CreateRed();
	}

	public static FieldUpperLimitFilterExpressionSyntax FieldUpperLimitFilterExpression(IdentifierNameSyntax leftHandSide, IdentifierNameSyntax identifier)
	{
		return FieldUpperLimitFilterExpression(leftHandSide, Token(SyntaxKind.EqualsToken), Token(SyntaxKind.FieldFormulaKeyword), Token(SyntaxKind.OpenParenToken), Token(SyntaxKind.UpperLimitFormulaKeyword), Token(SyntaxKind.OpenParenToken), Token(SyntaxKind.FilterFormulaKeyword), Token(SyntaxKind.OpenParenToken), identifier, Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.CloseParenToken));
	}

	public static FieldUpperLimitFilterExpressionSyntax FieldUpperLimitFilterExpression(string leftHandSide, string identifier)
	{
		return FieldUpperLimitFilterExpression(IdentifierName(leftHandSide), Token(SyntaxKind.EqualsToken), Token(SyntaxKind.FieldFormulaKeyword), Token(SyntaxKind.OpenParenToken), Token(SyntaxKind.UpperLimitFormulaKeyword), Token(SyntaxKind.OpenParenToken), Token(SyntaxKind.FilterFormulaKeyword), Token(SyntaxKind.OpenParenToken), IdentifierName(identifier), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.CloseParenToken), Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static FilterExpressionSyntax FilterExpression(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken filterFormulaKeywordToken, SyntaxToken openParenthesisToken, FilterExpressionValueSyntax filter, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = equalsToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		kind = filterFormulaKeywordToken.Kind;
		if (kind != SyntaxKind.FilterFormulaKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "filterFormulaKeywordToken");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (FilterExpressionSyntax)InternalSyntaxFactory.FilterExpression((leftHandSide == null) ? null : ((InternalIdentifierNameSyntax)leftHandSide.Node), (InternalSyntaxToken)equalsToken.Node, (InternalSyntaxToken)filterFormulaKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken.Node, (filter == null) ? null : ((InternalFilterExpressionValueSyntax)filter.Node), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static FilterExpressionSyntax FilterExpression(IdentifierNameSyntax leftHandSide, FilterExpressionValueSyntax filter)
	{
		return FilterExpression(leftHandSide, default, Token(SyntaxKind.FilterFormulaKeyword), Token(SyntaxKind.OpenParenToken), filter, Token(SyntaxKind.CloseParenToken));
	}

	public static FilterExpressionSyntax FilterExpression(FilterExpressionValueSyntax filter)
	{
		return FilterExpression(null, default, Token(SyntaxKind.FilterFormulaKeyword), Token(SyntaxKind.OpenParenToken), filter, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ParenthesizedFilterExpressionValueSyntax ParenthesizedFilterExpressionValue(SyntaxToken openParenthesisToken, FilterExpressionValueSyntax expression, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (ParenthesizedFilterExpressionValueSyntax)InternalSyntaxFactory.ParenthesizedFilterExpressionValue((InternalSyntaxToken)openParenthesisToken.Node, (expression == null) ? null : ((InternalFilterExpressionValueSyntax)expression.Node), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static ParenthesizedFilterExpressionValueSyntax ParenthesizedFilterExpressionValue(FilterExpressionValueSyntax expression)
	{
		return ParenthesizedFilterExpressionValue(Token(SyntaxKind.OpenParenToken), expression, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static UnaryFilterExpressionValueSyntax UnaryFilterExpressionValue(SyntaxKind kind, SyntaxToken operatorToken, IdentifierOrLiteralExpressionSyntax identifier)
	{
		if ((uint)(kind - 400) > 5u && kind != SyntaxKind.RangeToFilterExpression)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		SyntaxKind kind2 = operatorToken.Kind;
		if (kind2 != SyntaxKind.None && (uint)(kind2 - 24) > 5u && kind2 != SyntaxKind.DotDotToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "operatorToken");
		}
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		return (UnaryFilterExpressionValueSyntax)InternalSyntaxFactory.UnaryFilterExpressionValue(kind, (InternalSyntaxToken)operatorToken.Node, (identifier == null) ? null : ((InternalIdentifierOrLiteralExpressionSyntax)identifier.Node)).CreateRed();
	}

	public static UnaryFilterExpressionValueSyntax UnaryFilterExpressionValue(SyntaxKind kind, IdentifierOrLiteralExpressionSyntax identifier)
	{
		return UnaryFilterExpressionValue(kind, default, identifier);
	}

	private static SyntaxKind GetUnaryFilterExpressionValueOperatorTokenKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.UnaryNotEqualsFilterExpression => SyntaxKind.NotEqualsToken, 
			SyntaxKind.UnaryEqualsFilterExpression => SyntaxKind.EqualsToken, 
			SyntaxKind.UnaryLessThanFilterExpression => SyntaxKind.LessThanToken, 
			SyntaxKind.UnaryLessThanEqualsFilterExpression => SyntaxKind.LessThanEqualsToken, 
			SyntaxKind.UnaryGreaterThanFilterExpression => SyntaxKind.GreaterThanToken, 
			SyntaxKind.UnaryGreaterThanEqualsFilterExpression => SyntaxKind.GreaterThanEqualsToken, 
			SyntaxKind.RangeToFilterExpression => SyntaxKind.DotDotToken, 
			_ => throw new ArgumentOutOfRangeException("kind"), 
		};
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static BinaryFilterExpressionValueSyntax BinaryFilterExpressionValue(SyntaxKind kind, SyntaxNode left, SyntaxToken operatorToken, SyntaxNode right)
	{
		if ((uint)(kind - 406) > 2u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		SyntaxKind kind2 = operatorToken.Kind;
		if (kind2 != SyntaxKind.DotDotToken && (uint)(kind2 - 139) > 1u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "operatorToken");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		return (BinaryFilterExpressionValueSyntax)InternalSyntaxFactory.BinaryFilterExpressionValue(kind, left?.Node, (InternalSyntaxToken)operatorToken.Node, right?.Node).CreateRed();
	}

	public static BinaryFilterExpressionValueSyntax BinaryFilterExpressionValue(SyntaxKind kind, SyntaxNode left, SyntaxNode right)
	{
		return BinaryFilterExpressionValue(kind, left, Token(GetBinaryFilterExpressionValueOperatorTokenKind(kind)), right);
	}

	private static SyntaxKind GetBinaryFilterExpressionValueOperatorTokenKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.AndFilterExpression => SyntaxKind.AndFilterKeyword, 
			SyntaxKind.OrFilterExpression => SyntaxKind.OrFilterKeyword, 
			SyntaxKind.RangeBetweenFilterExpression => SyntaxKind.DotDotToken, 
			_ => throw new ArgumentOutOfRangeException("kind"), 
		};
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static RangeFromFilterExpressionValueSyntax RangeFromFilterExpressionValue(IdentifierOrLiteralExpressionSyntax left, SyntaxToken operatorToken)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		SyntaxKind kind = operatorToken.Kind;
		if (kind != SyntaxKind.DotDotToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "operatorToken");
		}
		return (RangeFromFilterExpressionValueSyntax)InternalSyntaxFactory.RangeFromFilterExpressionValue((left == null) ? null : ((InternalIdentifierOrLiteralExpressionSyntax)left.Node), (InternalSyntaxToken)operatorToken.Node).CreateRed();
	}

	public static RangeFromFilterExpressionValueSyntax RangeFromFilterExpressionValue(IdentifierOrLiteralExpressionSyntax left)
	{
		return RangeFromFilterExpressionValue(left, Token(SyntaxKind.DotDotToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static InvalidFilterExpressionValueSyntax InvalidFilterExpressionValue(SyntaxToken invalidToken)
	{
		return (InvalidFilterExpressionValueSyntax)InternalSyntaxFactory.InvalidFilterExpressionValue((InternalSyntaxToken)invalidToken.Node).CreateRed();
	}

	public static InvalidFilterExpressionValueSyntax InvalidFilterExpressionValue()
	{
		return InvalidFilterExpressionValue(default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static EmptyPropertySyntax EmptyProperty(SyntaxToken semicolonToken)
	{
		SyntaxKind kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (EmptyPropertySyntax)InternalSyntaxFactory.EmptyProperty((InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static EmptyPropertySyntax EmptyProperty()
	{
		return EmptyProperty(Token(SyntaxKind.SemicolonToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static PropertySyntax Property(PropertyNameSyntax name, SyntaxToken equalsToken, PropertyValueSyntax value, SyntaxToken semicolonToken)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		SyntaxKind kind = equalsToken.Kind;
		if (kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (PropertySyntax)InternalSyntaxFactory.Property((name == null) ? null : ((InternalPropertyNameSyntax)name.Node), (InternalSyntaxToken)equalsToken.Node, (value == null) ? null : ((InternalPropertyValueSyntax)value.Node), (InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static PropertySyntax Property(PropertyNameSyntax name, PropertyValueSyntax value)
	{
		return Property(name, Token(SyntaxKind.EqualsToken), value, Token(SyntaxKind.SemicolonToken));
	}

	public static PropertySyntax Property(PropertyNameSyntax name)
	{
		return Property(name, Token(SyntaxKind.EqualsToken), null, Token(SyntaxKind.SemicolonToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static VarSectionSyntax VarSection(SyntaxToken varKeyword, SyntaxList<VariableDeclarationSyntax> variables)
	{
		SyntaxKind kind = varKeyword.Kind;
		if (kind != SyntaxKind.VarKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "varKeyword");
		}
		return (VarSectionSyntax)InternalSyntaxFactory.VarSection((InternalSyntaxToken)varKeyword.Node, variables.Node.ToInternalList<InternalVariableDeclarationSyntax>()).CreateRed();
	}

	public static VarSectionSyntax VarSection(SyntaxList<VariableDeclarationSyntax> variables = default(SyntaxList<VariableDeclarationSyntax>))
	{
		return VarSection(Token(SyntaxKind.VarKeyword), variables);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static VariableDeclarationSyntax VariableDeclaration(SyntaxList<MemberAttributeSyntax> attributes, IdentifierNameSyntax name, SyntaxToken colonToken, TypeReferenceBaseSyntax type, SyntaxToken semicolonToken)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		SyntaxKind kind = colonToken.Kind;
		if (kind != SyntaxKind.ColonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "colonToken");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (VariableDeclarationSyntax)InternalSyntaxFactory.VariableDeclaration(attributes.Node.ToInternalList<InternalMemberAttributeSyntax>(), (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)colonToken.Node, (type == null) ? null : ((InternalTypeReferenceBaseSyntax)type.Node), (InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static VariableDeclarationSyntax VariableDeclaration(SyntaxList<MemberAttributeSyntax> attributes, IdentifierNameSyntax name, TypeReferenceBaseSyntax type)
	{
		return VariableDeclaration(attributes, name, Token(SyntaxKind.ColonToken), type, Token(SyntaxKind.SemicolonToken));
	}

	public static VariableDeclarationSyntax VariableDeclaration(IdentifierNameSyntax name, TypeReferenceBaseSyntax type)
	{
		return VariableDeclaration(default, name, Token(SyntaxKind.ColonToken), type, Token(SyntaxKind.SemicolonToken));
	}

	public static VariableDeclarationSyntax VariableDeclaration(string name, TypeReferenceBaseSyntax type)
	{
		return VariableDeclaration(default, IdentifierName(name), Token(SyntaxKind.ColonToken), type, Token(SyntaxKind.SemicolonToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ParameterSyntax Parameter(SyntaxToken varKeyword, IdentifierNameSyntax name, SyntaxToken colonToken, TypeReferenceBaseSyntax type)
	{
		SyntaxKind kind = varKeyword.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.VarKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "varKeyword");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		kind = colonToken.Kind;
		if (kind != SyntaxKind.ColonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "colonToken");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return (ParameterSyntax)InternalSyntaxFactory.Parameter((InternalSyntaxToken)varKeyword.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)colonToken.Node, (type == null) ? null : ((InternalTypeReferenceBaseSyntax)type.Node)).CreateRed();
	}

	public static ParameterSyntax Parameter(IdentifierNameSyntax name, TypeReferenceBaseSyntax type)
	{
		return Parameter(default, name, Token(SyntaxKind.ColonToken), type);
	}

	public static ParameterSyntax Parameter(string name, TypeReferenceBaseSyntax type)
	{
		return Parameter(default, IdentifierName(name), Token(SyntaxKind.ColonToken), type);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ReturnValueSyntax ReturnValue(IdentifierNameSyntax name, SyntaxToken colonToken, DataTypeSyntax dataType)
	{
		SyntaxKind kind = colonToken.Kind;
		if (kind != SyntaxKind.ColonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "colonToken");
		}
		if (dataType == null)
		{
			throw new ArgumentNullException("dataType");
		}
		return (ReturnValueSyntax)InternalSyntaxFactory.ReturnValue((name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (InternalSyntaxToken)colonToken.Node, (dataType == null) ? null : ((InternalDataTypeSyntax)dataType.Node)).CreateRed();
	}

	public static ReturnValueSyntax ReturnValue(IdentifierNameSyntax name, DataTypeSyntax dataType)
	{
		return ReturnValue(name, Token(SyntaxKind.ColonToken), dataType);
	}

	public static ReturnValueSyntax ReturnValue(DataTypeSyntax dataType)
	{
		return ReturnValue(null, Token(SyntaxKind.ColonToken), dataType);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static SimpleTypeReferenceSyntax SimpleTypeReference(ArraySyntax array, DataTypeSyntax dataType)
	{
		if (dataType == null)
		{
			throw new ArgumentNullException("dataType");
		}
		return (SimpleTypeReferenceSyntax)InternalSyntaxFactory.SimpleTypeReference((array == null) ? null : ((InternalArraySyntax)array.Node), (dataType == null) ? null : ((InternalDataTypeSyntax)dataType.Node)).CreateRed();
	}

	public static SimpleTypeReferenceSyntax SimpleTypeReference(DataTypeSyntax dataType)
	{
		return SimpleTypeReference(null, dataType);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static RecordTypeReferenceSyntax RecordTypeReference(ArraySyntax array, DataTypeSyntax dataType, SyntaxToken temporary)
	{
		if (dataType == null)
		{
			throw new ArgumentNullException("dataType");
		}
		SyntaxKind kind = temporary.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.TemporaryKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "temporary");
		}
		return (RecordTypeReferenceSyntax)InternalSyntaxFactory.RecordTypeReference((array == null) ? null : ((InternalArraySyntax)array.Node), (dataType == null) ? null : ((InternalDataTypeSyntax)dataType.Node), (InternalSyntaxToken)temporary.Node).CreateRed();
	}

	public static RecordTypeReferenceSyntax RecordTypeReference(ArraySyntax array, DataTypeSyntax dataType)
	{
		return RecordTypeReference(array, dataType, default);
	}

	public static RecordTypeReferenceSyntax RecordTypeReference(DataTypeSyntax dataType)
	{
		return RecordTypeReference(null, dataType, default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DotNetTypeReferenceSyntax DotNetTypeReference(ArraySyntax array, DataTypeSyntax dataType)
	{
		if (dataType == null)
		{
			throw new ArgumentNullException("dataType");
		}
		return (DotNetTypeReferenceSyntax)InternalSyntaxFactory.DotNetTypeReference((array == null) ? null : ((InternalArraySyntax)array.Node), (dataType == null) ? null : ((InternalDataTypeSyntax)dataType.Node)).CreateRed();
	}

	public static DotNetTypeReferenceSyntax DotNetTypeReference(DataTypeSyntax dataType)
	{
		return DotNetTypeReference(null, dataType);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ArraySyntax Array(SyntaxToken arrayKeyword, BracketedDimensionListSyntax dimensionList, SyntaxToken ofKeyword)
	{
		SyntaxKind kind = arrayKeyword.Kind;
		if (kind != SyntaxKind.ArrayKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "arrayKeyword");
		}
		if (dimensionList == null)
		{
			throw new ArgumentNullException("dimensionList");
		}
		kind = ofKeyword.Kind;
		if (kind != SyntaxKind.OfKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "ofKeyword");
		}
		return (ArraySyntax)InternalSyntaxFactory.Array((InternalSyntaxToken)arrayKeyword.Node, (dimensionList == null) ? null : ((InternalBracketedDimensionListSyntax)dimensionList.Node), (InternalSyntaxToken)ofKeyword.Node).CreateRed();
	}

	public static ArraySyntax Array(BracketedDimensionListSyntax dimensionList = null)
	{
		return Array(Token(SyntaxKind.ArrayKeyword), dimensionList ?? BracketedDimensionList(), Token(SyntaxKind.OfKeyword));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static BracketedDimensionListSyntax BracketedDimensionList(SyntaxToken openBracketToken, SeparatedSyntaxList<DimensionSyntax> dimensions, SyntaxToken closeBracketToken)
	{
		SyntaxKind kind = openBracketToken.Kind;
		if (kind != SyntaxKind.OpenBracketToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBracketToken");
		}
		kind = closeBracketToken.Kind;
		if (kind != SyntaxKind.CloseBracketToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBracketToken");
		}
		return (BracketedDimensionListSyntax)InternalSyntaxFactory.BracketedDimensionList((InternalSyntaxToken)openBracketToken.Node, dimensions.Node.ToInternalSeparatedList<InternalDimensionSyntax>(), (InternalSyntaxToken)closeBracketToken.Node).CreateRed();
	}

	public static BracketedDimensionListSyntax BracketedDimensionList(SeparatedSyntaxList<DimensionSyntax> dimensions = default(SeparatedSyntaxList<DimensionSyntax>))
	{
		return BracketedDimensionList(Token(SyntaxKind.OpenBracketToken), dimensions, Token(SyntaxKind.CloseBracketToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DimensionSyntax Dimension(SyntaxToken value)
	{
		SyntaxKind kind = value.Kind;
		if (kind != SyntaxKind.Int32LiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "value");
		}
		return (DimensionSyntax)InternalSyntaxFactory.Dimension((InternalSyntaxToken)value.Node).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static OptionDataTypeSyntax OptionDataType(SyntaxToken typeName, OptionValuesSyntax optionValues)
	{
		return (OptionDataTypeSyntax)InternalSyntaxFactory.OptionDataType((InternalSyntaxToken)typeName.Node, (optionValues == null) ? null : ((InternalOptionValuesSyntax)optionValues.Node)).CreateRed();
	}

	public static OptionDataTypeSyntax OptionDataType(SyntaxToken typeName)
	{
		return OptionDataType(typeName, null);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static SimpleNamedDataTypeSyntax SimpleNamedDataType(SyntaxToken typeName)
	{
		return (SimpleNamedDataTypeSyntax)InternalSyntaxFactory.SimpleNamedDataType((InternalSyntaxToken)typeName.Node).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static GenericNamedDataTypeSyntax GenericNamedDataType(SyntaxToken typeName, SyntaxToken ofKeyword, SyntaxToken openBracketToken, SeparatedSyntaxList<DataTypeSyntax> typeArguments, SyntaxToken closeBracketToken)
	{
		SyntaxKind kind = ofKeyword.Kind;
		if (kind != SyntaxKind.OfKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "ofKeyword");
		}
		kind = openBracketToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.OpenBracketToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBracketToken");
		}
		kind = closeBracketToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.CloseBracketToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBracketToken");
		}
		return (GenericNamedDataTypeSyntax)InternalSyntaxFactory.GenericNamedDataType((InternalSyntaxToken)typeName.Node, (InternalSyntaxToken)ofKeyword.Node, (InternalSyntaxToken)openBracketToken.Node, typeArguments.Node.ToInternalSeparatedList<InternalDataTypeSyntax>(), (InternalSyntaxToken)closeBracketToken.Node).CreateRed();
	}

	public static GenericNamedDataTypeSyntax GenericNamedDataType(SyntaxToken typeName, SeparatedSyntaxList<DataTypeSyntax> typeArguments)
	{
		return GenericNamedDataType(typeName, Token(SyntaxKind.OfKeyword), default, typeArguments, default);
	}

	public static GenericNamedDataTypeSyntax GenericNamedDataType(SyntaxToken typeName)
	{
		return GenericNamedDataType(typeName, Token(SyntaxKind.OfKeyword), default, default, default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static TextConstDataTypeSyntax TextConstDataType(SyntaxToken typeName, CommaSeparatedIdentifierEqualsStringListSyntax multilanguage)
	{
		return (TextConstDataTypeSyntax)InternalSyntaxFactory.TextConstDataType((InternalSyntaxToken)typeName.Node, (multilanguage == null) ? null : ((InternalCommaSeparatedIdentifierEqualsStringListSyntax)multilanguage.Node)).CreateRed();
	}

	public static TextConstDataTypeSyntax TextConstDataType(SyntaxToken typeName)
	{
		return TextConstDataType(typeName, null);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static LabelDataTypeSyntax LabelDataType(SyntaxToken typeName, LabelSyntax label)
	{
		if (label == null)
		{
			throw new ArgumentNullException("label");
		}
		return (LabelDataTypeSyntax)InternalSyntaxFactory.LabelDataType((InternalSyntaxToken)typeName.Node, (label == null) ? null : ((InternalLabelSyntax)label.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static DotNetDataTypeSyntax DotNetDataType(SyntaxToken typeName)
	{
		return (DotNetDataTypeSyntax)InternalSyntaxFactory.DotNetDataType((InternalSyntaxToken)typeName.Node).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static LengthDataTypeSyntax LengthDataType(SyntaxToken typeName, SyntaxToken openBracketToken, SyntaxToken length, SyntaxToken closeBracketToken)
	{
		SyntaxKind kind = openBracketToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.OpenBracketToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBracketToken");
		}
		kind = length.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.Int32LiteralToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "length");
		}
		kind = closeBracketToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.CloseBracketToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBracketToken");
		}
		return (LengthDataTypeSyntax)InternalSyntaxFactory.LengthDataType((InternalSyntaxToken)typeName.Node, (InternalSyntaxToken)openBracketToken.Node, (InternalSyntaxToken)length.Node, (InternalSyntaxToken)closeBracketToken.Node).CreateRed();
	}

	public static LengthDataTypeSyntax LengthDataType(SyntaxToken typeName, SyntaxToken length)
	{
		return LengthDataType(typeName, default, length, default);
	}

	public static LengthDataTypeSyntax LengthDataType(SyntaxToken typeName)
	{
		return LengthDataType(typeName, default, default, default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static SubtypedDataTypeSyntax SubtypedDataType(SyntaxToken typeName, ObjectNameOrIdSyntax subtype)
	{
		if (subtype == null)
		{
			throw new ArgumentNullException("subtype");
		}
		return (SubtypedDataTypeSyntax)InternalSyntaxFactory.SubtypedDataType((InternalSyntaxToken)typeName.Node, (subtype == null) ? null : ((InternalObjectNameOrIdSyntax)subtype.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static EnumDataTypeSyntax EnumDataType(SyntaxToken typeName, IdentifierNameSyntax enumTypeName)
	{
		if (enumTypeName == null)
		{
			throw new ArgumentNullException("enumTypeName");
		}
		return (EnumDataTypeSyntax)InternalSyntaxFactory.EnumDataType((InternalSyntaxToken)typeName.Node, (enumTypeName == null) ? null : ((InternalIdentifierNameSyntax)enumTypeName.Node)).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ParameterListSyntax ParameterList(SyntaxToken openParenthesisToken, SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (ParameterListSyntax)InternalSyntaxFactory.ParameterList((InternalSyntaxToken)openParenthesisToken.Node, parameters.Node.ToInternalSeparatedList<InternalParameterSyntax>(), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static ParameterListSyntax ParameterList(SeparatedSyntaxList<ParameterSyntax> parameters = default(SeparatedSyntaxList<ParameterSyntax>))
	{
		return ParameterList(Token(SyntaxKind.OpenParenToken), parameters, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static TriggerDeclarationSyntax TriggerDeclaration(SyntaxList<MemberAttributeSyntax> attributes, SyntaxToken triggerKeyword, IdentifierNameSyntax name, ParameterListSyntax parameterList, ReturnValueSyntax returnValue, SyntaxToken semicolonToken, VarSectionSyntax variables, BlockSyntax body)
	{
		SyntaxKind kind = triggerKeyword.Kind;
		if (kind != SyntaxKind.TriggerKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "triggerKeyword");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (parameterList == null)
		{
			throw new ArgumentNullException("parameterList");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (body == null)
		{
			throw new ArgumentNullException("body");
		}
		return (TriggerDeclarationSyntax)InternalSyntaxFactory.TriggerDeclaration(attributes.Node.ToInternalList<InternalMemberAttributeSyntax>(), (InternalSyntaxToken)triggerKeyword.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (parameterList == null) ? null : ((InternalParameterListSyntax)parameterList.Node), (returnValue == null) ? null : ((InternalReturnValueSyntax)returnValue.Node), (InternalSyntaxToken)semicolonToken.Node, (variables == null) ? null : ((InternalVarSectionSyntax)variables.Node), (body == null) ? null : ((InternalBlockSyntax)body.Node)).CreateRed();
	}

	public static TriggerDeclarationSyntax TriggerDeclaration(SyntaxList<MemberAttributeSyntax> attributes, IdentifierNameSyntax name, ParameterListSyntax parameterList, ReturnValueSyntax returnValue, VarSectionSyntax variables, BlockSyntax body)
	{
		return TriggerDeclaration(attributes, Token(SyntaxKind.TriggerKeyword), name, parameterList, returnValue, default, variables, body);
	}

	public static TriggerDeclarationSyntax TriggerDeclaration(IdentifierNameSyntax name)
	{
		return TriggerDeclaration(default, Token(SyntaxKind.TriggerKeyword), name, ParameterList(), null, default, null, Block());
	}

	public static TriggerDeclarationSyntax TriggerDeclaration(string name)
	{
		return TriggerDeclaration(default, Token(SyntaxKind.TriggerKeyword), IdentifierName(name), ParameterList(), null, default, null, Block());
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static EventTriggerDeclarationSyntax EventTriggerDeclaration(SyntaxList<MemberAttributeSyntax> attributes, SyntaxToken triggerKeyword, IdentifierNameSyntax publisher, SyntaxToken colonColonToken, IdentifierNameSyntax name, ParameterListSyntax parameterList, ReturnValueSyntax returnValue, SyntaxToken semicolonToken, VarSectionSyntax variables, BlockSyntax body)
	{
		SyntaxKind kind = triggerKeyword.Kind;
		if (kind != SyntaxKind.TriggerKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "triggerKeyword");
		}
		if (publisher == null)
		{
			throw new ArgumentNullException("publisher");
		}
		kind = colonColonToken.Kind;
		if (kind != SyntaxKind.ColonColonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "colonColonToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (parameterList == null)
		{
			throw new ArgumentNullException("parameterList");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		if (body == null)
		{
			throw new ArgumentNullException("body");
		}
		return (EventTriggerDeclarationSyntax)InternalSyntaxFactory.EventTriggerDeclaration(attributes.Node.ToInternalList<InternalMemberAttributeSyntax>(), (InternalSyntaxToken)triggerKeyword.Node, (publisher == null) ? null : ((InternalIdentifierNameSyntax)publisher.Node), (InternalSyntaxToken)colonColonToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (parameterList == null) ? null : ((InternalParameterListSyntax)parameterList.Node), (returnValue == null) ? null : ((InternalReturnValueSyntax)returnValue.Node), (InternalSyntaxToken)semicolonToken.Node, (variables == null) ? null : ((InternalVarSectionSyntax)variables.Node), (body == null) ? null : ((InternalBlockSyntax)body.Node)).CreateRed();
	}

	public static EventTriggerDeclarationSyntax EventTriggerDeclaration(SyntaxList<MemberAttributeSyntax> attributes, IdentifierNameSyntax publisher, IdentifierNameSyntax name, ParameterListSyntax parameterList, ReturnValueSyntax returnValue, VarSectionSyntax variables, BlockSyntax body)
	{
		return EventTriggerDeclaration(attributes, Token(SyntaxKind.TriggerKeyword), publisher, Token(SyntaxKind.ColonColonToken), name, parameterList, returnValue, default, variables, body);
	}

	public static EventTriggerDeclarationSyntax EventTriggerDeclaration(IdentifierNameSyntax publisher, IdentifierNameSyntax name)
	{
		return EventTriggerDeclaration(default, Token(SyntaxKind.TriggerKeyword), publisher, Token(SyntaxKind.ColonColonToken), name, ParameterList(), null, default, null, Block());
	}

	public static EventTriggerDeclarationSyntax EventTriggerDeclaration(string publisher, string name)
	{
		return EventTriggerDeclaration(default, Token(SyntaxKind.TriggerKeyword), IdentifierName(publisher), Token(SyntaxKind.ColonColonToken), IdentifierName(name), ParameterList(), null, default, null, Block());
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static MethodDeclarationSyntax MethodDeclaration(SyntaxList<MemberAttributeSyntax> attributes, SyntaxToken localKeyword, SyntaxToken procedureKeyword, IdentifierNameSyntax name, ParameterListSyntax parameterList, ReturnValueSyntax returnValue, SyntaxToken semicolonToken, VarSectionSyntax variables, BlockSyntax body)
	{
		SyntaxKind kind = localKeyword.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.LocalKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "localKeyword");
		}
		kind = procedureKeyword.Kind;
		if (kind != SyntaxKind.ProcedureKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "procedureKeyword");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (parameterList == null)
		{
			throw new ArgumentNullException("parameterList");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (MethodDeclarationSyntax)InternalSyntaxFactory.MethodDeclaration(attributes.Node.ToInternalList<InternalMemberAttributeSyntax>(), (InternalSyntaxToken)localKeyword.Node, (InternalSyntaxToken)procedureKeyword.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (parameterList == null) ? null : ((InternalParameterListSyntax)parameterList.Node), (returnValue == null) ? null : ((InternalReturnValueSyntax)returnValue.Node), (InternalSyntaxToken)semicolonToken.Node, (variables == null) ? null : ((InternalVarSectionSyntax)variables.Node), (body == null) ? null : ((InternalBlockSyntax)body.Node)).CreateRed();
	}

	public static MethodDeclarationSyntax MethodDeclaration(SyntaxList<MemberAttributeSyntax> attributes, IdentifierNameSyntax name, ParameterListSyntax parameterList, ReturnValueSyntax returnValue, VarSectionSyntax variables, BlockSyntax body)
	{
		return MethodDeclaration(attributes, default, Token(SyntaxKind.ProcedureKeyword), name, parameterList, returnValue, default, variables, body);
	}

	public static MethodDeclarationSyntax MethodDeclaration(IdentifierNameSyntax name)
	{
		return MethodDeclaration(default, default, Token(SyntaxKind.ProcedureKeyword), name, ParameterList(), null, default, null, null);
	}

	public static MethodDeclarationSyntax MethodDeclaration(string name)
	{
		return MethodDeclaration(default, default, Token(SyntaxKind.ProcedureKeyword), IdentifierName(name), ParameterList(), null, default, null, null);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static EventDeclarationSyntax EventDeclaration(SyntaxList<MemberAttributeSyntax> attributes, SyntaxToken eventKeyword, IdentifierNameSyntax name, ParameterListSyntax parameterList, SyntaxToken semicolonToken)
	{
		SyntaxKind kind = eventKeyword.Kind;
		if (kind != SyntaxKind.EventKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "eventKeyword");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (parameterList == null)
		{
			throw new ArgumentNullException("parameterList");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (EventDeclarationSyntax)InternalSyntaxFactory.EventDeclaration(attributes.Node.ToInternalList<InternalMemberAttributeSyntax>(), (InternalSyntaxToken)eventKeyword.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node), (parameterList == null) ? null : ((InternalParameterListSyntax)parameterList.Node), (InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static EventDeclarationSyntax EventDeclaration(SyntaxList<MemberAttributeSyntax> attributes, IdentifierNameSyntax name, ParameterListSyntax parameterList)
	{
		return EventDeclaration(attributes, Token(SyntaxKind.EventKeyword), name, parameterList, default);
	}

	public static EventDeclarationSyntax EventDeclaration(IdentifierNameSyntax name)
	{
		return EventDeclaration(default, Token(SyntaxKind.EventKeyword), name, ParameterList(), default);
	}

	public static EventDeclarationSyntax EventDeclaration(string name)
	{
		return EventDeclaration(default, Token(SyntaxKind.EventKeyword), IdentifierName(name), ParameterList(), default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static BlockSyntax Block(SyntaxToken beginKeywordToken, SyntaxList<StatementSyntax> statements, SyntaxToken endKeywordToken, SyntaxToken semicolonToken)
	{
		SyntaxKind kind = beginKeywordToken.Kind;
		if (kind != SyntaxKind.BeginKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "beginKeywordToken");
		}
		kind = endKeywordToken.Kind;
		if (kind != SyntaxKind.EndKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "endKeywordToken");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (BlockSyntax)InternalSyntaxFactory.Block((InternalSyntaxToken)beginKeywordToken.Node, statements.Node.ToInternalList<InternalStatementSyntax>(), (InternalSyntaxToken)endKeywordToken.Node, (InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static BlockSyntax Block(SyntaxList<StatementSyntax> statements = default(SyntaxList<StatementSyntax>))
	{
		return Block(Token(SyntaxKind.BeginKeyword), statements, Token(SyntaxKind.EndKeyword), default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static IfStatementSyntax IfStatement(SyntaxToken ifKeywordToken, CodeExpressionSyntax condition, SyntaxToken thenKeywordToken, StatementSyntax statement, SyntaxToken elseKeywordToken, StatementSyntax elseStatement)
	{
		SyntaxKind kind = ifKeywordToken.Kind;
		if (kind != SyntaxKind.IfKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "ifKeywordToken");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		kind = thenKeywordToken.Kind;
		if (kind != SyntaxKind.ThenKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "thenKeywordToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		kind = elseKeywordToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.ElseKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "elseKeywordToken");
		}
		return (IfStatementSyntax)InternalSyntaxFactory.IfStatement((InternalSyntaxToken)ifKeywordToken.Node, (condition == null) ? null : ((InternalCodeExpressionSyntax)condition.Node), (InternalSyntaxToken)thenKeywordToken.Node, (statement == null) ? null : ((InternalStatementSyntax)statement.Node), (InternalSyntaxToken)elseKeywordToken.Node, (elseStatement == null) ? null : ((InternalStatementSyntax)elseStatement.Node)).CreateRed();
	}

	public static IfStatementSyntax IfStatement(CodeExpressionSyntax condition, StatementSyntax statement, StatementSyntax elseStatement)
	{
		return IfStatement(Token(SyntaxKind.IfKeyword), condition, Token(SyntaxKind.ThenKeyword), statement, default, elseStatement);
	}

	public static IfStatementSyntax IfStatement(CodeExpressionSyntax condition, StatementSyntax statement)
	{
		return IfStatement(Token(SyntaxKind.IfKeyword), condition, Token(SyntaxKind.ThenKeyword), statement, default, null);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static OrphanedElseStatementSyntax OrphanedElseStatement(SyntaxToken elseKeywordToken, StatementSyntax elseStatement)
	{
		SyntaxKind kind = elseKeywordToken.Kind;
		if (kind != SyntaxKind.ElseKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "elseKeywordToken");
		}
		if (elseStatement == null)
		{
			throw new ArgumentNullException("elseStatement");
		}
		return (OrphanedElseStatementSyntax)InternalSyntaxFactory.OrphanedElseStatement((InternalSyntaxToken)elseKeywordToken.Node, (elseStatement == null) ? null : ((InternalStatementSyntax)elseStatement.Node)).CreateRed();
	}

	public static OrphanedElseStatementSyntax OrphanedElseStatement(StatementSyntax elseStatement)
	{
		return OrphanedElseStatement(Token(SyntaxKind.ElseKeyword), elseStatement);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static AssignmentStatementSyntax AssignmentStatement(CodeExpressionSyntax target, SyntaxToken assignmentToken, CodeExpressionSyntax source, SyntaxToken semicolonToken)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		SyntaxKind kind = assignmentToken.Kind;
		if (kind != SyntaxKind.AssignToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "assignmentToken");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (AssignmentStatementSyntax)InternalSyntaxFactory.AssignmentStatement((target == null) ? null : ((InternalCodeExpressionSyntax)target.Node), (InternalSyntaxToken)assignmentToken.Node, (source == null) ? null : ((InternalCodeExpressionSyntax)source.Node), (InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static AssignmentStatementSyntax AssignmentStatement(CodeExpressionSyntax target, CodeExpressionSyntax source)
	{
		return AssignmentStatement(target, Token(SyntaxKind.AssignToken), source, default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static CompoundAssignmentStatementSyntax CompoundAssignmentStatement(CodeExpressionSyntax target, SyntaxToken assignmentToken, CodeExpressionSyntax source, SyntaxToken semicolonToken)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		SyntaxKind kind = assignmentToken.Kind;
		if ((uint)(kind - 20) > 3u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "assignmentToken");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (CompoundAssignmentStatementSyntax)InternalSyntaxFactory.CompoundAssignmentStatement((target == null) ? null : ((InternalCodeExpressionSyntax)target.Node), (InternalSyntaxToken)assignmentToken.Node, (source == null) ? null : ((InternalCodeExpressionSyntax)source.Node), (InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static CompoundAssignmentStatementSyntax CompoundAssignmentStatement(CodeExpressionSyntax target, SyntaxToken assignmentToken, CodeExpressionSyntax source)
	{
		return CompoundAssignmentStatement(target, assignmentToken, source, default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ForStatementSyntax ForStatement(SyntaxToken forKeywordToken, CodeExpressionSyntax loopVariable, SyntaxToken assignToken, CodeExpressionSyntax initialValue, SyntaxToken operatorKeywordToken, CodeExpressionSyntax endValue, SyntaxToken doKeywordToken, StatementSyntax statement)
	{
		SyntaxKind kind = forKeywordToken.Kind;
		if (kind != SyntaxKind.ForKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "forKeywordToken");
		}
		if (loopVariable == null)
		{
			throw new ArgumentNullException("loopVariable");
		}
		kind = assignToken.Kind;
		if (kind != SyntaxKind.AssignToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "assignToken");
		}
		if (initialValue == null)
		{
			throw new ArgumentNullException("initialValue");
		}
		if (endValue == null)
		{
			throw new ArgumentNullException("endValue");
		}
		kind = doKeywordToken.Kind;
		if (kind != SyntaxKind.DoKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "doKeywordToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (ForStatementSyntax)InternalSyntaxFactory.ForStatement((InternalSyntaxToken)forKeywordToken.Node, (loopVariable == null) ? null : ((InternalCodeExpressionSyntax)loopVariable.Node), (InternalSyntaxToken)assignToken.Node, (initialValue == null) ? null : ((InternalCodeExpressionSyntax)initialValue.Node), (InternalSyntaxToken)operatorKeywordToken.Node, (endValue == null) ? null : ((InternalCodeExpressionSyntax)endValue.Node), (InternalSyntaxToken)doKeywordToken.Node, (statement == null) ? null : ((InternalStatementSyntax)statement.Node)).CreateRed();
	}

	public static ForStatementSyntax ForStatement(CodeExpressionSyntax loopVariable, CodeExpressionSyntax initialValue, SyntaxToken operatorKeywordToken, CodeExpressionSyntax endValue, StatementSyntax statement)
	{
		return ForStatement(Token(SyntaxKind.ForKeyword), loopVariable, Token(SyntaxKind.AssignToken), initialValue, operatorKeywordToken, endValue, Token(SyntaxKind.DoKeyword), statement);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static WhileStatementSyntax WhileStatement(SyntaxToken whileKeywordToken, CodeExpressionSyntax condition, SyntaxToken doKeywordToken, StatementSyntax statement)
	{
		SyntaxKind kind = whileKeywordToken.Kind;
		if (kind != SyntaxKind.WhileKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "whileKeywordToken");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		kind = doKeywordToken.Kind;
		if (kind != SyntaxKind.DoKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "doKeywordToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (WhileStatementSyntax)InternalSyntaxFactory.WhileStatement((InternalSyntaxToken)whileKeywordToken.Node, (condition == null) ? null : ((InternalCodeExpressionSyntax)condition.Node), (InternalSyntaxToken)doKeywordToken.Node, (statement == null) ? null : ((InternalStatementSyntax)statement.Node)).CreateRed();
	}

	public static WhileStatementSyntax WhileStatement(CodeExpressionSyntax condition, StatementSyntax statement)
	{
		return WhileStatement(Token(SyntaxKind.WhileKeyword), condition, Token(SyntaxKind.DoKeyword), statement);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static WithStatementSyntax WithStatement(SyntaxToken withKeywordToken, CodeExpressionSyntax withId, SyntaxToken doKeywordToken, StatementSyntax statement)
	{
		SyntaxKind kind = withKeywordToken.Kind;
		if (kind != SyntaxKind.WithKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "withKeywordToken");
		}
		if (withId == null)
		{
			throw new ArgumentNullException("withId");
		}
		kind = doKeywordToken.Kind;
		if (kind != SyntaxKind.DoKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "doKeywordToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (WithStatementSyntax)InternalSyntaxFactory.WithStatement((InternalSyntaxToken)withKeywordToken.Node, (withId == null) ? null : ((InternalCodeExpressionSyntax)withId.Node), (InternalSyntaxToken)doKeywordToken.Node, (statement == null) ? null : ((InternalStatementSyntax)statement.Node)).CreateRed();
	}

	public static WithStatementSyntax WithStatement(CodeExpressionSyntax withId, StatementSyntax statement)
	{
		return WithStatement(Token(SyntaxKind.WithKeyword), withId, Token(SyntaxKind.DoKeyword), statement);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ExitStatementSyntax ExitStatement(SyntaxToken exitKeywordToken, SyntaxToken openParenthesisToken, CodeExpressionSyntax exitValue, SyntaxToken closeParenthesisToken, SyntaxToken semicolonToken)
	{
		SyntaxKind kind = exitKeywordToken.Kind;
		if (kind != SyntaxKind.ExitKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "exitKeywordToken");
		}
		kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (ExitStatementSyntax)InternalSyntaxFactory.ExitStatement((InternalSyntaxToken)exitKeywordToken.Node, (InternalSyntaxToken)openParenthesisToken.Node, (exitValue == null) ? null : ((InternalCodeExpressionSyntax)exitValue.Node), (InternalSyntaxToken)closeParenthesisToken.Node, (InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static ExitStatementSyntax ExitStatement(CodeExpressionSyntax exitValue = null)
	{
		return ExitStatement(Token(SyntaxKind.ExitKeyword), default, exitValue, default, default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ExpressionStatementSyntax ExpressionStatement(CodeExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		SyntaxKind kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (ExpressionStatementSyntax)InternalSyntaxFactory.ExpressionStatement((expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node), (InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static ExpressionStatementSyntax ExpressionStatement(CodeExpressionSyntax expression)
	{
		return ExpressionStatement(expression, default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static RepeatStatementSyntax RepeatStatement(SyntaxToken repeatKeywordToken, SyntaxList<StatementSyntax> statements, SyntaxToken untilKeywordToken, CodeExpressionSyntax condition, SyntaxToken semicolonToken)
	{
		SyntaxKind kind = repeatKeywordToken.Kind;
		if (kind != SyntaxKind.RepeatKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "repeatKeywordToken");
		}
		kind = untilKeywordToken.Kind;
		if (kind != SyntaxKind.UntilKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "untilKeywordToken");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (RepeatStatementSyntax)InternalSyntaxFactory.RepeatStatement((InternalSyntaxToken)repeatKeywordToken.Node, statements.Node.ToInternalList<InternalStatementSyntax>(), (InternalSyntaxToken)untilKeywordToken.Node, (condition == null) ? null : ((InternalCodeExpressionSyntax)condition.Node), (InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static RepeatStatementSyntax RepeatStatement(SyntaxList<StatementSyntax> statements, CodeExpressionSyntax condition)
	{
		return RepeatStatement(Token(SyntaxKind.RepeatKeyword), statements, Token(SyntaxKind.UntilKeyword), condition, default);
	}

	public static RepeatStatementSyntax RepeatStatement(CodeExpressionSyntax condition)
	{
		return RepeatStatement(Token(SyntaxKind.RepeatKeyword), default, Token(SyntaxKind.UntilKeyword), condition, default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ForEachStatementSyntax ForEachStatement(SyntaxToken forEachKeywordToken, IdentifierNameSyntax iterationVariable, SyntaxToken inKeywordToken, CodeExpressionSyntax expression, SyntaxToken doKeywordToken, StatementSyntax statement)
	{
		SyntaxKind kind = forEachKeywordToken.Kind;
		if (kind != SyntaxKind.ForEachKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "forEachKeywordToken");
		}
		if (iterationVariable == null)
		{
			throw new ArgumentNullException("iterationVariable");
		}
		kind = inKeywordToken.Kind;
		if (kind != SyntaxKind.InKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "inKeywordToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		kind = doKeywordToken.Kind;
		if (kind != SyntaxKind.DoKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "doKeywordToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (ForEachStatementSyntax)InternalSyntaxFactory.ForEachStatement((InternalSyntaxToken)forEachKeywordToken.Node, (iterationVariable == null) ? null : ((InternalIdentifierNameSyntax)iterationVariable.Node), (InternalSyntaxToken)inKeywordToken.Node, (expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node), (InternalSyntaxToken)doKeywordToken.Node, (statement == null) ? null : ((InternalStatementSyntax)statement.Node)).CreateRed();
	}

	public static ForEachStatementSyntax ForEachStatement(IdentifierNameSyntax iterationVariable, CodeExpressionSyntax expression, StatementSyntax statement)
	{
		return ForEachStatement(Token(SyntaxKind.ForEachKeyword), iterationVariable, Token(SyntaxKind.InKeyword), expression, Token(SyntaxKind.DoKeyword), statement);
	}

	public static ForEachStatementSyntax ForEachStatement(string iterationVariable, CodeExpressionSyntax expression, StatementSyntax statement)
	{
		return ForEachStatement(Token(SyntaxKind.ForEachKeyword), IdentifierName(iterationVariable), Token(SyntaxKind.InKeyword), expression, Token(SyntaxKind.DoKeyword), statement);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static BreakStatementSyntax BreakStatement(SyntaxToken breakKeyword, SyntaxToken semicolonToken)
	{
		SyntaxKind kind = breakKeyword.Kind;
		if (kind != SyntaxKind.BreakKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "breakKeyword");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (BreakStatementSyntax)InternalSyntaxFactory.BreakStatement((InternalSyntaxToken)breakKeyword.Node, (InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static BreakStatementSyntax BreakStatement()
	{
		return BreakStatement(Token(SyntaxKind.BreakKeyword), default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static AssertErrorStatementSyntax AssertErrorStatement(SyntaxToken assertErrorKeywordToken, StatementSyntax statement)
	{
		SyntaxKind kind = assertErrorKeywordToken.Kind;
		if (kind != SyntaxKind.AssertErrorKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "assertErrorKeywordToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (AssertErrorStatementSyntax)InternalSyntaxFactory.AssertErrorStatement((InternalSyntaxToken)assertErrorKeywordToken.Node, (statement == null) ? null : ((InternalStatementSyntax)statement.Node)).CreateRed();
	}

	public static AssertErrorStatementSyntax AssertErrorStatement(StatementSyntax statement)
	{
		return AssertErrorStatement(Token(SyntaxKind.AssertErrorKeyword), statement);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static CaseStatementSyntax CaseStatement(SyntaxToken caseKeywordToken, CodeExpressionSyntax expression, SyntaxToken ofKeywordToken, SyntaxList<CaseLineSyntax> caseLines, CaseElseSyntax caseElse, SyntaxToken endKeywordToken, SyntaxToken semicolonToken)
	{
		SyntaxKind kind = caseKeywordToken.Kind;
		if (kind != SyntaxKind.CaseKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "caseKeywordToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		kind = ofKeywordToken.Kind;
		if (kind != SyntaxKind.OfKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "ofKeywordToken");
		}
		kind = endKeywordToken.Kind;
		if (kind != SyntaxKind.EndKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "endKeywordToken");
		}
		kind = semicolonToken.Kind;
		if (kind != SyntaxKind.None && kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (CaseStatementSyntax)InternalSyntaxFactory.CaseStatement((InternalSyntaxToken)caseKeywordToken.Node, (expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node), (InternalSyntaxToken)ofKeywordToken.Node, caseLines.Node.ToInternalList<InternalCaseLineSyntax>(), (caseElse == null) ? null : ((InternalCaseElseSyntax)caseElse.Node), (InternalSyntaxToken)endKeywordToken.Node, (InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static CaseStatementSyntax CaseStatement(CodeExpressionSyntax expression, SyntaxList<CaseLineSyntax> caseLines, CaseElseSyntax caseElse)
	{
		return CaseStatement(Token(SyntaxKind.CaseKeyword), expression, Token(SyntaxKind.OfKeyword), caseLines, caseElse, Token(SyntaxKind.EndKeyword), default);
	}

	public static CaseStatementSyntax CaseStatement(CodeExpressionSyntax expression)
	{
		return CaseStatement(Token(SyntaxKind.CaseKeyword), expression, Token(SyntaxKind.OfKeyword), default, null, Token(SyntaxKind.EndKeyword), default);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static CaseLineSyntax CaseLine(SeparatedSyntaxList<CodeExpressionSyntax> expressions, SyntaxToken colonToken, StatementSyntax statement)
	{
		SyntaxKind kind = colonToken.Kind;
		if (kind != SyntaxKind.ColonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "colonToken");
		}
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		return (CaseLineSyntax)InternalSyntaxFactory.CaseLine(expressions.Node.ToInternalSeparatedList<InternalCodeExpressionSyntax>(), (InternalSyntaxToken)colonToken.Node, (statement == null) ? null : ((InternalStatementSyntax)statement.Node)).CreateRed();
	}

	public static CaseLineSyntax CaseLine(SeparatedSyntaxList<CodeExpressionSyntax> expressions, StatementSyntax statement)
	{
		return CaseLine(expressions, Token(SyntaxKind.ColonToken), statement);
	}

	public static CaseLineSyntax CaseLine(StatementSyntax statement)
	{
		return CaseLine(default, Token(SyntaxKind.ColonToken), statement);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static CaseElseSyntax CaseElse(SyntaxToken elseKeywordToken, SyntaxList<StatementSyntax> elseStatements)
	{
		SyntaxKind kind = elseKeywordToken.Kind;
		if (kind != SyntaxKind.ElseKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "elseKeywordToken");
		}
		return (CaseElseSyntax)InternalSyntaxFactory.CaseElse((InternalSyntaxToken)elseKeywordToken.Node, elseStatements.Node.ToInternalList<InternalStatementSyntax>()).CreateRed();
	}

	public static CaseElseSyntax CaseElse(SyntaxList<StatementSyntax> elseStatements = default(SyntaxList<StatementSyntax>))
	{
		return CaseElse(Token(SyntaxKind.ElseKeyword), elseStatements);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static EmptyStatementSyntax EmptyStatement(SyntaxToken semicolonToken)
	{
		SyntaxKind kind = semicolonToken.Kind;
		if (kind != SyntaxKind.SemicolonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "semicolonToken");
		}
		return (EmptyStatementSyntax)InternalSyntaxFactory.EmptyStatement((InternalSyntaxToken)semicolonToken.Node).CreateRed();
	}

	public static EmptyStatementSyntax EmptyStatement()
	{
		return EmptyStatement(Token(SyntaxKind.SemicolonToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ParenthesizedExpressionSyntax ParenthesizedExpression(SyntaxToken openParenthesisToken, CodeExpressionSyntax expression, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (ParenthesizedExpressionSyntax)InternalSyntaxFactory.ParenthesizedExpression((InternalSyntaxToken)openParenthesisToken.Node, (expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static ParenthesizedExpressionSyntax ParenthesizedExpression(CodeExpressionSyntax expression)
	{
		return ParenthesizedExpression(Token(SyntaxKind.OpenParenToken), expression, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ElementAccessExpressionSyntax ElementAccessExpression(CodeExpressionSyntax expression, BracketedArgumentListSyntax argumentList)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		if (argumentList == null)
		{
			throw new ArgumentNullException("argumentList");
		}
		return (ElementAccessExpressionSyntax)InternalSyntaxFactory.ElementAccessExpression((expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node), (argumentList == null) ? null : ((InternalBracketedArgumentListSyntax)argumentList.Node)).CreateRed();
	}

	public static ElementAccessExpressionSyntax ElementAccessExpression(CodeExpressionSyntax expression)
	{
		return ElementAccessExpression(expression, BracketedArgumentList());
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static BracketedArgumentListSyntax BracketedArgumentList(SyntaxToken openBracketToken, SeparatedSyntaxList<CodeExpressionSyntax> arguments, SyntaxToken closeBracketToken)
	{
		SyntaxKind kind = openBracketToken.Kind;
		if (kind != SyntaxKind.OpenBracketToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openBracketToken");
		}
		kind = closeBracketToken.Kind;
		if (kind != SyntaxKind.CloseBracketToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeBracketToken");
		}
		return (BracketedArgumentListSyntax)InternalSyntaxFactory.BracketedArgumentList((InternalSyntaxToken)openBracketToken.Node, arguments.Node.ToInternalSeparatedList<InternalCodeExpressionSyntax>(), (InternalSyntaxToken)closeBracketToken.Node).CreateRed();
	}

	public static BracketedArgumentListSyntax BracketedArgumentList(SeparatedSyntaxList<CodeExpressionSyntax> arguments = default(SeparatedSyntaxList<CodeExpressionSyntax>))
	{
		return BracketedArgumentList(Token(SyntaxKind.OpenBracketToken), arguments, Token(SyntaxKind.CloseBracketToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static InvocationExpressionSyntax InvocationExpression(CodeExpressionSyntax expression, ArgumentListSyntax argumentList)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		if (argumentList == null)
		{
			throw new ArgumentNullException("argumentList");
		}
		return (InvocationExpressionSyntax)InternalSyntaxFactory.InvocationExpression((expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node), (argumentList == null) ? null : ((InternalArgumentListSyntax)argumentList.Node)).CreateRed();
	}

	public static InvocationExpressionSyntax InvocationExpression(CodeExpressionSyntax expression)
	{
		return InvocationExpression(expression, ArgumentList());
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static ArgumentListSyntax ArgumentList(SyntaxToken openParenthesisToken, SeparatedSyntaxList<CodeExpressionSyntax> arguments, SyntaxToken closeParenthesisToken)
	{
		SyntaxKind kind = openParenthesisToken.Kind;
		if (kind != SyntaxKind.OpenParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "openParenthesisToken");
		}
		kind = closeParenthesisToken.Kind;
		if (kind != SyntaxKind.CloseParenToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "closeParenthesisToken");
		}
		return (ArgumentListSyntax)InternalSyntaxFactory.ArgumentList((InternalSyntaxToken)openParenthesisToken.Node, arguments.Node.ToInternalSeparatedList<InternalCodeExpressionSyntax>(), (InternalSyntaxToken)closeParenthesisToken.Node).CreateRed();
	}

	public static ArgumentListSyntax ArgumentList(SeparatedSyntaxList<CodeExpressionSyntax> arguments = default(SeparatedSyntaxList<CodeExpressionSyntax>))
	{
		return ArgumentList(Token(SyntaxKind.OpenParenToken), arguments, Token(SyntaxKind.CloseParenToken));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static UnaryExpressionSyntax UnaryExpression(SyntaxKind kind, SyntaxToken operatorToken, CodeExpressionSyntax expression)
	{
		if ((uint)(kind - 210) > 2u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		SyntaxKind kind2 = operatorToken.Kind;
		if ((uint)(kind2 - 14) > 1u && kind2 != SyntaxKind.NotKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "operatorToken");
		}
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		return (UnaryExpressionSyntax)InternalSyntaxFactory.UnaryExpression(kind, (InternalSyntaxToken)operatorToken.Node, (expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node)).CreateRed();
	}

	public static UnaryExpressionSyntax UnaryExpression(SyntaxKind kind, CodeExpressionSyntax expression)
	{
		return UnaryExpression(kind, Token(GetUnaryExpressionOperatorTokenKind(kind)), expression);
	}

	private static SyntaxKind GetUnaryExpressionOperatorTokenKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.UnaryPlusExpression => SyntaxKind.PlusToken, 
			SyntaxKind.UnaryMinusExpression => SyntaxKind.MinusToken, 
			SyntaxKind.UnaryNotExpression => SyntaxKind.NotKeyword, 
			_ => throw new ArgumentOutOfRangeException("kind"), 
		};
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static BinaryExpressionSyntax BinaryExpression(SyntaxKind kind, CodeExpressionSyntax left, SyntaxToken operatorToken, CodeExpressionSyntax right)
	{
		if ((uint)(kind - 194) > 15u)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "kind");
		}
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		switch (operatorToken.Kind)
		{
		default:
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "operatorToken");
		case SyntaxKind.RDivToken:
		case SyntaxKind.PlusToken:
		case SyntaxKind.MinusToken:
		case SyntaxKind.MultiplyToken:
		case SyntaxKind.IDivToken:
		case SyntaxKind.ModuloToken:
		case SyntaxKind.LessThanToken:
		case SyntaxKind.LessThanEqualsToken:
		case SyntaxKind.NotEqualsToken:
		case SyntaxKind.EqualsToken:
		case SyntaxKind.GreaterThanToken:
		case SyntaxKind.GreaterThanEqualsToken:
		case SyntaxKind.DotDotToken:
		case SyntaxKind.AndKeyword:
		case SyntaxKind.OrKeyword:
		case SyntaxKind.XorKeyword:
			if (right == null)
			{
				throw new ArgumentNullException("right");
			}
			return (BinaryExpressionSyntax)InternalSyntaxFactory.BinaryExpression(kind, (left == null) ? null : ((InternalCodeExpressionSyntax)left.Node), (InternalSyntaxToken)operatorToken.Node, (right == null) ? null : ((InternalCodeExpressionSyntax)right.Node)).CreateRed();
		}
	}

	public static BinaryExpressionSyntax BinaryExpression(SyntaxKind kind, CodeExpressionSyntax left, CodeExpressionSyntax right)
	{
		return BinaryExpression(kind, left, Token(GetBinaryExpressionOperatorTokenKind(kind)), right);
	}

	private static SyntaxKind GetBinaryExpressionOperatorTokenKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.AddExpression => SyntaxKind.PlusToken, 
			SyntaxKind.SubtractExpression => SyntaxKind.MinusToken, 
			SyntaxKind.MultiplyExpression => SyntaxKind.MultiplyToken, 
			SyntaxKind.DivideExpression => SyntaxKind.RDivToken, 
			SyntaxKind.IntegerDivideExpression => SyntaxKind.IDivToken, 
			SyntaxKind.ModuloExpression => SyntaxKind.ModuloToken, 
			SyntaxKind.EqualsExpression => SyntaxKind.EqualsToken, 
			SyntaxKind.NotEqualsExpression => SyntaxKind.NotEqualsToken, 
			SyntaxKind.GreaterThanExpression => SyntaxKind.GreaterThanToken, 
			SyntaxKind.GreaterThanOrEqualExpression => SyntaxKind.GreaterThanEqualsToken, 
			SyntaxKind.LessThanExpression => SyntaxKind.LessThanToken, 
			SyntaxKind.LessThanOrEqualExpression => SyntaxKind.LessThanEqualsToken, 
			SyntaxKind.LogicalOrExpression => SyntaxKind.OrKeyword, 
			SyntaxKind.LogicalAndExpression => SyntaxKind.AndKeyword, 
			SyntaxKind.LogicalXorExpression => SyntaxKind.XorKeyword, 
			SyntaxKind.RangeExpression => SyntaxKind.DotDotToken, 
			_ => throw new ArgumentOutOfRangeException("kind"), 
		};
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static InListExpressionSyntax InListExpression(CodeExpressionSyntax expression, SyntaxToken inKeywordToken, BracketedArgumentListSyntax listExpression)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		SyntaxKind kind = inKeywordToken.Kind;
		if (kind != SyntaxKind.InKeyword)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "inKeywordToken");
		}
		if (listExpression == null)
		{
			throw new ArgumentNullException("listExpression");
		}
		return (InListExpressionSyntax)InternalSyntaxFactory.InListExpression((expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node), (InternalSyntaxToken)inKeywordToken.Node, (listExpression == null) ? null : ((InternalBracketedArgumentListSyntax)listExpression.Node)).CreateRed();
	}

	public static InListExpressionSyntax InListExpression(CodeExpressionSyntax expression, BracketedArgumentListSyntax listExpression)
	{
		return InListExpression(expression, Token(SyntaxKind.InKeyword), listExpression);
	}

	public static InListExpressionSyntax InListExpression(CodeExpressionSyntax expression)
	{
		return InListExpression(expression, Token(SyntaxKind.InKeyword), BracketedArgumentList());
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static MemberAccessExpressionSyntax MemberAccessExpression(CodeExpressionSyntax expression, SyntaxToken dotToken, IdentifierNameSyntax name)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		SyntaxKind kind = dotToken.Kind;
		if (kind != SyntaxKind.DotToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "dotToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return (MemberAccessExpressionSyntax)InternalSyntaxFactory.MemberAccessExpression((expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node), (InternalSyntaxToken)dotToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node)).CreateRed();
	}

	public static MemberAccessExpressionSyntax MemberAccessExpression(CodeExpressionSyntax expression, IdentifierNameSyntax name)
	{
		return MemberAccessExpression(expression, Token(SyntaxKind.DotToken), name);
	}

	public static MemberAccessExpressionSyntax MemberAccessExpression(CodeExpressionSyntax expression, string name)
	{
		return MemberAccessExpression(expression, Token(SyntaxKind.DotToken), IdentifierName(name));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static OptionAccessExpressionSyntax OptionAccessExpression(CodeExpressionSyntax expression, SyntaxToken colonColonToken, IdentifierNameSyntax name)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		SyntaxKind kind = colonColonToken.Kind;
		if (kind != SyntaxKind.ColonColonToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "colonColonToken");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return (OptionAccessExpressionSyntax)InternalSyntaxFactory.OptionAccessExpression((expression == null) ? null : ((InternalCodeExpressionSyntax)expression.Node), (InternalSyntaxToken)colonColonToken.Node, (name == null) ? null : ((InternalIdentifierNameSyntax)name.Node)).CreateRed();
	}

	public static OptionAccessExpressionSyntax OptionAccessExpression(CodeExpressionSyntax expression, IdentifierNameSyntax name)
	{
		return OptionAccessExpression(expression, Token(SyntaxKind.ColonColonToken), name);
	}

	public static OptionAccessExpressionSyntax OptionAccessExpression(CodeExpressionSyntax expression, string name)
	{
		return OptionAccessExpression(expression, Token(SyntaxKind.ColonColonToken), IdentifierName(name));
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static IdentifierNameSyntax IdentifierName(SyntaxToken identifier)
	{
		SyntaxKind kind = identifier.Kind;
		if (kind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "identifier");
		}
		return (IdentifierNameSyntax)InternalSyntaxFactory.IdentifierName((InternalSyntaxToken)identifier.Node).CreateRed();
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static QualifiedNameSyntax QualifiedName(NameSyntax left, SyntaxToken dotToken, SimpleNameSyntax right)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		SyntaxKind kind = dotToken.Kind;
		if (kind != SyntaxKind.DotToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "dotToken");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		return (QualifiedNameSyntax)InternalSyntaxFactory.QualifiedName((left == null) ? null : ((InternalNameSyntax)left.Node), (InternalSyntaxToken)dotToken.Node, (right == null) ? null : ((InternalSimpleNameSyntax)right.Node)).CreateRed();
	}

	public static QualifiedNameSyntax QualifiedName(NameSyntax left, SimpleNameSyntax right)
	{
		return QualifiedName(left, Token(SyntaxKind.DotToken), right);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static IdentifierEqualsStringSyntax IdentifierEqualsString(SyntaxToken identifier, SyntaxToken equalsToken, StringLiteralValueSyntax stringLiteral)
	{
		SyntaxKind kind = identifier.Kind;
		if (kind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "identifier");
		}
		kind = equalsToken.Kind;
		if (kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		if (stringLiteral == null)
		{
			throw new ArgumentNullException("stringLiteral");
		}
		return (IdentifierEqualsStringSyntax)InternalSyntaxFactory.IdentifierEqualsString((InternalSyntaxToken)identifier.Node, (InternalSyntaxToken)equalsToken.Node, (stringLiteral == null) ? null : ((InternalStringLiteralValueSyntax)stringLiteral.Node)).CreateRed();
	}

	public static IdentifierEqualsStringSyntax IdentifierEqualsString(SyntaxToken identifier, StringLiteralValueSyntax stringLiteral)
	{
		return IdentifierEqualsString(identifier, Token(SyntaxKind.EqualsToken), stringLiteral);
	}

	public static IdentifierEqualsStringSyntax IdentifierEqualsString(string identifier, StringLiteralValueSyntax stringLiteral)
	{
		return IdentifierEqualsString(Identifier(identifier), Token(SyntaxKind.EqualsToken), stringLiteral);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static IdentifierEqualsLiteralSyntax IdentifierEqualsLiteral(SyntaxToken identifier, SyntaxToken equalsToken, LiteralValueSyntax literal)
	{
		SyntaxKind kind = identifier.Kind;
		if (kind != SyntaxKind.IdentifierToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "identifier");
		}
		kind = equalsToken.Kind;
		if (kind != SyntaxKind.EqualsToken)
		{
			throw new ArgumentException(CodeAnalysisResources.WrongSyntaxKind, "equalsToken");
		}
		if (literal == null)
		{
			throw new ArgumentNullException("literal");
		}
		return (IdentifierEqualsLiteralSyntax)InternalSyntaxFactory.IdentifierEqualsLiteral((InternalSyntaxToken)identifier.Node, (InternalSyntaxToken)equalsToken.Node, (literal == null) ? null : ((InternalLiteralValueSyntax)literal.Node)).CreateRed();
	}

	public static IdentifierEqualsLiteralSyntax IdentifierEqualsLiteral(SyntaxToken identifier, LiteralValueSyntax literal)
	{
		return IdentifierEqualsLiteral(identifier, Token(SyntaxKind.EqualsToken), literal);
	}

	public static IdentifierEqualsLiteralSyntax IdentifierEqualsLiteral(string identifier, LiteralValueSyntax literal)
	{
		return IdentifierEqualsLiteral(Identifier(identifier), Token(SyntaxKind.EqualsToken), literal);
	}

	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static SkippedTokensTriviaSyntax SkippedTokensTrivia(SyntaxNode tokens)
	{
		if (tokens == null)
		{
			throw new ArgumentNullException("tokens");
		}
		return (SkippedTokensTriviaSyntax)InternalSyntaxFactory.SkippedTokensTrivia(tokens?.Node).CreateRed();
	}

	public static PageSyntax Page(int id, string name)
	{
		return Page(ObjectId(id), name);
	}

	public static PageFieldSyntax PageField(string name, string expression)
	{
		return PageField(IdentifierName(name), ParseExpression(expression));
	}

	public static PageExtensionSyntax PageExtension(int objectId, string objectName, string extendsPageName)
	{
		return PageExtension(ObjectId(objectId), objectName, ObjectNameOrId(IdentifierName(extendsPageName)));
	}

	public static ControlAddChangeSyntax ControlAddLastChange(string anchorName, params ControlBaseSyntax[] actions)
	{
		return ControlAddChange(AddLastKeyword, IdentifierName(Identifier(anchorName)), default(SyntaxList<ControlBaseSyntax>).AddRange(actions));
	}

	public static ActionAddChangeSyntax ActionAddLastChange(string anchorName, params ActionBaseSyntax[] actions)
	{
		return ActionAddChange(AddLastKeyword, IdentifierName(Identifier(anchorName)), default(SyntaxList<ActionBaseSyntax>).AddRange(actions));
	}

	public static ControlMoveChangeSyntax ControlMoveAfterChange(string anchorName, string controlName)
	{
		return ControlMoveChange(Token(SyntaxKind.MoveAfterKeyword), IdentifierName(anchorName), default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(IdentifierName(controlName)));
	}

	public static ActionMoveChangeSyntax ActionMoveAfterChange(string anchorName, string actionName)
	{
		return ActionMoveChange(Token(SyntaxKind.MoveAfterKeyword), IdentifierName(anchorName), default(SeparatedSyntaxList<IdentifierNameSyntax>).Add(IdentifierName(actionName)));
	}

	public static PropertySyntax PropertyLiteral(PropertyKind propertyKind, bool propertyValue)
	{
		return Property(propertyKind, propertyValue ? BooleanPropertyValue(BooleanLiteralValue(Token(SyntaxKind.TrueKeyword))) : BooleanPropertyValue(BooleanLiteralValue(Token(SyntaxKind.FalseKeyword))));
	}

	public static PropertySyntax PropertyLiteral(PropertyKind propertyKind, int propertyValue)
	{
		return Property(propertyKind, Int32PropertyValue((propertyValue < 0) ? Int32SignedLiteralValue(Token(SyntaxKind.MinusToken), Literal(Math.Abs(propertyValue))) : Int32SignedLiteralValue(Literal(propertyValue))));
	}

	public static PropertySyntax PropertyLiteral(PropertyKind propertyKind, string propertyValue)
	{
		return Property(propertyKind, StringPropertyValue(StringLiteralValue(Literal(propertyValue))));
	}

	public static PropertySyntax PropertyObjectReference(PropertyKind propertyKind, SyntaxToken objectTypeKeyword, int objectId)
	{
		return Property(propertyKind, QualifiedObjectReferencePropertyValue(objectTypeKeyword, ObjectNameOrId(LiteralExpression(Int32SignedLiteralValue(Literal(objectId))))));
	}

	public static PropertySyntax PropertyObjectReference(PropertyKind propertyKind, SyntaxToken objectTypeKeyword, string objectName)
	{
		return Property(propertyKind, QualifiedObjectReferencePropertyValue(objectTypeKeyword, ObjectNameOrId(IdentifierName(objectName))));
	}

	public static PropertySyntax Property(PropertyKind propertyKind, PropertyValueSyntax propertyValue)
	{
		return Property(propertyKind.ToString(), propertyValue);
	}

	public static PropertySyntax Property(string propertyName, PropertyValueSyntax propertyValue)
	{
		return Property(PropertyName(Identifier(propertyName)), propertyValue);
	}

	public static PropertySyntax Property(PageTypeKind propertyValue)
	{
		return Property(PropertyKind.PageType, EnumPropertyValue(IdentifierName(propertyValue.ToString())));
	}

	public static SyntaxTrivia EndOfLine(string text, bool elastic = true)
	{
		return InternalSyntaxFactory.EndOfLine(text, elastic);
	}

	public static SyntaxTrivia WhiteSpace(string text, bool elastic = true)
	{
		return InternalSyntaxFactory.Whitespace(text, elastic);
	}

	public static SyntaxToken Identifier(string text)
	{
		return new SyntaxToken(InternalSyntaxFactory.Identifier(ElasticMarker.UnderlyingNode, text.QuoteIdentifierIfNeeded(), ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken IdentifierNoQuotes(string text)
	{
		return new SyntaxToken(InternalSyntaxFactory.Identifier(ElasticMarker.UnderlyingNode, text, ElasticMarker.UnderlyingNode));
	}

	public static IdentifierNameSyntax IdentifierName(string name)
	{
		return IdentifierName(Identifier(name));
	}

	public static SyntaxToken Literal(string text, int value)
	{
		return new SyntaxToken(InternalSyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxToken Literal(int value)
	{
		return Literal(ObjectDisplay.FormatLiteral(value), value);
	}

	public static ObjectIdSyntax ObjectId(int id)
	{
		return ObjectId(Literal(id));
	}

	public static SyntaxToken Literal(string text)
	{
		return new SyntaxToken(InternalSyntaxFactory.Literal(ElasticMarker.UnderlyingNode, ObjectDisplay.FormatLiteral(text), text, ElasticMarker.UnderlyingNode));
	}

	public static SyntaxList<TNode> List<TNode>() where TNode : SyntaxNode
	{
		return default;
	}

	public static SyntaxList<TNode> SingletonList<TNode>(TNode node) where TNode : SyntaxNode
	{
		return new SyntaxList<TNode>(node);
	}

	public static SyntaxList<TNode> List<TNode>(IEnumerable<TNode> nodes) where TNode : SyntaxNode
	{
		if (nodes != null)
		{
			SyntaxListBuilder<TNode> syntaxListBuilder = ((nodes is ICollection<TNode> collection) ? new SyntaxListBuilder<TNode>(collection.Count) : SyntaxListBuilder<TNode>.Create());
			foreach (TNode node in nodes)
			{
				syntaxListBuilder.Add(node);
			}
			return syntaxListBuilder.ToList();
		}
		return default;
	}

	public static SyntaxList<TNode> List<TNode>(params TNode[] nodes) where TNode : SyntaxNode
	{
		return List((IEnumerable<TNode>)nodes);
	}

	public static SyntaxTriviaList TriviaList()
	{
		return default;
	}

	public static SyntaxTriviaList TriviaList(SyntaxTrivia trivia)
	{
		return new SyntaxTriviaList(trivia);
	}

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "trivias")]
	public static SyntaxTriviaList TriviaList(params SyntaxTrivia[] trivias)
	{
		if (trivias != null)
		{
			SyntaxTriviaListBuilder syntaxTriviaListBuilder = new SyntaxTriviaListBuilder(trivias.Length);
			syntaxTriviaListBuilder.Add(trivias);
			return syntaxTriviaListBuilder.ToList();
		}
		return default;
	}

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "trivias")]
	public static SyntaxTriviaList TriviaList(IEnumerable<SyntaxTrivia> trivias)
	{
		if (trivias == null)
		{
			return default;
		}
		SyntaxTriviaListBuilder syntaxTriviaListBuilder = SyntaxTriviaListBuilder.Create();
		foreach (SyntaxTrivia trivia in trivias)
		{
			syntaxTriviaListBuilder.Add(trivia);
		}
		return syntaxTriviaListBuilder.ToList();
	}

	public static SyntaxToken Token(SyntaxKind kind)
	{
		return new SyntaxToken(InternalSyntaxFactory.Token(ElasticMarker.UnderlyingNode, kind, ElasticMarker.UnderlyingNode));
	}

	internal static InternalSyntaxNode NodeOrNull(SyntaxNode node)
	{
		return node?.Node;
	}

	internal static InternalSyntaxNode NodeOrNull(SyntaxToken node)
	{
		if (!(node != default(SyntaxToken)))
		{
			return null;
		}
		return node.Node;
	}

	public static SyntaxTree SyntaxTree(SyntaxNode root, string path = "", Encoding encoding = null, ParseOptions options = null)
	{
		return Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTree.Create(root, path, encoding, null, options);
	}

	public static SyntaxTree ParseSyntaxTree(string text, string path = "", Encoding encoding = null, ParseOptions parseOptions = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ParseSyntaxTree(SourceText.From(text, encoding), path, parseOptions, cancellationToken);
	}

	public static SyntaxTriviaList ParseTrailingTrivia(string text, int offset = 0)
	{
		using Lexer lexer = new Lexer(text);
		return lexer.ScanSyntaxTrailingTrivia();
	}

	public static SyntaxTriviaList ParseLeadingTrivia(string text, int offset = 0)
	{
		using Lexer lexer = new Lexer(text);
		return lexer.ScanSyntaxLeadingTrivia();
	}

	public static SyntaxTree ParseSyntaxTree(SourceText text, string path = "", ParseOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTree.ParseObjectText(text, path, options, null, cancellationToken);
	}

	internal static CodeExpressionSyntax ParseExpression(string text)
	{
		using Lexer lexer = new Lexer(text);
		return (CodeExpressionSyntax)new LanguageParser(lexer, LexerMode.Code, null, null).ParseExpression().CreateRed();
	}

	public static bool AreEquivalent(SyntaxTree oldTree, SyntaxTree newTree, bool topLevel)
	{
		if (oldTree == null && newTree == null)
		{
			return true;
		}
		if (oldTree == null || newTree == null)
		{
			return false;
		}
		return SyntaxEquivalence.AreEquivalent(oldTree, newTree, null, topLevel);
	}

	public static bool AreEquivalent(SyntaxNode oldNode, SyntaxNode newNode, bool topLevel)
	{
		return SyntaxEquivalence.AreEquivalent(oldNode, newNode, null, topLevel);
	}

	public static bool AreEquivalent(SyntaxNode oldNode, SyntaxNode newNode, Func<SyntaxKind, bool> ignoreChildNode = null)
	{
		return SyntaxEquivalence.AreEquivalent(oldNode, newNode, ignoreChildNode, topLevel: false);
	}

	public static bool AreEquivalent(SyntaxToken oldToken, SyntaxToken newToken)
	{
		return SyntaxEquivalence.AreEquivalent(oldToken, newToken);
	}

	public static bool AreEquivalent(SyntaxTokenList oldList, SyntaxTokenList newList)
	{
		return SyntaxEquivalence.AreEquivalent(oldList, newList);
	}

	public static bool AreEquivalent<TNode>(SyntaxList<TNode> oldList, SyntaxList<TNode> newList, bool topLevel) where TNode : SyntaxNode
	{
		return SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, null, topLevel);
	}

	public static bool AreEquivalent<TNode>(SyntaxList<TNode> oldList, SyntaxList<TNode> newList, Func<SyntaxKind, bool> ignoreChildNode = null) where TNode : SyntaxNode
	{
		return SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, ignoreChildNode, topLevel: false);
	}

	public static bool AreEquivalent<TNode>(SeparatedSyntaxList<TNode> oldList, SeparatedSyntaxList<TNode> newList, bool topLevel) where TNode : SyntaxNode
	{
		return SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, null, topLevel);
	}

	public static bool AreEquivalent<TNode>(SeparatedSyntaxList<TNode> oldList, SeparatedSyntaxList<TNode> newList, Func<SyntaxKind, bool> ignoreChildNode = null) where TNode : SyntaxNode
	{
		return SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, ignoreChildNode, topLevel: false);
	}

	internal static SyntaxNode GetStandaloneNode(SyntaxNode node)
	{
		if (node == null || !(node is ExpressionSyntax))
		{
			return node;
		}
		SyntaxKind kind = node.Kind;
		if (kind != SyntaxKind.MemberAccessExpression && kind != SyntaxKind.IdentifierName)
		{
			return node;
		}
		SyntaxNode parent = node.Parent;
		if (parent == null)
		{
			return node;
		}
		switch (parent.Kind)
		{
		case SyntaxKind.MemberAccessExpression:
			if (((MemberAccessExpressionSyntax)parent).Name == node)
			{
				return parent;
			}
			break;
		case SyntaxKind.OptionAccessExpression:
			if (((OptionAccessExpressionSyntax)parent).Name == node)
			{
				return parent;
			}
			break;
		case SyntaxKind.InvocationExpression:
			return parent;
		}
		return node;
	}

	public static FiltersPropertyValueSyntax ParseFiltersPropertyValue(string propertyValue, bool tokenExpected = false)
	{
		using Lexer lexer = new Lexer(propertyValue);
		return FiltersPropertyValue((WhereExpressionSyntax)new ObjectParser(lexer, null, null)
		{
			LexerMode = LexerMode.Property
		}.ParseTableFilters(tokenExpected).CreateRed());
	}

	public static OrderByPropertyValueSyntax ParseOrderByPropertyValue(string propertyValue)
	{
		using Lexer lexer = new Lexer(propertyValue);
		ObjectParser objectParser = new ObjectParser(lexer, null, null)
		{
			LexerMode = LexerMode.Property
		};
		InternalSyntaxToken equalsToken = new InternalSyntaxToken(SyntaxKind.EqualsToken, 0);
		return (OrderByPropertyValueSyntax)objectParser.ParseOrderByPropertyValue(ref equalsToken).CreateRed();
	}
}
