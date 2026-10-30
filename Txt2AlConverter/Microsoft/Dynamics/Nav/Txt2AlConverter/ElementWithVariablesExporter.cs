using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class ElementWithVariablesExporter : IElementExporter
{
	private readonly IElementExporter nextExporter;

	private readonly AlExportOptions options;

	public ElementWithVariablesExporter(IElementExporter nextExporter, AlExportOptions options)
	{
		this.nextExporter = nextExporter;
		this.options = options;
	}

	public void Export(IElement element, IAlWriter writer)
	{
		ExportVariables(element, writer);
		nextExporter.Export(element, writer);
	}

	private void ExportVariables(IElement element, IAlWriter writer)
	{
		if (options.TableDataOnly)
		{
			return;
		}
		IElement[] array = element.GetSubElements(ElementType.Variable).ToArray();
		if (array.Length != 0)
		{
			if (writer.PendingEmptyLine)
			{
				writer.WriteLine();
			}
			writer.WriteLine("var");
			writer.PushIndent(writer.Indentation);
			IElement[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				AlUtilities.WriteVariableDeclaration(array2[i], writer);
				writer.WriteLine(';');
			}
			writer.PopIndent();
			writer.PendingEmptyLine = true;
		}
	}
}
