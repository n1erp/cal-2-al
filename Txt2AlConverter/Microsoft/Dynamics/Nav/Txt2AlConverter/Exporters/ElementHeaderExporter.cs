using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;

internal sealed class ElementHeaderExporter : IElementExporter
{
	private readonly IEnumerable<PropertyType> inlineProperties;

	private readonly IPropertiesConverter propertiesExporter;

	public ElementHeaderExporter(IPropertiesConverter propertiesExporter, IEnumerable<PropertyType> inlineProperties)
	{
		this.inlineProperties = inlineProperties;
		this.propertiesExporter = propertiesExporter;
	}

	public void Export(IElement element, IAlWriter writer)
	{
		writer.Write(AlUtilities.GetSectionTypeName(element).ToLowerInvariant());
		writer.Write('(');
		AlUtilities.WriteElementHeaderContent(writer, element, inlineProperties.ToArray(), propertiesExporter);
		writer.WriteLine(')');
		writer.PendingEmptyLine = false;
	}
}
