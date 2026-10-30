using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalUnaryExpressionSyntax : InternalCodeExpressionSyntax
{
	internal readonly InternalSyntaxToken _operatorToken;

	internal readonly InternalCodeExpressionSyntax _expression;

	internal override SyntaxNodeDefinition Definition => UnaryExpressionSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken OperatorToken => _operatorToken;

	public InternalCodeExpressionSyntax Expression => _expression;

	internal InternalUnaryExpressionSyntax(SyntaxKind kind, InternalSyntaxToken operatorToken, InternalCodeExpressionSyntax expression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		_operatorToken = operatorToken;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
	}

	internal InternalUnaryExpressionSyntax(SyntaxKind kind, InternalSyntaxToken operatorToken, InternalCodeExpressionSyntax expression, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		_operatorToken = operatorToken;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
	}

	internal InternalUnaryExpressionSyntax(SyntaxKind kind, InternalSyntaxToken operatorToken, InternalCodeExpressionSyntax expression)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		_operatorToken = operatorToken;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_operatorToken, 
			1 => _expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new UnaryExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitUnaryExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitUnaryExpression(this);
	}

	public InternalUnaryExpressionSyntax Update(InternalSyntaxToken operatorToken, InternalCodeExpressionSyntax expression)
	{
		if (operatorToken != OperatorToken || expression != Expression)
		{
			return InternalSyntaxFactory.UnaryExpression(Kind, operatorToken, expression).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalUnaryExpressionSyntax(Kind, _operatorToken, _expression, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalUnaryExpressionSyntax(Kind, _operatorToken, _expression, GetDiagnostics(), annotations);
	}
}
