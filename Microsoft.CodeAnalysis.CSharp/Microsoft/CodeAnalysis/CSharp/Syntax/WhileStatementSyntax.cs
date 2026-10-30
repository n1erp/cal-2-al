using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class WhileStatementSyntax : StatementSyntax
{
	private ExpressionSyntax condition;

	private StatementSyntax statement;

	public SyntaxToken WhileKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.WhileStatementSyntax)Green).whileKeyword, Position, 0);

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.WhileStatementSyntax)Green).openParenToken, GetChildPosition(1), GetChildIndex(1));

	public ExpressionSyntax Condition => GetRed(ref condition, 2);

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.WhileStatementSyntax)Green).closeParenToken, GetChildPosition(3), GetChildIndex(3));

	public StatementSyntax Statement => GetRed(ref statement, 4);

	internal WhileStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref condition, 2), 
			4 => GetRed(ref statement, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)condition, 
			4 => statement, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWhileStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitWhileStatement(this);
	}

	public WhileStatementSyntax Update(SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (whileKeyword != WhileKeyword || openParenToken != OpenParenToken || condition != Condition || closeParenToken != CloseParenToken || statement != Statement)
		{
			WhileStatementSyntax whileStatementSyntax = SyntaxFactory.WhileStatement(whileKeyword, openParenToken, condition, closeParenToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return whileStatementSyntax.WithAnnotations(annotations);
			}
			return whileStatementSyntax;
		}
		return this;
	}

	public WhileStatementSyntax WithWhileKeyword(SyntaxToken whileKeyword)
	{
		return Update(whileKeyword, OpenParenToken, Condition, CloseParenToken, Statement);
	}

	public WhileStatementSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(WhileKeyword, openParenToken, Condition, CloseParenToken, Statement);
	}

	public WhileStatementSyntax WithCondition(ExpressionSyntax condition)
	{
		return Update(WhileKeyword, OpenParenToken, condition, CloseParenToken, Statement);
	}

	public WhileStatementSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(WhileKeyword, OpenParenToken, Condition, closeParenToken, Statement);
	}

	public WhileStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(WhileKeyword, OpenParenToken, Condition, CloseParenToken, statement);
	}
}
