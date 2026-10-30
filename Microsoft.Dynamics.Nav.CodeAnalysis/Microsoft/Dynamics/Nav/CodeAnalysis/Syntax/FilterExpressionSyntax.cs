using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FilterExpressionSyntax : PropertyExpressionSyntax
{
	private IdentifierNameSyntax _leftHandSide;

	private FilterExpressionValueSyntax _filter;

	public IdentifierNameSyntax LeftHandSide => GetRedAtZero(ref _leftHandSide);

	public SyntaxToken EqualsToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalFilterExpressionSyntax)Node)._equalsToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken FilterFormulaKeywordToken => new SyntaxToken(this, ((InternalFilterExpressionSyntax)Node)._filterFormulaKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalFilterExpressionSyntax)Node)._openParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public FilterExpressionValueSyntax Filter => GetRed(ref _filter, 4);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalFilterExpressionSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	internal FilterExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _leftHandSide), 
			4 => GetRed(ref _filter, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_leftHandSide, 
			4 => _filter, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFilterExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFilterExpression(this);
	}

	public FilterExpressionSyntax Update(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken filterFormulaKeywordToken, SyntaxToken openParenthesisToken, FilterExpressionValueSyntax filter, SyntaxToken closeParenthesisToken)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || filterFormulaKeywordToken != FilterFormulaKeywordToken || openParenthesisToken != OpenParenthesisToken || filter != Filter || closeParenthesisToken != CloseParenthesisToken)
		{
			FilterExpressionSyntax filterExpressionSyntax = SyntaxFactory.FilterExpression(leftHandSide, equalsToken, filterFormulaKeywordToken, openParenthesisToken, filter, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return filterExpressionSyntax.WithAnnotations(annotations);
			}
			return filterExpressionSyntax;
		}
		return this;
	}

	public FilterExpressionSyntax WithLeftHandSide(IdentifierNameSyntax leftHandSide)
	{
		return Update(leftHandSide, EqualsToken, FilterFormulaKeywordToken, OpenParenthesisToken, Filter, CloseParenthesisToken);
	}

	public FilterExpressionSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(LeftHandSide, equalsToken, FilterFormulaKeywordToken, OpenParenthesisToken, Filter, CloseParenthesisToken);
	}

	public FilterExpressionSyntax WithFilterFormulaKeywordToken(SyntaxToken filterFormulaKeywordToken)
	{
		return Update(LeftHandSide, EqualsToken, filterFormulaKeywordToken, OpenParenthesisToken, Filter, CloseParenthesisToken);
	}

	public FilterExpressionSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(LeftHandSide, EqualsToken, FilterFormulaKeywordToken, openParenthesisToken, Filter, CloseParenthesisToken);
	}

	public FilterExpressionSyntax WithFilter(FilterExpressionValueSyntax filter)
	{
		return Update(LeftHandSide, EqualsToken, FilterFormulaKeywordToken, OpenParenthesisToken, filter, CloseParenthesisToken);
	}

	public FilterExpressionSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(LeftHandSide, EqualsToken, FilterFormulaKeywordToken, OpenParenthesisToken, Filter, closeParenthesisToken);
	}
}
