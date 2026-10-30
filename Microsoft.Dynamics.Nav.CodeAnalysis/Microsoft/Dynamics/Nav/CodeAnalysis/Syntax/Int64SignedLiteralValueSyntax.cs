using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class Int64SignedLiteralValueSyntax : LiteralValueSyntax
{
	public SyntaxToken Sign
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalInt64SignedLiteralValueSyntax)Node)._sign;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, Position, 0);
			}
			return default;
		}
	}

	public SyntaxToken Number => new SyntaxToken(this, ((InternalInt64SignedLiteralValueSyntax)Node)._number, GetChildPosition(1), GetChildIndex(1));

	internal Int64SignedLiteralValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitInt64SignedLiteralValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitInt64SignedLiteralValue(this);
	}

	public Int64SignedLiteralValueSyntax Update(SyntaxToken sign, SyntaxToken number)
	{
		if (sign != Sign || number != Number)
		{
			Int64SignedLiteralValueSyntax int64SignedLiteralValueSyntax = SyntaxFactory.Int64SignedLiteralValue(sign, number);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return int64SignedLiteralValueSyntax.WithAnnotations(annotations);
			}
			return int64SignedLiteralValueSyntax;
		}
		return this;
	}

	public Int64SignedLiteralValueSyntax WithSign(SyntaxToken sign)
	{
		return Update(sign, Number);
	}

	public Int64SignedLiteralValueSyntax WithNumber(SyntaxToken number)
	{
		return Update(Sign, number);
	}
}
