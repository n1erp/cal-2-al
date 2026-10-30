using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundAssertErrorStatement : BoundStatement, IAssertErrorStatement, IOperation
{
	public BoundStatement Statement { get; private set; }

	IOperation IAssertErrorStatement.Statement => Statement;

	protected override OperationKind StatementKind => OperationKind.AssertErrorStatement;

	public BoundAssertErrorStatement(SyntaxNode syntax, BoundStatement statement, bool hasErrors = false)
		: base(BoundKind.AssertErrorStatement, syntax, hasErrors || statement.HasErrors())
	{
		Statement = statement;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAssertErrorStatement(this);
	}

	public BoundAssertErrorStatement Update(BoundStatement statement)
	{
		if (statement != Statement)
		{
			return new BoundAssertErrorStatement(Syntax, statement, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitAssertErrorStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitAssertErrorStatement(this, argument);
	}
}
