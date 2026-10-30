using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class MediaClassTypeSymbol : LanguageClassTypeSymbol
{
	private static readonly Lazy<MediaClassTypeSymbol> instance = new Lazy<MediaClassTypeSymbol>(() => new MediaClassTypeSymbol("Media", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[7]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "HasValue", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "ExportFile", SymbolCompilationScope.Internal, isStatic: false, null, "ALExport", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Filename", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ExportStream", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALExport", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "Stream", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ImportFile", SymbolCompilationScope.Internal, isStatic: false, null, "ALImport", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Filename", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Description", 1),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "MimeType", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ImportStream", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALImport", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "Stream", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Description", 1),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "MimeType", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ImportStream", SymbolCompilationScope.Internal, isStatic: false, null, "ALImport", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[4]
		{
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "Stream", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Description", 1),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "MimeType", 2),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "FileName", 3)
		}, addCompilationTarget: false, VersionCompatibility.Parse("3.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "MediaId", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.GuidType)
	}), isThreadSafe: true);

	internal static MediaClassTypeSymbol Instance => instance.Value;

	private MediaClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
