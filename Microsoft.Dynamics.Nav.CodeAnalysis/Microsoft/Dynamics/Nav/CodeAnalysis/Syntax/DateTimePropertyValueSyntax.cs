using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class DateTimePropertyValueSyntax : LiteralPropertyValueSyntax
{
	private DateTimeLiteralValueSyntax _value;

	public DateTimeLiteralValueSyntax Value => GetRedAtZero(ref _value);

	internal DateTimePropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitDateTimePropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDateTimePropertyValue(this);
	}

	public DateTimePropertyValueSyntax Update(DateTimeLiteralValueSyntax value)
	{
		if (value != Value)
		{
			DateTimePropertyValueSyntax dateTimePropertyValueSyntax = SyntaxFactory.DateTimePropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return dateTimePropertyValueSyntax.WithAnnotations(annotations);
			}
			return dateTimePropertyValueSyntax;
		}
		return this;
	}

	public DateTimePropertyValueSyntax WithValue(DateTimeLiteralValueSyntax value)
	{
		return Update(value);
	}
}
