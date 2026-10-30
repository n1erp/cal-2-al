using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class BreakStatementSyntax : StatementSyntax
{
	public SyntaxToken BreakKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BreakStatementSyntax)Green).breakKeyword, Position, 0);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BreakStatementSyntax)Green).semicolonToken, GetChildPosition(1), GetChildIndex(1));

	internal BreakStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitBreakStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBreakStatement(this);
	}

	public BreakStatementSyntax Update(SyntaxToken breakKeyword, SyntaxToken semicolonToken)
	{
		if (breakKeyword != BreakKeyword || semicolonToken != SemicolonToken)
		{
			BreakStatementSyntax breakStatementSyntax = SyntaxFactory.BreakStatement(breakKeyword, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return breakStatementSyntax.WithAnnotations(annotations);
			}
			return breakStatementSyntax;
		}
		return this;
	}

	public BreakStatementSyntax WithBreakKeyword(SyntaxToken breakKeyword)
	{
		return Update(breakKeyword, SemicolonToken);
	}

	public BreakStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(BreakKeyword, semicolonToken);
	}
}
