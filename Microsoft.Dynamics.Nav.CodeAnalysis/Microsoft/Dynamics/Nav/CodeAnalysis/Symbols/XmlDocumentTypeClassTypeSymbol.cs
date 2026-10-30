using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class XmlDocumentTypeClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<XmlDocumentTypeClassTypeSymbol> instance = new Lazy<XmlDocumentTypeClassTypeSymbol>(() => new XmlDocumentTypeClassTypeSymbol("XmlDocumentType", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[27]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Create", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavXmlDocumentType", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlDocumentTypeType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Create", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavXmlDocumentType", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlDocumentTypeType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "PublicId", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Create", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavXmlDocumentType", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlDocumentTypeType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "PublicId", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "SystemId", 2)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Create", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavXmlDocumentType", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlDocumentTypeType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[4]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "PublicId", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "SystemId", 2),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "InternalSubSet", 3)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetName", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Result", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetName", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetSystemId", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Result", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetSystemId", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetInternalSubset", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Result", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetInternalSubset", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetPublicId", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Result", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetPublicId", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 0)
		}),
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
		})
	}), isThreadSafe: true);

	internal static XmlDocumentTypeClassTypeSymbol Instance => instance.Value;

	private XmlDocumentTypeClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
