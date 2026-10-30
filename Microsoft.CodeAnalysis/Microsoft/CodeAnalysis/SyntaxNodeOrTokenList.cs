using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public readonly struct SyntaxNodeOrTokenList : IEquatable<SyntaxNodeOrTokenList>, IReadOnlyCollection<SyntaxNodeOrToken>, IEnumerable<SyntaxNodeOrToken>, IEnumerable
{
	public struct Enumerator : IEnumerator<SyntaxNodeOrToken>, IEnumerator, IDisposable
	{
		private SyntaxNodeOrTokenList _list;

		private int _index;

		public SyntaxNodeOrToken Current => _list[_index];

		object IEnumerator.Current => Current;

		internal Enumerator(in SyntaxNodeOrTokenList list)
		{
			this = default;
			_list = list;
			_index = -1;
		}

		public bool MoveNext()
		{
			if (_index < _list.Count)
			{
				_index++;
			}
			return _index < _list.Count;
		}

		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		void IDisposable.Dispose()
		{
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

	private readonly SyntaxNode _node;

	internal readonly int index;

	internal SyntaxNode Node => _node;

	internal int Position => _node?.Position ?? 0;

	internal SyntaxNode Parent => _node?.Parent;

	public int Count
	{
		get
		{
			if (_node != null)
			{
				if (!_node.Green.IsList)
				{
					return 1;
				}
				return _node.SlotCount;
			}
			return 0;
		}
	}

	public SyntaxNodeOrToken this[int index]
	{
		get
		{
			if (_node != null)
			{
				if (!_node.IsList)
				{
					if (index == 0)
					{
						return _node;
					}
				}
				else if ((uint)index < (uint)_node.SlotCount)
				{
					GreenNode slot = _node.Green.GetSlot(index);
					if (slot.IsToken)
					{
						return new SyntaxToken(Parent, slot, _node.GetChildPosition(index), this.index + index);
					}
					return _node.GetNodeSlot(index);
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	public TextSpan FullSpan => _node?.FullSpan ?? default(TextSpan);

	public TextSpan Span => _node?.Span ?? default(TextSpan);

	private SyntaxNodeOrToken[] Nodes => this.ToArray();

	internal SyntaxNodeOrTokenList(SyntaxNode node, int index)
	{
		this = default;
		if (node != null)
		{
			_node = node;
			this.index = index;
		}
	}

	public SyntaxNodeOrTokenList(IEnumerable<SyntaxNodeOrToken> nodesAndTokens)
		: this(CreateNode(nodesAndTokens), 0)
	{
	}

	public SyntaxNodeOrTokenList(params SyntaxNodeOrToken[] nodesAndTokens)
		: this((IEnumerable<SyntaxNodeOrToken>)nodesAndTokens)
	{
	}

	private static SyntaxNode CreateNode(IEnumerable<SyntaxNodeOrToken> nodesAndTokens)
	{
		if (nodesAndTokens == null)
		{
			throw new ArgumentNullException("nodesAndTokens");
		}
		SyntaxNodeOrTokenListBuilder syntaxNodeOrTokenListBuilder = new SyntaxNodeOrTokenListBuilder(8);
		syntaxNodeOrTokenListBuilder.Add(nodesAndTokens);
		return syntaxNodeOrTokenListBuilder.ToList().Node;
	}

	public override string ToString()
	{
		if (_node == null)
		{
			return string.Empty;
		}
		return _node.ToString();
	}

	public string ToFullString()
	{
		if (_node == null)
		{
			return string.Empty;
		}
		return _node.ToFullString();
	}

	public SyntaxNodeOrToken First()
	{
		return this[0];
	}

	public SyntaxNodeOrToken FirstOrDefault()
	{
		if (!Any())
		{
			return default;
		}
		return this[0];
	}

	public SyntaxNodeOrToken Last()
	{
		return this[Count - 1];
	}

	public SyntaxNodeOrToken LastOrDefault()
	{
		if (!Any())
		{
			return default;
		}
		return this[Count - 1];
	}

	public int IndexOf(SyntaxNodeOrToken nodeOrToken)
	{
		int num = 0;
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current == nodeOrToken)
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	public bool Any()
	{
		return _node != null;
	}

	internal void CopyTo(int offset, GreenNode[] array, int arrayOffset, int count)
	{
		for (int i = 0; i < count; i++)
		{
			array[arrayOffset + i] = this[i + offset].UnderlyingNode;
		}
	}

	public SyntaxNodeOrTokenList Add(SyntaxNodeOrToken nodeOrToken)
	{
		return Insert(Count, nodeOrToken);
	}

	public SyntaxNodeOrTokenList AddRange(IEnumerable<SyntaxNodeOrToken> nodesOrTokens)
	{
		return InsertRange(Count, nodesOrTokens);
	}

	public SyntaxNodeOrTokenList Insert(int index, SyntaxNodeOrToken nodeOrToken)
	{
		if (nodeOrToken == default(SyntaxNodeOrToken))
		{
			throw new ArgumentOutOfRangeException("nodeOrToken");
		}
		return InsertRange(index, SpecializedCollections.SingletonEnumerable(nodeOrToken));
	}

	public SyntaxNodeOrTokenList InsertRange(int index, IEnumerable<SyntaxNodeOrToken> nodesAndTokens)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (nodesAndTokens == null)
		{
			throw new ArgumentNullException("nodesAndTokens");
		}
		if (nodesAndTokens.IsEmpty())
		{
			return this;
		}
		List<SyntaxNodeOrToken> list = this.ToList();
		list.InsertRange(index, nodesAndTokens);
		return CreateList(list[0].UnderlyingNode, list);
	}

	private static SyntaxNodeOrTokenList CreateList(GreenNode creator, List<SyntaxNodeOrToken> items)
	{
		if (items.Count == 0)
		{
			return default;
		}
		GreenNode greenNode = creator.CreateList(items.Select((SyntaxNodeOrToken n) => n.UnderlyingNode));
		if (greenNode.IsToken)
		{
			greenNode = creator.CreateList(new GreenNode[1] { greenNode }, alwaysCreateListNode: true);
		}
		return new SyntaxNodeOrTokenList(greenNode.CreateRed(), 0);
	}

	public SyntaxNodeOrTokenList RemoveAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		SyntaxNodeOrToken syntaxNodeOrToken = this[index];
		List<SyntaxNodeOrToken> list = this.ToList();
		list.RemoveAt(index);
		return CreateList(syntaxNodeOrToken.UnderlyingNode, list);
	}

	public SyntaxNodeOrTokenList Remove(SyntaxNodeOrToken nodeOrTokenInList)
	{
		int num = IndexOf(nodeOrTokenInList);
		if (num >= 0 && num < Count)
		{
			return RemoveAt(num);
		}
		return this;
	}

	public SyntaxNodeOrTokenList Replace(SyntaxNodeOrToken nodeOrTokenInList, SyntaxNodeOrToken newNodeOrToken)
	{
		if (newNodeOrToken == default(SyntaxNodeOrToken))
		{
			throw new ArgumentOutOfRangeException("newNodeOrToken");
		}
		return ReplaceRange(nodeOrTokenInList, new SyntaxNodeOrToken[1] { newNodeOrToken });
	}

	public SyntaxNodeOrTokenList ReplaceRange(SyntaxNodeOrToken nodeOrTokenInList, IEnumerable<SyntaxNodeOrToken> newNodesAndTokens)
	{
		int num = IndexOf(nodeOrTokenInList);
		if (num >= 0 && num < Count)
		{
			List<SyntaxNodeOrToken> list = this.ToList();
			list.RemoveAt(num);
			list.InsertRange(num, newNodesAndTokens);
			return CreateList(nodeOrTokenInList.UnderlyingNode, list);
		}
		throw new ArgumentOutOfRangeException("nodeOrTokenInList");
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<SyntaxNodeOrToken> IEnumerable<SyntaxNodeOrToken>.GetEnumerator()
	{
		if (_node != null)
		{
			return GetEnumerator();
		}
		return SpecializedCollections.EmptyEnumerator<SyntaxNodeOrToken>();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (_node != null)
		{
			return GetEnumerator();
		}
		return SpecializedCollections.EmptyEnumerator<SyntaxNodeOrToken>();
	}

	public static bool operator ==(SyntaxNodeOrTokenList left, SyntaxNodeOrTokenList right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SyntaxNodeOrTokenList left, SyntaxNodeOrTokenList right)
	{
		return !left.Equals(right);
	}

	public bool Equals(SyntaxNodeOrTokenList other)
	{
		return _node == other._node;
	}

	public override bool Equals(object obj)
	{
		if (obj is SyntaxNodeOrTokenList)
		{
			return Equals((SyntaxNodeOrTokenList)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _node?.GetHashCode() ?? 0;
	}
}
