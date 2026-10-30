using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class Int32SignedLiteralValueSyntax : LiteralValueSyntax
{
	public SyntaxToken Sign
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalInt32SignedLiteralValueSyntax)Node)._sign;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, Position, 0);
			}
			return default;
		}
	}

	public SyntaxToken Number => new SyntaxToken(this, ((InternalInt32SignedLiteralValueSyntax)Node)._number, GetChildPosition(1), GetChildIndex(1));

	internal Int32SignedLiteralValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitInt32SignedLiteralValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitInt32SignedLiteralValue(this);
	}

	public Int32SignedLiteralValueSyntax Update(SyntaxToken sign, SyntaxToken number)
	{
		if (sign != Sign || number != Number)
		{
			Int32SignedLiteralValueSyntax int32SignedLiteralValueSyntax = SyntaxFactory.Int32SignedLiteralValue(sign, number);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return int32SignedLiteralValueSyntax.WithAnnotations(annotations);
			}
			return int32SignedLiteralValueSyntax;
		}
		return this;
	}

	public Int32SignedLiteralValueSyntax WithSign(SyntaxToken sign)
	{
		return Update(sign, Number);
	}

	public Int32SignedLiteralValueSyntax WithNumber(SyntaxToken number)
	{
		return Update(Sign, number);
	}
}
