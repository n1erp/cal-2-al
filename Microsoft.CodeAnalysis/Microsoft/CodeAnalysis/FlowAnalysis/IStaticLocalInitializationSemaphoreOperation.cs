namespace Microsoft.CodeAnalysis.FlowAnalysis;

public interface IStaticLocalInitializationSemaphoreOperation : IOperation
{
	ILocalSymbol Local { get; }
}
