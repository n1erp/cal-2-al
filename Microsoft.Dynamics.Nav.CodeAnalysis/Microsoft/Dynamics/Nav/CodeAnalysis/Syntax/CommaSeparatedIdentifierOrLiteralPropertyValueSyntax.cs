using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class CommaSeparatedIdentifierOrLiteralPropertyValueSyntax : SeparatedListPropertyValueSyntax
{
	private SyntaxNode _values;

	public SeparatedSyntaxList<IdentifierOrLiteralExpressionSyntax> Values
	{
		get
		{
			SyntaxNode red = GetRed(ref _values, 0);
			if (red != null)
			{
				return new SeparatedSyntaxList<IdentifierOrLiteralExpressionSyntax>(red, 0);
			}
			return default;
		}
	}

	internal CommaSeparatedIdentifierOrLiteralPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitCommaSeparatedIdentifierOrLiteralPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitCommaSeparatedIdentifierOrLiteralPropertyValue(this);
	}

	public CommaSeparatedIdentifierOrLiteralPropertyValueSyntax Update(SeparatedSyntaxList<IdentifierOrLiteralExpressionSyntax> values)
	{
		if (values != Values)
		{
			CommaSeparatedIdentifierOrLiteralPropertyValueSyntax commaSeparatedIdentifierOrLiteralPropertyValueSyntax = SyntaxFactory.CommaSeparatedIdentifierOrLiteralPropertyValue(values);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return commaSeparatedIdentifierOrLiteralPropertyValueSyntax.WithAnnotations(annotations);
			}
			return commaSeparatedIdentifierOrLiteralPropertyValueSyntax;
		}
		return this;
	}

	public CommaSeparatedIdentifierOrLiteralPropertyValueSyntax WithValues(SeparatedSyntaxList<IdentifierOrLiteralExpressionSyntax> values)
	{
		return Update(values);
	}

	public CommaSeparatedIdentifierOrLiteralPropertyValueSyntax AddValues(params IdentifierOrLiteralExpressionSyntax[] items)
	{
		return Update(Values.AddRange(items));
	}
}
