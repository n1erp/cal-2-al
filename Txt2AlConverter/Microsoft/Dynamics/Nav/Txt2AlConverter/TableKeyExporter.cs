using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Table;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal sealed class TableKeyExporter : IElementExporter
{
	private class TableKeyHeaderExporter : HeaderExporter
	{
		private int sequentialNumberForKeyId;

		public TableKeyHeaderExporter(IPropertiesConverter propertiesExporter, IEnumerable<PropertyType> inlineProperties, IElementExporter bodyExporter)
			: base(propertiesExporter, inlineProperties, bodyExporter)
		{
			sequentialNumberForKeyId = 1;
		}

		protected override void ExportHeader(IElement element, IAlWriter writer)
		{
			writer.Write(AlUtilities.GetSectionTypeName(element).ToLowerInvariant());
			writer.Write('(');
			WriteKeyName(writer);
			AlUtilities.WriteElementHeaderContent(writer, element, InlineProperties.ToArray(), PropertyExporter);
			writer.WriteLine(')');
			sequentialNumberForKeyId++;
			writer.PendingEmptyLine = false;
		}

		private void WriteKeyName(IAlWriter writer)
		{
			writer.Write("Key" + sequentialNumberForKeyId);
			writer.Write(';');
		}
	}

	private readonly IElementExporter keyExporter;

	private readonly PropertyType[] template;

	public TableKeyExporter(AlExportOptions options)
	{
		template = new PropertyType[1] { PropertyType.Key };
		TablePropertiesConverter tablePropertiesConverter = new TablePropertiesConverter(options);
		keyExporter = new TableKeyHeaderExporter(tablePropertiesConverter, template, new ElementWithPropertiesExporter(tablePropertiesConverter, new NullExporter()));
	}

	public void Export(IElement element, IAlWriter writer)
	{
		if (!IsKeyElementDisabled(element))
		{
			keyExporter.Export(element, writer);
		}
	}

	private static bool IsKeyElementDisabled(IElement keyElement)
	{
		if (keyElement.TryGetStringProperty(PropertyType.Enabled, out var value))
		{
			return string.Equals(value, "no", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}
}
