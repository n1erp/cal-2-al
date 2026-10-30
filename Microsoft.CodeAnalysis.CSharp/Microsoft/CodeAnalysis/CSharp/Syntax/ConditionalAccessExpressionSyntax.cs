using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ConditionalAccessExpressionSyntax : ExpressionSyntax
{
	private ExpressionSyntax expression;

	private ExpressionSyntax whenNotNull;

	public ExpressionSyntax Expression => GetRedAtZero(ref expression);

	public SyntaxToken OperatorToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConditionalAccessExpressionSyntax)Green).operatorToken, GetChildPosition(1), GetChildIndex(1));

	public ExpressionSyntax WhenNotNull => GetRed(ref whenNotNull, 2);

	internal ConditionalAccessExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref expression), 
			2 => GetRed(ref whenNotNull, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)expression, 
			2 => whenNotNull, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConditionalAccessExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConditionalAccessExpression(this);
	}

	public ConditionalAccessExpressionSyntax Update(ExpressionSyntax expression, SyntaxToken operatorToken, ExpressionSyntax whenNotNull)
	{
		if (expression != Expression || operatorToken != OperatorToken || whenNotNull != WhenNotNull)
		{
			ConditionalAccessExpressionSyntax conditionalAccessExpressionSyntax = SyntaxFactory.ConditionalAccessExpression(expression, operatorToken, whenNotNull);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return conditionalAccessExpressionSyntax.WithAnnotations(annotations);
			}
			return conditionalAccessExpressionSyntax;
		}
		return this;
	}

	public ConditionalAccessExpressionSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(expression, OperatorToken, WhenNotNull);
	}

	public ConditionalAccessExpressionSyntax WithOperatorToken(SyntaxToken operatorToken)
	{
		return Update(Expression, operatorToken, WhenNotNull);
	}

	public ConditionalAccessExpressionSyntax WithWhenNotNull(ExpressionSyntax whenNotNull)
	{
		return Update(Expression, OperatorToken, whenNotNull);
	}
}
