using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Codeunit;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal sealed class CodeunitContentExporter : IElementExporter
{
	private readonly IElementExporter contentExporter;

	public CodeunitContentExporter(AlExportOptions options)
	{
		ElementWithTriggersExporter nextExporter = new ElementWithTriggersExporter(new ElementWithVariablesExporter(new ElementWithProceduresExporter(new NullExporter(), options), options), options);
		ElementWithPropertiesExporter nextExporter2 = new ElementWithPropertiesExporter(new CodeunitPropertiesConverter(), nextExporter);
		contentExporter = new ElementWithDocumentationExporter(nextExporter2);
	}

	public void Export(IElement element, IAlWriter writer)
	{
		contentExporter.Export(element, writer);
	}
}
