using System.Collections.Generic;
using System.Reflection.Internal;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Reflection.Metadata.Ecma335;

internal sealed class VirtualHeap : CriticalDisposableObject
{
	private struct PinnedBlob(GCHandle handle, int length)
	{
		public GCHandle Handle = handle;

		public readonly int Length = length;

		public unsafe MemoryBlock GetMemoryBlock()
		{
			return new MemoryBlock((byte*)(void*)Handle.AddrOfPinnedObject(), Length);
		}
	}

	private Dictionary<uint, PinnedBlob> _blobs;

	private VirtualHeap()
	{
		_blobs = new Dictionary<uint, PinnedBlob>();
	}

	protected override void Release()
	{
		try
		{
		}
		finally
		{
			Dictionary<uint, PinnedBlob> dictionary = Interlocked.Exchange(ref _blobs, null);
			if (dictionary != null)
			{
				foreach (KeyValuePair<uint, PinnedBlob> item in dictionary)
				{
					item.Value.Handle.Free();
				}
			}
		}
	}

	private Dictionary<uint, PinnedBlob> GetBlobs()
	{
		Dictionary<uint, PinnedBlob> blobs = _blobs;
		if (blobs == null)
		{
			throw new ObjectDisposedException("VirtualHeap");
		}
		return blobs;
	}

	public bool TryGetMemoryBlock(uint rawHandle, out MemoryBlock block)
	{
		if (!GetBlobs().TryGetValue(rawHandle, out var value))
		{
			block = default;
			return false;
		}
		block = value.GetMemoryBlock();
		return true;
	}

	internal MemoryBlock AddBlob(uint rawHandle, byte[] value)
	{
		Dictionary<uint, PinnedBlob> blobs = GetBlobs();
		MemoryBlock memoryBlock;
		try
		{
		}
		finally
		{
			PinnedBlob value2 = new PinnedBlob(GCHandle.Alloc(value, GCHandleType.Pinned), value.Length);
			blobs.Add(rawHandle, value2);
			memoryBlock = value2.GetMemoryBlock();
		}
		return memoryBlock;
	}

	internal static VirtualHeap GetOrCreateVirtualHeap(ref VirtualHeap lazyHeap)
	{
		if (lazyHeap == null)
		{
			Interlocked.CompareExchange(ref lazyHeap, new VirtualHeap(), null);
		}
		return lazyHeap;
	}
}
