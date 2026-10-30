using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageFieldReferencePropertyValueSyntax : IdentifierNamePropertyValueSyntax
{
	private IdentifierNameSyntax _value;

	public override IdentifierNameSyntax Value => GetRedAtZero(ref _value);

	internal PageFieldReferencePropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitPageFieldReferencePropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageFieldReferencePropertyValue(this);
	}

	public PageFieldReferencePropertyValueSyntax Update(IdentifierNameSyntax value)
	{
		if (value != Value)
		{
			PageFieldReferencePropertyValueSyntax pageFieldReferencePropertyValueSyntax = SyntaxFactory.PageFieldReferencePropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageFieldReferencePropertyValueSyntax.WithAnnotations(annotations);
			}
			return pageFieldReferencePropertyValueSyntax;
		}
		return this;
	}

	public PageFieldReferencePropertyValueSyntax WithValue(IdentifierNameSyntax value)
	{
		return Update(value);
	}
}
