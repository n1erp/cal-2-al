using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FieldUpperLimitExpressionSyntax : PropertyExpressionSyntax
{
	private IdentifierNameSyntax _leftHandSide;

	private IdentifierNameSyntax _identifier;

	public IdentifierNameSyntax LeftHandSide => GetRedAtZero(ref _leftHandSide);

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((InternalFieldUpperLimitExpressionSyntax)Node)._equalsToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken FieldFormulaKeywordToken => new SyntaxToken(this, ((InternalFieldUpperLimitExpressionSyntax)Node)._fieldFormulaKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalFieldUpperLimitExpressionSyntax)Node)._openParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public SyntaxToken UpperLimitFormulaKeywordToken => new SyntaxToken(this, ((InternalFieldUpperLimitExpressionSyntax)Node)._upperLimitFormulaKeywordToken, GetChildPosition(4), GetChildIndex(4));

	public SyntaxToken OpenParenthesisToken1 => new SyntaxToken(this, ((InternalFieldUpperLimitExpressionSyntax)Node)._openParenthesisToken1, GetChildPosition(5), GetChildIndex(5));

	public IdentifierNameSyntax Identifier => GetRed(ref _identifier, 6);

	public SyntaxToken CloseParenthesisToken1 => new SyntaxToken(this, ((InternalFieldUpperLimitExpressionSyntax)Node)._closeParenthesisToken1, GetChildPosition(7), GetChildIndex(7));

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalFieldUpperLimitExpressionSyntax)Node)._closeParenthesisToken, GetChildPosition(8), GetChildIndex(8));

	internal FieldUpperLimitExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _leftHandSide), 
			6 => GetRed(ref _identifier, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_leftHandSide, 
			6 => _identifier, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldUpperLimitExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFieldUpperLimitExpression(this);
	}

	public FieldUpperLimitExpressionSyntax Update(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken fieldFormulaKeywordToken, SyntaxToken openParenthesisToken, SyntaxToken upperLimitFormulaKeywordToken, SyntaxToken openParenthesisToken1, IdentifierNameSyntax identifier, SyntaxToken closeParenthesisToken1, SyntaxToken closeParenthesisToken)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || fieldFormulaKeywordToken != FieldFormulaKeywordToken || openParenthesisToken != OpenParenthesisToken || upperLimitFormulaKeywordToken != UpperLimitFormulaKeywordToken || openParenthesisToken1 != OpenParenthesisToken1 || identifier != Identifier || closeParenthesisToken1 != CloseParenthesisToken1 || closeParenthesisToken != CloseParenthesisToken)
		{
			FieldUpperLimitExpressionSyntax fieldUpperLimitExpressionSyntax = SyntaxFactory.FieldUpperLimitExpression(leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, upperLimitFormulaKeywordToken, openParenthesisToken1, identifier, closeParenthesisToken1, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fieldUpperLimitExpressionSyntax.WithAnnotations(annotations);
			}
			return fieldUpperLimitExpressionSyntax;
		}
		return this;
	}

	public FieldUpperLimitExpressionSyntax WithLeftHandSide(IdentifierNameSyntax leftHandSide)
	{
		return Update(leftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken, UpperLimitFormulaKeywordToken, OpenParenthesisToken1, Identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldUpperLimitExpressionSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(LeftHandSide, equalsToken, FieldFormulaKeywordToken, OpenParenthesisToken, UpperLimitFormulaKeywordToken, OpenParenthesisToken1, Identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldUpperLimitExpressionSyntax WithFieldFormulaKeywordToken(SyntaxToken fieldFormulaKeywordToken)
	{
		return Update(LeftHandSide, EqualsToken, fieldFormulaKeywordToken, OpenParenthesisToken, UpperLimitFormulaKeywordToken, OpenParenthesisToken1, Identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldUpperLimitExpressionSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, openParenthesisToken, UpperLimitFormulaKeywordToken, OpenParenthesisToken1, Identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldUpperLimitExpressionSyntax WithUpperLimitFormulaKeywordToken(SyntaxToken upperLimitFormulaKeywordToken)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken, upperLimitFormulaKeywordToken, OpenParenthesisToken1, Identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldUpperLimitExpressionSyntax WithOpenParenthesisToken1(SyntaxToken openParenthesisToken1)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken, UpperLimitFormulaKeywordToken, openParenthesisToken1, Identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldUpperLimitExpressionSyntax WithIdentifier(IdentifierNameSyntax identifier)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken, UpperLimitFormulaKeywordToken, OpenParenthesisToken1, identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldUpperLimitExpressionSyntax WithCloseParenthesisToken1(SyntaxToken closeParenthesisToken1)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken, UpperLimitFormulaKeywordToken, OpenParenthesisToken1, Identifier, closeParenthesisToken1, CloseParenthesisToken);
	}

	public FieldUpperLimitExpressionSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken, UpperLimitFormulaKeywordToken, OpenParenthesisToken1, Identifier, CloseParenthesisToken1, closeParenthesisToken);
	}
}
