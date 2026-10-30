using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FieldFilterExpressionSyntax : PropertyExpressionSyntax
{
	private IdentifierNameSyntax _leftHandSide;

	private IdentifierNameSyntax _identifier;

	public IdentifierNameSyntax LeftHandSide => GetRedAtZero(ref _leftHandSide);

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((InternalFieldFilterExpressionSyntax)Node)._equalsToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken FieldFormulaKeywordToken => new SyntaxToken(this, ((InternalFieldFilterExpressionSyntax)Node)._fieldFormulaKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalFieldFilterExpressionSyntax)Node)._openParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public SyntaxToken FilterFormulaKeywordToken => new SyntaxToken(this, ((InternalFieldFilterExpressionSyntax)Node)._filterFormulaKeywordToken, GetChildPosition(4), GetChildIndex(4));

	public SyntaxToken OpenParenthesisToken1 => new SyntaxToken(this, ((InternalFieldFilterExpressionSyntax)Node)._openParenthesisToken1, GetChildPosition(5), GetChildIndex(5));

	public IdentifierNameSyntax Identifier => GetRed(ref _identifier, 6);

	public SyntaxToken CloseParenthesisToken1 => new SyntaxToken(this, ((InternalFieldFilterExpressionSyntax)Node)._closeParenthesisToken1, GetChildPosition(7), GetChildIndex(7));

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalFieldFilterExpressionSyntax)Node)._closeParenthesisToken, GetChildPosition(8), GetChildIndex(8));

	internal FieldFilterExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitFieldFilterExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFieldFilterExpression(this);
	}

	public FieldFilterExpressionSyntax Update(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken fieldFormulaKeywordToken, SyntaxToken openParenthesisToken, SyntaxToken filterFormulaKeywordToken, SyntaxToken openParenthesisToken1, IdentifierNameSyntax identifier, SyntaxToken closeParenthesisToken1, SyntaxToken closeParenthesisToken)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || fieldFormulaKeywordToken != FieldFormulaKeywordToken || openParenthesisToken != OpenParenthesisToken || filterFormulaKeywordToken != FilterFormulaKeywordToken || openParenthesisToken1 != OpenParenthesisToken1 || identifier != Identifier || closeParenthesisToken1 != CloseParenthesisToken1 || closeParenthesisToken != CloseParenthesisToken)
		{
			FieldFilterExpressionSyntax fieldFilterExpressionSyntax = SyntaxFactory.FieldFilterExpression(leftHandSide, equalsToken, fieldFormulaKeywordToken, openParenthesisToken, filterFormulaKeywordToken, openParenthesisToken1, identifier, closeParenthesisToken1, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fieldFilterExpressionSyntax.WithAnnotations(annotations);
			}
			return fieldFilterExpressionSyntax;
		}
		return this;
	}

	public FieldFilterExpressionSyntax WithLeftHandSide(IdentifierNameSyntax leftHandSide)
	{
		return Update(leftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken, FilterFormulaKeywordToken, OpenParenthesisToken1, Identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldFilterExpressionSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(LeftHandSide, equalsToken, FieldFormulaKeywordToken, OpenParenthesisToken, FilterFormulaKeywordToken, OpenParenthesisToken1, Identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldFilterExpressionSyntax WithFieldFormulaKeywordToken(SyntaxToken fieldFormulaKeywordToken)
	{
		return Update(LeftHandSide, EqualsToken, fieldFormulaKeywordToken, OpenParenthesisToken, FilterFormulaKeywordToken, OpenParenthesisToken1, Identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldFilterExpressionSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, openParenthesisToken, FilterFormulaKeywordToken, OpenParenthesisToken1, Identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldFilterExpressionSyntax WithFilterFormulaKeywordToken(SyntaxToken filterFormulaKeywordToken)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken, filterFormulaKeywordToken, OpenParenthesisToken1, Identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldFilterExpressionSyntax WithOpenParenthesisToken1(SyntaxToken openParenthesisToken1)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken, FilterFormulaKeywordToken, openParenthesisToken1, Identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldFilterExpressionSyntax WithIdentifier(IdentifierNameSyntax identifier)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken, FilterFormulaKeywordToken, OpenParenthesisToken1, identifier, CloseParenthesisToken1, CloseParenthesisToken);
	}

	public FieldFilterExpressionSyntax WithCloseParenthesisToken1(SyntaxToken closeParenthesisToken1)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken, FilterFormulaKeywordToken, OpenParenthesisToken1, Identifier, closeParenthesisToken1, CloseParenthesisToken);
	}

	public FieldFilterExpressionSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(LeftHandSide, EqualsToken, FieldFormulaKeywordToken, OpenParenthesisToken, FilterFormulaKeywordToken, OpenParenthesisToken1, Identifier, CloseParenthesisToken1, closeParenthesisToken);
	}
}
