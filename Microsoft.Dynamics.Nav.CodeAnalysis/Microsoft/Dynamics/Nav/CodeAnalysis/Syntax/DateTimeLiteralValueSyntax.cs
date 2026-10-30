using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class DateTimeLiteralValueSyntax : LiteralValueSyntax
{
	public SyntaxToken Value => new SyntaxToken(this, ((InternalDateTimeLiteralValueSyntax)Node)._value, Position, 0);

	internal DateTimeLiteralValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitDateTimeLiteralValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDateTimeLiteralValue(this);
	}

	public DateTimeLiteralValueSyntax Update(SyntaxToken value)
	{
		if (value != Value)
		{
			DateTimeLiteralValueSyntax dateTimeLiteralValueSyntax = SyntaxFactory.DateTimeLiteralValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return dateTimeLiteralValueSyntax.WithAnnotations(annotations);
			}
			return dateTimeLiteralValueSyntax;
		}
		return this;
	}

	public DateTimeLiteralValueSyntax WithValue(SyntaxToken value)
	{
		return Update(value);
	}
}
