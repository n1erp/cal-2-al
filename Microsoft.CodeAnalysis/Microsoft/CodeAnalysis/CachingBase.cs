namespace Microsoft.CodeAnalysis;

internal abstract class CachingBase<TEntry>
{
	protected readonly int mask;

	protected readonly TEntry[] entries;

	internal CachingBase(int size)
	{
		int num = AlignSize(size);
		mask = num - 1;
		entries = new TEntry[num];
	}

	private static int AlignSize(int size)
	{
		size--;
		size |= size >> 1;
		size |= size >> 2;
		size |= size >> 4;
		size |= size >> 8;
		size |= size >> 16;
		return size + 1;
	}
}
