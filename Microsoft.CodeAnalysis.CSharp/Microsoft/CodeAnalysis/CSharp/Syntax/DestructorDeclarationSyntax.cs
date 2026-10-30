using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class DestructorDeclarationSyntax : BaseMethodDeclarationSyntax
{
	private SyntaxNode attributeLists;

	private ParameterListSyntax parameterList;

	private BlockSyntax body;

	private ArrowExpressionClauseSyntax expressionBody;

	public override SyntaxList<AttributeListSyntax> AttributeLists => new SyntaxList<AttributeListSyntax>(GetRed(ref attributeLists, 0));

	public override SyntaxTokenList Modifiers
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

	public SyntaxToken TildeToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DestructorDeclarationSyntax)Green).tildeToken, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DestructorDeclarationSyntax)Green).identifier, GetChildPosition(3), GetChildIndex(3));

	public override ParameterListSyntax ParameterList => GetRed(ref parameterList, 4);

	public override BlockSyntax Body => GetRed(ref body, 5);

	public override ArrowExpressionClauseSyntax ExpressionBody => GetRed(ref expressionBody, 6);

	public override SyntaxToken SemicolonToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken semicolonToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DestructorDeclarationSyntax)Green).semicolonToken;
			if (semicolonToken != null)
			{
				return new SyntaxToken(this, semicolonToken, GetChildPosition(7), GetChildIndex(7));
			}
			return default;
		}
	}

	internal DestructorDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			4 => GetRed(ref parameterList, 4), 
			5 => GetRed(ref body, 5), 
			6 => GetRed(ref expressionBody, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			4 => parameterList, 
			5 => body, 
			6 => expressionBody, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDestructorDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDestructorDeclaration(this);
	}

	public DestructorDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken tildeToken, SyntaxToken identifier, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || tildeToken != TildeToken || identifier != Identifier || parameterList != ParameterList || body != Body || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			DestructorDeclarationSyntax destructorDeclarationSyntax = SyntaxFactory.DestructorDeclaration(attributeLists, modifiers, tildeToken, identifier, parameterList, body, expressionBody, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return destructorDeclarationSyntax.WithAnnotations(annotations);
			}
			return destructorDeclarationSyntax;
		}
		return this;
	}

	internal override BaseMethodDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeLists(attributeLists);
	}

	public new DestructorDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, TildeToken, Identifier, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers)
	{
		return WithModifiers(modifiers);
	}

	public new DestructorDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, TildeToken, Identifier, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public DestructorDeclarationSyntax WithTildeToken(SyntaxToken tildeToken)
	{
		return Update(AttributeLists, Modifiers, tildeToken, Identifier, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public DestructorDeclarationSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(AttributeLists, Modifiers, TildeToken, identifier, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithParameterListCore(ParameterListSyntax parameterList)
	{
		return WithParameterList(parameterList);
	}

	public new DestructorDeclarationSyntax WithParameterList(ParameterListSyntax parameterList)
	{
		return Update(AttributeLists, Modifiers, TildeToken, Identifier, parameterList, Body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithBodyCore(BlockSyntax body)
	{
		return WithBody(body);
	}

	public new DestructorDeclarationSyntax WithBody(BlockSyntax body)
	{
		return Update(AttributeLists, Modifiers, TildeToken, Identifier, ParameterList, body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithExpressionBodyCore(ArrowExpressionClauseSyntax expressionBody)
	{
		return WithExpressionBody(expressionBody);
	}

	public new DestructorDeclarationSyntax WithExpressionBody(ArrowExpressionClauseSyntax expressionBody)
	{
		return Update(AttributeLists, Modifiers, TildeToken, Identifier, ParameterList, Body, expressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithSemicolonTokenCore(SyntaxToken semicolonToken)
	{
		return WithSemicolonToken(semicolonToken);
	}

	public new DestructorDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(AttributeLists, Modifiers, TildeToken, Identifier, ParameterList, Body, ExpressionBody, semicolonToken);
	}

	internal override BaseMethodDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddAttributeLists(items);
	}

	public new DestructorDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	internal override BaseMethodDeclarationSyntax AddModifiersCore(params SyntaxToken[] items)
	{
		return AddModifiers(items);
	}

	public new DestructorDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	internal override BaseMethodDeclarationSyntax AddParameterListParametersCore(params ParameterSyntax[] items)
	{
		return AddParameterListParameters(items);
	}

	public new DestructorDeclarationSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
	}

	internal override BaseMethodDeclarationSyntax AddBodyStatementsCore(params StatementSyntax[] items)
	{
		return AddBodyStatements(items);
	}

	public new DestructorDeclarationSyntax AddBodyStatements(params StatementSyntax[] items)
	{
		BlockSyntax blockSyntax = Body ?? SyntaxFactory.Block();
		return WithBody(blockSyntax.WithStatements(blockSyntax.Statements.AddRange(items)));
	}

	public DestructorDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken tildeToken, SyntaxToken identifier, ParameterListSyntax parameterList, BlockSyntax body, SyntaxToken semicolonToken)
	{
		return Update(attributeLists, modifiers, tildeToken, identifier, parameterList, body, null, semicolonToken);
	}
}
