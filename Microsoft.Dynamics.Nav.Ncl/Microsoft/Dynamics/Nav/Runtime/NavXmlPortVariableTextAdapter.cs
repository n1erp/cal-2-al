namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavXmlPortVariableTextAdapter : NavXmlPortTextAdapter
{
	internal NavXmlPortVariableTextAdapter(NavXmlPort xmlPortInstance)
		: base(xmlPortInstance)
	{
	}

	public override void WriteAttributeString(NavXmlPortNode node, string value)
	{
		WriteStringToStream(FieldDelimiter + value + FieldDelimiter, FieldSeparator);
	}

	public override void WriteNamespaceString(string prefix, string value)
	{
	}

	public override void WriteString(string text, NavXmlPortNode node)
	{
		if (node.MetaData.Parent != null)
		{
			WriteStringToStream(FieldDelimiter + text + FieldDelimiter, FieldSeparator);
		}
	}
}
