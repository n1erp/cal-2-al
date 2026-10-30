using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ForStatementSyntax : StatementSyntax
{
	private CodeExpressionSyntax _loopVariable;

	private CodeExpressionSyntax _initialValue;

	private CodeExpressionSyntax _endValue;

	private StatementSyntax _statement;

	public SyntaxToken ForKeywordToken => new SyntaxToken(this, ((InternalForStatementSyntax)Node)._forKeywordToken, Position, 0);

	public CodeExpressionSyntax LoopVariable => GetRed(ref _loopVariable, 1);

	public SyntaxToken AssignToken => new SyntaxToken(this, ((InternalForStatementSyntax)Node)._assignToken, GetChildPosition(2), GetChildIndex(2));

	public CodeExpressionSyntax InitialValue => GetRed(ref _initialValue, 3);

	public SyntaxToken OperatorKeywordToken => new SyntaxToken(this, ((InternalForStatementSyntax)Node)._operatorKeywordToken, GetChildPosition(4), GetChildIndex(4));

	public CodeExpressionSyntax EndValue => GetRed(ref _endValue, 5);

	public SyntaxToken DoKeywordToken => new SyntaxToken(this, ((InternalForStatementSyntax)Node)._doKeywordToken, GetChildPosition(6), GetChildIndex(6));

	public StatementSyntax Statement => GetRed(ref _statement, 7);

	internal ForStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref _loopVariable, 1), 
			3 => GetRed(ref _initialValue, 3), 
			5 => GetRed(ref _endValue, 5), 
			7 => GetRed(ref _statement, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)_loopVariable, 
			3 => _initialValue, 
			5 => _endValue, 
			7 => _statement, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitForStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitForStatement(this);
	}

	public ForStatementSyntax Update(SyntaxToken forKeywordToken, CodeExpressionSyntax loopVariable, SyntaxToken assignToken, CodeExpressionSyntax initialValue, SyntaxToken operatorKeywordToken, CodeExpressionSyntax endValue, SyntaxToken doKeywordToken, StatementSyntax statement)
	{
		if (forKeywordToken != ForKeywordToken || loopVariable != LoopVariable || assignToken != AssignToken || initialValue != InitialValue || operatorKeywordToken != OperatorKeywordToken || endValue != EndValue || doKeywordToken != DoKeywordToken || statement != Statement)
		{
			ForStatementSyntax forStatementSyntax = SyntaxFactory.ForStatement(forKeywordToken, loopVariable, assignToken, initialValue, operatorKeywordToken, endValue, doKeywordToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return forStatementSyntax.WithAnnotations(annotations);
			}
			return forStatementSyntax;
		}
		return this;
	}

	public ForStatementSyntax WithForKeywordToken(SyntaxToken forKeywordToken)
	{
		return Update(forKeywordToken, LoopVariable, AssignToken, InitialValue, OperatorKeywordToken, EndValue, DoKeywordToken, Statement);
	}

	public ForStatementSyntax WithLoopVariable(CodeExpressionSyntax loopVariable)
	{
		return Update(ForKeywordToken, loopVariable, AssignToken, InitialValue, OperatorKeywordToken, EndValue, DoKeywordToken, Statement);
	}

	public ForStatementSyntax WithAssignToken(SyntaxToken assignToken)
	{
		return Update(ForKeywordToken, LoopVariable, assignToken, InitialValue, OperatorKeywordToken, EndValue, DoKeywordToken, Statement);
	}

	public ForStatementSyntax WithInitialValue(CodeExpressionSyntax initialValue)
	{
		return Update(ForKeywordToken, LoopVariable, AssignToken, initialValue, OperatorKeywordToken, EndValue, DoKeywordToken, Statement);
	}

	public ForStatementSyntax WithOperatorKeywordToken(SyntaxToken operatorKeywordToken)
	{
		return Update(ForKeywordToken, LoopVariable, AssignToken, InitialValue, operatorKeywordToken, EndValue, DoKeywordToken, Statement);
	}

	public ForStatementSyntax WithEndValue(CodeExpressionSyntax endValue)
	{
		return Update(ForKeywordToken, LoopVariable, AssignToken, InitialValue, OperatorKeywordToken, endValue, DoKeywordToken, Statement);
	}

	public ForStatementSyntax WithDoKeywordToken(SyntaxToken doKeywordToken)
	{
		return Update(ForKeywordToken, LoopVariable, AssignToken, InitialValue, OperatorKeywordToken, EndValue, doKeywordToken, Statement);
	}

	public ForStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(ForKeywordToken, LoopVariable, AssignToken, InitialValue, OperatorKeywordToken, EndValue, DoKeywordToken, statement);
	}
}
