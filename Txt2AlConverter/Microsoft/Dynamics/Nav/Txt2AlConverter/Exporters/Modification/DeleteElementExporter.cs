using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class DeleteElementExporter : IElementExporter
{
	public void Export(IElement element, IAlWriter writer)
	{
		ExportHideElement(element, writer);
	}

	private static void ExportHideElement(IElement element, IAlWriter writer)
	{
		PropertyChangeExporter.ExportPropertyChangeSection(element, "Visible", "false", writer);
	}
}
