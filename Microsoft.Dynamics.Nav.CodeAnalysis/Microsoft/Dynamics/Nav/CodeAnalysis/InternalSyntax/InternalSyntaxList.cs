using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal abstract class InternalSyntaxList : InternalSyntaxNode
{
	internal class WithTwoChildren : InternalSyntaxList
	{
		private readonly InternalSyntaxNode child0;

		private readonly InternalSyntaxNode child1;

		internal WithTwoChildren(InternalSyntaxNode child0, InternalSyntaxNode child1)
		{
			SlotCount = 2;
			this.child0 = child0;
			AdjustFlagsAndWidth(child0);
			this.child1 = child1;
			AdjustFlagsAndWidth(child1);
		}

		internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
		{
			return new SyntaxList.WithTwoChildren(this, parent, position);
		}

		internal override InternalSyntaxNode GetSlot(int index)
		{
			return index switch
			{
				0 => child0, 
				1 => child1, 
				_ => null, 
			};
		}

		internal override void CopyTo(ArrayElement<InternalSyntaxNode>[] array, int offset)
		{
			array[offset].Value = child0;
			array[offset + 1].Value = child1;
		}

		public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
		{
			throw new NotSupportedException();
		}

		public override void Accept(InternalSyntaxVisitor visitor)
		{
			throw new NotSupportedException();
		}
	}

	internal class WithThreeChildren : InternalSyntaxList
	{
		private readonly InternalSyntaxNode child0;

		private readonly InternalSyntaxNode child1;

		private readonly InternalSyntaxNode child2;

		internal WithThreeChildren(InternalSyntaxNode child0, InternalSyntaxNode child1, InternalSyntaxNode child2)
		{
			SlotCount = 3;
			this.child0 = child0;
			AdjustFlagsAndWidth(child0);
			this.child1 = child1;
			AdjustFlagsAndWidth(child1);
			this.child2 = child2;
			AdjustFlagsAndWidth(child2);
		}

		internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
		{
			return new SyntaxList.WithThreeChildren(this, parent, position);
		}

		internal override InternalSyntaxNode GetSlot(int index)
		{
			return index switch
			{
				0 => child0, 
				1 => child1, 
				2 => child2, 
				_ => null, 
			};
		}

		internal override void CopyTo(ArrayElement<InternalSyntaxNode>[] array, int offset)
		{
			array[offset].Value = child0;
			array[offset + 1].Value = child1;
			array[offset + 2].Value = child2;
		}

		public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
		{
			throw new NotSupportedException();
		}

		public override void Accept(InternalSyntaxVisitor visitor)
		{
			throw new NotSupportedException();
		}
	}

	internal abstract class WithManyChildrenBase : InternalSyntaxList
	{
		protected readonly ArrayElement<InternalSyntaxNode>[] Children;

		internal WithManyChildrenBase(ArrayElement<InternalSyntaxNode>[] children)
		{
			Children = children;
			InitializeChildren();
		}

		private void InitializeChildren()
		{
			int num = Children.Length;
			if (num < 255)
			{
				SlotCount = (byte)num;
			}
			else
			{
				SlotCount = 255;
			}
			for (int i = 0; i < Children.Length; i++)
			{
				AdjustFlagsAndWidth(Children[i]);
			}
		}

		protected override int GetSlotCount()
		{
			return Children.Length;
		}

		internal override InternalSyntaxNode GetSlot(int index)
		{
			return Children[index];
		}

		internal override void CopyTo(ArrayElement<InternalSyntaxNode>[] array, int offset)
		{
			Array.Copy(Children, 0, array, offset, Children.Length);
		}

		internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
		{
			if (SlotCount > 1 && HasNodeTokenPattern())
			{
				return new SyntaxList.SeparatedWithManyChildren(this, parent, position);
			}
			return new SyntaxList.WithManyChildren(this, parent, position);
		}

		private bool HasNodeTokenPattern()
		{
			for (int i = 0; i < SlotCount; i++)
			{
				if (GetSlot(i).IsToken == ((i & 1) == 0))
				{
					return false;
				}
			}
			return true;
		}

		public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
		{
			throw new NotSupportedException();
		}

		public override void Accept(InternalSyntaxVisitor visitor)
		{
			throw new NotSupportedException();
		}
	}

	internal class WithManyChildren : WithManyChildrenBase
	{
		internal WithManyChildren(ArrayElement<InternalSyntaxNode>[] children)
			: base(children)
		{
		}
	}

	internal sealed class WithLotsOfChildren : WithManyChildrenBase
	{
		private readonly int[] childOffsets;

		internal WithLotsOfChildren(ArrayElement<InternalSyntaxNode>[] children)
			: base(children)
		{
			childOffsets = CalculateOffsets(children);
		}

		public override int GetSlotOffset(int index)
		{
			return childOffsets[index];
		}

		public override int FindSlotIndexContainingOffset(int offset)
		{
			DebugAssertHelper.Assert(offset >= 0 && offset < FullWidth);
			return childOffsets.BinarySearchUpperBound(offset) - 1;
		}

		private static int[] CalculateOffsets(ArrayElement<InternalSyntaxNode>[] children)
		{
			int num = children.Length;
			int[] array = new int[num];
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				array[i] = num2;
				num2 += children[i].Value.FullWidth;
			}
			return array;
		}
	}

	internal InternalSyntaxList()
		: base(SyntaxKind.List)
	{
	}

	internal static InternalSyntaxNode List(InternalSyntaxNode child)
	{
		return child;
	}

	internal static WithTwoChildren List(InternalSyntaxNode child0, InternalSyntaxNode child1)
	{
		DebugAssertHelper.Assert(child0 != null);
		DebugAssertHelper.Assert(child1 != null);
		return new WithTwoChildren(child0, child1);
	}

	internal static WithThreeChildren List(InternalSyntaxNode child0, InternalSyntaxNode child1, InternalSyntaxNode child2)
	{
		DebugAssertHelper.Assert(child0 != null);
		DebugAssertHelper.Assert(child1 != null);
		DebugAssertHelper.Assert(child2 != null);
		return new WithThreeChildren(child0, child1, child2);
	}

	internal static InternalSyntaxNode List(InternalSyntaxNode[] nodes)
	{
		return List(nodes, nodes.Length);
	}

	internal static InternalSyntaxNode List(InternalSyntaxNode[] nodes, int count)
	{
		ArrayElement<InternalSyntaxNode>[] array = new ArrayElement<InternalSyntaxNode>[count];
		for (int i = 0; i < count; i++)
		{
			DebugAssertHelper.Assert(nodes[i] != null);
			array[i].Value = nodes[i];
		}
		return List(array);
	}

	internal static InternalSyntaxNode List(InternalSyntaxListBuilder builder)
	{
		return builder?.ToListNode();
	}

	internal static InternalSyntaxNode List(IEnumerable<InternalSyntaxNode> nodes)
	{
		InternalSyntaxNode[] array = nodes.ToArray();
		return array.Length switch
		{
			0 => null, 
			1 => List(array[0]), 
			2 => List(array[0], array[1]), 
			3 => List(array[0], array[1], array[2]), 
			_ => List(array), 
		};
	}

	internal static InternalSyntaxList List(ArrayElement<InternalSyntaxNode>[] children)
	{
		if (children.Length < 10)
		{
			return new WithManyChildren(children);
		}
		return new WithLotsOfChildren(children);
	}

	internal abstract void CopyTo(ArrayElement<InternalSyntaxNode>[] array, int offset);

	internal static InternalSyntaxNode Concat(InternalSyntaxNode left, InternalSyntaxNode right)
	{
		if (left == null)
		{
			return right;
		}
		if (right == null)
		{
			return left;
		}
		InternalSyntaxList internalSyntaxList = left as InternalSyntaxList;
		InternalSyntaxList internalSyntaxList2 = right as InternalSyntaxList;
		if (internalSyntaxList != null)
		{
			if (internalSyntaxList2 != null)
			{
				ArrayElement<InternalSyntaxNode>[] array = new ArrayElement<InternalSyntaxNode>[left.SlotCount + right.SlotCount];
				internalSyntaxList.CopyTo(array, 0);
				internalSyntaxList2.CopyTo(array, left.SlotCount);
				return List(array);
			}
			ArrayElement<InternalSyntaxNode>[] array2 = new ArrayElement<InternalSyntaxNode>[left.SlotCount + 1];
			internalSyntaxList.CopyTo(array2, 0);
			array2[left.SlotCount].Value = right;
			return List(array2);
		}
		if (internalSyntaxList2 != null)
		{
			ArrayElement<InternalSyntaxNode>[] array3 = new ArrayElement<InternalSyntaxNode>[internalSyntaxList2.SlotCount + 1];
			array3[0].Value = left;
			internalSyntaxList2.CopyTo(array3, 1);
			return List(array3);
		}
		return List(left, right);
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		throw new NotSupportedException();
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		throw new NotSupportedException();
	}
}
internal struct InternalSyntaxList<TNode> : IEquatable<InternalSyntaxList<TNode>> where TNode : InternalSyntaxNode
{
	internal InternalSyntaxNode Node { get; }

