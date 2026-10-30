using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class HttpClientClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<HttpClientClassTypeSymbol> instance = new Lazy<HttpClientClassTypeSymbol>(() => new HttpClientClassTypeSymbol("HttpClient", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[13]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Clear", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALClear"),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetBaseAddress", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALGetBaseAddress", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetBaseAddress", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALSetBaseAddress", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NewBaseAddress", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DefaultRequestHeaders", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALDefaultRequestHeaders", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.HttpHeadersType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Timeout", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTimeout", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DurationType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DurationType, "SetTimeout", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Get", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALGet", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Path", 0),
			new BuiltInParameterSymbol(method, NavCorLib.HttpResponseMessageType, "Response", 1, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Delete", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALDelete", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Path", 0),
			new BuiltInParameterSymbol(method, NavCorLib.HttpResponseMessageType, "Response", 1, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Post", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALPost", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Path", 0),
			new BuiltInParameterSymbol(method, NavCorLib.HttpContentType, "Content", 1),
			new BuiltInParameterSymbol(method, NavCorLib.HttpResponseMessageType, "Response", 2, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Put", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALPut", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Path", 0),
			new BuiltInParameterSymbol(method, NavCorLib.HttpContentType, "Content", 1),
			new BuiltInParameterSymbol(method, NavCorLib.HttpResponseMessageType, "Response", 2, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Send", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALSend", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.HttpRequestMessageType, "Request", 0),
			new BuiltInParameterSymbol(method, NavCorLib.HttpResponseMessageType, "Response", 1, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "AddCertificate", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALAddCertificate", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Certificate", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Password", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "UseDefaultNetworkWindowsAuthentication", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, null, addCompilationTarget: false, VersionCompatibility.Parse("3.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "UseWindowsAuthentication", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "UserName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Password", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Domain", 2, isVar: false, isOptional: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("3.0"))
	}), isThreadSafe: true);

	internal static HttpClientClassTypeSymbol Instance => instance.Value;

	private HttpClientClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
