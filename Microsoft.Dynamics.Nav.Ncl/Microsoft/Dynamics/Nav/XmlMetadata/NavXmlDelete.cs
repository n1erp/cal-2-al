using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public class NavXmlDelete : NavXmlBase
{
	private string elementPath;

	private NavXmlNode elementId;

	private string destination;

	[XmlElement("elementId")]
	public NavXmlNode ElementId
	{
		get
		{
			return elementId;
		}
		set
		{
			elementId = value;
		}
	}

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

	[XmlAttribute("elementPath")]
	public string ElementPath
	{
		get
		{
			return elementPath;
		}
		set
		{
			elementPath = value;
		}
	}
}
