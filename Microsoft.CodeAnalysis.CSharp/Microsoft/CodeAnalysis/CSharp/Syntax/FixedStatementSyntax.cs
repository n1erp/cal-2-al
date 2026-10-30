using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class FixedStatementSyntax : StatementSyntax
{
	private VariableDeclarationSyntax declaration;

	private StatementSyntax statement;

	public SyntaxToken FixedKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FixedStatementSyntax)Green).fixedKeyword, Position, 0);

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FixedStatementSyntax)Green).openParenToken, GetChildPosition(1), GetChildIndex(1));

	public VariableDeclarationSyntax Declaration => GetRed(ref declaration, 2);

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FixedStatementSyntax)Green).closeParenToken, GetChildPosition(3), GetChildIndex(3));

	public StatementSyntax Statement => GetRed(ref statement, 4);

	internal FixedStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref declaration, 2), 
			4 => GetRed(ref statement, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)declaration, 
			4 => statement, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFixedStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFixedStatement(this);
	}

	public FixedStatementSyntax Update(SyntaxToken fixedKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (fixedKeyword != FixedKeyword || openParenToken != OpenParenToken || declaration != Declaration || closeParenToken != CloseParenToken || statement != Statement)
		{
			FixedStatementSyntax fixedStatementSyntax = SyntaxFactory.FixedStatement(fixedKeyword, openParenToken, declaration, closeParenToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fixedStatementSyntax.WithAnnotations(annotations);
			}
			return fixedStatementSyntax;
		}
		return this;
	}

	public FixedStatementSyntax WithFixedKeyword(SyntaxToken fixedKeyword)
	{
		return Update(fixedKeyword, OpenParenToken, Declaration, CloseParenToken, Statement);
	}

	public FixedStatementSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(FixedKeyword, openParenToken, Declaration, CloseParenToken, Statement);
	}

	public FixedStatementSyntax WithDeclaration(VariableDeclarationSyntax declaration)
	{
		return Update(FixedKeyword, OpenParenToken, declaration, CloseParenToken, Statement);
	}

	public FixedStatementSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(FixedKeyword, OpenParenToken, Declaration, closeParenToken, Statement);
	}

	public FixedStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(FixedKeyword, OpenParenToken, Declaration, CloseParenToken, statement);
	}

	public FixedStatementSyntax AddDeclarationVariables(params VariableDeclaratorSyntax[] items)
	{
		return WithDeclaration(Declaration.WithVariables(Declaration.Variables.AddRange(items)));
	}
}
