namespace Microsoft.CodeAnalysis.Operations;

public interface IMethodBodyBaseOperation : IOperation
{
	IBlockOperation BlockBody { get; }

	IBlockOperation ExpressionBody { get; }
}
