using System;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavJsonObject : NavJsonToken
{
	public new static NavJsonObject Default => Create();

	public NavList<NavText> ALKeys => Keys;

	public NavList<NavJsonToken> ALValues => Values;

	private JObject Value => (JObject)BackingToken;

	private NavList<NavText> Keys => new NavList<NavText>(from p in Value.Properties()
		select NavText.Create(p.Name));

	private NavList<NavJsonToken> Values => new NavList<NavJsonToken>(from p in Value.Properties()
		select NavJsonToken.Create(p.Value));

	public override NavNclType NclType => NavNclType.NavJsonObject;

	public bool ALContains(string key)
	{
		return Contains(key);
	}

	public bool ALGet(DataError errorLevel, string key, ByRef<NavJsonToken> value)
	{
		return Get(errorLevel, key, value);
	}

	public bool ALAdd(DataError errorLevel, string key, NavJsonToken value)
	{
		return Add(errorLevel, key, value);
	}

	public bool ALRemove(string key)
	{
		return Remove(key);
	}

	public bool ALReplace(DataError errorLevel, string key, NavJsonToken newValue)
	{
		return ReplaceProperty(errorLevel, key, newValue);
	}

	public override NavJsonToken ALClone()
	{
		return base.ALClone();
	}

	public void ALRemoveAll()
	{
		RemoveAll();
	}

	private NavJsonObject(JObject value)
		: base(value)
	{
	}

	internal static NavJsonObject Create(JObject obj)
	{
		return new NavJsonObject(obj);
	}

	public new static NavJsonObject CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavJsonObject result)
		{
			return result;
		}
		if (value is JObject obj)
		{
			return Create(obj);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavJsonObject), value);
	}

	protected override bool ReadFrom(DataError errorLevel, string data)
	{
		return ReadFrom(errorLevel, data, verifyType: true);
	}

	protected override bool ReadFrom(DataError errorLevel, NavInStream data)
	{
		return ReadFrom(errorLevel, data, verifyType: true);
	}

	protected override void Clear()
	{
		SetBackingToken(Default.BackingToken, verifyType: true);
	}

	protected override NavJsonToken Clone()
	{
		JToken token = BackingToken.DeepClone();
		return NavJsonToken.Create(token);
	}

	private static NavJsonObject Create()
	{
		return Create(new JObject());
	}

	private bool Contains(string key)
	{
		return Value.GetValue(key, StringComparison.Ordinal) != null;
	}

	private bool Get(DataError errorLevel, string key, ByRef<NavJsonToken> value)
	{
		JToken value2 = Value.GetValue(key, StringComparison.Ordinal);
		if (value2 == null)
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLJsonPropertyNotFoundException(key);
			}
			return false;
		}
		value.Value = NavJsonToken.Create(value2);
		return true;
	}

	private bool Add(DataError errorLevel, string key, NavJsonToken value)
	{
		JToken value2 = Value.GetValue(key, StringComparison.Ordinal);
		if (value2 != null)
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNclDuplicateJsonPropertyException(key);
			}
			return false;
		}
		Value.Add(key, value.BackingToken);
		return true;
	}

	private void RemoveAll()
	{
		foreach (JProperty item in Value.Properties())
		{
			item.Value = null;
		}
		Value.RemoveAll();
	}

	private bool Remove(string key)
	{
		JProperty jProperty = Value.Property(key);
		if (jProperty != null)
		{
			jProperty.Value = null;
			jProperty.Remove();
			return true;
		}
		return false;
	}

	private bool ReplaceProperty(DataError errorLevel, string key, NavJsonToken newValue)
	{
		if (!Remove(key))
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLJsonPropertyNotFoundException(key);
			}
			return false;
		}
		Add(errorLevel, key, newValue);
		return true;
	}
}
