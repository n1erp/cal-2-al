using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public class NavXmlAdd : NavXmlBase
{
	private string destination;

	private NavXmlNode node;

	private string parentPath;

	private string elementIdPath;

	private NavXmlNode elementId;

	[XmlElement("Node")]
	public NavXmlNode Node
	{
		get
		{
			return node;
		}
		set
		{
			node = value;
		}
	}

	[XmlAttribute("parentPath")]
	public string ParentPath
	{
		get
		{
			return parentPath;
		}
		set
		{
			parentPath = value;
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

	[XmlAttribute("elementIdPath")]
	public string ElementIdPath
	{
		get
		{
			return elementIdPath;
		}
		set
		{
			elementIdPath = value;
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
}
