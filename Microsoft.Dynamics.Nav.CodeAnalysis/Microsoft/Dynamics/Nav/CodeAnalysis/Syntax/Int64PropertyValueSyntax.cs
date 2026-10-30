using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class Int64PropertyValueSyntax : LiteralPropertyValueSyntax
{
	private Int64SignedLiteralValueSyntax _value;

	public Int64SignedLiteralValueSyntax Value => GetRedAtZero(ref _value);

	internal Int64PropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitInt64PropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitInt64PropertyValue(this);
	}

	public Int64PropertyValueSyntax Update(Int64SignedLiteralValueSyntax value)
	{
		if (value != Value)
		{
			Int64PropertyValueSyntax int64PropertyValueSyntax = SyntaxFactory.Int64PropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return int64PropertyValueSyntax.WithAnnotations(annotations);
			}
			return int64PropertyValueSyntax;
		}
		return this;
	}

	public Int64PropertyValueSyntax WithValue(Int64SignedLiteralValueSyntax value)
	{
		return Update(value);
	}
}
