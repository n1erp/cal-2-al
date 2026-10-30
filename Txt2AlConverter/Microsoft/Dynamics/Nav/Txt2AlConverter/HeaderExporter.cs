using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class HeaderExporter : IElementExporter
{
	private readonly IElementExporter bodyExporter;

	protected IEnumerable<PropertyType> InlineProperties { get; }

	protected IPropertiesConverter PropertyExporter { get; }

	public HeaderExporter(IPropertiesConverter propertyExporter, IEnumerable<PropertyType> inlineProperties, IElementExporter bodyExporter)
	{
		PropertyExporter = propertyExporter;
		InlineProperties = inlineProperties;
		this.bodyExporter = bodyExporter;
	}

	public void Export(IElement element, IAlWriter writer)
	{
		ExportHeader(element, writer);
		writer.WriteSection(() =>
		{
			bodyExporter.Export(element, writer);
		});
	}

	protected virtual void ExportHeader(IElement element, IAlWriter writer)
	{
		writer.Write(AlUtilities.GetSectionTypeName(element).ToLowerInvariant());
		writer.Write('(');
		AlUtilities.WriteElementHeaderContent(writer, element, InlineProperties.ToArray(), PropertyExporter);
		writer.WriteLine(')');
		writer.PendingEmptyLine = false;
	}
}
