using System;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Collections;

internal class PooledStringBuilder
{
	public readonly StringBuilder Builder = new StringBuilder();

	private readonly ObjectPool<PooledStringBuilder> pool;

	private static readonly ObjectPool<PooledStringBuilder> poolInstance = CreatePool();

	public int Length => Builder.Length;

	private PooledStringBuilder(ObjectPool<PooledStringBuilder> pool)
	{
		DebugAssertHelper.Assert(pool != null);
		this.pool = pool;
	}

	public void Free()
	{
		StringBuilder builder = Builder;
		if (builder.Capacity <= 1024)
		{
			builder.Clear();
			pool.Free(this);
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

	public static ObjectPool<PooledStringBuilder> CreatePool()
	{
		ObjectPool<PooledStringBuilder> pool = null;
		pool = new ObjectPool<PooledStringBuilder>(() => new PooledStringBuilder(pool), 32);
		return pool;
	}

	public static PooledStringBuilder GetInstance()
	{
		PooledStringBuilder pooledStringBuilder = poolInstance.Allocate();
		DebugAssertHelper.Assert(pooledStringBuilder.Builder.Length == 0);
		return pooledStringBuilder;
	}

	public static implicit operator StringBuilder(PooledStringBuilder obj)
	{
		return obj.Builder;
	}
}
