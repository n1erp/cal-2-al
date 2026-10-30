using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class QueryClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<QueryClassTypeSymbol> instance = new Lazy<QueryClassTypeSymbol>(() => new QueryClassTypeSymbol("Query", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[4]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsCsv", SymbolCompilationScope.Internal, isStatic: true, "NavQuery", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[4]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 1),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Format", 2, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FormatArgument", 3, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsCsv", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavQuery", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[4]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "OutStream", 1),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Format", 2, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FormatArgument", 3, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsXml", SymbolCompilationScope.Internal, isStatic: true, "NavQuery", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsXml", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavQuery", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "OutStream", 1)
		})
	}), isThreadSafe: true);

	internal static QueryClassTypeSymbol Instance => instance.Value;

	private QueryClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.ApplicationObject, NavTypeKind.Query, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
