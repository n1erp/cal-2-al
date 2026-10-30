using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public readonly struct SeparatedSyntaxList<TNode> : IEquatable<SeparatedSyntaxList<TNode>>, IReadOnlyList<TNode>, IEnumerable<TNode>, IEnumerable, IReadOnlyCollection<TNode> where TNode : SyntaxNode
{
	public struct Enumerator
	{
		private readonly SeparatedSyntaxList<TNode> _list;

		private int _index;

		public TNode Current => _list[_index];

		internal Enumerator(in SeparatedSyntaxList<TNode> list)
		{
			_list = list;
			_index = -1;
		}

		public bool MoveNext()
		{
			int num = _index + 1;
			if (num < _list.Count)
			{
				_index = num;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			_index = -1;
		}

		public override bool Equals(object obj)
		{
			throw new NotSupportedException();
		}

		public override int GetHashCode()
		{
			throw new NotSupportedException();
		}
	}

	private class EnumeratorImpl : IEnumerator<TNode>, IEnumerator, IDisposable
	{
		private Enumerator _e;

		public TNode Current => _e.Current;

		object IEnumerator.Current => _e.Current;

		internal EnumeratorImpl(in SeparatedSyntaxList<TNode> list)
		{
			_e = new Enumerator(in list);
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			return _e.MoveNext();
		}

		public void Reset()
		{
			_e.Reset();
		}
	}

	private readonly SyntaxNodeOrTokenList _list;

	private readonly int _count;

	private readonly int _separatorCount;

	internal SyntaxNode Node => _list.Node;

	public int Count => _count;

	public int SeparatorCount => _separatorCount;

	public TNode this[int index]
	{
		get
		{
			SyntaxNode node = _list.Node;
			if (node != null)
			{
				if (!node.IsList)
				{
					if (index == 0)
					{
						return (TNode)node;
					}
				}
				else if ((uint)index < (uint)_count)
				{
					return (TNode)node.GetNodeSlot(index << 1);
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	public TextSpan FullSpan => _list.FullSpan;

	public TextSpan Span => _list.Span;

	private TNode[] Nodes => this.ToArray();

	private SyntaxNodeOrToken[] NodesWithSeparators => _list.ToArray();

	internal SeparatedSyntaxList(SyntaxNodeOrTokenList list)
	{
		this = default;
		int count = list.Count;
		_count = count + 1 >> 1;
		_separatorCount = count >> 1;
		_list = list;
	}

	[Conditional("DEBUG")]
	private static void Validate(SyntaxNodeOrTokenList list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			_ = list[i];
		}
	}

	internal SeparatedSyntaxList(SyntaxNode node, int index)
		: this(new SyntaxNodeOrTokenList(node, index))
	{
	}

	public SyntaxToken GetSeparator(int index)
	{
		SyntaxNode node = _list.Node;
		if (node != null && (uint)index < (uint)_separatorCount)
		{
			index = (index << 1) + 1;
			GreenNode slot = node.Green.GetSlot(index);
			return new SyntaxToken(node.Parent, slot, node.GetChildPosition(index), _list.index + index);
		}
		throw new ArgumentOutOfRangeException("index");
	}

	public IEnumerable<SyntaxToken> GetSeparators()
	{
		return from n in _list
			where n.IsToken
			select n.AsToken();
	}

	public override string ToString()
	{
		return _list.ToString();
	}

	public string ToFullString()
	{
		return _list.ToFullString();
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
		for (int count = Count; i < count; i++)
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
		for (int count = Count; i < count; i++)
		{
			if (predicate(this[i]))
			{
				return i;
			}
		}
		return -1;
	}

	internal int IndexOf(int rawKind)
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			if (this[i].RawKind == rawKind)
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
		return _list.Any();
	}

	internal bool Any(Func<TNode, bool> predicate)
	{
		for (int i = 0; i < Count; i++)
		{
			if (predicate(this[i]))
			{
				return true;
			}
		}
		return false;
	}

	public SyntaxNodeOrTokenList GetWithSeparators()
	{
		return _list;
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
		return _list == other._list;
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
		return _list.GetHashCode();
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
					list.Add(node.Green.CreateSeparator<TNode>(node));
				}
				list.Add(node);
			}
		}
		if (num < withSeparators.Count && withSeparators[num].IsNode)
		{
			SyntaxNode syntaxNode = withSeparators[num].AsNode();
			list.Add(syntaxNode.Green.CreateSeparator<TNode>(syntaxNode));
		}
		return new SeparatedSyntaxList<TNode>(withSeparators.InsertRange(num, list));
	}

	private static bool KeepSeparatorWithPreviousNode(in SyntaxToken separator)
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

	public SeparatedSyntaxList<TNode> ReplaceSeparator(SyntaxToken separatorToken, SyntaxToken newSeparator)
	{
		SyntaxNodeOrTokenList withSeparators = GetWithSeparators();
		int num = withSeparators.IndexOf(separatorToken);
		if (num < 0)
		{
			throw new ArgumentException("separatorToken");
		}
		if (newSeparator.RawKind != withSeparators[num].RawKind || newSeparator.Language != withSeparators[num].Language)
		{
			throw new ArgumentException("newSeparator");
		}
		return new SeparatedSyntaxList<TNode>(withSeparators.Replace(separatorToken, newSeparator));
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

	public static implicit operator SeparatedSyntaxList<SyntaxNode>(SeparatedSyntaxList<TNode> nodes)
	{
		return new SeparatedSyntaxList<SyntaxNode>(nodes._list);
	}

	public static implicit operator SeparatedSyntaxList<TNode>(SeparatedSyntaxList<SyntaxNode> nodes)
	{
		return new SeparatedSyntaxList<TNode>(nodes._list);
	}
}
