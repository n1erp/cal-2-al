using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Page;

internal class PageContentExporter : IElementExporter
{
	private readonly IElementExporter contentExporter;

	public PageContentExporter(AlExportOptions options)
	{
		PageSectionsExporter nextExporter = new PageSectionsExporter(new UnresolvedDotNetEventsExporter(new ElementWithTriggersExporter(new ElementWithVariablesExporter(new ElementWithProceduresExporter(new NullExporter(), options), options), options)), options);
		ElementWithPropertiesExporter nextExporter2 = new ElementWithPropertiesExporter(new PagePropertiesConverter(), nextExporter);
		contentExporter = new ElementWithDocumentationExporter(nextExporter2);
	}

	public void Export(IElement element, IAlWriter writer)
	{
		contentExporter.Export(element, writer);
	}
}
