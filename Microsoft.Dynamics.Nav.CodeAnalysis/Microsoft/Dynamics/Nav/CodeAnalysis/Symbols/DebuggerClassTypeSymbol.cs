using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class DebuggerClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<DebuggerClassTypeSymbol> instance = new Lazy<DebuggerClassTypeSymbol>(() => new DebuggerClassTypeSymbol("Debugger", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[19]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Activate", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Deactivate", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsActive", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Attach", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "SessionID", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsAttached", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "DebuggedSessionID", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", "ALDebuggedSessionId", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsBreakpointHit", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Break", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "BreakOnError", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Ok", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "BreakOnRecordChanges", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Ok", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SkipSystemTriggers", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Ok", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Continue", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "StepInto", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "StepOut", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "StepOver", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Stop", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "DebuggingSessionID", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", "ALDebuggingSessionId", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetLastErrorText", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "EnableSqlTrace", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALDebugger", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "SessionID", 0),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "NewIsEnabled", 1, isVar: false, isOptional: true)
		})
	}), isThreadSafe: true);

	internal static DebuggerClassTypeSymbol Instance => instance.Value;

	private DebuggerClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Internal, getMethods)
	{
	}
}
