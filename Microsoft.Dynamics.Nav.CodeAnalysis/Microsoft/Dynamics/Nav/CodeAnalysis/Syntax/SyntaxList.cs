using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
public struct SyntaxList<TNode> : IReadOnlyList<TNode>, IReadOnlyCollection<TNode>, IEnumerable<TNode>, IEnumerable, IEquatable<SyntaxList<TNode>> where TNode : SyntaxNode
{
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
	[SuppressMessage("Performance", "RS0008", Justification = "Equality not actually implemented")]
	public struct Enumerator
	{
		private readonly SyntaxList<TNode> list;

		private int index;

		public TNode Current => (TNode)list.ItemInternal(index);

		internal Enumerator(SyntaxList<TNode> list)
		{
			this.list = list;
			index = -1;
		}

		public bool MoveNext()
		{
			int num = index + 1;
			if (num < list.Count)
			{
				index = num;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			index = -1;
		}
	}

	private class EnumeratorImpl : IEnumerator<TNode>, IDisposable, IEnumerator
	{
		private Enumerator e;

		public TNode Current => e.Current;

		object IEnumerator.Current => e.Current;

		internal EnumeratorImpl(SyntaxList<TNode> list)
		{
			e = new Enumerator(list);
		}

		public bool MoveNext()
		{
			return e.MoveNext();
		}

		void IDisposable.Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			e.Reset();
		}
	}

	private readonly SyntaxNode node;

	internal SyntaxNode Node => node;

	public int Count
	{
		get
		{
			if (node != null)
			{
				if (!node.IsList)
				{
					return 1;
				}
				return node.SlotCount;
			}
			return 0;
		}
	}

