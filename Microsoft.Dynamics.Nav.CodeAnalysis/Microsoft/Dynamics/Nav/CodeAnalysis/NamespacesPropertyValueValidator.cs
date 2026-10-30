using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class NamespacesPropertyValueValidator
{
	public static bool ValidatePropertyValue(string propertyName, string propertyValue, PropertyValueSyntax propertyValueSyntax, DiagnosticBag diagnostics)
	{
		if (propertyValueSyntax is CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax commaSeparatedIdentifierEqualsStringListPropertyValueSyntax)
		{
			Dictionary<string, SyntaxNode> dictionary = new Dictionary<string, SyntaxNode>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			bool flag = false;
			foreach (IdentifierEqualsStringSyntax value in commaSeparatedIdentifierEqualsStringListPropertyValueSyntax.List.Values)
			{
				string text = value.Identifier.ValueText.UnquoteIdentifier();
				if (dictionary.ContainsKey(text))
				{
					flag = true;
					diagnostics.Add(ErrorCode.ERR_DuplicateNamespacePrefix, value.GetLocation(), text);
					if (!hashSet.Contains(text))
					{
						hashSet.Add(text);
						diagnostics.Add(ErrorCode.ERR_DuplicateNamespacePrefix, dictionary[text].GetLocation(), text);
					}
				}
				else
				{
					dictionary.Add(text, value);
				}
			}
			return !flag;
		}
		return true;
	}
}
