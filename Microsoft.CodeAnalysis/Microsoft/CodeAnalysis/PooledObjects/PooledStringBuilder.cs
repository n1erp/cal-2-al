using System;
using System.Text;

namespace Microsoft.CodeAnalysis.PooledObjects;

internal class PooledStringBuilder
{
	public readonly StringBuilder Builder = new StringBuilder();

	private readonly ObjectPool<PooledStringBuilder> _pool;

	private static readonly ObjectPool<PooledStringBuilder> s_poolInstance = CreatePool();

	public int Length => Builder.Length;

	private PooledStringBuilder(ObjectPool<PooledStringBuilder> pool)
	{
		_pool = pool;
	}

	public void Free()
	{
		StringBuilder builder = Builder;
		if (builder.Capacity <= 1024)
		{
			builder.Clear();
			_pool.Free(this);
		}
	}

	[Obsolete("Consider calling ToStringAndFree instead.")]
	public new string ToString()
	{
		return Builder.ToString();
	}

	public string ToStringAndFree()
	{
		string result = Builder.ToString();
		Free();
		return result;
	}

	public string ToStringAndFree(int startIndex, int length)
	{
		string result = Builder.ToString(startIndex, length);
		Free();
		return result;
	}

	public static ObjectPool<PooledStringBuilder> CreatePool(int size = 32)
	{
		ObjectPool<PooledStringBuilder> pool = null;
		pool = new ObjectPool<PooledStringBuilder>(() => new PooledStringBuilder(pool), size);
		return pool;
	}

	public static PooledStringBuilder GetInstance()
	{
		return s_poolInstance.Allocate();
	}

	public static implicit operator StringBuilder(PooledStringBuilder obj)
	{
		return obj.Builder;
	}
}
