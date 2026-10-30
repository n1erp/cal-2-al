using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public class NavXmlMove : NavXmlBase
{
	private string destination;

	[XmlAttribute("destination")]
	public string Destination
	{
		get
		{
			return destination;
		}
		set
		{
			destination = value;
		}
	}
}
