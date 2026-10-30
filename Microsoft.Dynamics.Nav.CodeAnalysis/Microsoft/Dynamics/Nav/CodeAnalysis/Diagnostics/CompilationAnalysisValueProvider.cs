using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class CompilationAnalysisValueProvider<TKey, TValue> where TKey : class
{
	private readonly AnalysisValueProvider<TKey, TValue> analysisValueProvider;

	private readonly Dictionary<TKey, TValue> valueMap;

	public CompilationAnalysisValueProvider(AnalysisValueProvider<TKey, TValue> analysisValueProvider)
	{
		this.analysisValueProvider = analysisValueProvider;
		valueMap = new Dictionary<TKey, TValue>(analysisValueProvider.KeyComparer);
	}

	internal bool TryGetValue(TKey key, out TValue value)
	{
		lock (valueMap)
		{
			if (valueMap.TryGetValue(key, out value))
			{
				return true;
			}
		}
		if (!analysisValueProvider.TryGetValue(key, out value))
		{
			value = default;
			return false;
		}
		lock (valueMap)
		{
			if (valueMap.TryGetValue(key, out var value2))
			{
				value = value2;
			}
			else
			{
				valueMap.Add(key, value);
			}
		}
		return true;
	}
}
