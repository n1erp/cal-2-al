using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalBinaryFilterExpressionValueSyntax : InternalFilterExpressionValueSyntax
{
	internal readonly InternalSyntaxNode _left;

	internal readonly InternalSyntaxToken _operatorToken;

	internal readonly InternalSyntaxNode _right;

	internal override SyntaxNodeDefinition Definition => BinaryFilterExpressionValueSyntaxDefinition.Instance.Value;

	public InternalSyntaxNode Left => _left;

	public InternalSyntaxToken OperatorToken => _operatorToken;

	public InternalSyntaxNode Right => _right;

	internal InternalBinaryFilterExpressionValueSyntax(SyntaxKind kind, InternalSyntaxNode left, InternalSyntaxToken operatorToken, InternalSyntaxNode right, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(left);
		_left = left;
		AdjustFlagsAndWidth(operatorToken);
		_operatorToken = operatorToken;
		AdjustFlagsAndWidth(right);
		_right = right;
	}

	internal InternalBinaryFilterExpressionValueSyntax(SyntaxKind kind, InternalSyntaxNode left, InternalSyntaxToken operatorToken, InternalSyntaxNode right, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(left);
		_left = left;
		AdjustFlagsAndWidth(operatorToken);
		_operatorToken = operatorToken;
		AdjustFlagsAndWidth(right);
		_right = right;
	}

	internal InternalBinaryFilterExpressionValueSyntax(SyntaxKind kind, InternalSyntaxNode left, InternalSyntaxToken operatorToken, InternalSyntaxNode right)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(left);
		_left = left;
		AdjustFlagsAndWidth(operatorToken);
		_operatorToken = operatorToken;
		AdjustFlagsAndWidth(right);
		_right = right;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _left, 
			1 => _operatorToken, 
			2 => _right, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new BinaryFilterExpressionValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBinaryFilterExpressionValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitBinaryFilterExpressionValue(this);
	}

	public InternalBinaryFilterExpressionValueSyntax Update(InternalSyntaxNode left, InternalSyntaxToken operatorToken, InternalSyntaxNode right)
	{
		if (left != Left || operatorToken != OperatorToken || right != Right)
		{
			return InternalSyntaxFactory.BinaryFilterExpressionValue(Kind, left, operatorToken, right).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalBinaryFilterExpressionValueSyntax(Kind, _left, _operatorToken, _right, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalBinaryFilterExpressionValueSyntax(Kind, _left, _operatorToken, _right, GetDiagnostics(), annotations);
	}
}
