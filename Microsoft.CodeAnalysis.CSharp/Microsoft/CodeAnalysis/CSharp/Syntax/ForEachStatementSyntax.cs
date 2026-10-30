using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ForEachStatementSyntax : CommonForEachStatementSyntax
{
	private TypeSyntax type;

	private ExpressionSyntax expression;

	private StatementSyntax statement;

	public override SyntaxToken ForEachKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForEachStatementSyntax)Green).forEachKeyword, Position, 0);

	public override SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForEachStatementSyntax)Green).openParenToken, GetChildPosition(1), GetChildIndex(1));

	public TypeSyntax Type => GetRed(ref type, 2);

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForEachStatementSyntax)Green).identifier, GetChildPosition(3), GetChildIndex(3));

	public override SyntaxToken InKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForEachStatementSyntax)Green).inKeyword, GetChildPosition(4), GetChildIndex(4));

	public override ExpressionSyntax Expression => GetRed(ref expression, 5);

	public override SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForEachStatementSyntax)Green).closeParenToken, GetChildPosition(6), GetChildIndex(6));

	public override StatementSyntax Statement => GetRed(ref statement, 7);

	internal ForEachStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref type, 2), 
			5 => GetRed(ref expression, 5), 
			7 => GetRed(ref statement, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)type, 
			5 => expression, 
			7 => statement, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitForEachStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitForEachStatement(this);
	}

	public ForEachStatementSyntax Update(SyntaxToken forEachKeyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (forEachKeyword != ForEachKeyword || openParenToken != OpenParenToken || type != Type || identifier != Identifier || inKeyword != InKeyword || expression != Expression || closeParenToken != CloseParenToken || statement != Statement)
		{
			ForEachStatementSyntax forEachStatementSyntax = SyntaxFactory.ForEachStatement(forEachKeyword, openParenToken, type, identifier, inKeyword, expression, closeParenToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return forEachStatementSyntax.WithAnnotations(annotations);
			}
			return forEachStatementSyntax;
		}
		return this;
	}

	internal override CommonForEachStatementSyntax WithForEachKeywordCore(SyntaxToken forEachKeyword)
	{
		return WithForEachKeyword(forEachKeyword);
	}

	public new ForEachStatementSyntax WithForEachKeyword(SyntaxToken forEachKeyword)
	{
		return Update(forEachKeyword, OpenParenToken, Type, Identifier, InKeyword, Expression, CloseParenToken, Statement);
	}

	internal override CommonForEachStatementSyntax WithOpenParenTokenCore(SyntaxToken openParenToken)
	{
		return WithOpenParenToken(openParenToken);
	}

	public new ForEachStatementSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(ForEachKeyword, openParenToken, Type, Identifier, InKeyword, Expression, CloseParenToken, Statement);
	}

	public ForEachStatementSyntax WithType(TypeSyntax type)
	{
		return Update(ForEachKeyword, OpenParenToken, type, Identifier, InKeyword, Expression, CloseParenToken, Statement);
	}

	public ForEachStatementSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(ForEachKeyword, OpenParenToken, Type, identifier, InKeyword, Expression, CloseParenToken, Statement);
	}

	internal override CommonForEachStatementSyntax WithInKeywordCore(SyntaxToken inKeyword)
	{
		return WithInKeyword(inKeyword);
	}

	public new ForEachStatementSyntax WithInKeyword(SyntaxToken inKeyword)
	{
		return Update(ForEachKeyword, OpenParenToken, Type, Identifier, inKeyword, Expression, CloseParenToken, Statement);
	}

	internal override CommonForEachStatementSyntax WithExpressionCore(ExpressionSyntax expression)
	{
		return WithExpression(expression);
	}

	public new ForEachStatementSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(ForEachKeyword, OpenParenToken, Type, Identifier, InKeyword, expression, CloseParenToken, Statement);
	}

	internal override CommonForEachStatementSyntax WithCloseParenTokenCore(SyntaxToken closeParenToken)
	{
		return WithCloseParenToken(closeParenToken);
	}

	public new ForEachStatementSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(ForEachKeyword, OpenParenToken, Type, Identifier, InKeyword, Expression, closeParenToken, Statement);
	}

	internal override CommonForEachStatementSyntax WithStatementCore(StatementSyntax statement)
	{
		return WithStatement(statement);
	}

	public new ForEachStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(ForEachKeyword, OpenParenToken, Type, Identifier, InKeyword, Expression, CloseParenToken, statement);
	}
}
