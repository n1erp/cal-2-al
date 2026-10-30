using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalRangeFromFilterExpressionValueSyntax : InternalFilterExpressionValueSyntax
{
	internal readonly InternalIdentifierOrLiteralExpressionSyntax _left;

	internal readonly InternalSyntaxToken _operatorToken;

	internal override SyntaxNodeDefinition Definition => RangeFromFilterExpressionValueSyntaxDefinition.Instance.Value;

	public InternalIdentifierOrLiteralExpressionSyntax Left => _left;

	public InternalSyntaxToken OperatorToken => _operatorToken;

	internal InternalRangeFromFilterExpressionValueSyntax(SyntaxKind kind, InternalIdentifierOrLiteralExpressionSyntax left, InternalSyntaxToken operatorToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(left);
		_left = left;
		AdjustFlagsAndWidth(operatorToken);
		_operatorToken = operatorToken;
	}

	internal InternalRangeFromFilterExpressionValueSyntax(SyntaxKind kind, InternalIdentifierOrLiteralExpressionSyntax left, InternalSyntaxToken operatorToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(left);
		_left = left;
		AdjustFlagsAndWidth(operatorToken);
		_operatorToken = operatorToken;
	}

	internal InternalRangeFromFilterExpressionValueSyntax(SyntaxKind kind, InternalIdentifierOrLiteralExpressionSyntax left, InternalSyntaxToken operatorToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(left);
		_left = left;
		AdjustFlagsAndWidth(operatorToken);
		_operatorToken = operatorToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_left, 
			1 => _operatorToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new RangeFromFilterExpressionValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRangeFromFilterExpressionValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitRangeFromFilterExpressionValue(this);
	}

	public InternalRangeFromFilterExpressionValueSyntax Update(InternalIdentifierOrLiteralExpressionSyntax left, InternalSyntaxToken operatorToken)
	{
		if (left != Left || operatorToken != OperatorToken)
		{
			return InternalSyntaxFactory.RangeFromFilterExpressionValue(left, operatorToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalRangeFromFilterExpressionValueSyntax(Kind, _left, _operatorToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalRangeFromFilterExpressionValueSyntax(Kind, _left, _operatorToken, GetDiagnostics(), annotations);
	}
}
