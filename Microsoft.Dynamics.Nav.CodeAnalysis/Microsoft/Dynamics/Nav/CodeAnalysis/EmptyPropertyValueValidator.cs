using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class EmptyPropertyValueValidator
{
	public static bool ValidatePropertyValue(string propertyName, string propertyValue, PropertyValueSyntax propertyValueSyntax, DiagnosticBag diagnostics)
	{
		if (propertyValue.IsNullOrEmptyOrEmptyTextLiteral())
		{
			diagnostics.Add(ErrorCode.ERR_PropertyValueCannotBeEmpty, propertyValueSyntax.Location, propertyName);
			return false;
		}
		return true;
	}
}
