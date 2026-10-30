using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
public struct SeparatedSyntaxList<TNode> : IEquatable<SeparatedSyntaxList<TNode>>, IReadOnlyList<TNode>, IReadOnlyCollection<TNode>, IEnumerable<TNode>, IEnumerable where TNode : SyntaxNode
{
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
	[SuppressMessage("Performance", "RS0008", Justification = "Equality not actually implemented")]
	public struct Enumerator
	{
		private readonly SeparatedSyntaxList<TNode> list;

		private int index;

		public TNode Current => list[index];

		internal Enumerator(SeparatedSyntaxList<TNode> list)
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

		internal EnumeratorImpl(SeparatedSyntaxList<TNode> list)
		{
			e = new Enumerator(list);
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			return e.MoveNext();
		}

		public void Reset()
		{
			e.Reset();
		}
	}

	private readonly SyntaxNodeOrTokenList list;

	private readonly int count;

	private readonly int separatorCount;

	internal SyntaxNode Node => list.Node;

	public int Count => count;

	public int SeparatorCount => separatorCount;

	public TNode this[int index]
	{
		get
		{
			SyntaxNode node = list.Node;
			if (node != null)
			{
				if (!node.IsList)
				{
					if (index == 0)
					{
						return (TNode)node;
					}
				}
				else if ((uint)index < (uint)count)
				{
					return (TNode)node.GetNodeSlot(index << 1);
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	public TextSpan FullSpan => list.FullSpan;

	public TextSpan Span => list.Span;

	private TNode[] Nodes => this.ToArray();

	private SyntaxNodeOrToken[] NodesWithSeparators => list.ToArray();

	internal SeparatedSyntaxList(SyntaxNodeOrTokenList list)
	{
		this = default;
		int num = list.Count;
		count = num + 1 >> 1;
		separatorCount = num >> 1;
		this.list = list;
	}

	[Conditional("DEBUG")]
	private static void Validate(SyntaxNodeOrTokenList list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			SyntaxNodeOrToken syntaxNodeOrToken = list[i];
			if ((i & 1) == 0)
			{
				DebugAssertHelper.Assert(syntaxNodeOrToken.IsNode, "Node missing in separated list.");
			}
			else
			{
				DebugAssertHelper.Assert(syntaxNodeOrToken.IsToken, "Separator token missing in separated list.");
			}
		}
	}

	internal SeparatedSyntaxList(SyntaxNode node, int index)
		: this(new SyntaxNodeOrTokenList(node, index))
	{
	}

	public SyntaxToken GetSeparator(int index)
	{
		SyntaxNode node = list.Node;
		if (node != null)
		{
			DebugAssertHelper.Assert(node.IsList, "separated list cannot be a singleton separator");
			if ((uint)index < (uint)separatorCount)
			{
				index = (index << 1) + 1;
				InternalSyntaxNode slot = node.Node.GetSlot(index);
				DebugAssertHelper.Assert(slot.IsToken);
				return new SyntaxToken(node.Parent, slot, node.GetChildPosition(index), list.IndexInParentList + index);
			}
		}
		throw new ArgumentOutOfRangeException("index");
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public IEnumerable<SyntaxToken> GetSeparators()
	{
		return from n in list
			where n.IsToken
			select n.AsToken();
	}

	public override string ToString()
	{
		return list.ToString();
	}

	public string ToFullString()
	{
		return list.ToFullString();
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

	public bool Contains(TNode node)
	{
		return IndexOf(node) >= 0;
	}

	public int IndexOf(TNode node)
	{
		int i = 0;
		for (int num = Count; i < num; i++)
		{
			if (object.Equals(this[i], node))
			{
				return i;
			}
		}
		return -1;
	}

	public int IndexOf(Func<TNode, bool> predicate)
	{
		int i = 0;
		for (int num = Count; i < num; i++)
		{
			if (predicate(this[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public int LastIndexOf(TNode node)
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			if (object.Equals(this[num], node))
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

	public bool Any()
	{
		return list.Any();
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public SyntaxNodeOrTokenList GetWithSeparators()
	{
		return list;
	}

	public static bool operator ==(SeparatedSyntaxList<TNode> left, SeparatedSyntaxList<TNode> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SeparatedSyntaxList<TNode> left, SeparatedSyntaxList<TNode> right)
	{
		return !left.Equals(right);
	}

	public bool Equals(SeparatedSyntaxList<TNode> other)
	{
		return list == other.list;
	}

	public override bool Equals(object obj)
	{
		if (obj is SeparatedSyntaxList<TNode>)
		{
			return Equals((SeparatedSyntaxList<TNode>)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return list.GetHashCode();
	}

	public SeparatedSyntaxList<TNode> Add(TNode node)
	{
		return Insert(Count, node);
	}

	public SeparatedSyntaxList<TNode> AddRange(IEnumerable<TNode> nodes)
	{
		return InsertRange(Count, nodes);
	}

	public SeparatedSyntaxList<TNode> Insert(int index, TNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		return InsertRange(index, new TNode[1] { node });
	}

	public SeparatedSyntaxList<TNode> InsertRange(int index, IEnumerable<TNode> nodes)
	{
		if (nodes == null)
		{
			throw new ArgumentNullException("nodes");
		}
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		SyntaxNodeOrTokenList withSeparators = GetWithSeparators();
		int num = ((index < Count) ? withSeparators.IndexOf(this[index]) : withSeparators.Count);
		if (num > 0 && num < withSeparators.Count)
		{
			SyntaxNodeOrToken syntaxNodeOrToken = withSeparators[num - 1];
			if (syntaxNodeOrToken.IsToken && !KeepSeparatorWithPreviousNode(syntaxNodeOrToken.AsToken()))
			{
				num--;
			}
		}
		List<SyntaxNodeOrToken> list = new List<SyntaxNodeOrToken>();
		foreach (TNode node in nodes)
		{
			if (node != null)
			{
				if (list.Count > 0 || (num > 0 && withSeparators[num - 1].IsNode))
				{
					list.Add(node.Node.CreateSeparator<TNode>(node));
				}
				list.Add(node);
			}
		}
		if (num < withSeparators.Count && withSeparators[num].IsNode)
		{
			SyntaxNode syntaxNode = withSeparators[num].AsNode();
			list.Add(syntaxNode.Node.CreateSeparator<TNode>(syntaxNode));
		}
		return new SeparatedSyntaxList<TNode>(withSeparators.InsertRange(num, list));
	}

	private static bool KeepSeparatorWithPreviousNode(SyntaxToken separator)
	{
		foreach (SyntaxTrivia trailingTrivium in separator.TrailingTrivia)
		{
			if (trailingTrivium.UnderlyingNode.IsTriviaWithEndOfLine())
			{
				return true;
			}
		}
		return false;
	}

	public SeparatedSyntaxList<TNode> RemoveAt(int index)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return Remove(this[index]);
	}

	public SeparatedSyntaxList<TNode> Remove(TNode node)
	{
		SyntaxNodeOrTokenList withSeparators = GetWithSeparators();
		int num = withSeparators.IndexOf(node);
		if (num >= 0 && num <= withSeparators.Count)
		{
			withSeparators = withSeparators.RemoveAt(num);
			if (num < withSeparators.Count && withSeparators[num].IsToken)
			{
				withSeparators = withSeparators.RemoveAt(num);
			}
			else if (num > 0 && withSeparators[num - 1].IsToken)
			{
				withSeparators = withSeparators.RemoveAt(num - 1);
			}
			return new SeparatedSyntaxList<TNode>(withSeparators);
		}
		return this;
	}

	public SeparatedSyntaxList<TNode> Replace(TNode nodeInList, TNode newNode)
	{
		if (newNode == null)
		{
			throw new ArgumentNullException("newNode");
		}
		int num = IndexOf(nodeInList);
		if (num >= 0 && num < Count)
		{
			return new SeparatedSyntaxList<TNode>(GetWithSeparators().Replace(nodeInList, newNode));
		}
		throw new ArgumentOutOfRangeException("nodeInList");
	}

	public SeparatedSyntaxList<TNode> ReplaceRange(TNode nodeInList, IEnumerable<TNode> newNodes)
	{
		if (newNodes == null)
		{
			throw new ArgumentNullException("newNodes");
		}
		int num = IndexOf(nodeInList);
		if (num >= 0 && num < Count)
		{
			List<TNode> list = newNodes.ToList();
			if (list.Count == 0)
			{
				return Remove(nodeInList);
			}
			SeparatedSyntaxList<TNode> result = Replace(nodeInList, list[0]);
			if (list.Count > 1)
			{
				list.RemoveAt(0);
				return result.InsertRange(num + 1, list);
			}
			return result;
		}
		throw new ArgumentOutOfRangeException("nodeInList");
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
		return SpecializedCollections.EmptyEnumerator<TNode>();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (Any())
		{
			return new EnumeratorImpl(this);
		}
		return SpecializedCollections.EmptyEnumerator<TNode>();
	}
}
