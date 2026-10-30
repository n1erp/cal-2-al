using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
public struct SyntaxNodeOrTokenList : IEquatable<SyntaxNodeOrTokenList>, IReadOnlyCollection<SyntaxNodeOrToken>, IEnumerable<SyntaxNodeOrToken>, IEnumerable
{
	[SuppressMessage("Performance", "RS0008", Justification = "Equality not actually implemented")]
	public struct Enumerator : IEnumerator<SyntaxNodeOrToken>, IDisposable, IEnumerator
	{
		private readonly SyntaxNodeOrTokenList list;

		private int index;

		public SyntaxNodeOrToken Current => list[index];

		object IEnumerator.Current => Current;

		internal Enumerator(SyntaxNodeOrTokenList list)
		{
			this = default;
			this.list = list;
			index = -1;
		}

		public bool MoveNext()
		{
			if (index < list.Count)
			{
				index++;
			}
			return index < list.Count;
		}

		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		public void Dispose()
		{
		}
	}

	private readonly SyntaxNode node;

	internal readonly int IndexInParentList;

	internal SyntaxNode Node => node;

	internal int Position
	{
		get
		{
			if (node == null)
			{
				return 0;
			}
			return node.Position;
		}
	}

	internal SyntaxNode Parent
	{
		get
		{
			if (node == null)
			{
				return null;
			}
			return node.Parent;
		}
	}

	public int Count
	{
		get
		{
			if (node != null)
			{
				if (!node.Node.IsList)
				{
					return 1;
				}
				return node.SlotCount;
			}
			return 0;
		}
	}

	public SyntaxNodeOrToken this[int index]
	{
		get
		{
			if (node != null)
			{
				if (!node.IsList)
				{
					if (index == 0)
					{
						return node;
					}
				}
				else if ((uint)index < (uint)node.SlotCount)
				{
					InternalSyntaxNode slot = node.Node.GetSlot(index);
					if (slot.IsToken)
					{
						return new SyntaxToken(Parent, slot, node.GetChildPosition(index), IndexInParentList + index);
					}
					return node.GetNodeSlot(index);
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	public TextSpan FullSpan
	{
		get
		{
			if (node == null)
			{
				return default;
			}
			return node.FullSpan;
		}
	}

	public TextSpan Span
	{
		get
		{
			if (node == null)
			{
				return default;
			}
			return node.Span;
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private SyntaxNodeOrToken[] Nodes => this.ToArray();

	internal SyntaxNodeOrTokenList(SyntaxNode node, int index)
	{
		this = default;
		DebugAssertHelper.Assert(node != null || index == 0);
		if (node != null)
		{
			this.node = node;
			IndexInParentList = index;
		}
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
		return node != null;
	}

	internal void CopyTo(int offset, InternalSyntaxNode[] array, int arrayOffset, int count)
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

	public SyntaxNodeOrTokenList Insert(int insertIndex, SyntaxNodeOrToken nodeOrToken)
	{
		if (nodeOrToken == default(SyntaxNodeOrToken))
		{
			throw new ArgumentOutOfRangeException("nodeOrToken");
		}
		return InsertRange(insertIndex, SpecializedCollections.SingletonEnumerable(nodeOrToken));
	}

	public SyntaxNodeOrTokenList InsertRange(int insertIndex, IEnumerable<SyntaxNodeOrToken> nodesAndTokens)
	{
		if (insertIndex < 0 || insertIndex > Count)
		{
			throw new ArgumentOutOfRangeException("insertIndex");
		}
		if (nodesAndTokens == null)
		{
			throw new ArgumentNullException("nodesAndTokens");
		}
		if (!nodesAndTokens.Any())
		{
			return this;
		}
		List<SyntaxNodeOrToken> list = this.ToList();
		list.InsertRange(insertIndex, nodesAndTokens);
		return CreateList(list[0].UnderlyingNode, list);
	}

	private static SyntaxNodeOrTokenList CreateList(InternalSyntaxNode creator, List<SyntaxNodeOrToken> items)
	{
		if (items.Count == 0)
		{
			return default;
		}
		InternalSyntaxNode internalSyntaxNode = creator.CreateList(items.Select((SyntaxNodeOrToken n) => n.UnderlyingNode));
		if (internalSyntaxNode.IsToken)
		{
			internalSyntaxNode = creator.CreateList(new InternalSyntaxNode[1] { internalSyntaxNode }, alwaysCreateListNode: true);
		}
		return new SyntaxNodeOrTokenList(internalSyntaxNode.CreateRed(), 0);
	}

	public SyntaxNodeOrTokenList RemoveAt(int removeIndex)
	{
		if (removeIndex < 0 || removeIndex >= Count)
		{
			throw new ArgumentOutOfRangeException("removeIndex");
		}
		SyntaxNodeOrToken syntaxNodeOrToken = this[removeIndex];
		List<SyntaxNodeOrToken> list = this.ToList();
		list.RemoveAt(removeIndex);
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
		if (node != null)
		{
			return GetEnumerator();
		}
		return SpecializedCollections.EmptyEnumerator<SyntaxNodeOrToken>();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (node != null)
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
		return node == other.node;
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
		if (node == null)
		{
			return 0;
		}
		return node.GetHashCode();
	}
}
