using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class TypeDeclarationSyntax : BaseTypeDeclarationSyntax
{
	public abstract SyntaxToken Keyword { get; }

	public abstract TypeParameterListSyntax TypeParameterList { get; }

	public abstract SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses { get; }

	public abstract SyntaxList<MemberDeclarationSyntax> Members { get; }

	public int Arity
	{
		get
		{
			if (TypeParameterList != null)
			{
				return TypeParameterList.Parameters.Count;
			}
			return 0;
		}
	}

	internal TypeDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public TypeDeclarationSyntax WithKeyword(SyntaxToken keyword)
	{
		return WithKeywordCore(keyword);
	}

	internal abstract TypeDeclarationSyntax WithKeywordCore(SyntaxToken keyword);

	public TypeDeclarationSyntax WithTypeParameterList(TypeParameterListSyntax typeParameterList)
	{
		return WithTypeParameterListCore(typeParameterList);
	}

	internal abstract TypeDeclarationSyntax WithTypeParameterListCore(TypeParameterListSyntax typeParameterList);

	public TypeDeclarationSyntax AddTypeParameterListParameters(params TypeParameterSyntax[] items)
	{
		return AddTypeParameterListParametersCore(items);
	}

	internal abstract TypeDeclarationSyntax AddTypeParameterListParametersCore(params TypeParameterSyntax[] items);

	public TypeDeclarationSyntax WithConstraintClauses(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
	{
		return WithConstraintClausesCore(constraintClauses);
	}

	internal abstract TypeDeclarationSyntax WithConstraintClausesCore(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses);

	public TypeDeclarationSyntax AddConstraintClauses(params TypeParameterConstraintClauseSyntax[] items)
	{
		return AddConstraintClausesCore(items);
	}

	internal abstract TypeDeclarationSyntax AddConstraintClausesCore(params TypeParameterConstraintClauseSyntax[] items);

	public TypeDeclarationSyntax WithMembers(SyntaxList<MemberDeclarationSyntax> members)
	{
		return WithMembersCore(members);
	}

	internal abstract TypeDeclarationSyntax WithMembersCore(SyntaxList<MemberDeclarationSyntax> members);

	public TypeDeclarationSyntax AddMembers(params MemberDeclarationSyntax[] items)
	{
		return AddMembersCore(items);
	}

	internal abstract TypeDeclarationSyntax AddMembersCore(params MemberDeclarationSyntax[] items);

	public new TypeDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return (TypeDeclarationSyntax)WithAttributeListsCore(attributeLists);
	}

	public new TypeDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return (TypeDeclarationSyntax)WithModifiersCore(modifiers);
	}

	public new TypeDeclarationSyntax WithIdentifier(SyntaxToken identifier)
	{
		return (TypeDeclarationSyntax)WithIdentifierCore(identifier);
	}

	public new TypeDeclarationSyntax WithBaseList(BaseListSyntax baseList)
	{
		return (TypeDeclarationSyntax)WithBaseListCore(baseList);
	}

	public new TypeDeclarationSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return (TypeDeclarationSyntax)WithOpenBraceTokenCore(openBraceToken);
	}

	public new TypeDeclarationSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return (TypeDeclarationSyntax)WithCloseBraceTokenCore(closeBraceToken);
	}

	public new TypeDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return (TypeDeclarationSyntax)WithSemicolonTokenCore(semicolonToken);
	}

	public new TypeDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return (TypeDeclarationSyntax)AddAttributeListsCore(items);
	}

	public new TypeDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return (TypeDeclarationSyntax)AddModifiersCore(items);
	}

	public new BaseTypeDeclarationSyntax AddBaseListTypes(params BaseTypeSyntax[] items)
	{
		return AddBaseListTypesCore(items);
	}
}
