using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class TriggerExporter : ProcedureExporter
{
	public override void Export(IElement element, IAlWriter writer)
	{
		foreach (ElementTypeInfo item in from r in element.ElementTypeInfo.Relations.Values
			select r.Child into e
			where e.Category == CategoryType.Trigger
			select e)
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
			ExportFunctionBody(writer, trigger);
			writer.PendingEmptyLine = true;
		}
	}
}
