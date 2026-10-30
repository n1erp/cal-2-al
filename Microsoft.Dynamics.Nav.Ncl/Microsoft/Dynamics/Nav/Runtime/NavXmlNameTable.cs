using System.Xml;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlNameTable : NavValue, IXmlValueContainer<XmlNameTable>
{
	public static NavXmlNameTable Default => new NavXmlNameTable();

	internal XmlNameTable Value { get; }

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => false;

	public override NavNclType NclType => NavNclType.NavXmlNameTable;

	XmlNameTable IXmlValueContainer<XmlNameTable>.Value => Value;

	bool IXmlValueContainer<XmlNameTable>.HasValue => Value != null;

	public string ALAdd(string key)
	{
		return Add(key);
	}

	public bool ALGet(DataError errorLevel, string key, ByRef<NavText> result)
	{
		return Get(errorLevel, key, result);
	}

	internal NavXmlNameTable()
	{
		Value = new NameTable();
	}

	internal NavXmlNameTable(XmlNameTable nameTable)
	{
		Value = nameTable;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public static NavXmlNameTable CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavXmlNameTable result)
		{
			return result;
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavXmlNameTable), value);
	}

	private string Add(string key)
	{
		return Value.Add(key);
	}

	private bool Get(DataError errorLevel, string key, ByRef<NavText> result)
	{
		string text = Value.Get(key);
		if (text == null)
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLKeyNotFoundException(key);
			}
			return false;
		}
		result.Value = new NavText(result.Value.MaxLength, text);
		return true;
	}
}
