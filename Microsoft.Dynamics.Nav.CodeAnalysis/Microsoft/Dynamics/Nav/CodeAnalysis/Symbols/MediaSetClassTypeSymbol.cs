using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class MediaSetClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<MediaSetClassTypeSymbol> instance = new Lazy<MediaSetClassTypeSymbol>(() => new MediaSetClassTypeSymbol("MediaSet", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[8]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Count", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "ExportFile", SymbolCompilationScope.Internal, isStatic: false, null, "ALExport", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FilenamePrefix", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ImportFile", SymbolCompilationScope.Internal, isStatic: false, null, "ALImport", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Filename", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Description", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "MimeType", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ImportStream", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALImport", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "Stream", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Description", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "MimeType", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Insert", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "MediaId", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Item", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "MediaId", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Remove", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "MediaId", 0)
		})
	}), isThreadSafe: true);

	internal static MediaSetClassTypeSymbol Instance => instance.Value;

	private MediaSetClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