	public int Count
	{
		get
		{
			if (Node != null)
			{
				if (!Node.IsList)
				{
					return 1;
				}
				return Node.SlotCount;
			}
			return 0;
		}
	}

	public TNode this[int index]
	{
		get
		{
			if (Node == null)
			{
				return null;
			}
			if (Node.IsList)
			{
				DebugAssertHelper.Assert(index >= 0);
				DebugAssertHelper.Assert(index <= Node.SlotCount);
				return (TNode)Node.GetSlot(index);
			}
			if (index == 0)
			{
				return (TNode)Node;
			}
			throw new NotSupportedException();
		}
	}

	private TNode[] Nodes
	{
		get
		{
			TNode[] array = new TNode[Count];
			for (int i = 0; i < Count; i++)
			{
				array[i] = this[i];
			}
			return array;
		}
	}

	internal InternalSyntaxList(InternalSyntaxNode node)
	{
		Node = node;
	}

	public bool Any()
	{
		return Node != null;
	}

	internal void CopyTo(int offset, ArrayElement<InternalSyntaxNode>[] array, int arrayOffset, int count)
	{
		for (int i = 0; i < count; i++)
		{
			array[arrayOffset + i].Value = this[i + offset];
		}
	}

	public static bool operator ==(InternalSyntaxList<TNode> left, InternalSyntaxList<TNode> right)
	{
		return left.Node == right.Node;
	}

