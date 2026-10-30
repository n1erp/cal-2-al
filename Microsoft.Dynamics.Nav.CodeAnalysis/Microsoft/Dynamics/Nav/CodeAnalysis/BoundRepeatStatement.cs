using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundRepeatStatement : BoundLoopStatement, IWhileRepeatLoopStatement, ILoopStatement, IOperation
{
	public BoundStatementList Body { get; private set; }

	public BoundExpression Condition { get; private set; }

	protected override OperationKind StatementKind => OperationKind.LoopStatement;

	LoopKind ILoopStatement.LoopKind => LoopKind.Repeat;

	IOperation IWhileRepeatLoopStatement.Condition => Condition;

	IOperation ILoopStatement.Body => Body;

	public BoundRepeatStatement(SyntaxNode syntax, BoundStatementList body, BoundExpression condition, GeneratedLabelSymbol breakLabel, bool hasErrors = false)
		: base(BoundKind.RepeatStatement, syntax, breakLabel, hasErrors || body.HasErrors() || condition.HasErrors())
	{
		Body = body;
		Condition = condition;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitRepeatStatement(this);
	}

	public BoundRepeatStatement Update(BoundStatementList body, BoundExpression condition, GeneratedLabelSymbol breakLabel)
	{
		if (body != Body || condition != Condition || breakLabel != BreakLabel)
		{
			return new BoundRepeatStatement(Syntax, body, condition, breakLabel, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitWhileRepeatLoopStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitWhileRepeatLoopStatement(this, argument);
	}
}
