using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class SessionClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<SessionClassTypeSymbol> instance = new Lazy<SessionClassTypeSymbol>(() => new SessionClassTypeSymbol("Session", isAutoWith: true, (Symbol containingSymbol) => new MethodSymbol[14]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "ApplicationArea", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSession", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "ApplicationArea", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "StartSession", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSession", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[4]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "SessionId", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "CodeunitId", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Company", 2, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 3, isVar: true, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsSessionActive", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSession", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "SessionID", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CurrentExecutionMode", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSession", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.ExecutionModeType),
		new BuiltInMethodTypeSymbol(containingSymbol, "StopSession", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSession", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "SessionId", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Comment", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CurrentClientType", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSession", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.ClientTypeType),
		new BuiltInMethodTypeSymbol(containingSymbol, "DefaultClientType", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSession", null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.ClientTypeType),
		new BuiltInMethodTypeSymbol(containingSymbol, "BindSubscription", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSession", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.CodeunitType, "Codeunit", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "UnbindSubscription", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSession", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.CodeunitType, "Codeunit", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ApplicationIdentifier", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSession", "ALApplicationIdentifier", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "SendTraceTag", SymbolCompilationScope.Internal, isStatic: true, "ALSession", "ALSendTraceTag", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[5]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Tag", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Category", 1),
			new BuiltInParameterSymbol(method, NavCorLib.VerbosityType, "Verbosity", 2),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Message", 3),
			new BuiltInParameterSymbol(method, NavCorLib.DataClassificationType, "DataClassification", 4, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetExecutionContext", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSession", "ALExecutionContext", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.ExecutionContextType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetModuleExecutionContext", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSession", "ALGetModuleExecutionContext", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.ExecutionContextType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "AppId", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetCurrentModuleExecutionContext", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSession", "ALGetModuleExecutionContext", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.ExecutionContextType)
	}), isThreadSafe: true);

	internal static SessionClassTypeSymbol Instance => instance.Value;

	private SessionClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
