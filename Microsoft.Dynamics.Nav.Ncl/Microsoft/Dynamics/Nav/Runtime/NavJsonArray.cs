using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavJsonArray : NavJsonToken, IEnumerable<NavJsonToken>, IEnumerable
{
	public int ALCount => Count;

	public new static NavJsonArray Default => Create();

	private int Count => Value.Count;

	private JArray Value => (JArray)BackingToken;

	public override NavNclType NclType => NavNclType.NavJsonArray;

	public void ALAdd(NavJsonToken value)
	{
		Add(value);
	}

	public int ALIndexOf(NavJsonToken value)
	{
		return IndexOf(value);
	}

	public void ALRemoveAll()
	{
		RemoveAll();
	}

	public bool ALGet(DataError errorLevel, int index, ByRef<NavJsonToken> result)
	{
		return Get(errorLevel, index, result);
	}

	public bool ALSet(DataError errorLevel, int index, NavJsonToken value)
	{
		return Set(errorLevel, index, value);
	}

	public bool ALRemoveAt(DataError errorLevel, int index)
	{
		return RemoveAt(errorLevel, index);
	}

	public bool ALInsert(DataError errorLevel, int index, NavJsonToken value)
	{
		return Insert(errorLevel, index, value);
	}

	private NavJsonArray(JArray token)
		: base(token)
	{
	}

	IEnumerator<NavJsonToken> IEnumerable<NavJsonToken>.GetEnumerator()
	{
		List<JToken> list = Value.AsJEnumerable().ToList();
		foreach (JToken item in list)
		{
			yield return NavJsonToken.Create(item);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<NavJsonToken>)this).GetEnumerator();
	}

	internal static NavJsonArray Create(JArray array)
	{
		return new NavJsonArray(array);
	}

	public new static NavJsonArray CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavJsonArray result)
		{
			return result;
		}
		if (value is JArray array)
		{
			return Create(array);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavJsonArray), value);
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

	private static NavJsonArray Create()
	{
		return new NavJsonArray(new JArray());
	}

	private void Add(NavJsonToken value)
	{
		Value.Add(value.BackingToken);
	}

	private int IndexOf(NavJsonToken value)
	{
		return Value.IndexOf(value.BackingToken);
	}

	private void RemoveAll()
	{
		Value.Clear();
	}

	private bool Get(DataError errorLevel, int index, ByRef<NavJsonToken> result)
	{
		if (!IsIndexValid(index))
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLIndexOutOfBoundsException(index.ToString(CultureInfo.InvariantCulture));
			}
			return false;
		}
		JToken token = Value[index];
		result.Value = NavJsonToken.Create(token);
		return true;
	}

	private bool Set(DataError errorLevel, int index, NavJsonToken value)
	{
		if (!IsIndexValid(index))
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLIndexOutOfBoundsException(index.ToString(CultureInfo.InvariantCulture));
			}
			return false;
		}
		Value[index] = value.BackingToken;
		return true;
	}

	private bool RemoveAt(DataError errorLevel, int index)
	{
		if (!IsIndexValid(index))
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLIndexOutOfBoundsException(index.ToString(CultureInfo.InvariantCulture));
			}
			return false;
		}
		Value.RemoveAt(index);
		return true;
	}

	private bool Insert(DataError errorLevel, int index, NavJsonToken value)
	{
		if (!IsIndexValid(index) && index != Count)
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLIndexOutOfBoundsException(index.ToString(CultureInfo.InvariantCulture));
			}
			return false;
		}
		Value.Insert(index, value.BackingToken);
		return true;
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
