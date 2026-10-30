using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Query;

internal class QueryContentExporter : IElementExporter
{
	private readonly IElementExporter contentExporter;

	public QueryContentExporter(AlExportOptions options)
	{
		QuerySectionsExporter nextExporter = new QuerySectionsExporter(new ElementWithTriggersExporter(new ElementWithVariablesExporter(new ElementWithProceduresExporter(new NullExporter(), options), options), options), options);
		ElementWithPropertiesExporter nextExporter2 = new ElementWithPropertiesExporter(new QueryPropertiesConverter(), nextExporter);
		contentExporter = new ElementWithDocumentationExporter(nextExporter2);
	}

	public void Export(IElement element, IAlWriter writer)
	{
		contentExporter.Export(element, writer);
	}
}
