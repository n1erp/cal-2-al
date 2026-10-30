using System;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class InternalSyntaxListBuilder
{
	private ArrayElement<InternalSyntaxNode>[] nodes;

	public int Count { get; private set; }

	public InternalSyntaxNode this[int index]
	{
		get
		{
			return nodes[index];
		}
		set
		{
			nodes[index].Value = value;
		}
	}

	public InternalSyntaxListBuilder(int size)
	{
		nodes = new ArrayElement<InternalSyntaxNode>[size];
	}

	public void Clear()
	{
		Count = 0;
	}

	public void Add(InternalSyntaxNode item)
	{
		if (item == null)
		{
			return;
		}
		if (item.IsList)
		{
			int slotCount = item.SlotCount;
			EnsureAdditionalCapacity(slotCount);
			for (int i = 0; i < slotCount; i++)
			{
				Add(item.GetSlot(i));
			}
		}
		else
		{
			EnsureAdditionalCapacity(1);
			nodes[Count++].Value = item;
		}
	}

	public void AddRange(InternalSyntaxNode[] items)
	{
		AddRange(items, 0, items.Length);
	}

	public void AddRange(InternalSyntaxNode[] items, int offset, int length)
	{
		EnsureAdditionalCapacity(length - offset);
		_ = Count;
		for (int i = offset; i < length; i++)
		{
			Add(items[i]);
		}
	}

	[Conditional("DEBUG")]
	private void Validate(int start, int end)
	{
		for (int i = start; i < end; i++)
		{
			DebugAssertHelper.Assert(nodes[i].Value != null);
		}
	}

	public void AddRange(InternalSyntaxList<InternalSyntaxNode> list)
	{
		AddRange(list, 0, list.Count);
	}

	public void AddRange(InternalSyntaxList<InternalSyntaxNode> list, int offset, int length)
	{
		EnsureAdditionalCapacity(length - offset);
		_ = Count;
		for (int i = offset; i < length; i++)
		{
			Add(list[i]);
		}
	}

	public void AddRange<TNode>(InternalSyntaxList<TNode> list) where TNode : InternalSyntaxNode
	{
		AddRange(list, 0, list.Count);
	}

	public void AddRange<TNode>(InternalSyntaxList<TNode> list, int offset, int length) where TNode : InternalSyntaxNode
	{
		AddRange(new InternalSyntaxList<InternalSyntaxNode>(list.Node), offset, length);
	}

	internal void RemoveLast()
	{
		Count--;
		nodes[Count].Value = null;
	}

	private void EnsureAdditionalCapacity(int additionalCount)
	{
		int num = nodes.Length;
		int num2 = Count + additionalCount;
		if (num2 > num)
		{
			int num3;
			if (num2 < 8)
			{
				num3 = 8;
			}
			else
			{
				num3 = ((num2 >= 1073741823) ? int.MaxValue : Math.Max(num2, num * 2));
			}
			DebugAssertHelper.Assert(num3 >= num2);
			Array.Resize(ref nodes, num3);
		}
	}

	public bool Any(SyntaxKind kind)
	{
		for (int i = 0; i < Count; i++)
		{
			if (nodes[i].Value.Kind == kind)
			{
				return true;
			}
		}
		return false;
	}

	public InternalSyntaxNode[] ToArray()
	{
		InternalSyntaxNode[] array = new InternalSyntaxNode[Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = nodes[i];
		}
		return array;
	}

	internal InternalSyntaxNode ToListNode()
	{
		switch (Count)
		{
		case 0:
			return null;
		case 1:
			return nodes[0];
		case 2:
			return InternalSyntaxList.List(nodes[0], nodes[1]);
		case 3:
			return InternalSyntaxList.List(nodes[0], nodes[1], nodes[2]);
		default:
		{
			ArrayElement<InternalSyntaxNode>[] array = new ArrayElement<InternalSyntaxNode>[Count];
			Array.Copy(nodes, array, Count);
			return InternalSyntaxList.List(array);
		}
		}
	}

	public static implicit operator InternalSyntaxList<InternalSyntaxNode>(InternalSyntaxListBuilder builder)
	{
		return builder?.ToList() ?? default(InternalSyntaxList<InternalSyntaxNode>);
	}
}
internal struct InternalSyntaxListBuilder<TNode> where TNode : InternalSyntaxNode
{
	private readonly InternalSyntaxListBuilder builder;

	public bool IsNull => builder == null;

	public int Count => builder.Count;

	public TNode this[int index]
	{
		get
		{
			return (TNode)builder[index];
		}
		set
		{
			builder[index] = value;
		}
	}

	public InternalSyntaxListBuilder(int size)
		: this(new InternalSyntaxListBuilder(size))
	{
	}

	internal InternalSyntaxListBuilder(InternalSyntaxListBuilder builderParam)
	{
		builder = builderParam;
	}

	public void Clear()
	{
		builder.Clear();
	}

	public InternalSyntaxListBuilder<TNode> Add(TNode node)
	{
		builder.Add(node);
		return this;
	}

	public void AddRange(TNode[] items, int offset, int length)
	{
		builder.AddRange(items, offset, length);
	}

	public void AddRange(InternalSyntaxList<TNode> nodes)
	{
		builder.AddRange(nodes);
	}

	public void AddRange(InternalSyntaxList<TNode> nodes, int offset, int length)
	{
		builder.AddRange(nodes, offset, length);
	}

	public bool Any(SyntaxKind kind)
	{
		return builder.Any(kind);
	}

	public InternalSyntaxList<TNode> ToList()
	{
		return builder.ToList();
	}

	public InternalSyntaxNode ToListNode()
	{
		return builder.ToListNode();
	}

	public static implicit operator InternalSyntaxListBuilder(InternalSyntaxListBuilder<TNode> builder)
	{
		return builder.builder;
	}

	public static implicit operator InternalSyntaxList<TNode>(InternalSyntaxListBuilder<TNode> builder)
	{
		if (builder.builder != null)
		{
			return builder.ToList();
		}
		return default;
	}
}
