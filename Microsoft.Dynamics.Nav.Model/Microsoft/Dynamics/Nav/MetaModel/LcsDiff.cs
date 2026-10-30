using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.MetaModel;

internal class LcsDiff<T> : DiffFinder<T>
{
	private List<int[]> forwardHistory;

	private List<int[]> reverseHistory;

	private const int MaxDifferencesHistory = 1447;

	public LcsDiff()
	{
		forwardHistory = new List<int[]>(1447);
		reverseHistory = new List<int[]>(1447);
	}

	public override void Dispose()
	{
		base.Dispose();
		forwardHistory = null;
		reverseHistory = null;
		GC.SuppressFinalize(this);
	}

	protected override IDiffChange[] ComputeDiff(int originalStart, int originalEnd, int modifiedStart, int modifiedEnd)
	{
		IDiffChange[] changes = ComputeDiffRecursive(originalStart, originalEnd, modifiedStart, modifiedEnd, out var _);
		return ShiftChanges(changes);
	}

	private IDiffChange[] ComputeDiffRecursive(int originalStart, int originalEnd, int modifiedStart, int modifiedEnd, out bool quitEarly)
	{
		quitEarly = false;
		while (originalStart <= originalEnd && modifiedStart <= modifiedEnd && ElementsAreEqual(originalStart, modifiedStart))
		{
			originalStart++;
			modifiedStart++;
		}
		while (originalEnd >= originalStart && modifiedEnd >= modifiedStart && ElementsAreEqual(originalEnd, modifiedEnd))
		{
			originalEnd--;
			modifiedEnd--;
		}
		if (originalStart > originalEnd || modifiedStart > modifiedEnd)
		{
			IDiffChange[] result = ((modifiedStart <= modifiedEnd) ? new IDiffChange[1]
			{
				new DiffChange(originalStart, 0, modifiedStart, modifiedEnd - modifiedStart + 1)
			} : ((originalStart > originalEnd) ? new IDiffChange[0] : new IDiffChange[1]
			{
				new DiffChange(originalStart, originalEnd - originalStart + 1, modifiedStart, 0)
			}));
			return result;
		}
		IDiffChange[] array = ComputeRecursionPoint(originalStart, originalEnd, modifiedStart, modifiedEnd, out var midOriginal, out var midModified, out quitEarly);
		if (array != null)
		{
			return array;
		}
		if (!quitEarly)
		{
			IDiffChange[] left = ComputeDiffRecursive(originalStart, midOriginal, modifiedStart, midModified, out quitEarly);
			IDiffChange[] right;
			if (!quitEarly)
			{
				right = ComputeDiffRecursive(midOriginal + 1, originalEnd, midModified + 1, modifiedEnd, out quitEarly);
			}
			else
			{
				IDiffChange[] array2 = new DiffChange[1]
				{
					new DiffChange(midOriginal + 1, originalEnd - (midOriginal + 1) + 1, midModified + 1, modifiedEnd - (midModified + 1) + 1)
				};
				right = array2;
			}
			return ConcatenateChanges(left, right);
		}
		return new DiffChange[1]
		{
			new DiffChange(originalStart, originalEnd - originalStart + 1, modifiedStart, modifiedEnd - modifiedStart + 1)
		};
	}

