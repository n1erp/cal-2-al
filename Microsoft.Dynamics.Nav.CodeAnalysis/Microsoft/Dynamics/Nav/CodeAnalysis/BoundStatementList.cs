using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class BoundStatementList : BoundStatement, IStatementList, IOperation
{
	private static readonly ConditionalWeakTable<BoundStatementList, object> StatementListMappings = new ConditionalWeakTable<BoundStatementList, object>();

	public ImmutableArray<BoundStatement> Statements { get; private set; }

	ImmutableArray<IOperation> IStatementList.Statements => (ImmutableArray<IOperation>)StatementListMappings.GetValue(this, (BoundStatementList statements) => statements.Statements.Cast<BoundStatement, IOperation>().WhereAsArray((IOperation statement) => statement.Kind != OperationKind.None));

	protected override OperationKind StatementKind => OperationKind.StatementList;

	protected BoundStatementList(BoundKind kind, SyntaxNode syntax, ImmutableArray<BoundStatement> statements, bool hasErrors = false)
		: base(kind, syntax, hasErrors)
	{
		Statements = statements;
	}

	public BoundStatementList(SyntaxNode syntax, ImmutableArray<BoundStatement> statements, bool hasErrors = false)
		: base(BoundKind.StatementList, syntax, hasErrors || statements.HasErrors())
	{
		Statements = statements;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitStatementList(this);
	}

	public BoundStatementList Update(ImmutableArray<BoundStatement> statements)
	{
		if (statements != Statements)
		{
			return new BoundStatementList(Syntax, statements, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitStatementList(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitStatementList(this, argument);
	}
}
