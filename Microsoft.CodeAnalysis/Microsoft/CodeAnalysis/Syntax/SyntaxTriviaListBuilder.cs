using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.Syntax;

internal class SyntaxTriviaListBuilder
{
	private SyntaxTrivia[] _nodes;

	private int _count;

	public int Count => _count;

	public SyntaxTrivia this[int index]
	{
		get
		{
			if (index < 0 || index > _count)
			{
				throw new IndexOutOfRangeException();
			}
			return _nodes[index];
		}
	}

	public SyntaxTriviaListBuilder(int size)
	{
		_nodes = new SyntaxTrivia[size];
	}

	public static SyntaxTriviaListBuilder Create()
	{
		return new SyntaxTriviaListBuilder(4);
	}

	public static SyntaxTriviaList Create(IEnumerable<SyntaxTrivia> trivia)
	{
		if (trivia == null)
		{
			return default;
		}
		SyntaxTriviaListBuilder syntaxTriviaListBuilder = Create();
		syntaxTriviaListBuilder.AddRange(trivia);
		return syntaxTriviaListBuilder.ToList();
	}

	public void Clear()
	{
		_count = 0;
	}

	public void AddRange(IEnumerable<SyntaxTrivia> items)
	{
		if (items == null)
		{
			return;
		}
		foreach (SyntaxTrivia item in items)
		{
			Add(item);
		}
	}

	public SyntaxTriviaListBuilder Add(SyntaxTrivia item)
	{
		if (_nodes == null || _count >= _nodes.Length)
		{
			Grow((_count == 0) ? 8 : (_nodes.Length * 2));
		}
		_nodes[_count++] = item;
		return this;
	}

	public void Add(SyntaxTrivia[] items)
	{
		Add(items, 0, items.Length);
	}

	public void Add(SyntaxTrivia[] items, int offset, int length)
	{
		if (_nodes == null || _count + length > _nodes.Length)
		{
			Grow(_count + length);
		}
		Array.Copy(items, offset, _nodes, _count, length);
		_count += length;
	}

	public void Add(in SyntaxTriviaList list)
	{
		Add(in list, 0, list.Count);
	}

	public void Add(in SyntaxTriviaList list, int offset, int length)
	{
		if (_nodes == null || _count + length > _nodes.Length)
		{
			Grow(_count + length);
		}
		list.CopyTo(offset, _nodes, _count, length);
		_count += length;
	}

	private void Grow(int size)
	{
		SyntaxTrivia[] array = new SyntaxTrivia[size];
		Array.Copy(_nodes, array, _nodes.Length);
		_nodes = array;
	}

	public static implicit operator SyntaxTriviaList(SyntaxTriviaListBuilder builder)
	{
		return builder.ToList();
	}

	public SyntaxTriviaList ToList()
	{
		if (_count > 0)
		{
			switch (_count)
			{
			case 1:
				return new SyntaxTriviaList(default(SyntaxToken), _nodes[0].UnderlyingNode, 0);
			case 2:
				return new SyntaxTriviaList(default(SyntaxToken), Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(_nodes[0].UnderlyingNode, _nodes[1].UnderlyingNode), 0);
			case 3:
				return new SyntaxTriviaList(default(SyntaxToken), Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(_nodes[0].UnderlyingNode, _nodes[1].UnderlyingNode, _nodes[2].UnderlyingNode), 0);
			default:
			{
				ArrayElement<GreenNode>[] array = new ArrayElement<GreenNode>[_count];
				for (int i = 0; i < _count; i++)
				{
					array[i].Value = _nodes[i].UnderlyingNode;
				}
				return new SyntaxTriviaList(default(SyntaxToken), Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(array), 0);
			}
			}
		}
		return default;
	}
}
