using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class CommonForEachStatementSyntax : StatementSyntax
{
	public abstract SyntaxToken ForEachKeyword { get; }

	public abstract SyntaxToken OpenParenToken { get; }

	public abstract SyntaxToken InKeyword { get; }

	public abstract ExpressionSyntax Expression { get; }

	public abstract SyntaxToken CloseParenToken { get; }

	public abstract StatementSyntax Statement { get; }

	internal CommonForEachStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public CommonForEachStatementSyntax WithForEachKeyword(SyntaxToken forEachKeyword)
	{
		return WithForEachKeywordCore(forEachKeyword);
	}

	internal abstract CommonForEachStatementSyntax WithForEachKeywordCore(SyntaxToken forEachKeyword);

	public CommonForEachStatementSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return WithOpenParenTokenCore(openParenToken);
	}

	internal abstract CommonForEachStatementSyntax WithOpenParenTokenCore(SyntaxToken openParenToken);

	public CommonForEachStatementSyntax WithInKeyword(SyntaxToken inKeyword)
	{
		return WithInKeywordCore(inKeyword);
	}

	internal abstract CommonForEachStatementSyntax WithInKeywordCore(SyntaxToken inKeyword);

	public CommonForEachStatementSyntax WithExpression(ExpressionSyntax expression)
	{
		return WithExpressionCore(expression);
	}

	internal abstract CommonForEachStatementSyntax WithExpressionCore(ExpressionSyntax expression);

	public CommonForEachStatementSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return WithCloseParenTokenCore(closeParenToken);
	}

	internal abstract CommonForEachStatementSyntax WithCloseParenTokenCore(SyntaxToken closeParenToken);

	public CommonForEachStatementSyntax WithStatement(StatementSyntax statement)
	{
		return WithStatementCore(statement);
	}

	internal abstract CommonForEachStatementSyntax WithStatementCore(StatementSyntax statement);
}
