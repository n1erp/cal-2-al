using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.XmlPort;

internal class XmlPortElementHeaderExporter : IElementExporter
{
	public void Export(IElement element, IAlWriter writer)
	{
		writer.Write(AlUtilities.GetSectionTypeName(element).ToLowerInvariant());
		writer.Write('(');
		WriteHeaderContent(element, writer);
		writer.WriteLine(')');
		writer.PendingEmptyLine = false;
	}

	private void WriteHeaderContent(IElement element, IAlWriter writer)
	{
		string text = element.GetStringProperty(PropertyType.Name).QuoteIdentifierIfNeeded();
		string text2 = element.GetStringProperty(PropertyType.SourceType).ToLowerInvariant();
		string text3 = element.GetStringProperty(PropertyType.VariableName)?.ToLowerInvariant();
		string text4 = "";
		switch (text2)
		{
		case "table":
		{
			text4 = element.GetStringProperty(PropertyType.SourceTable);
			string value = text3 ?? text4;
			writer.Write(value.QuoteIdentifierIfNeeded() + ";" + text4.QuoteIdentifierIfNeeded());
			break;
		}
		case "field":
		{
			string stringProperty = element.GetStringProperty(PropertyType.SourceField);
			int? num = stringProperty?.IndexOf("::", StringComparison.InvariantCulture);
			if (num > 0)
			{
				string value2 = stringProperty.Substring(0, num.Value);
				string value3 = stringProperty.Substring(num.Value + 2);
				text4 = value2.QuoteIdentifierIfNeeded() + "." + value3.QuoteIdentifierIfNeeded();
			}
			writer.Write(text.QuoteIdentifierIfNeeded() + ";" + text4);
			break;
		}
		case "text":
			text3 = text3 ?? text;
			writer.Write(text3.QuoteIdentifierIfNeeded());
			break;
		}
	}
}
