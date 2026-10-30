using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class DecimalPlacesPropertyValueValidator
{
	public static bool ValidatePropertyValue(string propertyName, string propertyValue, PropertyValueSyntax propertyValueSyntax, DiagnosticBag diagnostics)
	{
		DecimalPlacesPropertyValueSyntax decimalPlacesPropertyValueSyntax = (DecimalPlacesPropertyValueSyntax)propertyValueSyntax;
		int value = (int)decimalPlacesPropertyValueSyntax.Value.MinimumToken.Value;
		if (!PropertyValidatorHelpers.IsWithinRange(value, 0, 255, diagnostics, decimalPlacesPropertyValueSyntax.Value.MinimumToken))
		{
			return false;
		}
		int? num = ((decimalPlacesPropertyValueSyntax.Value.ColonToken.Kind != SyntaxKind.ColonToken) ? new int?(value) : ((int?)decimalPlacesPropertyValueSyntax.Value.MaximumToken.Value));
		if (num.HasValue && decimalPlacesPropertyValueSyntax.Value.ColonToken.Kind == SyntaxKind.ColonToken)
		{
			return PropertyValidatorHelpers.IsWithinRange(num.Value, 0, 255, diagnostics, decimalPlacesPropertyValueSyntax.Value.MaximumToken);
		}
		return true;
	}
}
