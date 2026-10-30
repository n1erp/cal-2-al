using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class WidthPropertyValueValidator
{
	public static bool ValidatePropertyValue(string propertyName, string propertyValue, PropertyValueSyntax propertyValueSyntax, DiagnosticBag diagnostics)
	{
		return PropertyValidatorHelpers.IsPositive(propertyName, propertyValueSyntax, propertyValue, diagnostics);
	}
}
