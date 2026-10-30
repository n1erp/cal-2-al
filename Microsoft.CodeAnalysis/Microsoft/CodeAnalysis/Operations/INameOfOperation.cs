namespace Microsoft.CodeAnalysis.Operations;

public interface INameOfOperation : IOperation
{
	IOperation Argument { get; }
}
