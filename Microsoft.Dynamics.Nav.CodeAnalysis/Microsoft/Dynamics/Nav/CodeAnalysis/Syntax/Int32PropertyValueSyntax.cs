using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class Int32PropertyValueSyntax : LiteralPropertyValueSyntax
{
	private Int32SignedLiteralValueSyntax _value;

	public Int32SignedLiteralValueSyntax Value => GetRedAtZero(ref _value);

	internal Int32PropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitInt32PropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitInt32PropertyValue(this);
	}

	public Int32PropertyValueSyntax Update(Int32SignedLiteralValueSyntax value)
	{
		if (value != Value)
		{
			Int32PropertyValueSyntax int32PropertyValueSyntax = SyntaxFactory.Int32PropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return int32PropertyValueSyntax.WithAnnotations(annotations);
			}
			return int32PropertyValueSyntax;
		}
		return this;
	}

	public Int32PropertyValueSyntax WithValue(Int32SignedLiteralValueSyntax value)
	{
		return Update(value);
	}
}
