using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class PooledNameSymbolDictionary : Dictionary<string, ISymbol>
{
	private readonly ObjectPool<PooledNameSymbolDictionary> pool;

	private static readonly ObjectPool<PooledNameSymbolDictionary> poolInstance = CreatePool();

	private PooledNameSymbolDictionary(ObjectPool<PooledNameSymbolDictionary> pool)
		: base((IEqualityComparer<string>)SemanticFacts.NameEqualityComparer)
	{
		this.pool = pool;
	}

	public ImmutableDictionary<string, ISymbol> ToImmutableDictionaryAndFree()
	{
		ImmutableDictionary<string, ISymbol> result = this.ToImmutableDictionary(SemanticFacts.NameEqualityComparer);
		Free();
		return result;
	}

	public void Free()
	{
		Clear();
		if (pool != null)
		{
			pool.Free(this);
		}
	}

	public static ObjectPool<PooledNameSymbolDictionary> CreatePool()
	{
		ObjectPool<PooledNameSymbolDictionary> pool = null;
		pool = new ObjectPool<PooledNameSymbolDictionary>(() => new PooledNameSymbolDictionary(pool), 128);
		return pool;
	}

	public static PooledNameSymbolDictionary GetInstance()
	{
		PooledNameSymbolDictionary pooledNameSymbolDictionary = poolInstance.Allocate();
		DebugAssertHelper.Assert(pooledNameSymbolDictionary.Count == 0);
		return pooledNameSymbolDictionary;
	}
}
