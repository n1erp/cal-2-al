using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal class AnalysisValueProvider<TKey, TValue> where TKey : class
{
	private sealed class WrappedValue
	{
		public TValue Value { get; set; }
	}

	private readonly Func<TKey, TValue> computeValue;

	private readonly ConditionalWeakTable<TKey, WrappedValue> valueCache;

	private readonly ConditionalWeakTable<TKey, WrappedValue>.CreateValueCallback valueCacheCallback;

	internal IEqualityComparer<TKey> KeyComparer { get; private set; }

	public AnalysisValueProvider(Func<TKey, TValue> computeValue, IEqualityComparer<TKey> keyComparer)
	{
		this.computeValue = computeValue;
		KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
		valueCache = new ConditionalWeakTable<TKey, WrappedValue>();
		valueCacheCallback = ComputeValue;
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	internal bool TryGetValue(TKey key, out TValue value)
	{
		try
		{
			value = valueCache.GetValue(key, valueCacheCallback).Value;
			return true;
		}
		catch (Exception)
		{
			value = default;
			return false;
		}
	}

	private WrappedValue ComputeValue(TKey key)
	{
		TValue value = computeValue(key);
		return new WrappedValue
		{
			Value = value
		};
	}
}
