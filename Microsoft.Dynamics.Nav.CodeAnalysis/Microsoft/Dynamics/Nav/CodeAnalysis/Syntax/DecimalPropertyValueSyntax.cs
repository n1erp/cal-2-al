using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class DecimalPropertyValueSyntax : LiteralPropertyValueSyntax
{
	private DecimalSignedLiteralValueSyntax _value;

	public DecimalSignedLiteralValueSyntax Value => GetRedAtZero(ref _value);

	internal DecimalPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _value);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _value;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDecimalPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDecimalPropertyValue(this);
	}

	public DecimalPropertyValueSyntax Update(DecimalSignedLiteralValueSyntax value)
	{
		if (value != Value)
		{
			DecimalPropertyValueSyntax decimalPropertyValueSyntax = SyntaxFactory.DecimalPropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return decimalPropertyValueSyntax.WithAnnotations(annotations);
			}
			return decimalPropertyValueSyntax;
		}
		return this;
	}

	public DecimalPropertyValueSyntax WithValue(DecimalSignedLiteralValueSyntax value)
	{
		return Update(value);
	}
}
