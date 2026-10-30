using System;
using System.Xml;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class XmlUtilities
{
	public static void WriteStartEnd(this XmlWriter writer, string nodeName, Action action, string namespacee = null)
	{
		if (namespacee == null)
		{
			writer.WriteStartElement(nodeName);
		}
		else
		{
			writer.WriteStartElement(string.Empty, nodeName, namespacee);
		}
		action();
		writer.WriteEndElement();
	}
}
