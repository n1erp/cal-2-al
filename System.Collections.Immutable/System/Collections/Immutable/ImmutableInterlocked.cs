using System.Collections.Generic;
using System.Threading;

namespace System.Collections.Immutable;

public static class ImmutableInterlocked
{
	public static bool Update<T>(ref T location, Func<T, T> transformer) where T : class
	{
		Requires.NotNull(transformer, "transformer");
		T val = Volatile.Read(ref location);
		bool flag;
		do
		{
			T val2 = transformer(val);
			if (val == val2)
			{
				return false;
			}
			T val3 = Interlocked.CompareExchange(ref location, val2, val);
			flag = val == val3;
			val = val3;
		}
		while (!flag);
		return true;
	}

	public static bool Update<T, TArg>(ref T location, Func<T, TArg, T> transformer, TArg transformerArgument) where T : class
	{
		Requires.NotNull(transformer, "transformer");
		T val = Volatile.Read(ref location);
		bool flag;
		do
		{
			T val2 = transformer(val, transformerArgument);
			if (val == val2)
			{
				return false;
			}
			T val3 = Interlocked.CompareExchange(ref location, val2, val);
			flag = val == val3;
			val = val3;
		}
		while (!flag);
		return true;
	}

	public static ImmutableArray<T> InterlockedExchange<T>(ref ImmutableArray<T> location, ImmutableArray<T> value)
	{
		return new ImmutableArray<T>(Interlocked.Exchange(ref location.array, value.array));
	}

	public static ImmutableArray<T> InterlockedCompareExchange<T>(ref ImmutableArray<T> location, ImmutableArray<T> value, ImmutableArray<T> comparand)
	{
		return new ImmutableArray<T>(Interlocked.CompareExchange(ref location.array, value.array, comparand.array));
	}

	public static bool InterlockedInitialize<T>(ref ImmutableArray<T> location, ImmutableArray<T> value)
	{
		return InterlockedCompareExchange(ref location, value, default).IsDefault;
	}

	public static TValue GetOrAdd<TKey, TValue, TArg>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
	{
		Requires.NotNull(valueFactory, "valueFactory");
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		Requires.NotNull(immutableDictionary, "location");
		if (immutableDictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		value = valueFactory(key, factoryArgument);
		return GetOrAdd(ref location, key, value);
	}

	public static TValue GetOrAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> valueFactory)
	{
		Requires.NotNull(valueFactory, "valueFactory");
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		Requires.NotNull(immutableDictionary, "location");
		if (immutableDictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		value = valueFactory(key);
		return GetOrAdd(ref location, key, value);
	}

	public static TValue GetOrAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue value)
	{
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableDictionary, "location");
			if (immutableDictionary.TryGetValue(key, out var value2))
			{
				return value2;
			}
			ImmutableDictionary<TKey, TValue> value3 = immutableDictionary.Add(key, value);
			ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value3, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return value;
	}

	public static TValue AddOrUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
	{
		Requires.NotNull(addValueFactory, "addValueFactory");
		Requires.NotNull(updateValueFactory, "updateValueFactory");
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		TValue val;
		bool flag;
		do
		{
			Requires.NotNull(immutableDictionary, "location");
			val = ((!immutableDictionary.TryGetValue(key, out var value)) ? addValueFactory(key) : updateValueFactory(key, value));
			ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.SetItem(key, val);
			ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return val;
	}

	public static TValue AddOrUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
	{
		Requires.NotNull(updateValueFactory, "updateValueFactory");
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		TValue val;
		bool flag;
		do
		{
			Requires.NotNull(immutableDictionary, "location");
			val = ((!immutableDictionary.TryGetValue(key, out var value)) ? addValue : updateValueFactory(key, value));
			ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.SetItem(key, val);
			ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return val;
	}

	public static bool TryAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue value)
	{
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableDictionary, "location");
			if (immutableDictionary.ContainsKey(key))
			{
				return false;
			}
			ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.Add(key, value);
			ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return true;
	}

	public static bool TryUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue newValue, TValue comparisonValue)
	{
		EqualityComparer<TValue> equalityComparer = EqualityComparer<TValue>.Default;
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableDictionary, "location");
			if (!immutableDictionary.TryGetValue(key, out var value) || !equalityComparer.Equals(value, comparisonValue))
			{
				return false;
			}
			ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.SetItem(key, newValue);
			ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return true;
	}

	public static bool TryRemove<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, out TValue value)
	{
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableDictionary, "location");
			if (!immutableDictionary.TryGetValue(key, out value))
			{
				return false;
			}
			ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.Remove(key);
			ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return true;
	}

	public static bool TryPop<T>(ref ImmutableStack<T> location, out T value)
	{
		ImmutableStack<T> immutableStack = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableStack, "location");
			if (immutableStack.IsEmpty)
			{
				value = default;
				return false;
			}
			ImmutableStack<T> value2 = immutableStack.Pop(out value);
			ImmutableStack<T> immutableStack2 = Interlocked.CompareExchange(ref location, value2, immutableStack);
			flag = immutableStack == immutableStack2;
			immutableStack = immutableStack2;
		}
		while (!flag);
		return true;
	}

	public static void Push<T>(ref ImmutableStack<T> location, T value)
	{
		ImmutableStack<T> immutableStack = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableStack, "location");
			ImmutableStack<T> value2 = immutableStack.Push(value);
			ImmutableStack<T> immutableStack2 = Interlocked.CompareExchange(ref location, value2, immutableStack);
			flag = immutableStack == immutableStack2;
			immutableStack = immutableStack2;
		}
		while (!flag);
	}

	public static bool TryDequeue<T>(ref ImmutableQueue<T> location, out T value)
	{
		ImmutableQueue<T> immutableQueue = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableQueue, "location");
			if (immutableQueue.IsEmpty)
			{
				value = default;
				return false;
			}
			ImmutableQueue<T> value2 = immutableQueue.Dequeue(out value);
			ImmutableQueue<T> immutableQueue2 = Interlocked.CompareExchange(ref location, value2, immutableQueue);
			flag = immutableQueue == immutableQueue2;
			immutableQueue = immutableQueue2;
		}
		while (!flag);
		return true;
	}

	public static void Enqueue<T>(ref ImmutableQueue<T> location, T value)
	{
		ImmutableQueue<T> immutableQueue = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableQueue, "location");
			ImmutableQueue<T> value2 = immutableQueue.Enqueue(value);
			ImmutableQueue<T> immutableQueue2 = Interlocked.CompareExchange(ref location, value2, immutableQueue);
			flag = immutableQueue == immutableQueue2;
			immutableQueue = immutableQueue2;
		}
		while (!flag);
	}
}
