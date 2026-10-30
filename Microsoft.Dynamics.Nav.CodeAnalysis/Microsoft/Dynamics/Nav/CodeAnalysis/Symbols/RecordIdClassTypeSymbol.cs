using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class RecordIdClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<RecordIdClassTypeSymbol> instance = new Lazy<RecordIdClassTypeSymbol>(() => new RecordIdClassTypeSymbol("RecordId", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[2]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "TableNo", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "TableNo", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetRecord", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: true, isLocal: false, isObsolete: false, NavCorLib.RecordRefType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, null, addCompilationTarget: true)
	}), isThreadSafe: true);

	internal static RecordIdClassTypeSymbol Instance => instance.Value;

	private RecordIdClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
