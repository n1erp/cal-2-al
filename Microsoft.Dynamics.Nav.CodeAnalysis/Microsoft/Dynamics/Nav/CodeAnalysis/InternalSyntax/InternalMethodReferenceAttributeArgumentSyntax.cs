using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalMethodReferenceAttributeArgumentSyntax : InternalAttributeArgumentSyntax
{
	internal readonly InternalMemberAccessExpressionSyntax _methodReference;

	internal override SyntaxNodeDefinition Definition => MethodReferenceAttributeArgumentSyntaxDefinition.Instance.Value;

	public InternalMemberAccessExpressionSyntax MethodReference => _methodReference;

	internal InternalMethodReferenceAttributeArgumentSyntax(SyntaxKind kind, InternalMemberAccessExpressionSyntax methodReference, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(methodReference);
		_methodReference = methodReference;
	}

	internal InternalMethodReferenceAttributeArgumentSyntax(SyntaxKind kind, InternalMemberAccessExpressionSyntax methodReference, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(methodReference);
		_methodReference = methodReference;
	}

	internal InternalMethodReferenceAttributeArgumentSyntax(SyntaxKind kind, InternalMemberAccessExpressionSyntax methodReference)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(methodReference);
		_methodReference = methodReference;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _methodReference;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new MethodReferenceAttributeArgumentSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMethodReferenceAttributeArgument(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitMethodReferenceAttributeArgument(this);
	}

	public InternalMethodReferenceAttributeArgumentSyntax Update(InternalMemberAccessExpressionSyntax methodReference)
	{
		if (methodReference != MethodReference)
		{
			return InternalSyntaxFactory.MethodReferenceAttributeArgument(methodReference).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalMethodReferenceAttributeArgumentSyntax(Kind, _methodReference, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalMethodReferenceAttributeArgumentSyntax(Kind, _methodReference, GetDiagnostics(), annotations);
	}
}
