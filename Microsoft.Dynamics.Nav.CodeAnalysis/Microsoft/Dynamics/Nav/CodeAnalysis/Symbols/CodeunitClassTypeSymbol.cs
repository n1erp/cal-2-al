using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class CodeunitClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<CodeunitClassTypeSymbol> instance = new Lazy<CodeunitClassTypeSymbol>(() => new CodeunitClassTypeSymbol("Codeunit", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[1]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Run", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavCodeunit", "RunCodeunit", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 1, isVar: true, isOptional: true)
		})
	}), isThreadSafe: true);

	internal static CodeunitClassTypeSymbol Instance => instance.Value;

	private CodeunitClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.ApplicationObject, NavTypeKind.Codeunit, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
