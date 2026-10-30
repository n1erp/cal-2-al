using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class LookupSymbolsInfo : AbstractLookupSymbolsInfo<Symbol>
{
	private const int PoolSize = 64;

	private static readonly ObjectPool<LookupSymbolsInfo> pool = new ObjectPool<LookupSymbolsInfo>(() => new LookupSymbolsInfo(), 64);

	private LookupSymbolsInfo()
		: base((IEqualityComparer<string>)StringComparer.Ordinal)
	{
	}

	public void Free()
	{
		Clear();
		pool.Free(this);
	}

	public static LookupSymbolsInfo GetInstance()
	{
		LookupSymbolsInfo lookupSymbolsInfo = pool.Allocate();
		DebugAssertHelper.Assert(lookupSymbolsInfo.Count == 0);
		return lookupSymbolsInfo;
	}
}
