using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal class SyntaxTokenListBuilder
{
	private InternalSyntaxNode[] nodes;

	private int count;

	public int Count => count;

	public SyntaxTokenListBuilder(int size)
	{
		nodes = new InternalSyntaxNode[size];
		count = 0;
	}

	public static SyntaxTokenListBuilder Create()
	{
		return new SyntaxTokenListBuilder(8);
	}

	public void Add(SyntaxToken item)
	{
		Add((InternalSyntaxToken)item.Node);
	}

	internal void Add(InternalSyntaxToken item)
	{
		CheckSpace(1);
		nodes[count++] = item;
	}

	public void Add(SyntaxTokenList list)
	{
		Add(list, 0, list.Count);
	}

	public void Add(SyntaxTokenList list, int offset, int length)
	{
		CheckSpace(length);
		list.CopyTo(offset, nodes, count, length);
		count += length;
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
			nodes[count + i] = list[offset + i].Node;
		}
		count += length;
	}

	private void CheckSpace(int delta)
	{
		int num = count + delta;
		if (num > nodes.Length)
		{
			Grow(num);
		}
	}

	private void Grow(int newSize)
	{
		InternalSyntaxNode[] destinationArray = new InternalSyntaxNode[newSize];
		Array.Copy(nodes, destinationArray, nodes.Length);
		nodes = destinationArray;
	}

	public SyntaxTokenList ToList()
	{
		if (count > 0)
		{
			return count switch
			{
				1 => new SyntaxTokenList(null, nodes[0], 0, 0), 
				2 => new SyntaxTokenList(null, InternalSyntaxList.List(nodes[0], nodes[1]), 0, 0), 
				3 => new SyntaxTokenList(null, InternalSyntaxList.List(nodes[0], nodes[1], nodes[2]), 0, 0), 
				_ => new SyntaxTokenList(null, InternalSyntaxList.List(nodes, count), 0, 0), 
			};
		}
		return default;
	}

	public static implicit operator SyntaxTokenList(SyntaxTokenListBuilder builder)
	{
		return builder.ToList();
	}
}
