using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal sealed class ElementWithDocumentationExporter : IElementExporter
{
	private readonly IElementExporter nextExporter;

	public ElementWithDocumentationExporter(IElementExporter nextExporter)
	{
		this.nextExporter = nextExporter;
	}

	public void Export(IElement element, IAlWriter writer)
	{
		ExportDocumentation(element, writer);
		nextExporter.Export(element, writer);
	}

	private static void ExportDocumentation(IElement element, IAlWriter writer)
	{
		IElement element2 = element.GetSubElements(ElementType.Documentation).FirstOrDefault();
		if (element2 != null)
		{
			ExportDocumentationLines(element2, writer);
		}
	}

	internal static void ExportDocumentationLines(IElement documentation, IAlWriter writer)
	{
		foreach (string codeLine in documentation.GetCodeLines())
		{
			writer.Write("// ");
			writer.WriteLine(codeLine);
		}
		writer.PendingEmptyLine = true;
	}
}
