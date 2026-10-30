using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class InternalSyntaxListPool
{
	private ArrayElement<InternalSyntaxListBuilder>[] freeList = new ArrayElement<InternalSyntaxListBuilder>[10];

	private int freeIndex;

	internal InternalSyntaxListBuilder Allocate()
	{
		InternalSyntaxListBuilder result;
		if (freeIndex > 0)
		{
			freeIndex--;
			result = freeList[freeIndex].Value;
			freeList[freeIndex].Value = null;
		}
		else
		{
			result = new InternalSyntaxListBuilder(10);
		}
		return result;
	}

	internal InternalSyntaxListBuilder<TNode> Allocate<TNode>() where TNode : InternalSyntaxNode
	{
		return new InternalSyntaxListBuilder<TNode>(Allocate());
	}

	internal InternalSeparatedSyntaxListBuilder<TNode> AllocateSeparated<TNode>() where TNode : InternalSyntaxNode
	{
		return new InternalSeparatedSyntaxListBuilder<TNode>(Allocate());
	}

	internal void Free<TNode>(InternalSeparatedSyntaxListBuilder<TNode> item) where TNode : InternalSyntaxNode
	{
		Free(item.UnderlyingBuilder);
	}

	internal void Free(InternalSyntaxListBuilder item)
	{
		item.Clear();
		if (freeIndex >= freeList.Length)
		{
			Grow();
		}
		freeList[freeIndex].Value = item;
		freeIndex++;
	}

	private void Grow()
	{
		ArrayElement<InternalSyntaxListBuilder>[] destinationArray = new ArrayElement<InternalSyntaxListBuilder>[freeList.Length * 2];
		Array.Copy(freeList, destinationArray, freeList.Length);
		freeList = destinationArray;
	}
}
