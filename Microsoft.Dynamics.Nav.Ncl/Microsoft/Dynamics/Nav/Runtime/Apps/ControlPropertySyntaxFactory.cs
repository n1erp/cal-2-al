using System;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class ControlPropertySyntaxFactory : PropertySyntaxAbstractFactory
{
	internal override PropertySyntax GetPropertySyntax(PropertyKind propertyKind, string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		PropertyValueSyntax propertyValue;
		switch (propertyKind)
		{
		case PropertyKind.OptionCaptionML:
		case PropertyKind.OptionCaption:
			return null;
		case PropertyKind.Importance:
			propertyValue = SyntaxFactory.EnumPropertyValue(SyntaxFactory.IdentifierName(value));
			break;
		case PropertyKind.Visible:
		case PropertyKind.QuickEntry:
			propertyValue = PropertySyntaxAbstractFactory.CreateBooleanPropertyValue(value);
			break;
		default:
			throw new InvalidOperationException("The type is not supported right now");
		}
		return SyntaxFactory.Property(propertyKind, propertyValue);
	}
}
