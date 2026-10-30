using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
public struct ChildSyntaxList : IEquatable<ChildSyntaxList>, IReadOnlyList<SyntaxNodeOrToken>, IReadOnlyCollection<SyntaxNodeOrToken>, IEnumerable<SyntaxNodeOrToken>, IEnumerable
{
	public struct Enumerator
	{
		private SyntaxNode node;

		private int count;

		private int childIndex;

		public SyntaxNodeOrToken Current => ItemInternal(node, childIndex);

		internal Enumerator(SyntaxNode node, int count)
		{
			this.node = node;
			this.count = count;
			childIndex = -1;
		}

		internal void InitializeFrom(SyntaxNode node)
		{
			this.node = node;
			count = CountNodes(node.Node);
			childIndex = -1;
		}

		public bool MoveNext()
		{
			int num = childIndex + 1;
			if (num < count)
			{
				childIndex = num;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			childIndex = -1;
		}

		internal bool TryMoveNextAndGetCurrent(ref SyntaxNodeOrToken current)
		{
			if (!MoveNext())
			{
				return false;
			}
			current = ItemInternal(node, childIndex);
			return true;
		}

		internal SyntaxNode TryMoveNextAndGetCurrentAsNode()
		{
			while (MoveNext())
			{
				SyntaxNode syntaxNode = ItemInternalAsNode(node, childIndex);
				if (syntaxNode != null)
				{
					return syntaxNode;
				}
			}
			return null;
		}
	}

	private class EnumeratorImpl : IEnumerator<SyntaxNodeOrToken>, IDisposable, IEnumerator
	{
		private Enumerator enumerator;

		public SyntaxNodeOrToken Current => enumerator.Current;

		object IEnumerator.Current => enumerator.Current;

		internal EnumeratorImpl(SyntaxNode node, int count)
		{
			enumerator = new Enumerator(node, count);
		}

		public bool MoveNext()
		{
			return enumerator.MoveNext();
		}

		public void Reset()
		{
			enumerator.Reset();
		}

		public void Dispose()
		{
		}
	}

	public struct Reversed : IEnumerable<SyntaxNodeOrToken>, IEnumerable, IEquatable<Reversed>
	{
		public struct Enumerator
		{
			private readonly SyntaxNode _node;

			private readonly int _count;

			private int _childIndex;

			public SyntaxNodeOrToken Current => ItemInternal(_node, _childIndex);

			internal Enumerator(SyntaxNode node, int count)
			{
				_node = node;
				_count = count;
				_childIndex = count;
			}

			public bool MoveNext()
			{
				return --_childIndex >= 0;
			}

			public void Reset()
			{
				_childIndex = _count;
			}
		}

		private class EnumeratorImpl : IEnumerator<SyntaxNodeOrToken>, IDisposable, IEnumerator
		{
			private Enumerator enumerator;

			public SyntaxNodeOrToken Current => enumerator.Current;

			object IEnumerator.Current => enumerator.Current;

			internal EnumeratorImpl(SyntaxNode node, int count)
			{
				enumerator = new Enumerator(node, count);
			}

			public bool MoveNext()
			{
				return enumerator.MoveNext();
			}

			public void Reset()
			{
				enumerator.Reset();
			}

			public void Dispose()
			{
			}
		}

		private readonly SyntaxNode node;

		private readonly int count;

		internal Reversed(SyntaxNode node, int count)
		{
			this.node = node;
			this.count = count;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(node, count);
		}

		IEnumerator<SyntaxNodeOrToken> IEnumerable<SyntaxNodeOrToken>.GetEnumerator()
		{
			if (node == null)
			{
				return SpecializedCollections.EmptyEnumerator<SyntaxNodeOrToken>();
			}
			return new EnumeratorImpl(node, count);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (node == null)
			{
				return SpecializedCollections.EmptyEnumerator<SyntaxNodeOrToken>();
			}
			return new EnumeratorImpl(node, count);
		}

		public override int GetHashCode()
		{
			if (node == null)
			{
				return 0;
			}
			return Hash.Combine(node.GetHashCode(), count);
		}

		public override bool Equals(object obj)
		{
			if (obj is Reversed)
			{
				return Equals((Reversed)obj);
			}
			return false;
		}

		public bool Equals(Reversed other)
		{
			if (node == other.node)
			{
				return count == other.count;
			}
			return false;
		}
	}

	private readonly SyntaxNode node;

	private readonly int count;

	public int Count => count;

