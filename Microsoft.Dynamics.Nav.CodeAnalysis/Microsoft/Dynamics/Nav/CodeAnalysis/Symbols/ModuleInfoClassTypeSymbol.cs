using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class ModuleInfoClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<ModuleInfoClassTypeSymbol> instance = new Lazy<ModuleInfoClassTypeSymbol>(() => new ModuleInfoClassTypeSymbol("ModuleInfo", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[6]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "AppVersion", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.VersionType),
		new BuiltInMethodTypeSymbol(containingSymbol, "DataVersion", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.VersionType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Dependencies", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, () => NavCorLib.ConstructGenericType(ListClassTypeSymbolFactory.Instance, NavCorLib.ModuleDependencyInfoType)),
		new BuiltInMethodTypeSymbol(containingSymbol, "Id", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Name", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Publisher", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType)
	}), isThreadSafe: true);

	internal static ModuleInfoClassTypeSymbol Instance => instance.Value;

	private ModuleInfoClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
