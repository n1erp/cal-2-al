using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IPropertyReferenceOperation : IMemberReferenceOperation, IOperation
{
	IPropertySymbol Property { get; }

	ImmutableArray<IArgumentOperation> Arguments { get; }
}
