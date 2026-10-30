using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundExitStatement : BoundStatement, IExitStatement, IOperation
{
	public BoundExpression ReturnValue { get; private set; }

	IOperation IExitStatement.ReturnedValue => ReturnValue;

	protected override OperationKind StatementKind => OperationKind.ExitStatement;

	public BoundExitStatement(SyntaxNode syntax, BoundExpression returnValue, bool hasErrors = false)
		: base(BoundKind.ExitStatement, syntax, hasErrors || returnValue.HasErrors())
	{
		ReturnValue = returnValue;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitExitStatement(this);
	}

	public BoundExitStatement Update(BoundExpression returnValue)
	{
		if (returnValue != ReturnValue)
		{
			return new BoundExitStatement(Syntax, returnValue, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitExitStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitExitStatement(this, argument);
	}
}
