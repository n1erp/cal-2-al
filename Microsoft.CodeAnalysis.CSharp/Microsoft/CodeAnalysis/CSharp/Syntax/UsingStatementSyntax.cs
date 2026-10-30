using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class UsingStatementSyntax : StatementSyntax
{
	private VariableDeclarationSyntax declaration;

	private ExpressionSyntax expression;

	private StatementSyntax statement;

	public SyntaxToken UsingKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingStatementSyntax)Green).usingKeyword, Position, 0);

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingStatementSyntax)Green).openParenToken, GetChildPosition(1), GetChildIndex(1));

	public VariableDeclarationSyntax Declaration => GetRed(ref declaration, 2);

	public ExpressionSyntax Expression => GetRed(ref expression, 3);

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingStatementSyntax)Green).closeParenToken, GetChildPosition(4), GetChildIndex(4));

	public StatementSyntax Statement => GetRed(ref statement, 5);

	internal UsingStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref declaration, 2), 
			3 => GetRed(ref expression, 3), 
			5 => GetRed(ref statement, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)declaration, 
			3 => expression, 
			5 => statement, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitUsingStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitUsingStatement(this);
	}

	public UsingStatementSyntax Update(SyntaxToken usingKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (usingKeyword != UsingKeyword || openParenToken != OpenParenToken || declaration != Declaration || expression != Expression || closeParenToken != CloseParenToken || statement != Statement)
		{
			UsingStatementSyntax usingStatementSyntax = SyntaxFactory.UsingStatement(usingKeyword, openParenToken, declaration, expression, closeParenToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return usingStatementSyntax.WithAnnotations(annotations);
			}
			return usingStatementSyntax;
		}
		return this;
	}

	public UsingStatementSyntax WithUsingKeyword(SyntaxToken usingKeyword)
	{
		return Update(usingKeyword, OpenParenToken, Declaration, Expression, CloseParenToken, Statement);
	}

	public UsingStatementSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(UsingKeyword, openParenToken, Declaration, Expression, CloseParenToken, Statement);
	}

	public UsingStatementSyntax WithDeclaration(VariableDeclarationSyntax declaration)
	{
		return Update(UsingKeyword, OpenParenToken, declaration, Expression, CloseParenToken, Statement);
	}

	public UsingStatementSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(UsingKeyword, OpenParenToken, Declaration, expression, CloseParenToken, Statement);
	}

	public UsingStatementSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(UsingKeyword, OpenParenToken, Declaration, Expression, closeParenToken, Statement);
	}

	public UsingStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(UsingKeyword, OpenParenToken, Declaration, Expression, CloseParenToken, statement);
	}
}
