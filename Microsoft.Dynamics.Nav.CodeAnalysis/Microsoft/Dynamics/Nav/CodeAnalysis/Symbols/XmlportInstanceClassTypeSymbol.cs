using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class XmlportInstanceClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<XmlportInstanceClassTypeSymbol> instance = new Lazy<XmlportInstanceClassTypeSymbol>(() => new XmlportInstanceClassTypeSymbol("XmlportInstance", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[18]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "CurrentPath", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "CurrentPath", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Export", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Export", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Import", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Import", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Run", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Run", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "FieldDelimiter", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "FieldDelimiter", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Delimiter", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "FieldSeparator", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "FieldSeparator", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Separator", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RecordSeparator", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "RecordSeparator", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Separator", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TableSeparator", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "TableSeparator", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Separator", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Filename", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "FileName", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TextEncoding", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "TextEncoding", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TextEncodingType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextEncodingType, "Encoding", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetSource", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Source", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetDestination", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Destination", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "OutStream", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ImportFile", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ImportFile", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "ImportFile", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetTableView", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "SetTableView", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Break", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Break", isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "BreakUnbound", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "BreakUnbound", isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Skip", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Skip", isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Quit", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Quit", isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true)
	}), isThreadSafe: true);

	internal static XmlportInstanceClassTypeSymbol Instance => instance.Value;

	private XmlportInstanceClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.ApplicationObject, NavTypeKind.XmlPort, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
