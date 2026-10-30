using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(AvlTree<>.DebuggerProxy))]
internal class AvlTree<T> : IEnumerable<T>, IEnumerable
{
	private class DebuggerProxy
	{
		private readonly AvlTree<T> tree;

		private T[] contents;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] Contents
		{
			get
			{
				if (contents == null)
				{
					contents = tree.ToArray();
				}
				return contents;
			}
		}

		public DebuggerProxy(AvlTree<T> tree)
		{
			this.tree = tree;
		}
	}

	internal sealed class Node
	{
		internal const short LeaningLeft = -1;

		internal const short Balanced = 0;

		internal const short LeaningRight = 1;

		internal const short LeaningTooFarRight = 2;

		internal const short LeaningTooFarLeft = -2;

		internal static readonly Node EmptyNode = new Node();

		private readonly T key;

		private int height = -1;

		private Node left;

		private Node right;

		private int count;

		public bool IsEmpty => left == null;

		public Node Left => left;

		public Node Right => right;

		public int Count => count;

		public int Height => height;

		public T Value => key;

		public bool IsBalanced
		{
			get
			{
				int treeBalance = GetTreeBalance(this);
				if (treeBalance >= -1)
				{
					return treeBalance <= 1;
				}
				return false;
			}
		}

		private Node()
		{
			height = 0;
		}

		private Node(T key, Node left, Node right)
		{
			this.key = key;
			this.left = left;
			this.right = right;
			height = 1 + Math.Max(left.height, right.height);
			count = 1 + left.count + right.count;
		}

		public Enumerator GetEnumerator(Func<T, bool> isWithinRange)
		{
			return new Enumerator(this, isWithinRange);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "v={0},l={1},r={2},height={3}", key.ToString(), (left != null) ? left.key.ToString() : "null", (right != null) ? right.key.ToString() : "null", height);
		}

		internal IEnumerator<T> Reverse(Func<T, bool> isWithinRangeDelegate)
		{
			return new Enumerator(this, isWithinRangeDelegate, reverse: true);
		}

		internal Node Add(T newValue, IComparer<T> comparer, out bool mutated)
		{
			if (IsEmpty)
			{
				mutated = true;
				return new Node(newValue, this, this);
			}
			Node node = this;
			int num = comparer.Compare(newValue, key);
			if (num > 0)
			{
				Node rightParameter = right.Add(newValue, comparer, out mutated);
				if (mutated)
				{
					node = SetChild(null, rightParameter);
				}
			}
			else
			{
				if (num >= 0)
				{
					mutated = false;
					return this;
				}
				Node leftParameter = left.Add(newValue, comparer, out mutated);
				if (mutated)
				{
					node = SetChild(leftParameter);
				}
			}
			if (!mutated)
			{
				return node;
			}
			return MakeBalanced(node);
		}

		internal Node Remove(T keyToRemove, IComparer<T> comparer, out bool mutated, out T valueRemoved)
		{
			if (IsEmpty)
			{
				mutated = false;
				valueRemoved = default;
				return this;
			}
			Node node = this;
			int num = comparer.Compare(keyToRemove, key);
			if (num == 0)
			{
				mutated = true;
				valueRemoved = key;
				if (right.IsEmpty && left.IsEmpty)
				{
					node = EmptyNode;
				}
				else if (right.IsEmpty && !left.IsEmpty)
				{
					node = left;
				}
				else if (!right.IsEmpty && left.IsEmpty)
				{
					node = right;
				}
				else
				{
					Node node2 = right;
					while (!node2.left.IsEmpty)
					{
						node2 = node2.left;
					}
					Node rightParameter = right.Remove(node2.key, comparer, out var _, out var _);
					node = node2.SetChild(left, rightParameter);
				}
			}
			else if (num < 0)
			{
				Node leftParameter = left.Remove(keyToRemove, comparer, out mutated, out valueRemoved);
				if (mutated)
				{
					node = SetChild(leftParameter);
				}
			}
			else
			{
				Node rightParameter2 = right.Remove(keyToRemove, comparer, out mutated, out valueRemoved);
				if (mutated)
				{
					node = SetChild(null, rightParameter2);
				}
			}
			if (!node.IsEmpty)
			{
				return MakeBalanced(node);
			}
			return node;
		}

		internal int IndexOf(T keyParam, IComparer<T> comparer)
		{
			if (IsEmpty)
			{
				return -1;
			}
			int num = comparer.Compare(keyParam, key);
			if (num == 0)
			{
				return left.Count;
			}
			if (num > 0)
			{
				int num2 = right.IndexOf(keyParam, comparer);
				bool flag = num2 < 0;
				if (flag)
				{
					num2 = ~num2;
				}
				num2 = left.Count + 1 + num2;
				if (flag)
				{
					num2 = ~num2;
				}
				return num2;
			}
			return left.IndexOf(keyParam, comparer);
		}

		internal Node GetSubTree(T from, T to, IComparer<T> comparer)
		{
			Node node = this;
			while (!node.IsEmpty)
			{
				if (comparer.Compare(from, node.Value) > 0)
				{
					node = node.right;
					continue;
				}
				if (comparer.Compare(to, node.Value) < 0)
				{
					node = node.left;
					continue;
				}
				return node;
			}
			return EmptyNode;
		}

		internal Node GetSubTreeFrom(T from, IComparer<T> comparer)
		{
			Node node = this;
			while (!node.IsEmpty)
			{
				if (comparer.Compare(from, node.Value) > 0)
				{
					node = node.right;
					continue;
				}
				return node;
			}
			return EmptyNode;
		}

		internal Node GetSubTreeTo(T to, IComparer<T> comparer)
		{
			Node node = this;
			while (!node.IsEmpty)
			{
				if (comparer.Compare(to, node.Value) < 0)
				{
					node = node.left;
					continue;
				}
				return node;
			}
			return EmptyNode;
		}

		[Conditional("MYDEBUG")]
		internal void DebugValidateHeightAndBalance()
		{
			if (left != null)
			{
				_ = left.height;
			}
			if (right != null)
			{
				_ = right.height;
			}
		}

		internal void DebugDumpTree(StringBuilder stringBuilder, int indent)
		{
			if (!right.IsEmpty)
			{
				right.DebugDumpTree(stringBuilder, indent + 1);
			}
			stringBuilder.Append('\t', indent);
			if (!left.IsEmpty || !right.IsEmpty)
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0}] {1}", key, height);
			}
			else
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0}]", key);
			}
			stringBuilder.AppendLine();
			if (!left.IsEmpty)
			{
				left.DebugDumpTree(stringBuilder, indent + 1);
			}
		}

		internal Node Search(T keyToSearch, IComparer<T> comparer)
		{
			if (IsEmpty)
			{
				return this;
			}
			int num = comparer.Compare(keyToSearch, key);
			if (num == 0)
			{
				return this;
			}
			if (num > 0)
			{
				return right.Search(keyToSearch, comparer);
			}
			return left.Search(keyToSearch, comparer);
		}

		internal bool Contains(T keyParameter, IComparer<T> comparer)
		{
			return !Search(keyParameter, comparer).IsEmpty;
		}

		private static Node RotateLeft(Node tree)
		{
			if (tree.right.IsEmpty)
			{
				return tree;
			}
			Node node = tree.right;
			return node.SetChild(tree.SetChild(null, node.left));
		}

		private static Node RotateRight(Node tree)
		{
			if (tree.left.IsEmpty)
			{
				return tree;
			}
			Node node = tree.left;
			return node.SetChild(null, tree.SetChild(node.right));
		}

		private static Node RotateDoubleLeft(Node tree)
		{
			if (tree.right.IsEmpty)
			{
				return tree;
			}
			Node tree2 = tree.SetChild(null, RotateRight(tree.right));
			return RotateLeft(tree2);
		}

		private static Node RotateDoubleRight(Node tree)
		{
			if (tree.left.IsEmpty)
			{
				return tree;
			}
			Node tree2 = tree.SetChild(RotateLeft(tree.left));
			return RotateRight(tree2);
		}

		private static int GetTreeBalance(Node tree)
		{
			return GetHeight(tree.right) - GetHeight(tree.left);
		}

		private static int GetHeight(Node node)
		{
			return node?.height ?? 0;
		}

		private static bool IsLeaningTooFarRight(Node tree)
		{
			return GetTreeBalance(tree) >= 2;
		}

		private static bool IsLeaningTooFarLeft(Node tree)
		{
			return GetTreeBalance(tree) <= -2;
		}

		private static bool IsRightHeavy(Node tree)
		{
			return GetTreeBalance(tree) == 1;
		}

		private static bool IsLeftHeavy(Node tree)
		{
			return GetTreeBalance(tree) == -1;
		}

		private static Node MakeBalanced(Node tree)
		{
			if (tree.IsBalanced)
			{
				return tree;
			}
			if (IsLeaningTooFarRight(tree))
			{
				if (!IsLeftHeavy(tree.right))
				{
					return RotateLeft(tree);
				}
				return RotateDoubleLeft(tree);
			}
			if (IsLeaningTooFarLeft(tree))
			{
				if (!IsRightHeavy(tree.left))
				{
					return RotateRight(tree);
				}
				return RotateDoubleRight(tree);
			}
			return tree;
		}

		private static int Log2(int value)
		{
			int num = 0;
			while (value > 0)
			{
				num++;
				value >>= 1;
			}
			return num;
		}

		private Node SetChild(Node leftParameter = null, Node rightParameter = null)
		{
			if (leftParameter != null)
			{
				left = leftParameter;
			}
			if (rightParameter != null)
			{
				right = rightParameter;
			}
			height = 1 + Math.Max(left.height, right.height);
			count = 1 + left.count + right.count;
			return this;
		}

		[Conditional("DEBUG")]
		private void CheckInvariants()
		{
		}
	}

	public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private readonly bool reverse;

		private Node root;

		private Stack<Node> stack;

		private Node current;

		private Func<T, bool> isWithinRangeDelegate;

		public T Current
		{
			get
			{
				ThrowIfDisposed();
				if (current == null)
				{
					return default;
				}
				return current.Value;
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(Node root, Func<T, bool> isWithinRangeDelegate, bool reverse = false)
		{
			this.root = root;
			current = null;
			this.reverse = reverse;
			this.isWithinRangeDelegate = isWithinRangeDelegate;
			stack = new Stack<Node>(root.Height);
			Initialize(this.root);
		}

		public void Dispose()
		{
			root = null;
			current = null;
			if (stack != null)
			{
				stack.Clear();
				stack = null;
			}
		}

		public bool MoveNext()
		{
			ThrowIfDisposed();
			if (stack.Count == 0)
			{
				current = null;
				return false;
			}
			Node node = stack.Pop();
			current = node;
			Node node2 = (reverse ? current.Left : current.Right);
			while (!node2.IsEmpty)
			{
				Node node3 = (reverse ? node2.Right : node2.Left);
				Node node4 = (reverse ? node2.Left : node2.Right);
				if (HasNoRangeOrIsWithinRange(node2.Value))
				{
					stack.Push(node2);
					node2 = node3;
				}
				else
				{
					node2 = ((!node4.IsEmpty && !HasNoRangeOrNotWithinRange(node4.Value)) ? node4 : node3);
				}
			}
			return true;
		}

		public void Reset()
		{
			ThrowIfDisposed();
			stack.Clear();
			Initialize(root);
		}

		private void ThrowIfDisposed()
		{
			if (root == null)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}

		private void Initialize(Node node)
		{
			while (!node.IsEmpty)
			{
				Node node2 = (reverse ? node.Right : node.Left);
				Node node3 = (reverse ? node.Left : node.Right);
				if (HasNoRangeOrIsWithinRange(node.Value))
				{
					stack.Push(node);
					node = node2;
				}
				else
				{
					node = ((!node2.IsEmpty && !HasNoRangeOrNotWithinRange(node2.Value)) ? node2 : node3);
				}
			}
		}

		private bool HasNoRangeOrIsWithinRange(T value)
		{
			if (isWithinRangeDelegate != null)
			{
				return isWithinRangeDelegate(value);
			}
			return true;
		}

		private bool HasNoRangeOrNotWithinRange(T value)
		{
			if (isWithinRangeDelegate != null)
			{
				return !isWithinRangeDelegate(value);
			}
			return true;
		}
	}

	internal class ReverseEnumerable : IEnumerable<T>, IEnumerable
	{
		private readonly Node root;

		private readonly Func<T, bool> isWithinRangeDelegate;

		internal ReverseEnumerable(Node root, Func<T, bool> isWithinRangeDelegate)
		{
			this.root = root;
			this.isWithinRangeDelegate = isWithinRangeDelegate;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return root.Reverse(isWithinRangeDelegate);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public const int UndefinedCount = -1;

	private readonly IComparer<T> comparer;

	private Node currentTree;

	private readonly T lowerBound;

	private readonly bool isLowerBoundDefined;

	private readonly bool isUpperBoundDefined;

	private readonly T upperBound;

	public bool IsEmpty => currentTree.IsEmpty;

	public int Count
	{
		get
		{
			if (!isLowerBoundDefined && !isUpperBoundDefined)
			{
				return currentTree.Count;
			}
			return -1;
		}
	}

	public IComparer<T> Comparer => comparer;

	internal AvlTree(T lowerBound = default(T), T upperBound = default(T), bool isLowerBoundDefined = false, bool isUpperBoundDefined = false, IComparer<T> comparer = null)
	{
		this.comparer = comparer ?? Comparer<T>.Default;
		currentTree = Node.EmptyNode;
		this.lowerBound = lowerBound;
		this.upperBound = upperBound;
		this.isLowerBoundDefined = isLowerBoundDefined;
		this.isUpperBoundDefined = isUpperBoundDefined;
	}

	private AvlTree(Node currentTree, T lowerBound = default(T), T upperBound = default(T), bool isLowerBoundDefined = false, bool isUpperBoundDefined = false, IComparer<T> comparer = null)
		: this(lowerBound, upperBound, isLowerBoundDefined, isUpperBoundDefined, comparer)
	{
		this.currentTree = currentTree;
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public Enumerator GetEnumerator()
	{
		Func<T, bool> rangeCheckDelegate = GetRangeCheckDelegate();
		return currentTree.GetEnumerator(rangeCheckDelegate);
	}

	public bool Remove(T value, out T valueRemoved)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!IsWithinRange(value))
		{
			valueRemoved = default;
			return false;
		}
		int count = currentTree.Count;
		currentTree = currentTree.Remove(value, comparer, out var mutated, out valueRemoved);
		int count2 = currentTree.Count;
		return ((currentTree == Node.EmptyNode) | mutated) && count - count2 == 1;
	}

	public bool Add(T key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (!IsWithinRange(key))
		{
			throw new ArgumentOutOfRangeException("key");
		}
		currentTree = currentTree.Add(key, comparer, out var mutated);
		return mutated;
	}

	public T GetValue(T key)
	{
		if (!IsWithinRange(key))
		{
			return default;
		}
		Node node = currentTree.Search(key, comparer);
		return node.Value;
	}

	public bool Contains(T value)
	{
		if (!IsWithinRange(value))
		{
			return false;
		}
		return currentTree.Contains(value, comparer);
	}

	public void Clear()
	{
		currentTree = Node.EmptyNode;
	}

	public IEnumerable<T> Reverse()
	{
		Func<T, bool> rangeCheckDelegate = GetRangeCheckDelegate();
		return new ReverseEnumerable(currentTree, rangeCheckDelegate);
	}

	public int IndexOf(T value)
	{
		if (!IsWithinRange(value))
		{
			return -1;
		}
		return currentTree.IndexOf(value, comparer);
	}

	public AvlTree<T> GetViewBetween(T lowerValue, T upperValue, IComparer<T> comparerParameter = null)
	{
		IComparer<T> comparer = comparerParameter ?? this.comparer;
		if (this.comparer.Compare(lowerValue, upperValue) > 0)
		{
			return new AvlTree<T>();
		}
		if (isLowerBoundDefined && comparer.Compare(lowerBound, lowerValue) > 0)
		{
			throw new ArgumentOutOfRangeException("lowerValue");
		}
		if (isUpperBoundDefined && comparer.Compare(upperBound, upperValue) < 0)
		{
			throw new ArgumentOutOfRangeException("upperValue");
		}
		Node subTree = currentTree.GetSubTree(lowerValue, upperValue, comparer);
		if (subTree == Node.EmptyNode)
		{
			return new AvlTree<T>();
		}
		return new AvlTree<T>(subTree, lowerValue, upperValue, isLowerBoundDefined: true, isUpperBoundDefined: true, comparer);
	}

	public AvlTree<T> GetViewFrom(T fromValue, IComparer<T> comparerParameter = null)
	{
		IComparer<T> comparer = comparerParameter ?? this.comparer;
		if (isLowerBoundDefined && comparer.Compare(lowerBound, fromValue) > 0)
		{
			throw new ArgumentOutOfRangeException("fromValue");
		}
		Node subTreeFrom = currentTree.GetSubTreeFrom(fromValue, comparer);
		if (subTreeFrom == Node.EmptyNode)
		{
			return new AvlTree<T>();
		}
		return new AvlTree<T>(subTreeFrom, fromValue, default, isLowerBoundDefined: true, isUpperBoundDefined: false, comparer);
	}

	public AvlTree<T> GetViewTo(T toValue, IComparer<T> comparerParameter = null)
	{
		IComparer<T> comparer = comparerParameter ?? this.comparer;
		if (isUpperBoundDefined && comparer.Compare(upperBound, toValue) < 0)
		{
			throw new ArgumentOutOfRangeException("toValue");
		}
		Node subTreeTo = currentTree.GetSubTreeTo(toValue, comparer);
		if (subTreeTo == Node.EmptyNode)
		{
			return new AvlTree<T>();
		}
		return new AvlTree<T>(subTreeTo, default, toValue, isLowerBoundDefined: false, isUpperBoundDefined: true, comparer);
	}

	internal bool IsWithinRange(T item)
	{
		if (!isLowerBoundDefined && !isUpperBoundDefined)
		{
			return true;
		}
		int num = (isLowerBoundDefined ? comparer.Compare(lowerBound, item) : (-1));
		if (num > 0)
		{
			return false;
		}
		num = ((!isUpperBoundDefined) ? 1 : comparer.Compare(upperBound, item));
		if (num < 0)
		{
			return false;
		}
		return true;
	}

	private Func<T, bool> GetRangeCheckDelegate()
	{
		Func<T, bool> result = null;
		if (isUpperBoundDefined || isLowerBoundDefined)
		{
			result = IsWithinRange;
		}
		return result;
	}
}
