namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class TerminallNestedElementExporter : INestedElementExporter
{
	private readonly IElementExporter childlessElementExporter;

	public TerminallNestedElementExporter(IElementExporter childlessElementExporter)
	{
		this.childlessElementExporter = childlessElementExporter;
	}

	public void Export(ElementTreeNode elementTreeNode, IAlWriter writer)
	{
		childlessElementExporter.Export(elementTreeNode.Element, writer);
	}
}
