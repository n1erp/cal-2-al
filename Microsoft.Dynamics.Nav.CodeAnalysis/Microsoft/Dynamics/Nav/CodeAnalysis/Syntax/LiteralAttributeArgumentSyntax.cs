using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class LiteralAttributeArgumentSyntax : AttributeArgumentSyntax
{
	private LiteralExpressionSyntax _literal;

	public LiteralExpressionSyntax Literal => GetRedAtZero(ref _literal);

	internal LiteralAttributeArgumentSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitLiteralAttributeArgument(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitLiteralAttributeArgument(this);
	}

	public LiteralAttributeArgumentSyntax Update(LiteralExpressionSyntax literal)
	{
		if (literal != Literal)
		{
			LiteralAttributeArgumentSyntax literalAttributeArgumentSyntax = SyntaxFactory.LiteralAttributeArgument(literal);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return literalAttributeArgumentSyntax.WithAnnotations(annotations);
			}
			return literalAttributeArgumentSyntax;
		}
		return this;
	}

	public LiteralAttributeArgumentSyntax WithLiteral(LiteralExpressionSyntax literal)
	{
		return Update(literal);
	}
}
