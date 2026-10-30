using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class TableClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<TableClassTypeSymbol> instance = new Lazy<TableClassTypeSymbol>(() => new TableClassTypeSymbol("Table", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[83]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "FindFirst", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "FindLast", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "FindSet", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "ForUpdate", 0, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "UpdateKey", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Get", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 0, isVar: false, isOptional: true, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Find", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Which", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Next", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Steps", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Reset", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetCurrentKey", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field1", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field2", 1, isVar: false, isOptional: true, isParams: true, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Ascending", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Ascending", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetAscending", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Ascending", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetAscending", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "LockTable", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Wait", 0, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "VersionCheck", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CalcFields", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field1", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field2", 1, isVar: false, isOptional: true, isParams: true, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CalcSums", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field1", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field2", 1, isVar: false, isOptional: true, isParams: true, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetAutoCalcFields", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field1", 0, isVar: false, isOptional: true, isParams: true, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Count", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsEmpty", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "CountApprox", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "TableName", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "TableCaption", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "ChangeCompany", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "CompanyName", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CurrentKey", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Consistent", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Consistent", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetPosition", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "UseNames", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetPosition", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Init", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Insert", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "RunTrigger", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Modify", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "RunTrigger", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Delete", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "RunTrigger", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Rename", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value1", 0),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value2", 1, isVar: false, isOptional: true, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ModifyAll", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALModifyAllSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "NewValue", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "RunTrigger", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DeleteAll", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "RunTrigger", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ReadPermission", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "WritePermission", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "ReadConsistency", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "RecordLevelLocking", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Copy", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "FromRecord", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "ShareTable", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "AddLink", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "URL", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Description", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DeleteLink", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "ID", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DeleteLinks", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "CopyLinks", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "FromRecord", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "HasLinks", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetRange", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALSetRangeSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "FromValue", 1, isVar: false, isOptional: true, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "ToValue", 2, isVar: false, isOptional: true, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetFilter", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 1),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 2, isVar: false, isOptional: true, isParams: true, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetFilter", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetFilters", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetView", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "UseNames", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetView", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetRangeMin", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALGetRangeMinSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.JokerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: true, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetRangeMax", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALGetRangeMaxSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.JokerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: true, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CopyFilter", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "FromField", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Record.ToField", 1, isVar: false, isOptional: false, isParams: false, isMember: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CopyFilters", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "FromRecord", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "HasFilter", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetRecFilter", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "FilterGroup", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Group", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetPermissionFilter", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Mark", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Mark", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ClearMarks", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "MarkedOnly", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "MarkedOnly", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Validate", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALValidateSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "NewValue", 1, isVar: false, isOptional: true, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TestField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTestFieldSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TestField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTestFieldSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Value", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TestField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTestFieldSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Value", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TestField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTestFieldSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.BigIntegerType, "Value", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TestField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTestFieldSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.DecimalType, "Value", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TestField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTestFieldSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "Value", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TestField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTestFieldSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Value", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TestField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTestFieldSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.LabelType, "Value", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("3.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "TestField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTestFieldSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.TextConstType, "Value", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("3.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "TestField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTestFieldSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.CodeType, "Value", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TestField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTestFieldSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TestField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALTestFieldNavValueSafe", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true, addExpectedType: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "FieldError", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Text", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TransferFields", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "FromRecord", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "InitPrimaryKeyFields", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "FieldName", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "FieldCaption", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "FieldActive", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "FieldNo", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Relation", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SecurityFiltering", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.SecurityFilterType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.SecurityFilterType, "SecurityFiltering", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RecordId", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.RecordIdType),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsTemporary", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "CurrentCompany", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType)
	}), isThreadSafe: true);

	internal static TableClassTypeSymbol Instance => instance.Value;

	private TableClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
