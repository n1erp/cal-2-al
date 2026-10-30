using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Diagnostics;

public sealed class SyntaxTreeValueProvider<TValue>
{
	internal AnalysisValueProvider<SyntaxTree, TValue> CoreValueProvider { get; private set; }

	public SyntaxTreeValueProvider(Func<SyntaxTree, TValue> computeValue, IEqualityComparer<SyntaxTree> syntaxTreeComparer = null)
	{
		CoreValueProvider = new AnalysisValueProvider<SyntaxTree, TValue>(computeValue, syntaxTreeComparer ?? SyntaxTreeComparer.Instance);
	}
}
