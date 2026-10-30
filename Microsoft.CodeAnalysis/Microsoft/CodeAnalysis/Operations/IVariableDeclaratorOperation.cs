using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IVariableDeclaratorOperation : IOperation
{
	ILocalSymbol Symbol { get; }

	IVariableInitializerOperation Initializer { get; }

	ImmutableArray<IOperation> IgnoredArguments { get; }
}
