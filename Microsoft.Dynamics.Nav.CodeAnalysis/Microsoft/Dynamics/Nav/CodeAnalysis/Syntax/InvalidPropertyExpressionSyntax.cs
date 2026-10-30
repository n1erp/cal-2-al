using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class InvalidPropertyExpressionSyntax : PropertyExpressionSyntax
{
	private IdentifierNameSyntax _leftHandSide;

	public IdentifierNameSyntax LeftHandSide => GetRedAtZero(ref _leftHandSide);

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((InternalInvalidPropertyExpressionSyntax)Node)._equalsToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken ConstOrFieldOrFilterKeywordToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalInvalidPropertyExpressionSyntax)Node)._constOrFieldOrFilterKeywordToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(2), GetChildIndex(2));
			}
			return default;
		}
	}

	public SyntaxToken InvalidExpression
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalInvalidPropertyExpressionSyntax)Node)._invalidExpression;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(3), GetChildIndex(3));
			}
			return default;
		}
	}

	internal InvalidPropertyExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _leftHandSide);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _leftHandSide;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInvalidPropertyExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitInvalidPropertyExpression(this);
	}

	public InvalidPropertyExpressionSyntax Update(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken constOrFieldOrFilterKeywordToken, SyntaxToken invalidExpression)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || constOrFieldOrFilterKeywordToken != ConstOrFieldOrFilterKeywordToken || invalidExpression != InvalidExpression)
		{
			InvalidPropertyExpressionSyntax invalidPropertyExpressionSyntax = SyntaxFactory.InvalidPropertyExpression(leftHandSide, equalsToken, constOrFieldOrFilterKeywordToken, invalidExpression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return invalidPropertyExpressionSyntax.WithAnnotations(annotations);
			}
			return invalidPropertyExpressionSyntax;
		}
		return this;
	}

	public InvalidPropertyExpressionSyntax WithLeftHandSide(IdentifierNameSyntax leftHandSide)
	{
		return Update(leftHandSide, EqualsToken, ConstOrFieldOrFilterKeywordToken, InvalidExpression);
	}

	public InvalidPropertyExpressionSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(LeftHandSide, equalsToken, ConstOrFieldOrFilterKeywordToken, InvalidExpression);
	}

	public InvalidPropertyExpressionSyntax WithConstOrFieldOrFilterKeywordToken(SyntaxToken constOrFieldOrFilterKeywordToken)
	{
		return Update(LeftHandSide, EqualsToken, constOrFieldOrFilterKeywordToken, InvalidExpression);
	}

	public InvalidPropertyExpressionSyntax WithInvalidExpression(SyntaxToken invalidExpression)
	{
		return Update(LeftHandSide, EqualsToken, ConstOrFieldOrFilterKeywordToken, invalidExpression);
	}
}
