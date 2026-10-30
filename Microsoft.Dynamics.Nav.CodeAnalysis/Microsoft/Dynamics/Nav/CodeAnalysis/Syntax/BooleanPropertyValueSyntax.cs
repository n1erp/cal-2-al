using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class BooleanPropertyValueSyntax : LiteralPropertyValueSyntax
{
	private BooleanLiteralValueSyntax _value;

	public BooleanLiteralValueSyntax Value => GetRedAtZero(ref _value);

	internal BooleanPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitBooleanPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBooleanPropertyValue(this);
	}

	public BooleanPropertyValueSyntax Update(BooleanLiteralValueSyntax value)
	{
		if (value != Value)
		{
			BooleanPropertyValueSyntax booleanPropertyValueSyntax = SyntaxFactory.BooleanPropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return booleanPropertyValueSyntax.WithAnnotations(annotations);
			}
			return booleanPropertyValueSyntax;
		}
		return this;
	}

	public BooleanPropertyValueSyntax WithValue(BooleanLiteralValueSyntax value)
	{
		return Update(value);
	}
}
