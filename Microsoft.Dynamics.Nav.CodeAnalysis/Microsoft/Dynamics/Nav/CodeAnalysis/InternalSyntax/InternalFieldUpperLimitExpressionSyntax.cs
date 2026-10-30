using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalFieldUpperLimitExpressionSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalIdentifierNameSyntax _leftHandSide;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalSyntaxToken _fieldFormulaKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalSyntaxToken _upperLimitFormulaKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken1;

	internal readonly InternalIdentifierNameSyntax _identifier;

	internal readonly InternalSyntaxToken _closeParenthesisToken1;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => FieldUpperLimitExpressionSyntaxDefinition.Instance.Value;

	public InternalIdentifierNameSyntax LeftHandSide => _leftHandSide;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalSyntaxToken FieldFormulaKeywordToken => _fieldFormulaKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalSyntaxToken UpperLimitFormulaKeywordToken => _upperLimitFormulaKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken1 => _openParenthesisToken1;

	public InternalIdentifierNameSyntax Identifier => _identifier;

	public InternalSyntaxToken CloseParenthesisToken1 => _closeParenthesisToken1;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalFieldUpperLimitExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken upperLimitFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken1, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
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
		AdjustFlagsAndWidth(upperLimitFormulaKeywordToken);
		_upperLimitFormulaKeywordToken = upperLimitFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken1);
		_openParenthesisToken1 = openParenthesisToken1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(closeParenthesisToken1);
		_closeParenthesisToken1 = closeParenthesisToken1;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalFieldUpperLimitExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken upperLimitFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken1, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
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
		AdjustFlagsAndWidth(upperLimitFormulaKeywordToken);
		_upperLimitFormulaKeywordToken = upperLimitFormulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken1);
		_openParenthesisToken1 = openParenthesisToken1;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(closeParenthesisToken1);
		_closeParenthesisToken1 = closeParenthesisToken1;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalFieldUpperLimitExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken upperLimitFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken1, InternalSyntaxToken closeParenthesisToken)
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
		AdjustFlagsAndWidth(upperLimitFormulaKeywordToken);
		_upperLimitFormulaKeywordToken = upperLimitFormulaKeywordToken;
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
			4 => _upperLimitFormulaKeywordToken, 
			5 => _openParenthesisToken1, 
			6 => _identifier, 
			7 => _closeParenthesisToken1, 
			8 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new FieldUpperLimitExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldUpperLimitExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitFieldUpperLimitExpression(this);
	}

	public InternalFieldUpperLimitExpressionSyntax Update(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldFormulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken upperLimitFormulaKeywordToken, InternalSyntaxToken openParenthesisToken1, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken1, InternalSyntaxToken closeParenthesisToken)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || fieldFormulaKeywordToken != FieldFormulaKeywordToken || openParenthesisToken != OpenParenthesisToken || upperLimitFormulaKeywordToken != UpperLimitFormulaKeywordToken || openParenthesisToken1 != OpenParenthesisToken1 || identifier != Identifier || closeParenthesisToken1 != CloseParenthesisToken1 || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.FieldUpperLimitExpression(leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, upperLimitFormulaKeywordToken, openParenthesisToken1, identifier, closeParenthesisToken1, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalFieldUpperLimitExpressionSyntax(Kind, _leftHandSide, _equalsToken, _fieldFormulaKeywordToken, _openParenthesisToken, _upperLimitFormulaKeywordToken, _openParenthesisToken1, _identifier, _closeParenthesisToken1, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalFieldUpperLimitExpressionSyntax(Kind, _leftHandSide, _equalsToken, _fieldFormulaKeywordToken, _openParenthesisToken, _upperLimitFormulaKeywordToken, _openParenthesisToken1, _identifier, _closeParenthesisToken1, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
