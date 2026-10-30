using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class InvalidFilterExpressionValueSyntax : FilterExpressionValueSyntax
{
	public SyntaxToken InvalidToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalInvalidFilterExpressionValueSyntax)Node)._invalidToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, Position, 0);
			}
			return default;
		}
	}

	internal InvalidFilterExpressionValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInvalidFilterExpressionValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitInvalidFilterExpressionValue(this);
	}

	public InvalidFilterExpressionValueSyntax Update(SyntaxToken invalidToken)
	{
		if (invalidToken != InvalidToken)
		{
			InvalidFilterExpressionValueSyntax invalidFilterExpressionValueSyntax = SyntaxFactory.InvalidFilterExpressionValue(invalidToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return invalidFilterExpressionValueSyntax.WithAnnotations(annotations);
			}
			return invalidFilterExpressionValueSyntax;
		}
		return this;
	}

	public InvalidFilterExpressionValueSyntax WithInvalidToken(SyntaxToken invalidToken)
	{
		return Update(invalidToken);
	}
}
