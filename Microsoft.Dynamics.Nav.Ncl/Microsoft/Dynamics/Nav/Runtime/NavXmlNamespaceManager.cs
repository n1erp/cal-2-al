using System.Xml;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlNamespaceManager : NavValue, IXmlValueContainer<XmlNamespaceManager>
{
	public static NavXmlNamespaceManager Default => new NavXmlNamespaceManager();

	public NavXmlNameTable ALNameTable
	{
		get
		{
			return NameTable;
		}
		set
		{
			NameTable = value;
		}
	}

	internal XmlNamespaceManager Value { get; private set; }

	private NavXmlNameTable NameTable
	{
		get
		{
			return new NavXmlNameTable(Value.NameTable);
		}
		set
		{
			Value = new XmlNamespaceManager(value.Value);
		}
	}

	public override int GetBytesSize { get; }

	public override bool IsZeroOrEmpty { get; }

	public override NavNclType NclType { get; } = NavNclType.NavXmlNamespaceManager;

	bool IXmlValueContainer<XmlNamespaceManager>.HasValue => Value != null;

	XmlNamespaceManager IXmlValueContainer<XmlNamespaceManager>.Value => Value;

	public void ALAddNamespace(string prefix, string uri)
	{
		AddNamespace(prefix, uri);
	}

	public bool ALHasNamespace(string prefix)
	{
		return HasNamespace(prefix);
	}

	public bool ALLookupNamespace(DataError errorLevel, string prefix, ByRef<NavText> result)
	{
		return LookupNamespace(errorLevel, prefix, result);
	}

	public bool ALLookupPrefix(DataError errorLevel, string uri, ByRef<NavText> result)
	{
		return LookupPrefix(errorLevel, uri, result);
	}

	public void ALRemoveNamespace(string prefix, string uri)
	{
		RemoveNamespace(prefix, uri);
	}

	public void ALPushScope()
	{
		PushScope();
	}

	public bool ALPopScope()
	{
		return PopScope();
	}

	public static NavXmlNamespaceManager CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavXmlNamespaceManager result)
		{
			return result;
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavXmlNamespaceManager), value);
	}

	private NavXmlNamespaceManager()
	{
		Value = new XmlNamespaceManager(NavXmlNameTable.Default.Value);
	}

	private void AddNamespace(string prefix, string uri)
	{
		Value.AddNamespace(prefix, uri);
	}

	private bool HasNamespace(string prefix)
	{
		return Value.HasNamespace(prefix);
	}

	private bool LookupNamespace(DataError errorLevel, string prefix, ByRef<NavText> result)
	{
		string text = Value.LookupNamespace(prefix);
		if (text == null)
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLKeyNotFoundException(prefix);
			}
			return false;
		}
		result.Value = new NavText(result.Value.MaxLength, text);
		return true;
	}

	private bool LookupPrefix(DataError errorLevel, string uri, ByRef<NavText> result)
	{
		string text = Value.LookupPrefix(uri);
		if (text == null)
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLKeyNotFoundException(uri);
			}
			return false;
		}
		result.Value = new NavText(result.Value.MaxLength, text);
		return true;
	}

	private void RemoveNamespace(string prefix, string uri)
	{
		Value.RemoveNamespace(prefix, uri);
	}

	private void PushScope()
	{
		Value.PushScope();
	}

	private bool PopScope()
	{
		return Value.PopScope();
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
