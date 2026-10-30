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
public struct SyntaxTokenList : IEquatable<SyntaxTokenList>, IReadOnlyList<SyntaxToken>, IReadOnlyCollection<SyntaxToken>, IEnumerable<SyntaxToken>, IEnumerable
{
	[StructLayout(LayoutKind.Auto)]
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
	[SuppressMessage("Performance", "RS0008", Justification = "Equality not actually implemented")]
	public struct Enumerator
	{
		private readonly SyntaxNode parent;

		private readonly InternalSyntaxNode singleNodeOrList;

		private readonly int baseIndex;

		private readonly int count;

		private int index;

		private InternalSyntaxNode current;

		private int position;

		public SyntaxToken Current
		{
			get
			{
				if (current == null)
				{
					throw new InvalidOperationException();
				}
				return new SyntaxToken(parent, current, position, baseIndex + index);
			}
		}

		internal Enumerator(ref SyntaxTokenList list)
		{
			parent = list.parent;
			singleNodeOrList = list.Node;
			baseIndex = list.indexInParent;
			count = list.Count;
			index = -1;
			current = null;
			position = list.Position;
		}

		public bool MoveNext()
		{
			if (count == 0 || count <= index + 1)
			{
				current = null;
				return false;
			}
			index++;
			if (current != null)
			{
				position += current.FullWidth;
			}
			current = GetGreenNodeAt(singleNodeOrList, index);
			DebugAssertHelper.Assert(current != null);
			return true;
		}
	}

	private class EnumeratorImpl : IEnumerator<SyntaxToken>, IDisposable, IEnumerator
	{
		private Enumerator enumerator;

		public SyntaxToken Current => enumerator.Current;

		object IEnumerator.Current => enumerator.Current;

		internal EnumeratorImpl(ref SyntaxTokenList list)
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

	private readonly SyntaxNode parent;

	private readonly int indexInParent;

	internal InternalSyntaxNode Node { get; }

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
						return new SyntaxToken(parent, Node.GetSlot(index), Position + Node.GetSlotOffset(index), indexInParent + index);
					}
				}
				else if (index == 0)
				{
					return new SyntaxToken(parent, Node, Position, indexInParent);
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

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private SyntaxToken[] Nodes => this.ToArray();

	internal SyntaxTokenList(SyntaxNode parent, InternalSyntaxNode tokenOrList, int position, int index)
	{
		DebugAssertHelper.Assert(tokenOrList != null || (position == 0 && index == 0 && parent == null));
		DebugAssertHelper.Assert(position >= 0);
		DebugAssertHelper.Assert(tokenOrList == null || tokenOrList.IsToken || tokenOrList.IsList);
		this.parent = parent;
		Node = tokenOrList;
		Position = position;
		indexInParent = index;
	}

	internal SyntaxTokenList(SyntaxToken token)
	{
		parent = token.Parent;
		Node = token.Node;
		Position = token.Position;
		indexInParent = 0;
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

	internal void CopyTo(int offset, InternalSyntaxNode[] array, int arrayOffset, int count)
	{
		DebugAssertHelper.Assert(Count >= offset + count);
		for (int i = 0; i < count; i++)
		{
			array[arrayOffset + i] = GetGreenNodeAt(offset + i);
		}
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

	internal int IndexOf(SyntaxKind rawKind)
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			if (this[i].Kind == rawKind)
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

	public SyntaxTokenList Insert(int insertIndex, SyntaxToken token)
	{
		if (token == default(SyntaxToken))
		{
			throw new ArgumentOutOfRangeException("token");
		}
		return InsertRange(insertIndex, new SyntaxToken[1] { token });
	}

	public SyntaxTokenList InsertRange(int insertIndex, IEnumerable<SyntaxToken> tokens)
	{
		if (insertIndex < 0 || insertIndex > Count)
		{
			throw new ArgumentOutOfRangeException("insertIndex");
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
		list.InsertRange(insertIndex, tokens);
		if (list.Count == 0)
		{
			return this;
		}
		return new SyntaxTokenList(null, list[0].Node.CreateList(list.Select((SyntaxToken n) => n.Node)), 0, 0);
	}

	public SyntaxTokenList RemoveAt(int removeIndex)
	{
		if (removeIndex < 0 || removeIndex >= Count)
		{
			throw new ArgumentOutOfRangeException("removeIndex");
		}
		List<SyntaxToken> list = this.ToList();
		list.RemoveAt(removeIndex);
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
		return new Enumerator(ref this);
	}

	IEnumerator<SyntaxToken> IEnumerable<SyntaxToken>.GetEnumerator()
	{
		if (Node == null)
		{
			return SpecializedCollections.EmptyEnumerator<SyntaxToken>();
		}
		return new EnumeratorImpl(ref this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (Node == null)
		{
			return SpecializedCollections.EmptyEnumerator<SyntaxToken>();
		}
		return new EnumeratorImpl(ref this);
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
		if (Node == other.Node && parent == other.parent)
		{
			return indexInParent == other.indexInParent;
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
		return Hash.Combine(Node.GetHashCode(), indexInParent.GetHashCode());
	}

	public static SyntaxTokenList Create(SyntaxToken token)
	{
		return new SyntaxTokenList(token);
	}
}
