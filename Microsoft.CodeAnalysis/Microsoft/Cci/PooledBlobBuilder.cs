using System;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.Cci;

internal sealed class PooledBlobBuilder : BlobBuilder, IDisposable
{
	private const int PoolSize = 128;

	private const int ChunkSize = 1024;

	private static ObjectPool<PooledBlobBuilder> s_chunkPool = new ObjectPool<PooledBlobBuilder>(() => new PooledBlobBuilder(1024), 128);

	private PooledBlobBuilder(int size)
		: base(size)
	{
	}

	public static PooledBlobBuilder GetInstance(int size = 1024)
	{
		return s_chunkPool.Allocate();
	}

	protected override BlobBuilder AllocateChunk(int minimalSize)
	{
		if (minimalSize <= 1024)
		{
			return s_chunkPool.Allocate();
		}
		return new BlobBuilder(minimalSize);
	}

	protected override void FreeChunk()
	{
		s_chunkPool.Free(this);
	}

	public new void Free()
	{
		base.Free();
	}

	void IDisposable.Dispose()
	{
		Free();
	}
}
