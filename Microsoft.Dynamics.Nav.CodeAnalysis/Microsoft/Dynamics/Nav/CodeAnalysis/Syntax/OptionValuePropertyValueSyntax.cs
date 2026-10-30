using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class OptionValuePropertyValueSyntax : IdentifierNamePropertyValueSyntax
{
	private IdentifierNameSyntax _value;

	public override IdentifierNameSyntax Value => GetRedAtZero(ref _value);

	internal OptionValuePropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitOptionValuePropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitOptionValuePropertyValue(this);
	}

	public OptionValuePropertyValueSyntax Update(IdentifierNameSyntax value)
	{
		if (value != Value)
		{
			OptionValuePropertyValueSyntax optionValuePropertyValueSyntax = SyntaxFactory.OptionValuePropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return optionValuePropertyValueSyntax.WithAnnotations(annotations);
			}
			return optionValuePropertyValueSyntax;
		}
		return this;
	}

	public OptionValuePropertyValueSyntax WithValue(IdentifierNameSyntax value)
	{
		return Update(value);
	}
}
