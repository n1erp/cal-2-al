using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
	public SyntaxToken Token => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LiteralExpressionSyntax)Green).token, Position, 0);

	internal LiteralExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLiteralExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLiteralExpression(this);
	}

	public LiteralExpressionSyntax Update(SyntaxToken token)
	{
		if (token != Token)
		{
			LiteralExpressionSyntax literalExpressionSyntax = SyntaxFactory.LiteralExpression(Kind(), token);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return literalExpressionSyntax.WithAnnotations(annotations);
			}
			return literalExpressionSyntax;
		}
		return this;
	}

	public LiteralExpressionSyntax WithToken(SyntaxToken token)
	{
		return Update(token);
	}
}
