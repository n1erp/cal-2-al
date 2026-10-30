using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ClientSideBooleanExpressionPropertyValueSyntax : ExpressionPropertyValueSyntax
{
	private CodeExpressionSyntax _expression;

	public override CodeExpressionSyntax Expression => GetRedAtZero(ref _expression);

	internal ClientSideBooleanExpressionPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _expression);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _expression;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitClientSideBooleanExpressionPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitClientSideBooleanExpressionPropertyValue(this);
	}

	public ClientSideBooleanExpressionPropertyValueSyntax Update(CodeExpressionSyntax expression)
	{
		if (expression != Expression)
		{
			ClientSideBooleanExpressionPropertyValueSyntax clientSideBooleanExpressionPropertyValueSyntax = SyntaxFactory.ClientSideBooleanExpressionPropertyValue(expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return clientSideBooleanExpressionPropertyValueSyntax.WithAnnotations(annotations);
			}
			return clientSideBooleanExpressionPropertyValueSyntax;
		}
		return this;
	}

	public ClientSideBooleanExpressionPropertyValueSyntax WithExpression(CodeExpressionSyntax expression)
	{
		return Update(expression);
	}
}
