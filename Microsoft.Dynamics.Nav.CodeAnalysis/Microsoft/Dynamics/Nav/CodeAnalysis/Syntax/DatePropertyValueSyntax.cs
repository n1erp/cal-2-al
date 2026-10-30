using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class DatePropertyValueSyntax : LiteralPropertyValueSyntax
{
	private DateLiteralValueSyntax _value;

	public DateLiteralValueSyntax Value => GetRedAtZero(ref _value);

	internal DatePropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitDatePropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDatePropertyValue(this);
	}

	public DatePropertyValueSyntax Update(DateLiteralValueSyntax value)
	{
		if (value != Value)
		{
			DatePropertyValueSyntax datePropertyValueSyntax = SyntaxFactory.DatePropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return datePropertyValueSyntax.WithAnnotations(annotations);
			}
			return datePropertyValueSyntax;
		}
		return this;
	}

	public DatePropertyValueSyntax WithValue(DateLiteralValueSyntax value)
	{
		return Update(value);
	}
}
