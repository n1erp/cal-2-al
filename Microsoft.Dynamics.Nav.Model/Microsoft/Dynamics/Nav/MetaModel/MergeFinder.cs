using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.MetaModel;

internal static class MergeFinder
{
	public static IMergeChange[] Merge(IList<string> modified, IList<string> target, IEqualityComparer<string> comparer, MergeOptions mergeOptions)
	{
		List<IMergeChange> list = new List<IMergeChange>();
		IDiffChange[] array;
		using (DiffFinder<string> diffFinder = DiffFinder<string>.LcsDiff)
		{
			array = diffFinder.Diff(modified, target, comparer);
		}
		IDiffChange[] array2 = array;
		foreach (IDiffChange diffChange in array2)
		{
			MergeChange item = new MergeChange(MergeChangeType.Conflict, diffChange, diffChange);
			list.Add(item);
		}
		return list.ToArray();
	}

	public static IMergeChange[] Merge(IList<string> original, IList<string> modified, IList<string> target, IEqualityComparer<string> comparer, MergeOptions mergeOptions)
	{
		List<IMergeChange> list = new List<IMergeChange>();
		IDiffChange[] array;
		IDiffChange[] array2;
		using (DiffFinder<string> diffFinder = DiffFinder<string>.LcsDiff)
		{
			array = diffFinder.Diff(original, modified, comparer);
			array2 = diffFinder.Diff(original, target, comparer);
		}
		int num = 0;
		int num2 = 0;
		MergeChange mergeChange = null;
		while (num < array.Length && num2 < array2.Length)
		{
			IDiffChange diffChange = array[num];
			IDiffChange diffChange2 = array2[num2];
			int num3 = Math.Max(diffChange.OriginalStart + diffChange.OriginalLength - 1, diffChange.OriginalStart);
			int num4 = Math.Max(diffChange2.OriginalStart + diffChange2.OriginalLength - 1, diffChange2.OriginalStart);
			if (num3 < diffChange2.OriginalStart && (!mergeOptions.AdjacentChangesConflict || diffChange.ChangeType == DiffChangeType.Insert || num3 < diffChange2.OriginalStart - 1))
			{
				if (mergeChange == null)
				{
					mergeChange = new MergeChange(MergeChangeType.Modified);
				}
				mergeChange.AddModifiedChange(diffChange);
				list.Add(mergeChange);
				mergeChange = null;
				num++;
				continue;
			}
			if (num4 < diffChange.OriginalStart && (!mergeOptions.AdjacentChangesConflict || diffChange2.ChangeType == DiffChangeType.Insert || num4 < diffChange.OriginalStart - 1))
			{
				if (mergeChange == null)
				{
					mergeChange = new MergeChange(MergeChangeType.Target);
				}
				mergeChange.AddTargetChange(diffChange2);
				list.Add(mergeChange);
				mergeChange = null;
				num2++;
				continue;
			}
			if (ChangesIdentical(modified, diffChange, target, diffChange2, comparer))
			{
				list.Add(new MergeChange(MergeChangeType.Both, diffChange, diffChange2));
				num++;
				num2++;
				continue;
			}
			if (mergeChange == null)
			{
				mergeChange = new MergeChange(MergeChangeType.Conflict);
			}
			if (num3 < num4)
			{
				mergeChange.AddModifiedChange(diffChange);
				num++;
			}
			else
			{
				mergeChange.AddTargetChange(diffChange2);
				num2++;
			}
		}
		if (num < array.Length)
		{
			for (int i = num; i < array.Length; i++)
			{
				if (mergeChange != null && ((mergeChange.TargetChange.OriginalEnd >= array[i].OriginalStart && mergeOptions.AdjacentChangesConflict) || mergeChange.TargetChange.OriginalEnd > array[i].OriginalStart))
				{
					mergeChange.AddModifiedChange(array[i]);
					continue;
				}
				if (mergeChange != null)
				{
					list.Add(mergeChange);
					mergeChange = null;
				}
				list.Add(new MergeChange(MergeChangeType.Modified, array[i], null));
			}
		}
		else if (num2 < array2.Length)
		{
			for (int j = num2; j < array2.Length; j++)
			{
				if (mergeChange != null && ((mergeChange.ModifiedChange.OriginalEnd >= array2[j].OriginalStart && mergeOptions.AdjacentChangesConflict) || mergeChange.ModifiedChange.OriginalEnd > array2[j].OriginalStart))
				{
					mergeChange.AddTargetChange(array2[j]);
					continue;
				}
				if (mergeChange != null)
				{
					list.Add(mergeChange);
					mergeChange = null;
				}
				list.Add(new MergeChange(MergeChangeType.Target, null, array2[j]));
			}
		}
		if (mergeChange != null)
		{
			list.Add(mergeChange);
		}
		return list.ToArray();
	}

	private static bool ChangesIdentical(IList<string> leftSequence, IDiffChange leftChange, IList<string> rightSequence, IDiffChange rightChange, IEqualityComparer<string> comparer)
	{
		if (leftChange.OriginalStart != rightChange.OriginalStart || leftChange.OriginalLength != rightChange.OriginalLength || leftChange.ModifiedLength != rightChange.ModifiedLength)
		{
			return false;
		}
		int num = leftChange.ModifiedStart;
		int num2 = rightChange.ModifiedStart;
		while (num < leftChange.ModifiedStart + leftChange.ModifiedLength)
		{
			if (!comparer.Equals(leftSequence[num], rightSequence[num2]))
			{
				return false;
			}
			num++;
			num2++;
		}
		return true;
	}
}
