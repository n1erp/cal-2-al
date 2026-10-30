using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class RefValueExpressionSyntax : ExpressionSyntax
{
	private ExpressionSyntax expression;

	private TypeSyntax type;

	public SyntaxToken Keyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.RefValueExpressionSyntax)Green).keyword, Position, 0);

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.RefValueExpressionSyntax)Green).openParenToken, GetChildPosition(1), GetChildIndex(1));

	public ExpressionSyntax Expression => GetRed(ref expression, 2);

	public SyntaxToken Comma => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.RefValueExpressionSyntax)Green).comma, GetChildPosition(3), GetChildIndex(3));

	public TypeSyntax Type => GetRed(ref type, 4);

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.RefValueExpressionSyntax)Green).closeParenToken, GetChildPosition(5), GetChildIndex(5));

	internal RefValueExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref expression, 2), 
			4 => GetRed(ref type, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)expression, 
			4 => type, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRefValueExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitRefValueExpression(this);
	}

	public RefValueExpressionSyntax Update(SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken comma, TypeSyntax type, SyntaxToken closeParenToken)
	{
		if (keyword != Keyword || openParenToken != OpenParenToken || expression != Expression || comma != Comma || type != Type || closeParenToken != CloseParenToken)
		{
			RefValueExpressionSyntax refValueExpressionSyntax = SyntaxFactory.RefValueExpression(keyword, openParenToken, expression, comma, type, closeParenToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return refValueExpressionSyntax.WithAnnotations(annotations);
			}
			return refValueExpressionSyntax;
		}
		return this;
	}

	public RefValueExpressionSyntax WithKeyword(SyntaxToken keyword)
	{
		return Update(keyword, OpenParenToken, Expression, Comma, Type, CloseParenToken);
	}

	public RefValueExpressionSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(Keyword, openParenToken, Expression, Comma, Type, CloseParenToken);
	}

	public RefValueExpressionSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(Keyword, OpenParenToken, expression, Comma, Type, CloseParenToken);
	}

	public RefValueExpressionSyntax WithComma(SyntaxToken comma)
	{
		return Update(Keyword, OpenParenToken, Expression, comma, Type, CloseParenToken);
	}

	public RefValueExpressionSyntax WithType(TypeSyntax type)
	{
		return Update(Keyword, OpenParenToken, Expression, Comma, type, CloseParenToken);
	}

	public RefValueExpressionSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(Keyword, OpenParenToken, Expression, Comma, Type, closeParenToken);
	}
}
