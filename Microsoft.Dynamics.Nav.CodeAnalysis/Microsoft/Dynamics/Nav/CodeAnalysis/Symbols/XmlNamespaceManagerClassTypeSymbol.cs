using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class XmlNamespaceManagerClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<XmlNamespaceManagerClassTypeSymbol> instance = new Lazy<XmlNamespaceManagerClassTypeSymbol>(() => new XmlNamespaceManagerClassTypeSymbol("XmlNamespaceManager", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[8]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "NameTable", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.XmlNameTableType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.XmlNameTableType, "NewValue", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "AddNamespace", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Prefix", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Uri", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "HasNamespace", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Prefix", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "LookupNamespace", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Prefix", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Result", 1, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "LookupPrefix", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Uri", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Result", 1, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RemoveNamespace", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Prefix", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Uri", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "PushScope", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "PopScope", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true)
	}), isThreadSafe: true);

	internal static XmlNamespaceManagerClassTypeSymbol Instance => instance.Value;

	private XmlNamespaceManagerClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
