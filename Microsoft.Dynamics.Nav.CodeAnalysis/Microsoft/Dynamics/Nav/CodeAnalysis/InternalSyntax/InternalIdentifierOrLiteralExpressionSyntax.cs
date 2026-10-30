using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalIdentifierOrLiteralExpressionSyntax : InternalSyntaxNode
{
	internal readonly InternalCodeExpressionSyntax _identifier;

	internal override SyntaxNodeDefinition Definition => IdentifierOrLiteralExpressionSyntaxDefinition.Instance.Value;

	public InternalCodeExpressionSyntax Identifier => _identifier;

	internal InternalIdentifierOrLiteralExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax identifier, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal InternalIdentifierOrLiteralExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax identifier, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
	}

	internal InternalIdentifierOrLiteralExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax identifier)
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
		return new IdentifierOrLiteralExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIdentifierOrLiteralExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitIdentifierOrLiteralExpression(this);
	}

	public InternalIdentifierOrLiteralExpressionSyntax Update(InternalCodeExpressionSyntax identifier)
	{
		if (identifier != Identifier)
		{
			return InternalSyntaxFactory.IdentifierOrLiteralExpression(identifier).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalIdentifierOrLiteralExpressionSyntax(Kind, _identifier, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalIdentifierOrLiteralExpressionSyntax(Kind, _identifier, GetDiagnostics(), annotations);
	}
}
