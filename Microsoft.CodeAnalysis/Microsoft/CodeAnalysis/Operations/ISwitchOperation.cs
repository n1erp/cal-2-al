using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface ISwitchOperation : IOperation
{
	IOperation Value { get; }

	ImmutableArray<ISwitchCaseOperation> Cases { get; }

	ILabelSymbol ExitLabel { get; }

	ImmutableArray<ILocalSymbol> Locals { get; }
}
