namespace Microsoft.CodeAnalysis.FlowAnalysis;

public interface IFlowAnonymousFunctionOperation : IOperation
{
	IMethodSymbol Symbol { get; }
}
