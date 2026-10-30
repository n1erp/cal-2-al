using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class LabelPropertyValueSyntax : PropertyValueSyntax
{
	private LabelSyntax _value;

	public LabelSyntax Value => GetRedAtZero(ref _value);

	internal LabelPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitLabelPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitLabelPropertyValue(this);
	}

	public LabelPropertyValueSyntax Update(LabelSyntax value)
	{
		if (value != Value)
		{
			LabelPropertyValueSyntax labelPropertyValueSyntax = SyntaxFactory.LabelPropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return labelPropertyValueSyntax.WithAnnotations(annotations);
			}
			return labelPropertyValueSyntax;
		}
		return this;
	}

	public LabelPropertyValueSyntax WithValue(LabelSyntax value)
	{
		return Update(value);
	}
}
