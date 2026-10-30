using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class TextClassTypeSymbol : LanguageClassTypeSymbol
{
	internal class StrSubstNoStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return generator.EmitBuiltInMethodCallWithJokerArgumentHandling(call);
		}

		public StrSubstNoStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	private static readonly Lazy<TextClassTypeSymbol> instance = new Lazy<TextClassTypeSymbol>(() => new TextClassTypeSymbol("Text", isAutoWith: true, (Symbol containingSymbol) => new MethodSymbol[36]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Contains", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALContains", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "EndsWith", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALEndsWith", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "IndexOf", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALIndexOf", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "StartIndex", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "IndexOfAny", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALIndexOfAny", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Values", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "StartIndex", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "IndexOfAny", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALIndexOfAny", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, () => NavCorLib.ConstructGenericType(ListClassTypeSymbolFactory.Instance, NavCorLib.CharType), "Values", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "StartIndex", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "LastIndexOf", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALLastIndexOf", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Value", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "StartIndex", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "PadLeft", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALPadLeft", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TextType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Count", 0),
			new BuiltInParameterSymbol(method, NavCorLib.CharType, "Char", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "PadRight", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALPadRight", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TextType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Count", 0),
			new BuiltInParameterSymbol(method, NavCorLib.CharType, "Char", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Remove", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALRemove", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TextType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "StartIndex", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Count", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Replace", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALReplace", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TextType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "OldValue", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "NewValue", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "StartsWith", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALStartsWith", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Split", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALSplit", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, () => NavCorLib.ConstructGenericType(ListClassTypeSymbolFactory.Instance, NavCorLib.TextType), returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Separators", 0, isVar: false, isOptional: true, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Split", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALSplit", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, () => NavCorLib.ConstructGenericType(ListClassTypeSymbolFactory.Instance, NavCorLib.TextType), returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, () => NavCorLib.ConstructGenericType(ListClassTypeSymbolFactory.Instance, NavCorLib.TextType), "Separators", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Split", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALSplit", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, () => NavCorLib.ConstructGenericType(ListClassTypeSymbolFactory.Instance, NavCorLib.TextType), returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, () => NavCorLib.ConstructGenericType(ListClassTypeSymbolFactory.Instance, NavCorLib.CharType), "Separators", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Substring", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALSubstring", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TextType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "StartIndex", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Count", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ToLower", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALToLower", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TextType),
		new BuiltInMethodTypeSymbol(containingSymbol, "ToUpper", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALToUpper", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TextType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Trim", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALTrim", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TextType),
		new BuiltInMethodTypeSymbol(containingSymbol, "TrimStart", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALTrimStart", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TextType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Chars", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TrimEnd", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavTextExtensions", "ALTrimEnd", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TextType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Chars", 0, isVar: false, isOptional: true)
		}),
		new StrSubstNoStaticBuiltInMethodTypeSymbol(containingSymbol, "StrSubstNo", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value1", 1, isVar: false, isOptional: true, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "StrPos", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "SubString", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "StrLen", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "IncStr", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CopyStr", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Position", 1),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "MaxStrLen", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "MaxStrLen", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: true, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.VariantType, "Variant", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "PadStr", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FillCharacter", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DelChr", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Where", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Which", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "StrCheckSum", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "WeightString", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Modulus", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ConvertStr", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FromCharacters", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "ToCharacters", 2)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "LowerCase", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", "ALLowercase", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "UpperCase", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", "ALUppercase", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SelectStr", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", "ALSelectString", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Number", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "CommaString", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DelStr", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Position", 1),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "InsStr", SymbolCompilationScope.DefinedByOwner, isStatic: true, "ALSystemString", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "SubString", 1),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Position", 2)
		})
	}), isThreadSafe: true);

	internal static TextClassTypeSymbol Instance => instance.Value;

	private TextClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
