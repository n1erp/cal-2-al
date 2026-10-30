using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalBinaryExpressionSyntax : InternalCodeExpressionSyntax
{
	internal readonly InternalCodeExpressionSyntax _left;

	internal readonly InternalSyntaxToken _operatorToken;

	internal readonly InternalCodeExpressionSyntax _right;

	internal override SyntaxNodeDefinition Definition => BinaryExpressionSyntaxDefinition.Instance.Value;

	public InternalCodeExpressionSyntax Left => _left;

	public InternalSyntaxToken OperatorToken => _operatorToken;

	public InternalCodeExpressionSyntax Right => _right;

	internal InternalBinaryExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax left, InternalSyntaxToken operatorToken, InternalCodeExpressionSyntax right, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
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

	internal InternalBinaryExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax left, InternalSyntaxToken operatorToken, InternalCodeExpressionSyntax right, LexerMode mode)
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

	internal InternalBinaryExpressionSyntax(SyntaxKind kind, InternalCodeExpressionSyntax left, InternalSyntaxToken operatorToken, InternalCodeExpressionSyntax right)
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
			0 => (InternalSyntaxNode)_left, 
			1 => _operatorToken, 
			2 => _right, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new BinaryExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBinaryExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitBinaryExpression(this);
	}

	public InternalBinaryExpressionSyntax Update(InternalCodeExpressionSyntax left, InternalSyntaxToken operatorToken, InternalCodeExpressionSyntax right)
	{
		if (left != Left || operatorToken != OperatorToken || right != Right)
		{
			return InternalSyntaxFactory.BinaryExpression(Kind, left, operatorToken, right).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalBinaryExpressionSyntax(Kind, _left, _operatorToken, _right, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalBinaryExpressionSyntax(Kind, _left, _operatorToken, _right, GetDiagnostics(), annotations);
	}
}
