using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalFieldFilterExpressionSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalIdentifierNameSyntax _leftHandSide;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalSyntaxToken _fieldFormulaKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalSyntaxToken _filterFormulaKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken1;

	internal readonly InternalIdentifierNameSyntax _identifier;

	internal readonly InternalSyntaxToken _closeParenthesisToken1;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => FieldFilterExpressionSyntaxDefinition.Instance.Value;

	public InternalIdentifierNameSyntax LeftHandSide => _leftHandSide;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalSyntaxToken FieldFormulaKeywordToken => _fieldFormulaKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalSyntaxToken FilterFormulaKeywordToken => _filterFormulaKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken1 => _openParenthesisToken1;

	public InternalIdentifierNameSyntax Identifier => _identifier;

	public InternalSyntaxToken CloseParenthesisToken1 => _closeParenthesisToken1;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalFieldFilterExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken1, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(fieldFormulaKeywordToken);
		_fieldFormulaKeywordToken = fieldFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(filterFormulaKeywordToken);
		_filterFormulaKeywordToken = filterFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken1);
		_openParenthesisToken1 = openParenthesisToken1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(closeParenthesisToken1);
		_closeParenthesisToken1 = closeParenthesisToken1;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalFieldFilterExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken1, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(fieldFormulaKeywordToken);
		_fieldFormulaKeywordToken = fieldFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(filterFormulaKeywordToken);
		_filterFormulaKeywordToken = filterFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken1);
		_openParenthesisToken1 = openParenthesisToken1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(closeParenthesisToken1);
		_closeParenthesisToken1 = closeParenthesisToken1;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalFieldFilterExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken1, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(fieldFormulaKeywordToken);
		_fieldFormulaKeywordToken = fieldFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(filterFormulaKeywordToken);
		_filterFormulaKeywordToken = filterFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken1);
		_openParenthesisToken1 = openParenthesisToken1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(closeParenthesisToken1);
		_closeParenthesisToken1 = closeParenthesisToken1;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_leftHandSide, 
			1 => _equalsToken, 
			2 => _fieldFormulaKeywordToken, 
			3 => _openParenthesisToken, 
			4 => _filterFormulaKeywordToken, 
			5 => _openParenthesisToken1, 
			6 => _identifier, 
			7 => _closeParenthesisToken1, 
			8 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new FieldFilterExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldFilterExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitFieldFilterExpression(this);
	}

	public InternalFieldFilterExpressionSyntax Update(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken filterFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken1, InternalSyntaxToken closeParenthesisToken)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || fieldFormulaKeywordToken != FieldFormulaKeywordToken || openParenthesisToken != OpenParenthesisToken || filterFormulaKeywordToken != FilterFormulaKeywordToken || openParenthesisToken1 != OpenParenthesisToken1 || identifier != Identifier || closeParenthesisToken1 != CloseParenthesisToken1 || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.FieldFilterExpression(leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, filterFormulaKeywordToken, openParenthesisToken1, identifier, closeParenthesisToken1, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalFieldFilterExpressionSyntax(Kind, _leftHandSide, _equalsToken, _fieldFormulaKeywordToken, _openParenthesisToken, _filterFormulaKeywordToken, _openParenthesisToken1, _identifier, _closeParenthesisToken1, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalFieldFilterExpressionSyntax(Kind, _leftHandSide, _equalsToken, _fieldFormulaKeywordToken, _openParenthesisToken, _filterFormulaKeywordToken, _openParenthesisToken1, _identifier, _closeParenthesisToken1, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
