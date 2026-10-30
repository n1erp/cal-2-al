using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class CharAllowedPropertyValueValidator
{
	public static bool ValidatePropertyValue(string propertyName, string propertyValue, PropertyValueSyntax propertyValueSyntax, DiagnosticBag diagnostics)
	{
		if (propertyValue.Length % 2 != 0)
		{
			diagnostics.Add(ErrorCode.ERR_CharAllowedFormatIsWrong, propertyValueSyntax.Location);
			return false;
		}
		for (int i = 1; i < propertyValue.Length - 1; i += 2)
		{
			if (propertyValue[i] > propertyValue[i + 1])
			{
				diagnostics.Add(ErrorCode.ERR_CharAllowedFormatIsWrong, propertyValueSyntax.Location);
				return false;
			}
		}
		return true;
	}
}
