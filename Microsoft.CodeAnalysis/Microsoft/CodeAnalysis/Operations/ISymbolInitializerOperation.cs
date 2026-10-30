using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface ISymbolInitializerOperation : IOperation
{
	ImmutableArray<ILocalSymbol> Locals { get; }

	IOperation Value { get; }
}
