using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface ILoopOperation : IOperation
{
	LoopKind LoopKind { get; }

	IOperation Body { get; }

	ImmutableArray<ILocalSymbol> Locals { get; }

	ILabelSymbol ContinueLabel { get; }

	ILabelSymbol ExitLabel { get; }
}
