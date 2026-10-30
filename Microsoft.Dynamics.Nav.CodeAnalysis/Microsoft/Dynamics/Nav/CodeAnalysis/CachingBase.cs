namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal abstract class CachingBase<TEntry>
{
	protected int Mask { get; set; }

	protected TEntry[] Entries { get; set; }

	internal CachingBase(int size)
	{
		int num = AlignSize(size);
		Mask = num - 1;
		Entries = new TEntry[num];
	}

	private static int AlignSize(int size)
	{
		DebugAssertHelper.Assert(size > 0);
		size--;
		size |= size >> 1;
		size |= size >> 2;
		size |= size >> 4;
		size |= size >> 8;
		size |= size >> 16;
		return size + 1;
	}
}
