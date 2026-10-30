using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace Roslyn.Utilities;

internal static class TextChangeRangeExtensions
{
	public static TextChangeRange? Accumulate(this TextChangeRange? accumulatedTextChangeSoFar, IEnumerable<TextChangeRange> changesInNextVersion)
	{
		if (!changesInNextVersion.Any())
		{
			return accumulatedTextChangeSoFar;
		}
		TextChangeRange value = TextChangeRange.Collapse(changesInNextVersion);
		if (!accumulatedTextChangeSoFar.HasValue)
		{
			return value;
		}
		int start = accumulatedTextChangeSoFar.Value.Span.Start;
		int num = accumulatedTextChangeSoFar.Value.Span.End;
		int num2 = accumulatedTextChangeSoFar.Value.Span.Start + accumulatedTextChangeSoFar.Value.NewLength;
		if (value.Span.Start < start)
		{
			start = value.Span.Start;
		}
		if (num2 > value.Span.End)
		{
			num2 = num2 + value.NewLength - value.Span.Length;
		}
		else
		{
			num = num + value.Span.End - num2;
			num2 = value.Span.Start + value.NewLength;
		}
		return new TextChangeRange(TextSpan.FromBounds(start, num), num2 - start);
	}
}
