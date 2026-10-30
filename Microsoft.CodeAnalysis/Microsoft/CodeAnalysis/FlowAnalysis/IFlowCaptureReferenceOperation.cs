namespace Microsoft.CodeAnalysis.FlowAnalysis;

public interface IFlowCaptureReferenceOperation : IOperation
{
	CaptureId Id { get; }
}
