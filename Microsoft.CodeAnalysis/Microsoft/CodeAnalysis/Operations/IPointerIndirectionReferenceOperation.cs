namespace Microsoft.CodeAnalysis.Operations;

internal interface IPointerIndirectionReferenceOperation : IOperation
{
	IOperation Pointer { get; }
}
