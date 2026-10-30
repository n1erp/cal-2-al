using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.Syntax;

internal class SyntaxListBuilder
{
	private ArrayElement<GreenNode>[] _nodes;

	public int Count { get; private set; }

	public SyntaxListBuilder(int size)
	{
		_nodes = new ArrayElement<GreenNode>[size];
	}

	public void Clear()
	{
		Count = 0;
	}

	public void Add(SyntaxNode item)
	{
		AddInternal(item.Green);
	}

	internal void AddInternal(GreenNode item)
	{
		if (item == null)
		{
			throw new ArgumentNullException();
		}
		if (_nodes == null || Count >= _nodes.Length)
		{
			Grow((Count == 0) ? 8 : (_nodes.Length * 2));
		}
		_nodes[Count++].Value = item;
	}

	public void AddRange(SyntaxNode[] items)
	{
		AddRange(items, 0, items.Length);
	}

	public void AddRange(SyntaxNode[] items, int offset, int length)
	{
		if (_nodes == null || Count + length > _nodes.Length)
		{
			Grow(Count + length);
		}
		int num = offset;
		int num2 = Count;
		while (num < offset + length)
		{
			_nodes[num2].Value = items[num].Green;
			num++;
			num2++;
		}
		_ = Count;
		Count += length;
	}

	[Conditional("DEBUG")]
	private void Validate(int start, int end)
	{
		for (int i = start; i < end; i++)
		{
			if (_nodes[i].Value == null)
			{
				throw new ArgumentException("Cannot add a null node.");
			}
		}
	}

	public void AddRange(SyntaxList<SyntaxNode> list)
	{
		AddRange(list, 0, list.Count);
	}

	public void AddRange(SyntaxList<SyntaxNode> list, int offset, int count)
	{
		if (_nodes == null || Count + count > _nodes.Length)
		{
			Grow(Count + count);
		}
		int num = Count;
		int i = offset;
		for (int num2 = offset + count; i < num2; i++)
		{
			_nodes[num].Value = list.ItemInternal(i).Green;
			num++;
		}
		_ = Count;
		Count += count;
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
		if (_nodes == null || Count + count > _nodes.Length)
		{
			Grow(Count + count);
		}
		int num = Count;
		int i = offset;
		for (int num2 = offset + count; i < num2; i++)
		{
			_nodes[num].Value = list[i].UnderlyingNode;
			num++;
		}
		_ = Count;
		Count += count;
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
		ArrayElement<GreenNode>[] array = new ArrayElement<GreenNode>[size];
		Array.Copy(_nodes, array, _nodes.Length);
		_nodes = array;
	}

	public bool Any(int kind)
	{
		for (int i = 0; i < Count; i++)
		{
			if (_nodes[i].Value.RawKind == kind)
			{
				return true;
			}
		}
		return false;
	}

	internal GreenNode ToListNode()
	{
		switch (Count)
		{
		case 0:
			return null;
		case 1:
			return _nodes[0].Value;
		case 2:
			return Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(_nodes[0].Value, _nodes[1].Value);
		case 3:
			return Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(_nodes[0].Value, _nodes[1].Value, _nodes[2].Value);
		default:
		{
			ArrayElement<GreenNode>[] array = new ArrayElement<GreenNode>[Count];
			for (int i = 0; i < Count; i++)
			{
				array[i].Value = _nodes[i].Value;
			}
			return Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(array);
		}
		}
	}

	public static implicit operator SyntaxList<SyntaxNode>(SyntaxListBuilder builder)
	{
		return builder?.ToList() ?? default(SyntaxList<SyntaxNode>);
	}

	internal void RemoveLast()
	{
		Count--;
		_nodes[Count] = default;
	}
}
internal struct SyntaxListBuilder<TNode> where TNode : SyntaxNode
{
	private readonly SyntaxListBuilder _builder;

	public bool IsNull => _builder == null;

	public int Count => _builder.Count;

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
		_builder = builder;
	}

	public void Clear()
	{
		_builder.Clear();
	}

	public SyntaxListBuilder<TNode> Add(TNode node)
	{
		_builder.Add(node);
		return this;
	}

	public void AddRange(TNode[] items, int offset, int length)
	{
		_builder.AddRange(items, offset, length);
	}

	public void AddRange(SyntaxList<TNode> nodes)
	{
		_builder.AddRange(nodes);
	}

	public void AddRange(SyntaxList<TNode> nodes, int offset, int length)
	{
		_builder.AddRange(nodes, offset, length);
	}

	public bool Any(int kind)
	{
		return _builder.Any(kind);
	}

	public SyntaxList<TNode> ToList()
	{
		return _builder.ToList();
	}

	public static implicit operator SyntaxListBuilder(SyntaxListBuilder<TNode> builder)
	{
		return builder._builder;
	}

	public static implicit operator SyntaxList<TNode>(SyntaxListBuilder<TNode> builder)
	{
		if (builder._builder != null)
		{
			return builder.ToList();
		}
		return default;
	}
}