	public static bool operator !=(InternalSyntaxList<TNode> left, InternalSyntaxList<TNode> right)
	{
		return left.Node != right.Node;
	}

	public bool Equals(InternalSyntaxList<TNode> other)
	{
		return Node == other.Node;
	}

	public override bool Equals(object obj)
	{
		if (obj is InternalSyntaxList<TNode>)
		{
			return Equals((InternalSyntaxList<TNode>)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (Node == null)
		{
			return 0;
		}
		return Node.GetHashCode();
	}

	public InternalSeparatedSyntaxList<TOther> AsSeparatedList<TOther>() where TOther : InternalSyntaxNode
	{
		return new InternalSeparatedSyntaxList<TOther>(this);
	}

	public static implicit operator InternalSyntaxList<TNode>(TNode node)
	{
		return new InternalSyntaxList<TNode>(node);
	}

	public static implicit operator InternalSyntaxList<TNode>(InternalSyntaxList<InternalSyntaxNode> nodes)
	{
		return new InternalSyntaxList<TNode>(nodes.Node);
	}

	public static implicit operator InternalSyntaxList<InternalSyntaxNode>(InternalSyntaxList<TNode> nodes)
	{
		return new InternalSyntaxList<InternalSyntaxNode>(nodes.Node);
	}
}
