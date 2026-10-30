using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class CommaSeparatedStringsPropertyValueSyntax : SeparatedListPropertyValueSyntax
{
	private SyntaxNode _values;

	public SeparatedSyntaxList<StringLiteralValueSyntax> Values
	{
		get
		{
			SyntaxNode red = GetRed(ref _values, 0);
			if (red != null)
			{
				return new SeparatedSyntaxList<StringLiteralValueSyntax>(red, 0);
			}
			return default;
		}
	}

	internal CommaSeparatedStringsPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _values);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _values;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCommaSeparatedStringsPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitCommaSeparatedStringsPropertyValue(this);
	}

	public CommaSeparatedStringsPropertyValueSyntax Update(SeparatedSyntaxList<StringLiteralValueSyntax> values)
	{
		if (values != Values)
		{
			CommaSeparatedStringsPropertyValueSyntax commaSeparatedStringsPropertyValueSyntax = SyntaxFactory.CommaSeparatedStringsPropertyValue(values);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return commaSeparatedStringsPropertyValueSyntax.WithAnnotations(annotations);
			}
			return commaSeparatedStringsPropertyValueSyntax;
		}
		return this;
	}

	public CommaSeparatedStringsPropertyValueSyntax WithValues(SeparatedSyntaxList<StringLiteralValueSyntax> values)
	{
		return Update(values);
	}

	public CommaSeparatedStringsPropertyValueSyntax AddValues(params StringLiteralValueSyntax[] items)
	{
		return Update(Values.AddRange(items));
	}
}
