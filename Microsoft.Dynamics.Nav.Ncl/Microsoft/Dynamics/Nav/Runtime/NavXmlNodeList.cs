using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavXmlNodeList : NavValue, IXmlValueContainer<IList<NavXmlNode>>, IEnumerable<NavXmlNode>, IEnumerable
{
	public static NavXmlNodeList Default => new NavXmlNodeList(new List<NavXmlNode>());

	public int ALCount => Count;

	private int Count => Elements.Count;

	private IList<NavXmlNode> Elements { get; }

	public override int GetBytesSize { get; }

	public override bool IsZeroOrEmpty { get; }

	public override NavNclType NclType { get; } = NavNclType.NavXmlNodeList;

	bool IXmlValueContainer<IList<NavXmlNode>>.HasValue => Elements != null;

	IList<NavXmlNode> IXmlValueContainer<IList<NavXmlNode>>.Value => Elements;

	public bool ALGet(DataError errorLevel, int index, ByRef<NavXmlNode> node)
	{
		return Get(errorLevel, index, node);
	}

	internal NavXmlNodeList(IList<NavXmlNode> elements)
	{
		Elements = elements;
	}

	public static NavXmlNodeList CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavXmlNodeList result)
		{
			return result;
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavXmlNodeList), value);
	}

	private bool Get(DataError errorLevel, int index, ByRef<NavXmlNode> node)
	{
		if (!IsIndexValid(index - 1))
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLIndexOutOfBoundsException(index.ToString(CultureInfo.InvariantCulture));
			}
			return false;
		}
		node.Value = Elements[index - 1];
		return true;
	}

	public override int GetHashCode()
	{
		return Elements.GetHashCode();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<NavXmlNode>)this).GetEnumerator();
	}

	IEnumerator<NavXmlNode> IEnumerable<NavXmlNode>.GetEnumerator()
	{
		return Elements.GetEnumerator();
	}

	private bool IsIndexValid(int index)
	{
		if (0 <= index)
		{
			return index < Count;
		}
		return false;
	}
}
