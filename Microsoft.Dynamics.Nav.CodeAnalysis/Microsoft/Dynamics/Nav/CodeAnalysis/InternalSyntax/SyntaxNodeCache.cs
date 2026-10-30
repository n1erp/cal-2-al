using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class SyntaxNodeCache
{
	private struct Entry
	{
		internal readonly int Hash;

		internal readonly InternalSyntaxNode Node;

		internal Entry(int hash, InternalSyntaxNode node)
		{
			Hash = hash;
			Node = node;
		}
	}

	private const int CacheSizeBits = 16;

	private const int CacheSize = 65536;

	private const int CacheMask = 65535;

	private static readonly Entry[] cache = new Entry[65536];

	internal static void AddNode(InternalSyntaxNode node, int hash)
	{
		if (AllChildrenInCache(node) && !node.IsMissing)
		{
			DebugAssertHelper.Assert(node.GetCacheHash() == hash);
			int num = hash & 0xFFFF;
			cache[num] = new Entry(hash, node);
		}
	}

	private static bool CanBeCached(InternalSyntaxNode child1)
	{
		return child1?.IsCacheable ?? true;
	}

	private static bool CanBeCached(InternalSyntaxNode child1, InternalSyntaxNode child2)
	{
		if (CanBeCached(child1))
		{
			return CanBeCached(child2);
		}
		return false;
	}

	private static bool CanBeCached(InternalSyntaxNode child1, InternalSyntaxNode child2, InternalSyntaxNode child3)
	{
		if (CanBeCached(child1) && CanBeCached(child2))
		{
			return CanBeCached(child3);
		}
		return false;
	}

	private static bool ChildInCache(InternalSyntaxNode child)
	{
		if (child == null || child.SlotCount == 0)
		{
			return true;
		}
		int num = child.GetCacheHash() & 0xFFFF;
		return cache[num].Node == child;
	}

	private static bool AllChildrenInCache(InternalSyntaxNode node)
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

	internal static InternalSyntaxNode TryGetNode(int kind, InternalSyntaxNode child1, LexerMode mode, out int hash)
	{
		return TryGetNode(kind, child1, GetFlags(), mode, out hash);
	}

	internal static InternalSyntaxNode TryGetNode(int kind, InternalSyntaxNode child1, out int hash)
	{
		return TryGetNode(kind, child1, GetFlags(), LexerMode.Default, out hash);
	}

	private static InternalSyntaxNode TryGetNode(int kind, InternalSyntaxNode child1, InternalSyntaxNode.NodeFlags flags, LexerMode mode, out int hash)
	{
		if (CanBeCached(child1))
		{
			int num = (hash = GetCacheHash(kind, flags, mode, child1));
			int num2 = num & 0xFFFF;
			Entry entry = cache[num2];
			if (entry.Hash == num && entry.Node != null && entry.Node.IsCacheEquivalent(kind, flags, mode, child1))
			{
				return entry.Node;
			}
		}
		else
		{
			hash = -1;
		}
		return null;
	}

	internal static InternalSyntaxNode TryGetNode(int kind, InternalSyntaxNode child1, InternalSyntaxNode child2, LexerMode mode, out int hash)
	{
		return TryGetNode(kind, child1, child2, GetFlags(), mode, out hash);
	}

	internal static InternalSyntaxNode TryGetNode(int kind, InternalSyntaxNode child1, InternalSyntaxNode child2, out int hash)
	{
		return TryGetNode(kind, child1, child2, GetFlags(), LexerMode.Default, out hash);
	}

	private static InternalSyntaxNode TryGetNode(int kind, InternalSyntaxNode child1, InternalSyntaxNode child2, InternalSyntaxNode.NodeFlags flags, LexerMode mode, out int hash)
	{
		if (CanBeCached(child1, child2))
		{
			int num = (hash = GetCacheHash(kind, flags, mode, child1, child2));
			int num2 = num & 0xFFFF;
			Entry entry = cache[num2];
			if (entry.Hash == num && entry.Node != null && entry.Node.IsCacheEquivalent(kind, flags, mode, child1, child2))
			{
				return entry.Node;
			}
		}
		else
		{
			hash = -1;
		}
		return null;
	}

	internal static InternalSyntaxNode TryGetNode(int kind, InternalSyntaxNode child1, InternalSyntaxNode child2, InternalSyntaxNode child3, LexerMode mode, out int hash)
	{
		return TryGetNode(kind, child1, child2, child3, GetFlags(), mode, out hash);
	}

	internal static InternalSyntaxNode TryGetNode(int kind, InternalSyntaxNode child1, InternalSyntaxNode child2, InternalSyntaxNode child3, out int hash)
	{
		return TryGetNode(kind, child1, child2, child3, GetFlags(), LexerMode.Default, out hash);
	}

	private static InternalSyntaxNode TryGetNode(int kind, InternalSyntaxNode child1, InternalSyntaxNode child2, InternalSyntaxNode child3, InternalSyntaxNode.NodeFlags flags, LexerMode mode, out int hash)
	{
		if (CanBeCached(child1, child2, child3))
		{
			int num = (hash = GetCacheHash(kind, flags, mode, child1, child2, child3));
			int num2 = num & 0xFFFF;
			Entry entry = cache[num2];
			if (entry.Hash == num && entry.Node != null && entry.Node.IsCacheEquivalent(kind, flags, mode, child1, child2, child3))
			{
				return entry.Node;
			}
		}
		else
		{
			hash = -1;
		}
		return null;
	}

	private static InternalSyntaxNode.NodeFlags GetFlags()
	{
		return InternalSyntaxNode.NodeFlags.IsNotMissing;
	}

	private static int GetCacheHash(int kind, InternalSyntaxNode.NodeFlags flags, LexerMode mode, InternalSyntaxNode child1)
	{
		int h = (int)((uint)flags ^ (uint)kind) ^ (int)mode;
		h = Hash.Combine(RuntimeHelpers.GetHashCode(child1), h);
		return h & 0x7FFFFFFF;
	}

	private static int GetCacheHash(int kind, InternalSyntaxNode.NodeFlags flags, LexerMode mode, InternalSyntaxNode child1, InternalSyntaxNode child2)
	{
		int num = (int)((uint)flags ^ (uint)kind) ^ (int)mode;
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

	private static int GetCacheHash(int kind, InternalSyntaxNode.NodeFlags flags, LexerMode mode, InternalSyntaxNode child1, InternalSyntaxNode child2, InternalSyntaxNode child3)
	{
		int num = (int)((uint)flags ^ (uint)kind) ^ (int)mode;
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
