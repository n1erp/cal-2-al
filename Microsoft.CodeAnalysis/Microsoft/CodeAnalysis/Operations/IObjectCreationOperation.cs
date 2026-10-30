using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IObjectCreationOperation : IOperation
{
	IMethodSymbol Constructor { get; }

	ImmutableArray<IArgumentOperation> Arguments { get; }

	IObjectOrCollectionInitializerOperation Initializer { get; }
}
