using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class VersionClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<VersionClassTypeSymbol> instance = new Lazy<VersionClassTypeSymbol>(() => new VersionClassTypeSymbol("Version", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[6]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Create", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavVersion", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.VersionType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Version", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Create", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavVersion", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.VersionType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[4]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Major", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Minor", 1),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Build", 2, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Revision", 3, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Major", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Minor", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Build", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Revision", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType)
	}), isThreadSafe: true);

	internal static VersionClassTypeSymbol Instance => instance.Value;

	private VersionClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
