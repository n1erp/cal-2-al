using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class SynthesizedLocalOrdinalsDispenser
{
	private PooledDictionary<long, int> _lazyMap;

	private static long MakeKey(SynthesizedLocalKind localKind, int syntaxOffset)
	{
		return ((long)syntaxOffset << 8) | (long)localKind;
	}

	public void Free()
	{
		if (_lazyMap != null)
		{
			_lazyMap.Free();
			_lazyMap = null;
		}
	}

	public int AssignLocalOrdinal(SynthesizedLocalKind localKind, int syntaxOffset)
	{
		if (localKind == SynthesizedLocalKind.UserDefined)
		{
			return 0;
		}
		long key = MakeKey(localKind, syntaxOffset);
		int value;
		if (_lazyMap == null)
		{
			_lazyMap = PooledDictionary<long, int>.GetInstance();
			value = 0;
		}
		else if (!_lazyMap.TryGetValue(key, out value))
		{
			value = 0;
		}
		_lazyMap[key] = value + 1;
		return value;
	}
}
