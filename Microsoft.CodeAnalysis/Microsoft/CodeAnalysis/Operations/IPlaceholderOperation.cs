namespace Microsoft.CodeAnalysis.Operations;

internal interface IPlaceholderOperation : IOperation
{
	PlaceholderKind PlaceholderKind { get; }
}
