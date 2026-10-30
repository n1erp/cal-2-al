using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class IdentifierOrLiteralPropertyValueSyntax : PropertyValueSyntax
{
	private IdentifierOrLiteralExpressionSyntax _identifier;

	public IdentifierOrLiteralExpressionSyntax Identifier => GetRedAtZero(ref _identifier);

	internal IdentifierOrLiteralPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _identifier);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _identifier;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIdentifierOrLiteralPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitIdentifierOrLiteralPropertyValue(this);
	}

	public IdentifierOrLiteralPropertyValueSyntax Update(IdentifierOrLiteralExpressionSyntax identifier)
	{
		if (identifier != Identifier)
		{
			IdentifierOrLiteralPropertyValueSyntax identifierOrLiteralPropertyValueSyntax = SyntaxFactory.IdentifierOrLiteralPropertyValue(identifier);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return identifierOrLiteralPropertyValueSyntax.WithAnnotations(annotations);
			}
			return identifierOrLiteralPropertyValueSyntax;
		}
		return this;
	}

	public IdentifierOrLiteralPropertyValueSyntax WithIdentifier(IdentifierOrLiteralExpressionSyntax identifier)
	{
		return Update(identifier);
	}
}
