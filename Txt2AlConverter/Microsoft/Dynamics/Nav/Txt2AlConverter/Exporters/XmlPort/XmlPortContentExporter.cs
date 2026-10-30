using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.XmlPort;

internal class XmlPortContentExporter : IElementExporter
{
	private readonly IElementExporter contentExporter;

	public XmlPortContentExporter(AlExportOptions options)
	{
		XmlPortSectionsExporter nextExporter = new XmlPortSectionsExporter(new ElementWithTriggersExporter(new ElementWithVariablesExporter(new ElementWithProceduresExporter(new NullExporter(), options), options), options), options);
		ElementWithPropertiesExporter nextExporter2 = new ElementWithPropertiesExporter(new XmlPortPropertiesConverter(), nextExporter);
		contentExporter = new ElementWithDocumentationExporter(nextExporter2);
	}

	public void Export(IElement element, IAlWriter writer)
	{
		contentExporter.Export(element, writer);
	}
}
