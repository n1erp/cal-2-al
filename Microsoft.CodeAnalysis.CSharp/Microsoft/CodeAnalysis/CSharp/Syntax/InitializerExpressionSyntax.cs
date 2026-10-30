using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class InitializerExpressionSyntax : ExpressionSyntax
{
	private SyntaxNode expressions;

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.InitializerExpressionSyntax)Green).openBraceToken, Position, 0);

	public SeparatedSyntaxList<ExpressionSyntax> Expressions
	{
		get
		{
			SyntaxNode red = GetRed(ref expressions, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<ExpressionSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.InitializerExpressionSyntax)Green).closeBraceToken, GetChildPosition(2), GetChildIndex(2));

	internal InitializerExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref expressions, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return expressions;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInitializerExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitInitializerExpression(this);
	}

	public InitializerExpressionSyntax Update(SyntaxToken openBraceToken, SeparatedSyntaxList<ExpressionSyntax> expressions, SyntaxToken closeBraceToken)
	{
		if (openBraceToken != OpenBraceToken || expressions != Expressions || closeBraceToken != CloseBraceToken)
		{
			InitializerExpressionSyntax initializerExpressionSyntax = SyntaxFactory.InitializerExpression(Kind(), openBraceToken, expressions, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return initializerExpressionSyntax.WithAnnotations(annotations);
			}
			return initializerExpressionSyntax;
		}
		return this;
	}

	public InitializerExpressionSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(openBraceToken, Expressions, CloseBraceToken);
	}

	public InitializerExpressionSyntax WithExpressions(SeparatedSyntaxList<ExpressionSyntax> expressions)
	{
		return Update(OpenBraceToken, expressions, CloseBraceToken);
	}

	public InitializerExpressionSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(OpenBraceToken, Expressions, closeBraceToken);
	}

	public InitializerExpressionSyntax AddExpressions(params ExpressionSyntax[] items)
	{
		return WithExpressions(Expressions.AddRange(items));
	}
}
