using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class ListClassTypeSymbol : GenericLanguageClassTypeSymbol
{
	internal ListClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods, TypeSymbol[] typeArguments)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.List, SymbolCompilationScope.Personalization, getMethods, typeArguments)
	{
	}
}
