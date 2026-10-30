namespace Microsoft.CodeAnalysis.Operations;

public interface IInstanceReferenceOperation : IOperation
{
	InstanceReferenceKind ReferenceKind { get; }
}
