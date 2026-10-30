using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class CompilationAnalysisValueProvider<TKey, TValue> where TKey : class
{
	private readonly AnalysisValueProvider<TKey, TValue> _analysisValueProvider;

	private readonly Dictionary<TKey, TValue> _valueMap;

	public CompilationAnalysisValueProvider(AnalysisValueProvider<TKey, TValue> analysisValueProvider)
	{
		_analysisValueProvider = analysisValueProvider;
		_valueMap = new Dictionary<TKey, TValue>(analysisValueProvider.KeyComparer);
	}

	internal bool TryGetValue(TKey key, out TValue value)
	{
		lock (_valueMap)
		{
			if (_valueMap.TryGetValue(key, out value))
			{
				return true;
			}
		}
		if (!_analysisValueProvider.TryGetValue(key, out value))
		{
			value = default;
			return false;
		}
		lock (_valueMap)
		{
			if (_valueMap.TryGetValue(key, out var value2))
			{
				value = value2;
			}
			else
			{
				_valueMap.Add(key, value);
			}
		}
		return true;
	}
}
