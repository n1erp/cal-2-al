namespace Microsoft.CodeAnalysis.Operations;

public interface IEventReferenceOperation : IMemberReferenceOperation, IOperation
{
	IEventSymbol Event { get; }
}
