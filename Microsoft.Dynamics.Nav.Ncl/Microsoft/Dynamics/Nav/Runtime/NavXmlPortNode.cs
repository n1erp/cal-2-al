using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

[DebuggerDisplay("{metadata.NodeName} : {metadata.NodeType} -> {metadata.SourceType}")]
public abstract class NavXmlPortNode
{
	private List<NavXmlPortNode> attributeChildren = new List<NavXmlPortNode>();

	private List<NavXmlPortNode> elementChildren = new List<NavXmlPortNode>();

	private MetaXmlPortNode metadata;

	private NavXmlPortNode nodeTableParent;

	private bool breakNode;

	private bool skipNode;

	private bool recordInitialized;

	private bool linkRecordInserted;

	private int occurs;

	internal MetaXmlPortNode MetaData
	{
		get
		{
			return metadata;
		}
		set
		{
			metadata = value;
		}
	}

	internal NavXmlPortNode NodeTableParent
	{
		get
		{
			return nodeTableParent;
		}
		set
		{
			nodeTableParent = value;
		}
	}

	internal List<NavXmlPortNode> AttributeChildren => attributeChildren;

	internal List<NavXmlPortNode> ElementChildren => elementChildren;

	internal bool HasAttributes => attributeChildren.Count > 0;

	internal bool HasChildren => elementChildren.Count > 0;

	internal bool BreakNode
	{
		get
		{
			return breakNode;
		}
		set
		{
			breakNode = value;
		}
	}

	internal bool SkipNode
	{
		get
		{
			return skipNode;
		}
		set
		{
			skipNode = value;
		}
	}

	internal bool RecordInitialized
	{
		get
		{
			return recordInitialized;
		}
		set
		{
			recordInitialized = value;
		}
	}

	internal bool LinkRecordInserted
	{
		get
		{
			return linkRecordInserted;
		}
		set
		{
			linkRecordInserted = value;
		}
	}

	internal int Occurs
	{
		get
		{
			return occurs;
		}
		set
		{
			occurs = value;
		}
	}

	public override int GetHashCode()
	{
		return metadata.Id.GetHashCode();
	}
}
