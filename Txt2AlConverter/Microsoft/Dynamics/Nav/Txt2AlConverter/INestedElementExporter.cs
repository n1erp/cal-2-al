namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal interface INestedElementExporter
{
	void Export(ElementTreeNode elementTreeNode, IAlWriter writer);
}
