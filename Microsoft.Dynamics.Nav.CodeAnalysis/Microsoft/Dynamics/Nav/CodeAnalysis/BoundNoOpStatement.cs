using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundNoOpStatement : BoundStatement, IEmptyStatement, IOperation
{
	protected override OperationKind StatementKind => OperationKind.EmptyStatement;

	public BoundNoOpStatement(SyntaxNode syntax, bool hasErrors)
		: base(BoundKind.NoOpStatement, syntax, hasErrors)
	{
	}

	public BoundNoOpStatement(SyntaxNode syntax)
		: base(BoundKind.NoOpStatement, syntax)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNoOpStatement(this);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitEmptyStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitEmptyStatement(this, argument);
	}
}
