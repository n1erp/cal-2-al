using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class HttpContentClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<HttpContentClassTypeSymbol> instance = new Lazy<HttpContentClassTypeSymbol>(() => new HttpContentClassTypeSymbol("HttpContent", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[6]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Clear", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALClear"),
		new BuiltInMethodTypeSymbol(containingSymbol, "WriteFrom", SymbolCompilationScope.DefinedByOwner, false, null, "ALLoadFrom", false, false, false, false, false, null, false, false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Text", 0)
		}, false, null, false),
		new BuiltInMethodTypeSymbol(containingSymbol, "WriteFrom", SymbolCompilationScope.DefinedByOwner, false, null, "ALLoadFrom", false, false, false, false, false, null, false, false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 0)
		}, false, null, false),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetHeaders", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.HttpHeadersType, "Headers", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ReadAs", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALReadAs", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "OutputString", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ReadAs", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALReadAs", isProperty: false, addDataError: true, addThis: true, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 0, isVar: true)
		})
	}), isThreadSafe: true);

	internal static HttpContentClassTypeSymbol Instance => instance.Value;

	private HttpContentClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
