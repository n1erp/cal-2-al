using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundForStatement : BoundLoopStatement, IForLoopStatement, ILoopStatement, IOperation
{
	public BoundExpression LoopVariable { get; private set; }

	public BoundExpression InitialValue { get; private set; }

	public BoundExpression EndValue { get; private set; }

	public BoundStatement Body { get; private set; }

	public bool IsDownto { get; private set; }

	protected override OperationKind StatementKind => OperationKind.LoopStatement;

	LoopKind ILoopStatement.LoopKind => LoopKind.For;

	IOperation ILoopStatement.Body => Body;

	IOperation IForLoopStatement.LoopVariable => LoopVariable;

	IOperation IForLoopStatement.InitialValue => InitialValue;

	IOperation IForLoopStatement.EndValue => EndValue;

	bool IForLoopStatement.IsDownTo => IsDownto;

	public BoundForStatement(SyntaxNode syntax, BoundExpression loopVariable, BoundExpression initialValue, BoundExpression endValue, BoundStatement body, bool isDownto, GeneratedLabelSymbol breakLabel, bool hasErrors = false)
		: base(BoundKind.ForStatement, syntax, breakLabel, hasErrors || loopVariable.HasErrors() || initialValue.HasErrors() || endValue.HasErrors() || body.HasErrors())
	{
		LoopVariable = loopVariable;
		InitialValue = initialValue;
		EndValue = endValue;
		Body = body;
		IsDownto = isDownto;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitForStatement(this);
	}

	public BoundForStatement Update(BoundExpression loopVariable, BoundExpression initialValue, BoundExpression endValue, BoundStatement body, bool isDownto, GeneratedLabelSymbol breakLabel)
	{
		if (loopVariable != LoopVariable || initialValue != InitialValue || endValue != EndValue || body != Body || isDownto != IsDownto || breakLabel != BreakLabel)
		{
			return new BoundForStatement(Syntax, loopVariable, initialValue, endValue, body, isDownto, breakLabel, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitForLoopStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitForLoopStatement(this, argument);
	}
}
