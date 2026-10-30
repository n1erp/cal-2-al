using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalParenthesizedFilterExpressionValueSyntax : InternalFilterExpressionValueSyntax
{
	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalFilterExpressionValueSyntax _expression;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => ParenthesizedFilterExpressionValueSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalFilterExpressionValueSyntax Expression => _expression;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalParenthesizedFilterExpressionValueSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalFilterExpressionValueSyntax expression, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
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

	internal InternalParenthesizedFilterExpressionValueSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalFilterExpressionValueSyntax expression, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
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

	internal InternalParenthesizedFilterExpressionValueSyntax(SyntaxKind kind, InternalSyntaxToken openParenthesisToken, InternalFilterExpressionValueSyntax expression, InternalSyntaxToken closeParenthesisToken)
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
		return new ParenthesizedFilterExpressionValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParenthesizedFilterExpressionValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedFilterExpressionValue(this);
	}

	public InternalParenthesizedFilterExpressionValueSyntax Update(InternalSyntaxToken openParenthesisToken, InternalFilterExpressionValueSyntax expression, InternalSyntaxToken closeParenthesisToken)
	{
		if (openParenthesisToken != OpenParenthesisToken || expression != Expression || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.ParenthesizedFilterExpressionValue(openParenthesisToken, expression, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalParenthesizedFilterExpressionValueSyntax(Kind, _openParenthesisToken, _expression, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalParenthesizedFilterExpressionValueSyntax(Kind, _openParenthesisToken, _expression, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
