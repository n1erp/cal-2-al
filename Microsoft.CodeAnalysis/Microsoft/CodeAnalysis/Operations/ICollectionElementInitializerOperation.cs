using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

[Obsolete("ICollectionElementInitializerOperation has been replaced with IInvocationOperation and IDynamicInvocationOperation", true)]
public interface ICollectionElementInitializerOperation : IOperation
{
	IMethodSymbol AddMethod { get; }

	ImmutableArray<IOperation> Arguments { get; }

	bool IsDynamic { get; }
}
