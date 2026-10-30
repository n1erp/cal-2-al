using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class HttpResponseMessageClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<HttpResponseMessageClassTypeSymbol> instance = new Lazy<HttpResponseMessageClassTypeSymbol>(() => new HttpResponseMessageClassTypeSymbol("HttpResponseMessage", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[6]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Content", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALContent", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.HttpContentType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Headers", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALHeaders", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.HttpHeadersType),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsSuccessStatusCode", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALIsSuccessStatusCode", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsBlockedByEnvironment", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, null, addCompilationTarget: false, VersionCompatibility.Parse("2.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "ReasonPhrase", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALReasonPhrase", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "HttpStatusCode", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALHttpStatusCode", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType)
	}), isThreadSafe: true);

	internal static HttpResponseMessageClassTypeSymbol Instance => instance.Value;

	private HttpResponseMessageClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
