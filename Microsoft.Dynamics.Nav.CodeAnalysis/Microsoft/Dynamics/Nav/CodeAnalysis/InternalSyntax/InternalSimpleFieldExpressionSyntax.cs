using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalSimpleFieldExpressionSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalIdentifierNameSyntax _leftHandSide;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalSyntaxToken _fieldKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _identifier;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => SimpleFieldExpressionSyntaxDefinition.Instance.Value;

	public InternalIdentifierNameSyntax LeftHandSide => _leftHandSide;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalSyntaxToken FieldKeywordToken => _fieldKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalIdentifierNameSyntax Identifier => _identifier;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalSimpleFieldExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(fieldKeywordToken);
		_fieldKeywordToken = fieldKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalSimpleFieldExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(fieldKeywordToken);
		_fieldKeywordToken = fieldKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalSimpleFieldExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(fieldKeywordToken);
		_fieldKeywordToken = fieldKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(identifier);
		_identifier = identifier;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_leftHandSide, 
			1 => _equalsToken, 
			2 => _fieldKeywordToken, 
			3 => _openParenthesisToken, 
			4 => _identifier, 
			5 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new SimpleFieldExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSimpleFieldExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitSimpleFieldExpression(this);
	}

	public InternalSimpleFieldExpressionSyntax Update(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax identifier, InternalSyntaxToken closeParenthesisToken)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || fieldKeywordToken != FieldKeywordToken || openParenthesisToken != OpenParenthesisToken || identifier != Identifier || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.SimpleFieldExpression(leftHandSide, equalsToken, fieldKeywordToken, openParenthesisToken, identifier, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalSimpleFieldExpressionSyntax(Kind, _leftHandSide, _equalsToken, _fieldKeywordToken, _openParenthesisToken, _identifier, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalSimpleFieldExpressionSyntax(Kind, _leftHandSide, _equalsToken, _fieldKeywordToken, _openParenthesisToken, _identifier, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
