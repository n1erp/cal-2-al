using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class ErrorInfoClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<ErrorInfoClassTypeSymbol> instance = new Lazy<ErrorInfoClassTypeSymbol>(() => new ErrorInfoClassTypeSymbol("ErrorInfo", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[4]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "ErrorType", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.ErrorTypeType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.ErrorTypeType, "ErrorType", 0, isVar: false, isOptional: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("3.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "Verbosity", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.VerbosityType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.VerbosityType, "Verbosity", 0, isVar: false, isOptional: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("3.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "DataClassification", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DataClassificationType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DataClassificationType, "DataClassification", 0, isVar: false, isOptional: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("3.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "Message", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Message", 0, isVar: false, isOptional: true)
		}, addCompilationTarget: false, VersionCompatibility.Parse("3.0"))
	}), isThreadSafe: true);

	internal static ErrorInfoClassTypeSymbol Instance => instance.Value;

	private ErrorInfoClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Internal, getMethods, VersionCompatibility.Parse("3.0"))
	{
	}
}
