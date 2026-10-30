using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IPropertyInitializerOperation : ISymbolInitializerOperation, IOperation
{
	ImmutableArray<IPropertySymbol> InitializedProperties { get; }
}
