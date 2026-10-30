using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class InStreamClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<InStreamClassTypeSymbol> instance = new Lazy<InStreamClassTypeSymbol>(() => new InStreamClassTypeSymbol("InStream", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[11]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Read", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Variable", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Read", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.ByteType, "Variable", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Read", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.CharType, "Variable", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Read", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Variable", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Read", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BigIntegerType, "Variable", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Read", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DecimalType, "Variable", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Read", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "Variable", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Read", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Variable", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Read", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Variable", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "EOS", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALIsEOS", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "ReadText", SymbolCompilationScope.DefinedByOwner, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Variable", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1, isVar: false, isOptional: true)
		})
	}), isThreadSafe: true);

	internal static InStreamClassTypeSymbol Instance => instance.Value;

	private InStreamClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
