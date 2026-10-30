using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public class NavXmlUpdate : NavXmlBase
{
	private string uri;

	private string name;

	private string valueField;

	private string elementPath;

	private NavXmlNode elementId;

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
}
