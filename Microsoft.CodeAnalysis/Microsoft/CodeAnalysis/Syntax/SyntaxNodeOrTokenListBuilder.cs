using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.Syntax;

internal class SyntaxNodeOrTokenListBuilder
{
	private GreenNode[] _nodes;

	private int _count;

	public int Count => _count;

	public SyntaxNodeOrToken this[int index]
	{
		get
		{
			GreenNode greenNode = _nodes[index];
			if (greenNode != null && greenNode.IsToken)
			{
				return new SyntaxNodeOrToken(null, greenNode, 0, 0);
			}
			return greenNode.CreateRed();
		}
		set
		{
			_nodes[index] = value.UnderlyingNode;
		}
	}

	public SyntaxNodeOrTokenListBuilder(int size)
	{
		_nodes = new GreenNode[size];
		_count = 0;
	}

	public static SyntaxNodeOrTokenListBuilder Create()
	{
		return new SyntaxNodeOrTokenListBuilder(8);
	}

	public void Clear()
	{
		_count = 0;
	}

	internal void Add(GreenNode item)
	{
		if (_nodes == null || _count >= _nodes.Length)
		{
			Grow((_count == 0) ? 8 : (_nodes.Length * 2));
		}
		_nodes[_count++] = item;
	}

	public void Add(SyntaxNode item)
	{
		Add(item.Green);
	}

	public void Add(in SyntaxToken item)
	{
		Add(item.Node);
	}

	public void Add(in SyntaxNodeOrToken item)
	{
		Add(item.UnderlyingNode);
	}

	public void Add(SyntaxNodeOrTokenList list)
	{
		Add(list, 0, list.Count);
	}

	public void Add(SyntaxNodeOrTokenList list, int offset, int length)
	{
		if (_nodes == null || _count + length > _nodes.Length)
		{
			Grow(_count + length);
		}
		list.CopyTo(offset, _nodes, _count, length);
		_count += length;
	}

	public void Add(IEnumerable<SyntaxNodeOrToken> nodeOrTokens)
	{
		foreach (SyntaxNodeOrToken nodeOrToken in nodeOrTokens)
		{
			Add(nodeOrToken);
		}
	}

	internal void RemoveLast()
	{
		_count--;
		_nodes[_count] = null;
	}

	private void Grow(int size)
	{
		GreenNode[] array = new GreenNode[size];
		Array.Copy(_nodes, array, _nodes.Length);
		_nodes = array;
	}

	public SyntaxNodeOrTokenList ToList()
	{
		if (_count > 0)
		{
			switch (_count)
			{
			case 1:
				if (_nodes[0].IsToken)
				{
					return new SyntaxNodeOrTokenList(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(new GreenNode[1] { _nodes[0] }).CreateRed(), 0);
				}
				return new SyntaxNodeOrTokenList(_nodes[0].CreateRed(), 0);
			case 2:
				return new SyntaxNodeOrTokenList(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(_nodes[0], _nodes[1]).CreateRed(), 0);
			case 3:
				return new SyntaxNodeOrTokenList(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(_nodes[0], _nodes[1], _nodes[2]).CreateRed(), 0);
			default:
			{
				ArrayElement<GreenNode>[] array = new ArrayElement<GreenNode>[_count];
				for (int i = 0; i < _count; i++)
				{
					array[i].Value = _nodes[i];
				}
				return new SyntaxNodeOrTokenList(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList.List(array).CreateRed(), 0);
			}
			}
		}
		return default;
	}
}
