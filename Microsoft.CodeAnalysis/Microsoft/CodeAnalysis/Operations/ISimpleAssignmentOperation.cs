namespace Microsoft.CodeAnalysis.Operations;

public interface ISimpleAssignmentOperation : IAssignmentOperation, IOperation
{
	bool IsRef { get; }
}
