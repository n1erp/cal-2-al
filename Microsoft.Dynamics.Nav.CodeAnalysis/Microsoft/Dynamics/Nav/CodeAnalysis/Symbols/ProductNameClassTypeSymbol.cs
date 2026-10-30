using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class ProductNameClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<ProductNameClassTypeSymbol> instance = new Lazy<ProductNameClassTypeSymbol>(() => new ProductNameClassTypeSymbol("ProductName", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[3]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Full", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALProductName", "ALProductNameFull", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Short", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALProductName", "ALProductNameShort", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Marketing", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALProductName", "ALMarketingName", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType)
	}), isThreadSafe: true);

	internal static ProductNameClassTypeSymbol Instance => instance.Value;

	private ProductNameClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
