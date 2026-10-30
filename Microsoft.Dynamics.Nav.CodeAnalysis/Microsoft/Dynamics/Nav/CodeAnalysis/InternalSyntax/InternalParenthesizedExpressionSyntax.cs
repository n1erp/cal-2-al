using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalParenthesizedExpressionSyntax : InternalCodeExpressionSyntax
{
	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalCodeExpressionSyntax _expression;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => ParenthesizedExpressionSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalCodeExpressionSyntax Expression => _expression;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalParenthesizedExpressionSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalParenthesizedExpressionSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalParenthesizedExpressionSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_openParenthesisToken, 
			1 => _expression, 
			2 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ParenthesizedExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParenthesizedExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedExpression(this);
	}

	public InternalParenthesizedExpressionSyntax Update(InternalSyntaxToken openParenthesisToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken closeParenthesisToken)
	{
		if (openParenthesisToken != OpenParenthesisToken || expression != Expression || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.ParenthesizedExpression(openParenthesisToken, expression, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalParenthesizedExpressionSyntax(Kind, _openParenthesisToken, _expression, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalParenthesizedExpressionSyntax(Kind, _openParenthesisToken, _expression, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
