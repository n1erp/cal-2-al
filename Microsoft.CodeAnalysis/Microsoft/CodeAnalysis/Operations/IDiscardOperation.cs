namespace Microsoft.CodeAnalysis.Operations;

public interface IDiscardOperation : IOperation
{
	IDiscardSymbol DiscardSymbol { get; }
}
