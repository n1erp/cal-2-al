using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal class SyntaxTriviaListBuilder
{
	private SyntaxTrivia[] nodes;

	private int count;

	private SyntaxTrivia[] previous;

	public int Count => count;

	public SyntaxTrivia this[int index]
	{
		get
		{
			if (index < 0 || index > count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return nodes[index];
		}
	}

	public SyntaxTriviaListBuilder(int size)
	{
		nodes = new SyntaxTrivia[size];
	}

	public static SyntaxTriviaListBuilder Create()
	{
		return new SyntaxTriviaListBuilder(4);
	}

	public void Clear()
	{
		count = 0;
	}

	public SyntaxTriviaListBuilder Add(SyntaxTrivia item)
	{
		if (nodes == null || count >= nodes.Length)
		{
			Grow((count == 0) ? 8 : (nodes.Length * 2));
		}
		nodes[count++] = item;
		return this;
	}

	public void Add(SyntaxTrivia[] items)
	{
		Add(items, 0, items.Length);
	}

	public void Add(SyntaxTrivia[] items, int offset, int length)
	{
		if (nodes == null || count + length > nodes.Length)
		{
			Grow(count + length);
		}
		Array.Copy(items, offset, nodes, count, length);
		count += length;
	}

	public void Add(SyntaxTriviaList list)
	{
		Add(list, 0, list.Count);
	}

	public void Add(SyntaxTriviaList list, int offset, int length)
	{
		if (nodes == null || count + length > nodes.Length)
		{
			Grow(count + length);
		}
		list.CopyTo(offset, nodes, count, length);
		count += length;
	}

	private void Grow(int size)
	{
		SyntaxTrivia[] destinationArray = new SyntaxTrivia[size];
		if (previous != null)
		{
			Array.Copy(previous, destinationArray, count);
			previous = null;
		}
		else
		{
			Array.Copy(nodes, destinationArray, nodes.Length);
		}
		nodes = destinationArray;
	}

	public static implicit operator SyntaxTriviaList(SyntaxTriviaListBuilder builder)
	{
		return builder.ToList();
	}

	public SyntaxTriviaList ToList()
	{
		if (count > 0)
		{
			if (previous != null)
			{
				Grow(count);
			}
			switch (count)
			{
			case 1:
				return new SyntaxTriviaList(default, nodes[0].UnderlyingNode, 0);
			case 2:
				return new SyntaxTriviaList(default, InternalSyntaxList.List(nodes[0].UnderlyingNode, nodes[1].UnderlyingNode), 0);
			case 3:
				return new SyntaxTriviaList(default, InternalSyntaxList.List(nodes[0].UnderlyingNode, nodes[1].UnderlyingNode, nodes[2].UnderlyingNode), 0);
			default:
			{
				ArrayElement<InternalSyntaxNode>[] array = new ArrayElement<InternalSyntaxNode>[count];
				for (int i = 0; i < count; i++)
				{
					array[i].Value = nodes[i].UnderlyingNode;
				}
				return new SyntaxTriviaList(default, InternalSyntaxList.List(array), 0);
			}
			}
		}
		return default;
	}
}
