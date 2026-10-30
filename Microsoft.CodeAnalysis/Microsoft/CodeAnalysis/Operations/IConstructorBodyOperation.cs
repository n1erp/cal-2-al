using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IConstructorBodyOperation : IMethodBodyBaseOperation, IOperation
{
	ImmutableArray<ILocalSymbol> Locals { get; }

	IOperation Initializer { get; }
}
