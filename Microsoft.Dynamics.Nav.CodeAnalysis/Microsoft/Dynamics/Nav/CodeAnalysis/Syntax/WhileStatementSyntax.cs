using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class WhileStatementSyntax : StatementSyntax
{
	private CodeExpressionSyntax _condition;

	private StatementSyntax _statement;

	public SyntaxToken WhileKeywordToken => new SyntaxToken(this, ((InternalWhileStatementSyntax)Node)._whileKeywordToken, Position, 0);

	public CodeExpressionSyntax Condition => GetRed(ref _condition, 1);

	public SyntaxToken DoKeywordToken => new SyntaxToken(this, ((InternalWhileStatementSyntax)Node)._doKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public StatementSyntax Statement => GetRed(ref _statement, 3);

	internal WhileStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref _condition, 1), 
			3 => GetRed(ref _statement, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)_condition, 
			3 => _statement, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWhileStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitWhileStatement(this);
	}

	public WhileStatementSyntax Update(SyntaxToken whileKeywordToken, CodeExpressionSyntax condition, SyntaxToken doKeywordToken, StatementSyntax statement)
	{
		if (whileKeywordToken != WhileKeywordToken || condition != Condition || doKeywordToken != DoKeywordToken || statement != Statement)
		{
			WhileStatementSyntax whileStatementSyntax = SyntaxFactory.WhileStatement(whileKeywordToken, condition, doKeywordToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return whileStatementSyntax.WithAnnotations(annotations);
			}
			return whileStatementSyntax;
		}
		return this;
	}

	public WhileStatementSyntax WithWhileKeywordToken(SyntaxToken whileKeywordToken)
	{
		return Update(whileKeywordToken, Condition, DoKeywordToken, Statement);
	}

	public WhileStatementSyntax WithCondition(CodeExpressionSyntax condition)
	{
		return Update(WhileKeywordToken, condition, DoKeywordToken, Statement);
	}

	public WhileStatementSyntax WithDoKeywordToken(SyntaxToken doKeywordToken)
	{
		return Update(WhileKeywordToken, Condition, doKeywordToken, Statement);
	}

	public WhileStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(WhileKeywordToken, Condition, DoKeywordToken, statement);
	}
}
