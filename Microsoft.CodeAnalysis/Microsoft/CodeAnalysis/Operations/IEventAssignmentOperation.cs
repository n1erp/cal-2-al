namespace Microsoft.CodeAnalysis.Operations;

public interface IEventAssignmentOperation : IOperation
{
	IOperation EventReference { get; }

	IOperation HandlerValue { get; }

	bool Adds { get; }
}
