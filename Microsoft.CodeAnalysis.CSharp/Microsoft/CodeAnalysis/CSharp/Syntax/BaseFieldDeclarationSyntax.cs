using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class BaseFieldDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract SyntaxList<AttributeListSyntax> AttributeLists { get; }

	public abstract SyntaxTokenList Modifiers { get; }

	public abstract VariableDeclarationSyntax Declaration { get; }

	public abstract SyntaxToken SemicolonToken { get; }

	internal BaseFieldDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public BaseFieldDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeListsCore(attributeLists);
	}

	internal abstract BaseFieldDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists);

	public BaseFieldDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return AddAttributeListsCore(items);
	}

	internal abstract BaseFieldDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items);

	public BaseFieldDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return WithModifiersCore(modifiers);
	}

	internal abstract BaseFieldDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers);

	public BaseFieldDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return AddModifiersCore(items);
	}

	internal abstract BaseFieldDeclarationSyntax AddModifiersCore(params SyntaxToken[] items);

	public BaseFieldDeclarationSyntax WithDeclaration(VariableDeclarationSyntax declaration)
	{
		return WithDeclarationCore(declaration);
	}

	internal abstract BaseFieldDeclarationSyntax WithDeclarationCore(VariableDeclarationSyntax declaration);

	public BaseFieldDeclarationSyntax AddDeclarationVariables(params VariableDeclaratorSyntax[] items)
	{
		return AddDeclarationVariablesCore(items);
	}

	internal abstract BaseFieldDeclarationSyntax AddDeclarationVariablesCore(params VariableDeclaratorSyntax[] items);

	public BaseFieldDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return WithSemicolonTokenCore(semicolonToken);
	}

	internal abstract BaseFieldDeclarationSyntax WithSemicolonTokenCore(SyntaxToken semicolonToken);
}
