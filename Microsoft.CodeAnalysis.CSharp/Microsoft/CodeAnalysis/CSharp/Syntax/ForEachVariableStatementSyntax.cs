using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ForEachVariableStatementSyntax : CommonForEachStatementSyntax
{
	private ExpressionSyntax variable;

	private ExpressionSyntax expression;

	private StatementSyntax statement;

	public override SyntaxToken ForEachKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForEachVariableStatementSyntax)Green).forEachKeyword, Position, 0);

	public override SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForEachVariableStatementSyntax)Green).openParenToken, GetChildPosition(1), GetChildIndex(1));

	public ExpressionSyntax Variable => GetRed(ref variable, 2);

	public override SyntaxToken InKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForEachVariableStatementSyntax)Green).inKeyword, GetChildPosition(3), GetChildIndex(3));

	public override ExpressionSyntax Expression => GetRed(ref expression, 4);

	public override SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForEachVariableStatementSyntax)Green).closeParenToken, GetChildPosition(5), GetChildIndex(5));

	public override StatementSyntax Statement => GetRed(ref statement, 6);

	internal ForEachVariableStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref variable, 2), 
			4 => GetRed(ref expression, 4), 
			6 => GetRed(ref statement, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)variable, 
			4 => expression, 
			6 => statement, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitForEachVariableStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitForEachVariableStatement(this);
	}

	public ForEachVariableStatementSyntax Update(SyntaxToken forEachKeyword, SyntaxToken openParenToken, ExpressionSyntax variable, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (forEachKeyword != ForEachKeyword || openParenToken != OpenParenToken || variable != Variable || inKeyword != InKeyword || expression != Expression || closeParenToken != CloseParenToken || statement != Statement)
		{
			ForEachVariableStatementSyntax forEachVariableStatementSyntax = SyntaxFactory.ForEachVariableStatement(forEachKeyword, openParenToken, variable, inKeyword, expression, closeParenToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return forEachVariableStatementSyntax.WithAnnotations(annotations);
			}
			return forEachVariableStatementSyntax;
		}
		return this;
	}

	internal override CommonForEachStatementSyntax WithForEachKeywordCore(SyntaxToken forEachKeyword)
	{
		return WithForEachKeyword(forEachKeyword);
	}

	public new ForEachVariableStatementSyntax WithForEachKeyword(SyntaxToken forEachKeyword)
	{
		return Update(forEachKeyword, OpenParenToken, Variable, InKeyword, Expression, CloseParenToken, Statement);
	}

	internal override CommonForEachStatementSyntax WithOpenParenTokenCore(SyntaxToken openParenToken)
	{
		return WithOpenParenToken(openParenToken);
	}

	public new ForEachVariableStatementSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(ForEachKeyword, openParenToken, Variable, InKeyword, Expression, CloseParenToken, Statement);
	}

	public ForEachVariableStatementSyntax WithVariable(ExpressionSyntax variable)
	{
		return Update(ForEachKeyword, OpenParenToken, variable, InKeyword, Expression, CloseParenToken, Statement);
	}

	internal override CommonForEachStatementSyntax WithInKeywordCore(SyntaxToken inKeyword)
	{
		return WithInKeyword(inKeyword);
	}

	public new ForEachVariableStatementSyntax WithInKeyword(SyntaxToken inKeyword)
	{
		return Update(ForEachKeyword, OpenParenToken, Variable, inKeyword, Expression, CloseParenToken, Statement);
	}

	internal override CommonForEachStatementSyntax WithExpressionCore(ExpressionSyntax expression)
	{
		return WithExpression(expression);
	}

	public new ForEachVariableStatementSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(ForEachKeyword, OpenParenToken, Variable, InKeyword, expression, CloseParenToken, Statement);
	}

	internal override CommonForEachStatementSyntax WithCloseParenTokenCore(SyntaxToken closeParenToken)
	{
		return WithCloseParenToken(closeParenToken);
	}

	public new ForEachVariableStatementSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(ForEachKeyword, OpenParenToken, Variable, InKeyword, Expression, closeParenToken, Statement);
	}

	internal override CommonForEachStatementSyntax WithStatementCore(StatementSyntax statement)
	{
		return WithStatement(statement);
	}

	public new ForEachVariableStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(ForEachKeyword, OpenParenToken, Variable, InKeyword, Expression, CloseParenToken, statement);
	}
}
