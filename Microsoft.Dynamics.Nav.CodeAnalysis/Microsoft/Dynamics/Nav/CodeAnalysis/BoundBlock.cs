using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundBlock : BoundStatementList, IBlockStatement, IStatementList, IOperation
{
	private static readonly ConditionalWeakTable<BoundBlock, object> BlockStatementsMappings = new ConditionalWeakTable<BoundBlock, object>();

	ImmutableArray<IOperation> IStatementList.Statements => (ImmutableArray<IOperation>)BlockStatementsMappings.GetValue(this, (BoundBlock blockStatement) => blockStatement.Statements.Cast<BoundStatement, IOperation>().WhereAsArray((IOperation statement) => statement.Kind != OperationKind.None));

	protected override OperationKind StatementKind => OperationKind.BlockStatement;

	public BoundBlock(SyntaxNode syntax, ImmutableArray<BoundStatement> statements, bool hasErrors = false)
		: base(BoundKind.Block, syntax, statements, hasErrors || statements.HasErrors())
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBlock(this);
	}

	public new BoundBlock Update(ImmutableArray<BoundStatement> statements)
	{
		if (statements != Statements)
		{
			return new BoundBlock(Syntax, statements, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitBlockStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitBlockStatement(this, argument);
	}
}
