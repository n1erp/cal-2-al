using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ThisExpressionSyntax : InstanceExpressionSyntax
{
	public SyntaxToken Token => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ThisExpressionSyntax)Green).token, Position, 0);

	internal ThisExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitThisExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitThisExpression(this);
	}

	public ThisExpressionSyntax Update(SyntaxToken token)
	{
		if (token != Token)
		{
			ThisExpressionSyntax thisExpressionSyntax = SyntaxFactory.ThisExpression(token);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return thisExpressionSyntax.WithAnnotations(annotations);
			}
			return thisExpressionSyntax;
		}
		return this;
	}

	public ThisExpressionSyntax WithToken(SyntaxToken token)
	{
		return Update(token);
	}
}
