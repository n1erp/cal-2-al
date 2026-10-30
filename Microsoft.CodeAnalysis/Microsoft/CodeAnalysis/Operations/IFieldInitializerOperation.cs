using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IFieldInitializerOperation : ISymbolInitializerOperation, IOperation
{
	ImmutableArray<IFieldSymbol> InitializedFields { get; }
}
