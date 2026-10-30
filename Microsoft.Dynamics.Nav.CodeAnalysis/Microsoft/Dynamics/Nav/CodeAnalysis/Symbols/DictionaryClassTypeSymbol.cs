using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class DictionaryClassTypeSymbol : GenericLanguageClassTypeSymbol
{
	internal DictionaryClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods, TypeSymbol[] typeArguments)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.Dictionary, SymbolCompilationScope.Personalization, getMethods, typeArguments)
	{
	}
}
