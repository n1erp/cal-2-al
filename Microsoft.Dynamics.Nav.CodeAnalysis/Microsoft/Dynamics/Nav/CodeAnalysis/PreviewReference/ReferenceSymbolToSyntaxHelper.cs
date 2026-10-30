using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.PreviewReference;

internal static class ReferenceSymbolToSyntaxHelper
{
	internal static PropertySyntax[] ConvertProperties(IEnumerable<PropertySymbol> properties, Symbol parentSymbol)
	{
		DebugAssertHelper.Assert(properties != null);
		DebugAssertHelper.Assert(parentSymbol != null);
		if (parentSymbol.Kind == SymbolKind.Change && parentSymbol is ReferenceChangeModifyControlSymbol)
		{
			return ConvertControlModifySymbolPropertiesToSyntaxList(properties);
		}
		return new PropertySyntax[0];
	}

	private static PropertySyntax[] ConvertControlModifySymbolPropertiesToSyntaxList(IEnumerable<PropertySymbol> properties)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (PropertySymbol property in properties)
		{
			stringBuilder.Append(property.Name);
			stringBuilder.Append(" = ");
			stringBuilder.Append(property.ValueText);
			stringBuilder.AppendLine(";");
		}
		SyntaxList<PropertySyntaxOrEmpty> propertyList = ((PageCustomizationSyntax)((ObjectCompilationUnitSyntax)SyntaxTree.ParseObjectText(string.Format(CultureInfo.InvariantCulture, "pagecustomization MyCustomization \r\n{{\r\n    layout\r\n    {{\r\n        modify(MyFakeControl)\r\n        {{\r\n            {0}\r\n        }}\r\n    }}\r\n}}", stringBuilder.ToString())).GetRoot()).Objects[0]).Layout.Changes[0].GetPropertyList();
		ArrayBuilder<PropertySyntax> instance = ArrayBuilder<PropertySyntax>.GetInstance();
		try
		{
			foreach (PropertySyntaxOrEmpty item in propertyList)
			{
				if (item.Kind == SyntaxKind.Property)
				{
					instance.Add((PropertySyntax)item);
				}
			}
			return instance.ToArray();
		}
		finally
		{
			instance.Free();
		}
	}
}
