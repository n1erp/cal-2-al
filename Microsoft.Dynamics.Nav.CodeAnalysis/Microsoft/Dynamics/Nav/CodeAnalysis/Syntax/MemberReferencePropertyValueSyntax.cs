using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class MemberReferencePropertyValueSyntax : IdentifierNamePropertyValueSyntax
{
	private IdentifierNameSyntax _value;

	public override IdentifierNameSyntax Value => GetRedAtZero(ref _value);

	internal MemberReferencePropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitMemberReferencePropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitMemberReferencePropertyValue(this);
	}

	public MemberReferencePropertyValueSyntax Update(IdentifierNameSyntax value)
	{
		if (value != Value)
		{
			MemberReferencePropertyValueSyntax memberReferencePropertyValueSyntax = SyntaxFactory.MemberReferencePropertyValue(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return memberReferencePropertyValueSyntax.WithAnnotations(annotations);
			}
			return memberReferencePropertyValueSyntax;
		}
		return this;
	}

	public MemberReferencePropertyValueSyntax WithValue(IdentifierNameSyntax value)
	{
		return Update(value);
	}
}
