using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SizeOfExpressionSyntax : ExpressionSyntax
{
	private TypeSyntax type;

	public SyntaxToken Keyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SizeOfExpressionSyntax)Green).keyword, Position, 0);

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SizeOfExpressionSyntax)Green).openParenToken, GetChildPosition(1), GetChildIndex(1));

	public TypeSyntax Type => GetRed(ref type, 2);

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SizeOfExpressionSyntax)Green).closeParenToken, GetChildPosition(3), GetChildIndex(3));

	internal SizeOfExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref type, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return type;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSizeOfExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSizeOfExpression(this);
	}

	public SizeOfExpressionSyntax Update(SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken)
	{
		if (keyword != Keyword || openParenToken != OpenParenToken || type != Type || closeParenToken != CloseParenToken)
		{
			SizeOfExpressionSyntax sizeOfExpressionSyntax = SyntaxFactory.SizeOfExpression(keyword, openParenToken, type, closeParenToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return sizeOfExpressionSyntax.WithAnnotations(annotations);
			}
			return sizeOfExpressionSyntax;
		}
		return this;
	}

	public SizeOfExpressionSyntax WithKeyword(SyntaxToken keyword)
	{
		return Update(keyword, OpenParenToken, Type, CloseParenToken);
	}

	public SizeOfExpressionSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(Keyword, openParenToken, Type, CloseParenToken);
	}

	public SizeOfExpressionSyntax WithType(TypeSyntax type)
	{
		return Update(Keyword, OpenParenToken, type, CloseParenToken);
	}

	public SizeOfExpressionSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(Keyword, OpenParenToken, Type, closeParenToken);
	}
}
