using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class BaseMethodDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract SyntaxList<AttributeListSyntax> AttributeLists { get; }

	public abstract SyntaxTokenList Modifiers { get; }

	public abstract ParameterListSyntax ParameterList { get; }

	public abstract BlockSyntax Body { get; }

	public abstract ArrowExpressionClauseSyntax ExpressionBody { get; }

	public abstract SyntaxToken SemicolonToken { get; }

	internal BaseMethodDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public BaseMethodDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeListsCore(attributeLists);
	}

	internal abstract BaseMethodDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists);

	public BaseMethodDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return AddAttributeListsCore(items);
	}

	internal abstract BaseMethodDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items);

	public BaseMethodDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return WithModifiersCore(modifiers);
	}

	internal abstract BaseMethodDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers);

	public BaseMethodDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return AddModifiersCore(items);
	}

	internal abstract BaseMethodDeclarationSyntax AddModifiersCore(params SyntaxToken[] items);

	public BaseMethodDeclarationSyntax WithParameterList(ParameterListSyntax parameterList)
	{
		return WithParameterListCore(parameterList);
	}

	internal abstract BaseMethodDeclarationSyntax WithParameterListCore(ParameterListSyntax parameterList);

	public BaseMethodDeclarationSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		return AddParameterListParametersCore(items);
	}

	internal abstract BaseMethodDeclarationSyntax AddParameterListParametersCore(params ParameterSyntax[] items);

	public BaseMethodDeclarationSyntax WithBody(BlockSyntax body)
	{
		return WithBodyCore(body);
	}

	internal abstract BaseMethodDeclarationSyntax WithBodyCore(BlockSyntax body);

	public BaseMethodDeclarationSyntax AddBodyStatements(params StatementSyntax[] items)
	{
		return AddBodyStatementsCore(items);
	}

	internal abstract BaseMethodDeclarationSyntax AddBodyStatementsCore(params StatementSyntax[] items);

	public BaseMethodDeclarationSyntax WithExpressionBody(ArrowExpressionClauseSyntax expressionBody)
	{
		return WithExpressionBodyCore(expressionBody);
	}

	internal abstract BaseMethodDeclarationSyntax WithExpressionBodyCore(ArrowExpressionClauseSyntax expressionBody);

	public BaseMethodDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return WithSemicolonTokenCore(semicolonToken);
	}

	internal abstract BaseMethodDeclarationSyntax WithSemicolonTokenCore(SyntaxToken semicolonToken);
}
