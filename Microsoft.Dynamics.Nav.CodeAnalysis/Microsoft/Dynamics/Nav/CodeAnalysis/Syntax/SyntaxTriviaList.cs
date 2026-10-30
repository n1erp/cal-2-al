using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[StructLayout(LayoutKind.Auto)]
[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
public struct SyntaxTriviaList : IEquatable<SyntaxTriviaList>, IReadOnlyList<SyntaxTrivia>, IReadOnlyCollection<SyntaxTrivia>, IEnumerable<SyntaxTrivia>, IEnumerable
{
	[StructLayout(LayoutKind.Auto)]
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
	public struct Enumerator
	{
		private SyntaxToken token;

		private InternalSyntaxNode singleNodeOrList;

		private int baseIndex;

		private int count;

		private int index;

		private InternalSyntaxNode current;

		private int position;

		public SyntaxTrivia Current
		{
			get
			{
				if (current == null)
				{
					throw new InvalidOperationException();
				}
				return new SyntaxTrivia(token, current, position, baseIndex + index);
			}
		}

		internal Enumerator(ref SyntaxTriviaList list)
		{
			token = list.Token;
			singleNodeOrList = list.Node;
			baseIndex = list.Index;
			count = list.Count;
			index = -1;
			current = null;
			position = list.Position;
		}

		internal void InitializeFrom(ref SyntaxToken initToken, InternalSyntaxNode internalSyntaxNode, int initIndex, int initPosition)
		{
			token = initToken;
			singleNodeOrList = internalSyntaxNode;
			baseIndex = initIndex;
			count = ((!internalSyntaxNode.IsList) ? 1 : internalSyntaxNode.SlotCount);
			index = -1;
			current = null;
			position = initPosition;
		}

		internal void InitializeFromLeadingTrivia(ref SyntaxToken initToken)
		{
			InitializeFrom(ref initToken, initToken.Node.GetLeadingTrivia(), 0, initToken.Position);
		}

		internal void InitializeFromTrailingTrivia(ref SyntaxToken initToken)
		{
			InternalSyntaxNode leadingTrivia = initToken.Node.GetLeadingTrivia();
			int initIndex = 0;
			if (leadingTrivia != null)
			{
				initIndex = ((!leadingTrivia.IsList) ? 1 : leadingTrivia.SlotCount);
			}
			InternalSyntaxNode trailingTrivia = initToken.Node.GetTrailingTrivia();
			int num = initToken.Position + initToken.FullWidth;
			if (trailingTrivia != null)
			{
				num -= trailingTrivia.FullWidth;
			}
			InitializeFrom(ref initToken, trailingTrivia, initIndex, num);
		}

		public bool MoveNext()
		{
			int num = index + 1;
			if (num >= count)
			{
				current = null;
				return false;
			}
			index = num;
			if (current != null)
			{
				position += current.FullWidth;
			}
			current = GetGreenNodeAt(singleNodeOrList, num);
			return true;
		}

		internal bool TryMoveNextAndGetCurrent(ref SyntaxTrivia resultCurrent)
		{
			if (!MoveNext())
			{
				return false;
			}
			resultCurrent = new SyntaxTrivia(token, current, position, baseIndex + index);
			return true;
		}
	}

	private class EnumeratorImpl : IEnumerator<SyntaxTrivia>, IDisposable, IEnumerator
	{
		private Enumerator syntaxTriviaListEnumerator;

		public SyntaxTrivia Current => syntaxTriviaListEnumerator.Current;

		object IEnumerator.Current => syntaxTriviaListEnumerator.Current;

		internal EnumeratorImpl(ref SyntaxTriviaList list)
		{
			syntaxTriviaListEnumerator = new Enumerator(ref list);
		}

		public bool MoveNext()
		{
			return syntaxTriviaListEnumerator.MoveNext();
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		public void Dispose()
		{
		}
	}

	public struct Reversed(SyntaxTriviaList list) : IEnumerable<SyntaxTrivia>, IEnumerable, IEquatable<Reversed>
	{
		[StructLayout(LayoutKind.Auto)]
		public struct Enumerator
		{
			private readonly SyntaxToken _token;

			private readonly InternalSyntaxNode _singleNodeOrList;

			private readonly int _baseIndex;

			private readonly int _count;

			private int _index;

			private InternalSyntaxNode _current;

			private int _position;

			public SyntaxTrivia Current
			{
				get
				{
					if (_current == null)
					{
						throw new InvalidOperationException();
					}
					return new SyntaxTrivia(_token, _current, _position, _baseIndex + _index);
				}
			}

			public Enumerator(ref SyntaxTriviaList list)
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

		private class ReversedEnumeratorImpl : IEnumerator<SyntaxTrivia>, IDisposable, IEnumerator
		{
			private Enumerator enumerator;

			public SyntaxTrivia Current => enumerator.Current;

			object IEnumerator.Current => enumerator.Current;

			internal ReversedEnumeratorImpl(ref SyntaxTriviaList list)
			{
				enumerator = new Enumerator(ref list);
			}

			public bool MoveNext()
			{
				return enumerator.MoveNext();
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}

			public void Dispose()
			{
			}
		}

		private SyntaxTriviaList list = list;

		public Enumerator GetEnumerator()
		{
			return new Enumerator(ref list);
		}

		IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator()
		{
			if (list.Count == 0)
			{
				return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
			}
			return new ReversedEnumeratorImpl(ref list);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (list.Count == 0)
			{
				return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
			}
			return new ReversedEnumeratorImpl(ref list);
		}

		public override int GetHashCode()
		{
			return list.GetHashCode();
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
			return list.Equals(other.list);
		}
	}

	public static readonly SyntaxTriviaList Empty;

	internal SyntaxToken Token { get; }

	internal InternalSyntaxNode Node { get; }

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

	internal SyntaxTriviaList(SyntaxToken token, InternalSyntaxNode node, int position, int index = 0)
	{
		Token = token;
		Node = node;
		Position = position;
		Index = index;
	}

	internal SyntaxTriviaList(SyntaxToken token, InternalSyntaxNode node)
	{
		Token = token;
		Node = node;
		Position = token.Position;
		Index = 0;
	}

	internal SyntaxTriviaList(SyntaxTrivia trivia)
	{
		Token = default;
		Node = trivia.UnderlyingNode;
		Position = 0;
		Index = 0;
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
		return new Enumerator(ref this);
	}

	IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator()
	{
		if (Node == null)
		{
			return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
		}
		return new EnumeratorImpl(ref this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (Node == null)
		{
			return SpecializedCollections.EmptyEnumerator<SyntaxTrivia>();
		}
		return new EnumeratorImpl(ref this);
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
			if (this[i].Kind == (SyntaxKind)rawKind)
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

	public SyntaxTriviaList InsertRange(int index, IEnumerable<SyntaxTrivia> trivia)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		List<SyntaxTrivia> list = trivia.ToList();
		if (list.Count == 0)
		{
			return this;
		}
		List<SyntaxTrivia> list2 = this.ToList();
		list2.InsertRange(index, list);
		if (list2.Count == 0)
		{
			return this;
		}
		return new SyntaxTriviaList(default, list2[0].UnderlyingNode.CreateList(list2.Select((SyntaxTrivia n) => n.UnderlyingNode)), 0);
	}

	public SyntaxTriviaList RemoveAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		List<SyntaxTrivia> list = this.ToList();
		list.RemoveAt(index);
		return new SyntaxTriviaList(default, Node.CreateList(list.Select((SyntaxTrivia n) => n.UnderlyingNode)), 0);
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
			return new SyntaxTriviaList(default, Node.CreateList(list.Select((SyntaxTrivia n) => n.UnderlyingNode)), 0);
		}
		throw new ArgumentOutOfRangeException("triviaInList");
	}

	private InternalSyntaxNode GetGreenNodeAt(int i)
	{
		return GetGreenNodeAt(Node, i);
	}

	private static InternalSyntaxNode GetGreenNodeAt(InternalSyntaxNode node, int i)
	{
		DebugAssertHelper.Assert(node.IsList || (i == 0 && !node.IsList));
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
		return Token.GetHashCode() ^ Node.GetHashCode() ^ Index.GetHashCode();
	}

	internal void CopyTo(int offset, SyntaxTrivia[] array, int arrayOffset, int count)
	{
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0 || Count < offset + count)
		{
			throw new ArgumentOutOfRangeException("count");
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
}
