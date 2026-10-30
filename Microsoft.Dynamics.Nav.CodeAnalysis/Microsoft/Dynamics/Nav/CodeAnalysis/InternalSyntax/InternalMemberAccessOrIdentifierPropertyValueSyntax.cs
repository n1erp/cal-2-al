using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalMemberAccessOrIdentifierPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalCodeExpressionSyntax _identifier;

	internal override SyntaxNodeDefinition Definition => MemberAccessOrIdentifierPropertyValueSyntaxDefinition.Instance.Value;

	public InternalCodeExpressionSyntax Identifier => _identifier;

	internal InternalMemberAccessOrIdentifierPropertyValueSyntax(SyntaxKind kind, InternalCodeExpressionSyntax identifier, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal InternalMemberAccessOrIdentifierPropertyValueSyntax(SyntaxKind kind, InternalCodeExpressionSyntax identifier, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal InternalMemberAccessOrIdentifierPropertyValueSyntax(SyntaxKind kind, InternalCodeExpressionSyntax identifier)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _identifier;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new MemberAccessOrIdentifierPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMemberAccessOrIdentifierPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitMemberAccessOrIdentifierPropertyValue(this);
	}

	public InternalMemberAccessOrIdentifierPropertyValueSyntax Update(InternalCodeExpressionSyntax identifier)
	{
		if (identifier != Identifier)
		{
			return InternalSyntaxFactory.MemberAccessOrIdentifierPropertyValue(identifier).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalMemberAccessOrIdentifierPropertyValueSyntax(Kind, _identifier, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalMemberAccessOrIdentifierPropertyValueSyntax(Kind, _identifier, GetDiagnostics(), annotations);
	}
}
