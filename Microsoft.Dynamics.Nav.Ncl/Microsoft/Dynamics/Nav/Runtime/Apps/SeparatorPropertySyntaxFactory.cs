using System;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SeparatorPropertySyntaxFactory : PropertySyntaxAbstractFactory
{
	internal override PropertySyntax GetPropertySyntax(PropertyKind propertyKind, string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (propertyKind == PropertyKind.Description || propertyKind == PropertyKind.Visible)
		{
			return null;
		}
		throw new InvalidOperationException("The type is not supported right now");
	}
}
