using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalFieldUpperLimitFilterExpressionSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalIdentifierNameSyntax _leftHandSide;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalSyntaxToken _fieldFormulaKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken1;

	internal readonly InternalSyntaxToken _upperLimitFormulaKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken2;

	internal readonly InternalSyntaxToken _filterFormulaKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken3;

	internal readonly InternalIdentifierNameSyntax _identifier;

	internal readonly InternalSyntaxToken _closeParenthesisToken3;

	internal readonly InternalSyntaxToken _closeParenthesisToken2;

	internal readonly InternalSyntaxToken _closeParenthesisToken1;

	internal override SyntaxNodeDefinition Definition => FieldUpperLimitFilterExpressionSyntaxDefinition.Instance.Value;

	public InternalIdentifierNameSyntax LeftHandSide => _leftHandSide;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalSyntaxToken FieldFormulaKeywordToken => _fieldFormulaKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken1 => _openParenthesisToken1;

	public InternalSyntaxToken UpperLimitFormulaKeywordToken => _upperLimitFormulaKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken2 => _openParenthesisToken2;

	public InternalSyntaxToken FilterFormulaKeywordToken => _filterFormulaKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken3 => _openParenthesisToken3;

	public InternalIdentifierNameSyntax Identifier => _identifier;

	public InternalSyntaxToken CloseParenthesisToken3 => _closeParenthesisToken3;

	public InternalSyntaxToken CloseParenthesisToken2 => _closeParenthesisToken2;

	public InternalSyntaxToken CloseParenthesisToken1 => _closeParenthesisToken1;

	internal InternalFieldUpperLimitFilterExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalSyntaxToken upperLimitFormulaKeywordToken, InternalSyntaxToken openParenthesisToken2, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken3, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken3, InternalSyntaxToken closeParenthesisToken2, InternalSyntaxToken closeParenthesisToken1, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 12;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(fieldFormulaKeywordToken);
		_fieldFormulaKeywordToken = fieldFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken1);
		_openParenthesisToken1 = openParenthesisToken1;
		AdjustFlagsAndWidth(upperLimitFormulaKeywordToken);
		_upperLimitFormulaKeywordToken = upperLimitFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken2);
		_openParenthesisToken2 = openParenthesisToken2;
		AdjustFlagsAndWidth(filterFormulaKeywordToken);
		_filterFormulaKeywordToken = filterFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken3);
		_openParenthesisToken3 = openParenthesisToken3;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(closeParenthesisToken3);
		_closeParenthesisToken3 = closeParenthesisToken3;
		AdjustFlagsAndWidth(closeParenthesisToken2);
		_closeParenthesisToken2 = closeParenthesisToken2;
		AdjustFlagsAndWidth(closeParenthesisToken1);
		_closeParenthesisToken1 = closeParenthesisToken1;
	}

	internal InternalFieldUpperLimitFilterExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalSyntaxToken upperLimitFormulaKeywordToken, InternalSyntaxToken openParenthesisToken2, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken3, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken3, InternalSyntaxToken closeParenthesisToken2, InternalSyntaxToken closeParenthesisToken1, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 12;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(fieldFormulaKeywordToken);
		_fieldFormulaKeywordToken = fieldFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken1);
		_openParenthesisToken1 = openParenthesisToken1;
		AdjustFlagsAndWidth(upperLimitFormulaKeywordToken);
		_upperLimitFormulaKeywordToken = upperLimitFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken2);
		_openParenthesisToken2 = openParenthesisToken2;
		AdjustFlagsAndWidth(filterFormulaKeywordToken);
		_filterFormulaKeywordToken = filterFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken3);
		_openParenthesisToken3 = openParenthesisToken3;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(closeParenthesisToken3);
		_closeParenthesisToken3 = closeParenthesisToken3;
		AdjustFlagsAndWidth(closeParenthesisToken2);
		_closeParenthesisToken2 = closeParenthesisToken2;
		AdjustFlagsAndWidth(closeParenthesisToken1);
		_closeParenthesisToken1 = closeParenthesisToken1;
	}

	internal InternalFieldUpperLimitFilterExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalSyntaxToken upperLimitFormulaKeywordToken, InternalSyntaxToken openParenthesisToken2, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken3, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken3, InternalSyntaxToken closeParenthesisToken2, InternalSyntaxToken closeParenthesisToken1)
		: base(kind)
	{
		SlotCount = 12;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(fieldFormulaKeywordToken);
		_fieldFormulaKeywordToken = fieldFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken1);
		_openParenthesisToken1 = openParenthesisToken1;
		AdjustFlagsAndWidth(upperLimitFormulaKeywordToken);
		_upperLimitFormulaKeywordToken = upperLimitFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken2);
		_openParenthesisToken2 = openParenthesisToken2;
		AdjustFlagsAndWidth(filterFormulaKeywordToken);
		_filterFormulaKeywordToken = filterFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken3);
		_openParenthesisToken3 = openParenthesisToken3;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(closeParenthesisToken3);
		_closeParenthesisToken3 = closeParenthesisToken3;
		AdjustFlagsAndWidth(closeParenthesisToken2);
		_closeParenthesisToken2 = closeParenthesisToken2;
		AdjustFlagsAndWidth(closeParenthesisToken1);
		_closeParenthesisToken1 = closeParenthesisToken1;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_leftHandSide, 
			1 => _equalsToken, 
			2 => _fieldFormulaKeywordToken, 
			3 => _openParenthesisToken1, 
			4 => _upperLimitFormulaKeywordToken, 
			5 => _openParenthesisToken2, 
			6 => _filterFormulaKeywordToken, 
			7 => _openParenthesisToken3, 
			8 => _identifier, 
			9 => _closeParenthesisToken3, 
			10 => _closeParenthesisToken2, 
			11 => _closeParenthesisToken1, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new FieldUpperLimitFilterExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldUpperLimitFilterExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitFieldUpperLimitFilterExpression(this);
	}

	public InternalFieldUpperLimitFilterExpressionSyntax Update(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalSyntaxToken upperLimitFormulaKeywordToken, InternalSyntaxToken openParenthesisToken2, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken3, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken3, InternalSyntaxToken closeParenthesisToken2, InternalSyntaxToken closeParenthesisToken1)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || fieldFormulaKeywordToken != FieldFormulaKeywordToken || openParenthesisToken1 != OpenParenthesisToken1 || upperLimitFormulaKeywordToken != UpperLimitFormulaKeywordToken || openParenthesisToken2 != OpenParenthesisToken2 || filterFormulaKeywordToken != FilterFormulaKeywordToken || openParenthesisToken3 != OpenParenthesisToken3 || identifier != Identifier || closeParenthesisToken3 != CloseParenthesisToken3 || closeParenthesisToken2 != CloseParenthesisToken2 || closeParenthesisToken1 != CloseParenthesisToken1)
		{
			return InternalSyntaxFactory.FieldUpperLimitFilterExpression(leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken1, upperLimitFormulaKeywordToken, openParenthesisToken2, filterFormulaKeywordToken, openParenthesisToken3, identifier, closeParenthesisToken3, closeParenthesisToken2, closeParenthesisToken1).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalFieldUpperLimitFilterExpressionSyntax(Kind, _leftHandSide, _equalsToken, _fieldFormulaKeywordToken, _openParenthesisToken1, _upperLimitFormulaKeywordToken, _openParenthesisToken2, _filterFormulaKeywordToken, _openParenthesisToken3, _identifier, _closeParenthesisToken3, _closeParenthesisToken2, _closeParenthesisToken1, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalFieldUpperLimitFilterExpressionSyntax(Kind, _leftHandSide, _equalsToken, _fieldFormulaKeywordToken, _openParenthesisToken1, _upperLimitFormulaKeywordToken, _openParenthesisToken2, _filterFormulaKeywordToken, _openParenthesisToken3, _identifier, _closeParenthesisToken3, _closeParenthesisToken2, _closeParenthesisToken1, GetDiagnostics(), annotations);
	}
}
