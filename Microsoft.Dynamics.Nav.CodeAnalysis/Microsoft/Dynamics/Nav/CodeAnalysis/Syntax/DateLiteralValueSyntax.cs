using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class DateLiteralValueSyntax : LiteralValueSyntax
{
	public SyntaxToken Value => new SyntaxToken(this, ((InternalDateLiteralValueSyntax)Node)._value, Position, 0);

	internal DateLiteralValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitDateLiteralValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDateLiteralValue(this);
	}

	public DateLiteralValueSyntax Update(SyntaxToken value)
	{
		if (value != Value)
		{
			DateLiteralValueSyntax dateLiteralValueSyntax = SyntaxFactory.DateLiteralValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return dateLiteralValueSyntax.WithAnnotations(annotations);
			}
			return dateLiteralValueSyntax;
		}
		return this;
	}

	public DateLiteralValueSyntax WithValue(SyntaxToken value)
	{
		return Update(value);
	}
}
