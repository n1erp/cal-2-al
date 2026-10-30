using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class BaseTypeDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract SyntaxList<AttributeListSyntax> AttributeLists { get; }

	public abstract SyntaxTokenList Modifiers { get; }

	public abstract SyntaxToken Identifier { get; }

	public abstract BaseListSyntax BaseList { get; }

	public abstract SyntaxToken OpenBraceToken { get; }

	public abstract SyntaxToken CloseBraceToken { get; }

	public abstract SyntaxToken SemicolonToken { get; }

	internal BaseTypeDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public BaseTypeDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeListsCore(attributeLists);
	}

	internal abstract BaseTypeDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists);

	public BaseTypeDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return AddAttributeListsCore(items);
	}

	internal abstract BaseTypeDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items);

	public BaseTypeDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return WithModifiersCore(modifiers);
	}

	internal abstract BaseTypeDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers);

	public BaseTypeDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return AddModifiersCore(items);
	}

	internal abstract BaseTypeDeclarationSyntax AddModifiersCore(params SyntaxToken[] items);

	public BaseTypeDeclarationSyntax WithIdentifier(SyntaxToken identifier)
	{
		return WithIdentifierCore(identifier);
	}

	internal abstract BaseTypeDeclarationSyntax WithIdentifierCore(SyntaxToken identifier);

	public BaseTypeDeclarationSyntax WithBaseList(BaseListSyntax baseList)
	{
		return WithBaseListCore(baseList);
	}

	internal abstract BaseTypeDeclarationSyntax WithBaseListCore(BaseListSyntax baseList);

	public BaseTypeDeclarationSyntax AddBaseListTypes(params BaseTypeSyntax[] items)
	{
		return AddBaseListTypesCore(items);
	}

	internal abstract BaseTypeDeclarationSyntax AddBaseListTypesCore(params BaseTypeSyntax[] items);

	public BaseTypeDeclarationSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return WithOpenBraceTokenCore(openBraceToken);
	}

	internal abstract BaseTypeDeclarationSyntax WithOpenBraceTokenCore(SyntaxToken openBraceToken);

	public BaseTypeDeclarationSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return WithCloseBraceTokenCore(closeBraceToken);
	}

	internal abstract BaseTypeDeclarationSyntax WithCloseBraceTokenCore(SyntaxToken closeBraceToken);

	public BaseTypeDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return WithSemicolonTokenCore(semicolonToken);
	}

	internal abstract BaseTypeDeclarationSyntax WithSemicolonTokenCore(SyntaxToken semicolonToken);
}
