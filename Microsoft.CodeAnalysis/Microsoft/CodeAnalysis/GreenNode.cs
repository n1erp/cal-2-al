using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class GreenNode : IObjectWritable
{
	[Flags]
	internal enum NodeFlags : byte
	{
		None = 0,
		ContainsDiagnostics = 1,
		ContainsStructuredTrivia = 2,
		ContainsDirectives = 4,
		ContainsSkippedText = 8,
		ContainsAnnotations = 0x10,
		IsNotMissing = 0x20,
		FactoryContextIsInAsync = 0x40,
		FactoryContextIsInQuery = 0x80,
		FactoryContextIsInIterator = FactoryContextIsInQuery,
		InheritMask = ContainsDiagnostics | ContainsStructuredTrivia | ContainsDirectives | ContainsSkippedText | ContainsAnnotations | IsNotMissing
	}

	internal const int ListKind = 1;

	private readonly ushort _kind;

	protected NodeFlags flags;

	private byte _slotCount;

	private int _fullWidth;

	private static readonly ConditionalWeakTable<GreenNode, DiagnosticInfo[]> s_diagnosticsTable = new ConditionalWeakTable<GreenNode, DiagnosticInfo[]>();

	private static readonly ConditionalWeakTable<GreenNode, SyntaxAnnotation[]> s_annotationsTable = new ConditionalWeakTable<GreenNode, SyntaxAnnotation[]>();

	private static readonly DiagnosticInfo[] s_noDiagnostics = Array.Empty<DiagnosticInfo>();

	private static readonly SyntaxAnnotation[] s_noAnnotations = Array.Empty<SyntaxAnnotation>();

	private static readonly IEnumerable<SyntaxAnnotation> s_noAnnotationsEnumerable = SpecializedCollections.EmptyEnumerable<SyntaxAnnotation>();

	private const ushort ExtendedSerializationInfoMask = 32768;

	internal const int MaxCachedChildNum = 3;

	public abstract string Language { get; }

	public int RawKind => _kind;

	public bool IsList => RawKind == 1;

	public abstract string KindText { get; }

	public virtual bool IsStructuredTrivia => false;

	public virtual bool IsDirective => false;

	public virtual bool IsToken => false;

	public virtual bool IsTrivia => false;

	public virtual bool IsSkippedTokensTrivia => false;

	public virtual bool IsDocumentationCommentTrivia => false;

	public int SlotCount
	{
		get
		{
			int slotCount = _slotCount;
			if (slotCount == 255)
			{
				slotCount = GetSlotCount();
			}
			return slotCount;
		}
		protected set
		{
			_slotCount = (byte)value;
		}
	}

	internal NodeFlags Flags => flags;

	internal bool IsMissing => (flags & NodeFlags.IsNotMissing) == 0;

	internal bool ParsedInAsync => (flags & NodeFlags.FactoryContextIsInAsync) != 0;

	internal bool ParsedInQuery => (flags & NodeFlags.FactoryContextIsInQuery) != 0;

	internal bool ParsedInIterator => (flags & NodeFlags.FactoryContextIsInQuery) != 0;

	public bool ContainsSkippedText => (flags & NodeFlags.ContainsSkippedText) != 0;

	public bool ContainsStructuredTrivia => (flags & NodeFlags.ContainsStructuredTrivia) != 0;

	public bool ContainsDirectives => (flags & NodeFlags.ContainsDirectives) != 0;

	public bool ContainsDiagnostics => (flags & NodeFlags.ContainsDiagnostics) != 0;

	public bool ContainsAnnotations => (flags & NodeFlags.ContainsAnnotations) != 0;

	public int FullWidth
	{
		get
		{
			return _fullWidth;
		}
		protected set
		{
			_fullWidth = value;
		}
	}

	public virtual int Width => _fullWidth - GetLeadingTriviaWidth() - GetTrailingTriviaWidth();

	public bool HasLeadingTrivia => GetLeadingTriviaWidth() != 0;

	public bool HasTrailingTrivia => GetTrailingTriviaWidth() != 0;

	bool IObjectWritable.ShouldReuseInSerialization => ShouldReuseInSerialization;

	internal virtual bool ShouldReuseInSerialization => IsCacheable;

	public virtual int RawContextualKind => RawKind;

	internal bool IsCacheable
	{
		get
		{
			if ((flags & NodeFlags.InheritMask) == NodeFlags.IsNotMissing)
			{
				return SlotCount <= 3;
			}
			return false;
		}
	}

	private string GetDebuggerDisplay()
	{
		return GetType().Name + " " + KindText + " " + ToString();
	}

	protected GreenNode(ushort kind)
	{
		_kind = kind;
	}

	protected GreenNode(ushort kind, int fullWidth)
	{
		_kind = kind;
		_fullWidth = fullWidth;
	}

	protected GreenNode(ushort kind, DiagnosticInfo[] diagnostics, int fullWidth)
	{
		_kind = kind;
		_fullWidth = fullWidth;
		if (diagnostics != null && diagnostics.Length != 0)
		{
			flags |= NodeFlags.ContainsDiagnostics;
			s_diagnosticsTable.Add(this, diagnostics);
		}
	}

	protected GreenNode(ushort kind, DiagnosticInfo[] diagnostics)
	{
		_kind = kind;
		if (diagnostics != null && diagnostics.Length != 0)
		{
			flags |= NodeFlags.ContainsDiagnostics;
			s_diagnosticsTable.Add(this, diagnostics);
		}
	}

	protected GreenNode(ushort kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: this(kind, diagnostics)
	{
		if (annotations == null || annotations.Length == 0)
		{
			return;
		}
		for (int i = 0; i < annotations.Length; i++)
		{
			if (annotations[i] == null)
			{
				throw new ArgumentException("", "annotations");
			}
		}
		flags |= NodeFlags.ContainsAnnotations;
		s_annotationsTable.Add(this, annotations);
	}

	protected GreenNode(ushort kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, int fullWidth)
		: this(kind, diagnostics, fullWidth)
	{
		if (annotations == null || annotations.Length == 0)
		{
			return;
		}
		for (int i = 0; i < annotations.Length; i++)
		{
			if (annotations[i] == null)
			{
				throw new ArgumentException("", "annotations");
			}
		}
		flags |= NodeFlags.ContainsAnnotations;
		s_annotationsTable.Add(this, annotations);
	}

	protected void AdjustFlagsAndWidth(GreenNode node)
	{
		flags |= node.flags & NodeFlags.InheritMask;
		_fullWidth += node._fullWidth;
	}

	internal abstract GreenNode GetSlot(int index);

	protected virtual int GetSlotCount()
	{
		return _slotCount;
	}

	public virtual int GetSlotOffset(int index)
	{
		int num = 0;
		for (int i = 0; i < index; i++)
		{
			GreenNode slot = GetSlot(i);
			if (slot != null)
			{
				num += slot.FullWidth;
			}
		}
		return num;
	}

	internal Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList ChildNodesAndTokens()
	{
		return new Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList(this);
	}

	internal IEnumerable<GreenNode> EnumerateNodes()
	{
		yield return this;
		Stack<Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList.Enumerator> stack = new Stack<Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList.Enumerator>(24);
		stack.Push(ChildNodesAndTokens().GetEnumerator());
		while (stack.Count > 0)
		{
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList.Enumerator item = stack.Pop();
			if (item.MoveNext())
			{
				GreenNode current = item.Current;
				stack.Push(item);
				yield return current;
				if (!current.IsToken)
				{
					stack.Push(current.ChildNodesAndTokens().GetEnumerator());
				}
			}
		}
	}

	public virtual int FindSlotIndexContainingOffset(int offset)
	{
		int num = 0;
		int num2 = 0;
		while (true)
		{
			GreenNode slot = GetSlot(num2);
			if (slot != null)
			{
				num += slot.FullWidth;
				if (offset < num)
				{
					break;
				}
			}
			num2++;
		}
		return num2;
	}

	internal void SetFlags(NodeFlags flags)
	{
		this.flags |= flags;
	}

	internal void ClearFlags(NodeFlags flags)
	{
		this.flags &= (NodeFlags)(byte)(~(int)flags);
	}

	public virtual int GetLeadingTriviaWidth()
	{
		if (FullWidth == 0)
		{
			return 0;
		}
		return GetFirstTerminal().GetLeadingTriviaWidth();
	}

	public virtual int GetTrailingTriviaWidth()
	{
		if (FullWidth == 0)
		{
			return 0;
		}
		return GetLastTerminal().GetTrailingTriviaWidth();
	}

	internal GreenNode(ObjectReader reader)
	{
		ushort num = reader.ReadUInt16();
		_kind = (ushort)(num & -32769);
		if ((num & 0x8000) != 0)
		{
			DiagnosticInfo[] array = (DiagnosticInfo[])reader.ReadValue();
			if (array != null && array.Length != 0)
			{
				flags |= NodeFlags.ContainsDiagnostics;
				s_diagnosticsTable.Add(this, array);
			}
			SyntaxAnnotation[] array2 = (SyntaxAnnotation[])reader.ReadValue();
			if (array2 != null && array2.Length != 0)
			{
				flags |= NodeFlags.ContainsAnnotations;
				s_annotationsTable.Add(this, array2);
			}
		}
	}

	void IObjectWritable.WriteTo(ObjectWriter writer)
	{
		WriteTo(writer);
	}

	internal virtual void WriteTo(ObjectWriter writer)
	{
		ushort kind = _kind;
		bool flag = GetDiagnostics().Length != 0;
		bool flag2 = GetAnnotations().Length != 0;
		if (flag | flag2)
		{
			kind |= 0x8000;
			writer.WriteUInt16(kind);
			writer.WriteValue(flag ? GetDiagnostics() : null);
			writer.WriteValue(flag2 ? GetAnnotations() : null);
		}
		else
		{
			writer.WriteUInt16(kind);
		}
	}

	public bool HasAnnotations(string annotationKind)
	{
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == s_noAnnotations)
		{
			return false;
		}
		SyntaxAnnotation[] array = annotations;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Kind == annotationKind)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnnotations(IEnumerable<string> annotationKinds)
	{
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == s_noAnnotations)
		{
			return false;
		}
		SyntaxAnnotation[] array = annotations;
		foreach (SyntaxAnnotation syntaxAnnotation in array)
		{
			if (annotationKinds.Contains(syntaxAnnotation.Kind))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnnotation(SyntaxAnnotation annotation)
	{
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == s_noAnnotations)
		{
			return false;
		}
		SyntaxAnnotation[] array = annotations;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == annotation)
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(string annotationKind)
	{
		if (string.IsNullOrWhiteSpace(annotationKind))
		{
			throw new ArgumentNullException("annotationKind");
		}
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == s_noAnnotations)
		{
			return s_noAnnotationsEnumerable;
		}
		return GetAnnotationsSlow(annotations, annotationKind);
	}

	private static IEnumerable<SyntaxAnnotation> GetAnnotationsSlow(SyntaxAnnotation[] annotations, string annotationKind)
	{
		foreach (SyntaxAnnotation syntaxAnnotation in annotations)
		{
			if (syntaxAnnotation.Kind == annotationKind)
			{
				yield return syntaxAnnotation;
			}
		}
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(IEnumerable<string> annotationKinds)
	{
		if (annotationKinds == null)
		{
			throw new ArgumentNullException("annotationKinds");
		}
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == s_noAnnotations)
		{
			return s_noAnnotationsEnumerable;
		}
		return GetAnnotationsSlow(annotations, annotationKinds);
	}

	private static IEnumerable<SyntaxAnnotation> GetAnnotationsSlow(SyntaxAnnotation[] annotations, IEnumerable<string> annotationKinds)
	{
		foreach (SyntaxAnnotation syntaxAnnotation in annotations)
		{
			if (annotationKinds.Contains(syntaxAnnotation.Kind))
			{
				yield return syntaxAnnotation;
			}
		}
	}

	public SyntaxAnnotation[] GetAnnotations()
	{
		if (ContainsAnnotations && s_annotationsTable.TryGetValue(this, out var value))
		{
			return value;
		}
		return s_noAnnotations;
	}

	internal abstract GreenNode SetAnnotations(SyntaxAnnotation[] annotations);

	internal DiagnosticInfo[] GetDiagnostics()
	{
		if (ContainsDiagnostics && s_diagnosticsTable.TryGetValue(this, out var value))
		{
			return value;
		}
		return s_noDiagnostics;
	}

	internal abstract GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics);

	public virtual string ToFullString()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringWriter writer = new StringWriter(instance.Builder, CultureInfo.InvariantCulture);
		WriteTo(writer, leading: true, trailing: true);
		return instance.ToStringAndFree();
	}

	public override string ToString()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringWriter writer = new StringWriter(instance.Builder, CultureInfo.InvariantCulture);
		WriteTo(writer, leading: false, trailing: false);
		return instance.ToStringAndFree();
	}

	public void WriteTo(TextWriter writer)
	{
		WriteTo(writer, leading: true, trailing: true);
	}

	protected internal void WriteTo(TextWriter writer, bool leading, bool trailing)
	{
		Stack<(GreenNode, bool, bool)> stack = new Stack<(GreenNode, bool, bool)>();
		stack.Push((this, leading, trailing));
		ProcessStack(writer, stack);
	}

	private static void ProcessStack(TextWriter writer, Stack<(GreenNode node, bool leading, bool trailing)> stack)
	{
		while (stack.Count > 0)
		{
			var (greenNode, flag, flag2) = stack.Pop();
			if (greenNode.IsToken)
			{
				greenNode.WriteTokenTo(writer, flag, flag2);
				continue;
			}
			if (greenNode.IsTrivia)
			{
				greenNode.WriteTriviaTo(writer);
				continue;
			}
			int firstNonNullChildIndex = GetFirstNonNullChildIndex(greenNode);
			int lastNonNullChildIndex = GetLastNonNullChildIndex(greenNode);
			for (int num = lastNonNullChildIndex; num >= firstNonNullChildIndex; num--)
			{
				GreenNode slot = greenNode.GetSlot(num);
				if (slot != null)
				{
					bool flag3 = num == firstNonNullChildIndex;
					bool flag4 = num == lastNonNullChildIndex;
					stack.Push((slot, flag | !flag3, flag2 | !flag4));
				}
			}
		}
	}

	private static int GetFirstNonNullChildIndex(GreenNode node)
	{
		int slotCount = node.SlotCount;
		int i;
		for (i = 0; i < slotCount && node.GetSlot(i) == null; i++)
		{
		}
		return i;
	}

	private static int GetLastNonNullChildIndex(GreenNode node)
	{
		int num = node.SlotCount - 1;
		while (num >= 0 && node.GetSlot(num) == null)
		{
			num--;
		}
		return num;
	}

	protected virtual void WriteTriviaTo(TextWriter writer)
	{
		throw new NotImplementedException();
	}

	protected virtual void WriteTokenTo(TextWriter writer, bool leading, bool trailing)
	{
		throw new NotImplementedException();
	}

	public virtual object GetValue()
	{
		return null;
	}

	public virtual string GetValueText()
	{
		return string.Empty;
	}

	public virtual GreenNode GetLeadingTriviaCore()
	{
		return null;
	}

	public virtual GreenNode GetTrailingTriviaCore()
	{
		return null;
	}

	public virtual GreenNode WithLeadingTrivia(GreenNode trivia)
	{
		return this;
	}

	public virtual GreenNode WithTrailingTrivia(GreenNode trivia)
	{
		return this;
	}

	internal GreenNode GetFirstTerminal()
	{
		GreenNode greenNode = this;
		do
		{
			GreenNode greenNode2 = null;
			int i = 0;
			for (int slotCount = greenNode.SlotCount; i < slotCount; i++)
			{
				GreenNode slot = greenNode.GetSlot(i);
				if (slot != null)
				{
					greenNode2 = slot;
					break;
				}
			}
			greenNode = greenNode2;
		}
		while (greenNode?._slotCount > 0);
		return greenNode;
	}

	internal GreenNode GetLastTerminal()
	{
		GreenNode greenNode = this;
		do
		{
			GreenNode greenNode2 = null;
			for (int num = greenNode.SlotCount - 1; num >= 0; num--)
			{
				GreenNode slot = greenNode.GetSlot(num);
				if (slot != null)
				{
					greenNode2 = slot;
					break;
				}
			}
			greenNode = greenNode2;
		}
		while (greenNode?._slotCount > 0);
		return greenNode;
	}

	internal GreenNode GetLastNonmissingTerminal()
	{
		GreenNode greenNode = this;
		do
		{
			GreenNode greenNode2 = null;
			for (int num = greenNode.SlotCount - 1; num >= 0; num--)
			{
				GreenNode slot = greenNode.GetSlot(num);
				if (slot != null && !slot.IsMissing)
				{
					greenNode2 = slot;
					break;
				}
			}
			greenNode = greenNode2;
		}
		while (greenNode?._slotCount > 0);
		return greenNode;
	}

	public virtual bool IsEquivalentTo(GreenNode other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return EquivalentToInternal(this, other);
	}

	private static bool EquivalentToInternal(GreenNode node1, GreenNode node2)
	{
		if (node1.RawKind != node2.RawKind)
		{
			if (node1.IsList && node1.SlotCount == 1)
			{
				node1 = node1.GetSlot(0);
			}
			if (node2.IsList && node2.SlotCount == 1)
			{
				node2 = node2.GetSlot(0);
			}
			if (node1.RawKind != node2.RawKind)
			{
				return false;
			}
		}
		if (node1._fullWidth != node2._fullWidth)
		{
			return false;
		}
		int slotCount = node1.SlotCount;
		if (slotCount != node2.SlotCount)
		{
			return false;
		}
		for (int i = 0; i < slotCount; i++)
		{
			GreenNode slot = node1.GetSlot(i);
			GreenNode slot2 = node2.GetSlot(i);
			if (slot != null && slot2 != null && !slot.IsEquivalentTo(slot2))
			{
				return false;
			}
		}
		return true;
	}

	public abstract SyntaxNode GetStructure(SyntaxTrivia parentTrivia);

	public abstract SyntaxToken CreateSeparator<TNode>(SyntaxNode element) where TNode : SyntaxNode;

	public abstract bool IsTriviaWithEndOfLine();

	public virtual GreenNode CreateList(IEnumerable<GreenNode> nodes, bool alwaysCreateListNode = false)
	{
		if (nodes == null)
		{
			return null;
		}
		GreenNode[] array = nodes.ToArray();
		switch (array.Length)
		{
		case 0:
			return null;
		case 1:
			if (!alwaysCreateListNode)
			{
				return array[0];
			}
			break;
		case 2:
			return SyntaxList.List(array[0], array[1]);
		case 3:
			return SyntaxList.List(array[0], array[1], array[2]);
		}
		return SyntaxList.List(array);
	}

	public SyntaxNode CreateRed()
	{
		return CreateRed(null, 0);
	}

	internal abstract SyntaxNode CreateRed(SyntaxNode parent, int position);

	internal int GetCacheHash()
	{
		int num = (int)flags ^ RawKind;
		int slotCount = SlotCount;
		for (int i = 0; i < slotCount; i++)
		{
			GreenNode slot = GetSlot(i);
			if (slot != null)
			{
				num = Hash.Combine(RuntimeHelpers.GetHashCode(slot), num);
			}
		}
		return num & 0x7FFFFFFF;
	}

	internal bool IsCacheEquivalent(int kind, NodeFlags flags, GreenNode child1)
	{
		if (RawKind == kind && this.flags == flags)
		{
			return GetSlot(0) == child1;
		}
		return false;
	}

	internal bool IsCacheEquivalent(int kind, NodeFlags flags, GreenNode child1, GreenNode child2)
	{
		if (RawKind == kind && this.flags == flags && GetSlot(0) == child1)
		{
			return GetSlot(1) == child2;
		}
		return false;
	}

	internal bool IsCacheEquivalent(int kind, NodeFlags flags, GreenNode child1, GreenNode child2, GreenNode child3)
	{
		if (RawKind == kind && this.flags == flags && GetSlot(0) == child1 && GetSlot(1) == child2)
		{
			return GetSlot(2) == child3;
		}
		return false;
	}

	internal GreenNode AddError(DiagnosticInfo err)
	{
		DiagnosticInfo[] array;
		if (GetDiagnostics() == null)
		{
			array = new DiagnosticInfo[1] { err };
		}
		else
		{
			array = GetDiagnostics();
			int num = array.Length;
			Array.Resize(ref array, num + 1);
			array[num] = err;
		}
		return SetDiagnostics(array);
	}
}
