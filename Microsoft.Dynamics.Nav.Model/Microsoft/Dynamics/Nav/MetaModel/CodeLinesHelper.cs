using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.MetaModel;

internal static class CodeLinesHelper
{
	internal static bool CompareLines(IEnumerable<string> left, IEnumerable<string> right, IEqualityComparer<string> comparer = null)
	{
		if (comparer == null)
		{
			comparer = DiffLineComparer.DefaultComparer;
		}
		IEnumerator<string> enumerator = left.GetEnumerator();
		IEnumerator<string> enumerator2 = right.GetEnumerator();
		bool flag;
		bool flag2;
		while (((flag = enumerator.MoveNext()) == (flag2 = enumerator2.MoveNext())) & flag)
		{
			if (!comparer.Equals(enumerator.Current, enumerator2.Current))
			{
				return false;
			}
		}
		if (!flag)
		{
			return !flag2;
		}
		return false;
	}

	private static bool TryGetCompressedRange(string s, out int low, out int high)
	{
		low = 0;
		high = 0;
		int num;
		if (!s.StartsWith("#") || (num = s.IndexOf("..", StringComparison.Ordinal)) <= 0)
		{
			return false;
		}
		if (int.TryParse(s.Substring(1, num - 1), out low) && int.TryParse(s.Substring(num + 2), out high))
		{
			return true;
		}
		return false;
	}

	private static int GetCompressedRangeLength(string s)
	{
		if (TryGetCompressedRange(s, out var low, out var high))
		{
			return high - low;
		}
		return 0;
	}

	private static IEnumerable<Tuple<int, string>> GetOriginalWithLineNumbers(IEnumerable<string> original)
	{
		int line = 1;
		foreach (string b in original)
		{
			yield return new Tuple<int, string>(line, b);
			line += GetCompressedRangeLength(b) + 1;
		}
	}

	internal static IEnumerable<string> ExpandCompressed(IEnumerable<string> compressed, IEnumerable<string> original)
	{
		List<Tuple<int, string>> originalList = GetOriginalWithLineNumbers(original).ToList();
		foreach (string item in compressed)
		{
			if (TryGetCompressedRange(item, out var low, out var high))
			{
				foreach (string item2 in from o in originalList
					where low <= o.Item1 && o.Item1 <= high
					select o.Item2)
				{
					yield return item2;
				}
			}
			else
			{
				yield return item;
			}
		}
	}

	internal static bool MergeCode(IList<string> original, IList<string> target, IList<string> modified, out IEnumerable<string> mergedCode, MergeOutputMode outputMode = MergeOutputMode.Normal)
	{
		IMergeChange[] array = MergeFinder.Merge(original, modified, target, DiffLineComparer.DefaultComparer, new MergeOptions());
		mergedCode = MergeOutput.Output(original, modified, target, array, outputMode);
		if (outputMode == MergeOutputMode.Normal)
		{
			return array.All((IMergeChange c) => c.ChangeType != MergeChangeType.Conflict);
		}
		return true;
	}
}
