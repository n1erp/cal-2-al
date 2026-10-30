using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Report;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Report;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class ReportContentExporter : IElementExporter
{
	private readonly IElementExporter contentExporter;

	public ReportContentExporter(AlExportOptions options)
	{
		ReportPropertiesExporter nextExporter = new ReportPropertiesExporter(new ReportSectionsExporter(new ElementWithTriggersExporter(new ElementWithVariablesExporter(new ElementWithProceduresExporter(new NullExporter(), options), options), options), options));
		contentExporter = new ElementWithDocumentationExporter(nextExporter);
	}

	public void Export(IElement element, IAlWriter writer)
	{
		contentExporter.Export(element, writer);
	}
}
