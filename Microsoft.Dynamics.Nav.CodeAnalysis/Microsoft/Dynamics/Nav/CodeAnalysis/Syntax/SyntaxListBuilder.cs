using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal class SyntaxListBuilder
{
	private ArrayElement<InternalSyntaxNode>[] nodes;

	public int Count { get; private set; }

	public SyntaxListBuilder(int size)
	{
		nodes = new ArrayElement<InternalSyntaxNode>[size];
		Count = 0;
	}

	public void Clear()
	{
		Count = 0;
	}

	public void Add(SyntaxNode item)
	{
		AddInternal(item.Node);
	}

	internal void AddInternal(InternalSyntaxNode item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (nodes == null || Count >= nodes.Length)
		{
			Grow((Count == 0) ? 8 : (nodes.Length * 2));
		}
		nodes[Count++].Value = item;
	}

	public void AddRange(SyntaxNode[] items)
	{
		AddRange(items, 0, items.Length);
	}

	public void AddRange(SyntaxNode[] items, int offset, int length)
	{
		if (nodes == null || Count + length > nodes.Length)
		{
			Grow(Count + length);
		}
		int num = offset;
		int num2 = Count;
		while (num < offset + length)
		{
			nodes[num2].Value = items[num].Node;
			num++;
			num2++;
		}
		int count = Count;
		Count += length;
		Validate(count, Count);
	}

	private void Validate(int start, int end)
	{
		for (int i = start; i < end; i++)
		{
			if (nodes[i].Value == null)
			{
				throw new ArgumentException("Cannot add a null SyntaxNode.");
			}
		}
	}

	public void AddRange(SyntaxList<SyntaxNode> list)
	{
		AddRange(list, 0, list.Count);
	}

	public void AddRange(SyntaxList<SyntaxNode> list, int offset, int count)
	{
		if (nodes == null || Count + count > nodes.Length)
		{
			Grow(Count + count);
		}
		int num = Count;
		int i = offset;
		for (int num2 = offset + count; i < num2; i++)
		{
			nodes[num].Value = list.ItemInternal(i).Node;
			num++;
		}
		int count2 = Count;
		Count += count;
		Validate(count2, Count);
	}

	public void AddRange<TNode>(SyntaxList<TNode> list) where TNode : SyntaxNode
	{
		AddRange(list, 0, list.Count);
	}

	public void AddRange<TNode>(SyntaxList<TNode> list, int offset, int count) where TNode : SyntaxNode
	{
		AddRange(new SyntaxList<SyntaxNode>(list.Node), offset, count);
	}

	public void AddRange(SyntaxNodeOrTokenList list)
	{
		AddRange(list, 0, list.Count);
	}

	public void AddRange(SyntaxNodeOrTokenList list, int offset, int count)
	{
		if (nodes == null || Count + count > nodes.Length)
		{
			Grow(Count + count);
		}
		int num = Count;
		int i = offset;
		for (int num2 = offset + count; i < num2; i++)
		{
			nodes[num].Value = list[i].UnderlyingNode;
			num++;
		}
		int count2 = Count;
		Count += count;
		Validate(count2, Count);
	}

	public void AddRange(SyntaxTokenList list)
	{
		AddRange(list, 0, list.Count);
	}

	public void AddRange(SyntaxTokenList list, int offset, int length)
	{
		AddRange(new SyntaxList<SyntaxNode>(list.Node.CreateRed()), offset, length);
	}

	private void Grow(int size)
	{
		ArrayElement<InternalSyntaxNode>[] destinationArray = new ArrayElement<InternalSyntaxNode>[size];
		Array.Copy(nodes, destinationArray, nodes.Length);
		nodes = destinationArray;
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

	internal InternalSyntaxNode ToListNode()
	{
		switch (Count)
		{
		case 0:
			return null;
		case 1:
			return nodes[0].Value;
		case 2:
			return InternalSyntaxList.List(nodes[0].Value, nodes[1].Value);
		case 3:
			return InternalSyntaxList.List(nodes[0].Value, nodes[1].Value, nodes[2].Value);
		default:
		{
			ArrayElement<InternalSyntaxNode>[] array = new ArrayElement<InternalSyntaxNode>[Count];
			for (int i = 0; i < Count; i++)
			{
				array[i].Value = nodes[i].Value;
			}
			return InternalSyntaxList.List(array);
		}
		}
	}

	public static implicit operator SyntaxList<SyntaxNode>(SyntaxListBuilder builder)
	{
		return builder?.ToList() ?? default(SyntaxList<SyntaxNode>);
	}
}
internal struct SyntaxListBuilder<TNode> where TNode : SyntaxNode
{
	private readonly SyntaxListBuilder builder;

	public bool IsNull => builder == null;

	public int Count => builder.Count;

	public SyntaxListBuilder(int size)
		: this(new SyntaxListBuilder(size))
	{
	}

	public static SyntaxListBuilder<TNode> Create()
	{
		return new SyntaxListBuilder<TNode>(8);
	}

	internal SyntaxListBuilder(SyntaxListBuilder builder)
	{
		this.builder = builder;
	}

	public void Clear()
	{
		builder.Clear();
	}

	public SyntaxListBuilder<TNode> Add(TNode node)
	{
		builder.Add(node);
		return this;
	}

	public void AddRange(TNode[] items, int offset, int length)
	{
		builder.AddRange(items, offset, length);
	}

	public void AddRange(SyntaxList<TNode> nodes)
	{
		builder.AddRange(nodes);
	}

	public void AddRange(SyntaxList<TNode> nodes, int offset, int length)
	{
		builder.AddRange(nodes, offset, length);
	}

	public bool Any(SyntaxKind kind)
	{
		return builder.Any(kind);
	}

	public SyntaxList<TNode> ToList()
	{
		return builder.ToList();
	}

	public static implicit operator SyntaxListBuilder(SyntaxListBuilder<TNode> builder)
	{
		return builder.builder;
	}

	public static implicit operator SyntaxList<TNode>(SyntaxListBuilder<TNode> builder)
	{
		if (builder.builder != null)
		{
			return builder.ToList();
		}
		return default;
	}
}
