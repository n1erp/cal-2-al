using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class LiteralExpressionSyntax : CodeExpressionSyntax
{
	private LiteralValueSyntax _literal;

	public LiteralValueSyntax Literal => GetRedAtZero(ref _literal);

	internal LiteralExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _literal);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _literal;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLiteralExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitLiteralExpression(this);
	}

	public LiteralExpressionSyntax Update(LiteralValueSyntax literal)
	{
		if (literal != Literal)
		{
			LiteralExpressionSyntax literalExpressionSyntax = SyntaxFactory.LiteralExpression(literal);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return literalExpressionSyntax.WithAnnotations(annotations);
			}
			return literalExpressionSyntax;
		}
		return this;
	}

	public LiteralExpressionSyntax WithLiteral(LiteralValueSyntax literal)
	{
		return Update(literal);
	}
}
