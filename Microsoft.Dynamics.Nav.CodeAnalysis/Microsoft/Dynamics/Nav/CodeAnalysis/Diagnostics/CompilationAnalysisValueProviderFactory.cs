using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class CompilationAnalysisValueProviderFactory
{
	private Dictionary<object, object> lazySharedStateProviderMap;

	public CompilationAnalysisValueProvider<TKey, TValue> GetValueProvider<TKey, TValue>(AnalysisValueProvider<TKey, TValue> analysisSharedStateProvider) where TKey : class
	{
		if (lazySharedStateProviderMap == null)
		{
			Interlocked.CompareExchange(ref lazySharedStateProviderMap, new Dictionary<object, object>(), null);
		}
		object value;
		lock (lazySharedStateProviderMap)
		{
			if (!lazySharedStateProviderMap.TryGetValue(analysisSharedStateProvider, out value))
			{
				value = new CompilationAnalysisValueProvider<TKey, TValue>(analysisSharedStateProvider);
				lazySharedStateProviderMap[analysisSharedStateProvider] = value;
			}
		}
		return value as CompilationAnalysisValueProvider<TKey, TValue>;
	}
}
