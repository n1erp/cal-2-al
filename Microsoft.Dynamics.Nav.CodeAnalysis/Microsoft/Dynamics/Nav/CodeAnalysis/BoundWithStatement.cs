using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundWithStatement : BoundStatement, IWithStatement, IOperation
{
	public BoundExpression WithId { get; private set; }

	public BoundStatement Statement { get; private set; }

	protected override OperationKind StatementKind => OperationKind.WithStatement;

	IOperation IWithStatement.Value => WithId;

	IOperation IWithStatement.Body => Statement;

	public BoundWithStatement(SyntaxNode syntax, BoundExpression withId, BoundStatement statement, bool hasErrors = false)
		: base(BoundKind.WithStatement, syntax, hasErrors || withId.HasErrors() || statement.HasErrors())
	{
		WithId = withId;
		Statement = statement;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitWithStatement(this);
	}

	public BoundWithStatement Update(BoundExpression withId, BoundStatement statement)
	{
		if (withId != WithId || statement != Statement)
		{
			return new BoundWithStatement(Syntax, withId, statement, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitWithStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitWithStatement(this, argument);
	}
}
