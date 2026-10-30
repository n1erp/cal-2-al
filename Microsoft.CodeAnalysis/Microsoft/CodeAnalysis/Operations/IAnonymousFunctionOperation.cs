namespace Microsoft.CodeAnalysis.Operations;

public interface IAnonymousFunctionOperation : IOperation
{
	IMethodSymbol Symbol { get; }

	IBlockOperation Body { get; }
}