	private IDiffChange[] ComputeRecursionPoint(int originalStart, int originalEnd, int modifiedStart, int modifiedEnd, out int midOriginal, out int midModified, out bool quitEarly)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		originalStart--;
		modifiedStart--;
		midOriginal = 0;
		midModified = 0;
		forwardHistory.Clear();
		reverseHistory.Clear();
		int num5 = originalEnd - originalStart + (modifiedEnd - modifiedStart);
		int num6 = num5 + 1;
		int[] array = new int[num6];
		int[] array2 = new int[num6];
		int num7 = modifiedEnd - modifiedStart;
		int num8 = originalEnd - originalStart;
		int num9 = originalStart - modifiedStart;
		int num10 = originalEnd - modifiedEnd;
		bool flag = (num8 - num7) % 2 == 0;
		array[num7] = originalStart;
		array2[num8] = originalEnd;
		quitEarly = false;
		for (int i = 1; i <= num5 / 2 + 1; i++)
		{
			int num11 = 0;
			int num12 = 0;
			num = ClipDiagonalBound(num7 - i, i, num7, num6);
			num2 = ClipDiagonalBound(num7 + i, i, num7, num6);
			int num13 = num;
			int num16;
			int num14;
			int num15;
			while (num13 <= num2)
			{
				num14 = ((num13 != num && (num13 >= num2 || array[num13 - 1] >= array[num13 + 1])) ? (array[num13 - 1] + 1) : array[num13 + 1]);
				num15 = num14 - (num13 - num7) - num9;
				num16 = num14;
				while (num14 < originalEnd && num15 < modifiedEnd && ElementsAreEqual(num14 + 1, num15 + 1))
				{
					num14++;
					num15++;
				}
				array[num13] = num14;
				if (num14 + num15 > num11 + num12)
				{
					num11 = num14;
					num12 = num15;
				}
				if (flag || Math.Abs(num13 - num8) > i - 1 || num14 < array2[num13])
				{
					num13 += 2;
					continue;
				}
				goto IL_014d;
			}
			int num17 = (num11 - originalStart + (num12 - modifiedStart) - i) / 2;
			if (ContinueDifferencePredicate != null && !ContinueDifferencePredicate(num11, OriginalSequence, num17))
			{
				quitEarly = true;
				midOriginal = num11;
				midModified = num12;
				if (num17 > 0 && i <= 1448)
				{
					break;
				}
				originalStart++;
				modifiedStart++;
				return new DiffChange[1]
				{
					new DiffChange(originalStart, originalEnd - originalStart + 1, modifiedStart, modifiedEnd - modifiedStart + 1)
				};
			}
			num3 = ClipDiagonalBound(num8 - i, i, num8, num6);
			num4 = ClipDiagonalBound(num8 + i, i, num8, num6);
			int num18 = num3;
			int num19;
			while (num18 <= num4)
			{
				num14 = ((num18 != num3 && (num18 >= num4 || array2[num18 - 1] < array2[num18 + 1])) ? array2[num18 - 1] : (array2[num18 + 1] - 1));
				num15 = num14 - (num18 - num8) - num10;
				num19 = num14;
				while (num14 > originalStart && num15 > modifiedStart && ElementsAreEqual(num14, num15))
				{
					num14--;
					num15--;
				}
				array2[num18] = num14;
				if (!flag || Math.Abs(num18 - num7) > i || num14 > array[num18])
				{
					num18 += 2;
					continue;
				}
				goto IL_0299;
			}
			if (i <= 1447)
			{
				int[] array3 = new int[num2 - num + 2];
				array3[0] = num7 - num + 1;
				Array.Copy(array, num, array3, 1, num2 - num + 1);
				forwardHistory.Add(array3);
				array3 = new int[num4 - num3 + 2];
				array3[0] = num8 - num3 + 1;
				Array.Copy(array2, num3, array3, 1, num4 - num3 + 1);
				reverseHistory.Add(array3);
			}
			continue;
			IL_0299:
			midOriginal = num14;
			midModified = num15;
			if (num19 >= array[num18] && i <= 1448)
			{
				break;
			}
			return null;
			IL_014d:
			midOriginal = num14;
			midModified = num15;
			if (num16 <= array2[num13] && i <= 1448)
			{
				break;
			}
			return null;
		}
		IDiffChange[] reverseChanges;
		using (DiffChangeHelper diffChangeHelper = new DiffChangeHelper())
		{
			int num20 = num;
			int num21 = num2;
			int num22 = midOriginal - midModified - num9;
			int num23 = int.MinValue;
			int num24 = forwardHistory.Count - 1;
			do
			{
				int num25 = num22 + num7;
				if (num25 == num20 || (num25 < num21 && array[num25 - 1] < array[num25 + 1]))
				{
					int num14 = array[num25 + 1];
					int num15 = num14 - num22 - num9;
					if (num14 < num23)
					{
						diffChangeHelper.MarkNextChange();
					}
					num23 = num14;
					diffChangeHelper.AddModifiedElement(num14 + 1, num15);
					num22 = num25 + 1 - num7;
				}
				else
				{
					int num14 = array[num25 - 1] + 1;
					int num15 = num14 - num22 - num9;
					if (num14 < num23)
					{
						diffChangeHelper.MarkNextChange();
					}
					num23 = num14 - 1;
					diffChangeHelper.AddOriginalElement(num14, num15 + 1);
					num22 = num25 - 1 - num7;
				}
				if (num24 >= 0)
				{
					array = forwardHistory[num24];
					num7 = array[0];
					num20 = 1;
					num21 = array.Length - 1;
				}
			}
			while (--num24 >= -1);
			reverseChanges = diffChangeHelper.ReverseChanges;
		}
		IDiffChange[] right;
		if (quitEarly)
		{
			int num26 = midOriginal + 1;
			int num27 = midModified + 1;
			if (reverseChanges != null && reverseChanges.Length != 0)
			{
				IDiffChange diffChange = reverseChanges[reverseChanges.Length - 1];
				num26 = Math.Max(num26, diffChange.OriginalEnd);
				num27 = Math.Max(num27, diffChange.ModifiedEnd);
			}
			IDiffChange[] array4 = new DiffChange[1]
			{
				new DiffChange(num26, originalEnd - num26 + 1, num27, modifiedEnd - num27 + 1)
			};
			right = array4;
		}
		else
		{
			using DiffChangeHelper diffChangeHelper2 = new DiffChangeHelper();
			int num28 = num3;
			int num29 = num4;
			int num30 = midOriginal - midModified - num10;
			int num31 = int.MaxValue;
			int num32 = (flag ? (reverseHistory.Count - 1) : (reverseHistory.Count - 2));
			do
			{
				int num33 = num30 + num8;
				if (num33 == num28 || (num33 < num29 && array2[num33 - 1] >= array2[num33 + 1]))
				{
					int num14 = array2[num33 + 1] - 1;
					int num15 = num14 - num30 - num10;
					if (num14 > num31)
					{
						diffChangeHelper2.MarkNextChange();
					}
					num31 = num14 + 1;
					diffChangeHelper2.AddOriginalElement(num14 + 1, num15 + 1);
					num30 = num33 + 1 - num8;
				}
				else
				{
					int num14 = array2[num33 - 1];
					int num15 = num14 - num30 - num10;
					if (num14 > num31)
					{
						diffChangeHelper2.MarkNextChange();
					}
					num31 = num14;
					diffChangeHelper2.AddModifiedElement(num14 + 1, num15 + 1);
					num30 = num33 - 1 - num8;
				}
				if (num32 >= 0)
				{
					array2 = reverseHistory[num32];
					num8 = array2[0];
					num28 = 1;
					num29 = array2.Length - 1;
				}
			}
			while (--num32 >= -1);
			right = diffChangeHelper2.Changes;
		}
		return ConcatenateChanges(reverseChanges, right);
	}

	private IDiffChange[] ShiftChanges(IDiffChange[] changes)
	{
		for (int i = 0; i < changes.Length; i++)
		{
			DiffChange diffChange = changes[i] as DiffChange;
			int num = ((i < changes.Length - 1) ? changes[i + 1].OriginalStart : OriginalSequence.Count);
			int num2 = ((i < changes.Length - 1) ? changes[i + 1].ModifiedStart : ModifiedSequence.Count);
			bool flag = diffChange.OriginalLength > 0;
			bool flag2 = diffChange.ModifiedLength > 0;
			while (diffChange.OriginalStart + diffChange.OriginalLength < num && diffChange.ModifiedStart + diffChange.ModifiedLength < num2 && (!flag || OriginalElementsAreEqual(diffChange.OriginalStart, diffChange.OriginalStart + diffChange.OriginalLength)) && (!flag2 || ModifiedElementsAreEqual(diffChange.ModifiedStart, diffChange.ModifiedStart + diffChange.ModifiedLength)))
			{
				diffChange.OriginalStart++;
				diffChange.ModifiedStart++;
			}
		}
		List<IDiffChange> list = new List<IDiffChange>(changes.Length);
		for (int j = 0; j < changes.Length; j++)
		{
			if (j < changes.Length - 1 && ChangesOverlap(changes[j], changes[j + 1], out var mergedChange))
			{
				list.Add(mergedChange);
				j++;
			}
			else
			{
				list.Add(changes[j]);
			}
		}
		return list.ToArray();
	}

	private static IDiffChange[] ConcatenateChanges(IDiffChange[] left, IDiffChange[] right)
	{
		if (left.Length == 0 || right.Length == 0)
		{
			if (right.Length == 0)
			{
				return left;
			}
			return right;
		}
		if (ChangesOverlap(left[left.Length - 1], right[0], out var mergedChange))
		{
			IDiffChange[] array = new IDiffChange[left.Length + right.Length - 1];
			Array.Copy(left, 0, array, 0, left.Length - 1);
			array[left.Length - 1] = mergedChange;
			Array.Copy(right, 1, array, left.Length, right.Length - 1);
			return array;
		}
		IDiffChange[] array2 = new IDiffChange[left.Length + right.Length];
		Array.Copy(left, 0, array2, 0, left.Length);
		Array.Copy(right, 0, array2, left.Length, right.Length);
		return array2;
	}

	private static bool ChangesOverlap(IDiffChange left, IDiffChange right, out IDiffChange mergedChange)
	{
		if (left.OriginalStart + left.OriginalLength >= right.OriginalStart || left.ModifiedStart + left.ModifiedLength >= right.ModifiedStart)
		{
			int originalStart = left.OriginalStart;
			int originalLength = left.OriginalLength;
			int modifiedStart = left.ModifiedStart;
			int modifiedLength = left.ModifiedLength;
			if (left.OriginalStart + left.OriginalLength >= right.OriginalStart)
			{
				originalLength = right.OriginalStart + right.OriginalLength - left.OriginalStart;
			}
			if (left.ModifiedStart + left.ModifiedLength >= right.ModifiedStart)
			{
				modifiedLength = right.ModifiedStart + right.ModifiedLength - left.ModifiedStart;
			}
			mergedChange = new DiffChange(originalStart, originalLength, modifiedStart, modifiedLength);
			return true;
		}
		mergedChange = null;
		return false;
	}

	private static int ClipDiagonalBound(int diagonal, int numDifferences, int diagonalBaseIndex, int numDiagonals)
	{
		if (diagonal >= 0 && diagonal < numDiagonals)
		{
			return diagonal;
		}
		int num = numDiagonals - diagonalBaseIndex - 1;
		bool flag = numDifferences % 2 == 0;
		if (diagonal < 0)
		{
			bool flag2 = diagonalBaseIndex % 2 == 0;
			if (flag != flag2)
			{
				return 1;
			}
			return 0;
		}
		bool flag3 = num % 2 == 0;
		if (flag != flag3)
		{
			return numDiagonals - 2;
		}
		return numDiagonals - 1;
	}
}
