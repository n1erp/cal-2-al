namespace Microsoft.CodeAnalysis.Operations;

internal interface IAggregateQueryOperation : IOperation
{
	IOperation Group { get; }

	IOperation Aggregation { get; }
}
