namespace Microsoft.CodeAnalysis.Operations;

public interface IInterpolationOperation : IInterpolatedStringContentOperation, IOperation
{
	IOperation Expression { get; }

	IOperation Alignment { get; }

	IOperation FormatString { get; }
}
