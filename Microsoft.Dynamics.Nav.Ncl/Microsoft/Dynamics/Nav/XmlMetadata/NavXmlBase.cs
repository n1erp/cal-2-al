using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public abstract class NavXmlBase
{
	private string id;

	[XmlAttribute("id")]
	public string Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}
}
