namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class OverriddenMethodTypeParameterMap : OverriddenMethodTypeParameterMapBase
{
	public OverriddenMethodTypeParameterMap(SourceOrdinaryMethodSymbol overridingMethod)
		: base(overridingMethod)
	{
	}

	protected override MethodSymbol GetOverriddenMethod(SourceOrdinaryMethodSymbol overridingMethod)
	{
		MethodSymbol methodSymbol = overridingMethod;
		do
		{
			methodSymbol = methodSymbol.OverriddenMethod;
		}
		while ((object)methodSymbol != null && methodSymbol.IsOverride);
		return methodSymbol;
	}
}
