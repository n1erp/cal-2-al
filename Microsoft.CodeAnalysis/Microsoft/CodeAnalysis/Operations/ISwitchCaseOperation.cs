using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface ISwitchCaseOperation : IOperation
{
	ImmutableArray<ICaseClauseOperation> Clauses { get; }

	ImmutableArray<IOperation> Body { get; }

	ImmutableArray<ILocalSymbol> Locals { get; }
}
