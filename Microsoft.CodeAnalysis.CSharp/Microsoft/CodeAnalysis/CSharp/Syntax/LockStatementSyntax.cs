using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class LockStatementSyntax : StatementSyntax
{
	private ExpressionSyntax expression;

	private StatementSyntax statement;

	public SyntaxToken LockKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LockStatementSyntax)Green).lockKeyword, Position, 0);

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LockStatementSyntax)Green).openParenToken, GetChildPosition(1), GetChildIndex(1));

	public ExpressionSyntax Expression => GetRed(ref expression, 2);

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LockStatementSyntax)Green).closeParenToken, GetChildPosition(3), GetChildIndex(3));

	public StatementSyntax Statement => GetRed(ref statement, 4);

	internal LockStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref expression, 2), 
			4 => GetRed(ref statement, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)expression, 
			4 => statement, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLockStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLockStatement(this);
	}

	public LockStatementSyntax Update(SyntaxToken lockKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (lockKeyword != LockKeyword || openParenToken != OpenParenToken || expression != Expression || closeParenToken != CloseParenToken || statement != Statement)
		{
			LockStatementSyntax lockStatementSyntax = SyntaxFactory.LockStatement(lockKeyword, openParenToken, expression, closeParenToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return lockStatementSyntax.WithAnnotations(annotations);
			}
			return lockStatementSyntax;
		}
		return this;
	}

	public LockStatementSyntax WithLockKeyword(SyntaxToken lockKeyword)
	{
		return Update(lockKeyword, OpenParenToken, Expression, CloseParenToken, Statement);
	}

	public LockStatementSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(LockKeyword, openParenToken, Expression, CloseParenToken, Statement);
	}

	public LockStatementSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(LockKeyword, OpenParenToken, expression, CloseParenToken, Statement);
	}

	public LockStatementSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(LockKeyword, OpenParenToken, Expression, closeParenToken, Statement);
	}

	public LockStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(LockKeyword, OpenParenToken, Expression, CloseParenToken, statement);
	}
}
