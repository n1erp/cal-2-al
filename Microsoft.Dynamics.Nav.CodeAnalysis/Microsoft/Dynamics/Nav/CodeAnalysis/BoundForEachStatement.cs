using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundForEachStatement : BoundLoopStatement, IForEachLoopStatement, ILoopStatement, IOperation
{
	public Conversion ElementConversion { get; private set; }

	public BoundExpression IterationVariable { get; private set; }

	public BoundExpression Expression { get; private set; }

	public BoundStatement Body { get; private set; }

	protected override OperationKind StatementKind => OperationKind.LoopStatement;

	LoopKind ILoopStatement.LoopKind => LoopKind.ForEach;

	IOperation ILoopStatement.Body => Body;

	IOperation IForEachLoopStatement.IterationVariable => IterationVariable;

	IOperation IForEachLoopStatement.Expression => Expression;

	public BoundForEachStatement(SyntaxNode syntax, Conversion elementConversion, BoundExpression iterationVariable, BoundExpression expression, BoundStatement body, GeneratedLabelSymbol breakLabel, bool hasErrors = false)
		: base(BoundKind.ForEachStatement, syntax, breakLabel, hasErrors || iterationVariable.HasErrors() || expression.HasErrors() || body.HasErrors())
	{
		ElementConversion = elementConversion;
		IterationVariable = iterationVariable;
		Expression = expression;
		Body = body;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitForEachStatement(this);
	}

	public BoundForEachStatement Update(Conversion elementConversion, BoundExpression iterationVariable, BoundExpression expression, BoundStatement body, GeneratedLabelSymbol breakLabel)
	{
		if (elementConversion != ElementConversion || iterationVariable != IterationVariable || expression != Expression || body != Body || breakLabel != BreakLabel)
		{
			return new BoundForEachStatement(Syntax, elementConversion, iterationVariable, expression, body, breakLabel, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitForEachLoopStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitForEachLoopStatement(this, argument);
	}
}
