using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class KeyRefClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<KeyRefClassTypeSymbol> instance = new Lazy<KeyRefClassTypeSymbol>(() => new KeyRefClassTypeSymbol("KeyRef", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[4]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "FieldCount", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "FieldIndex", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: true, isLocal: false, isObsolete: false, NavCorLib.FieldRefType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Record", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: true, isLocal: false, isObsolete: false, NavCorLib.RecordRefType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Active", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType)
	}), isThreadSafe: true);

	internal static KeyRefClassTypeSymbol Instance => instance.Value;

	private KeyRefClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
