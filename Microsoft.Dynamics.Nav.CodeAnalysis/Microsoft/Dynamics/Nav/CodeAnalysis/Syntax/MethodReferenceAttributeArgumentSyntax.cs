using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class MethodReferenceAttributeArgumentSyntax : AttributeArgumentSyntax
{
	private MemberAccessExpressionSyntax _methodReference;

	public MemberAccessExpressionSyntax MethodReference => GetRedAtZero(ref _methodReference);

	internal MethodReferenceAttributeArgumentSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _methodReference);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _methodReference;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMethodReferenceAttributeArgument(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitMethodReferenceAttributeArgument(this);
	}

	public MethodReferenceAttributeArgumentSyntax Update(MemberAccessExpressionSyntax methodReference)
	{
		if (methodReference != MethodReference)
		{
			MethodReferenceAttributeArgumentSyntax methodReferenceAttributeArgumentSyntax = SyntaxFactory.MethodReferenceAttributeArgument(methodReference);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return methodReferenceAttributeArgumentSyntax.WithAnnotations(annotations);
			}
			return methodReferenceAttributeArgumentSyntax;
		}
		return this;
	}

	public MethodReferenceAttributeArgumentSyntax WithMethodReference(MemberAccessExpressionSyntax methodReference)
	{
		return Update(methodReference);
	}
}
