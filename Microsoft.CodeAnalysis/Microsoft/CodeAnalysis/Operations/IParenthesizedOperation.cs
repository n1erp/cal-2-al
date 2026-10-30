namespace Microsoft.CodeAnalysis.Operations;

public interface IParenthesizedOperation : IOperation
{
	IOperation Operand { get; }
}
