namespace Microsoft.CodeAnalysis.Operations;

internal interface IWithOperation : IOperation
{
	IOperation Body { get; }

	IOperation Value { get; }
}
