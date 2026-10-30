using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal class ControlAddInDocket
{
	private readonly List<ControlAddInEntry> controlAddIns = new List<ControlAddInEntry>();

	public IList<ControlAddInEntry> ControlAddIns => controlAddIns;

	public ControlAddInEntry AddInForFile(string fileName)
	{
		return ControlAddIns.FirstOrDefault((ControlAddInEntry c) => string.Equals(Path.GetFileName(fileName), c.FileName, StringComparison.OrdinalIgnoreCase));
	}

	public XElement ToXml()
	{
		XName name = ControlAddInXmlPart.ControlAddInDocket.ToString();
		object[] content = ControlAddIns.Select((ControlAddInEntry c) => c.ToXml()).ToArray();
		return new XElement(name, content);
	}

	public static ControlAddInDocket FromXml(XContainer element)
	{
		ControlAddInDocket controlAddInDocket = new ControlAddInDocket();
		foreach (XElement item in element.Elements(ControlAddInXmlPart.ControlAddIn.ToString()))
		{
			controlAddInDocket.ControlAddIns.Add(ControlAddInEntry.FromXml(item));
		}
		return controlAddInDocket;
	}
}
