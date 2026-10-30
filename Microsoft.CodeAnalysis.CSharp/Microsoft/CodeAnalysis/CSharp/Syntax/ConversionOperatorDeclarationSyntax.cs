using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ConversionOperatorDeclarationSyntax : BaseMethodDeclarationSyntax
{
	private SyntaxNode attributeLists;

	private TypeSyntax type;

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

	public SyntaxToken ImplicitOrExplicitKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorDeclarationSyntax)Green).implicitOrExplicitKeyword, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken OperatorKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorDeclarationSyntax)Green).operatorKeyword, GetChildPosition(3), GetChildIndex(3));

	public TypeSyntax Type => GetRed(ref type, 4);

	public override ParameterListSyntax ParameterList => GetRed(ref parameterList, 5);

	public override BlockSyntax Body => GetRed(ref body, 6);

	public override ArrowExpressionClauseSyntax ExpressionBody => GetRed(ref expressionBody, 7);

	public override SyntaxToken SemicolonToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken semicolonToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConversionOperatorDeclarationSyntax)Green).semicolonToken;
			if (semicolonToken != null)
			{
				return new SyntaxToken(this, semicolonToken, GetChildPosition(8), GetChildIndex(8));
			}
			return default;
		}
	}

	internal ConversionOperatorDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			4 => GetRed(ref type, 4), 
			5 => GetRed(ref parameterList, 5), 
			6 => GetRed(ref body, 6), 
			7 => GetRed(ref expressionBody, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			4 => type, 
			5 => parameterList, 
			6 => body, 
			7 => expressionBody, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConversionOperatorDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConversionOperatorDeclaration(this);
	}

	public ConversionOperatorDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, TypeSyntax type, ParameterListSyntax parameterList, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || implicitOrExplicitKeyword != ImplicitOrExplicitKeyword || operatorKeyword != OperatorKeyword || type != Type || parameterList != ParameterList || body != Body || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			ConversionOperatorDeclarationSyntax conversionOperatorDeclarationSyntax = SyntaxFactory.ConversionOperatorDeclaration(attributeLists, modifiers, implicitOrExplicitKeyword, operatorKeyword, type, parameterList, body, expressionBody, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return conversionOperatorDeclarationSyntax.WithAnnotations(annotations);
			}
			return conversionOperatorDeclarationSyntax;
		}
		return this;
	}

	internal override BaseMethodDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeLists(attributeLists);
	}

	public new ConversionOperatorDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, ImplicitOrExplicitKeyword, OperatorKeyword, Type, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers)
	{
		return WithModifiers(modifiers);
	}

	public new ConversionOperatorDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, ImplicitOrExplicitKeyword, OperatorKeyword, Type, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public ConversionOperatorDeclarationSyntax WithImplicitOrExplicitKeyword(SyntaxToken implicitOrExplicitKeyword)
	{
		return Update(AttributeLists, Modifiers, implicitOrExplicitKeyword, OperatorKeyword, Type, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public ConversionOperatorDeclarationSyntax WithOperatorKeyword(SyntaxToken operatorKeyword)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, operatorKeyword, Type, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	public ConversionOperatorDeclarationSyntax WithType(TypeSyntax type)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, OperatorKeyword, type, ParameterList, Body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithParameterListCore(ParameterListSyntax parameterList)
	{
		return WithParameterList(parameterList);
	}

	public new ConversionOperatorDeclarationSyntax WithParameterList(ParameterListSyntax parameterList)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, OperatorKeyword, Type, parameterList, Body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithBodyCore(BlockSyntax body)
	{
		return WithBody(body);
	}

	public new ConversionOperatorDeclarationSyntax WithBody(BlockSyntax body)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, OperatorKeyword, Type, ParameterList, body, ExpressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithExpressionBodyCore(ArrowExpressionClauseSyntax expressionBody)
	{
		return WithExpressionBody(expressionBody);
	}

	public new ConversionOperatorDeclarationSyntax WithExpressionBody(ArrowExpressionClauseSyntax expressionBody)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, OperatorKeyword, Type, ParameterList, Body, expressionBody, SemicolonToken);
	}

	internal override BaseMethodDeclarationSyntax WithSemicolonTokenCore(SyntaxToken semicolonToken)
	{
		return WithSemicolonToken(semicolonToken);
	}

	public new ConversionOperatorDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(AttributeLists, Modifiers, ImplicitOrExplicitKeyword, OperatorKeyword, Type, ParameterList, Body, ExpressionBody, semicolonToken);
	}

	internal override BaseMethodDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddAttributeLists(items);
	}

	public new ConversionOperatorDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	internal override BaseMethodDeclarationSyntax AddModifiersCore(params SyntaxToken[] items)
	{
		return AddModifiers(items);
	}

	public new ConversionOperatorDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	internal override BaseMethodDeclarationSyntax AddParameterListParametersCore(params ParameterSyntax[] items)
	{
		return AddParameterListParameters(items);
	}

	public new ConversionOperatorDeclarationSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
	}

	internal override BaseMethodDeclarationSyntax AddBodyStatementsCore(params StatementSyntax[] items)
	{
		return AddBodyStatements(items);
	}

	public new ConversionOperatorDeclarationSyntax AddBodyStatements(params StatementSyntax[] items)
	{
		BlockSyntax blockSyntax = Body ?? SyntaxFactory.Block();
		return WithBody(blockSyntax.WithStatements(blockSyntax.Statements.AddRange(items)));
	}
}
