using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class TestPartClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<TestPartClassTypeSymbol> instance = new Lazy<TestPartClassTypeSymbol>(() => new TestPartClassTypeSymbol("TestPart", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[18]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "New", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "ValidationErrorCount", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetValidationError", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "GetField", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: true, NavCorLib.TestFieldType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Id", 0)
		}, addCompilationTarget: false, VersionCompatibility.Parse("3.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "Caption", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Next", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Previous", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Prev", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALPrevious", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: true, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, null, addCompilationTarget: false, VersionCompatibility.Parse("3.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "First", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Last", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsExpanded", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Expand", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Expand", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GoToRecord", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Rec", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GoToKey", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 0, isVar: false, isOptional: true, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "FindFirstField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TestFieldType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "FindNextField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TestFieldType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "FindPreviousField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TestFieldType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Editable", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType)
	}), isThreadSafe: true);

	internal static TestPartClassTypeSymbol Instance => instance.Value;

	private TestPartClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Extension, getMethods)
	{
	}
}
