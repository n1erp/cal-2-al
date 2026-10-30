using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ImagePropertyValueSyntax : IdentifierNamePropertyValueSyntax
{
	private IdentifierNameSyntax _value;

	public override IdentifierNameSyntax Value => GetRedAtZero(ref _value);

	internal ImagePropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitImagePropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitImagePropertyValue(this);
	}

	public ImagePropertyValueSyntax Update(IdentifierNameSyntax value)
	{
		if (value != Value)
		{
			ImagePropertyValueSyntax imagePropertyValueSyntax = SyntaxFactory.ImagePropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return imagePropertyValueSyntax.WithAnnotations(annotations);
			}
			return imagePropertyValueSyntax;
		}
		return this;
	}

	public ImagePropertyValueSyntax WithValue(IdentifierNameSyntax value)
	{
		return Update(value);
	}
}
