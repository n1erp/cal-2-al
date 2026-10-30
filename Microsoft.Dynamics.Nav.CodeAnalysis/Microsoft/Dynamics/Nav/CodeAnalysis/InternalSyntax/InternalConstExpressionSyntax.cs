using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalConstExpressionSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalIdentifierNameSyntax _leftHandSide;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalSyntaxToken _constKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierOrLiteralExpressionSyntax _identifier;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => ConstExpressionSyntaxDefinition.Instance.Value;

	public InternalIdentifierNameSyntax LeftHandSide => _leftHandSide;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalSyntaxToken ConstKeywordToken => _constKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalIdentifierOrLiteralExpressionSyntax Identifier => _identifier;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalConstExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken constKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierOrLiteralExpressionSyntax identifier, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(constKeywordToken);
		_constKeywordToken = constKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			_identifier = identifier;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalConstExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken constKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierOrLiteralExpressionSyntax identifier, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(constKeywordToken);
		_constKeywordToken = constKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			_identifier = identifier;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalConstExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken constKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierOrLiteralExpressionSyntax identifier, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(constKeywordToken);
		_constKeywordToken = constKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			_identifier = identifier;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_leftHandSide, 
			1 => _equalsToken, 
			2 => _constKeywordToken, 
			3 => _openParenthesisToken, 
			4 => _identifier, 
			5 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ConstExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConstExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitConstExpression(this);
	}

	public InternalConstExpressionSyntax Update(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken constKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierOrLiteralExpressionSyntax identifier, InternalSyntaxToken closeParenthesisToken)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || constKeywordToken != ConstKeywordToken || openParenthesisToken != OpenParenthesisToken || identifier != Identifier || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.ConstExpression(leftHandSide, equalsToken, constKeywordToken, openParenthesisToken, identifier, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalConstExpressionSyntax(Kind, _leftHandSide, _equalsToken, _constKeywordToken, _openParenthesisToken, _identifier, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalConstExpressionSyntax(Kind, _leftHandSide, _equalsToken, _constKeywordToken, _openParenthesisToken, _identifier, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
