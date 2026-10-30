using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class NotificationClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<NotificationClassTypeSymbol> instance = new Lazy<NotificationClassTypeSymbol>(() => new NotificationClassTypeSymbol("Notification", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[9]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Id", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "Id", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "AddAction", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Caption", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "CodeunitID", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "MethodName", 2)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetData", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Message", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Message", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Scope", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NotificationScopeType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.NotificationScopeType, "Scope", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Send", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Recall", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetData", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "HasData", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0)
		})
	}), isThreadSafe: true);

	internal static NotificationClassTypeSymbol Instance => instance.Value;

	private NotificationClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
