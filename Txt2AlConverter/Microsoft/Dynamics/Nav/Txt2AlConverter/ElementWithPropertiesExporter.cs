using System;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class ElementWithPropertiesExporter : IElementExporter
{
	private readonly IElementExporter nextExporter;

	private readonly IPropertiesConverter propertyExporter;

	public ElementWithPropertiesExporter(IPropertiesConverter propertyExporter, IElementExporter nextExporter)
	{
		this.propertyExporter = propertyExporter;
		this.nextExporter = nextExporter;
	}

	public void Export(IElement element, IAlWriter writer)
	{
		ExportProperties(element, writer);
		nextExporter.Export(element, writer);
	}

	protected virtual void ExportProperties(IElement element, IAlWriter writer)
	{
		PropertyTypeInfo[] array = (from v in element.ElementTypeInfo.Properties.Values
			select v.Property into p
			where element.GetPropertySetState(p.PropertyType) && !propertyExporter.IsPropertyIgnored(element, p)
			select p).ToArray();
		if (writer.PendingEmptyLine)
		{
			writer.WriteLine();
		}
		PropertyTypeInfo[] array2 = array;
		foreach (PropertyTypeInfo propInfo in array2)
		{
			WriteProperty(element, writer, propInfo);
		}
		propertyExporter.WriteMandatoryProperties(element, writer, array);
		writer.PendingEmptyLine = true;
	}

	private void WriteProperty(IElement element, IAlWriter writer, PropertyTypeInfo propInfo)
	{
		bool flag = propertyExporter.TryCommentOutProperty(element, propInfo, out var commentPart);
		if (!string.IsNullOrWhiteSpace(commentPart))
		{
			writer.WriteLine(commentPart);
		}
		bool flag2 = false;
		if (flag)
		{
			if (!propertyExporter.GetPropertyValue(element, propInfo, writer.State).Contains(Environment.NewLine))
			{
				writer.Write("//");
			}
			else
			{
				writer.WriteLine("/*");
				flag2 = true;
			}
		}
		string propertyName = propertyExporter.GetPropertyName(element, propInfo);
		writer.WritePropertyWithValueAsAction(propertyName, () =>
		{
			writer.Write(propertyExporter.GetPropertyValue(element, propInfo, writer.State));
		});
		if (flag2)
		{
			writer.WriteLine("*/");
		}
		if (AlUtilities.IsMultilanguageProperty(element, propInfo.PropertyType))
		{
			TranslationUtilities.ProcessAndGetLabelSyntax(writer.State.TranslationExtractor, element, propInfo.PropertyType);
		}
	}
}
