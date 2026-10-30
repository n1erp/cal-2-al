using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class XmlElementClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<XmlElementClassTypeSymbol> instance = new Lazy<XmlElementClassTypeSymbol>(() => new XmlElementClassTypeSymbol("XmlElement", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[48]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Create", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavXmlElement", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlElementType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Create", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavXmlElement", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlElementType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "LocalName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NamespaceUri", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Create", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavXmlElement", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlElementType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "LocalName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NamespaceUri", 1),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Content", 2, isVar: false, isOptional: false, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Create", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavXmlElement", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlElementType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Content", 1, isVar: false, isOptional: false, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "HasAttributes", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "HasElements", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsEmpty", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Name", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "LocalName", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "NamespaceUri", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "InnerXml", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "InnerText", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetNamespaceOfPrefix", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Prefix", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Result", 1, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetPrefixOfNamespace", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Namespace", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Result", 1, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RemoveAllAttributes", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "RemoveAttribute", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RemoveAttribute", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "LocalName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NamespaceUri", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RemoveAttribute", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.XmlAttributeType, "Attribute", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetAttribute", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetAttribute", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "LocalName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NamespaceUri", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 2)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Attributes", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlAttributeCollectionType),
		new BuiltInMethodTypeSymbol(containingSymbol, "AsXmlNode", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlNodeType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetParent", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.XmlElementType, "Parent", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetDocument", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.XmlDocumentType, "Document", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "AddAfterSelf", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Content", 0, isVar: false, isOptional: false, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "AddBeforeSelf", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Content", 0, isVar: false, isOptional: false, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ReplaceWith", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Node", 0, isVar: false, isOptional: false, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Remove", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "WriteTo", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "OutStream", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "WriteTo", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.XmlWriteOptionsType, "WriteOptions", 0),
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "OutStream", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "WriteTo", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Text", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "WriteTo", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.XmlWriteOptionsType, "WriteOptions", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Text", 1, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SelectSingleNode", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "XPath", 0),
			new BuiltInParameterSymbol(method, NavCorLib.XmlNodeType, "Node", 1, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SelectSingleNode", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "XPath", 0),
			new BuiltInParameterSymbol(method, NavCorLib.XmlNamespaceManagerType, "NamespaceManager", 1),
			new BuiltInParameterSymbol(method, NavCorLib.XmlNodeType, "Node", 2, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SelectNodes", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "XPath", 0),
			new BuiltInParameterSymbol(method, NavCorLib.XmlNodeListType, "NodeList", 1, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SelectNodes", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "XPath", 0),
			new BuiltInParameterSymbol(method, NavCorLib.XmlNamespaceManagerType, "NamespaceManager", 1),
			new BuiltInParameterSymbol(method, NavCorLib.XmlNodeListType, "NodeList", 2, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Add", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Content", 0, isVar: false, isOptional: false, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "AddFirst", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Content", 0, isVar: false, isOptional: false, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ReplaceNodes", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Content", 0, isVar: false, isOptional: false, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RemoveNodes", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetChildNodes", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlNodeListType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetChildElements", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlNodeListType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetChildElements", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlNodeListType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetChildElements", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlNodeListType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "LocalName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NamespaceUri", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetDescendantNodes", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlNodeListType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetDescendantElements", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlNodeListType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetDescendantElements", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlNodeListType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetDescendantElements", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlNodeListType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "LocalName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NamespaceUri", 1)
		})
	}), isThreadSafe: true);

	internal static XmlElementClassTypeSymbol Instance => instance.Value;

	private XmlElementClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
