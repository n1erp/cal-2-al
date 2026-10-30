namespace Microsoft.CodeAnalysis.Operations;

public interface IDelegateCreationOperation : IOperation
{
	IOperation Target { get; }
}
