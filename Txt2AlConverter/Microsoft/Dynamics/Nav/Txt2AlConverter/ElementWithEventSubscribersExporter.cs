using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal sealed class ElementWithEventSubscribersExporter : IElementExporter
{
	private readonly IElementExporter exporter;

	private readonly ProcedureExporter procedureExporter;

	public ElementWithEventSubscribersExporter(IElementExporter exporter)
	{
		this.exporter = exporter;
		procedureExporter = new ProcedureExporter();
	}

	public void Export(IElement element, IAlWriter writer)
	{
		ExportEventSubscribers(element, writer);
		exporter.Export(element, writer);
	}

	private void ExportEventSubscribers(IElement element, IAlWriter writer)
	{
		IElement containingApplicationObjectElement = element.GetContainingApplicationObjectElement();
		if (containingApplicationObjectElement == null)
		{
			return;
		}
		foreach (IElement item in from e in containingApplicationObjectElement.GetSubElements(ElementType.Event)
			where AlUtilities.IsEventDefinedByElement(e, element)
			select e)
		{
			procedureExporter.Export(item, writer);
		}
	}
}
