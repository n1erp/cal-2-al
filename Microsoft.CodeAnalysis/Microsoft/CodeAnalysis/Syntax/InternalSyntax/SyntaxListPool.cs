using System;

namespace Microsoft.CodeAnalysis.Syntax.InternalSyntax;

internal class SyntaxListPool
{
	private ArrayElement<SyntaxListBuilder>[] _freeList = new ArrayElement<SyntaxListBuilder>[10];

	private int _freeIndex;

	internal SyntaxListPool()
	{
	}

	internal SyntaxListBuilder Allocate()
	{
		SyntaxListBuilder result;
		if (_freeIndex > 0)
		{
			_freeIndex--;
			result = _freeList[_freeIndex].Value;
			_freeList[_freeIndex].Value = null;
		}
		else
		{
			result = new SyntaxListBuilder(10);
		}
		return result;
	}

	internal SyntaxListBuilder<TNode> Allocate<TNode>() where TNode : GreenNode
	{
		return new SyntaxListBuilder<TNode>(Allocate());
	}

	internal SeparatedSyntaxListBuilder<TNode> AllocateSeparated<TNode>() where TNode : GreenNode
	{
		return new SeparatedSyntaxListBuilder<TNode>(Allocate());
	}

	internal void Free<TNode>(in SeparatedSyntaxListBuilder<TNode> item) where TNode : GreenNode
	{
		Free(item.UnderlyingBuilder);
	}

	internal void Free(SyntaxListBuilder item)
	{
		item.Clear();
		if (_freeIndex >= _freeList.Length)
		{
			Grow();
		}
		_freeList[_freeIndex].Value = item;
		_freeIndex++;
	}

	private void Grow()
	{
		ArrayElement<SyntaxListBuilder>[] array = new ArrayElement<SyntaxListBuilder>[_freeList.Length * 2];
		Array.Copy(_freeList, array, _freeList.Length);
		_freeList = array;
	}

	public SyntaxList<TNode> ToListAndFree<TNode>(SyntaxListBuilder<TNode> item) where TNode : GreenNode
	{
		SyntaxList<TNode> result = item.ToList();
		Free(item);
		return result;
	}
}
