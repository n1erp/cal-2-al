using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SecurityFilteringAttributeValidator : IAttributeValidator
{
	public static IAttributeValidator Instance { get; } = new SecurityFilteringAttributeValidator();

	public void Validate(AttributeSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.HasErrors())
		{
			TypeSymbol attributeTargetVariableType = symbol.GetAttributeTargetVariableType();
			switch (attributeTargetVariableType.NavTypeKind)
			{
			case NavTypeKind.Query:
				ValidateQueryAttribute(symbol, attributeTargetVariableType, diagnostics);
				break;
			default:
				AddInvalidSecurityFilteringValueDiagnostic(diagnostics, symbol, attributeTargetVariableType);
				break;
			case NavTypeKind.RecordRef:
			case NavTypeKind.Record:
				break;
			}
		}
	}

	private static void ValidateQueryAttribute(AttributeSymbol symbol, TypeSymbol query, DiagnosticBag diagnostics)
	{
		if (symbol.Arguments[0].GetValueAsEnum<SystemOptionKinds.SecurityFilterKind>() == SystemOptionKinds.SecurityFilterKind.Validated)
		{
			AddInvalidSecurityFilteringValueDiagnostic(diagnostics, symbol, query);
		}
	}

	private static void AddInvalidSecurityFilteringValueDiagnostic(DiagnosticBag diagnostics, AttributeSymbol symbol, TypeSymbol variableSymbolType)
	{
		string valueText = symbol.Arguments[0].ValueText;
		diagnostics.Add(ErrorCode.ERR_InvalidAttributeValueForVariableType, symbol.GetLocation(), valueText, symbol.AttributeInfo.Kind.ToString(), variableSymbolType.NavTypeKind.ToString());
	}
}
