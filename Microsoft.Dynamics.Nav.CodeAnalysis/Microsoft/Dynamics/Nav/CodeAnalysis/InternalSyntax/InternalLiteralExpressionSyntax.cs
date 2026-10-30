using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalLiteralExpressionSyntax : InternalCodeExpressionSyntax
{
	internal readonly InternalLiteralValueSyntax _literal;

	internal override SyntaxNodeDefinition Definition => LiteralExpressionSyntaxDefinition.Instance.Value;

	public InternalLiteralValueSyntax Literal => _literal;

	internal InternalLiteralExpressionSyntax(SyntaxKind kind, InternalLiteralValueSyntax literal, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(literal);
		_literal = literal;
	}

	internal InternalLiteralExpressionSyntax(SyntaxKind kind, InternalLiteralValueSyntax literal, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(literal);
		_literal = literal;
	}

	internal InternalLiteralExpressionSyntax(SyntaxKind kind, InternalLiteralValueSyntax literal)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(literal);
		_literal = literal;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _literal;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new LiteralExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLiteralExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitLiteralExpression(this);
	}

	public InternalLiteralExpressionSyntax Update(InternalLiteralValueSyntax literal)
	{
		if (literal != Literal)
		{
			return InternalSyntaxFactory.LiteralExpression(literal).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalLiteralExpressionSyntax(Kind, _literal, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalLiteralExpressionSyntax(Kind, _literal, GetDiagnostics(), annotations);
	}
}
