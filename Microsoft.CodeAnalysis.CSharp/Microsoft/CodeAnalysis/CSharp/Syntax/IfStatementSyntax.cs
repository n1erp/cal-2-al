using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class IfStatementSyntax : StatementSyntax
{
	private ExpressionSyntax condition;

	private StatementSyntax statement;

	private ElseClauseSyntax @else;

	public SyntaxToken IfKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IfStatementSyntax)Green).ifKeyword, Position, 0);

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IfStatementSyntax)Green).openParenToken, GetChildPosition(1), GetChildIndex(1));

	public ExpressionSyntax Condition => GetRed(ref condition, 2);

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IfStatementSyntax)Green).closeParenToken, GetChildPosition(3), GetChildIndex(3));

	public StatementSyntax Statement => GetRed(ref statement, 4);

	public ElseClauseSyntax Else => GetRed(ref @else, 5);

	internal IfStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref condition, 2), 
			4 => GetRed(ref statement, 4), 
			5 => GetRed(ref @else, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)condition, 
			4 => statement, 
			5 => @else, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIfStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIfStatement(this);
	}

	public IfStatementSyntax Update(SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax @else)
	{
		if (ifKeyword != IfKeyword || openParenToken != OpenParenToken || condition != Condition || closeParenToken != CloseParenToken || statement != Statement || @else != Else)
		{
			IfStatementSyntax ifStatementSyntax = SyntaxFactory.IfStatement(ifKeyword, openParenToken, condition, closeParenToken, statement, @else);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return ifStatementSyntax.WithAnnotations(annotations);
			}
			return ifStatementSyntax;
		}
		return this;
	}

	public IfStatementSyntax WithIfKeyword(SyntaxToken ifKeyword)
	{
		return Update(ifKeyword, OpenParenToken, Condition, CloseParenToken, Statement, Else);
	}

	public IfStatementSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(IfKeyword, openParenToken, Condition, CloseParenToken, Statement, Else);
	}

	public IfStatementSyntax WithCondition(ExpressionSyntax condition)
	{
		return Update(IfKeyword, OpenParenToken, condition, CloseParenToken, Statement, Else);
	}

	public IfStatementSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(IfKeyword, OpenParenToken, Condition, closeParenToken, Statement, Else);
	}

	public IfStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(IfKeyword, OpenParenToken, Condition, CloseParenToken, statement, Else);
	}

	public IfStatementSyntax WithElse(ElseClauseSyntax @else)
	{
		return Update(IfKeyword, OpenParenToken, Condition, CloseParenToken, Statement, @else);
	}
}
