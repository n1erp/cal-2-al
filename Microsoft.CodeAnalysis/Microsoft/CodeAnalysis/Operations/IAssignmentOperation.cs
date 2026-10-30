namespace Microsoft.CodeAnalysis.Operations;

public interface IAssignmentOperation : IOperation
{
	IOperation Target { get; }

	IOperation Value { get; }
}
