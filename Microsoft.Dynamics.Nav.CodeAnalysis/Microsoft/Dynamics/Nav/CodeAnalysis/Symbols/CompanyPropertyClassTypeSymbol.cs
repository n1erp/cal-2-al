using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class CompanyPropertyClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<CompanyPropertyClassTypeSymbol> instance = new Lazy<CompanyPropertyClassTypeSymbol>(() => new CompanyPropertyClassTypeSymbol("CompanyProperty", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[2]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "DisplayName", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALCompanyProperty", "ALDisplayName", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "UrlName", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALCompanyProperty", "ALUrlName", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType)
	}), isThreadSafe: true);

	internal static CompanyPropertyClassTypeSymbol Instance => instance.Value;

	private CompanyPropertyClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
