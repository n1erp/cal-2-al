using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class BaseExpressionSyntax : InstanceExpressionSyntax
{
	public SyntaxToken Token => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseExpressionSyntax)Green).token, Position, 0);

	internal BaseExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitBaseExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBaseExpression(this);
	}

	public BaseExpressionSyntax Update(SyntaxToken token)
	{
		if (token != Token)
		{
			BaseExpressionSyntax baseExpressionSyntax = SyntaxFactory.BaseExpression(token);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return baseExpressionSyntax.WithAnnotations(annotations);
			}
			return baseExpressionSyntax;
		}
		return this;
	}

	public BaseExpressionSyntax WithToken(SyntaxToken token)
	{
		return Update(token);
	}
}
