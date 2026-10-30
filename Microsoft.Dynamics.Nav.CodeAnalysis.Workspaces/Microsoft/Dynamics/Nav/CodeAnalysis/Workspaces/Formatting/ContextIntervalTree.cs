using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class ContextIntervalTree<T> : SimpleIntervalTree<T>
{
	private readonly Func<T, int, int, bool> containPredicate;

	private readonly Func<T, int, int, bool> edgeExclusivePredicate;

	private readonly Func<T, int, int, bool> edgeInclusivePredicate;

	public ContextIntervalTree(IIntervalIntrospector<T> introspector)
		: base(introspector, (IEnumerable<T>)null)
	{
		edgeExclusivePredicate = ContainsEdgeExclusive;
		edgeInclusivePredicate = ContainsEdgeInclusive;
		containPredicate = (T value, int start, int end) => IntervalTree<T>.Contains(value, start, end, Introspector);
	}

	public T GetSmallestEdgeExclusivelyContainingInterval(int start, int length)
	{
		return GetSmallestContainingIntervalWorker(start, length, edgeExclusivePredicate);
	}

	public T GetSmallestEdgeInclusivelyContainingInterval(int start, int length)
	{
		return GetSmallestContainingIntervalWorker(start, length, edgeInclusivePredicate);
	}

	public T GetSmallestContainingInterval(int start, int length)
	{
		return GetSmallestContainingIntervalWorker(start, length, containPredicate);
	}

	private bool ContainsEdgeExclusive(T value, int start, int length)
	{
		int num = start + length;
		int end = IntervalTree<T>.GetEnd(value, Introspector);
		if (Introspector.GetStart(value) < start)
		{
			return num < end;
		}
		return false;
	}

	private bool ContainsEdgeInclusive(T value, int start, int length)
	{
		int num = start + length;
		int end = IntervalTree<T>.GetEnd(value, Introspector);
		if (Introspector.GetStart(value) <= start)
		{
			return num <= end;
		}
		return false;
	}

	private T GetSmallestContainingIntervalWorker(int start, int length, Func<T, int, int, bool> predicate)
	{
		T val = default;
		if (root == null || MaxEndValue(root) < start)
		{
			return val;
		}
		int num = start + length;
		using PooledObject<Stack<Node>> pooledObject = SharedPools.Default<Stack<Node>>().GetPooledObject();
		Stack<Node> stack = pooledObject.Object;
		stack.Push(root);
		while (stack.Count > 0)
		{
			Node node = stack.Peek();
			if (Introspector.GetStart(node.Value) <= start)
			{
				Node right = node.Right;
				if (right != null && num < MaxEndValue(right))
				{
					stack.Push(right);
					continue;
				}
			}
			Node left = node.Left;
			if (left != null && num <= MaxEndValue(left))
			{
				stack.Push(left);
				continue;
			}
			while (stack.Count > 0)
			{
				node = stack.Pop();
				if (predicate(node.Value, start, length) && (EqualityComparer<T>.Default.Equals(val, default) || (Introspector.GetStart(val) <= Introspector.GetStart(node.Value) && Introspector.GetLength(node.Value) < Introspector.GetLength(val))))
				{
					val = node.Value;
				}
				if (stack.Count == 0)
				{
					return val;
				}
				Node node2 = stack.Peek();
				if (node2.Left != node && node2.Right == node && node2.Left != null && num <= MaxEndValue(node2.Left) && (EqualityComparer<T>.Default.Equals(val, default) || Introspector.GetStart(node2.Value) == Introspector.GetStart(node.Value)))
				{
					stack.Push(node2.Left);
					break;
				}
			}
		}
		return val;
	}
}
