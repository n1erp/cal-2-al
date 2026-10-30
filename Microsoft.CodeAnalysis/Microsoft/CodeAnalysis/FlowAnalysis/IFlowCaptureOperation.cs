namespace Microsoft.CodeAnalysis.FlowAnalysis;

public interface IFlowCaptureOperation : IOperation
{
	CaptureId Id { get; }

	IOperation Value { get; }
}
