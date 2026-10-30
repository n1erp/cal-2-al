namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public sealed class SymbolDisplayFormat
{
	public SymbolDisplayParameterOptions ParameterOptions { get; }

	public SymbolDisplayMethodOptions MethodOptions { get; }

	public SymbolDisplayTypedSymbolOptions TypedSymbolOptions { get; }

	public SymbolDisplayVariableOptions VariableOptions { get; }

	public SymbolDisplayReturnValueOptions ReturnValueOptions { get; }

	public static SymbolDisplayFormat ErrorMessageFormat { get; } = new SymbolDisplayFormat(SymbolDisplayMethodOptions.IncludeParameters, SymbolDisplayParameterOptions.IncludeIsVar | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeOptionalBrackets, SymbolDisplayTypedSymbolOptions.IncludeName, SymbolDisplayVariableOptions.IncludeName, SymbolDisplayReturnValueOptions.IncludeName);

	public static SymbolDisplayFormat SignatureFormat { get; } = new SymbolDisplayFormat(SymbolDisplayMethodOptions.IncludeType | SymbolDisplayMethodOptions.IncludeAccessibility | SymbolDisplayMethodOptions.IncludeParameters | SymbolDisplayMethodOptions.IncludeKeyword, SymbolDisplayParameterOptions.IncludeIsVar | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeOptionalBrackets | SymbolDisplayParameterOptions.IncludeParams, SymbolDisplayTypedSymbolOptions.IncludeType | SymbolDisplayTypedSymbolOptions.IncludeName, SymbolDisplayVariableOptions.IncludeType | SymbolDisplayVariableOptions.IncludeName, SymbolDisplayReturnValueOptions.IncludeType | SymbolDisplayReturnValueOptions.IncludeName);

	public static SymbolDisplayFormat OutLineFormat { get; } = new SymbolDisplayFormat(SymbolDisplayMethodOptions.IncludeParameters, SymbolDisplayParameterOptions.IncludeIsVar | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeOptionalBrackets | SymbolDisplayParameterOptions.IncludeParams, SymbolDisplayTypedSymbolOptions.IncludeType | SymbolDisplayTypedSymbolOptions.IncludeName, SymbolDisplayVariableOptions.IncludeType | SymbolDisplayVariableOptions.IncludeName, SymbolDisplayReturnValueOptions.IncludeType | SymbolDisplayReturnValueOptions.IncludeName);

	public static SymbolDisplayFormat HoverFormat { get; } = new SymbolDisplayFormat(SymbolDisplayMethodOptions.IncludeType | SymbolDisplayMethodOptions.IncludeAccessibility | SymbolDisplayMethodOptions.IncludeParameters | SymbolDisplayMethodOptions.IncludeKeyword | SymbolDisplayMethodOptions.IncludeModifiers, SymbolDisplayParameterOptions.IncludeIsVar | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeOptionalBrackets | SymbolDisplayParameterOptions.IncludeParams | SymbolDisplayParameterOptions.IncludeSymbolKind, SymbolDisplayTypedSymbolOptions.IncludeType | SymbolDisplayTypedSymbolOptions.IncludeName | SymbolDisplayTypedSymbolOptions.IncludeSymbolKind, SymbolDisplayVariableOptions.IncludeType | SymbolDisplayVariableOptions.IncludeName | SymbolDisplayVariableOptions.IncludeSymbolKind, SymbolDisplayReturnValueOptions.IncludeType | SymbolDisplayReturnValueOptions.IncludeName | SymbolDisplayReturnValueOptions.IncludeSymbolKind);

	public static SymbolDisplayFormat ShortFormat { get; } = new SymbolDisplayFormat(SymbolDisplayMethodOptions.IncludeParameters | SymbolDisplayMethodOptions.IncludeKeyword, SymbolDisplayParameterOptions.IncludeIsVar | SymbolDisplayParameterOptions.IncludeType, SymbolDisplayTypedSymbolOptions.IncludeType, SymbolDisplayVariableOptions.IncludeType, SymbolDisplayReturnValueOptions.IncludeType);

	public static SymbolDisplayFormat NameOnlyFormat { get; } = new SymbolDisplayFormat(SymbolDisplayMethodOptions.None, SymbolDisplayParameterOptions.IncludeName, SymbolDisplayTypedSymbolOptions.IncludeName, SymbolDisplayVariableOptions.IncludeName, SymbolDisplayReturnValueOptions.IncludeName);

	public static SymbolDisplayFormat MethodCompletionFormat { get; } = new SymbolDisplayFormat(SymbolDisplayMethodOptions.IncludeType | SymbolDisplayMethodOptions.IncludeParameters, SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.UseDeclarationSeparator, SymbolDisplayTypedSymbolOptions.None, SymbolDisplayVariableOptions.None, SymbolDisplayReturnValueOptions.IncludeType | SymbolDisplayReturnValueOptions.IncludeName);

	public static SymbolDisplayFormat TestFormat { get; } = new SymbolDisplayFormat(SymbolDisplayMethodOptions.IncludeType | SymbolDisplayMethodOptions.IncludeAccessibility | SymbolDisplayMethodOptions.IncludeParameters | SymbolDisplayMethodOptions.IncludeKeyword, SymbolDisplayParameterOptions.IncludeIsVar | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeOptionalBrackets | SymbolDisplayParameterOptions.IncludeParams, SymbolDisplayTypedSymbolOptions.IncludeType | SymbolDisplayTypedSymbolOptions.IncludeName, SymbolDisplayVariableOptions.IncludeType | SymbolDisplayVariableOptions.IncludeName | SymbolDisplayVariableOptions.IncludeSymbolKind, SymbolDisplayReturnValueOptions.IncludeType | SymbolDisplayReturnValueOptions.IncludeName | SymbolDisplayReturnValueOptions.IncludeSymbolKind);

	public SymbolDisplayFormat(SymbolDisplayMethodOptions methodOptions = SymbolDisplayMethodOptions.None, SymbolDisplayParameterOptions parameterOptions = SymbolDisplayParameterOptions.None, SymbolDisplayTypedSymbolOptions fieldOptions = SymbolDisplayTypedSymbolOptions.None, SymbolDisplayVariableOptions variableOptions = SymbolDisplayVariableOptions.None, SymbolDisplayReturnValueOptions returnValueOptions = SymbolDisplayReturnValueOptions.None)
	{
		MethodOptions = methodOptions;
		ParameterOptions = parameterOptions;
		TypedSymbolOptions = fieldOptions;
		VariableOptions = variableOptions;
		ReturnValueOptions = returnValueOptions;
	}
}
