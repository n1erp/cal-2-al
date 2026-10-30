using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class DictionaryClassTypeSymbolFactory : GenericLanguageClassTypeSymbolFactory
{
	internal static DictionaryClassTypeSymbolFactory Instance { get; } = new DictionaryClassTypeSymbolFactory();

	public override bool RequiresTemplateArgumentValidation { get; } = true;

	protected override IGenericTypeConstraintValidator TypeConstraintValidator { get; } = DictionaryTypeConstraintValidator.Instance;

	private DictionaryClassTypeSymbolFactory()
	{
	}

	protected override NavTypeSymbol ConstructNavType(TypeSymbol[] templateArguments)
	{
		return new GenericNavTypeSymbol(NavTypeKind.Dictionary, () => ConstructLanguageClass(templateArguments));
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	private static GenericLanguageClassTypeSymbol ConstructLanguageClass(TypeSymbol[] templateArguments)
	{
		return new DictionaryClassTypeSymbol("Dictionary", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[10]
		{
			new BuiltInMethodTypeSymbol(containingSymbol, "Count", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
			new BuiltInMethodTypeSymbol(containingSymbol, "Keys", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, () => NavCorLib.ConstructGenericType(ListClassTypeSymbolFactory.Instance, templateArguments[0])),
			new BuiltInMethodTypeSymbol(containingSymbol, "Values", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, () => NavCorLib.ConstructGenericType(ListClassTypeSymbolFactory.Instance, templateArguments[1])),
			new BuiltInMethodTypeSymbol(containingSymbol, "Get", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
			{
				new BuiltInParameterSymbol(method, templateArguments[0], "Key", 0),
				new BuiltInParameterSymbol(method, templateArguments[1], "Value", 1, isVar: true)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "Get", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, templateArguments[1], returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
			{
				new BuiltInParameterSymbol(method, templateArguments[0], "Key", 0)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "Set", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
			{
				new BuiltInParameterSymbol(method, templateArguments[0], "Key", 0),
				new BuiltInParameterSymbol(method, templateArguments[1], "Value", 1)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "Set", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
			{
				new BuiltInParameterSymbol(method, templateArguments[0], "Key", 0),
				new BuiltInParameterSymbol(method, templateArguments[1], "Value", 1),
				new BuiltInParameterSymbol(method, templateArguments[1], "OldValue", 2, isVar: true)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "Add", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
			{
				new BuiltInParameterSymbol(method, templateArguments[0], "Key", 0),
				new BuiltInParameterSymbol(method, templateArguments[1], "Value", 1)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "ContainsKey", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
			{
				new BuiltInParameterSymbol(method, templateArguments[0], "Key", 0)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "Remove", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
			{
				new BuiltInParameterSymbol(method, templateArguments[0], "Key", 0)
			})
		}, templateArguments);
	}
}
