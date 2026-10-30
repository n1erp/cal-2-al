using System;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Syntax.InternalSyntax;

internal class SyntaxListBuilder
{
	private ArrayElement<GreenNode>[] _nodes;

	public int Count { get; private set; }

	public GreenNode this[int index]
	{
		get
		{
			return _nodes[index];
		}
		set
		{
			_nodes[index].Value = value;
		}
	}

	public SyntaxListBuilder(int size)
	{
		_nodes = new ArrayElement<GreenNode>[size];
	}

	public static SyntaxListBuilder Create()
	{
		return new SyntaxListBuilder(8);
	}

	public void Clear()
	{
		Count = 0;
	}

	public void Add(GreenNode item)
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
			_nodes[Count++].Value = item;
		}
	}

	public void AddRange(GreenNode[] items)
	{
		AddRange(items, 0, items.Length);
	}

	public void AddRange(GreenNode[] items, int offset, int length)
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
		}
	}

	public void AddRange(SyntaxList<GreenNode> list)
	{
		AddRange(list, 0, list.Count);
	}

	public void AddRange(SyntaxList<GreenNode> list, int offset, int length)
	{
		EnsureAdditionalCapacity(length - offset);
		_ = Count;
		for (int i = offset; i < length; i++)
		{
			Add(list[i]);
		}
	}

	public void AddRange<TNode>(SyntaxList<TNode> list) where TNode : GreenNode
	{
		AddRange(list, 0, list.Count);
	}

	public void AddRange<TNode>(SyntaxList<TNode> list, int offset, int length) where TNode : GreenNode
	{
		AddRange(new SyntaxList<GreenNode>(list.Node), offset, length);
	}

	public void RemoveLast()
	{
		Count--;
		_nodes[Count].Value = null;
	}

	private void EnsureAdditionalCapacity(int additionalCount)
	{
		int num = _nodes.Length;
		int num2 = Count + additionalCount;
		if (num2 > num)
		{
			int newSize;
			if (num2 < 8)
			{
				newSize = 8;
			}
			else
			{
				newSize = ((num2 >= 1073741823) ? int.MaxValue : Math.Max(num2, num * 2));
			}
			Array.Resize(ref _nodes, newSize);
		}
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

	public GreenNode[] ToArray()
	{
		GreenNode[] array = new GreenNode[Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _nodes[i];
		}
		return array;
	}

	internal GreenNode ToListNode()
	{
		switch (Count)
		{
		case 0:
			return null;
		case 1:
			return _nodes[0];
		case 2:
			return SyntaxList.List(_nodes[0], _nodes[1]);
		case 3:
			return SyntaxList.List(_nodes[0], _nodes[1], _nodes[2]);
		default:
		{
			ArrayElement<GreenNode>[] array = new ArrayElement<GreenNode>[Count];
			Array.Copy(_nodes, array, Count);
			return SyntaxList.List(array);
		}
		}
	}

	public SyntaxList<GreenNode> ToList()
	{
		return new SyntaxList<GreenNode>(ToListNode());
	}

	public SyntaxList<TNode> ToList<TNode>() where TNode : GreenNode
	{
		return new SyntaxList<TNode>(ToListNode());
	}
}
internal struct SyntaxListBuilder<TNode> where TNode : GreenNode
{
	private readonly SyntaxListBuilder _builder;

	public bool IsNull => _builder == null;

	public int Count => _builder.Count;

	public TNode this[int index]
	{
		get
		{
			return (TNode)_builder[index];
		}
		set
		{
			_builder[index] = value;
		}
	}

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

	public GreenNode ToListNode()
	{
		return _builder.ToListNode();
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

	public SyntaxList<TDerived> ToList<TDerived>() where TDerived : GreenNode
	{
		return new SyntaxList<TDerived>(ToListNode());
	}
}
