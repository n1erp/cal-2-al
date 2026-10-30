namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavXmlPortFixedTextAdapter : NavXmlPortTextAdapter
{
	internal NavXmlPortFixedTextAdapter(NavXmlPort xmlPortInstance)
		: base(xmlPortInstance)
	{
	}

	private static string MakeFixedWidthValue(int width, string value)
	{
		if (width > 0)
		{
			if (width < value.Length)
			{
				return value.Substring(0, width);
			}
			return value.PadRight(width, ' ');
		}
		return string.Empty;
	}

	public override void WriteAttributeString(NavXmlPortNode node, string value)
	{
		WriteStringToStream(MakeFixedWidthValue(node.MetaData.Width, value), "");
	}

	public override void WriteNamespaceString(string prefix, string value)
	{
	}

	public override void WriteString(string text, NavXmlPortNode node)
	{
		if (node.MetaData.Parent != null)
		{
			WriteStringToStream(MakeFixedWidthValue(node.MetaData.Width, text), "");
		}
	}
}
