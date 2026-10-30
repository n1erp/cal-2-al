using System;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.Syntax;

internal class SyntaxTokenListBuilder
{
	private GreenNode[] _nodes;

	private int _count;

	public int Count => _count;

	public SyntaxTokenListBuilder(int size)
	{
		_nodes = new GreenNode[size];
		_count = 0;
	}

	public static SyntaxTokenListBuilder Create()
	{
		return new SyntaxTokenListBuilder(8);
	}

	public void Add(SyntaxToken item)
	{
		Add(item.Node);
	}

	internal void Add(GreenNode item)
	{
		CheckSpace(1);
		_nodes[_count++] = item;
	}

	public void Add(SyntaxTokenList list)
	{
		Add(list, 0, list.Count);
	}

	public void Add(SyntaxTokenList list, int offset, int length)
	{
		CheckSpace(length);
		list.CopyTo(offset, _nodes, _count, length);
		_count += length;
	}

	public void Add(SyntaxToken[] list)
	{
		Add(list, 0, list.Length);
	}

	public void Add(SyntaxToken[] list, int offset, int length)
	{
		CheckSpace(length);
		for (int i = 0; i < length; i++)
		{
			_nodes[_count + i] = list[offset + i].Node;
		}
		_count += length;
	}

	private void CheckSpace(int delta)
	{
		int num = _count + delta;
		if (num > _nodes.Length)
		{
			Grow(num);
		}
	}

	private void Grow(int newSize)
	{
		GreenNode[] array = new GreenNode[newSize];
		Array.Copy(_nodes, array, _nodes.Length);
		_nodes = array;
	}

	public SyntaxTokenList ToList()
	{
		if (_count > 0)
		{
			return _count switch
			{
				1 => new SyntaxTokenList(null, _nodes[0], 0, 0), 
				2 => new SyntaxTokenList(null, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(_nodes[0], _nodes[1]), 0, 0), 
				3 => new SyntaxTokenList(null, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(_nodes[0], _nodes[1], _nodes[2]), 0, 0), 
				_ => new SyntaxTokenList(null, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(_nodes, _count), 0, 0), 
			};
		}
		return default;
	}

	public static implicit operator SyntaxTokenList(SyntaxTokenListBuilder builder)
	{
		return builder.ToList();
	}
}
