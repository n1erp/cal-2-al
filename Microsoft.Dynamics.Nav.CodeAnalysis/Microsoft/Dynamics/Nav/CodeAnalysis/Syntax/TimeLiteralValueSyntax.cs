using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class TimeLiteralValueSyntax : LiteralValueSyntax
{
	public SyntaxToken Value => new SyntaxToken(this, ((InternalTimeLiteralValueSyntax)Node)._value, Position, 0);

	internal TimeLiteralValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitTimeLiteralValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTimeLiteralValue(this);
	}

	public TimeLiteralValueSyntax Update(SyntaxToken value)
	{
		if (value != Value)
		{
			TimeLiteralValueSyntax timeLiteralValueSyntax = SyntaxFactory.TimeLiteralValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return timeLiteralValueSyntax.WithAnnotations(annotations);
			}
			return timeLiteralValueSyntax;
		}
		return this;
	}

	public TimeLiteralValueSyntax WithValue(SyntaxToken value)
	{
		return Update(value);
	}
}
