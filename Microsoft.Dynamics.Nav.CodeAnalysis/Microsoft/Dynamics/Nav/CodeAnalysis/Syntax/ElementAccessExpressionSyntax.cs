using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ElementAccessExpressionSyntax : CodeExpressionSyntax
{
	private CodeExpressionSyntax _expression;

	private BracketedArgumentListSyntax _argumentList;

	public CodeExpressionSyntax Expression => GetRedAtZero(ref _expression);

	public BracketedArgumentListSyntax ArgumentList => GetRed(ref _argumentList, 1);

	internal ElementAccessExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _expression), 
			1 => GetRed(ref _argumentList, 1), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_expression, 
			1 => _argumentList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitElementAccessExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitElementAccessExpression(this);
	}

	public ElementAccessExpressionSyntax Update(CodeExpressionSyntax expression, BracketedArgumentListSyntax argumentList)
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

	public ElementAccessExpressionSyntax WithExpression(CodeExpressionSyntax expression)
	{
		return Update(expression, ArgumentList);
	}

	public ElementAccessExpressionSyntax WithArgumentList(BracketedArgumentListSyntax argumentList)
	{
		return Update(Expression, argumentList);
	}

	public ElementAccessExpressionSyntax AddArgumentListArguments(params CodeExpressionSyntax[] items)
	{
		return WithArgumentList(ArgumentList.WithArguments(ArgumentList.Arguments.AddRange(items)));
	}
}
