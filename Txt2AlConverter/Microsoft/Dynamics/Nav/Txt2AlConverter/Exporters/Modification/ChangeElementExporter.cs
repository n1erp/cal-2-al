using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class ChangeElementExporter : INestedElementExporter
{
	private IExporterLocator ExporterLocator { get; }

	public ChangeElementExporter(IExporterLocator exporterLocator)
	{
		ExporterLocator = exporterLocator;
	}

	public void Export(ElementTreeNode elementTreeNode, IAlWriter writer)
	{
		GetElementExporter(elementTreeNode.Element).Export(elementTreeNode, writer);
	}

	private INestedElementExporter GetElementExporter(IElement child)
	{
		string elementKey = ExporterLocator.GetElementKey(child);
		return ExporterLocator.GetService<INestedElementExporter>(elementKey);
	}
}
