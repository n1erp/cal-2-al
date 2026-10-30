using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalLiteralAttributeArgumentSyntax : InternalAttributeArgumentSyntax
{
	internal readonly InternalLiteralExpressionSyntax _literal;

	internal override SyntaxNodeDefinition Definition => LiteralAttributeArgumentSyntaxDefinition.Instance.Value;

	public InternalLiteralExpressionSyntax Literal => _literal;

	internal InternalLiteralAttributeArgumentSyntax(SyntaxKind kind, InternalLiteralExpressionSyntax literal, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(literal);
		_literal = literal;
	}

	internal InternalLiteralAttributeArgumentSyntax(SyntaxKind kind, InternalLiteralExpressionSyntax literal, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(literal);
		_literal = literal;
	}

	internal InternalLiteralAttributeArgumentSyntax(SyntaxKind kind, InternalLiteralExpressionSyntax literal)
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
		return new LiteralAttributeArgumentSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLiteralAttributeArgument(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitLiteralAttributeArgument(this);
	}

	public InternalLiteralAttributeArgumentSyntax Update(InternalLiteralExpressionSyntax literal)
	{
		if (literal != Literal)
		{
			return InternalSyntaxFactory.LiteralAttributeArgument(literal).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalLiteralAttributeArgumentSyntax(Kind, _literal, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalLiteralAttributeArgumentSyntax(Kind, _literal, GetDiagnostics(), annotations);
	}
}
