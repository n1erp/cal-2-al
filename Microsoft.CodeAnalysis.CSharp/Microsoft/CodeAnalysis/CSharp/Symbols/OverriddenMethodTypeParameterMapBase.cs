using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class OverriddenMethodTypeParameterMapBase
{
	private readonly SourceOrdinaryMethodSymbol _overridingMethod;

	private TypeMap _lazyTypeMap;

	private MethodSymbol _lazyOverriddenMethod = ErrorMethodSymbol.UnknownMethod;

	public SourceOrdinaryMethodSymbol OverridingMethod => _overridingMethod;

	public TypeMap TypeMap
	{
		get
		{
			if (_lazyTypeMap == null)
			{
				MethodSymbol overriddenMethod = OverriddenMethod;
				if ((object)overriddenMethod != null)
				{
					ImmutableArray<TypeParameterSymbol> typeParameters = overriddenMethod.TypeParameters;
					ImmutableArray<TypeParameterSymbol> typeParameters2 = _overridingMethod.TypeParameters;
					TypeMap value = new TypeMap(typeParameters, typeParameters2, allowAlpha: true);
					Interlocked.CompareExchange(ref _lazyTypeMap, value, null);
				}
			}
			return _lazyTypeMap;
		}
	}

	private MethodSymbol OverriddenMethod
	{
		get
		{
			if ((object)_lazyOverriddenMethod == ErrorMethodSymbol.UnknownMethod)
			{
				Interlocked.CompareExchange(ref _lazyOverriddenMethod, GetOverriddenMethod(_overridingMethod), ErrorMethodSymbol.UnknownMethod);
			}
			return _lazyOverriddenMethod;
		}
	}

	protected OverriddenMethodTypeParameterMapBase(SourceOrdinaryMethodSymbol overridingMethod)
	{
		_overridingMethod = overridingMethod;
	}

	public TypeParameterSymbol GetOverriddenTypeParameter(int ordinal)
	{
		return OverriddenMethod?.TypeParameters[ordinal];
	}

	protected abstract MethodSymbol GetOverriddenMethod(SourceOrdinaryMethodSymbol overridingMethod);
}
