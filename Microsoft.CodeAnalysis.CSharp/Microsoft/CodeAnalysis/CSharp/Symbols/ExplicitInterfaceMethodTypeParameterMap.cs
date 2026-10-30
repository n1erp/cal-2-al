using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ExplicitInterfaceMethodTypeParameterMap : OverriddenMethodTypeParameterMapBase
{
	public ExplicitInterfaceMethodTypeParameterMap(SourceOrdinaryMethodSymbol implementationMethod)
		: base(implementationMethod)
	{
	}

	protected override MethodSymbol GetOverriddenMethod(SourceOrdinaryMethodSymbol overridingMethod)
	{
		ImmutableArray<MethodSymbol> explicitInterfaceImplementations = overridingMethod.ExplicitInterfaceImplementations;
		if (explicitInterfaceImplementations.Length <= 0)
		{
			return null;
		}
		return explicitInterfaceImplementations[0];
	}
}
