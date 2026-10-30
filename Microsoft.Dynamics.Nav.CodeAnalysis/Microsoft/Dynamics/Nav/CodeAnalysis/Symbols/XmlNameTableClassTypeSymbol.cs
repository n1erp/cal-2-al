using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class XmlNameTableClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<XmlNameTableClassTypeSymbol> instance = new Lazy<XmlNameTableClassTypeSymbol>(() => new XmlNameTableClassTypeSymbol("XmlNameTable", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[2]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Add", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Key", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Get", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Key", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Result", 1, isVar: true)
		})
	}), isThreadSafe: true);

	internal static XmlNameTableClassTypeSymbol Instance => instance.Value;

	private XmlNameTableClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
