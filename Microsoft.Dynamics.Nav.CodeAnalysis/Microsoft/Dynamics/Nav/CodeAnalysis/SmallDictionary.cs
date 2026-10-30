using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class SmallDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>, IEnumerable
{
	private abstract class Node
	{
		public K Key { get; }

		public V Value { get; internal set; }

		public virtual Node Next => null;

		protected Node(K key, V value)
		{
			Key = key;
			Value = value;
		}
	}

	private sealed class NodeLinked : Node
	{
		public override Node Next { get; }

		public NodeLinked(K key, V value, Node next)
			: base(key, value)
		{
			Next = next;
		}
	}

	private sealed class AvlNodeHead : AvlNode
	{
		private Node next;

		public override Node Next => next;

		internal AvlNodeHead(int hashCode, K key, V value, Node next)
			: base(hashCode, key, value)
		{
			this.next = next;
		}

		internal void SetNext(Node newNext)
		{
			next = newNext;
		}
	}

	private abstract class HashedNode : Node
	{
		public readonly int HashCode;

		protected HashedNode(int hashCode, K key, V value)
			: base(key, value)
		{
			HashCode = hashCode;
		}
	}

	private class AvlNode : HashedNode
	{
		public AvlNode Left { get; internal set; }

		public AvlNode Right { get; internal set; }

		public sbyte Balance { get; internal set; }

		public AvlNode(int hashCode, K key, V value)
			: base(hashCode, key, value)
		{
		}
	}

	internal struct KeyCollection(SmallDictionary<K, V> dict) : IEnumerable<K>, IEnumerable
	{
		public struct Enumerator
		{
			private readonly Stack<AvlNode> stack;

			private Node next;

			private Node current;

			public K Current => current.Key;

			public Enumerator(SmallDictionary<K, V> dict)
			{
				this = default;
				AvlNode root = dict.root;
				if (root != null)
				{
					if (root.Left == root.Right)
					{
						next = dict.root;
						return;
					}
					stack = new Stack<AvlNode>(dict.HeightApprox());
					stack.Push(dict.root);
				}
			}

			public bool MoveNext()
			{
				if (next != null)
				{
					current = next;
					next = next.Next;
					return true;
				}
				if (stack == null || stack.Count == 0)
				{
					return false;
				}
				AvlNode avlNode = (AvlNode)(current = stack.Pop());
				next = avlNode.Next;
				PushIfNotNull(avlNode.Left);
				PushIfNotNull(avlNode.Right);
				return true;
			}

			private void PushIfNotNull(AvlNode child)
			{
				if (child != null)
				{
					stack.Push(child);
				}
			}
		}

		public class EnumerableImpl : IEnumerator<K>, IDisposable, IEnumerator
		{
			private Enumerator e;

			K IEnumerator<K>.Current => e.Current;

			object IEnumerator.Current => e.Current;

			public EnumerableImpl(Enumerator e)
			{
				this.e = e;
			}

			void IDisposable.Dispose()
			{
			}

			bool IEnumerator.MoveNext()
			{
				return e.MoveNext();
			}

			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}
		}

		private readonly SmallDictionary<K, V> dict = dict;

		public Enumerator GetEnumerator()
		{
			return new Enumerator(dict);
		}

		IEnumerator<K> IEnumerable<K>.GetEnumerator()
		{
			return new EnumerableImpl(GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}

	internal struct ValueCollection(SmallDictionary<K, V> dict) : IEnumerable<V>, IEnumerable
	{
		public struct Enumerator
		{
			private readonly Stack<AvlNode> stack;

			private Node next;

			private Node current;

			public V Current => current.Value;

			public Enumerator(SmallDictionary<K, V> dict)
			{
				this = default;
				AvlNode root = dict.root;
				if (root != null)
				{
					if (root.Left == root.Right)
					{
						next = dict.root;
						return;
					}
					stack = new Stack<AvlNode>(dict.HeightApprox());
					stack.Push(dict.root);
				}
			}

			public bool MoveNext()
			{
				if (next != null)
				{
					current = next;
					next = next.Next;
					return true;
				}
				if (stack == null || stack.Count == 0)
				{
					return false;
				}
				AvlNode avlNode = (AvlNode)(current = stack.Pop());
				next = avlNode.Next;
				PushIfNotNull(avlNode.Left);
				PushIfNotNull(avlNode.Right);
				return true;
			}

			private void PushIfNotNull(AvlNode child)
			{
				if (child != null)
				{
					stack.Push(child);
				}
			}
		}

		public class EnumerableImpl : IEnumerator<V>, IDisposable, IEnumerator
		{
			private Enumerator e;

			V IEnumerator<V>.Current => e.Current;

			object IEnumerator.Current => e.Current;

			public EnumerableImpl(Enumerator e)
			{
				this.e = e;
			}

			void IDisposable.Dispose()
			{
			}

			bool IEnumerator.MoveNext()
			{
				return e.MoveNext();
			}

			void IEnumerator.Reset()
			{
				throw new NotImplementedException();
			}
		}

		private readonly SmallDictionary<K, V> dict = dict;

		public Enumerator GetEnumerator()
		{
			return new Enumerator(dict);
		}

		IEnumerator<V> IEnumerable<V>.GetEnumerator()
		{
			return new EnumerableImpl(GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}

	public struct Enumerator
	{
		private readonly Stack<AvlNode> stack;

		private Node next;

		private Node current;

		public KeyValuePair<K, V> Current => new KeyValuePair<K, V>(current.Key, current.Value);

		public Enumerator(SmallDictionary<K, V> dict)
		{
			this = default;
			AvlNode root = dict.root;
			if (root != null)
			{
				if (root.Left == root.Right)
				{
					next = dict.root;
					return;
				}
				stack = new Stack<AvlNode>(dict.HeightApprox());
				stack.Push(dict.root);
			}
		}

		public bool MoveNext()
		{
			if (next != null)
			{
				current = next;
				next = next.Next;
				return true;
			}
			if (stack == null || stack.Count == 0)
			{
				return false;
			}
			AvlNode avlNode = (AvlNode)(current = stack.Pop());
			next = avlNode.Next;
			PushIfNotNull(avlNode.Left);
			PushIfNotNull(avlNode.Right);
			return true;
		}

		private void PushIfNotNull(AvlNode child)
		{
			if (child != null)
			{
				stack.Push(child);
			}
		}
	}

	public class EnumerableImpl : IEnumerator<KeyValuePair<K, V>>, IDisposable, IEnumerator
	{
		private Enumerator e;

		KeyValuePair<K, V> IEnumerator<KeyValuePair<K, V>>.Current => e.Current;

		object IEnumerator.Current => e.Current;

		public EnumerableImpl(Enumerator e)
		{
			this.e = e;
		}

		void IDisposable.Dispose()
		{
		}

		bool IEnumerator.MoveNext()
		{
			return e.MoveNext();
		}

		void IEnumerator.Reset()
		{
			throw new NotImplementedException();
		}
	}

	private AvlNode root;

	private readonly IEqualityComparer<K> comparer;

	public static readonly SmallDictionary<K, V> Empty = new SmallDictionary<K, V>(null);

	public V this[K key]
	{
		get
		{
			if (!TryGetValue(key, out var value))
			{
				throw new InvalidOperationException("key not found");
			}
			return value;
		}
		set
		{
			Insert(GetHashCode(key), key, value, add: false);
		}
	}

	public KeyCollection Keys => new KeyCollection(this);

	public ValueCollection Values => new ValueCollection(this);

	public SmallDictionary()
		: this((IEqualityComparer<K>)EqualityComparer<K>.Default)
	{
	}

	public SmallDictionary(IEqualityComparer<K> comparer)
	{
		this.comparer = comparer;
	}

	public SmallDictionary(SmallDictionary<K, V> other, IEqualityComparer<K> comparer)
		: this(comparer)
	{
		foreach (KeyValuePair<K, V> item in other)
		{
			Add(item.Key, item.Value);
		}
	}

	private bool CompareKeys(K k1, K k2)
	{
		return comparer.Equals(k1, k2);
	}

	private int GetHashCode(K k)
	{
		return comparer.GetHashCode(k);
	}

	public bool TryGetValue(K key, out V value)
	{
		if (root != null)
		{
			return TryGetValue(GetHashCode(key), key, out value);
		}
		value = default;
		return false;
	}

	public void Add(K key, V value)
	{
		Insert(GetHashCode(key), key, value, add: true);
	}

	public bool ContainsKey(K key)
	{
		V value;
		return TryGetValue(key, out value);
	}

	[Conditional("DEBUG")]
	internal void AssertBalanced()
	{
	}

	private bool TryGetValue(int hashCode, K key, out V value)
	{
		AvlNode avlNode = root;
		while (true)
		{
			if (avlNode.HashCode > hashCode)
			{
				avlNode = avlNode.Left;
			}
			else
			{
				if (avlNode.HashCode >= hashCode)
				{
					break;
				}
				avlNode = avlNode.Right;
			}
			if (avlNode == null)
			{
				value = default;
				return false;
			}
		}
		if (CompareKeys(avlNode.Key, key))
		{
			value = avlNode.Value;
			return true;
		}
		return GetFromList(avlNode.Next, key, out value);
	}

	private bool GetFromList(Node next, K key, out V value)
	{
		while (next != null)
		{
			if (CompareKeys(key, next.Key))
			{
				value = next.Value;
				return true;
			}
			next = next.Next;
		}
		value = default;
		return false;
	}

	private void Insert(int hashCode, K key, V value, bool add)
	{
		AvlNode avlNode = root;
		if (avlNode == null)
		{
			root = new AvlNode(hashCode, key, value);
			return;
		}
		AvlNode avlNode2 = null;
		AvlNode avlNode3 = avlNode;
		AvlNode avlNode4 = null;
		while (true)
		{
			int hashCode2 = avlNode.HashCode;
			if (avlNode.Balance != 0)
			{
				avlNode4 = avlNode2;
				avlNode3 = avlNode;
			}
			if (hashCode2 > hashCode)
			{
				if (avlNode.Left == null)
				{
					avlNode = (avlNode.Left = new AvlNode(hashCode, key, value));
					break;
				}
				avlNode2 = avlNode;
				avlNode = avlNode.Left;
				continue;
			}
			if (hashCode2 < hashCode)
			{
				if (avlNode.Right == null)
				{
					avlNode = (avlNode.Right = new AvlNode(hashCode, key, value));
					break;
				}
				avlNode2 = avlNode;
				avlNode = avlNode.Right;
				continue;
			}
			HandleInsert(avlNode, avlNode2, key, value, add);
			return;
		}
		DebugAssertHelper.Assert(avlNode3 != avlNode);
		AvlNode avlNode7 = avlNode3;
		do
		{
			DebugAssertHelper.Assert(avlNode7.HashCode != hashCode);
			if (avlNode7.HashCode < hashCode)
			{
				avlNode7.Balance--;
				avlNode7 = avlNode7.Right;
			}
			else
			{
				avlNode7.Balance++;
				avlNode7 = avlNode7.Left;
			}
		}
		while (avlNode7 != avlNode);
		AvlNode avlNode8;
		switch (avlNode3.Balance)
		{
		case -2:
			avlNode8 = ((avlNode3.Right.Balance < 0) ? LeftSimple(avlNode3) : LeftComplex(avlNode3));
			break;
		case 2:
			avlNode8 = ((avlNode3.Left.Balance > 0) ? RightSimple(avlNode3) : RightComplex(avlNode3));
			break;
		default:
			return;
		}
		if (avlNode4 == null)
		{
			root = avlNode8;
		}
		else if (avlNode3 == avlNode4.Left)
		{
			avlNode4.Left = avlNode8;
		}
		else
		{
			avlNode4.Right = avlNode8;
		}
	}

	private static AvlNode LeftSimple(AvlNode unbalanced)
	{
		AvlNode right = unbalanced.Right;
		unbalanced.Right = right.Left;
		right.Left = unbalanced;
		unbalanced.Balance = 0;
		right.Balance = 0;
		return right;
	}

	private static AvlNode RightSimple(AvlNode unbalanced)
	{
		AvlNode left = unbalanced.Left;
		unbalanced.Left = left.Right;
		left.Right = unbalanced;
		unbalanced.Balance = 0;
		left.Balance = 0;
		return left;
	}

	private static AvlNode LeftComplex(AvlNode unbalanced)
	{
		AvlNode right = unbalanced.Right;
		AvlNode left = right.Left;
		right.Left = left.Right;
		left.Right = right;
		unbalanced.Right = left.Left;
		left.Left = unbalanced;
		sbyte balance = left.Balance;
		left.Balance = 0;
		if (balance < 0)
		{
			right.Balance = 0;
			unbalanced.Balance = 1;
		}
		else
		{
			right.Balance = (sbyte)(-balance);
			unbalanced.Balance = 0;
		}
		return left;
	}

	private static AvlNode RightComplex(AvlNode unbalanced)
	{
		AvlNode left = unbalanced.Left;
		AvlNode right = left.Right;
		left.Right = right.Left;
		right.Left = left;
		unbalanced.Left = right.Right;
		right.Right = unbalanced;
		sbyte balance = right.Balance;
		right.Balance = 0;
		if (balance < 0)
		{
			left.Balance = 1;
			unbalanced.Balance = 0;
		}
		else
		{
			left.Balance = 0;
			unbalanced.Balance = (sbyte)(-balance);
		}
		return right;
	}

	private void HandleInsert(AvlNode node, AvlNode parent, K key, V value, bool add)
	{
		Node node2 = node;
		do
		{
			if (CompareKeys(node2.Key, key))
			{
				if (add)
				{
					throw new InvalidOperationException();
				}
				node2.Value = value;
				return;
			}
			node2 = node2.Next;
		}
		while (node2 != null);
		AddNode(node, parent, key, value);
	}

	private void AddNode(AvlNode node, AvlNode parent, K key, V value)
	{
		if (node is AvlNodeHead avlNodeHead)
		{
			NodeLinked next = new NodeLinked(key, value, avlNodeHead.Next);
			avlNodeHead.SetNext(next);
			return;
		}
		AvlNodeHead avlNodeHead2 = new AvlNodeHead(node.HashCode, key, value, node);
		avlNodeHead2.Balance = node.Balance;
		avlNodeHead2.Left = node.Left;
		avlNodeHead2.Right = node.Right;
		if (parent == null)
		{
			root = avlNodeHead2;
		}
		else if (node == parent.Left)
		{
			parent.Left = avlNodeHead2;
		}
		else
		{
			parent.Right = avlNodeHead2;
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
	{
		return new EnumerableImpl(GetEnumerator());
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new NotImplementedException();
	}

	private int HeightApprox()
	{
		int num = 0;
		for (AvlNode left = root; left != null; left = left.Left)
		{
			num++;
		}
		return num + num / 2;
	}
}
