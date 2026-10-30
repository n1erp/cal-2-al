using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class LocalFunctionStatementSyntax : StatementSyntax
{
	private TypeSyntax returnType;

	private TypeParameterListSyntax typeParameterList;

	private ParameterListSyntax parameterList;

	private SyntaxNode constraintClauses;

	private BlockSyntax body;

	private ArrowExpressionClauseSyntax expressionBody;

	public SyntaxTokenList Modifiers
	{
		get
		{
			GreenNode slot = Green.GetSlot(0);
			if (slot != null)
			{
				return new SyntaxTokenList(this, slot, Position, 0);
			}
			return default;
		}
	}

	public TypeSyntax ReturnType => GetRed(ref returnType, 1);

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LocalFunctionStatementSyntax)Green).identifier, GetChildPosition(2), GetChildIndex(2));

	public TypeParameterListSyntax TypeParameterList => GetRed(ref typeParameterList, 3);

	public ParameterListSyntax ParameterList => GetRed(ref parameterList, 4);

	public SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses => new SyntaxList<TypeParameterConstraintClauseSyntax>(GetRed(ref constraintClauses, 5));

	public BlockSyntax Body => GetRed(ref body, 6);

	public ArrowExpressionClauseSyntax ExpressionBody => GetRed(ref expressionBody, 7);

	public SyntaxToken SemicolonToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken semicolonToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LocalFunctionStatementSyntax)Green).semicolonToken;
			if (semicolonToken != null)
			{
				return new SyntaxToken(this, semicolonToken, GetChildPosition(8), GetChildIndex(8));
			}
			return default;
		}
	}

	internal LocalFunctionStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref returnType, 1), 
			3 => GetRed(ref typeParameterList, 3), 
			4 => GetRed(ref parameterList, 4), 
			5 => GetRed(ref constraintClauses, 5), 
			6 => GetRed(ref body, 6), 
			7 => GetRed(ref expressionBody, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => returnType, 
			3 => typeParameterList, 
			4 => parameterList, 
			5 => constraintClauses, 
			6 => body, 
			7 => expressionBody, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLocalFunctionStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLocalFunctionStatement(this);
	}

	public LocalFunctionStatementSyntax Update(SyntaxTokenList modifiers, TypeSyntax returnType, SyntaxToken identifier, TypeParameterListSyntax typeParameterList, ParameterListSyntax parameterList, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (modifiers != Modifiers || returnType != ReturnType || identifier != Identifier || typeParameterList != TypeParameterList || parameterList != ParameterList || constraintClauses != ConstraintClauses || body != Body || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			LocalFunctionStatementSyntax localFunctionStatementSyntax = SyntaxFactory.LocalFunctionStatement(modifiers, returnType, identifier, typeParameterList, parameterList, constraintClauses, body, expressionBody, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return localFunctionStatementSyntax.WithAnnotations(annotations);
			}
			return localFunctionStatementSyntax;
		}
		return this;
	}

	public LocalFunctionStatementSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(modifiers, ReturnType, Identifier, TypeParameterList, ParameterList, ConstraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	public LocalFunctionStatementSyntax WithReturnType(TypeSyntax returnType)
	{
		return Update(Modifiers, returnType, Identifier, TypeParameterList, ParameterList, ConstraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	public LocalFunctionStatementSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(Modifiers, ReturnType, identifier, TypeParameterList, ParameterList, ConstraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	public LocalFunctionStatementSyntax WithTypeParameterList(TypeParameterListSyntax typeParameterList)
	{
		return Update(Modifiers, ReturnType, Identifier, typeParameterList, ParameterList, ConstraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	public LocalFunctionStatementSyntax WithParameterList(ParameterListSyntax parameterList)
	{
		return Update(Modifiers, ReturnType, Identifier, TypeParameterList, parameterList, ConstraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	public LocalFunctionStatementSyntax WithConstraintClauses(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
	{
		return Update(Modifiers, ReturnType, Identifier, TypeParameterList, ParameterList, constraintClauses, Body, ExpressionBody, SemicolonToken);
	}

	public LocalFunctionStatementSyntax WithBody(BlockSyntax body)
	{
		return Update(Modifiers, ReturnType, Identifier, TypeParameterList, ParameterList, ConstraintClauses, body, ExpressionBody, SemicolonToken);
	}

	public LocalFunctionStatementSyntax WithExpressionBody(ArrowExpressionClauseSyntax expressionBody)
	{
		return Update(Modifiers, ReturnType, Identifier, TypeParameterList, ParameterList, ConstraintClauses, Body, expressionBody, SemicolonToken);
	}

	public LocalFunctionStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(Modifiers, ReturnType, Identifier, TypeParameterList, ParameterList, ConstraintClauses, Body, ExpressionBody, semicolonToken);
	}

	public LocalFunctionStatementSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	public LocalFunctionStatementSyntax AddTypeParameterListParameters(params TypeParameterSyntax[] items)
	{
		TypeParameterListSyntax typeParameterListSyntax = TypeParameterList ?? SyntaxFactory.TypeParameterList();
		return WithTypeParameterList(typeParameterListSyntax.WithParameters(typeParameterListSyntax.Parameters.AddRange(items)));
	}

	public LocalFunctionStatementSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
	}

	public LocalFunctionStatementSyntax AddConstraintClauses(params TypeParameterConstraintClauseSyntax[] items)
	{
		return WithConstraintClauses(ConstraintClauses.AddRange(items));
	}

	public LocalFunctionStatementSyntax AddBodyStatements(params StatementSyntax[] items)
	{
		BlockSyntax blockSyntax = Body ?? SyntaxFactory.Block();
		return WithBody(blockSyntax.WithStatements(blockSyntax.Statements.AddRange(items)));
	}
}
