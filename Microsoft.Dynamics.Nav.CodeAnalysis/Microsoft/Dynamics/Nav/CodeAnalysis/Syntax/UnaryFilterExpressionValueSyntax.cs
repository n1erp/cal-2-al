using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class UnaryFilterExpressionValueSyntax : FilterExpressionValueSyntax
{
	private IdentifierOrLiteralExpressionSyntax _identifier;

	public SyntaxToken OperatorToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalUnaryFilterExpressionValueSyntax)Node)._operatorToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, Position, 0);
			}
			return default;
		}
	}

	public IdentifierOrLiteralExpressionSyntax Identifier => GetRed(ref _identifier, 1);

	internal UnaryFilterExpressionValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _identifier, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _identifier;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitUnaryFilterExpressionValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitUnaryFilterExpressionValue(this);
	}

	public UnaryFilterExpressionValueSyntax Update(SyntaxToken operatorToken, IdentifierOrLiteralExpressionSyntax identifier)
	{
		if (operatorToken != OperatorToken || identifier != Identifier)
		{
			UnaryFilterExpressionValueSyntax unaryFilterExpressionValueSyntax = SyntaxFactory.UnaryFilterExpressionValue(Kind, operatorToken, identifier);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return unaryFilterExpressionValueSyntax.WithAnnotations(annotations);
			}
			return unaryFilterExpressionValueSyntax;
		}
		return this;
	}

	public UnaryFilterExpressionValueSyntax WithOperatorToken(SyntaxToken operatorToken)
	{
		return Update(operatorToken, Identifier);
	}

	public UnaryFilterExpressionValueSyntax WithIdentifier(IdentifierOrLiteralExpressionSyntax identifier)
	{
		return Update(OperatorToken, identifier);
	}
}
