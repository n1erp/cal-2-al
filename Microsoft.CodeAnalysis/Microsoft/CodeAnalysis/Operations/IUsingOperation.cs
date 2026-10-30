using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IUsingOperation : IOperation
{
	IOperation Body { get; }

	IOperation Resources { get; }

	ImmutableArray<ILocalSymbol> Locals { get; }
}
