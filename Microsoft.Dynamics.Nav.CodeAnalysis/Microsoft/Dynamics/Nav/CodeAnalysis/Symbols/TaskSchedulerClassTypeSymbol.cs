using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class TaskSchedulerClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<TaskSchedulerClassTypeSymbol> instance = new Lazy<TaskSchedulerClassTypeSymbol>(() => new TaskSchedulerClassTypeSymbol("TaskScheduler", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[5]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "CreateTask", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALTaskScheduler", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[6]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "CodeunitId", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "FailureCodeunitId", 1),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "IsReady", 2, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Company", 3, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.DateTimeType, "NotBefore", 4, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.RecordIdType, "RecordID", 5, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TaskExists", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALTaskScheduler", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "Task", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CancelTask", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALTaskScheduler", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "Task", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetTaskReady", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALTaskScheduler", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "Task", 0),
			new BuiltInParameterSymbol(method, NavCorLib.DateTimeType, "NotBefore", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CanCreateTask", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALTaskScheduler", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType)
	}), isThreadSafe: true);

	internal static TaskSchedulerClassTypeSymbol Instance => instance.Value;

	private TaskSchedulerClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
