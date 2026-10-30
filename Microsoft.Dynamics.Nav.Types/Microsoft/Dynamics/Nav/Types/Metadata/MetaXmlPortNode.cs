using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public abstract class MetaXmlPortNode
{
	private Guid id = Guid.Empty;

	private Guid parentId = Guid.Empty;

	private int indentation;

	private int width;

	private string nodeName;

	private string nodePrefix = string.Empty;

	private XmlPortNodeType nodeType = XmlPortNodeType.Element;

	private XmlPortNodeSourceType sourceType = XmlPortNodeSourceType.Text;

	private MetaXmlPortNode parent;

	private List<MetaXmlPortNode> children = new List<MetaXmlPortNode>();

	public abstract XmlPortNodeMinOccurs MinOccurs { get; }

	public abstract XmlPortNodeMaxOccurs MaxOccurs { get; }

	public abstract XmlPortNodeOccurrence Occurrence { get; }

	public abstract bool Unbound { get; }

	public Guid Id
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

	public Guid ParentId
	{
		get
		{
			return parentId;
		}
		set
		{
			parentId = value;
		}
	}

	public int Indentation
	{
		get
		{
			return indentation;
		}
		set
		{
			indentation = value;
		}
	}

	public int Width
	{
		get
		{
			return width;
		}
		set
		{
			width = value;
		}
	}

	public string NodeName
	{
		get
		{
			return nodeName;
		}
		set
		{
			nodeName = value;
		}
	}

	public string NodePrefix
	{
		get
		{
			return nodePrefix;
		}
		set
		{
			nodePrefix = value;
		}
	}

	public XmlPortNodeType NodeType
	{
		get
		{
			return nodeType;
		}
		set
		{
			nodeType = value;
		}
	}

	public XmlPortNodeSourceType SourceType
	{
		get
		{
			return sourceType;
		}
		set
		{
			sourceType = value;
		}
	}

	public MetaXmlPortNode Parent
	{
		get
		{
			return parent;
		}
		set
		{
			parent = value;
		}
	}

	internal List<MetaXmlPortNode> Children => children;

	internal bool HasChildren => children.Count > 0;

	internal List<MetaXmlPortNode> AttributeChildren => children.FindAll((MetaXmlPortNode node) => node.NodeType == XmlPortNodeType.Attribute);

	internal List<MetaXmlPortNode> ElementChildren => children.FindAll((MetaXmlPortNode node) => node.NodeType == XmlPortNodeType.Element);

	internal bool HasElementChildren
	{
		get
		{
			Predicate<MetaXmlPortNode> match = (MetaXmlPortNode node) => node.NodeType == XmlPortNodeType.Element;
			return children.Find(match) != null;
		}
	}
}
