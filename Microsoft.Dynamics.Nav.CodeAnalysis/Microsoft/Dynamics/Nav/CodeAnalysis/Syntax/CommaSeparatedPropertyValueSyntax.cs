using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class CommaSeparatedPropertyValueSyntax : SeparatedListPropertyValueSyntax
{
	private SyntaxNode _values;

	public SeparatedSyntaxList<IdentifierNameSyntax> Values
	{
		get
		{
			SyntaxNode red = GetRed(ref _values, 0);
			if (red != null)
			{
				return new SeparatedSyntaxList<IdentifierNameSyntax>(red, 0);
			}
			return default;
		}
	}

	internal CommaSeparatedPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitCommaSeparatedPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitCommaSeparatedPropertyValue(this);
	}

	public CommaSeparatedPropertyValueSyntax Update(SeparatedSyntaxList<IdentifierNameSyntax> values)
	{
		if (values != Values)
		{
			CommaSeparatedPropertyValueSyntax commaSeparatedPropertyValueSyntax = SyntaxFactory.CommaSeparatedPropertyValue(values);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return commaSeparatedPropertyValueSyntax.WithAnnotations(annotations);
			}
			return commaSeparatedPropertyValueSyntax;
		}
		return this;
	}

	public CommaSeparatedPropertyValueSyntax WithValues(SeparatedSyntaxList<IdentifierNameSyntax> values)
	{
		return Update(values);
	}

	public CommaSeparatedPropertyValueSyntax AddValues(params IdentifierNameSyntax[] items)
	{
		return Update(Values.AddRange(items));
	}
}