	public TNode this[int index]
	{
		get
		{
			if (node != null)
			{
				if (node.IsList)
				{
					if ((uint)index < (uint)node.SlotCount)
					{
						return (TNode)node.GetNodeSlot(index);
					}
				}
				else if (index == 0)
				{
					return (TNode)node;
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	private TNode[] Nodes => this.ToArray();

	internal SyntaxList(SyntaxNode node)
	{
		this.node = node;
	}

	internal SyntaxNode ItemInternal(int index)
	{
		if (node.IsList)
		{
			return node.GetNodeSlot(index);
		}
		DebugAssertHelper.Assert(index == 0);
		return node;
	}

	public override string ToString()
	{
		if (node == null)
		{
			return string.Empty;
		}
		return node.ToString();
	}

	public string ToFullString()
	{
		if (node == null)
		{
			return string.Empty;
		}
		return node.ToFullString();
	}

	public SyntaxList<TNode> Add(TNode newNode)
	{
		return Insert(Count, newNode);
	}

	public SyntaxList<TNode> AddRange(IEnumerable<TNode> nodes)
	{
		return InsertRange(Count, nodes);
	}

	public SyntaxList<TNode> Insert(int index, TNode newNode)
	{
		if (newNode == null)
		{
			throw new ArgumentNullException("newNode");
		}
		return InsertRange(index, new TNode[1] { newNode });
	}

	public SyntaxList<TNode> InsertRange(int index, IEnumerable<TNode> nodes)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (nodes == null)
		{
			throw new ArgumentNullException("nodes");
		}
		List<TNode> list = this.ToList();
		list.InsertRange(index, nodes);
		if (list.Count == 0)
		{
			return this;
		}
		return CreateList(list[0].Node, list);
	}

	public SyntaxList<TNode> RemoveAt(int index)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return Remove(this[index]);
	}

	public SyntaxList<TNode> Remove(TNode removeNode)
	{
		return CreateList(this.Where((TNode x) => x != removeNode).ToList());
	}

	public SyntaxList<TNode> Replace(TNode nodeInList, TNode newNode)
	{
		return ReplaceRange(nodeInList, new TNode[1] { newNode });
	}

	public SyntaxList<TNode> ReplaceRange(TNode nodeInList, IEnumerable<TNode> newNodes)
	{
		if (nodeInList == null)
		{
			throw new ArgumentNullException("nodeInList");
		}
		if (newNodes == null)
		{
			throw new ArgumentNullException("newNodes");
		}
		int num = IndexOf(nodeInList);
		if (num >= 0 && num < Count)
		{
			List<TNode> list = this.ToList();
			list.RemoveAt(num);
			list.InsertRange(num, newNodes);
			return CreateList(list);
		}
		throw new ArgumentOutOfRangeException("nodeInList");
	}

	private static SyntaxList<TNode> CreateList(List<TNode> items)
	{
		if (items.Count == 0)
		{
			return default;
		}
		return CreateList(items[0].Node, items);
	}

	private static SyntaxList<TNode> CreateList(InternalSyntaxNode creator, List<TNode> items)
	{
		if (items.Count == 0)
		{
			return default;
		}
		return new SyntaxList<TNode>(creator.CreateList(items.Select((TNode n) => n.Node)).CreateRed());
	}

	public TNode First()
	{
		return this[0];
	}

	public TNode FirstOrDefault()
	{
		if (Any())
		{
			return this[0];
		}
		return null;
	}

	public TNode Last()
	{
		return this[Count - 1];
	}

	public TNode LastOrDefault()
	{
		if (Any())
		{
			return this[Count - 1];
		}
		return null;
	}

	public bool Any()
	{
		DebugAssertHelper.Assert(node == null || Count != 0);
		return node != null;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
	{
		if (Any())
		{
			return new EnumeratorImpl(this);
		}
		return EmptyEnumerator<TNode>.GetInstance();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (Any())
		{
			return new EnumeratorImpl(this);
		}
		return EmptyEnumerator<TNode>.GetInstance();
	}

	public static bool operator ==(SyntaxList<TNode> left, SyntaxList<TNode> right)
	{
		return left.node == right.node;
	}

	public static bool operator !=(SyntaxList<TNode> left, SyntaxList<TNode> right)
	{
		return left.node != right.node;
	}

	public bool Equals(SyntaxList<TNode> other)
	{
		return node == other.node;
	}

	public override bool Equals(object obj)
	{
		if (obj is SyntaxList<TNode>)
		{
			return Equals((SyntaxList<TNode>)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return node.GetHashCode();
	}

	public static implicit operator SyntaxList<TNode>(SyntaxList<SyntaxNode> nodes)
	{
		return new SyntaxList<TNode>(nodes.node);
	}

	public static implicit operator SyntaxList<SyntaxNode>(SyntaxList<TNode> nodes)
	{
		return new SyntaxList<SyntaxNode>(nodes.Node);
	}

	public int IndexOf(TNode indexNode)
	{
		int num = 0;
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (object.Equals(enumerator.Current, indexNode))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public int IndexOf(Func<TNode, bool> predicate)
	{
		int num = 0;
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			TNode current = enumerator.Current;
			if (predicate(current))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public int LastIndexOf(TNode indexNode)
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			if (object.Equals(this[num], indexNode))
			{
				return num;
			}
		}
		return -1;
	}

	public int LastIndexOf(Func<TNode, bool> predicate)
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			if (predicate(this[num]))
			{
				return num;
			}
		}
		return -1;
	}
}
internal abstract class SyntaxList : SyntaxNode
{
	internal class WithTwoChildren : SyntaxList
	{
		private SyntaxNode child0;

		private SyntaxNode child1;

		internal WithTwoChildren(InternalSyntaxList green, SyntaxNode parent, int position)
			: base(green, parent, position)
		{
		}

		internal override SyntaxNode GetNodeSlot(int index)
		{
			return index switch
			{
				0 => GetRedElement(ref child0, 0), 
				1 => GetRedElementIfNotToken(ref child1), 
				_ => null, 
			};
		}

		internal override SyntaxNode GetCachedSlot(int index)
		{
			return index switch
			{
				0 => child0, 
				1 => child1, 
				_ => null, 
			};
		}

		public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
		{
			throw new NotImplementedException();
		}

		public override void Accept(SyntaxVisitor visitor)
		{
			throw new NotImplementedException();
		}
	}

	internal class WithThreeChildren : SyntaxList
	{
		private SyntaxNode child0;

		private SyntaxNode child1;

		private SyntaxNode child2;

		internal WithThreeChildren(InternalSyntaxList green, SyntaxNode parent, int position)
			: base(green, parent, position)
		{
		}

		internal override SyntaxNode GetNodeSlot(int index)
		{
			return index switch
			{
				0 => GetRedElement(ref child0, 0), 
				1 => GetRedElementIfNotToken(ref child1), 
				2 => GetRedElement(ref child2, 2), 
				_ => null, 
			};
		}

		internal override SyntaxNode GetCachedSlot(int index)
		{
			return index switch
			{
				0 => child0, 
				1 => child1, 
				2 => child2, 
				_ => null, 
			};
		}

		public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
		{
			throw new NotImplementedException();
		}

		public override void Accept(SyntaxVisitor visitor)
		{
			throw new NotImplementedException();
		}
	}

	internal class WithManyChildren : SyntaxList
	{
		private readonly ArrayElement<SyntaxNode>[] children;

		internal WithManyChildren(InternalSyntaxList green, SyntaxNode parent, int position)
			: base(green, parent, position)
		{
			children = new ArrayElement<SyntaxNode>[green.SlotCount];
		}

		internal override SyntaxNode GetNodeSlot(int index)
		{
			return GetRedElement(ref children[index].Value, index);
		}

		internal override SyntaxNode GetCachedSlot(int index)
		{
			return children[index];
		}

		public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
		{
			throw new NotImplementedException();
		}

		public override void Accept(SyntaxVisitor visitor)
		{
			throw new NotImplementedException();
		}
	}

	internal class SeparatedWithManyChildren : SyntaxList
	{
		private readonly ArrayElement<SyntaxNode>[] children;

		internal SeparatedWithManyChildren(InternalSyntaxList green, SyntaxNode parent, int position)
			: base(green, parent, position)
		{
			children = new ArrayElement<SyntaxNode>[green.SlotCount + 1 >> 1];
		}

		internal override SyntaxNode GetNodeSlot(int i)
		{
			if ((i & 1) != 0)
			{
				return null;
			}
			return GetRedElement(ref children[i >> 1].Value, i);
		}

		internal override SyntaxNode GetCachedSlot(int i)
		{
			if ((i & 1) != 0)
			{
				return null;
			}
			return children[i >> 1].Value;
		}

		public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
		{
			throw new NotImplementedException();
		}

		public override void Accept(SyntaxVisitor visitor)
		{
			throw new NotImplementedException();
		}
	}

	internal SyntaxList(InternalSyntaxList green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}
}
