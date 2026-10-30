using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ElementAccessExpressionSyntax : ExpressionSyntax
{
	private ExpressionSyntax expression;

	private BracketedArgumentListSyntax argumentList;

	public ExpressionSyntax Expression => GetRedAtZero(ref expression);

	public BracketedArgumentListSyntax ArgumentList => GetRed(ref argumentList, 1);

	internal ElementAccessExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitElementAccessExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitElementAccessExpression(this);
	}

	public ElementAccessExpressionSyntax Update(ExpressionSyntax expression, BracketedArgumentListSyntax argumentList)
	{
		if (expression != Expression || argumentList != ArgumentList)
		{
			ElementAccessExpressionSyntax elementAccessExpressionSyntax = SyntaxFactory.ElementAccessExpression(expression, argumentList);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return elementAccessExpressionSyntax.WithAnnotations(annotations);
			}
			return elementAccessExpressionSyntax;
		}
		return this;
	}

	public ElementAccessExpressionSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(expression, ArgumentList);
	}

	public ElementAccessExpressionSyntax WithArgumentList(BracketedArgumentListSyntax argumentList)
	{
		return Update(Expression, argumentList);
	}

	public ElementAccessExpressionSyntax AddArgumentListArguments(params ArgumentSyntax[] items)
	{
		return WithArgumentList(ArgumentList.WithArguments(ArgumentList.Arguments.AddRange(items)));
	}
}
