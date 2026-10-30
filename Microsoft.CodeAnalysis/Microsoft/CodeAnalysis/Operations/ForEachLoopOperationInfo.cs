using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal struct ForEachLoopOperationInfo(ITypeSymbol elementType, IMethodSymbol getEnumeratorMethod, IPropertySymbol currentProperty, IMethodSymbol moveNextMethod, bool needsDispose, bool knownToImplementIDisposable, IConvertibleConversion currentConversion, IConvertibleConversion elementConversion, Lazy<ImmutableArray<IArgumentOperation>> getEnumeratorArguments = null, Lazy<ImmutableArray<IArgumentOperation>> moveNextArguments = null, Lazy<ImmutableArray<IArgumentOperation>> currentArguments = null)
{
	public readonly ITypeSymbol ElementType = elementType;

	public readonly IMethodSymbol GetEnumeratorMethod = getEnumeratorMethod;

	public readonly IPropertySymbol CurrentProperty = currentProperty;

	public readonly IMethodSymbol MoveNextMethod = moveNextMethod;

	public readonly bool NeedsDispose = needsDispose;

	public readonly bool KnownToImplementIDisposable = knownToImplementIDisposable;

	public readonly IConvertibleConversion CurrentConversion = currentConversion;

	public readonly IConvertibleConversion ElementConversion = elementConversion;

	public readonly Lazy<ImmutableArray<IArgumentOperation>> GetEnumeratorArguments = getEnumeratorArguments;

	public readonly Lazy<ImmutableArray<IArgumentOperation>> MoveNextArguments = moveNextArguments;

	public readonly Lazy<ImmutableArray<IArgumentOperation>> CurrentArguments = currentArguments;
}
