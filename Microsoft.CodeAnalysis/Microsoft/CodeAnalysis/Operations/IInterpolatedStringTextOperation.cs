namespace Microsoft.CodeAnalysis.Operations;

public interface IInterpolatedStringTextOperation : IInterpolatedStringContentOperation, IOperation
{
	IOperation Text { get; }
}
