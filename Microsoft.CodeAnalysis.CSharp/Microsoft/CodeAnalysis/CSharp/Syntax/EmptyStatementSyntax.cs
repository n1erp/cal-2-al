using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class EmptyStatementSyntax : StatementSyntax
{
	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EmptyStatementSyntax)Green).semicolonToken, Position, 0);

	internal EmptyStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitEmptyStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitEmptyStatement(this);
	}

	public EmptyStatementSyntax Update(SyntaxToken semicolonToken)
	{
		if (semicolonToken != SemicolonToken)
		{
			EmptyStatementSyntax emptyStatementSyntax = SyntaxFactory.EmptyStatement(semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return emptyStatementSyntax.WithAnnotations(annotations);
			}
			return emptyStatementSyntax;
		}
		return this;
	}

	public EmptyStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(semicolonToken);
	}
}
