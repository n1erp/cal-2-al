using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalFilterExpressionSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalIdentifierNameSyntax _leftHandSide;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalSyntaxToken _filterFormulaKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalFilterExpressionValueSyntax _filter;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => FilterExpressionSyntaxDefinition.Instance.Value;

	public InternalIdentifierNameSyntax LeftHandSide => _leftHandSide;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalSyntaxToken FilterFormulaKeywordToken => _filterFormulaKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalFilterExpressionValueSyntax Filter => _filter;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalFilterExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalFilterExpressionValueSyntax filter, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		if (leftHandSide != null)
		{
			AdjustFlagsAndWidth(leftHandSide);
			_leftHandSide = leftHandSide;
		}
		if (equalsToken != null)
		{
			AdjustFlagsAndWidth(equalsToken);
			_equalsToken = equalsToken;
		}
		AdjustFlagsAndWidth(filterFormulaKeywordToken);
		_filterFormulaKeywordToken = filterFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(filter);
		_filter = filter;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalFilterExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalFilterExpressionValueSyntax filter, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 6;
		if (leftHandSide != null)
		{
			AdjustFlagsAndWidth(leftHandSide);
			_leftHandSide = leftHandSide;
		}
		if (equalsToken != null)
		{
			AdjustFlagsAndWidth(equalsToken);
			_equalsToken = equalsToken;
		}
		AdjustFlagsAndWidth(filterFormulaKeywordToken);
		_filterFormulaKeywordToken = filterFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(filter);
		_filter = filter;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalFilterExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalFilterExpressionValueSyntax filter, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 6;
		if (leftHandSide != null)
		{
			AdjustFlagsAndWidth(leftHandSide);
			_leftHandSide = leftHandSide;
		}
		if (equalsToken != null)
		{
			AdjustFlagsAndWidth(equalsToken);
			_equalsToken = equalsToken;
		}
		AdjustFlagsAndWidth(filterFormulaKeywordToken);
		_filterFormulaKeywordToken = filterFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(filter);
		_filter = filter;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_leftHandSide, 
			1 => _equalsToken, 
			2 => _filterFormulaKeywordToken, 
			3 => _openParenthesisToken, 
			4 => _filter, 
			5 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new FilterExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFilterExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitFilterExpression(this);
	}

	public InternalFilterExpressionSyntax Update(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalFilterExpressionValueSyntax filter, InternalSyntaxToken closeParenthesisToken)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || filterFormulaKeywordToken != FilterFormulaKeywordToken || openParenthesisToken != OpenParenthesisToken || filter != Filter || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.FilterExpression(leftHandSide, equalsToken, filterFormulaKeywordToken, openParenthesisToken, filter, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalFilterExpressionSyntax(Kind, _leftHandSide, _equalsToken, _filterFormulaKeywordToken, _openParenthesisToken, _filter, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalFilterExpressionSyntax(Kind, _leftHandSide, _equalsToken, _filterFormulaKeywordToken, _openParenthesisToken, _filter, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
