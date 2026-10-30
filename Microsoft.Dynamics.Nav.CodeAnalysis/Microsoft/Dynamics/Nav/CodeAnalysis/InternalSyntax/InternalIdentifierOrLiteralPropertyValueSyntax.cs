using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalIdentifierOrLiteralPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalIdentifierOrLiteralExpressionSyntax _identifier;

	internal override SyntaxNodeDefinition Definition => IdentifierOrLiteralPropertyValueSyntaxDefinition.Instance.Value;

	public InternalIdentifierOrLiteralExpressionSyntax Identifier => _identifier;

	internal InternalIdentifierOrLiteralPropertyValueSyntax(SyntaxKind kind, InternalIdentifierOrLiteralExpressionSyntax identifier, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal InternalIdentifierOrLiteralPropertyValueSyntax(SyntaxKind kind, InternalIdentifierOrLiteralExpressionSyntax identifier, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal InternalIdentifierOrLiteralPropertyValueSyntax(SyntaxKind kind, InternalIdentifierOrLiteralExpressionSyntax identifier)
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
		return new IdentifierOrLiteralPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIdentifierOrLiteralPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitIdentifierOrLiteralPropertyValue(this);
	}

	public InternalIdentifierOrLiteralPropertyValueSyntax Update(InternalIdentifierOrLiteralExpressionSyntax identifier)
	{
		if (identifier != Identifier)
		{
			return InternalSyntaxFactory.IdentifierOrLiteralPropertyValue(identifier).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalIdentifierOrLiteralPropertyValueSyntax(Kind, _identifier, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalIdentifierOrLiteralPropertyValueSyntax(Kind, _identifier, GetDiagnostics(), annotations);
	}
}
