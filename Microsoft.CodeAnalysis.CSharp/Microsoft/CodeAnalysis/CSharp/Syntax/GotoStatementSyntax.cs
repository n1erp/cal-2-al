using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class GotoStatementSyntax : StatementSyntax
{
	private ExpressionSyntax expression;

	public SyntaxToken GotoKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.GotoStatementSyntax)Green).gotoKeyword, Position, 0);

	public SyntaxToken CaseOrDefaultKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken caseOrDefaultKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.GotoStatementSyntax)Green).caseOrDefaultKeyword;
			if (caseOrDefaultKeyword != null)
			{
				return new SyntaxToken(this, caseOrDefaultKeyword, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public ExpressionSyntax Expression => GetRed(ref expression, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.GotoStatementSyntax)Green).semicolonToken, GetChildPosition(3), GetChildIndex(3));

	internal GotoStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref expression, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return expression;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitGotoStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitGotoStatement(this);
	}

	public GotoStatementSyntax Update(SyntaxToken gotoKeyword, SyntaxToken caseOrDefaultKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (gotoKeyword != GotoKeyword || caseOrDefaultKeyword != CaseOrDefaultKeyword || expression != Expression || semicolonToken != SemicolonToken)
		{
			GotoStatementSyntax gotoStatementSyntax = SyntaxFactory.GotoStatement(Kind(), gotoKeyword, caseOrDefaultKeyword, expression, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return gotoStatementSyntax.WithAnnotations(annotations);
			}
			return gotoStatementSyntax;
		}
		return this;
	}

	public GotoStatementSyntax WithGotoKeyword(SyntaxToken gotoKeyword)
	{
		return Update(gotoKeyword, CaseOrDefaultKeyword, Expression, SemicolonToken);
	}

	public GotoStatementSyntax WithCaseOrDefaultKeyword(SyntaxToken caseOrDefaultKeyword)
	{
		return Update(GotoKeyword, caseOrDefaultKeyword, Expression, SemicolonToken);
	}

	public GotoStatementSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(GotoKeyword, CaseOrDefaultKeyword, expression, SemicolonToken);
	}

	public GotoStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(GotoKeyword, CaseOrDefaultKeyword, Expression, semicolonToken);
	}
}
