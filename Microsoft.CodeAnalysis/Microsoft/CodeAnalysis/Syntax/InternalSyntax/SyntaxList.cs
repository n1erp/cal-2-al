using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Syntax.InternalSyntax;

internal abstract class SyntaxList : GreenNode
{
	internal sealed class WithLotsOfChildren : WithManyChildrenBase
	{
		private readonly int[] _childOffsets;

		static WithLotsOfChildren()
		{
			ObjectBinder.RegisterTypeReader(typeof(WithLotsOfChildren), (ObjectReader r) => new WithLotsOfChildren(r));
		}

		internal WithLotsOfChildren(ArrayElement<GreenNode>[] children)
			: base(children)
		{
			_childOffsets = CalculateOffsets(children);
		}

		internal WithLotsOfChildren(DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, ArrayElement<GreenNode>[] children, int[] childOffsets)
			: base(diagnostics, annotations, children)
		{
			_childOffsets = childOffsets;
		}

		internal WithLotsOfChildren(ObjectReader reader)
			: base(reader)
		{
			_childOffsets = CalculateOffsets(children);
		}

		internal override void WriteTo(ObjectWriter writer)
		{
			base.WriteTo(writer);
		}

		public override int GetSlotOffset(int index)
		{
			return _childOffsets[index];
		}

		public override int FindSlotIndexContainingOffset(int offset)
		{
			return _childOffsets.BinarySearchUpperBound(offset) - 1;
		}

		private static int[] CalculateOffsets(ArrayElement<GreenNode>[] children)
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

		internal override GreenNode SetDiagnostics(DiagnosticInfo[] errors)
		{
			return new WithLotsOfChildren(errors, GetAnnotations(), children, _childOffsets);
		}

		internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new WithLotsOfChildren(GetDiagnostics(), annotations, children, _childOffsets);
		}
	}

	internal abstract class WithManyChildrenBase : SyntaxList
	{
		internal readonly ArrayElement<GreenNode>[] children;

		internal WithManyChildrenBase(ArrayElement<GreenNode>[] children)
		{
			this.children = children;
			InitializeChildren();
		}

		internal WithManyChildrenBase(DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, ArrayElement<GreenNode>[] children)
			: base(diagnostics, annotations)
		{
			this.children = children;
			InitializeChildren();
		}

		private void InitializeChildren()
		{
			int num = children.Length;
			if (num < 255)
			{
				SlotCount = (byte)num;
			}
			else
			{
				SlotCount = 255;
			}
			for (int i = 0; i < children.Length; i++)
			{
				AdjustFlagsAndWidth(children[i]);
			}
		}

		internal WithManyChildrenBase(ObjectReader reader)
			: base(reader)
		{
			int num = reader.ReadInt32();
			children = new ArrayElement<GreenNode>[num];
			for (int i = 0; i < num; i++)
			{
				children[i].Value = (GreenNode)reader.ReadValue();
			}
			InitializeChildren();
		}

		internal override void WriteTo(ObjectWriter writer)
		{
			base.WriteTo(writer);
			writer.WriteInt32(children.Length);
			for (int i = 0; i < children.Length; i++)
			{
				writer.WriteValue(children[i].Value);
			}
		}

		protected override int GetSlotCount()
		{
			return children.Length;
		}

		internal override GreenNode GetSlot(int index)
		{
			return children[index];
		}

		internal override void CopyTo(ArrayElement<GreenNode>[] array, int offset)
		{
			Array.Copy(children, 0, array, offset, children.Length);
		}

		internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
		{
			bool flag = SlotCount > 1 && HasNodeTokenPattern();
			if (parent != null && parent.ShouldCreateWeakList())
			{
				if (!flag)
				{
					return new Microsoft.CodeAnalysis.Syntax.SyntaxList.WithManyWeakChildren(this, parent, position);
				}
				return new Microsoft.CodeAnalysis.Syntax.SyntaxList.SeparatedWithManyWeakChildren(this, parent, position);
			}
			if (!flag)
			{
				return new Microsoft.CodeAnalysis.Syntax.SyntaxList.WithManyChildren(this, parent, position);
			}
			return new Microsoft.CodeAnalysis.Syntax.SyntaxList.SeparatedWithManyChildren(this, parent, position);
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
	}

	internal sealed class WithManyChildren : WithManyChildrenBase
	{
		static WithManyChildren()
		{
			ObjectBinder.RegisterTypeReader(typeof(WithManyChildren), (ObjectReader r) => new WithManyChildren(r));
		}

		internal WithManyChildren(ArrayElement<GreenNode>[] children)
			: base(children)
		{
		}

		internal WithManyChildren(DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, ArrayElement<GreenNode>[] children)
			: base(diagnostics, annotations, children)
		{
		}

		internal WithManyChildren(ObjectReader reader)
			: base(reader)
		{
		}

		internal override GreenNode SetDiagnostics(DiagnosticInfo[] errors)
		{
			return new WithManyChildren(errors, GetAnnotations(), children);
		}

		internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new WithManyChildren(GetDiagnostics(), annotations, children);
		}
	}

	internal class WithThreeChildren : SyntaxList
	{
		private readonly GreenNode _child0;

		private readonly GreenNode _child1;

		private readonly GreenNode _child2;

		static WithThreeChildren()
		{
			ObjectBinder.RegisterTypeReader(typeof(WithThreeChildren), (ObjectReader r) => new WithThreeChildren(r));
		}

		internal WithThreeChildren(GreenNode child0, GreenNode child1, GreenNode child2)
		{
			SlotCount = 3;
			AdjustFlagsAndWidth(child0);
			_child0 = child0;
			AdjustFlagsAndWidth(child1);
			_child1 = child1;
			AdjustFlagsAndWidth(child2);
			_child2 = child2;
		}

		internal WithThreeChildren(DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, GreenNode child0, GreenNode child1, GreenNode child2)
			: base(diagnostics, annotations)
		{
			SlotCount = 3;
			AdjustFlagsAndWidth(child0);
			_child0 = child0;
			AdjustFlagsAndWidth(child1);
			_child1 = child1;
			AdjustFlagsAndWidth(child2);
			_child2 = child2;
		}

		internal WithThreeChildren(ObjectReader reader)
			: base(reader)
		{
			SlotCount = 3;
			_child0 = (GreenNode)reader.ReadValue();
			AdjustFlagsAndWidth(_child0);
			_child1 = (GreenNode)reader.ReadValue();
			AdjustFlagsAndWidth(_child1);
			_child2 = (GreenNode)reader.ReadValue();
			AdjustFlagsAndWidth(_child2);
		}

		internal override void WriteTo(ObjectWriter writer)
		{
			base.WriteTo(writer);
			writer.WriteValue(_child0);
			writer.WriteValue(_child1);
			writer.WriteValue(_child2);
		}

		internal override GreenNode GetSlot(int index)
		{
			return index switch
			{
				0 => _child0, 
				1 => _child1, 
				2 => _child2, 
				_ => null, 
			};
		}

		internal override void CopyTo(ArrayElement<GreenNode>[] array, int offset)
		{
			array[offset].Value = _child0;
			array[offset + 1].Value = _child1;
			array[offset + 2].Value = _child2;
		}

		internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
		{
			return new Microsoft.CodeAnalysis.Syntax.SyntaxList.WithThreeChildren(this, parent, position);
		}

		internal override GreenNode SetDiagnostics(DiagnosticInfo[] errors)
		{
			return new WithThreeChildren(errors, GetAnnotations(), _child0, _child1, _child2);
		}

		internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new WithThreeChildren(GetDiagnostics(), annotations, _child0, _child1, _child2);
		}
	}

	internal class WithTwoChildren : SyntaxList
	{
		private readonly GreenNode _child0;

		private readonly GreenNode _child1;

		static WithTwoChildren()
		{
			ObjectBinder.RegisterTypeReader(typeof(WithTwoChildren), (ObjectReader r) => new WithTwoChildren(r));
		}

		internal WithTwoChildren(GreenNode child0, GreenNode child1)
		{
			SlotCount = 2;
			AdjustFlagsAndWidth(child0);
			_child0 = child0;
			AdjustFlagsAndWidth(child1);
			_child1 = child1;
		}

		internal WithTwoChildren(DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, GreenNode child0, GreenNode child1)
			: base(diagnostics, annotations)
		{
			SlotCount = 2;
			AdjustFlagsAndWidth(child0);
			_child0 = child0;
			AdjustFlagsAndWidth(child1);
			_child1 = child1;
		}

		internal WithTwoChildren(ObjectReader reader)
			: base(reader)
		{
			SlotCount = 2;
			_child0 = (GreenNode)reader.ReadValue();
			AdjustFlagsAndWidth(_child0);
			_child1 = (GreenNode)reader.ReadValue();
			AdjustFlagsAndWidth(_child1);
		}

		internal override void WriteTo(ObjectWriter writer)
		{
			base.WriteTo(writer);
			writer.WriteValue(_child0);
			writer.WriteValue(_child1);
		}

		internal override GreenNode GetSlot(int index)
		{
			return index switch
			{
				0 => _child0, 
				1 => _child1, 
				_ => null, 
			};
		}

		internal override void CopyTo(ArrayElement<GreenNode>[] array, int offset)
		{
			array[offset].Value = _child0;
			array[offset + 1].Value = _child1;
		}

		internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
		{
			return new Microsoft.CodeAnalysis.Syntax.SyntaxList.WithTwoChildren(this, parent, position);
		}

		internal override GreenNode SetDiagnostics(DiagnosticInfo[] errors)
		{
			return new WithTwoChildren(errors, GetAnnotations(), _child0, _child1);
		}

		internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
		{
			return new WithTwoChildren(GetDiagnostics(), annotations, _child0, _child1);
		}
	}

	public sealed override string Language
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public sealed override string KindText
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal SyntaxList()
		: base(1)
	{
	}

	internal SyntaxList(DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(1, diagnostics, annotations)
	{
	}

	internal SyntaxList(ObjectReader reader)
		: base(reader)
	{
	}

	internal static GreenNode List(GreenNode child)
	{
		return child;
	}

	internal static WithTwoChildren List(GreenNode child0, GreenNode child1)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(1, child0, child1, out var hash);
		if (greenNode != null)
		{
			return (WithTwoChildren)greenNode;
		}
		WithTwoChildren withTwoChildren = new WithTwoChildren(child0, child1);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(withTwoChildren, hash);
		}
		return withTwoChildren;
	}

	internal static WithThreeChildren List(GreenNode child0, GreenNode child1, GreenNode child2)
	{
		GreenNode greenNode = SyntaxNodeCache.TryGetNode(1, child0, child1, child2, out var hash);
		if (greenNode != null)
		{
			return (WithThreeChildren)greenNode;
		}
		WithThreeChildren withThreeChildren = new WithThreeChildren(child0, child1, child2);
		if (hash >= 0)
		{
			SyntaxNodeCache.AddNode(withThreeChildren, hash);
		}
		return withThreeChildren;
	}

	internal static GreenNode List(GreenNode[] nodes)
	{
		return List(nodes, nodes.Length);
	}

	internal static GreenNode List(GreenNode[] nodes, int count)
	{
		ArrayElement<GreenNode>[] array = new ArrayElement<GreenNode>[count];
		for (int i = 0; i < count; i++)
		{
			array[i].Value = nodes[i];
		}
		return List(array);
	}

	internal static SyntaxList List(ArrayElement<GreenNode>[] children)
	{
		if (children.Length < 10)
		{
			return new WithManyChildren(children);
		}
		return new WithLotsOfChildren(children);
	}

	internal abstract void CopyTo(ArrayElement<GreenNode>[] array, int offset);

	internal static GreenNode Concat(GreenNode left, GreenNode right)
	{
		if (left == null)
		{
			return right;
		}
		if (right == null)
		{
			return left;
		}
		SyntaxList syntaxList = left as SyntaxList;
		SyntaxList syntaxList2 = right as SyntaxList;
		if (syntaxList != null)
		{
			if (syntaxList2 != null)
			{
				ArrayElement<GreenNode>[] array = new ArrayElement<GreenNode>[left.SlotCount + right.SlotCount];
				syntaxList.CopyTo(array, 0);
				syntaxList2.CopyTo(array, left.SlotCount);
				return List(array);
			}
			ArrayElement<GreenNode>[] array2 = new ArrayElement<GreenNode>[left.SlotCount + 1];
			syntaxList.CopyTo(array2, 0);
			array2[left.SlotCount].Value = right;
			return List(array2);
		}
		if (syntaxList2 != null)
		{
			ArrayElement<GreenNode>[] array3 = new ArrayElement<GreenNode>[syntaxList2.SlotCount + 1];
			array3[0].Value = left;
			syntaxList2.CopyTo(array3, 1);
			return List(array3);
		}
		return List(left, right);
	}

	public sealed override SyntaxNode GetStructure(SyntaxTrivia parentTrivia)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public sealed override SyntaxToken CreateSeparator<TNode>(SyntaxNode element)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public sealed override bool IsTriviaWithEndOfLine()
	{
		return false;
	}
}
internal struct SyntaxList<TNode> : IEquatable<SyntaxList<TNode>> where TNode : GreenNode
{
	internal struct Enumerator
	{
		private SyntaxList<TNode> _list;

