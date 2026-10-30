using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class XmlportClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<XmlportClassTypeSymbol> instance = new Lazy<XmlportClassTypeSymbol>(() => new XmlportClassTypeSymbol("Xmlport", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[3]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Import", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavXmlPort", "Import", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 1, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 2, isVar: true, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Export", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavXmlPort", "Export", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "OutStream", 1, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 2, isVar: true, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Run", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavXmlPort", "Run", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[4]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "RequestWindow", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Import", 2, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 3, isVar: true, isOptional: true)
		})
	}), isThreadSafe: true);

	internal static XmlportClassTypeSymbol Instance => instance.Value;

	private XmlportClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.ApplicationObject, NavTypeKind.XmlPort, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
