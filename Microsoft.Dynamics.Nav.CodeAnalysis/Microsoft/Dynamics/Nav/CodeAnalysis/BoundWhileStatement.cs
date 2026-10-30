using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundWhileStatement : BoundLoopStatement, IWhileRepeatLoopStatement, ILoopStatement, IOperation
{
	public BoundExpression Condition { get; private set; }

	public BoundStatement Body { get; private set; }

	protected override OperationKind StatementKind => OperationKind.LoopStatement;

	LoopKind ILoopStatement.LoopKind => LoopKind.While;

	IOperation IWhileRepeatLoopStatement.Condition => Condition;

	IOperation ILoopStatement.Body => Body;

	public BoundWhileStatement(SyntaxNode syntax, BoundExpression condition, BoundStatement body, GeneratedLabelSymbol breakLabel, bool hasErrors = false)
		: base(BoundKind.WhileStatement, syntax, breakLabel, hasErrors || condition.HasErrors() || body.HasErrors())
	{
		Condition = condition;
		Body = body;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitWhileStatement(this);
	}

	public BoundWhileStatement Update(BoundExpression condition, BoundStatement body, GeneratedLabelSymbol breakLabel)
	{
		if (condition != Condition || body != Body || breakLabel != BreakLabel)
		{
			return new BoundWhileStatement(Syntax, condition, body, breakLabel, HasErrors)
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
