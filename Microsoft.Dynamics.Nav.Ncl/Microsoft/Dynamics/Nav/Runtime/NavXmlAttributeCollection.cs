using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavXmlAttributeCollection : NavValue, IXmlValueContainer<IList<XAttribute>>, IEnumerable<NavXmlAttribute>, IEnumerable
{
	private IList<XAttribute> cachedAttributeList;

	private readonly XElement element;

	public static NavXmlAttributeCollection Default => new NavXmlAttributeCollection();

	public int ALCount => Count;

	private int Count => cachedAttributeList.Count;

	private XElement Element
	{
		get
		{
			if (element == null)
			{
				throw new NavNCLNotInitializedException(GetType());
			}
			return element;
		}
	}

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => false;

	public override NavNclType NclType => NavNclType.NavXmlAttributeCollection;

	IList<XAttribute> IXmlValueContainer<IList<XAttribute>>.Value => cachedAttributeList;

	bool IXmlValueContainer<IList<XAttribute>>.HasValue => Element != null;

	public bool ALGet(DataError errorLevel, int index, ByRef<NavXmlAttribute> result)
	{
		return Get(errorLevel, index, result);
	}

	public bool ALGet(DataError errorLevel, string name, ByRef<NavXmlAttribute> result)
	{
		return Get(errorLevel, name, result);
	}

	public bool ALGet(DataError errorLevel, string localName, string namespaceUri, ByRef<NavXmlAttribute> result)
	{
		return Get(errorLevel, localName, namespaceUri, result);
	}

	public void ALRemoveAll()
	{
		RemoveAll();
	}

	public void ALRemove(NavXmlAttribute attribute)
	{
		Remove(attribute);
	}

	public void ALRemove(string name)
	{
		Remove(name);
	}

	public void ALRemove(string localName, string namespaceUri)
	{
		Remove(localName, namespaceUri);
	}

	public void ALSet(string name, string value)
	{
		Set(name, value);
	}

	public void ALSet(string localName, string namespaceUri, string value)
	{
		Set(localName, namespaceUri, value);
	}

	internal NavXmlAttributeCollection()
	{
		cachedAttributeList = new List<XAttribute>();
	}

	internal NavXmlAttributeCollection(XElement element)
	{
		this.element = element;
		cachedAttributeList = Element.Attributes().ToList();
		Element.Changed += ElementUpdated;
	}

	public static NavXmlAttributeCollection CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavXmlAttributeCollection result)
		{
			return result;
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavXmlAttributeCollection), value);
	}

	private void ElementUpdated(object sender, XObjectChangeEventArgs e)
	{
		if (e.ObjectChange == XObjectChange.Add || e.ObjectChange == XObjectChange.Remove)
		{
			cachedAttributeList = Element.Attributes().ToList();
		}
	}

	private bool Get(DataError errorLevel, int index, ByRef<NavXmlAttribute> result)
	{
		if (!IsIndexValid(index - 1))
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLIndexOutOfBoundsException(index.ToString(CultureInfo.InvariantCulture));
			}
			return false;
		}
		result.Value = new NavXmlAttribute(cachedAttributeList[index - 1]);
		return true;
	}

	private bool Get(DataError errorLevel, string name, ByRef<NavXmlAttribute> result)
	{
		XAttribute xAttribute = Element.Attribute(name);
		if (xAttribute == null)
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLXmlNodeNotFoundException(name);
			}
			return false;
		}
		result.Value = new NavXmlAttribute(xAttribute);
		return true;
	}

	private bool Get(DataError errorLevel, string localName, string namespaceUri, ByRef<NavXmlAttribute> result)
	{
		XAttribute xAttribute = Element.Attribute(XName.Get(localName, namespaceUri));
		if (xAttribute == null)
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLXmlNodeNotFoundException(localName, namespaceUri);
			}
			return false;
		}
		result.Value = new NavXmlAttribute(xAttribute);
		return true;
	}

	private bool Append(DataError errorLevel, NavXmlAttribute attribute)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			Element.Add(attribute.Value);
		});
	}

	private bool Prepend(DataError errorLevel, NavXmlAttribute attribute)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			Element.AddFirst(attribute.Value);
		});
	}

	private void Set(string name, string value)
	{
		XAttribute xAttribute = Element.Attribute(name);
		if (xAttribute == null)
		{
			Element.Add(new XAttribute(name, value));
		}
		else
		{
			xAttribute.Value = value;
		}
	}

	private void Set(string localName, string namespaceUri, string value)
	{
		XAttribute xAttribute = Element.Attribute(XName.Get(localName, namespaceUri));
		if (xAttribute == null)
		{
			Element.Add(new XAttribute(XName.Get(localName, namespaceUri), value));
		}
		else
		{
			xAttribute.Value = value;
		}
	}

	private void RemoveAll()
	{
		Element.RemoveAttributes();
	}

	private void Remove(NavXmlAttribute attribute)
	{
		Element.SetAttributeValue(attribute.Value.Name, null);
	}

	private void Remove(string name)
	{
		Element.SetAttributeValue(name, null);
	}

	private void Remove(string localName, string namespaceUri)
	{
		Element.SetAttributeValue(XName.Get(localName, namespaceUri), null);
	}

	private bool IsIndexValid(int index)
	{
		if (0 <= index)
		{
			return index < Count;
		}
		return false;
	}

	~NavXmlAttributeCollection()
	{
		if (element != null)
		{
			element.Changed -= ElementUpdated;
		}
	}

	public override int GetHashCode()
	{
		return Element.GetHashCode();
	}

	IEnumerator<NavXmlAttribute> IEnumerable<NavXmlAttribute>.GetEnumerator()
	{
		IList<XAttribute> list = cachedAttributeList;
		foreach (XAttribute item in list)
		{
			yield return new NavXmlAttribute(item);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<NavXmlAttribute>)this).GetEnumerator();
	}
}
