using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class DecimalSignedLiteralValueSyntax : LiteralValueSyntax
{
	public SyntaxToken Sign
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalDecimalSignedLiteralValueSyntax)Node)._sign;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, Position, 0);
			}
			return default;
		}
	}

	public SyntaxToken Number => new SyntaxToken(this, ((InternalDecimalSignedLiteralValueSyntax)Node)._number, GetChildPosition(1), GetChildIndex(1));

	internal DecimalSignedLiteralValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitDecimalSignedLiteralValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDecimalSignedLiteralValue(this);
	}

	public DecimalSignedLiteralValueSyntax Update(SyntaxToken sign, SyntaxToken number)
	{
		if (sign != Sign || number != Number)
		{
			DecimalSignedLiteralValueSyntax decimalSignedLiteralValueSyntax = SyntaxFactory.DecimalSignedLiteralValue(sign, number);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return decimalSignedLiteralValueSyntax.WithAnnotations(annotations);
			}
			return decimalSignedLiteralValueSyntax;
		}
		return this;
	}

	public DecimalSignedLiteralValueSyntax WithSign(SyntaxToken sign)
	{
		return Update(sign, Number);
	}

	public DecimalSignedLiteralValueSyntax WithNumber(SyntaxToken number)
	{
		return Update(Sign, number);
	}
}
