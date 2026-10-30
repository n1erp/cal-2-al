using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class RequestPageClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<RequestPageClassTypeSymbol> instance = new Lazy<RequestPageClassTypeSymbol>(() => new RequestPageClassTypeSymbol("RequestPage", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[9]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Editable", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Editable", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "NewEditable", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Caption", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "PageCaption", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NewCaption", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "LookupMode", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "LookupMode", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "NewLookupMode", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ObjectId", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ObjectID", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "UseNames", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveRecord", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "SaveRecord", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Update", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Update", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "SaveRecord", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetSelectionFilter", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "SetSelectionFilter", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Activate", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Activate", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Refresh", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Close", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Close", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true)
	}), isThreadSafe: true);

	internal static RequestPageClassTypeSymbol Instance => instance.Value;

	private RequestPageClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.ApplicationObject, NavTypeKind.RequestPage, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
