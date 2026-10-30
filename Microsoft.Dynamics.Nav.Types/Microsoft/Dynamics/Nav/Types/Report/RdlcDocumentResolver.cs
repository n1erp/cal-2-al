using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Types.Report;

internal class RdlcDocumentResolver
{
	private readonly List<IRdlcLayoutElement> layoutElements;

	private readonly RdlcDocumentContainer container;

	public RdlcDocumentResolver(RdlcDocumentContainer container, List<IRdlcLayoutElement> layoutElements)
	{
		this.layoutElements = layoutElements;
		this.container = container;
	}

	public void Resolve(bool throwOnErrors)
	{
		foreach (IRdlcLayoutElement layoutElement in layoutElements)
		{
			layoutElement.Clear();
		}
		if (container.Root == null)
		{
			return;
		}
		XElement xElement = container.Root.Element(container.Ns + "ReportSections");
		if (xElement == null || xElement.Element(container.Ns + "ReportSection") == null)
		{
			return;
		}
		foreach (XElement item in from el in xElement.Descendants()
			where !el.HasElements && !string.IsNullOrEmpty(el.Value) && el.Value[0] == '='
			select el)
		{
			foreach (IRdlcLayoutElement layoutElement2 in layoutElements)
			{
				layoutElement2.ResolveReference(throwOnErrors, item);
			}
		}
	}
}
