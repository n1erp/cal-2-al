using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal sealed class ElementWithTriggersExporter : IElementExporter
{
	private readonly IElementExporter exporter;

	private readonly AlExportOptions options;

	private readonly ProcedureExporter procedureExporter;

	public ElementWithTriggersExporter(IElementExporter exporter, AlExportOptions options)
	{
		this.exporter = exporter;
		this.options = options;
		procedureExporter = new ProcedureExporter();
	}

	public void Export(IElement element, IAlWriter writer)
	{
		ExportTriggers(element, writer);
		exporter.Export(element, writer);
	}

	private void ExportTriggers(IElement element, IAlWriter writer)
	{
		foreach (ElementTypeInfo item in element.ElementTypeInfo.Relations.Values.Select((ElementRelation r) => r.Child).Where(IsTrigger))
		{
			if (item.ElementType != ElementType.Documentation)
			{
				IElement firstSubElement = element.GetFirstSubElement(item.ElementType);
				if (firstSubElement != null)
				{
					ExportTrigger(element, writer, item, firstSubElement);
				}
			}
		}
	}

	private bool IsTrigger(ElementTypeInfo element)
	{
		if (options.TableDataOnly)
		{
			return false;
		}
		return element.Category == CategoryType.Trigger;
	}

	private void ExportTrigger(IElement element, IAlWriter writer, ElementTypeInfo elemInfo, IElement trigger)
	{
		TriggerTypeInfo triggerTypeInfo = AlUtilities.GetTriggerTypeInfo(element, elemInfo.Name);
		if (triggerTypeInfo != null)
		{
			if (writer.PendingEmptyLine)
			{
				writer.WriteLine();
			}
			writer.WriteLine(triggerTypeInfo.GetSignatureString());
			procedureExporter.ExportFunctionBody(writer, trigger);
			writer.PendingEmptyLine = true;
		}
	}
}
