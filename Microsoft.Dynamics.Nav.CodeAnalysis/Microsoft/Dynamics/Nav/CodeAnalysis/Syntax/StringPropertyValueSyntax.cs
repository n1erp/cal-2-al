using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class StringPropertyValueSyntax : LiteralPropertyValueSyntax
{
	private StringLiteralValueSyntax _value;

	public StringLiteralValueSyntax Value => GetRedAtZero(ref _value);

	internal StringPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitStringPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitStringPropertyValue(this);
	}

	public StringPropertyValueSyntax Update(StringLiteralValueSyntax value)
	{
		if (value != Value)
		{
			StringPropertyValueSyntax stringPropertyValueSyntax = SyntaxFactory.StringPropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return stringPropertyValueSyntax.WithAnnotations(annotations);
			}
			return stringPropertyValueSyntax;
		}
		return this;
	}

	public StringPropertyValueSyntax WithValue(StringLiteralValueSyntax value)
	{
		return Update(value);
	}
}
