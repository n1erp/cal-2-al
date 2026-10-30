using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class MemberAccessOrIdentifierPropertyValueSyntax : PropertyValueSyntax
{
	private CodeExpressionSyntax _identifier;

	public CodeExpressionSyntax Identifier => GetRedAtZero(ref _identifier);

	internal MemberAccessOrIdentifierPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _identifier);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _identifier;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMemberAccessOrIdentifierPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitMemberAccessOrIdentifierPropertyValue(this);
	}

	public MemberAccessOrIdentifierPropertyValueSyntax Update(CodeExpressionSyntax identifier)
	{
		if (identifier != Identifier)
		{
			MemberAccessOrIdentifierPropertyValueSyntax memberAccessOrIdentifierPropertyValueSyntax = SyntaxFactory.MemberAccessOrIdentifierPropertyValue(identifier);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return memberAccessOrIdentifierPropertyValueSyntax.WithAnnotations(annotations);
			}
			return memberAccessOrIdentifierPropertyValueSyntax;
		}
		return this;
	}

	public MemberAccessOrIdentifierPropertyValueSyntax WithIdentifier(CodeExpressionSyntax identifier)
	{
		return Update(identifier);
	}
}
