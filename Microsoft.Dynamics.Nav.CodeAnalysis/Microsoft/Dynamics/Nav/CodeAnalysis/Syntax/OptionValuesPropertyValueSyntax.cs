using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class OptionValuesPropertyValueSyntax : LiteralPropertyValueSyntax
{
	private OptionValuesSyntax _value;

	public OptionValuesSyntax Value => GetRedAtZero(ref _value);

	internal OptionValuesPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitOptionValuesPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitOptionValuesPropertyValue(this);
	}

	public OptionValuesPropertyValueSyntax Update(OptionValuesSyntax value)
	{
		if (value != Value)
		{
			OptionValuesPropertyValueSyntax optionValuesPropertyValueSyntax = SyntaxFactory.OptionValuesPropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return optionValuesPropertyValueSyntax.WithAnnotations(annotations);
			}
			return optionValuesPropertyValueSyntax;
		}
		return this;
	}

	public OptionValuesPropertyValueSyntax WithValue(OptionValuesSyntax value)
	{
		return Update(value);
	}

	public OptionValuesPropertyValueSyntax AddValueOptions(params IdentifierNameOrEmptySyntax[] items)
	{
		return Update(Value.WithOptions(Value.Options.AddRange(items)));
	}
}
