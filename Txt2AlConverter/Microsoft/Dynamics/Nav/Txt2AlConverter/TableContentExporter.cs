using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Table;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Table;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class TableContentExporter : IElementExporter
{
	private readonly IElementExporter contentExporter;

	public TableContentExporter(AlExportOptions options)
	{
		TableSectionsExporter nextExporter = new TableSectionsExporter(new ElementWithTriggersExporter(new ElementWithVariablesExporter(new ElementWithProceduresExporter(new NullExporter(), options), options), options), options);
		ElementWithPropertiesExporter nextExporter2 = new ElementWithPropertiesExporter(new TablePropertiesConverter(options), nextExporter);
		contentExporter = new ElementWithDocumentationExporter(nextExporter2);
	}

	public void Export(IElement element, IAlWriter writer)
	{
		contentExporter.Export(element, writer);
	}
}
