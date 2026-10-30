using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class DoStatementSyntax : StatementSyntax
{
	private StatementSyntax statement;

	private ExpressionSyntax condition;

	public SyntaxToken DoKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DoStatementSyntax)Green).doKeyword, Position, 0);

	public StatementSyntax Statement => GetRed(ref statement, 1);

	public SyntaxToken WhileKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DoStatementSyntax)Green).whileKeyword, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DoStatementSyntax)Green).openParenToken, GetChildPosition(3), GetChildIndex(3));

	public ExpressionSyntax Condition => GetRed(ref condition, 4);

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DoStatementSyntax)Green).closeParenToken, GetChildPosition(5), GetChildIndex(5));

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DoStatementSyntax)Green).semicolonToken, GetChildPosition(6), GetChildIndex(6));

	internal DoStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref statement, 1), 
			4 => GetRed(ref condition, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)statement, 
			4 => condition, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDoStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDoStatement(this);
	}

	public DoStatementSyntax Update(SyntaxToken doKeyword, StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken)
	{
		if (doKeyword != DoKeyword || statement != Statement || whileKeyword != WhileKeyword || openParenToken != OpenParenToken || condition != Condition || closeParenToken != CloseParenToken || semicolonToken != SemicolonToken)
		{
			DoStatementSyntax doStatementSyntax = SyntaxFactory.DoStatement(doKeyword, statement, whileKeyword, openParenToken, condition, closeParenToken, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return doStatementSyntax.WithAnnotations(annotations);
			}
			return doStatementSyntax;
		}
		return this;
	}

	public DoStatementSyntax WithDoKeyword(SyntaxToken doKeyword)
	{
		return Update(doKeyword, Statement, WhileKeyword, OpenParenToken, Condition, CloseParenToken, SemicolonToken);
	}

	public DoStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(DoKeyword, statement, WhileKeyword, OpenParenToken, Condition, CloseParenToken, SemicolonToken);
	}

	public DoStatementSyntax WithWhileKeyword(SyntaxToken whileKeyword)
	{
		return Update(DoKeyword, Statement, whileKeyword, OpenParenToken, Condition, CloseParenToken, SemicolonToken);
	}

	public DoStatementSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(DoKeyword, Statement, WhileKeyword, openParenToken, Condition, CloseParenToken, SemicolonToken);
	}

	public DoStatementSyntax WithCondition(ExpressionSyntax condition)
	{
		return Update(DoKeyword, Statement, WhileKeyword, OpenParenToken, condition, CloseParenToken, SemicolonToken);
	}

	public DoStatementSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(DoKeyword, Statement, WhileKeyword, OpenParenToken, Condition, closeParenToken, SemicolonToken);
	}

	public DoStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(DoKeyword, Statement, WhileKeyword, OpenParenToken, Condition, CloseParenToken, semicolonToken);
	}
}
