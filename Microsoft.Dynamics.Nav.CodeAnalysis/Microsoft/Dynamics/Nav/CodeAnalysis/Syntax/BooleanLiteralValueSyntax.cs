using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class BooleanLiteralValueSyntax : LiteralValueSyntax
{
	public SyntaxToken Value => new SyntaxToken(this, ((InternalBooleanLiteralValueSyntax)Node)._value, Position, 0);

	internal BooleanLiteralValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitBooleanLiteralValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBooleanLiteralValue(this);
	}

	public BooleanLiteralValueSyntax Update(SyntaxToken value)
	{
		if (value != Value)
		{
			BooleanLiteralValueSyntax booleanLiteralValueSyntax = SyntaxFactory.BooleanLiteralValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return booleanLiteralValueSyntax.WithAnnotations(annotations);
			}
			return booleanLiteralValueSyntax;
		}
		return this;
	}

	public BooleanLiteralValueSyntax WithValue(SyntaxToken value)
	{
		return Update(value);
	}
}
