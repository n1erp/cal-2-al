using System;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Roslyn.Utilities;

internal class TextKeyedCache<T> where T : class
{
	private class SharedEntryValue
	{
		public readonly string Text;

		public readonly T Item;

		public SharedEntryValue(string Text, T item)
		{
			this.Text = Text;
			Item = item;
		}
	}

	private const int LocalSizeBits = 11;

	private const int LocalSize = 2048;

	private const int LocalSizeMask = 2047;

	private const int SharedSizeBits = 16;

	private const int SharedSize = 65536;

	private const int SharedSizeMask = 65535;

	private const int SharedBucketBits = 4;

	private const int SharedBucketSize = 16;

	private const int SharedBucketSizeMask = 15;

	private readonly (string Text, int HashCode, T Item)[] _localTable = new (string, int, T)[2048];

	private static readonly (int HashCode, SharedEntryValue Entry)[] s_sharedTable = new (int, SharedEntryValue)[65536];

	private readonly (int HashCode, SharedEntryValue Entry)[] _sharedTableInst = s_sharedTable;

	private readonly StringTable _strings;

	private Random _random;

	private readonly ObjectPool<TextKeyedCache<T>> _pool;

	private static readonly ObjectPool<TextKeyedCache<T>> s_staticPool = CreatePool();

	internal TextKeyedCache()
		: this((ObjectPool<TextKeyedCache<T>>)null)
	{
	}

	private TextKeyedCache(ObjectPool<TextKeyedCache<T>> pool)
	{
		_pool = pool;
		_strings = new StringTable();
	}

	private static ObjectPool<TextKeyedCache<T>> CreatePool()
	{
		ObjectPool<TextKeyedCache<T>> pool = null;
		pool = new ObjectPool<TextKeyedCache<T>>(() => new TextKeyedCache<T>(pool), Environment.ProcessorCount * 4);
		return pool;
	}

	public static TextKeyedCache<T> GetInstance()
	{
		return s_staticPool.Allocate();
	}

	public void Free()
	{
		_pool.Free(this);
	}

	internal T FindItem(char[] chars, int start, int len, int hashCode)
	{
		ref(string, int, T) reference = ref _localTable[LocalIdxFromHash(hashCode)];
		var (text, _, _) = reference;
		if (text != null && reference.Item2 == hashCode && StringTable.TextEquals(text, chars, start, len))
		{
			return reference.Item3;
		}
		SharedEntryValue sharedEntryValue = FindSharedEntry(chars, start, len, hashCode);
		if (sharedEntryValue != null)
		{
			reference.Item2 = hashCode;
			reference.Item1 = sharedEntryValue.Text;
			return reference.Item3 = sharedEntryValue.Item;
		}
		return null;
	}

	private SharedEntryValue FindSharedEntry(char[] chars, int start, int len, int hashCode)
	{
		(int, SharedEntryValue)[] sharedTableInst = _sharedTableInst;
		int num = SharedIdxFromHash(hashCode);
		SharedEntryValue sharedEntryValue = null;
		for (int i = 1; i < 17; i++)
		{
			int num2;
			(num2, sharedEntryValue) = sharedTableInst[num];
			if (sharedEntryValue == null || (num2 == hashCode && StringTable.TextEquals(sharedEntryValue.Text, chars, start, len)))
			{
				break;
			}
			sharedEntryValue = null;
			num = (num + i) & 0xFFFF;
		}
		return sharedEntryValue;
	}

	internal void AddItem(char[] chars, int start, int len, int hashCode, T item)
	{
		string text = _strings.Add(chars, start, len);
		SharedEntryValue e = new SharedEntryValue(text, item);
		AddSharedEntry(hashCode, e);
		ref(string Text, int HashCode, T Item) reference = ref _localTable[LocalIdxFromHash(hashCode)];
		reference.HashCode = hashCode;
		reference.Text = text;
		reference.Item = item;
	}

	private void AddSharedEntry(int hashCode, SharedEntryValue e)
	{
		(int, SharedEntryValue)[] sharedTableInst = _sharedTableInst;
		int num = SharedIdxFromHash(hashCode);
		int num2 = num;
		int num3 = 1;
		while (true)
		{
			if (num3 < 17)
			{
				if (sharedTableInst[num2].Item2 == null)
				{
					num = num2;
					break;
				}
				num2 = (num2 + num3) & 0xFFFF;
				num3++;
				continue;
			}
			int num4 = NextRandom() & 0xF;
			num = (num + (num4 * num4 + num4) / 2) & 0xFFFF;
			break;
		}
		sharedTableInst[num].Item1 = hashCode;
		Volatile.Write(ref sharedTableInst[num].Item2, e);
	}

	private static int LocalIdxFromHash(int hash)
	{
		return hash & 0x7FF;
	}

	private static int SharedIdxFromHash(int hash)
	{
		return (hash ^ (hash >> 11)) & 0xFFFF;
	}

	private int NextRandom()
	{
		return _random?.Next() ?? (_random = new Random()).Next();
	}
}
