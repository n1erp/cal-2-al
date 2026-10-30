using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class RangeFromFilterExpressionValueSyntax : FilterExpressionValueSyntax
{
	private IdentifierOrLiteralExpressionSyntax _left;

	public IdentifierOrLiteralExpressionSyntax Left => GetRedAtZero(ref _left);

	public SyntaxToken OperatorToken => new SyntaxToken(this, ((InternalRangeFromFilterExpressionValueSyntax)Node)._operatorToken, GetChildPosition(1), GetChildIndex(1));

	internal RangeFromFilterExpressionValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _left);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _left;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRangeFromFilterExpressionValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitRangeFromFilterExpressionValue(this);
	}

	public RangeFromFilterExpressionValueSyntax Update(IdentifierOrLiteralExpressionSyntax left, SyntaxToken operatorToken)
	{
		if (left != Left || operatorToken != OperatorToken)
		{
			RangeFromFilterExpressionValueSyntax rangeFromFilterExpressionValueSyntax = SyntaxFactory.RangeFromFilterExpressionValue(left, operatorToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return rangeFromFilterExpressionValueSyntax.WithAnnotations(annotations);
			}
			return rangeFromFilterExpressionValueSyntax;
		}
		return this;
	}

	public RangeFromFilterExpressionValueSyntax WithLeft(IdentifierOrLiteralExpressionSyntax left)
	{
		return Update(left, OperatorToken);
	}

	public RangeFromFilterExpressionValueSyntax WithOperatorToken(SyntaxToken operatorToken)
	{
		return Update(Left, operatorToken);
	}
}
