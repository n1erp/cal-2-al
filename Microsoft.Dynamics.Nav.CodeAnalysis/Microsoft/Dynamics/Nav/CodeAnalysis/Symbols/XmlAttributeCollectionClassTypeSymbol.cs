using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class XmlAttributeCollectionClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<XmlAttributeCollectionClassTypeSymbol> instance = new Lazy<XmlAttributeCollectionClassTypeSymbol>(() => new XmlAttributeCollectionClassTypeSymbol("XmlAttributeCollection", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[10]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Count", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Get", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0),
			new BuiltInParameterSymbol(method, NavCorLib.XmlAttributeType, "Result", 1, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Get", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.XmlAttributeType, "Result", 1, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Get", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "LocalName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NamespaceUri", 1),
			new BuiltInParameterSymbol(method, NavCorLib.XmlAttributeType, "Result", 2, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RemoveAll", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Remove", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.XmlAttributeType, "Attribute", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Remove", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Remove", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "LocalName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NamespaceUri", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Set", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Set", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "LocalName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NamespaceUri", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 2)
		})
	}), isThreadSafe: true);

	internal static XmlAttributeCollectionClassTypeSymbol Instance => instance.Value;

	private XmlAttributeCollectionClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
