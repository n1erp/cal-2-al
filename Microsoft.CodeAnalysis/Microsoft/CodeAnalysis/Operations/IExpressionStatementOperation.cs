namespace Microsoft.CodeAnalysis.Operations;

public interface IExpressionStatementOperation : IOperation
{
	IOperation Operation { get; }
}
