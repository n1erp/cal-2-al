using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ConstExpressionSyntax : PropertyExpressionSyntax
{
	private IdentifierNameSyntax _leftHandSide;

	private IdentifierOrLiteralExpressionSyntax _identifier;

	public IdentifierNameSyntax LeftHandSide => GetRedAtZero(ref _leftHandSide);

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((InternalConstExpressionSyntax)Node)._equalsToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken ConstKeywordToken => new SyntaxToken(this, ((InternalConstExpressionSyntax)Node)._constKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalConstExpressionSyntax)Node)._openParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public IdentifierOrLiteralExpressionSyntax Identifier => GetRed(ref _identifier, 4);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalConstExpressionSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	internal ConstExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _leftHandSide), 
			4 => GetRed(ref _identifier, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_leftHandSide, 
			4 => _identifier, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConstExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitConstExpression(this);
	}

	public ConstExpressionSyntax Update(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken constKeywordToken, SyntaxToken openParenthesisToken, IdentifierOrLiteralExpressionSyntax identifier, SyntaxToken closeParenthesisToken)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || constKeywordToken != ConstKeywordToken || openParenthesisToken != OpenParenthesisToken || identifier != Identifier || closeParenthesisToken != CloseParenthesisToken)
		{
			ConstExpressionSyntax constExpressionSyntax = SyntaxFactory.ConstExpression(leftHandSide, equalsToken, constKeywordToken, openParenthesisToken, identifier, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return constExpressionSyntax.WithAnnotations(annotations);
			}
			return constExpressionSyntax;
		}
		return this;
	}

	public ConstExpressionSyntax WithLeftHandSide(IdentifierNameSyntax leftHandSide)
	{
		return Update(leftHandSide, EqualsToken, ConstKeywordToken, OpenParenthesisToken, Identifier, CloseParenthesisToken);
	}

	public ConstExpressionSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(LeftHandSide, equalsToken, ConstKeywordToken, OpenParenthesisToken, Identifier, CloseParenthesisToken);
	}

	public ConstExpressionSyntax WithConstKeywordToken(SyntaxToken constKeywordToken)
	{
		return Update(LeftHandSide, EqualsToken, constKeywordToken, OpenParenthesisToken, Identifier, CloseParenthesisToken);
	}

	public ConstExpressionSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(LeftHandSide, EqualsToken, ConstKeywordToken, openParenthesisToken, Identifier, CloseParenthesisToken);
	}

	public ConstExpressionSyntax WithIdentifier(IdentifierOrLiteralExpressionSyntax identifier)
	{
		return Update(LeftHandSide, EqualsToken, ConstKeywordToken, OpenParenthesisToken, identifier, CloseParenthesisToken);
	}

	public ConstExpressionSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(LeftHandSide, EqualsToken, ConstKeywordToken, OpenParenthesisToken, Identifier, closeParenthesisToken);
	}
}
