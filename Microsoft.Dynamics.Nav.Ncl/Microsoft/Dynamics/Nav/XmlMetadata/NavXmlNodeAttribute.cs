using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

[XmlRoot("Node")]
public class NavXmlNodeAttribute
{
	private string valueField;

	private string name;

	private string uri;

	[XmlAttribute("value")]
	public string Value
	{
		get
		{
			return valueField;
		}
		set
		{
			valueField = value;
		}
	}

	[XmlAttribute("name")]
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	[XmlAttribute("uri")]
	public string Uri
	{
		get
		{
			return uri;
		}
		set
		{
			uri = value;
		}
	}
}
