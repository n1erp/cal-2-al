using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal abstract class PropertySyntaxAbstractFactory
{
	internal abstract PropertySyntax GetPropertySyntax(PropertyKind propertyKind, string value);

	protected static PropertyValueSyntax CreateBooleanPropertyValue(string value)
	{
		value = ConverterHelper.ConvertBooleanValue(value);
		if (value == null)
		{
			return null;
		}
		if (!Convert.ToBoolean(value, CultureInfo.InvariantCulture))
		{
			return SyntaxFactory.BooleanPropertyValue(SyntaxFactory.BooleanLiteralValue(SyntaxFactory.Token(SyntaxKind.FalseKeyword)));
		}
		return SyntaxFactory.BooleanPropertyValue(SyntaxFactory.BooleanLiteralValue(SyntaxFactory.Token(SyntaxKind.TrueKeyword)));
	}
}
