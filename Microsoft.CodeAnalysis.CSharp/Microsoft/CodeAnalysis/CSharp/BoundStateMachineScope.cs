using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundStateMachineScope : BoundStatement
{
	public ImmutableArray<StateMachineFieldSymbol> Fields { get; }

	public BoundStatement Statement { get; }

	public BoundStateMachineScope(SyntaxNode syntax, ImmutableArray<StateMachineFieldSymbol> fields, BoundStatement statement, bool hasErrors = false)
		: base(BoundKind.StateMachineScope, syntax, hasErrors || statement.HasErrors())
	{
		Fields = fields;
		Statement = statement;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitStateMachineScope(this);
	}

	public BoundStateMachineScope Update(ImmutableArray<StateMachineFieldSymbol> fields, BoundStatement statement)
	{
		if (fields != Fields || statement != Statement)
		{
			return new BoundStateMachineScope(Syntax, fields, statement, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