		private int _index;

		public TNode Current => _list[_index];

		internal Enumerator(SyntaxList<TNode> list)
		{
			_list = list;
			_index = -1;
		}

		public bool MoveNext()
		{
			int num = _index + 1;
			if (num < _list.Count)
			{
				_index = num;
				return true;
			}
			return false;
		}
	}

	private readonly GreenNode _node;

	internal GreenNode Node => _node;

	public int Count
	{
		get
		{
			if (_node != null)
			{
				if (!_node.IsList)
				{
					return 1;
				}
				return _node.SlotCount;
			}
			return 0;
		}
	}

	public TNode this[int index]
	{
		get
		{
			if (_node == null)
			{
				return null;
			}
			if (_node.IsList)
			{
				return (TNode)_node.GetSlot(index);
			}
			if (index == 0)
			{
				return (TNode)_node;
			}
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal TNode[] Nodes
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

	public TNode Last
	{
		get
		{
			GreenNode node = _node;
			if (node.IsList)
			{
				return (TNode)node.GetSlot(node.SlotCount - 1);
			}
			return (TNode)node;
		}
	}

	internal SyntaxList(GreenNode node)
	{
		_node = node;
	}

	internal GreenNode ItemUntyped(int index)
	{
		GreenNode node = _node;
		if (node.IsList)
		{
			return node.GetSlot(index);
		}
		return node;
	}

	public bool Any()
	{
		return _node != null;
	}

	public bool Any(int kind)
	{
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.RawKind == kind)
			{
				return true;
			}
		}
		return false;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	internal void CopyTo(int offset, ArrayElement<GreenNode>[] array, int arrayOffset, int count)
	{
		for (int i = 0; i < count; i++)
		{
			array[arrayOffset + i].Value = this[i + offset];
		}
	}

	public static bool operator ==(SyntaxList<TNode> left, SyntaxList<TNode> right)
	{
		return left._node == right._node;
	}

	public static bool operator !=(SyntaxList<TNode> left, SyntaxList<TNode> right)
	{
		return left._node != right._node;
	}

	public bool Equals(SyntaxList<TNode> other)
	{
		return _node == other._node;
	}

	public override bool Equals(object obj)
	{
		if (obj is SyntaxList<TNode>)
		{
			return Equals((SyntaxList<TNode>)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (_node == null)
		{
			return 0;
		}
		return _node.GetHashCode();
	}

	public SeparatedSyntaxList<TOther> AsSeparatedList<TOther>() where TOther : GreenNode
	{
		return new SeparatedSyntaxList<TOther>(this);
	}

	public static implicit operator SyntaxList<TNode>(TNode node)
	{
		return new SyntaxList<TNode>(node);
	}

	public static implicit operator SyntaxList<TNode>(SyntaxList<GreenNode> nodes)
	{
		return new SyntaxList<TNode>(nodes._node);
	}

	public static implicit operator SyntaxList<GreenNode>(SyntaxList<TNode> nodes)
	{
		return new SyntaxList<GreenNode>(nodes.Node);
	}
}
