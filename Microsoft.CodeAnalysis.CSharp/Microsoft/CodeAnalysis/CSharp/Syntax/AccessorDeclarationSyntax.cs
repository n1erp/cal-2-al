using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class AccessorDeclarationSyntax : CSharpSyntaxNode
{
	private SyntaxNode attributeLists;

	private BlockSyntax body;

	private ArrowExpressionClauseSyntax expressionBody;

	public SyntaxList<AttributeListSyntax> AttributeLists => new SyntaxList<AttributeListSyntax>(GetRed(ref attributeLists, 0));

	public SyntaxTokenList Modifiers
	{
		get
		{
			GreenNode slot = Green.GetSlot(1);
			if (slot != null)
			{
				return new SyntaxTokenList(this, slot, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken Keyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AccessorDeclarationSyntax)Green).keyword, GetChildPosition(2), GetChildIndex(2));

	public BlockSyntax Body => GetRed(ref body, 3);

	public ArrowExpressionClauseSyntax ExpressionBody => GetRed(ref expressionBody, 4);

	public SyntaxToken SemicolonToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken semicolonToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AccessorDeclarationSyntax)Green).semicolonToken;
			if (semicolonToken != null)
			{
				return new SyntaxToken(this, semicolonToken, GetChildPosition(5), GetChildIndex(5));
			}
			return default;
		}
	}

	internal AccessorDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			3 => GetRed(ref body, 3), 
			4 => GetRed(ref expressionBody, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			3 => body, 
			4 => expressionBody, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAccessorDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAccessorDeclaration(this);
	}

	public AccessorDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken keyword, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || keyword != Keyword || body != Body || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			AccessorDeclarationSyntax accessorDeclarationSyntax = SyntaxFactory.AccessorDeclaration(Kind(), attributeLists, modifiers, keyword, body, expressionBody, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return accessorDeclarationSyntax.WithAnnotations(annotations);
			}
			return accessorDeclarationSyntax;
		}
		return this;
	}

	public AccessorDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, Keyword, Body, ExpressionBody, SemicolonToken);
	}

	public AccessorDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, Keyword, Body, ExpressionBody, SemicolonToken);
	}

	public AccessorDeclarationSyntax WithKeyword(SyntaxToken keyword)
	{
		return Update(AttributeLists, Modifiers, keyword, Body, ExpressionBody, SemicolonToken);
	}

	public AccessorDeclarationSyntax WithBody(BlockSyntax body)
	{
		return Update(AttributeLists, Modifiers, Keyword, body, ExpressionBody, SemicolonToken);
	}

	public AccessorDeclarationSyntax WithExpressionBody(ArrowExpressionClauseSyntax expressionBody)
	{
		return Update(AttributeLists, Modifiers, Keyword, Body, expressionBody, SemicolonToken);
	}

	public AccessorDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(AttributeLists, Modifiers, Keyword, Body, ExpressionBody, semicolonToken);
	}

	public AccessorDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	public AccessorDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	public AccessorDeclarationSyntax AddBodyStatements(params StatementSyntax[] items)
	{
		BlockSyntax blockSyntax = Body ?? SyntaxFactory.Block();
		return WithBody(blockSyntax.WithStatements(blockSyntax.Statements.AddRange(items)));
	}

	public AccessorDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken keyword, BlockSyntax body, SyntaxToken semicolonToken)
	{
		return Update(attributeLists, modifiers, keyword, body, null, semicolonToken);
	}
}
