using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class InvalidPropertyValueSyntax : PropertyValueSyntax
{
	public SyntaxToken InvalidToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalInvalidPropertyValueSyntax)Node)._invalidToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, Position, 0);
			}
			return default;
		}
	}

	internal InvalidPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitInvalidPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitInvalidPropertyValue(this);
	}

	public InvalidPropertyValueSyntax Update(SyntaxToken invalidToken)
	{
		if (invalidToken != InvalidToken)
		{
			InvalidPropertyValueSyntax invalidPropertyValueSyntax = SyntaxFactory.InvalidPropertyValue(invalidToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return invalidPropertyValueSyntax.WithAnnotations(annotations);
			}
			return invalidPropertyValueSyntax;
		}
		return this;
	}

	public InvalidPropertyValueSyntax WithInvalidToken(SyntaxToken invalidToken)
	{
		return Update(invalidToken);
	}
}
