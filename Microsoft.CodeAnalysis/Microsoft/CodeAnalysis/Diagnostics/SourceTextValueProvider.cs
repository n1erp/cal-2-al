using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

public sealed class SourceTextValueProvider<TValue>
{
	internal AnalysisValueProvider<SourceText, TValue> CoreValueProvider { get; private set; }

	public SourceTextValueProvider(Func<SourceText, TValue> computeValue, IEqualityComparer<SourceText> sourceTextComparer = null)
	{
		CoreValueProvider = new AnalysisValueProvider<SourceText, TValue>(computeValue, sourceTextComparer ?? SourceTextComparer.Instance);
	}
}
