using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ContinueStatementSyntax : StatementSyntax
{
	public SyntaxToken ContinueKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ContinueStatementSyntax)Green).continueKeyword, Position, 0);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ContinueStatementSyntax)Green).semicolonToken, GetChildPosition(1), GetChildIndex(1));

	internal ContinueStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitContinueStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitContinueStatement(this);
	}

	public ContinueStatementSyntax Update(SyntaxToken continueKeyword, SyntaxToken semicolonToken)
	{
		if (continueKeyword != ContinueKeyword || semicolonToken != SemicolonToken)
		{
			ContinueStatementSyntax continueStatementSyntax = SyntaxFactory.ContinueStatement(continueKeyword, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return continueStatementSyntax.WithAnnotations(annotations);
			}
			return continueStatementSyntax;
		}
		return this;
	}

	public ContinueStatementSyntax WithContinueKeyword(SyntaxToken continueKeyword)
	{
		return Update(continueKeyword, SemicolonToken);
	}

	public ContinueStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ContinueKeyword, semicolonToken);
	}
}
