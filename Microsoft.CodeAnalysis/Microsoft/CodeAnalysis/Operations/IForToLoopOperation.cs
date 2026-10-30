using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IForToLoopOperation : ILoopOperation, IOperation
{
	IOperation LoopControlVariable { get; }

	IOperation InitialValue { get; }

	IOperation LimitValue { get; }

	IOperation StepValue { get; }

	bool IsChecked { get; }

	ImmutableArray<IOperation> NextVariables { get; }
}
