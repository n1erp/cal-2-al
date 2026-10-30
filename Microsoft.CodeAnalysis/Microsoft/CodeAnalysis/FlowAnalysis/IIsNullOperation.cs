namespace Microsoft.CodeAnalysis.FlowAnalysis;

public interface IIsNullOperation : IOperation
{
	IOperation Operand { get; }
}
