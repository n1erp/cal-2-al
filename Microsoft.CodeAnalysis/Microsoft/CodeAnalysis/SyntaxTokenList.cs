using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

[StructLayout(LayoutKind.Auto)]
public readonly struct SyntaxTokenList : IEquatable<SyntaxTokenList>, IReadOnlyList<SyntaxToken>, IEnumerable<SyntaxToken>, IEnumerable, IReadOnlyCollection<SyntaxToken>
{
	[StructLayout(LayoutKind.Auto)]
	public struct Enumerator
	{
		private readonly SyntaxNode _parent;

		private readonly GreenNode _singleNodeOrList;

		private readonly int _baseIndex;

		private readonly int _count;

		private int _index;

		private GreenNode _current;

		private int _position;

		public SyntaxToken Current
		{
			get
			{
				if (_current == null)
				{
					throw new InvalidOperationException();
				}
				return new SyntaxToken(_parent, _current, _position, _baseIndex + _index);
			}
		}

		internal Enumerator(in SyntaxTokenList list)
		{
			_parent = list._parent;
			_singleNodeOrList = list.Node;
			_baseIndex = list._index;
			_count = list.Count;
			_index = -1;
			_current = null;
			_position = list.Position;
		}

		public bool MoveNext()
		{
			if (_count == 0 || _count <= _index + 1)
			{
				_current = null;
				return false;
			}
			_index++;
			if (_current != null)
			{
				_position += _current.FullWidth;
			}
			_current = GetGreenNodeAt(_singleNodeOrList, _index);
			return true;
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

	private class EnumeratorImpl : IEnumerator<SyntaxToken>, IEnumerator, IDisposable
	{
		private Enumerator _enumerator;

		public SyntaxToken Current => _enumerator.Current;

		object IEnumerator.Current => _enumerator.Current;

		internal EnumeratorImpl(in SyntaxTokenList list)
		{
			_enumerator = new Enumerator(in list);
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		public void Dispose()
		{
		}
	}

	public readonly struct Reversed(SyntaxTokenList list) : IEnumerable<SyntaxToken>, IEnumerable, IEquatable<Reversed>
	{
		[StructLayout(LayoutKind.Auto)]
		public struct Enumerator
		{
			private readonly SyntaxNode _parent;

			private readonly GreenNode _singleNodeOrList;

			private readonly int _baseIndex;

			private readonly int _count;

			private int _index;

			private GreenNode _current;

			private int _position;

			public SyntaxToken Current
			{
				get
				{
					if (_current == null)
					{
						throw new InvalidOperationException();
					}
					return new SyntaxToken(_parent, _current, _position, _baseIndex + _index);
				}
			}

			internal Enumerator(in SyntaxTokenList list)
			{
				this = default;
				if (list.Any())
				{
					_parent = list._parent;
					_singleNodeOrList = list.Node;
					_baseIndex = list._index;
					_count = list.Count;
					_index = _count;
					_current = null;
					SyntaxToken syntaxToken = list.Last();
					_position = syntaxToken.Position + syntaxToken.FullWidth;
				}
			}

			public bool MoveNext()
			{
				if (_count == 0 || _index <= 0)
				{
					_current = null;
					return false;
				}
				_index--;
				_current = GetGreenNodeAt(_singleNodeOrList, _index);
				_position -= _current.FullWidth;
				return true;
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

		private class EnumeratorImpl : IEnumerator<SyntaxToken>, IEnumerator, IDisposable
		{
			private Enumerator _enumerator;

			public SyntaxToken Current => _enumerator.Current;

			object IEnumerator.Current => _enumerator.Current;

			internal EnumeratorImpl(in SyntaxTokenList list)
			{
				_enumerator = new Enumerator(in list);
			}

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}

			public void Dispose()
			{
			}
		}

		private readonly SyntaxTokenList _list = list;

		public Enumerator GetEnumerator()
		{
			return new Enumerator(in _list);
		}

		IEnumerator<SyntaxToken> IEnumerable<SyntaxToken>.GetEnumerator()
		{
			if (_list.Count == 0)
			{
				return SpecializedCollections.EmptyEnumerator<SyntaxToken>();
			}
			return new EnumeratorImpl(in _list);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (_list.Count == 0)
			{
				return SpecializedCollections.EmptyEnumerator<SyntaxToken>();
			}
			return new EnumeratorImpl(in _list);
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
			return _list.Equals(other._list);
		}

		public override int GetHashCode()
		{
			return _list.GetHashCode();
		}
	}

	private readonly SyntaxNode _parent;

	private readonly int _index;

	internal GreenNode Node { get; }

	internal int Position { get; }

	public int Count
	{
		get
		{
			if (Node != null)
			{
				if (!Node.IsList)
				{
					return 1;
				}
				return Node.SlotCount;
			}
			return 0;
		}
	}

	public SyntaxToken this[int index]
	{
		get
		{
			if (Node != null)
			{
				if (Node.IsList)
				{
					if ((uint)index < (uint)Node.SlotCount)
					{
						return new SyntaxToken(_parent, Node.GetSlot(index), Position + Node.GetSlotOffset(index), _index + index);
					}
				}
				else if (index == 0)
				{
					return new SyntaxToken(_parent, Node, Position, _index);
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	public TextSpan FullSpan
	{
		get
		{
			if (Node == null)
			{
				return default;
			}
			return new TextSpan(Position, Node.FullWidth);
		}
	}

	public TextSpan Span
	{
		get
		{
			if (Node == null)
			{
				return default;
			}
			return TextSpan.FromBounds(Position + Node.GetLeadingTriviaWidth(), Position + Node.FullWidth - Node.GetTrailingTriviaWidth());
		}
	}

	private SyntaxToken[] Nodes => this.ToArray();

	internal SyntaxTokenList(SyntaxNode parent, GreenNode tokenOrList, int position, int index)
	{
		_parent = parent;
		Node = tokenOrList;
		Position = position;
		_index = index;
	}

	public SyntaxTokenList(SyntaxToken token)
	{
		_parent = token.Parent;
		Node = token.Node;
		Position = token.Position;
		_index = 0;
	}

	public SyntaxTokenList(params SyntaxToken[] tokens)
		: this(null, CreateNode(tokens), 0, 0)
	{
	}

	public SyntaxTokenList(IEnumerable<SyntaxToken> tokens)
		: this(null, CreateNode(tokens), 0, 0)
	{
	}

	private static GreenNode CreateNode(SyntaxToken[] tokens)
	{
		if (tokens == null)
		{
			return null;
		}
		SyntaxTokenListBuilder syntaxTokenListBuilder = new SyntaxTokenListBuilder(tokens.Length);
		for (int i = 0; i < tokens.Length; i++)
		{
			syntaxTokenListBuilder.Add(tokens[i].Node);
		}
		return syntaxTokenListBuilder.ToList().Node;
	}

	private static GreenNode CreateNode(IEnumerable<SyntaxToken> tokens)
	{
		if (tokens == null)
		{
			return null;
		}
		SyntaxTokenListBuilder syntaxTokenListBuilder = SyntaxTokenListBuilder.Create();
		foreach (SyntaxToken token in tokens)
		{
			syntaxTokenListBuilder.Add(token.Node);
		}
		return syntaxTokenListBuilder.ToList().Node;
	}

	public override string ToString()
	{
		if (Node == null)
		{
			return string.Empty;
		}
		return Node.ToString();
	}

	public string ToFullString()
	{
		if (Node == null)
		{
			return string.Empty;
		}
		return Node.ToFullString();
	}

	public SyntaxToken First()
	{
		if (Any())
		{
			return this[0];
		}
		throw new InvalidOperationException();
	}

	public SyntaxToken Last()
	{
		if (Any())
		{
			return this[Count - 1];
		}
		throw new InvalidOperationException();
	}

	public bool Any()
	{
		return Node != null;
	}

	public Reversed Reverse()
	{
		return new Reversed(this);
	}

	internal void CopyTo(int offset, GreenNode[] array, int arrayOffset, int count)
	{
		for (int i = 0; i < count; i++)
		{
			array[arrayOffset + i] = GetGreenNodeAt(offset + i);
		}
	}

	private GreenNode GetGreenNodeAt(int i)
	{
		return GetGreenNodeAt(Node, i);
	}

	private static GreenNode GetGreenNodeAt(GreenNode node, int i)
	{
		if (!node.IsList)
		{
			return node;
		}
		return node.GetSlot(i);
	}

	public int IndexOf(SyntaxToken tokenInList)
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			if (this[i] == tokenInList)
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

	public SyntaxTokenList Add(SyntaxToken token)
	{
		return Insert(Count, token);
	}

	public SyntaxTokenList AddRange(IEnumerable<SyntaxToken> tokens)
	{
		return InsertRange(Count, tokens);
	}

	public SyntaxTokenList Insert(int index, SyntaxToken token)
	{
		if (token == default(SyntaxToken))
		{
			throw new ArgumentOutOfRangeException("token");
		}
		return InsertRange(index, new SyntaxToken[1] { token });
	}

	public SyntaxTokenList InsertRange(int index, IEnumerable<SyntaxToken> tokens)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (tokens == null)
		{
			throw new ArgumentNullException("tokens");
		}
		if (tokens.ToList().Count == 0)
		{
			return this;
		}
		List<SyntaxToken> list = this.ToList();
		list.InsertRange(index, tokens);
		if (list.Count == 0)
		{
			return this;
		}
		return new SyntaxTokenList(null, list[0].Node.CreateList(list.Select((SyntaxToken n) => n.Node)), 0, 0);
	}

	public SyntaxTokenList RemoveAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		List<SyntaxToken> list = this.ToList();
		list.RemoveAt(index);
		return new SyntaxTokenList(null, Node.CreateList(list.Select((SyntaxToken n) => n.Node)), 0, 0);
	}

	public SyntaxTokenList Remove(SyntaxToken tokenInList)
	{
		int num = IndexOf(tokenInList);
		if (num >= 0 && num <= Count)
		{
			return RemoveAt(num);
		}
		return this;
	}

	public SyntaxTokenList Replace(SyntaxToken tokenInList, SyntaxToken newToken)
	{
		if (newToken == default(SyntaxToken))
		{
			throw new ArgumentOutOfRangeException("newToken");
		}
		return ReplaceRange(tokenInList, new SyntaxToken[1] { newToken });
	}

	public SyntaxTokenList ReplaceRange(SyntaxToken tokenInList, IEnumerable<SyntaxToken> newTokens)
	{
		int num = IndexOf(tokenInList);
		if (num >= 0 && num <= Count)
		{
			List<SyntaxToken> list = this.ToList();
			list.RemoveAt(num);
			list.InsertRange(num, newTokens);
			return new SyntaxTokenList(null, Node.CreateList(list.Select((SyntaxToken n) => n.Node)), 0, 0);
		}
		throw new ArgumentOutOfRangeException("tokenInList");
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<SyntaxToken> IEnumerable<SyntaxToken>.GetEnumerator()
	{
		if (Node == null)
		{
			return SpecializedCollections.EmptyEnumerator<SyntaxToken>();
		}
		return new EnumeratorImpl(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (Node == null)
		{
			return SpecializedCollections.EmptyEnumerator<SyntaxToken>();
		}
		return new EnumeratorImpl(this);
	}

	public static bool operator ==(SyntaxTokenList left, SyntaxTokenList right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SyntaxTokenList left, SyntaxTokenList right)
	{
		return !left.Equals(right);
	}

	public bool Equals(SyntaxTokenList other)
	{
		if (Node == other.Node && _parent == other._parent)
		{
			return _index == other._index;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SyntaxTokenList)
		{
			return Equals((SyntaxTokenList)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Node, _index);
	}

	public static SyntaxTokenList Create(SyntaxToken token)
	{
		return new SyntaxTokenList(token);
	}
}
