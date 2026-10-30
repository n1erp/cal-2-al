using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class ModuleDependencyInfoClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<ModuleDependencyInfoClassTypeSymbol> instance = new Lazy<ModuleDependencyInfoClassTypeSymbol>(() => new ModuleDependencyInfoClassTypeSymbol("ModuleDependencyInfo", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[3]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Id", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Name", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Publisher", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType)
	}), isThreadSafe: true);

	internal static ModuleDependencyInfoClassTypeSymbol Instance => instance.Value;

	private ModuleDependencyInfoClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
