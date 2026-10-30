using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundIfStatement : BoundStatement, IIfStatement, IOperation
{
	public BoundExpression Condition { get; private set; }

	public BoundStatement Consequence { get; private set; }

	public BoundStatement AlternativeOpt { get; private set; }

	IOperation IIfStatement.Condition => Condition;

	IOperation IIfStatement.IfTrueStatement => Consequence;

	IOperation IIfStatement.IfFalseStatement => AlternativeOpt;

	protected override OperationKind StatementKind => OperationKind.IfStatement;

	public BoundIfStatement(SyntaxNode syntax, BoundExpression condition, BoundStatement consequence, BoundStatement alternativeOpt, bool hasErrors = false)
		: base(BoundKind.IfStatement, syntax, hasErrors || condition.HasErrors() || consequence.HasErrors() || alternativeOpt.HasErrors())
	{
		Condition = condition;
		Consequence = consequence;
		AlternativeOpt = alternativeOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitIfStatement(this);
	}

	public BoundIfStatement Update(BoundExpression condition, BoundStatement consequence, BoundStatement alternativeOpt)
	{
		if (condition != Condition || consequence != Consequence || alternativeOpt != AlternativeOpt)
		{
			return new BoundIfStatement(Syntax, condition, consequence, alternativeOpt, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitIfStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitIfStatement(this, argument);
	}
}
