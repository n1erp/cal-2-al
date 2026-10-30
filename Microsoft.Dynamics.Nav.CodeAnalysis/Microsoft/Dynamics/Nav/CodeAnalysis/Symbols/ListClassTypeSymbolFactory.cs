using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class ListClassTypeSymbolFactory : GenericLanguageClassTypeSymbolFactory
{
	internal static ListClassTypeSymbolFactory Instance { get; } = new ListClassTypeSymbolFactory();

	public override bool RequiresTemplateArgumentValidation { get; } = true;

	protected override IGenericTypeConstraintValidator TypeConstraintValidator { get; } = ListTypeConstraintValidator.Instance;

	private ListClassTypeSymbolFactory()
	{
	}

	protected override NavTypeSymbol ConstructNavType(TypeSymbol[] templateArguments)
	{
		return new GenericNavTypeSymbol(NavTypeKind.List, () => ConstructLanguageClass(templateArguments));
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	private static GenericLanguageClassTypeSymbol ConstructLanguageClass(TypeSymbol[] templateArguments)
	{
		return new ListClassTypeSymbol("List", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[18]
		{
			new BuiltInMethodTypeSymbol(containingSymbol, "Count", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
			new BuiltInMethodTypeSymbol(containingSymbol, "Add", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
			{
				new BuiltInParameterSymbol(method, templateArguments[0], "Value", 0)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "AddRange", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
			{
				new BuiltInParameterSymbol(method, templateArguments[0], "Value", 0),
				new BuiltInParameterSymbol(method, templateArguments[0], "Values", 1, isVar: false, isOptional: true, isParams: true)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "AddRange", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
			{
				new BuiltInParameterSymbol(method, () => NavCorLib.ConstructGenericType(Instance, templateArguments[0]), "Values", 0)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "Get", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
			{
				new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0),
				new BuiltInParameterSymbol(method, templateArguments[0], "Result", 1, isVar: true)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "Get", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, templateArguments[0], returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
			{
				new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "Set", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
			{
				new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0),
				new BuiltInParameterSymbol(method, templateArguments[0], "NewValue", 1)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "Set", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
			{
				new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0),
				new BuiltInParameterSymbol(method, templateArguments[0], "NewValue", 1),
				new BuiltInParameterSymbol(method, templateArguments[0], "OldValue", 2, isVar: true)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "Contains", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
			{
				new BuiltInParameterSymbol(method, templateArguments[0], "Value", 0)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "IndexOf", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
			{
				new BuiltInParameterSymbol(method, templateArguments[0], "Value", 0)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "Insert", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
			{
				new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0),
				new BuiltInParameterSymbol(method, templateArguments[0], "Value", 1)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "LastIndexOf", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
			{
				new BuiltInParameterSymbol(method, templateArguments[0], "Value", 0)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "Remove", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
			{
				new BuiltInParameterSymbol(method, templateArguments[0], "Value", 0)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "RemoveAt", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
			{
				new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "RemoveRange", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
			{
				new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0),
				new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Count", 1)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "GetRange", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, () => NavCorLib.ConstructGenericType(Instance, templateArguments[0]), returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
			{
				new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0),
				new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Count", 1)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "GetRange", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
			{
				new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0),
				new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Count", 1),
				new BuiltInParameterSymbol(method, () => NavCorLib.ConstructGenericType(Instance, templateArguments[0]), "Result", 2, isVar: true)
			}),
			new BuiltInMethodTypeSymbol(containingSymbol, "Reverse", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true)
		}, templateArguments);
	}
}