	public SyntaxNodeOrToken this[int index]
	{
		get
		{
			if ((uint)index < (uint)count)
			{
				return ItemInternal(node, index);
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	internal SyntaxNode Node => node;

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private SyntaxNodeOrToken[] Nodes => this.ToArray();

	internal ChildSyntaxList(SyntaxNode node)
	{
		this.node = node;
		count = CountNodes(node.Node);
	}

	internal static int CountNodes(InternalSyntaxNode green)
	{
		int num = 0;
		int i = 0;
		for (int slotCount = green.SlotCount; i < slotCount; i++)
		{
			InternalSyntaxNode slot = green.GetSlot(i);
			if (slot != null)
			{
				num = (slot.IsList ? (num + slot.SlotCount) : (num + 1));
			}
		}
		return num;
	}

	private static int Occupancy(InternalSyntaxNode green)
	{
		if (!green.IsList)
		{
			return 1;
		}
		return green.SlotCount;
	}

	internal static SyntaxNodeOrToken ItemInternal(SyntaxNode node, int index)
	{
		InternalSyntaxNode internalSyntaxNode = node.Node;
		int num = index;
		int num2 = 0;
		int num3 = node.Position;
		InternalSyntaxNode slot;
		while (true)
		{
			slot = internalSyntaxNode.GetSlot(num2);
			if (slot != null)
			{
				int num4 = Occupancy(slot);
				if (num < num4)
				{
					break;
				}
				num -= num4;
				num3 += slot.FullWidth;
			}
			num2++;
		}
		SyntaxNode nodeSlot = node.GetNodeSlot(num2);
		if (!slot.IsList)
		{
			if (nodeSlot != null)
			{
				return nodeSlot;
			}
		}
		else if (nodeSlot != null)
		{
			SyntaxNode nodeSlot2 = nodeSlot.GetNodeSlot(num);
			if (nodeSlot2 != null)
			{
				return nodeSlot2;
			}
			slot = slot.GetSlot(num);
			num3 = nodeSlot.GetChildPosition(num);
		}
		else
		{
			num3 += slot.GetSlotOffset(num);
			slot = slot.GetSlot(num);
		}
		return new SyntaxNodeOrToken(node, slot, num3, index);
	}

	internal static SyntaxNodeOrToken ChildThatContainsPosition(SyntaxNode node, int targetPosition)
	{
		DebugAssertHelper.Assert(node.FullSpan.Contains(targetPosition));
		InternalSyntaxNode internalSyntaxNode = node.Node;
		int num = node.Position;
		int num2 = 0;
		DebugAssertHelper.Assert(!internalSyntaxNode.IsList);
		int num3 = 0;
		InternalSyntaxNode slot;
		while (true)
		{
			slot = internalSyntaxNode.GetSlot(num3);
			if (slot != null)
			{
				int num4 = num + slot.FullWidth;
				if (targetPosition < num4)
				{
					break;
				}
				num = num4;
				num2 += Occupancy(slot);
			}
			num3++;
		}
		internalSyntaxNode = slot;
		SyntaxNode nodeSlot = node.GetNodeSlot(num3);
		if (!internalSyntaxNode.IsList)
		{
			if (nodeSlot != null)
			{
				return nodeSlot;
			}
		}
		else
		{
			num3 = internalSyntaxNode.FindSlotIndexContainingOffset(targetPosition - num);
			if (nodeSlot != null)
			{
				nodeSlot = nodeSlot.GetNodeSlot(num3);
				if (nodeSlot != null)
				{
					return nodeSlot;
				}
			}
			num += internalSyntaxNode.GetSlotOffset(num3);
			internalSyntaxNode = internalSyntaxNode.GetSlot(num3);
			num2 += num3;
		}
		return new SyntaxNodeOrToken(node, internalSyntaxNode, num, num2);
	}

	internal static SyntaxNode ItemInternalAsNode(SyntaxNode node, int index)
	{
		InternalSyntaxNode internalSyntaxNode = node.Node;
		int num = index;
		int num2 = 0;
		InternalSyntaxNode slot;
		while (true)
		{
			slot = internalSyntaxNode.GetSlot(num2);
			if (slot != null)
			{
				int num3 = Occupancy(slot);
				if (num < num3)
				{
					break;
				}
				num -= num3;
			}
			num2++;
		}
		SyntaxNode nodeSlot = node.GetNodeSlot(num2);
		if (slot.IsList && nodeSlot != null)
		{
			return nodeSlot.GetNodeSlot(num);
		}
		return nodeSlot;
	}

	public bool Any()
	{
		return count != 0;
	}

	public SyntaxNodeOrToken First()
	{
		if (Any())
		{
			return this[0];
		}
		throw new InvalidOperationException();
	}

	public SyntaxNodeOrToken Last()
	{
		if (Any())
		{
			return this[count - 1];
		}
		throw new InvalidOperationException();
	}

	public Reversed Reverse()
	{
		return new Reversed(node, count);
	}

	public Enumerator GetEnumerator()
	{
		if (node == null)
		{
			return default;
		}
		return new Enumerator(node, count);
	}

	IEnumerator<SyntaxNodeOrToken> IEnumerable<SyntaxNodeOrToken>.GetEnumerator()
	{
		if (node == null)
		{
			return SpecializedCollections.EmptyEnumerator<SyntaxNodeOrToken>();
		}
		return new EnumeratorImpl(node, count);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (node == null)
		{
			return SpecializedCollections.EmptyEnumerator<SyntaxNodeOrToken>();
		}
		return new EnumeratorImpl(node, count);
	}

	public override bool Equals(object obj)
	{
		if (obj is InternalChildSyntaxList)
		{
			return Equals((InternalChildSyntaxList)obj);
		}
		return false;
	}

	public bool Equals(ChildSyntaxList other)
	{
		return node == other.node;
	}

	public override int GetHashCode()
	{
		if (node == null)
		{
			return 0;
		}
		return node.GetHashCode();
	}

	public static bool operator ==(ChildSyntaxList list1, ChildSyntaxList list2)
	{
		return list1.Equals(list2);
	}

	public static bool operator !=(ChildSyntaxList list1, ChildSyntaxList list2)
	{
		return !list1.Equals(list2);
	}
}
