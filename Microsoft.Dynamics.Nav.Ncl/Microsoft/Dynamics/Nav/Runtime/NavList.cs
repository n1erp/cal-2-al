using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavList<T> : NavValue, IEnumerable<T>, IEnumerable
{
	private static class TrappableListOperationExecutor
	{
		public static bool ExecuteOperation(DataError errorLevel, Action operation)
		{
			return TrappableOperationExecutor.Execute(errorLevel, operation, MapNativeException);
		}

		public static TResult ExecuteOperation<TResult>(Func<TResult> factory)
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
			return exception;
		}
	}

	public static NavList<T> Default => new NavList<T>();

	public int ALCount => Count;

	private int Count => Value.Count;

	internal List<T> Value { get; }

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => false;

	public override NavNclType NclType => NavNclType.NavList;

	public void ALAdd(T value)
	{
		Add(value);
	}

	public void ALAddRange(params T[] values)
	{
		AddRange(values);
	}

	public void ALAddRange(NavList<T> values)
	{
		AddRange(values);
	}

	public bool ALGet(DataError errorLevel, int index, ByRef<T> result)
	{
		return Get(errorLevel, index, result);
	}

	public T ALGet(int index)
	{
		return Get(index);
	}

	public bool ALSet(DataError errorLevel, int index, T newValue)
	{
		return Set(errorLevel, index, newValue);
	}

	public bool ALSet(DataError errorLevel, int index, T newValue, ByRef<T> oldValue)
	{
		return Set(errorLevel, index, newValue, oldValue);
	}

	public bool ALContains(T value)
	{
		return Contains(value);
	}

	public int ALIndexOf(T value)
	{
		return IndexOf(value);
	}

	public bool ALInsert(DataError errorLevel, int alIndex, T value)
	{
		return Insert(errorLevel, alIndex, value);
	}

	public int ALLastIndexOf(T value)
	{
		return LastIndexOf(value);
	}

	public bool ALRemove(T value)
	{
		return Remove(value);
	}

	public bool ALRemoveAt(DataError errorLevel, int alIndex)
	{
		return RemoveAt(errorLevel, alIndex);
	}

	public bool ALRemoveRange(DataError errorLevel, int alIndex, int count)
	{
		return RemoveRange(errorLevel, alIndex, count);
	}

	public NavList<T> ALGetRange(int alIndex, int count)
	{
		return GetRange(alIndex, count);
	}

	public bool ALGetRange(DataError errorLevel, int alIndex, int count, ByRef<NavList<T>> result)
	{
		return GetRange(errorLevel, alIndex, count, result);
	}

	public void ALReverse()
	{
		Reverse();
	}

	internal NavList()
		: this((IEnumerable<T>)new T[0])
	{
	}

	internal NavList(IEnumerable<T> values)
	{
		Value = new List<T>(values);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return ((IEnumerable<T>)Value).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<T>)Value).GetEnumerator();
	}

	private static int ConvertToInternalIndex(int index)
	{
		return --index;
	}

	private static int ConvertToExternalIndex(int index)
	{
		return ++index;
	}

	private void Add(T value)
	{
		Value.Add(value);
	}

	private void AddRange(IEnumerable<T> values)
	{
		if (values != null)
		{
			Value.AddRange(values);
		}
	}

	private bool Get(DataError errorLevel, int alIndex, ByRef<T> result)
	{
		return TrappableListOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			int index = ConvertToInternalIndex(alIndex);
			result.Value = Value[index];
		});
	}

	private T Get(int alIndex)
	{
		return TrappableListOperationExecutor.ExecuteOperation(() =>
		{
			int index = ConvertToInternalIndex(alIndex);
			return Value[index];
		});
	}

	private bool Set(DataError errorLevel, int alIndex, T newValue)
	{
		return TrappableListOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			int index = ConvertToInternalIndex(alIndex);
			Value[index] = newValue;
		});
	}

	private bool Set(DataError errorLevel, int alIndex, T newValue, ByRef<T> oldValue)
	{
		return TrappableListOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			int index = ConvertToInternalIndex(alIndex);
			oldValue.Value = Value[index];
			Value[index] = newValue;
		});
	}

	private bool Contains(T value)
	{
		return Value.Contains(value);
	}

	private int IndexOf(T value)
	{
		int index = Value.IndexOf(value);
		return ConvertToExternalIndex(index);
	}

	private bool Insert(DataError errorLevel, int alIndex, T value)
	{
		return TrappableListOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			int index = ConvertToInternalIndex(alIndex);
			Value.Insert(index, value);
		});
	}

	private int LastIndexOf(T value)
	{
		int index = Value.LastIndexOf(value);
		return ConvertToExternalIndex(index);
	}

	private bool Remove(T value)
	{
		return Value.Remove(value);
	}

	private bool RemoveAt(DataError errorLevel, int alIndex)
	{
		return TrappableListOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			int index = ConvertToInternalIndex(alIndex);
			Value.RemoveAt(index);
		});
	}

	private bool RemoveRange(DataError errorLevel, int alIndex, int count)
	{
		return TrappableListOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			int index = ConvertToInternalIndex(alIndex);
			Value.RemoveRange(index, count);
		});
	}

	private NavList<T> GetRange(int alIndex, int count)
	{
		return TrappableListOperationExecutor.ExecuteOperation(() =>
		{
			int index = ConvertToInternalIndex(alIndex);
			List<T> range = Value.GetRange(index, count);
			return new NavList<T>(range);
		});
	}

	private bool GetRange(DataError errorLevel, int alIndex, int count, ByRef<NavList<T>> result)
	{
		return TrappableListOperationExecutor.ExecuteOperation(errorLevel, () =>
		{
			int index = ConvertToInternalIndex(alIndex);
			List<T> range = Value.GetRange(index, count);
			result.Value = new NavList<T>(range);
		});
	}

	private void Reverse()
	{
		Value.Reverse();
	}
}
