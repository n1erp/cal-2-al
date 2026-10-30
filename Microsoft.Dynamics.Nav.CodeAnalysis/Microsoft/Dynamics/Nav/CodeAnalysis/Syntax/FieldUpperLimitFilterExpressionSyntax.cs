using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FieldUpperLimitFilterExpressionSyntax : PropertyExpressionSyntax
{
	private IdentifierNameSyntax _leftHandSide;

	private IdentifierNameSyntax _identifier;

	public IdentifierNameSyntax LeftHandSide => GetRedAtZero(ref _leftHandSide);

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((InternalFieldUpperLimitFilterExpressionSyntax)Node)._equalsToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken FieldFormulaKeywordToken => new SyntaxToken(this, ((InternalFieldUpperLimitFilterExpressionSyntax)Node)._fieldFormulaKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken OpenParenthesisToken1 => new SyntaxToken(this, ((InternalFieldUpperLimitFilterExpressionSyntax)Node)._openParenthesisToken1, GetChildPosition(3), GetChildIndex(3));

	public SyntaxToken UpperLimitFormulaKeywordToken => new SyntaxToken(this, ((InternalFieldUpperLimitFilterExpressionSyntax)Node)._upperLimitFormulaKeywordToken, GetChildPosition(4), GetChildIndex(4));

	public SyntaxToken OpenParenthesisToken2 => new SyntaxToken(this, ((InternalFieldUpperLimitFilterExpressionSyntax)Node)._openParenthesisToken2, GetChildPosition(5), GetChildIndex(5));

	public SyntaxToken FilterFormulaKeywordToken => new SyntaxToken(this, ((InternalFieldUpperLimitFilterExpressionSyntax)Node)._filterFormulaKeywordToken, GetChildPosition(6), GetChildIndex(6));

	public SyntaxToken OpenParenthesisToken3 => new SyntaxToken(this, ((InternalFieldUpperLimitFilterExpressionSyntax)Node)._openParenthesisToken3, GetChildPosition(7), GetChildIndex(7));

	public IdentifierNameSyntax Identifier => GetRed(ref _identifier, 8);

	public SyntaxToken CloseParenthesisToken3 => new SyntaxToken(this, ((InternalFieldUpperLimitFilterExpressionSyntax)Node)._closeParenthesisToken3, GetChildPosition(9), GetChildIndex(9));

	public SyntaxToken CloseParenthesisToken2 => new SyntaxToken(this, ((InternalFieldUpperLimitFilterExpressionSyntax)Node)._closeParenthesisToken2, GetChildPosition(10), GetChildIndex(10));

	public SyntaxToken CloseParenthesisToken1 => new SyntaxToken(this, ((InternalFieldUpperLimitFilterExpressionSyntax)Node)._closeParenthesisToken1, GetChildPosition(11), GetChildIndex(11));

	internal FieldUpperLimitFilterExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _leftHandSide), 
			8 => GetRed(ref _identifier, 8), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_leftHandSide, 
			8 => _identifier, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldUpperLimitFilterExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFieldUpperLimitFilterExpression(this);
	}

	public FieldUpperLimitFilterExpressionSyntax Update(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken fieldFormulaKeywordToken, SyntaxToken openParenthesisToken1, SyntaxToken upperLimitFormulaKeywordToken, SyntaxToken openParenthesisToken2, SyntaxToken filterFormulaKeywordToken, SyntaxToken openParenthesisToken3, IdentifierNameSyntax identifier, SyntaxToken closeParenthesisToken3, SyntaxToken closeParenthesisToken2, SyntaxToken closeParenthesisToken1)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || fieldFormulaKeywordToken != FieldFormulaKeywordToken || openParenthesisToken1 != OpenParenthesisToken1 || upperLimitFormulaKeywordToken != UpperLimitFormulaKeywordToken || openParenthesisToken2 != OpenParenthesisToken2 || filterFormulaKeywordToken != FilterFormulaKeywordToken || openParenthesisToken3 != OpenParenthesisToken3 || identifier != Identifier || closeParenthesisToken3 != CloseParenthesisToken3 || closeParenthesisToken2 != CloseParenthesisToken2 || closeParenthesisToken1 != CloseParenthesisToken1)
		{
			FieldUpperLimitFilterExpressionSyntax fieldUpperLimitFilterExpressionSyntax = SyntaxFactory.FieldUpperLimitFilterExpression(leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken1, upperLimitFormulaKeywordToken, openParenthesisToken2, filterFormulaKeywordToken, openParenthesisToken3, identifier, closeParenthesisToken3, closeParenthesisToken2, closeParenthesisToken1);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fieldUpperLimitFilterExpressionSyntax.WithAnnotations(annotations);
			}
			return fieldUpperLimitFilterExpressionSyntax;
		}
		return this;
	}

	public FieldUpperLimitFilterExpressionSyntax WithLeftHandSide(IdentifierNameSyntax leftHandSide)
	{
		return Update(leftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken1, UpperLimitFormulaKeywordToken, OpenParenthesisToken2, FilterFormulaKeywordToken, OpenParenthesisToken3, Identifier, CloseParenthesisToken3, CloseParenthesisToken2, CloseParenthesisToken1);
	}

	public FieldUpperLimitFilterExpressionSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(LeftHandSide, equalsToken, FieldFormulaKeywordToken, OpenParenthesisToken1, UpperLimitFormulaKeywordToken, OpenParenthesisToken2, FilterFormulaKeywordToken, OpenParenthesisToken3, Identifier, CloseParenthesisToken3, CloseParenthesisToken2, CloseParenthesisToken1);
	}

	public FieldUpperLimitFilterExpressionSyntax WithFieldFormulaKeywordToken(SyntaxToken fieldFormulaKeywordToken)
	{
		return Update(LeftHandSide, EqualsToken, fieldFormulaKeywordToken, OpenParenthesisToken1, UpperLimitFormulaKeywordToken, OpenParenthesisToken2, FilterFormulaKeywordToken, OpenParenthesisToken3, Identifier, CloseParenthesisToken3, CloseParenthesisToken2, CloseParenthesisToken1);
	}

	public FieldUpperLimitFilterExpressionSyntax WithOpenParenthesisToken1(SyntaxToken openParenthesisToken1)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, openParenthesisToken1, UpperLimitFormulaKeywordToken, OpenParenthesisToken2, FilterFormulaKeywordToken, OpenParenthesisToken3, Identifier, CloseParenthesisToken3, CloseParenthesisToken2, CloseParenthesisToken1);
	}

	public FieldUpperLimitFilterExpressionSyntax WithUpperLimitFormulaKeywordToken(SyntaxToken upperLimitFormulaKeywordToken)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken1, upperLimitFormulaKeywordToken, OpenParenthesisToken2, FilterFormulaKeywordToken, OpenParenthesisToken3, Identifier, CloseParenthesisToken3, CloseParenthesisToken2, CloseParenthesisToken1);
	}

	public FieldUpperLimitFilterExpressionSyntax WithOpenParenthesisToken2(SyntaxToken openParenthesisToken2)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken1, UpperLimitFormulaKeywordToken, openParenthesisToken2, FilterFormulaKeywordToken, OpenParenthesisToken3, Identifier, CloseParenthesisToken3, CloseParenthesisToken2, CloseParenthesisToken1);
	}

	public FieldUpperLimitFilterExpressionSyntax WithFilterFormulaKeywordToken(SyntaxToken filterFormulaKeywordToken)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken1, UpperLimitFormulaKeywordToken, OpenParenthesisToken2, filterFormulaKeywordToken, OpenParenthesisToken3, Identifier, CloseParenthesisToken3, CloseParenthesisToken2, CloseParenthesisToken1);
	}

	public FieldUpperLimitFilterExpressionSyntax WithOpenParenthesisToken3(SyntaxToken openParenthesisToken3)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken1, UpperLimitFormulaKeywordToken, OpenParenthesisToken2, FilterFormulaKeywordToken, openParenthesisToken3, Identifier, CloseParenthesisToken3, CloseParenthesisToken2, CloseParenthesisToken1);
	}

	public FieldUpperLimitFilterExpressionSyntax WithIdentifier(IdentifierNameSyntax identifier)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken1, UpperLimitFormulaKeywordToken, OpenParenthesisToken2, FilterFormulaKeywordToken, OpenParenthesisToken3, identifier, CloseParenthesisToken3, CloseParenthesisToken2, CloseParenthesisToken1);
	}

	public FieldUpperLimitFilterExpressionSyntax WithCloseParenthesisToken3(SyntaxToken closeParenthesisToken3)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken1, UpperLimitFormulaKeywordToken, OpenParenthesisToken2, FilterFormulaKeywordToken, OpenParenthesisToken3, Identifier, closeParenthesisToken3, CloseParenthesisToken2, CloseParenthesisToken1);
	}

	public FieldUpperLimitFilterExpressionSyntax WithCloseParenthesisToken2(SyntaxToken closeParenthesisToken2)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken1, UpperLimitFormulaKeywordToken, OpenParenthesisToken2, FilterFormulaKeywordToken, OpenParenthesisToken3, Identifier, CloseParenthesisToken3, closeParenthesisToken2, CloseParenthesisToken1);
	}

	public FieldUpperLimitFilterExpressionSyntax WithCloseParenthesisToken1(SyntaxToken closeParenthesisToken1)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken1, UpperLimitFormulaKeywordToken, OpenParenthesisToken2, FilterFormulaKeywordToken, OpenParenthesisToken3, Identifier, CloseParenthesisToken3, CloseParenthesisToken2, closeParenthesisToken1);
	}
}
