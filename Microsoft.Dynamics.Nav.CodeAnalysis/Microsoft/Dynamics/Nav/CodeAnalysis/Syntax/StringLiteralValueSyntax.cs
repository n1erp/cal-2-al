using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class StringLiteralValueSyntax : LiteralValueSyntax
{
	public SyntaxToken Value => new SyntaxToken(this, ((InternalStringLiteralValueSyntax)Node)._value, Position, 0);

	internal StringLiteralValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitStringLiteralValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitStringLiteralValue(this);
	}

	public StringLiteralValueSyntax Update(SyntaxToken value)
	{
		if (value != Value)
		{
			StringLiteralValueSyntax stringLiteralValueSyntax = SyntaxFactory.StringLiteralValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return stringLiteralValueSyntax.WithAnnotations(annotations);
			}
			return stringLiteralValueSyntax;
		}
		return this;
	}

	public StringLiteralValueSyntax WithValue(SyntaxToken value)
	{
		return Update(value);
	}
}
