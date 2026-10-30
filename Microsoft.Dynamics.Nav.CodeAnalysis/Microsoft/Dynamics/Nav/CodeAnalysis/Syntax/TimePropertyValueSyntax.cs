using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class TimePropertyValueSyntax : LiteralPropertyValueSyntax
{
	private TimeLiteralValueSyntax _value;

	public TimeLiteralValueSyntax Value => GetRedAtZero(ref _value);

	internal TimePropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitTimePropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTimePropertyValue(this);
	}

	public TimePropertyValueSyntax Update(TimeLiteralValueSyntax value)
	{
		if (value != Value)
		{
			TimePropertyValueSyntax timePropertyValueSyntax = SyntaxFactory.TimePropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return timePropertyValueSyntax.WithAnnotations(annotations);
			}
			return timePropertyValueSyntax;
		}
		return this;
	}

	public TimePropertyValueSyntax WithValue(TimeLiteralValueSyntax value)
	{
		return Update(value);
	}
}
