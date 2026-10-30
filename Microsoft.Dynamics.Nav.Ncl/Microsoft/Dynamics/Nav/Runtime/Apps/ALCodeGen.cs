using System;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class ALCodeGen
{
	public static string GetPersonalizationALCodeFromSyntax(PageCustomizationSyntax pageCustomization, string objName)
	{
		if (pageCustomization == null)
		{
			throw new ArgumentNullException("pageCustomization");
		}
		if (objName == null)
		{
			throw new ArgumentNullException("objName");
		}
		return pageCustomization.WithName(SyntaxFactory.IdentifierName(objName)).NormalizeWhiteSpace().ToString();
	}
}
