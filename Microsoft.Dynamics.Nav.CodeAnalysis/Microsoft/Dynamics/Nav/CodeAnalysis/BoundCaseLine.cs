using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundCaseLine : BoundStatement, ICaseLineOperation, IOperation
{
	public ImmutableArray<BoundExpression> Expressions { get; private set; }

	public BoundStatement Statement { get; private set; }

	ImmutableArray<IOperation> ICaseLineOperation.Expressions => Expressions.As<IOperation>();

	IOperation ICaseLineOperation.Statement => Statement;

	protected override OperationKind StatementKind => OperationKind.CaseLine;

	public BoundCaseLine(SyntaxNode syntax, ImmutableArray<BoundExpression> expressions, BoundStatement statement, bool hasErrors = false)
		: base(BoundKind.CaseLine, syntax, hasErrors || expressions.HasErrors() || statement.HasErrors())
	{
		Expressions = expressions;
		Statement = statement;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCaseLine(this);
	}

	public BoundCaseLine Update(ImmutableArray<BoundExpression> expressions, BoundStatement statement)
	{
		if (expressions != Expressions || statement != Statement)
		{
			return new BoundCaseLine(Syntax, expressions, statement, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitCaseLine(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitCaseLine(this, argument);
	}
}
