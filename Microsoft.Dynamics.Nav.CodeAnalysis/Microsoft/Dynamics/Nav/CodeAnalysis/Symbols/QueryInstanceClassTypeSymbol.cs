using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class QueryInstanceClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<QueryInstanceClassTypeSymbol> instance = new Lazy<QueryInstanceClassTypeSymbol>(() => new QueryInstanceClassTypeSymbol("QueryInstance", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[16]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "SetFilter", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Column", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 1),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 2, isVar: false, isOptional: true, isParams: true, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Open", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Read", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Close", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "ColumnName", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Column", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ColumnCaption", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Column", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ColumnNo", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Column", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetFilter", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Column", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetFilters", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetRange", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALSetRangeSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Column", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "FromValue", 1, isVar: false, isOptional: true, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "ToValue", 2, isVar: false, isOptional: true, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsCsv", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Format", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FormatArgument", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsCsv", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "OutStream", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Format", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FormatArgument", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsXml", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsXml", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "OutStream", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TopNumberOfRows", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTopNumberOfRowsToReturn", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "NewRows", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SecurityFiltering", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.SecurityFilterType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.SecurityFilterType, "NewSecurityFiltering", 0, isVar: false, isOptional: true)
		})
	}), isThreadSafe: true);

	internal static QueryInstanceClassTypeSymbol Instance => instance.Value;

	private QueryInstanceClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.ApplicationObject, NavTypeKind.Query, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
