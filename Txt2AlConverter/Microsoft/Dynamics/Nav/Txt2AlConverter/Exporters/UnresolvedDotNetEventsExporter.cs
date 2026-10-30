using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;

internal sealed class UnresolvedDotNetEventsExporter : IElementExporter
{
	private readonly IElementExporter exporter;

	private readonly ProcedureExporter procedureExporter;

	public UnresolvedDotNetEventsExporter(IElementExporter exporter)
	{
		this.exporter = exporter;
		procedureExporter = new CommentedProcedureExporter();
	}

	public void Export(IElement element, IAlWriter writer)
	{
		ExportEventSubscribers(element, writer);
		exporter.Export(element, writer);
	}

	private void ExportEventSubscribers(IElement element, IAlWriter writer)
	{
		IEnumerable<IElement> source = from e in element.GetSubElements()
			where e.IsUserControl()
			select e;
		IEnumerable<IElement> source2 = from e in element.GetSubElements(ElementType.Variable)
			where e.IsOfType("DotNet")
			select e;
		foreach (IElement eventTrigger in element.GetSubElements(ElementType.Event))
		{
			if (!source.Any((IElement userControl) => AlUtilities.IsEventDefinedByElement(eventTrigger, userControl)) && !source2.Any((IElement variable) => AlUtilities.IsEventDefinedByElement(eventTrigger, variable)))
			{
				string triggerEventFullName = GetTriggerEventFullName(eventTrigger);
				writer.WriteLine();
				writer.WriteLinePossiblyCommentedOut(" Could not resolve the usercontrol owning '" + triggerEventFullName + "'", commentedOut: true);
				writer.PendingEmptyLine = false;
				procedureExporter.Export(eventTrigger, writer);
			}
		}
	}

	private static string GetTriggerEventFullName(IElement eventTrigger)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}@{1}::{2}@{3}", eventTrigger.GetStringProperty(PropertyType.EventSource), eventTrigger.GetStringProperty(PropertyType.EventSourceId), eventTrigger.GetStringProperty(PropertyType.Name), eventTrigger.Id);
	}
}
