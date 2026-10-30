namespace Microsoft.CodeAnalysis.Operations;

public interface ILocalFunctionOperation : IOperation
{
	IMethodSymbol Symbol { get; }

	IBlockOperation Body { get; }

	IBlockOperation IgnoredBody { get; }
}
