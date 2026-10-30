using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

[XmlRoot("Node")]
public class NavXmlNode
{
	private Collection<NavXmlNode> childNodes;

	private Collection<NavXmlNodeAttribute> attributes;

	private string name;

	private string uri;

	private string id;

	[XmlArray("Nodes")]
	[XmlArrayItem("Node", typeof(NavXmlNode))]
	public Collection<NavXmlNode> ChildNodes
	{
		get
		{
			return childNodes;
		}
		set
		{
			childNodes = value;
		}
	}

	[XmlArray("Attributes")]
	[XmlArrayItem("Attribute", typeof(NavXmlNodeAttribute))]
	public Collection<NavXmlNodeAttribute> Attributes
	{
		get
		{
			return attributes;
		}
		set
		{
			attributes = value;
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

	public NavXmlNode()
	{
		childNodes = new Collection<NavXmlNode>();
		attributes = new Collection<NavXmlNodeAttribute>();
	}
}
