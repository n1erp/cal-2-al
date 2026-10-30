using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class DecimalPlacesPropertyValueSyntax : PropertyValueSyntax
{
	private DecimalPlacesSyntax _value;

	public DecimalPlacesSyntax Value => GetRedAtZero(ref _value);

	internal DecimalPlacesPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitDecimalPlacesPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDecimalPlacesPropertyValue(this);
	}

	public DecimalPlacesPropertyValueSyntax Update(DecimalPlacesSyntax value)
	{
		if (value != Value)
		{
			DecimalPlacesPropertyValueSyntax decimalPlacesPropertyValueSyntax = SyntaxFactory.DecimalPlacesPropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return decimalPlacesPropertyValueSyntax.WithAnnotations(annotations);
			}
			return decimalPlacesPropertyValueSyntax;
		}
		return this;
	}

	public DecimalPlacesPropertyValueSyntax WithValue(DecimalPlacesSyntax value)
	{
		return Update(value);
	}
}
