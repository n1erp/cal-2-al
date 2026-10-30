using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class XmlCommentClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<XmlCommentClassTypeSymbol> instance = new Lazy<XmlCommentClassTypeSymbol>(() => new XmlCommentClassTypeSymbol("XmlComment", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[17]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Create", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavXmlComment", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlCommentType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Value", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NewValue", 0, isVar: false, isOptional: true)
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

	internal static XmlCommentClassTypeSymbol Instance => instance.Value;

	private XmlCommentClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
