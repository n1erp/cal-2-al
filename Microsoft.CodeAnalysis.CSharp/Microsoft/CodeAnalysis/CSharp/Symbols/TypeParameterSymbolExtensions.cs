namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class TypeParameterSymbolExtensions
{
	public static bool DependsOn(this TypeParameterSymbol typeParameter1, TypeParameterSymbol typeParameter2)
	{
		return FunctionExtensions.TransitiveClosure((TypeParameterSymbol x) => x.ConstraintTypesNoUseSiteDiagnostics.OfType<TypeParameterSymbol>(), typeParameter1).Contains(typeParameter2);
	}
}
