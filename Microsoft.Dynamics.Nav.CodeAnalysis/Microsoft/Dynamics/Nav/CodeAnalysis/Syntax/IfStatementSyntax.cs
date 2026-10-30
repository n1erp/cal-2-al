using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class IfStatementSyntax : StatementSyntax
{
	private CodeExpressionSyntax _condition;

	private StatementSyntax _statement;

	private StatementSyntax _elseStatement;

	public SyntaxToken IfKeywordToken => new SyntaxToken(this, ((InternalIfStatementSyntax)Node)._ifKeywordToken, Position, 0);

	public CodeExpressionSyntax Condition => GetRed(ref _condition, 1);

	public SyntaxToken ThenKeywordToken => new SyntaxToken(this, ((InternalIfStatementSyntax)Node)._thenKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public StatementSyntax Statement => GetRed(ref _statement, 3);

	public SyntaxToken ElseKeywordToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalIfStatementSyntax)Node)._elseKeywordToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(4), GetChildIndex(4));
			}
			return default;
		}
	}

	public StatementSyntax ElseStatement => GetRed(ref _elseStatement, 5);

	internal IfStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref _condition, 1), 
			3 => GetRed(ref _statement, 3), 
			5 => GetRed(ref _elseStatement, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)_condition, 
			3 => _statement, 
			5 => _elseStatement, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIfStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitIfStatement(this);
	}

	public IfStatementSyntax Update(SyntaxToken ifKeywordToken, CodeExpressionSyntax condition, SyntaxToken thenKeywordToken, StatementSyntax statement, SyntaxToken elseKeywordToken, StatementSyntax elseStatement)
	{
		if (ifKeywordToken != IfKeywordToken || condition != Condition || thenKeywordToken != ThenKeywordToken || statement != Statement || elseKeywordToken != ElseKeywordToken || elseStatement != ElseStatement)
		{
			IfStatementSyntax ifStatementSyntax = SyntaxFactory.IfStatement(ifKeywordToken, condition, thenKeywordToken, statement, elseKeywordToken, elseStatement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return ifStatementSyntax.WithAnnotations(annotations);
			}
			return ifStatementSyntax;
		}
		return this;
	}

	public IfStatementSyntax WithIfKeywordToken(SyntaxToken ifKeywordToken)
	{
		return Update(ifKeywordToken, Condition, ThenKeywordToken, Statement, ElseKeywordToken, ElseStatement);
	}

	public IfStatementSyntax WithCondition(CodeExpressionSyntax condition)
	{
		return Update(IfKeywordToken, condition, ThenKeywordToken, Statement, ElseKeywordToken, ElseStatement);
	}

	public IfStatementSyntax WithThenKeywordToken(SyntaxToken thenKeywordToken)
	{
		return Update(IfKeywordToken, Condition, thenKeywordToken, Statement, ElseKeywordToken, ElseStatement);
	}

	public IfStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(IfKeywordToken, Condition, ThenKeywordToken, statement, ElseKeywordToken, ElseStatement);
	}

	public IfStatementSyntax WithElseKeywordToken(SyntaxToken elseKeywordToken)
	{
		return Update(IfKeywordToken, Condition, ThenKeywordToken, Statement, elseKeywordToken, ElseStatement);
	}

	public IfStatementSyntax WithElseStatement(StatementSyntax elseStatement)
	{
		return Update(IfKeywordToken, Condition, ThenKeywordToken, Statement, ElseKeywordToken, elseStatement);
	}
}
