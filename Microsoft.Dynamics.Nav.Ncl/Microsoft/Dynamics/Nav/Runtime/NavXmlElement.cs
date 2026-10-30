using System;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlElement : NavXmlContainerGeneric<object>
{
	private readonly Lazy<NavXmlAttributeCollection> attributeCollection;

	public new static NavXmlElement Default => new NavXmlElement(null);

	public bool ALHasAttributes => HasAttributes;

	public bool ALHasElements => HasElements;

	public bool ALIsEmpty => IsEmpty;

	public string ALName => Name;

	public string ALLocalName => LocalName;

	public string ALNamespaceUri => NamespaceUri;

	public string ALInnerXml => InnerXml;

	public string ALInnerText => InnerText;

	internal new XElement Value => (XElement)base.Value;

	public override NavNclType NclType => NavNclType.NavXmlElement;

	private bool HasAttributes => Value.HasAttributes;

	private bool HasElements => Value.HasElements;

	private bool IsEmpty => Value.IsEmpty;

	private string Name
	{
		get
		{
			if (string.IsNullOrEmpty(Value.Name.Namespace.NamespaceName))
			{
				return LocalName;
			}
			return Value.GetPrefixOfNamespace(Value.Name.Namespace) + ":" + Value.Name.LocalName;
		}
	}

	private string LocalName => Value.Name.LocalName;

	private string NamespaceUri => Value.Name.NamespaceName;

	private string InnerXml
	{
		get
		{
			XmlReader xmlReader = Value.CreateReader();
			xmlReader.MoveToContent();
			return xmlReader.ReadInnerXml();
		}
	}

	private string InnerText => Value.Value;

	public static NavXmlElement ALCreate(string name)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() => new NavXmlElement(new XElement(name)));
	}

	public static NavXmlElement ALCreate(string name, params object[] content)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() =>
		{
			object[] content2 = XmlNodeMapper.UnwrapXmlNodes(content);
			return new NavXmlElement(new XElement(name, content2));
		});
	}

	public static NavXmlElement ALCreate(string localName, string namespaceUri)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() => new NavXmlElement(new XElement(XName.Get(localName, namespaceUri))));
	}

	public static NavXmlElement ALCreate(string localName, string namespaceUri, params object[] content)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() =>
		{
			object[] content2 = XmlNodeMapper.UnwrapXmlNodes(content);
			return new NavXmlElement(new XElement(XName.Get(localName, namespaceUri), content2));
		});
	}

	public bool ALGetNamespaceOfPrefix(DataError errorLevel, string prefix, ByRef<NavText> result)
	{
		return GetNamespaceOfPrefix(errorLevel, prefix, result);
	}

	public bool ALGetPrefixOfNamespace(DataError errorLevel, string ns, ByRef<NavText> result)
	{
		return GetPrefixOfNamespace(errorLevel, ns, result);
	}

	public void ALRemoveAllAttributes()
	{
		RemoveAllAttributes();
	}

	public void ALRemoveAttribute(string localName)
	{
		RemoveAttribute(localName);
	}

	public void ALRemoveAttribute(string localName, string namespaceUri)
	{
		RemoveAttribute(localName, namespaceUri);
	}

	public void ALRemoveAttribute(NavXmlAttribute attribute)
	{
		RemoveAttribute(attribute);
	}

	public void ALSetAttribute(string name, string value)
	{
		SetAttribute(name, value);
	}

	public void ALSetAttribute(string name, string namespaceUri, string value)
	{
		SetAttribute(name, namespaceUri, value);
	}

	public NavXmlAttributeCollection ALAttributes()
	{
		return Attributes();
	}

	internal NavXmlElement(XElement element)
		: base((object)element)
	{
		attributeCollection = new Lazy<NavXmlAttributeCollection>(() => new NavXmlAttributeCollection(element));
	}

	private bool GetNamespaceOfPrefix(DataError errorLevel, string prefix, ByRef<NavText> result)
	{
		XNamespace namespaceOfPrefix = Value.GetNamespaceOfPrefix(prefix);
		result.Value = new NavText(result.Value.MaxLength, namespaceOfPrefix.NamespaceName);
		return true;
	}

	private bool GetPrefixOfNamespace(DataError errorLevel, string ns, ByRef<NavText> result)
	{
		string text = Value.GetPrefixOfNamespace(ns) ?? string.Empty;
		result.Value = new NavText(result.Value.MaxLength, text);
		return true;
	}

	private void RemoveAllAttributes()
	{
		attributeCollection.Value.ALRemoveAll();
	}

	private void RemoveAttribute(string localName)
	{
		attributeCollection.Value.ALRemove(localName);
	}

	private void RemoveAttribute(string localName, string namespaceUri)
	{
		attributeCollection.Value.ALRemove(localName, namespaceUri);
	}

	private void RemoveAttribute(NavXmlAttribute attribute)
	{
		attributeCollection.Value.ALRemove(attribute);
	}

	private void SetAttribute(string name, string value)
	{
		attributeCollection.Value.ALSet(name, value);
	}

	private void SetAttribute(string name, string namespaceUri, string value)
	{
		attributeCollection.Value.ALSet(name, namespaceUri, value);
	}

	private NavXmlAttributeCollection Attributes()
	{
		return attributeCollection.Value;
	}
}
