using System.Runtime.CompilerServices;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Syntax.InternalSyntax;

internal static class SyntaxNodeCache
{
	private struct Entry
	{
		public readonly int hash;

		public readonly GreenNode node;

		internal Entry(int hash, GreenNode node)
		{
			this.hash = hash;
			this.node = node;
		}
	}

	private const int CacheSizeBits = 16;

	private const int CacheSize = 65536;

	private const int CacheMask = 65535;

	private static readonly Entry[] s_cache = new Entry[65536];

	internal static void AddNode(GreenNode node, int hash)
	{
		if (AllChildrenInCache(node) && !node.IsMissing)
		{
			int num = hash & 0xFFFF;
			s_cache[num] = new Entry(hash, node);
		}
	}

	private static bool CanBeCached(GreenNode child1)
	{
		return child1?.IsCacheable ?? true;
	}

	private static bool CanBeCached(GreenNode child1, GreenNode child2)
	{
		if (CanBeCached(child1))
		{
			return CanBeCached(child2);
		}
		return false;
	}

	private static bool CanBeCached(GreenNode child1, GreenNode child2, GreenNode child3)
	{
		if (CanBeCached(child1) && CanBeCached(child2))
		{
			return CanBeCached(child3);
		}
		return false;
	}

	private static bool ChildInCache(GreenNode child)
	{
		if (child == null || child.SlotCount == 0)
		{
			return true;
		}
		int num = child.GetCacheHash() & 0xFFFF;
		return s_cache[num].node == child;
	}

	private static bool AllChildrenInCache(GreenNode node)
	{
		int slotCount = node.SlotCount;
		for (int i = 0; i < slotCount; i++)
		{
			if (!ChildInCache(node.GetSlot(i)))
			{
				return false;
			}
		}
		return true;
	}

	internal static GreenNode TryGetNode(int kind, GreenNode child1, out int hash)
	{
		return TryGetNode(kind, child1, GetDefaultNodeFlags(), out hash);
	}

	internal static GreenNode TryGetNode(int kind, GreenNode child1, GreenNode.NodeFlags flags, out int hash)
	{
		if (CanBeCached(child1))
		{
			int num = (hash = GetCacheHash(kind, flags, child1));
			int num2 = num & 0xFFFF;
			Entry entry = s_cache[num2];
			if (entry.hash == num && entry.node != null && entry.node.IsCacheEquivalent(kind, flags, child1))
			{
				return entry.node;
			}
		}
		else
		{
			hash = -1;
		}
		return null;
	}

	internal static GreenNode TryGetNode(int kind, GreenNode child1, GreenNode child2, out int hash)
	{
		return TryGetNode(kind, child1, child2, GetDefaultNodeFlags(), out hash);
	}

	internal static GreenNode TryGetNode(int kind, GreenNode child1, GreenNode child2, GreenNode.NodeFlags flags, out int hash)
	{
		if (CanBeCached(child1, child2))
		{
			int num = (hash = GetCacheHash(kind, flags, child1, child2));
			int num2 = num & 0xFFFF;
			Entry entry = s_cache[num2];
			if (entry.hash == num && entry.node != null && entry.node.IsCacheEquivalent(kind, flags, child1, child2))
			{
				return entry.node;
			}
		}
		else
		{
			hash = -1;
		}
		return null;
	}

	internal static GreenNode TryGetNode(int kind, GreenNode child1, GreenNode child2, GreenNode child3, out int hash)
	{
		return TryGetNode(kind, child1, child2, child3, GetDefaultNodeFlags(), out hash);
	}

	internal static GreenNode TryGetNode(int kind, GreenNode child1, GreenNode child2, GreenNode child3, GreenNode.NodeFlags flags, out int hash)
	{
		if (CanBeCached(child1, child2, child3))
		{
			int num = (hash = GetCacheHash(kind, flags, child1, child2, child3));
			int num2 = num & 0xFFFF;
			Entry entry = s_cache[num2];
			if (entry.hash == num && entry.node != null && entry.node.IsCacheEquivalent(kind, flags, child1, child2, child3))
			{
				return entry.node;
			}
		}
		else
		{
			hash = -1;
		}
		return null;
	}

	public static GreenNode.NodeFlags GetDefaultNodeFlags()
	{
		return GreenNode.NodeFlags.IsNotMissing;
	}

	private static int GetCacheHash(int kind, GreenNode.NodeFlags flags, GreenNode child1)
	{
		int currentKey = (int)flags ^ kind;
		currentKey = Hash.Combine(RuntimeHelpers.GetHashCode(child1), currentKey);
		return currentKey & 0x7FFFFFFF;
	}

	private static int GetCacheHash(int kind, GreenNode.NodeFlags flags, GreenNode child1, GreenNode child2)
	{
		int num = (int)flags ^ kind;
		if (child1 != null)
		{
			num = Hash.Combine(RuntimeHelpers.GetHashCode(child1), num);
		}
		if (child2 != null)
		{
			num = Hash.Combine(RuntimeHelpers.GetHashCode(child2), num);
		}
		return num & 0x7FFFFFFF;
	}

	private static int GetCacheHash(int kind, GreenNode.NodeFlags flags, GreenNode child1, GreenNode child2, GreenNode child3)
	{
		int num = (int)flags ^ kind;
		if (child1 != null)
		{
			num = Hash.Combine(RuntimeHelpers.GetHashCode(child1), num);
		}
		if (child2 != null)
		{
			num = Hash.Combine(RuntimeHelpers.GetHashCode(child2), num);
		}
		if (child3 != null)
		{
			num = Hash.Combine(RuntimeHelpers.GetHashCode(child3), num);
		}
		return num & 0x7FFFFFFF;
	}
}
