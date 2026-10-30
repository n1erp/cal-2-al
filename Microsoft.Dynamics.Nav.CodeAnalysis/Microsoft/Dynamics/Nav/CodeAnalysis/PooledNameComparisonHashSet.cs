using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class PooledNameComparisonHashSet : HashSet<string>
{
	private readonly ObjectPool<PooledNameComparisonHashSet> pool;

	private static readonly ObjectPool<PooledNameComparisonHashSet> poolInstance = CreatePool();

	private PooledNameComparisonHashSet(ObjectPool<PooledNameComparisonHashSet> pool)
		: base((IEqualityComparer<string>)SemanticFacts.NameEqualityComparer)
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

	public static ObjectPool<PooledNameComparisonHashSet> CreatePool()
	{
		ObjectPool<PooledNameComparisonHashSet> pool = null;
		pool = new ObjectPool<PooledNameComparisonHashSet>(() => new PooledNameComparisonHashSet(pool), 128);
		return pool;
	}

	public static PooledNameComparisonHashSet GetInstance()
	{
		PooledNameComparisonHashSet pooledNameComparisonHashSet = poolInstance.Allocate();
		DebugAssertHelper.Assert(pooledNameComparisonHashSet.Count == 0);
		return pooledNameComparisonHashSet;
	}
}
