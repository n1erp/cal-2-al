using System;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class StackGroupPropertySyntaxFactory : PropertySyntaxAbstractFactory
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
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
			return null;
		case PropertyKind.Description:
			propertyValue = SyntaxFactory.StringPropertyValue(SyntaxFactory.StringLiteralValue(SyntaxFactory.Literal(value)));
			break;
		case PropertyKind.Visible:
			propertyValue = PropertySyntaxAbstractFactory.CreateBooleanPropertyValue(value);
			break;
		default:
			throw new InvalidOperationException("The type is not supported right now");
		}
		return SyntaxFactory.Property(propertyKind, propertyValue);
	}
}
