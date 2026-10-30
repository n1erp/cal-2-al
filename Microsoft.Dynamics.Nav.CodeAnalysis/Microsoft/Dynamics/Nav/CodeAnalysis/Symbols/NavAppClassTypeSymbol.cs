using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class NavAppClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<NavAppClassTypeSymbol> instance = new Lazy<NavAppClassTypeSymbol>(() => new NavAppClassTypeSymbol("NavApp", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[8]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "GetArchiveRecordRef", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALNavApp", "ALNavAppGetArchiveRecordRef", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "TableNo", 0),
			new BuiltInParameterSymbol(method, NavCorLib.RecordRefType, "RecordRef", 1, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetArchiveVersion", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALNavApp", "ALNavAppGetArchiveVersion", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "RestoreArchiveData", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALNavApp", "ALNavAppRestoreArchiveData", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "TableNo", 0),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "RunTrigger", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DeleteArchiveData", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALNavApp", "ALNavAppDeleteArchiveData", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "TableNo", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "LoadPackageData", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALNavApp", "ALNavAppLoadPackageData", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "TableNo", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsInstalling", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALNavApp", "ALNavAppIsInstalling", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetCurrentModuleInfo", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALNavApp", "ALGetCurrentModuleInfo", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.ModuleInfoType, "Info", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetModuleInfo", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALNavApp", "ALGetModuleInfo", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "AppId", 0),
			new BuiltInParameterSymbol(method, NavCorLib.ModuleInfoType, "Info", 1, isVar: true)
		})
	}), isThreadSafe: true);

	internal static NavAppClassTypeSymbol Instance => instance.Value;

	private NavAppClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
