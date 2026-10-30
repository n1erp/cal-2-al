using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class StringLookup<T>
{
	public class OrdinalCaseInsensiviveComparer : IComparer<char>, IComparer<string>, IComparer<KeyValuePair<string, T>>
	{
		internal OrdinalCaseInsensiviveComparer()
		{
		}

		public int Compare(char x, char y)
		{
			if (x == y)
			{
				return 0;
			}
			if (x >= 'A' && x <= 'Z')
			{
				x = (char)(x + 32);
			}
			if (y >= 'A' && y <= 'Z')
			{
				y = (char)(y + 32);
			}
			return x - y;
		}

		public int Compare(string x, string y)
		{
			if (x == null)
			{
				if (y == null)
				{
					return 0;
				}
				return -1;
			}
			if (y == null)
			{
				return 1;
			}
			int num = ((x.Length < y.Length) ? x.Length : y.Length);
			for (int i = 0; i < num; i++)
			{
				int num2 = Compare(x[i], y[i]);
				if (num2 != 0)
				{
					return num2;
				}
			}
			return x.Length - y.Length;
		}

		public int Compare(KeyValuePair<string, T> x, KeyValuePair<string, T> y)
		{
			return Compare(x.Key, y.Key);
		}
	}

	private KeyValuePair<string, T>[] pairs;

	private static OrdinalCaseInsensiviveComparer comparer = new OrdinalCaseInsensiviveComparer();

	public int Count => pairs.Length;

	public IEnumerable<KeyValuePair<string, T>> StringValuePairs => pairs;

	public IEnumerable<T> Values => pairs.Select((KeyValuePair<string, T> kvp) => kvp.Value);

	public static OrdinalCaseInsensiviveComparer Comparer => comparer;

	public StringLookup()
	{
		pairs = new KeyValuePair<string, T>[0];
	}

	public StringLookup(IEnumerable<T> source, Func<T, string> keySelector)
	{
		pairs = source.Select((T e) => new KeyValuePair<string, T>(keySelector(e), e)).OrderBy((KeyValuePair<string, T> kvp) => kvp.Key, Comparer).ToArray();
	}

	public StringLookup(IEnumerable<KeyValuePair<string, T>> source)
	{
		pairs = source.OrderBy((KeyValuePair<string, T> kvp) => kvp.Key, Comparer).ToArray();
	}

	public bool TryGetValue(string key, out T value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		int num = 0;
		int num2 = Count - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num >> 1);
			int num4 = Comparer.Compare(pairs[num3].Key, key);
			if (num4 == 0)
			{
				value = pairs[num3].Value;
				return true;
			}
			if (num4 < 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		value = default;
		return false;
	}

	public T GetValue(string key)
	{
		if (TryGetValue(key, out var value))
		{
			return value;
		}
		throw new InvalidOperationException($"Unable to find a {typeof(T).ToString()} from string {key}");
	}

	public bool TryGetValue(char[] key, int startIndex, int length, out T value)
	{
		string key2 = new string(key, startIndex, length);
		return TryGetValue(key2, out value);
	}
}
