using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

[StructLayout(LayoutKind.Auto)]
public readonly struct SyntaxTriviaList : IEquatable<SyntaxTriviaList>, IReadOnlyList<SyntaxTrivia>, IEnumerable<SyntaxTrivia>, IEnumerable, IReadOnlyCollection<SyntaxTrivia>
{
	[StructLayout(LayoutKind.Auto)]
	public struct Enumerator
	{
		private SyntaxToken _token;

		private GreenNode _singleNodeOrList;

		private int _baseIndex;

		private int _count;

		private int _index;

		private GreenNode _current;

		private int _position;

		public SyntaxTrivia Current
		{
			get
			{
				if (_current == null)
				{
					throw new InvalidOperationException();
				}
				return new SyntaxTrivia(in _token, _current, _position, _baseIndex + _index);
			}
		}

		internal Enumerator(in SyntaxTriviaList list)
		{
			_token = list.Token;
			_singleNodeOrList = list.Node;
			_baseIndex = list.Index;
			_count = list.Count;
			_index = -1;
			_current = null;
			_position = list.Position;
		}

		private void InitializeFrom(in SyntaxToken token, GreenNode greenNode, int index, int position)
		{
			_token = token;
			_singleNodeOrList = greenNode;
			_baseIndex = index;
			_count = ((!greenNode.IsList) ? 1 : greenNode.SlotCount);
			_index = -1;
			_current = null;
			_position = position;
		}

		internal void InitializeFromLeadingTrivia(in SyntaxToken token)
		{
			InitializeFrom(in token, token.Node.GetLeadingTriviaCore(), 0, token.Position);
		}

		internal void InitializeFromTrailingTrivia(in SyntaxToken token)
		{
			GreenNode leadingTriviaCore = token.Node.GetLeadingTriviaCore();
			int index = 0;
			if (leadingTriviaCore != null)
			{
				index = ((!leadingTriviaCore.IsList) ? 1 : leadingTriviaCore.SlotCount);
			}
			GreenNode trailingTriviaCore = token.Node.GetTrailingTriviaCore();
			int num = token.Position + token.FullWidth;
			if (trailingTriviaCore != null)
			{
				num -= trailingTriviaCore.FullWidth;
			}
			InitializeFrom(in token, trailingTriviaCore, index, num);
		}

		public bool MoveNext()
		{
			int num = _index + 1;
			if (num >= _count)
			{
				_current = null;
				return false;
			}
			_index = num;
			if (_current != null)
			{
				_position += _current.FullWidth;
			}
			_current = GetGreenNodeAt(_singleNodeOrList, num);
			return true;
		}

		internal bool TryMoveNextAndGetCurrent(out SyntaxTrivia current)
		{
			if (!MoveNext())
			{
				current = default;
				return false;
			}
			current = new SyntaxTrivia(in _token, _current, _position, _baseIndex + _index);
			return true;
		}
	}

	private class EnumeratorImpl : IEnumerator<SyntaxTrivia>, IEnumerator, IDisposable
	{
		private Enumerator _enumerator;

		public SyntaxTrivia Current => _enumerator.Current;

		object IEnumerator.Current => _enumerator.Current;

		internal EnumeratorImpl(in SyntaxTriviaList list)
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

	public readonly struct Reversed(SyntaxTriviaList list) : IEnumerable<SyntaxTrivia>, IEnumerable, IEquatable<Reversed>
	{
		[StructLayout(LayoutKind.Auto)]
		public struct Enumerator
		{
			private readonly SyntaxToken _token;

			private readonly GreenNode _singleNodeOrList;

			private readonly int _baseIndex;

			private readonly int _count;

			private int _index;

			private GreenNode _current;

			private int _position;

			public SyntaxTrivia Current
			{
				get
				{
					if (_current == null)
					{
						throw new InvalidOperationException();
					}
					return new SyntaxTrivia(in _token, _current, _position, _baseIndex + _index);
				}
			}

			internal Enumerator(in SyntaxTriviaList list)
			{
				this = default;
				if (list.Any())
				{
					_token = list.Token;
					_singleNodeOrList = list.Node;
					_baseIndex = list.Index;
					_count = list.Count;
					_index = _count;
					_current = null;
					SyntaxTrivia syntaxTrivia = list.Last();
					_position = syntaxTrivia.Position + syntaxTrivia.FullWidth;
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
		}

		private class ReversedEnumeratorImpl : IEnumerator<SyntaxTrivia>, IEnumerator, IDisposable
		{
			private Enumerator _enumerator;

			public SyntaxTrivia Current => _enumerator.Current;

			object IEnumerator.Current => _enumerator.Current;

			internal ReversedEnumeratorImpl(in SyntaxTriviaList list)
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

		private readonly SyntaxTriviaList _list = list;

		public Enumerator GetEnumerator()
		{
			return new Enumerator(in _list);
		}

		IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator()
		{
			if (_list.Count == 0)
			{
				return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
			}
			return new ReversedEnumeratorImpl(in _list);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (_list.Count == 0)
			{
				return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
			}
			return new ReversedEnumeratorImpl(in _list);
		}

		public override int GetHashCode()
		{
			return _list.GetHashCode();
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
	}

	private static readonly ObjectPool<SyntaxTriviaListBuilder> s_builderPool = new ObjectPool<SyntaxTriviaListBuilder>(() => SyntaxTriviaListBuilder.Create());

	public static SyntaxTriviaList Empty => default;

	internal SyntaxToken Token { get; }

	internal GreenNode Node { get; }

	internal int Position { get; }

	internal int Index { get; }

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

	public SyntaxTrivia this[int index]
	{
		get
		{
			if (Node != null)
			{
				if (Node.IsList)
				{
					if ((uint)index < (uint)Node.SlotCount)
					{
						return new SyntaxTrivia(Token, Node.GetSlot(index), Position + Node.GetSlotOffset(index), Index + index);
					}
				}
				else if (index == 0)
				{
					return new SyntaxTrivia(Token, Node, Position, Index);
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

	private SyntaxTrivia[] Nodes => this.ToArray();

	internal SyntaxTriviaList(in SyntaxToken token, GreenNode node, int position, int index = 0)
	{
		Token = token;
		Node = node;
		Position = position;
		Index = index;
	}

	internal SyntaxTriviaList(in SyntaxToken token, GreenNode node)
	{
		Token = token;
		Node = node;
		Position = token.Position;
		Index = 0;
	}

	public SyntaxTriviaList(SyntaxTrivia trivia)
	{
		Token = default;
		Node = trivia.UnderlyingNode;
		Position = 0;
		Index = 0;
	}

	public SyntaxTriviaList(params SyntaxTrivia[] trivias)
		: this(default(SyntaxToken), CreateNode(trivias), 0)
	{
	}

	public SyntaxTriviaList(IEnumerable<SyntaxTrivia> trivias)
		: this(default(SyntaxToken), SyntaxTriviaListBuilder.Create(trivias).Node, 0)
	{
	}

	private static GreenNode CreateNode(SyntaxTrivia[] trivias)
	{
		if (trivias == null)
		{
			return null;
		}
		SyntaxTriviaListBuilder syntaxTriviaListBuilder = new SyntaxTriviaListBuilder(trivias.Length);
		syntaxTriviaListBuilder.Add(trivias);
		return syntaxTriviaListBuilder.ToList().Node;
	}

	public SyntaxTrivia ElementAt(int index)
	{
		return this[index];
	}

	public SyntaxTrivia First()
	{
		if (Any())
		{
			return this[0];
		}
		throw new InvalidOperationException();
	}

	public SyntaxTrivia Last()
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

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public int IndexOf(SyntaxTrivia triviaInList)
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			if (this[i] == triviaInList)
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

	public SyntaxTriviaList Add(SyntaxTrivia trivia)
	{
		return Insert(Count, trivia);
	}

	public SyntaxTriviaList AddRange(IEnumerable<SyntaxTrivia> trivia)
	{
		return InsertRange(Count, trivia);
	}

	public SyntaxTriviaList Insert(int index, SyntaxTrivia trivia)
	{
		if (trivia == default(SyntaxTrivia))
		{
			throw new ArgumentOutOfRangeException("trivia");
		}
		return InsertRange(index, new SyntaxTrivia[1] { trivia });
	}

	private static SyntaxTriviaListBuilder GetBuilder()
	{
		return s_builderPool.Allocate();
	}

	private static void ClearAndFreeBuilder(SyntaxTriviaListBuilder builder)
	{
		if (builder.Count <= 16)
		{
			builder.Clear();
			s_builderPool.Free(builder);
		}
	}

	public SyntaxTriviaList InsertRange(int index, IEnumerable<SyntaxTrivia> trivia)
	{
		int count = Count;
		if (index < 0 || index > count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (trivia == null)
		{
			throw new ArgumentNullException("trivia");
		}
		if (trivia is ICollection<SyntaxTrivia> { Count: 0 })
		{
			return this;
		}
		SyntaxTriviaListBuilder builder = GetBuilder();
		try
		{
			for (int i = 0; i < index; i++)
			{
				builder.Add(this[i]);
			}
			builder.AddRange(trivia);
			for (int j = index; j < count; j++)
			{
				builder.Add(this[j]);
			}
			return (builder.Count == count) ? this : builder.ToList();
		}
		finally
		{
			ClearAndFreeBuilder(builder);
		}
	}

	public SyntaxTriviaList RemoveAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		List<SyntaxTrivia> list = this.ToList();
		list.RemoveAt(index);
		return new SyntaxTriviaList(default(SyntaxToken), Node.CreateList(list.Select((SyntaxTrivia n) => n.UnderlyingNode)), 0);
	}

	public SyntaxTriviaList Remove(SyntaxTrivia triviaInList)
	{
		int num = IndexOf(triviaInList);
		if (num >= 0 && num < Count)
		{
			return RemoveAt(num);
		}
		return this;
	}

	public SyntaxTriviaList Replace(SyntaxTrivia triviaInList, SyntaxTrivia newTrivia)
	{
		if (newTrivia == default(SyntaxTrivia))
		{
			throw new ArgumentOutOfRangeException("newTrivia");
		}
		return ReplaceRange(triviaInList, new SyntaxTrivia[1] { newTrivia });
	}

	public SyntaxTriviaList ReplaceRange(SyntaxTrivia triviaInList, IEnumerable<SyntaxTrivia> newTrivia)
	{
		int num = IndexOf(triviaInList);
		if (num >= 0 && num < Count)
		{
			List<SyntaxTrivia> list = this.ToList();
			list.RemoveAt(num);
			list.InsertRange(num, newTrivia);
			return new SyntaxTriviaList(default(SyntaxToken), Node.CreateList(list.Select((SyntaxTrivia n) => n.UnderlyingNode)), 0);
		}
		throw new ArgumentOutOfRangeException("triviaInList");
	}

	IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator()
	{
		if (Node == null)
		{
			return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
		}
		return new EnumeratorImpl(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (Node == null)
		{
			return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
		}
		return new EnumeratorImpl(this);
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

	public bool Equals(SyntaxTriviaList other)
	{
		if (Node == other.Node && Index == other.Index)
		{
			return Token.Equals(other.Token);
		}
		return false;
	}

	public static bool operator ==(SyntaxTriviaList left, SyntaxTriviaList right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SyntaxTriviaList left, SyntaxTriviaList right)
	{
		return !left.Equals(right);
	}

	public override bool Equals(object obj)
	{
		if (obj is SyntaxTriviaList)
		{
			return Equals((SyntaxTriviaList)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Token.GetHashCode(), Hash.Combine(Node, Index));
	}

	internal void CopyTo(int offset, SyntaxTrivia[] array, int arrayOffset, int count)
	{
		if (offset < 0 || count < 0 || Count < offset + count)
		{
			throw new IndexOutOfRangeException();
		}
		if (count != 0)
		{
			SyntaxTrivia syntaxTrivia = (array[arrayOffset] = this[offset]);
			int num = syntaxTrivia.Position;
			SyntaxTrivia syntaxTrivia2 = syntaxTrivia;
			for (int i = 1; i < count; i++)
			{
				num += syntaxTrivia2.FullWidth;
				syntaxTrivia2 = (array[arrayOffset + i] = new SyntaxTrivia(Token, GetGreenNodeAt(offset + i), num, Index + i));
			}
		}
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

	public static SyntaxTriviaList Create(SyntaxTrivia trivia)
	{
		return new SyntaxTriviaList(trivia);
	}
}
