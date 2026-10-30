using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class SimpleFieldExpressionSyntax : PropertyExpressionSyntax
{
	private IdentifierNameSyntax _leftHandSide;

	private IdentifierNameSyntax _identifier;

	public IdentifierNameSyntax LeftHandSide => GetRedAtZero(ref _leftHandSide);

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((InternalSimpleFieldExpressionSyntax)Node)._equalsToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken FieldKeywordToken => new SyntaxToken(this, ((InternalSimpleFieldExpressionSyntax)Node)._fieldKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalSimpleFieldExpressionSyntax)Node)._openParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public IdentifierNameSyntax Identifier => GetRed(ref _identifier, 4);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalSimpleFieldExpressionSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	internal SimpleFieldExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitSimpleFieldExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitSimpleFieldExpression(this);
	}

	public SimpleFieldExpressionSyntax Update(IdentifierNameSyntax leftHandSide, SyntaxToken equalsToken, SyntaxToken fieldKeywordToken, SyntaxToken openParenthesisToken, IdentifierNameSyntax identifier, SyntaxToken closeParenthesisToken)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || fieldKeywordToken != FieldKeywordToken || openParenthesisToken != OpenParenthesisToken || identifier != Identifier || closeParenthesisToken != CloseParenthesisToken)
		{
			SimpleFieldExpressionSyntax simpleFieldExpressionSyntax = SyntaxFactory.SimpleFieldExpression(leftHandSide, equalsToken, fieldKeywordToken, openParenthesisToken, identifier, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return simpleFieldExpressionSyntax.WithAnnotations(annotations);
			}
			return simpleFieldExpressionSyntax;
		}
		return this;
	}

	public SimpleFieldExpressionSyntax WithLeftHandSide(IdentifierNameSyntax leftHandSide)
	{
		return Update(leftHandSide, EqualsToken, FieldKeywordToken, OpenParenthesisToken, Identifier, CloseParenthesisToken);
	}

	public SimpleFieldExpressionSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(LeftHandSide, equalsToken, FieldKeywordToken, OpenParenthesisToken, Identifier, CloseParenthesisToken);
	}

	public SimpleFieldExpressionSyntax WithFieldKeywordToken(SyntaxToken fieldKeywordToken)
	{
		return Update(LeftHandSide, EqualsToken, fieldKeywordToken, OpenParenthesisToken, Identifier, CloseParenthesisToken);
	}

	public SimpleFieldExpressionSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(LeftHandSide, EqualsToken, FieldKeywordToken, openParenthesisToken, Identifier, CloseParenthesisToken);
	}

	public SimpleFieldExpressionSyntax WithIdentifier(IdentifierNameSyntax identifier)
	{
		return Update(LeftHandSide, EqualsToken, FieldKeywordToken, OpenParenthesisToken, identifier, CloseParenthesisToken);
	}

	public SimpleFieldExpressionSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(LeftHandSide, EqualsToken, FieldKeywordToken, OpenParenthesisToken, Identifier, closeParenthesisToken);
	}
}
