using System.Reflection.Internal;

namespace System.Reflection.Metadata.Ecma335;

internal readonly struct GuidHeap(MemoryBlock block)
{
	internal readonly MemoryBlock Block = block;

	internal Guid GetGuid(GuidHandle handle)
	{
		if (handle.IsNil)
		{
			return default;
		}
		return Block.PeekGuid((handle.Index - 1) * 16);
	}
}
