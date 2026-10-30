using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class IsolatedStorageClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<IsolatedStorageClassTypeSymbol> instance = new Lazy<IsolatedStorageClassTypeSymbol>(() => new IsolatedStorageClassTypeSymbol("IsolatedStorage", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[6]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Set", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALIsolatedStorage", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Key", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 1),
			new BuiltInParameterSymbol(method, NavCorLib.DataScopeType, "DataScope", 2, isVar: false, isOptional: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("2.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "Get", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALIsolatedStorage", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Key", 0),
			new BuiltInParameterSymbol(method, NavCorLib.DataScopeType, "DataScope", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Value", 2, isVar: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("2.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "Get", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALIsolatedStorage", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Key", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Value", 1, isVar: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("2.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "Contains", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALIsolatedStorage", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Key", 0),
			new BuiltInParameterSymbol(method, NavCorLib.DataScopeType, "DataScope", 1, isVar: false, isOptional: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("2.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "Delete", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALIsolatedStorage", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Key", 0),
			new BuiltInParameterSymbol(method, NavCorLib.DataScopeType, "DataScope", 1, isVar: false, isOptional: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("2.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetEncrypted", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALIsolatedStorage", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Key", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 1),
			new BuiltInParameterSymbol(method, NavCorLib.DataScopeType, "DataScope", 2, isVar: false, isOptional: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("2.0"))
	}), isThreadSafe: true);

	internal static IsolatedStorageClassTypeSymbol Instance => instance.Value;

	private IsolatedStorageClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods, VersionCompatibility.Parse("2.0"))
	{
	}
}
