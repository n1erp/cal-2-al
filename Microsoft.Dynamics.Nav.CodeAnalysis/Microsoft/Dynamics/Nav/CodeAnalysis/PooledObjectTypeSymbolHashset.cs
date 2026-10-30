using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class PooledObjectTypeSymbolHashset : HashSet<IObjectTypeSymbol>
{
	private readonly ObjectPool<PooledObjectTypeSymbolHashset> pool;

	private static readonly ObjectPool<PooledObjectTypeSymbolHashset> poolInstance = CreatePool();

	private PooledObjectTypeSymbolHashset(ObjectPool<PooledObjectTypeSymbolHashset> pool)
		: base((IEqualityComparer<IObjectTypeSymbol>)SemanticFacts.ObjectTypeSymbolEqualityComparer)
	{
		this.pool = pool;
	}

	public void Free()
	{
		Clear();
		if (pool != null)
		{
			pool.Free(this);
		}
	}

	public static ObjectPool<PooledObjectTypeSymbolHashset> CreatePool()
	{
		ObjectPool<PooledObjectTypeSymbolHashset> pool = null;
		pool = new ObjectPool<PooledObjectTypeSymbolHashset>(() => new PooledObjectTypeSymbolHashset(pool), 128);
		return pool;
	}

	public static PooledObjectTypeSymbolHashset GetInstance()
	{
		PooledObjectTypeSymbolHashset pooledObjectTypeSymbolHashset = poolInstance.Allocate();
		DebugAssertHelper.Assert(pooledObjectTypeSymbolHashset.Count == 0);
		return pooledObjectTypeSymbolHashset;
	}
}
