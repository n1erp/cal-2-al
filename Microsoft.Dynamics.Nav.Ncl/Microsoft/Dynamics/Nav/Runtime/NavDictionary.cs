using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDictionary<TKey, TValue> : NavValue
{
	private static class TrappableDictionaryOperationExecutor
	{
		public static bool ExecuteOperation(DataError errorLevel, Action operation)
		{
			return TrappableOperationExecutor.Execute(errorLevel, operation, MapNativeException);
		}

		public static T ExecuteOperation<T>(Func<T> factory)
		{
			return TrappableOperationExecutor.ExecuteFactory(factory, MapNativeException);
		}

		private static Exception MapNativeException(Exception exception)
		{
			if (exception is ArgumentOutOfRangeException)
			{
				return new NavNCLArgumentOutOfRangeException();
			}
			if (exception is ArgumentException)
			{
				return new NavNCLArgumentException();
			}
			if (exception is KeyNotFoundException)
			{
				return new NavNCLKeyNotFoundException();
			}
			return exception;
		}
	}

	private NavList<TKey> keys;

	private NavList<TValue> values;

	public static NavDictionary<TKey, TValue> Default => new NavDictionary<TKey, TValue>();

	public int ALCount => Count;

	public NavList<TKey> ALKeys => Keys;

	public NavList<TValue> ALValues => Values;

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => false;

	public override NavNclType NclType => NavNclType.NavDictionary;

	private int Count => Value.Count;

	internal IDictionary<TKey, TValue> Value { get; }

	private NavList<TKey> Keys
	{
		get
		{
			if (keys.Value == Value.Keys)
			{
				return keys;
			}
			Interlocked.Exchange(ref keys, new NavList<TKey>(Value.Keys));
			return keys;
		}
	}

	private NavList<TValue> Values
	{
		get
		{
			if (values.Value == Value.Values)
			{
				return values;
			}
			Interlocked.Exchange(ref values, new NavList<TValue>(Value.Values));
			return values;
		}
	}

	public bool ALGet(DataError errorLevel, TKey key, ByRef<TValue> result)
	{
		return Get(errorLevel, key, result);
	}

	public TValue ALGet(TKey key)
	{
		return Get(key);
	}

	public bool ALAdd(DataError errorLevel, TKey key, TValue value)
	{
		return Add(errorLevel, key, value);
	}

	public bool ALContainsKey(TKey key)
	{
		return ContainsKey(key);
	}

	public bool ALRemove(TKey key)
	{
		return Remove(key);
	}

	public void ALSet(TKey key, TValue value)
	{
		Set(key, value);
	}

	public bool ALSet(TKey key, TValue value, ByRef<TValue> oldValue)
	{
		return Set(key, value, oldValue);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	private NavDictionary()
	{
		Value = new Dictionary<TKey, TValue>();
		keys = new NavList<TKey>(Value.Keys);
		values = new NavList<TValue>(Value.Values);
	}

	private bool Get(DataError errorLevel, TKey key, ByRef<TValue> result)
	{
		return TrappableDictionaryOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			result.Value = Value[key];
		});
	}

	private TValue Get(TKey key)
	{
		return TrappableDictionaryOperationExecutor.ExecuteOperation(() => Value[key]);
	}

	private bool Add(DataError errorLevel, TKey key, TValue value)
	{
		return TrappableDictionaryOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			Value.Add(key, value);
		});
	}

	private bool ContainsKey(TKey key)
	{
		return TrappableDictionaryOperationExecutor.ExecuteOperation(() => Value.ContainsKey(key));
	}

	private bool Remove(TKey key)
	{
		return TrappableDictionaryOperationExecutor.ExecuteOperation(() => Value.Remove(key));
	}

	private void Set(TKey key, TValue value)
	{
		TrappableDictionaryOperationExecutor.ExecuteOperation(DataError.ThrowError, () =>
		{
			Value[key] = value;
		});
	}

	private bool Set(TKey key, TValue value, ByRef<TValue> oldValue)
	{
		return TrappableDictionaryOperationExecutor.ExecuteOperation(() =>
		{
			TValue value2 = default;
			bool result = false;
			if (Value.TryGetValue(key, out value2))
			{
				result = true;
				oldValue.Value = value2;
			}
			Value[key] = value;
			return result;
		});
	}

	public static NavDictionary<NavText, NavText> Clone(NavDictionary<NavText, NavText> sourceDictionary)
	{
		NavDictionary<NavText, NavText> navDictionary = NavDictionary<NavText, NavText>.Default;
		if (sourceDictionary != null)
		{
			foreach (KeyValuePair<NavText, NavText> item in sourceDictionary.Value)
			{
				navDictionary.ALAdd(DataError.ThrowError, new NavText(item.Key.MaxLength, item.Key.Value), new NavText(item.Value.MaxLength, item.Value.Value));
			}
		}
		return navDictionary;
	}
}
