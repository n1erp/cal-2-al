using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class XmlWriteOptionsClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<XmlWriteOptionsClassTypeSymbol> instance = new Lazy<XmlWriteOptionsClassTypeSymbol>(() => new XmlWriteOptionsClassTypeSymbol("XmlWriteOptions", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[1]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "PreserveWhitespace", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "NewValue", 0, isVar: false, isOptional: true)
		})
	}), isThreadSafe: true);

	internal static XmlWriteOptionsClassTypeSymbol Instance => instance.Value;

	private XmlWriteOptionsClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
