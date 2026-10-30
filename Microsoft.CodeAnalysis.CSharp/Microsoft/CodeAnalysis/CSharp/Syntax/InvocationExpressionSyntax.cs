using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class InvocationExpressionSyntax : ExpressionSyntax
{
	private ExpressionSyntax expression;

	private ArgumentListSyntax argumentList;

	public ExpressionSyntax Expression => GetRedAtZero(ref expression);

	public ArgumentListSyntax ArgumentList => GetRed(ref argumentList, 1);

	internal InvocationExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref expression), 
			1 => GetRed(ref argumentList, 1), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)expression, 
			1 => argumentList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInvocationExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitInvocationExpression(this);
	}

	public InvocationExpressionSyntax Update(ExpressionSyntax expression, ArgumentListSyntax argumentList)
	{
		if (expression != Expression || argumentList != ArgumentList)
		{
			InvocationExpressionSyntax invocationExpressionSyntax = SyntaxFactory.InvocationExpression(expression, argumentList);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return invocationExpressionSyntax.WithAnnotations(annotations);
			}
			return invocationExpressionSyntax;
		}
		return this;
	}

	public InvocationExpressionSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(expression, ArgumentList);
	}

	public InvocationExpressionSyntax WithArgumentList(ArgumentListSyntax argumentList)
	{
		return Update(Expression, argumentList);
	}

	public InvocationExpressionSyntax AddArgumentListArguments(params ArgumentSyntax[] items)
	{
		return WithArgumentList(ArgumentList.WithArguments(ArgumentList.Arguments.AddRange(items)));
	}
}
