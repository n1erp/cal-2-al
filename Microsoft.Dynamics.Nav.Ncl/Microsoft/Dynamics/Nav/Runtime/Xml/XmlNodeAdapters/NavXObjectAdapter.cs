using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Xml.XmlNodeAdapters;

internal abstract class NavXObjectAdapter<TObject> : INavXmlNode where TObject : XObject
{
	private readonly TObject value;

	protected TObject Value
	{
		get
		{
			if (value == null)
			{
				throw new NavNCLNotInitializedException(typeof(NavXmlNode));
			}
			return value;
		}
	}

	public XmlNodeType NodeType => Value.NodeType;

	public string OuterXml => Value.ToString();

	protected NavXObjectAdapter(TObject value)
	{
		this.value = value;
	}

	public bool GetDocument(DataError errorLevel, ByRef<NavXmlDocument> result)
	{
		XDocument document = Value.Document;
		if (document == null)
		{
			return TrappableXmlOperationExecutor.RaiseXmlNodeNotFound(errorLevel);
		}
		result.Value = new NavXmlDocument(document);
		return true;
	}

	public bool GetParent(DataError errorLevel, ByRef<NavXmlElement> result)
	{
		XElement parent = Value.Parent;
		if (parent == null)
		{
			return TrappableXmlOperationExecutor.RaiseXmlNodeNotFound(errorLevel);
		}
		result.Value = new NavXmlElement(parent);
		return true;
	}

	public abstract bool AddAfterSelf(DataError errorLevel, params object[] content);

	public abstract bool AddBeforeSelf(DataError errorLevel, params object[] content);

	public abstract bool Remove(DataError errorLevel);

	public abstract bool ReplaceWith(DataError errorLevel, params object[] content);

	public abstract bool SelectSingleNode(DataError errorLevel, string xpath, ByRef<NavXmlNode> result);

	public bool WriteTo(DataError errorLevel, NavOutStream data)
	{
		return WriteTo(errorLevel, NavXmlWriteOptions.Default, data);
	}

	public abstract bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, NavOutStream data);

	public bool WriteTo(DataError errorLevel, ByRef<NavText> data)
	{
		return WriteTo(errorLevel, NavXmlWriteOptions.Default, data);
	}

	public abstract bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, ByRef<NavText> data);

	public abstract bool SelectSingleNode(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNode> result);

	public abstract bool SelectNodes(DataError errorLevel, string xpath, ByRef<NavXmlNodeList> result);

	public abstract bool SelectNodes(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNodeList> result);
}
