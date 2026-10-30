using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.MetaModel;

public static class MergeOutput
{
	public static readonly string InlineOriginal = "{>>>>>>>} ORIGINAL";

	public static readonly string InlineModified = "{=======} MODIFIED";

	public static readonly string InlineTarget = "{=======} TARGET";

	public static readonly string InlineEnd = "{<<<<<<<}";

	private static void WriteElementRange(IList<string> sequence, int startIndex, int endIndex, IList<string> result)
	{
		for (int i = startIndex; i <= endIndex; i++)
		{
			result.Add(sequence[i]);
		}
	}

	internal static IList<string> Output(IList<string> original, IList<string> modified, IList<string> target, IMergeChange[] mergeList, MergeOutputMode mode = MergeOutputMode.Normal)
	{
		List<string> list = new List<string>();
		int num = 0;
		int startIndex = 0;
		foreach (IMergeChange mergeChange in mergeList)
		{
			switch (mergeChange.ChangeType)
			{
			case MergeChangeType.Modified:
			case MergeChangeType.Both:
			{
				int originalStart2 = mergeChange.ModifiedChange.OriginalStart;
				num = mergeChange.ModifiedChange.OriginalStart + mergeChange.ModifiedChange.OriginalLength - 1;
				WriteElementRange(original, startIndex, originalStart2 - 1, list);
				WriteElementRange(modified, mergeChange.ModifiedChange.ModifiedStart, mergeChange.ModifiedChange.ModifiedStart + mergeChange.ModifiedChange.ModifiedLength - 1, list);
				break;
			}
			case MergeChangeType.Target:
			{
				int originalStart = mergeChange.TargetChange.OriginalStart;
				num = mergeChange.TargetChange.OriginalStart + mergeChange.TargetChange.OriginalLength - 1;
				WriteElementRange(original, startIndex, originalStart - 1, list);
				WriteElementRange(target, mergeChange.TargetChange.ModifiedStart, mergeChange.TargetChange.ModifiedStart + mergeChange.TargetChange.ModifiedLength - 1, list);
				break;
			}
			case MergeChangeType.Conflict:
			{
				int num2 = Math.Min(mergeChange.ModifiedChange.OriginalStart, mergeChange.TargetChange.OriginalStart);
				num = Math.Max(mergeChange.ModifiedChange.OriginalStart + mergeChange.ModifiedChange.OriginalLength, mergeChange.TargetChange.OriginalStart + mergeChange.TargetChange.OriginalLength) - 1;
				WriteElementRange(original, startIndex, num2 - 1, list);
				switch (mode)
				{
				case MergeOutputMode.Normal:
					list.Add(InlineOriginal);
					WriteElementRange(original, num2, num, list);
					list.Add(InlineModified);
					WriteElementRange(original, num2, mergeChange.ModifiedChange.OriginalStart - 1, list);
					WriteElementRange(modified, mergeChange.ModifiedChange.ModifiedStart, mergeChange.ModifiedChange.ModifiedStart + mergeChange.ModifiedChange.ModifiedLength - 1, list);
					WriteElementRange(original, mergeChange.ModifiedChange.OriginalStart + mergeChange.ModifiedChange.OriginalLength, num, list);
					list.Add(InlineTarget);
					WriteElementRange(original, num2, mergeChange.TargetChange.OriginalStart - 1, list);
					WriteElementRange(target, mergeChange.TargetChange.ModifiedStart, mergeChange.TargetChange.ModifiedStart + mergeChange.TargetChange.ModifiedLength - 1, list);
					WriteElementRange(original, mergeChange.TargetChange.OriginalStart + mergeChange.TargetChange.OriginalLength, num, list);
					list.Add(InlineEnd);
					break;
				case MergeOutputMode.ModifiedFirst:
					WriteElementRange(original, num2, mergeChange.ModifiedChange.OriginalStart - 1, list);
					WriteElementRange(modified, mergeChange.ModifiedChange.ModifiedStart, mergeChange.ModifiedChange.ModifiedStart + mergeChange.ModifiedChange.ModifiedLength - 1, list);
					WriteElementRange(original, mergeChange.ModifiedChange.OriginalStart + mergeChange.ModifiedChange.OriginalLength, num, list);
					WriteElementRange(original, num2, mergeChange.TargetChange.OriginalStart - 1, list);
					WriteElementRange(target, mergeChange.TargetChange.ModifiedStart, mergeChange.TargetChange.ModifiedStart + mergeChange.TargetChange.ModifiedLength - 1, list);
					WriteElementRange(original, mergeChange.TargetChange.OriginalStart + mergeChange.TargetChange.OriginalLength, num, list);
					break;
				case MergeOutputMode.TargetFirst:
					WriteElementRange(original, num2, mergeChange.TargetChange.OriginalStart - 1, list);
					WriteElementRange(target, mergeChange.TargetChange.ModifiedStart, mergeChange.TargetChange.ModifiedStart + mergeChange.TargetChange.ModifiedLength - 1, list);
					WriteElementRange(original, mergeChange.TargetChange.OriginalStart + mergeChange.TargetChange.OriginalLength, num, list);
					WriteElementRange(original, num2, mergeChange.ModifiedChange.OriginalStart - 1, list);
					WriteElementRange(modified, mergeChange.ModifiedChange.ModifiedStart, mergeChange.ModifiedChange.ModifiedStart + mergeChange.ModifiedChange.ModifiedLength - 1, list);
					WriteElementRange(original, mergeChange.ModifiedChange.OriginalStart + mergeChange.ModifiedChange.OriginalLength, num, list);
					break;
				}
				break;
			}
			}
			startIndex = num + 1;
		}
		WriteElementRange(original, startIndex, original.Count - 1, list);
		return list;
	}
}
