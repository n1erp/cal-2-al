using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class HttpRequestMessageClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<HttpRequestMessageClassTypeSymbol> instance = new Lazy<HttpRequestMessageClassTypeSymbol>(() => new HttpRequestMessageClassTypeSymbol("HttpRequestMessage", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[5]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Content", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALContent", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.HttpContentType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.HttpContentType, "SetContent", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Method", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALMethod", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NewMethod", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetRequestUri", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetRequestUri", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "RequestUri", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetHeaders", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.HttpHeadersType, "Headers", 0, isVar: true)
		})
	}), isThreadSafe: true);

	internal static HttpRequestMessageClassTypeSymbol Instance => instance.Value;

	private HttpRequestMessageClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
