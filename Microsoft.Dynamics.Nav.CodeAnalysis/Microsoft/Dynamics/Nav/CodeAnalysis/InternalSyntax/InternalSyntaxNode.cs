using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class InternalSyntaxNode
{
	[Flags]
	internal enum NodeFlags : byte
	{
		None = 0,
		ContainsDiagnostics = 1,
		ContainsStructuredTrivia = 2,
		ContainsSkippedText = 8,
		ContainsAnnotations = 0x10,
		IsNotMissing = 0x20
	}

	private static readonly ConditionalWeakTable<SyntaxNode, Dictionary<SyntaxTrivia, SyntaxNode>> StructuresTable = new ConditionalWeakTable<SyntaxNode, Dictionary<SyntaxTrivia, SyntaxNode>>();

	private byte slotCount;

	private static readonly ConditionalWeakTable<InternalSyntaxNode, DiagnosticInfo[]> DiagnosticsTable = new ConditionalWeakTable<InternalSyntaxNode, DiagnosticInfo[]>();

	private static readonly ConditionalWeakTable<InternalSyntaxNode, SyntaxAnnotation[]> AnnotationsTable = new ConditionalWeakTable<InternalSyntaxNode, SyntaxAnnotation[]>();

	private static readonly DiagnosticInfo[] NoDiagnostics = SpecializedCollections.EmptyArray<DiagnosticInfo>();

	private static readonly SyntaxAnnotation[] NoAnnotations = SpecializedCollections.EmptyArray<SyntaxAnnotation>();

	private static readonly IEnumerable<SyntaxAnnotation> NoAnnotationsEnumerable = SpecializedCollections.EmptyEnumerable<SyntaxAnnotation>();

	internal const int MaxCachedChildNum = 3;

	internal LexerMode LexedMode { get; private set; }

	internal SyntaxKind Kind { get; private set; }

	protected NodeFlags Flags { get; set; }

	internal virtual SyntaxNodeDefinition Definition => null;

	internal string KindText => Kind.ToString();

	public int FullWidth { get; protected set; }

	public static AbstractSyntaxNavigator Navigator => SyntaxNavigator.Instance;

	public virtual int Width => FullWidth - GetLeadingTriviaWidth() - GetTrailingTriviaWidth();

	public bool IsList => Kind == SyntaxKind.List;

	public bool ContainsAnnotations => (Flags & NodeFlags.ContainsAnnotations) != 0;

	public bool ContainsStructuredTrivia => (Flags & NodeFlags.ContainsStructuredTrivia) != 0;

	public bool ContainsDiagnostics => (Flags & NodeFlags.ContainsDiagnostics) != 0;

	public bool ContainsSkippedText => (Flags & NodeFlags.ContainsSkippedText) != 0;

	internal bool IsMissing => (Flags & NodeFlags.IsNotMissing) == 0;

	public virtual bool IsToken => false;

	public virtual bool IsStructuredTrivia => false;

	public int SlotCount
	{
		get
		{
			int num = slotCount;
			if (num == 255)
			{
				num = GetSlotCount();
			}
			return num;
		}
		protected set
		{
			slotCount = (byte)value;
		}
	}

	internal bool IsCacheable
	{
		get
		{
			if (Flags == NodeFlags.IsNotMissing)
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

	protected InternalSyntaxNode(SyntaxKind kind)
	{
		LexedMode = LexerMode.Default;
		Kind = kind;
	}

	protected InternalSyntaxNode(SyntaxKind kind, LexerMode mode)
		: this(kind)
	{
		LexedMode = mode;
	}

	protected InternalSyntaxNode(SyntaxKind kind, int fullWidth, LexerMode mode)
	{
		LexedMode = mode;
		Kind = kind;
		FullWidth = fullWidth;
	}

	protected InternalSyntaxNode(SyntaxKind kind, int fullWidth)
		: this(kind, fullWidth, LexerMode.Default)
	{
	}

	protected InternalSyntaxNode(SyntaxKind kind, SyntaxAnnotation[] annotations)
	{
		LexedMode = LexerMode.Default;
		Kind = kind;
		if (annotations == null || annotations.Length == 0)
		{
			return;
		}
		for (int i = 0; i < annotations.Length; i++)
		{
			if (annotations[i] == null)
			{
				throw new ArgumentException("annotation");
			}
		}
		Flags |= NodeFlags.ContainsAnnotations;
		AnnotationsTable.Add(this, annotations);
	}

	protected InternalSyntaxNode(SyntaxKind kind, SyntaxAnnotation[] annotations, int fullWidth)
	{
		LexedMode = LexerMode.Default;
		Kind = kind;
		FullWidth = fullWidth;
		if (annotations == null || annotations.Length == 0)
		{
			return;
		}
		for (int i = 0; i < annotations.Length; i++)
		{
			if (annotations[i] == null)
			{
				throw new ArgumentException("annotation");
			}
		}
		Flags |= NodeFlags.ContainsAnnotations;
		AnnotationsTable.Add(this, annotations);
	}

	protected InternalSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics, int fullWidth)
	{
		LexedMode = LexerMode.Default;
		Kind = kind;
		FullWidth = fullWidth;
		if (diagnostics != null && diagnostics.Length != 0)
		{
			Flags |= NodeFlags.ContainsDiagnostics;
			DiagnosticsTable.Add(this, diagnostics);
		}
	}

	protected InternalSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics)
	{
		LexedMode = LexerMode.Default;
		Kind = kind;
		if (diagnostics != null && diagnostics.Length != 0)
		{
			Flags |= NodeFlags.ContainsDiagnostics;
			DiagnosticsTable.Add(this, diagnostics);
		}
	}

	protected InternalSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: this(kind, diagnostics)
	{
		if (annotations != null && annotations.Length != 0)
		{
			if (annotations.Any((SyntaxAnnotation annotation) => annotation == null))
			{
				throw new ArgumentException(string.Empty, "annotations");
			}
			Flags |= NodeFlags.ContainsAnnotations;
			AnnotationsTable.Add(this, annotations);
		}
	}

	protected InternalSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, int fullWidth)
		: this(kind, diagnostics, fullWidth)
	{
		if (annotations != null && annotations.Length != 0)
		{
			if (annotations.Any((SyntaxAnnotation annotation) => annotation == null))
			{
				throw new ArgumentException(string.Empty, "annotations");
			}
			Flags |= NodeFlags.ContainsAnnotations;
			AnnotationsTable.Add(this, annotations);
		}
	}

	public override string ToString()
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		WriteTo(stringWriter, leading: false, trailing: false);
		return stringWriter.ToString();
	}

	public string ToFullString()
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		WriteTo(stringWriter, leading: true, trailing: true);
		return stringWriter.ToString();
	}

	public string ToString(bool leading, bool trailing)
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		WriteTo(stringWriter, leading, trailing);
		return stringWriter.ToString();
	}

	protected void AdjustFlagsAndWidth(InternalSyntaxNode node)
	{
		DebugAssertHelper.Assert(node != null, "PERF: caller must ensure that node!=null, we do not want to re-check that here.");
		Flags |= node.Flags;
		FullWidth += node.FullWidth;
	}

	public virtual int GetLeadingTriviaWidth()
	{
		if (FullWidth == 0)
		{
			return 0;
		}
		return GetFirstTerminal()?.GetLeadingTriviaWidth() ?? 0;
	}

	public virtual int GetTrailingTriviaWidth()
	{
		if (FullWidth == 0)
		{
			return 0;
		}
		return GetLastTerminal()?.GetTrailingTriviaWidth() ?? 0;
	}

	public virtual InternalSyntaxNode WithLeadingTriviaBase(InternalSyntaxNode trivia)
	{
		return this;
	}

	public virtual InternalSyntaxNode WithTrailingTriviaBase(InternalSyntaxNode trivia)
	{
		return this;
	}

	internal virtual InternalSyntaxNode WithLeadingTrivia(InternalSyntaxNode trivia)
	{
		return this;
	}

	internal virtual InternalSyntaxNode WithTrailingTrivia(InternalSyntaxNode trivia)
	{
		return this;
	}

	public virtual int GetSlotOffset(int index)
	{
		DebugAssertHelper.Assert(index < 11);
		int num = 0;
		for (int i = 0; i < index; i++)
		{
			InternalSyntaxNode slot = GetSlot(i);
			if (slot != null)
			{
				num += slot.FullWidth;
			}
		}
		return num;
	}

	public virtual int FindSlotIndexContainingOffset(int offset)
	{
		DebugAssertHelper.Assert(0 <= offset && offset < FullWidth);
		int num = 0;
		int num2 = 0;
		while (true)
		{
			DebugAssertHelper.Assert(num2 < SlotCount);
			InternalSyntaxNode slot = GetSlot(num2);
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

	internal InternalSyntaxNode GetFirstTerminal()
	{
		InternalSyntaxNode internalSyntaxNode = this;
		do
		{
			InternalSyntaxNode internalSyntaxNode2 = null;
			int i = 0;
			for (int num = internalSyntaxNode.SlotCount; i < num; i++)
			{
				InternalSyntaxNode slot = internalSyntaxNode.GetSlot(i);
				if (slot != null)
				{
					internalSyntaxNode2 = slot;
					break;
				}
			}
			internalSyntaxNode = internalSyntaxNode2;
		}
		while (internalSyntaxNode != null && internalSyntaxNode.slotCount > 0);
		return internalSyntaxNode;
	}

	internal InternalSyntaxNode GetLastTerminal()
	{
		InternalSyntaxNode internalSyntaxNode = this;
		do
		{
			InternalSyntaxNode internalSyntaxNode2 = null;
			for (int num = internalSyntaxNode.SlotCount - 1; num >= 0; num--)
			{
				InternalSyntaxNode slot = internalSyntaxNode.GetSlot(num);
				if (slot != null)
				{
					internalSyntaxNode2 = slot;
					break;
				}
			}
			internalSyntaxNode = internalSyntaxNode2;
		}
		while (internalSyntaxNode != null && internalSyntaxNode.slotCount > 0);
		return internalSyntaxNode;
	}

	internal InternalSyntaxNode GetLastNonmissingTerminal()
	{
		InternalSyntaxNode internalSyntaxNode = this;
		do
		{
			InternalSyntaxNode internalSyntaxNode2 = null;
			for (int num = internalSyntaxNode.SlotCount - 1; num >= 0; num--)
			{
				InternalSyntaxNode slot = internalSyntaxNode.GetSlot(num);
				if (slot != null && !slot.IsMissing)
				{
					internalSyntaxNode2 = slot;
					break;
				}
			}
			internalSyntaxNode = internalSyntaxNode2;
		}
		while ((internalSyntaxNode?.slotCount ?? 0) > 0);
		return internalSyntaxNode;
	}

	public virtual object GetValue()
	{
		return null;
	}

	public virtual string GetValueText()
	{
		return string.Empty;
	}

	internal InternalChildSyntaxList ChildNodesAndTokens()
	{
		return new InternalChildSyntaxList(this);
	}

	public virtual SyntaxToken CreateSeparator<TNode>(SyntaxNode element)
	{
		return SyntaxFactory.Token(SyntaxKind.CommaToken);
	}

	public virtual bool IsTriviaWithEndOfLine()
	{
		if (Kind != SyntaxKind.EndOfLineTrivia)
		{
			return Kind == SyntaxKind.LineCommentTrivia;
		}
		return true;
	}

	public SyntaxNode CreateRed()
	{
		return CreateRed(null, 0);
	}

	internal abstract SyntaxNode CreateRed(SyntaxNode parent, int position);

	public abstract TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor);

	public abstract void Accept(InternalSyntaxVisitor visitor);

	internal virtual InternalSyntaxNode CreateList(IEnumerable<InternalSyntaxNode> nodes, bool alwaysCreateListNode = false)
	{
		if (nodes == null)
		{
			return null;
		}
		InternalSyntaxNode[] array = nodes.Select((InternalSyntaxNode n) => n).ToArray();
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
			return InternalSyntaxList.List(array[0], array[1]);
		case 3:
			return InternalSyntaxList.List(array[0], array[1], array[2]);
		}
		return InternalSyntaxList.List(array);
	}

	internal InternalSyntaxList<T> ToList<T>() where T : InternalSyntaxNode
	{
		return new InternalSyntaxList<T>(this);
	}

	public virtual bool IsEquivalentTo(InternalSyntaxNode other)
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

	private static bool EquivalentToInternal(InternalSyntaxNode node1, InternalSyntaxNode node2)
	{
		if (node1.Kind != node2.Kind)
		{
			if (node1.IsList && node1.SlotCount == 1)
			{
				node1 = node1.GetSlot(0);
			}
			if (node2.IsList && node2.SlotCount == 1)
			{
				node2 = node2.GetSlot(0);
			}
			if (node1.Kind != node2.Kind)
			{
				return false;
			}
		}
		if (node1.FullWidth != node2.FullWidth)
		{
			return false;
		}
		int num = node1.SlotCount;
		if (num != node2.SlotCount)
		{
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			InternalSyntaxNode slot = node1.GetSlot(i);
			InternalSyntaxNode slot2 = node2.GetSlot(i);
			if (slot != null && slot2 != null && !slot.IsEquivalentTo(slot2))
			{
				return false;
			}
		}
		return true;
	}

	public virtual InternalSyntaxNode GetLeadingTrivia()
	{
		return null;
	}

	public virtual InternalSyntaxNode GetTrailingTrivia()
	{
		return null;
	}

	public static SyntaxNode GetStructure(SyntaxTrivia trivia)
	{
		if (trivia.HasStructure)
		{
			SyntaxNode parent = trivia.Token.Parent;
			if (parent != null)
			{
				Dictionary<SyntaxTrivia, SyntaxNode> orCreateValue = StructuresTable.GetOrCreateValue(parent);
				SyntaxNode value;
				lock (orCreateValue)
				{
					if (!orCreateValue.TryGetValue(trivia, out value))
					{
						value = StructuredTriviaSyntax.Create(trivia);
						orCreateValue.Add(trivia, value);
					}
				}
				return value;
			}
			return StructuredTriviaSyntax.Create(trivia);
		}
		return null;
	}

	public bool HasAnnotations(AnnotationKind annotationKind)
	{
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == NoAnnotations)
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

	public bool HasAnnotations(IEnumerable<AnnotationKind> annotationKinds)
	{
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == NoAnnotations)
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
		if (annotations == NoAnnotations)
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

	public IEnumerable<SyntaxAnnotation> GetAnnotations(AnnotationKind annotationKind)
	{
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == NoAnnotations)
		{
			return NoAnnotationsEnumerable;
		}
		return GetAnnotationsSlow(annotations, annotationKind);
	}

	private static IEnumerable<SyntaxAnnotation> GetAnnotationsSlow(SyntaxAnnotation[] annotations, AnnotationKind annotationKind)
	{
		foreach (SyntaxAnnotation syntaxAnnotation in annotations)
		{
			if (syntaxAnnotation.Kind == annotationKind)
			{
				yield return syntaxAnnotation;
			}
		}
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(IEnumerable<AnnotationKind> annotationKinds)
	{
		SyntaxAnnotation[] annotations = GetAnnotations();
		if (annotations == NoAnnotations)
		{
			return NoAnnotationsEnumerable;
		}
		return GetAnnotationsSlow(annotations, annotationKinds);
	}

	private static IEnumerable<SyntaxAnnotation> GetAnnotationsSlow(SyntaxAnnotation[] annotations, IEnumerable<AnnotationKind> annotationKinds)
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
		if (ContainsAnnotations && AnnotationsTable.TryGetValue(this, out var value))
		{
			DebugAssertHelper.Assert(value.Length != 0, "we should return nonempty annotations or NoAnnotations");
			return value;
		}
		return NoAnnotations;
	}

	internal virtual InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		throw new NotImplementedException();
	}

	public TNode WithAnnotations<TNode>(IEnumerable<SyntaxAnnotation> annotations) where TNode : InternalSyntaxNode
	{
		List<SyntaxAnnotation> list = new List<SyntaxAnnotation>();
		foreach (SyntaxAnnotation annotation in annotations)
		{
			if (!list.Contains(annotation))
			{
				list.Add(annotation);
			}
		}
		if (list.Count == 0)
		{
			SyntaxAnnotation[] annotations2 = GetAnnotations();
			if (annotations2 == null || annotations2.Length == 0)
			{
				return (TNode)this;
			}
			return (TNode)SetAnnotations(null);
		}
		return (TNode)SetAnnotations(list.ToArray());
	}

	internal DiagnosticInfo[] GetDiagnostics()
	{
		if (ContainsDiagnostics && DiagnosticsTable.TryGetValue(this, out var value))
		{
			return value;
		}
		return NoDiagnostics;
	}

	internal abstract InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics);

	internal abstract InternalSyntaxNode GetSlot(int index);

	protected virtual int GetSlotCount()
	{
		return slotCount;
	}

	public virtual void WriteTo(TextWriter writer)
	{
		WriteTo(writer, leading: true, trailing: true);
	}

	protected internal virtual void WriteTo(TextWriter writer, bool leading, bool trailing)
	{
		bool flag = true;
		int num = SlotCount - 1;
		while (num >= 0 && GetSlot(num) == null)
		{
			num--;
		}
		for (int i = 0; i <= num; i++)
		{
			InternalSyntaxNode slot = GetSlot(i);
			if (slot != null)
			{
				slot.WriteTo(writer, leading | !flag, trailing | (i < num));
				flag = false;
			}
		}
	}

	internal IEnumerable<InternalSyntaxNode> EnumerateNodes()
	{
		yield return this;
		Stack<InternalChildSyntaxList.Enumerator> stack = new Stack<InternalChildSyntaxList.Enumerator>(24);
		stack.Push(ChildNodesAndTokens().GetEnumerator());
		while (stack.Count > 0)
		{
			InternalChildSyntaxList.Enumerator item = stack.Pop();
			if (item.MoveNext())
			{
				InternalSyntaxNode current = item.Current;
				stack.Push(item);
				yield return current;
				if (!(current is InternalSyntaxToken))
				{
					stack.Push(current.ChildNodesAndTokens().GetEnumerator());
				}
			}
		}
	}

	public InternalSyntaxToken GetFirstToken()
	{
		return (InternalSyntaxToken)GetFirstTerminal();
	}

	public InternalSyntaxToken GetLastToken()
	{
		return (InternalSyntaxToken)GetLastTerminal();
	}

	public InternalSyntaxToken GetLastNonmissingToken()
	{
		return (InternalSyntaxToken)GetLastNonmissingTerminal();
	}

	internal int GetCacheHash()
	{
		DebugAssertHelper.Assert(IsCacheable);
		int num = (int)((uint)Flags ^ (uint)Kind) ^ (int)LexedMode;
		int num2 = SlotCount;
		for (int i = 0; i < num2; i++)
		{
			InternalSyntaxNode slot = GetSlot(i);
			if (slot != null)
			{
				num = Hash.Combine(RuntimeHelpers.GetHashCode(slot), num);
			}
		}
		return num & 0x7FFFFFFF;
	}

	internal bool IsCacheEquivalent(int kind, NodeFlags flagsParameter, LexerMode mode, InternalSyntaxNode child1)
	{
		DebugAssertHelper.Assert(IsCacheable);
		if (Kind == (SyntaxKind)kind && LexedMode == mode && Flags == flagsParameter)
		{
			return GetSlot(0) == child1;
		}
		return false;
	}

	internal bool IsCacheEquivalent(int kind, NodeFlags flagsParameter, LexerMode mode, InternalSyntaxNode child1, InternalSyntaxNode child2)
	{
		DebugAssertHelper.Assert(IsCacheable);
		if (Kind == (SyntaxKind)kind && LexedMode == mode && Flags == flagsParameter && GetSlot(0) == child1)
		{
			return GetSlot(1) == child2;
		}
		return false;
	}

	internal bool IsCacheEquivalent(int kind, NodeFlags flagsParameter, LexerMode mode, InternalSyntaxNode child1, InternalSyntaxNode child2, InternalSyntaxNode child3)
	{
		DebugAssertHelper.Assert(IsCacheable);
		if (Kind == (SyntaxKind)kind && LexedMode == mode && Flags == flagsParameter && GetSlot(0) == child1 && GetSlot(1) == child2)
		{
			return GetSlot(2) == child3;
		}
		return false;
	}
}
