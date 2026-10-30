using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class InvocationExpressionSyntax : CodeExpressionSyntax
{
	private CodeExpressionSyntax _expression;

	private ArgumentListSyntax _argumentList;

	public CodeExpressionSyntax Expression => GetRedAtZero(ref _expression);

	public ArgumentListSyntax ArgumentList => GetRed(ref _argumentList, 1);

	internal InvocationExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitInvocationExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitInvocationExpression(this);
	}

	public InvocationExpressionSyntax Update(CodeExpressionSyntax expression, ArgumentListSyntax argumentList)
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

	public InvocationExpressionSyntax WithExpression(CodeExpressionSyntax expression)
	{
		return Update(expression, ArgumentList);
	}

	public InvocationExpressionSyntax WithArgumentList(ArgumentListSyntax argumentList)
	{
		return Update(Expression, argumentList);
	}

	public InvocationExpressionSyntax AddArgumentListArguments(params CodeExpressionSyntax[] items)
	{
		return WithArgumentList(ArgumentList.WithArguments(ArgumentList.Arguments.AddRange(items)));
	}
}
