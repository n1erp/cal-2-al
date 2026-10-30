using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.MetaModel;

internal abstract class DiffFinder<T> : IDisposable
{
	private IList<T> originalSequence;

	private IList<T> modifiedSequence;

	private int[] originalIds;

	private int[] modifiedIds;

	private IEqualityComparer<T> elementComparer;

	private ContinueDifferencePredicate<T> continueDifferencePredicate;

	protected IList<T> OriginalSequence => originalSequence;

	protected IList<T> ModifiedSequence => modifiedSequence;

	protected IEqualityComparer<T> ElementComparer => elementComparer;

	public static DiffFinder<T> LcsDiff => new LcsDiff<T>();

	protected ContinueDifferencePredicate<T> ContinueDifferencePredicate => continueDifferencePredicate;

	public virtual void Dispose()
	{
		originalIds = null;
		modifiedIds = null;
		GC.SuppressFinalize(this);
	}

	protected bool ElementsAreEqual(int originalIndex, int modifiedIndex)
	{
		return ElementsAreEqual(originalIndex, firstIsOriginal: true, modifiedIndex, secondIsOriginal: false);
	}

	protected bool OriginalElementsAreEqual(int firstIndex, int secondIndex)
	{
		return ElementsAreEqual(firstIndex, firstIsOriginal: true, secondIndex, secondIsOriginal: true);
	}

	protected bool ModifiedElementsAreEqual(int firstIndex, int secondIndex)
	{
		return ElementsAreEqual(firstIndex, firstIsOriginal: false, secondIndex, secondIsOriginal: false);
	}

	private bool ElementsAreEqual(int firstIndex, bool firstIsOriginal, int secondIndex, bool secondIsOriginal)
	{
		int num = (firstIsOriginal ? originalIds[firstIndex] : modifiedIds[firstIndex]);
		int num2 = (secondIsOriginal ? originalIds[secondIndex] : modifiedIds[secondIndex]);
		if (num != 0 && num2 != 0)
		{
			return num == num2;
		}
		T x = (firstIsOriginal ? OriginalSequence[firstIndex] : ModifiedSequence[firstIndex]);
		T y = (secondIsOriginal ? OriginalSequence[secondIndex] : ModifiedSequence[secondIndex]);
		return ElementComparer.Equals(x, y);
	}

	private void ComputeUniqueIdentifiers(int originalStart, int originalEnd, int modifiedStart, int modifiedEnd)
	{
		Dictionary<T, int> dictionary = new Dictionary<T, int>(OriginalSequence.Count + ModifiedSequence.Count, ElementComparer);
		int num = 1;
		for (int i = originalStart; i <= originalEnd; i++)
		{
			T key = OriginalSequence[i];
			if (!dictionary.TryGetValue(key, out originalIds[i]))
			{
				originalIds[i] = num++;
				dictionary.Add(key, originalIds[i]);
			}
		}
		for (int j = modifiedStart; j <= modifiedEnd; j++)
		{
			T key2 = ModifiedSequence[j];
			if (!dictionary.TryGetValue(key2, out modifiedIds[j]))
			{
				modifiedIds[j] = num++;
				dictionary.Add(key2, modifiedIds[j]);
			}
		}
	}

	public IDiffChange[] Diff(IList<T> original, IList<T> modified, IEqualityComparer<T> comparer)
	{
		return Diff(original, modified, comparer, null);
	}

	public IDiffChange[] Diff(IList<T> original, IList<T> modified, IEqualityComparer<T> comparer, ContinueDifferencePredicate<T> predicate)
	{
		originalSequence = original;
		modifiedSequence = modified;
		elementComparer = comparer;
		continueDifferencePredicate = predicate;
		originalIds = new int[OriginalSequence.Count];
		modifiedIds = new int[ModifiedSequence.Count];
		int num = 0;
		int num2 = OriginalSequence.Count - 1;
		int num3 = 0;
		int num4 = ModifiedSequence.Count - 1;
		while (num <= num2 && num3 <= num4 && ElementsAreEqual(num, num3))
		{
			num++;
			num3++;
		}
		while (num2 >= num && num4 >= num3 && ElementsAreEqual(num2, num4))
		{
			num2--;
			num4--;
		}
		if (num > num2 || num3 > num4)
		{
			IDiffChange[] result = ((num3 <= num4) ? new IDiffChange[1]
			{
				new DiffChange(num, 0, num3, num4 - num3 + 1)
			} : ((num > num2) ? new IDiffChange[0] : new IDiffChange[1]
			{
				new DiffChange(num, num2 - num + 1, num3, 0)
			}));
			return result;
		}
		ComputeUniqueIdentifiers(num, num2, num3, num4);
		return ComputeDiff(num, num2, num3, num4);
	}

	protected abstract IDiffChange[] ComputeDiff(int originalStart, int originalEnd, int modifiedStart, int modifiedEnd);
}
