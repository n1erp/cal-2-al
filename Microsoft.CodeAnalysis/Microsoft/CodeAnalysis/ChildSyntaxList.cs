using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public readonly struct ChildSyntaxList : IEquatable<ChildSyntaxList>, IReadOnlyList<SyntaxNodeOrToken>, IEnumerable<SyntaxNodeOrToken>, IEnumerable, IReadOnlyCollection<SyntaxNodeOrToken>
{
	public struct Enumerator
	{
		private SyntaxNode _node;

		private int _count;

		private int _childIndex;

		public SyntaxNodeOrToken Current => ItemInternal(_node, _childIndex);

		internal Enumerator(SyntaxNode node, int count)
		{
			_node = node;
			_count = count;
			_childIndex = -1;
		}

		internal void InitializeFrom(SyntaxNode node)
		{
			_node = node;
			_count = CountNodes(node.Green);
			_childIndex = -1;
		}

		public bool MoveNext()
		{
			int num = _childIndex + 1;
			if (num < _count)
			{
				_childIndex = num;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			_childIndex = -1;
		}

		internal bool TryMoveNextAndGetCurrent(out SyntaxNodeOrToken current)
		{
			if (!MoveNext())
			{
				current = default;
				return false;
			}
			current = ItemInternal(_node, _childIndex);
			return true;
		}

		internal SyntaxNode TryMoveNextAndGetCurrentAsNode()
		{
			while (MoveNext())
			{
				SyntaxNode syntaxNode = ItemInternalAsNode(_node, _childIndex);
				if (syntaxNode != null)
				{
					return syntaxNode;
				}
			}
			return null;
		}
	}

	private class EnumeratorImpl : IEnumerator<SyntaxNodeOrToken>, IEnumerator, IDisposable
	{
		private Enumerator _enumerator;

		public SyntaxNodeOrToken Current => _enumerator.Current;

		object IEnumerator.Current => _enumerator.Current;

		internal EnumeratorImpl(SyntaxNode node, int count)
		{
			_enumerator = new Enumerator(node, count);
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		public void Reset()
		{
			_enumerator.Reset();
		}

		public void Dispose()
		{
		}
	}

	public readonly struct Reversed : IEnumerable<SyntaxNodeOrToken>, IEnumerable, IEquatable<Reversed>
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

		private class EnumeratorImpl : IEnumerator<SyntaxNodeOrToken>, IEnumerator, IDisposable
		{
			private Enumerator _enumerator;

			public SyntaxNodeOrToken Current => _enumerator.Current;

			object IEnumerator.Current => _enumerator.Current;

			internal EnumeratorImpl(SyntaxNode node, int count)
			{
				_enumerator = new Enumerator(node, count);
			}

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Reset()
			{
				_enumerator.Reset();
			}

			public void Dispose()
			{
			}
		}

		private readonly SyntaxNode _node;

		private readonly int _count;

		internal Reversed(SyntaxNode node, int count)
		{
			_node = node;
			_count = count;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(_node, _count);
		}

		IEnumerator<SyntaxNodeOrToken> IEnumerable<SyntaxNodeOrToken>.GetEnumerator()
		{
			if (_node == null)
			{
				return SpecializedCollections.EmptyEnumerator<SyntaxNodeOrToken>();
			}
			return new EnumeratorImpl(_node, _count);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (_node == null)
			{
				return SpecializedCollections.EmptyEnumerator<SyntaxNodeOrToken>();
			}
			return new EnumeratorImpl(_node, _count);
		}

		public override int GetHashCode()
		{
			if (_node == null)
			{
				return 0;
			}
			return Hash.Combine(_node.GetHashCode(), _count);
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
			if (_node == other._node)
			{
				return _count == other._count;
			}
			return false;
		}
	}

	private readonly SyntaxNode _node;

	private readonly int _count;

	public int Count => _count;

	public SyntaxNodeOrToken this[int index]
	{
		get
		{
			if ((uint)index < (uint)_count)
			{
				return ItemInternal(_node, index);
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	internal SyntaxNode Node => _node;

	private SyntaxNodeOrToken[] Nodes => this.ToArray();

	internal ChildSyntaxList(SyntaxNode node)
	{
		_node = node;
		_count = CountNodes(node.Green);
	}

	internal static int CountNodes(GreenNode green)
	{
		int num = 0;
		int i = 0;
		for (int slotCount = green.SlotCount; i < slotCount; i++)
		{
			GreenNode slot = green.GetSlot(i);
			if (slot != null)
			{
				num = (slot.IsList ? (num + slot.SlotCount) : (num + 1));
			}
		}
		return num;
	}

	private static int Occupancy(GreenNode green)
	{
		if (!green.IsList)
		{
			return 1;
		}
		return green.SlotCount;
	}

	internal static SyntaxNodeOrToken ItemInternal(SyntaxNode node, int index)
	{
		GreenNode green = node.Green;
		int num = index;
		int num2 = 0;
		int num3 = node.Position;
		GreenNode slot;
		while (true)
		{
			slot = green.GetSlot(num2);
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
		GreenNode green = node.Green;
		int num = node.Position;
		int num2 = 0;
		int num3 = 0;
		GreenNode slot;
		while (true)
		{
			slot = green.GetSlot(num3);
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
		green = slot;
		SyntaxNode nodeSlot = node.GetNodeSlot(num3);
		if (!green.IsList)
		{
			if (nodeSlot != null)
			{
				return nodeSlot;
			}
		}
		else
		{
			num3 = green.FindSlotIndexContainingOffset(targetPosition - num);
			if (nodeSlot != null)
			{
				nodeSlot = nodeSlot.GetNodeSlot(num3);
				if (nodeSlot != null)
				{
					return nodeSlot;
				}
			}
			num += green.GetSlotOffset(num3);
			green = green.GetSlot(num3);
			num2 += num3;
		}
		return new SyntaxNodeOrToken(node, green, num, num2);
	}

	internal static SyntaxNode ItemInternalAsNode(SyntaxNode node, int index)
	{
		GreenNode green = node.Green;
		int num = index;
		int num2 = 0;
		GreenNode slot;
		while (true)
		{
			slot = green.GetSlot(num2);
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
		return _count != 0;
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
			return this[_count - 1];
		}
		throw new InvalidOperationException();
	}

	public Reversed Reverse()
	{
		return new Reversed(_node, _count);
	}

	public Enumerator GetEnumerator()
	{
		if (_node == null)
		{
			return default;
		}
		return new Enumerator(_node, _count);
	}

	IEnumerator<SyntaxNodeOrToken> IEnumerable<SyntaxNodeOrToken>.GetEnumerator()
	{
		if (_node == null)
		{
			return SpecializedCollections.EmptyEnumerator<SyntaxNodeOrToken>();
		}
		return new EnumeratorImpl(_node, _count);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (_node == null)
		{
			return SpecializedCollections.EmptyEnumerator<SyntaxNodeOrToken>();
		}
		return new EnumeratorImpl(_node, _count);
	}

	public override bool Equals(object obj)
	{
		if (obj is ChildSyntaxList)
		{
			return Equals((ChildSyntaxList)obj);
		}
		return false;
	}

	public bool Equals(ChildSyntaxList other)
	{
		return _node == other._node;
	}

	public override int GetHashCode()
	{
		return _node?.GetHashCode() ?? 0;
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
