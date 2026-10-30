using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavXmlContainerGeneric<T> : NavXmlNodeGeneric<T> where T : class
{
	internal new XContainer Value => (XContainer)(object)base.Value;

	public bool ALAdd(DataError errorLevel, params object[] content)
	{
		return Add(errorLevel, content);
	}

	public bool ALAddFirst(DataError errorLevel, params object[] content)
	{
		return AddFirst(errorLevel, content);
	}

	public void ALRemoveNodes()
	{
		RemoveNodes();
	}

	public bool ALReplaceNodes(DataError errorLevel, params object[] content)
	{
		return ReplaceNodes(errorLevel, content);
	}

	public NavXmlNodeList ALGetChildNodes()
	{
		return GetChildNodes();
	}

	public NavXmlNodeList ALGetChildElements()
	{
		return GetChildElements();
	}

	public NavXmlNodeList ALGetChildElements(string name)
	{
		return GetChildElements(name);
	}

	public NavXmlNodeList ALGetChildElements(string localName, string namespaceUri)
	{
		return GetChildElements(localName, namespaceUri);
	}

	public NavXmlNodeList ALGetDescendantNodes()
	{
		return GetDescendantNodes();
	}

	public NavXmlNodeList ALGetDescendantElements()
	{
		return GetDescendantElements();
	}

	public NavXmlNodeList ALGetDescendantElements(string name)
	{
		return GetDescendantElements(name);
	}

	public NavXmlNodeList ALGetDescendantElements(string localName, string namespaceUri)
	{
		return GetDescendantElements(localName, namespaceUri);
	}

	internal NavXmlContainerGeneric(T value)
		: base(value)
	{
	}

	private bool Add(DataError errorLevel, params object[] content)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			object[] content2 = XmlNodeMapper.UnwrapXmlNodes(content);
			Value.Add(content2);
		});
	}

	private bool AddFirst(DataError errorLevel, params object[] content)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			object[] content2 = XmlNodeMapper.UnwrapXmlNodes(content);
			Value.AddFirst(content2);
		});
	}

	private void RemoveNodes()
	{
		Value.RemoveNodes();
	}

	private bool ReplaceNodes(DataError errorLevel, params object[] content)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			object[] content2 = XmlNodeMapper.UnwrapXmlNodes(content);
			Value.ReplaceNodes(content2);
		});
	}

	private NavXmlNodeList GetChildNodes()
	{
		return XmlNodeMapper.CreateXmlNodeList(Value.Nodes());
	}

	private NavXmlNodeList GetChildElements()
	{
		return XmlNodeMapper.CreateXmlNodeList(Value.Elements());
	}

	private NavXmlNodeList GetChildElements(string name)
	{
		IEnumerable<XElement> values = Value.Elements(name);
		return XmlNodeMapper.CreateXmlNodeList(values);
	}

	private NavXmlNodeList GetChildElements(string localName, string namespaceUri)
	{
		IEnumerable<XElement> values = Value.Elements(XName.Get(localName, namespaceUri));
		return XmlNodeMapper.CreateXmlNodeList(values);
	}

	private NavXmlNodeList GetDescendantNodes()
	{
		return XmlNodeMapper.CreateXmlNodeList(Value.DescendantNodes());
	}

	private NavXmlNodeList GetDescendantElements()
	{
		return XmlNodeMapper.CreateXmlNodeList(Value.Descendants());
	}

	private NavXmlNodeList GetDescendantElements(string name)
	{
		IEnumerable<XElement> values = Value.Descendants(name);
		return XmlNodeMapper.CreateXmlNodeList(values);
	}

	private NavXmlNodeList GetDescendantElements(string localName, string namespaceUri)
	{
		IEnumerable<XElement> values = Value.Descendants(XName.Get(localName, namespaceUri));
		return XmlNodeMapper.CreateXmlNodeList(values);
	}
}
