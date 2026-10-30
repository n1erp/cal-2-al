using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class ReportClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<ReportClassTypeSymbol> instance = new Lazy<ReportClassTypeSymbol>(() => new ReportClassTypeSymbol("Report", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[16]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Run", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavReport", "Run", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[4]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "RequestWindow", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "SystemPrinter", 2, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 3, isVar: true, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RunModal", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavReport", "RunModal", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[4]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "RequestWindow", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "SystemPrinter", 2, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 3, isVar: true, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsHtml", SymbolCompilationScope.Internal, isStatic: true, "NavReport", "SaveAsHtml", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 1),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 2, isVar: true, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsXml", SymbolCompilationScope.Internal, isStatic: true, "NavReport", "SaveAsXml", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 1),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 2, isVar: true, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsPdf", SymbolCompilationScope.Internal, isStatic: true, "NavReport", "SaveAsPdf", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 1),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 2, isVar: true, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsExcel", SymbolCompilationScope.Internal, isStatic: true, "NavReport", "SaveAsExcel", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 1),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 2, isVar: true, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsWord", SymbolCompilationScope.Internal, isStatic: true, "NavReport", "SaveAsWord", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 1),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 2, isVar: true, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "WordXmlPart", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavReport", "WordXmlPart", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "ExtendedFormat", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "WordLayout", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavReport", "WordLayout", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RdlcLayout", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavReport", "RDLCLayout", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DefaultLayout", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavReport", "DefaultLayout", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DefaultLayoutType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RunRequestPage", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavReport", "RunRequestPage", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Parameters", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Execute", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavReport", "Execute", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Parameters", 1),
			new BuiltInParameterSymbol(method, NavCorLib.RecordRefType, "RecordRef", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Print", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavReport", "Print", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[4]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Parameters", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "PrinterName", 2, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.RecordRefType, "RecordRef", 3, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAs", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavReport", "SaveAs", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[5]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Parameters", 1),
			new BuiltInParameterSymbol(method, NavCorLib.ReportFormatType, "Format", 2),
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "OutStream", 3, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.RecordRefType, "RecordRef", 4, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetSubstituteReportId", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavReport", "GetSubstituteReportId", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "ReportId", 0)
		}, addCompilationTarget: false, VersionCompatibility.Parse("2.0"))
	}), isThreadSafe: true);

	internal static ReportClassTypeSymbol Instance => instance.Value;

	private ReportClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.ApplicationObject, NavTypeKind.Report, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
