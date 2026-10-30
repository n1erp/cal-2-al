using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class TestFilterClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<TestFilterClassTypeSymbol> instance = new Lazy<TestFilterClassTypeSymbol>(() => new TestFilterClassTypeSymbol("TestFilter", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[5]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "SetFilter", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TestFilterFieldType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetFilter", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TestFilterFieldType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetCurrentKey", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TestFilterFieldType, "Field1", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.TestFilterFieldType, "Field2", 1, isVar: false, isOptional: true, isParams: true, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Ascending", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Value", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CurrentKey", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType)
	}), isThreadSafe: true);

	internal static TestFilterClassTypeSymbol Instance => instance.Value;

	private TestFilterClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Extension, getMethods)
	{
	}
}
