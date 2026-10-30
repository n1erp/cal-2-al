using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Mono.Cecil;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

internal class PooledTypeReferenceSet : HashSet<TypeReference>
{
	private class TypeReferenceEqualityComparer : IEqualityComparer<TypeReference>
	{
		public static IEqualityComparer<TypeReference> Instance { get; } = new TypeReferenceEqualityComparer();

		private TypeReferenceEqualityComparer()
		{
		}

		public bool Equals(TypeReference x, TypeReference y)
		{
			return string.Equals(x.FullName, y.FullName);
		}

		public int GetHashCode(TypeReference obj)
		{
			return obj.FullName.GetHashCode();
		}
	}

	private static readonly ObjectPool<PooledTypeReferenceSet> poolInstance = CreatePool();

	private readonly ObjectPool<PooledTypeReferenceSet> pool;

	private PooledTypeReferenceSet(ObjectPool<PooledTypeReferenceSet> pool)
		: base(TypeReferenceEqualityComparer.Instance)
	{
		this.pool = pool;
	}

	public static ObjectPool<PooledTypeReferenceSet> CreatePool()
	{
		ObjectPool<PooledTypeReferenceSet> pool = null;
		pool = new ObjectPool<PooledTypeReferenceSet>(() => new PooledTypeReferenceSet(pool), 128);
		return pool;
	}

	public static PooledTypeReferenceSet GetInstance()
	{
		PooledTypeReferenceSet pooledTypeReferenceSet = poolInstance.Allocate();
		DebugAssertHelper.Assert(pooledTypeReferenceSet.Count == 0);
		return pooledTypeReferenceSet;
	}

	public void Free()
	{
		Clear();
		if (pool != null)
		{
			pool.Free(this);
		}
	}
}
