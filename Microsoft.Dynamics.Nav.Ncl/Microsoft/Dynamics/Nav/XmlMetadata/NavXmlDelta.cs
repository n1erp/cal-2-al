using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

[XmlRoot("delta")]
public class NavXmlDelta
{
	private Collection<NavXmlBase> items;

	[XmlArray("changes")]
	[XmlArrayItem("move", typeof(NavXmlMove))]
	[XmlArrayItem("order", typeof(NavXmlOrder))]
	[XmlArrayItem("update", typeof(NavXmlUpdate))]
	[XmlArrayItem("delete", typeof(NavXmlDelete))]
	[XmlArrayItem("add", typeof(NavXmlAdd))]
	public Collection<NavXmlBase> Items
	{
		get
		{
			return items;
		}
		set
		{
			items = value;
		}
	}

	public NavXmlDelta()
	{
		items = new Collection<NavXmlBase>();
	}

	public static NavXmlDelta Build(IXPathNavigable source, IXPathNavigable target, IXmlDeltaCalculationRules rules = null)
	{
		NavXmlDeltaCalculation navXmlDeltaCalculation = new NavXmlDeltaCalculation(rules);
		return navXmlDeltaCalculation.Build(source, target);
	}

	public static void Apply(XmlDocument document, NavXmlDelta delta)
	{
		NavXmlDeltaCalculation navXmlDeltaCalculation = new NavXmlDeltaCalculation();
		navXmlDeltaCalculation.Apply(document, delta);
	}
}
