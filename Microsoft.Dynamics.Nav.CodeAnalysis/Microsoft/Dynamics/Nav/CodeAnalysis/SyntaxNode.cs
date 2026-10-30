using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public abstract class SyntaxNode
{
	private struct ChildSyntaxListEnumeratorStack : IDisposable
	{
		private static readonly ObjectPool<ChildSyntaxList.Enumerator[]> stackPool = new ObjectPool<ChildSyntaxList.Enumerator[]>(() => new ChildSyntaxList.Enumerator[16]);

		private ChildSyntaxList.Enumerator[] stack;

		private int stackPtr;

		public bool IsNotEmpty => stackPtr >= 0;

		public ChildSyntaxListEnumeratorStack(SyntaxNode startingNode, Func<SyntaxNode, bool> descendIntoChildren)
		{
			if (descendIntoChildren == null || descendIntoChildren(startingNode))
			{
				stack = stackPool.Allocate();
				stackPtr = 0;
				stack[0].InitializeFrom(startingNode);
			}
			else
			{
				stack = null;
				stackPtr = -1;
			}
		}

		public bool TryGetNextInSpan(ref TextSpan span, out SyntaxNodeOrToken value)
		{
			value = default;
			while (stack[stackPtr].TryMoveNextAndGetCurrent(ref value))
			{
				if (IsInSpan(ref span, value.FullSpan))
				{
					return true;
				}
			}
			stackPtr--;
			return false;
		}

		public SyntaxNode TryGetNextAsNodeInSpan(ref TextSpan span)
		{
			SyntaxNode syntaxNode;
			while ((syntaxNode = stack[stackPtr].TryMoveNextAndGetCurrentAsNode()) != null)
			{
				if (IsInSpan(ref span, syntaxNode.FullSpan))
				{
					return syntaxNode;
				}
			}
			stackPtr--;
			return null;
		}

		public void PushChildren(SyntaxNode node)
		{
			if (++stackPtr >= stack.Length)
			{
				Array.Resize(ref stack, checked(stackPtr * 2));
			}
			stack[stackPtr].InitializeFrom(node);
		}

		public void PushChildren(SyntaxNode node, Func<SyntaxNode, bool> descendIntoChildren)
		{
			if (descendIntoChildren == null || descendIntoChildren(node))
			{
				PushChildren(node);
			}
		}

		public void Dispose()
		{
			if (stack != null && stack.Length < 256)
			{
				Array.Clear(stack, 0, stack.Length);
				stackPool.Free(stack);
			}
		}
	}

	private struct TriviaListEnumeratorStack : IDisposable
	{
		private static readonly ObjectPool<SyntaxTriviaList.Enumerator[]> sstackPool = new ObjectPool<SyntaxTriviaList.Enumerator[]>(() => new SyntaxTriviaList.Enumerator[16]);

		private SyntaxTriviaList.Enumerator[] stack;

		private int stackPtr;

		public bool TryGetNext(out SyntaxTrivia value)
		{
			value = default;
			if (stack[stackPtr].TryMoveNextAndGetCurrent(ref value))
			{
				return true;
			}
			stackPtr--;
			return false;
		}

		public void PushLeadingTrivia(ref SyntaxToken token)
		{
			Grow();
			stack[stackPtr].InitializeFromLeadingTrivia(ref token);
		}

		public void PushTrailingTrivia(ref SyntaxToken token)
		{
			Grow();
			stack[stackPtr].InitializeFromTrailingTrivia(ref token);
		}

		private void Grow()
		{
			if (stack == null)
			{
				stack = sstackPool.Allocate();
				stackPtr = -1;
			}
			if (++stackPtr >= stack.Length)
			{
				Array.Resize(ref stack, checked(stackPtr * 2));
			}
		}

		public void Dispose()
		{
			SyntaxTriviaList.Enumerator[] array = stack;
			if (array != null && array.Length < 256)
			{
				Array.Clear(stack, 0, stack.Length);
				sstackPool.Free(stack);
			}
		}
	}

	private struct TwoEnumeratorListStack : IDisposable
	{
		public enum Which : byte
		{
			Node,
			Trivia
		}

		private ChildSyntaxListEnumeratorStack nodeStack;

		private TriviaListEnumeratorStack triviaStack;

		private readonly ArrayBuilder<Which> discriminatorStack;

		public bool IsNotEmpty
		{
			get
			{
				ArrayBuilder<Which> arrayBuilder = discriminatorStack;
				if (arrayBuilder == null)
				{
					return false;
				}
				return arrayBuilder.Count > 0;
			}
		}

		public TwoEnumeratorListStack(SyntaxNode startingNode, Func<SyntaxNode, bool> descendIntoChildren)
		{
			nodeStack = new ChildSyntaxListEnumeratorStack(startingNode, descendIntoChildren);
			triviaStack = default;
			if (nodeStack.IsNotEmpty)
			{
				discriminatorStack = ArrayBuilder<Which>.GetInstance();
				discriminatorStack.Push(Which.Node);
			}
			else
			{
				discriminatorStack = null;
			}
		}

		public Which PeekNext()
		{
			return discriminatorStack.Peek();
		}

		public bool TryGetNextInSpan(ref TextSpan span, out SyntaxNodeOrToken value)
		{
			if (nodeStack.TryGetNextInSpan(ref span, out value))
			{
				return true;
			}
			discriminatorStack.Pop();
			return false;
		}

		public bool TryGetNext(out SyntaxTrivia value)
		{
			if (triviaStack.TryGetNext(out value))
			{
				return true;
			}
			discriminatorStack.Pop();
			return false;
		}

		public void PushChildren(SyntaxNode node, Func<SyntaxNode, bool> descendIntoChildren)
		{
			if (descendIntoChildren == null || descendIntoChildren(node))
			{
				nodeStack.PushChildren(node);
				discriminatorStack.Push(Which.Node);
			}
		}

		public void PushLeadingTrivia(ref SyntaxToken token)
		{
			triviaStack.PushLeadingTrivia(ref token);
			discriminatorStack.Push(Which.Trivia);
		}

		public void PushTrailingTrivia(ref SyntaxToken token)
		{
			triviaStack.PushTrailingTrivia(ref token);
			discriminatorStack.Push(Which.Trivia);
		}

		public void Dispose()
		{
			nodeStack.Dispose();
			triviaStack.Dispose();
			discriminatorStack?.Free();
		}
	}

	private struct ThreeEnumeratorListStack : IDisposable
	{
		public enum Which : byte
		{
			Node,
			Trivia,
			Token
		}

		private ChildSyntaxListEnumeratorStack nodeStack;

		private TriviaListEnumeratorStack triviaStack;

		private readonly ArrayBuilder<SyntaxNodeOrToken> tokenStack;

		private readonly ArrayBuilder<Which> discriminatorStack;

		public bool IsNotEmpty
		{
			get
			{
				ArrayBuilder<Which> arrayBuilder = discriminatorStack;
				if (arrayBuilder == null)
				{
					return false;
				}
				return arrayBuilder.Count > 0;
			}
		}

		public ThreeEnumeratorListStack(SyntaxNode startingNode, Func<SyntaxNode, bool> descendIntoChildren)
		{
			nodeStack = new ChildSyntaxListEnumeratorStack(startingNode, descendIntoChildren);
			triviaStack = default;
			if (nodeStack.IsNotEmpty)
			{
				tokenStack = ArrayBuilder<SyntaxNodeOrToken>.GetInstance();
				discriminatorStack = ArrayBuilder<Which>.GetInstance();
				discriminatorStack.Push(Which.Node);
			}
			else
			{
				tokenStack = null;
				discriminatorStack = null;
			}
		}

		public Which PeekNext()
		{
			return discriminatorStack.Peek();
		}

		public bool TryGetNextInSpan(ref TextSpan span, out SyntaxNodeOrToken value)
		{
			if (nodeStack.TryGetNextInSpan(ref span, out value))
			{
				return true;
			}
			discriminatorStack.Pop();
			return false;
		}

		public bool TryGetNext(out SyntaxTrivia value)
		{
			if (triviaStack.TryGetNext(out value))
			{
				return true;
			}
			discriminatorStack.Pop();
			return false;
		}

		public SyntaxNodeOrToken PopToken()
		{
			discriminatorStack.Pop();
			return tokenStack.Pop();
		}

		public void PushChildren(SyntaxNode node, Func<SyntaxNode, bool> descendIntoChildren)
		{
			if (descendIntoChildren == null || descendIntoChildren(node))
			{
				nodeStack.PushChildren(node);
				discriminatorStack.Push(Which.Node);
			}
		}

		public void PushLeadingTrivia(ref SyntaxToken token)
		{
			triviaStack.PushLeadingTrivia(ref token);
			discriminatorStack.Push(Which.Trivia);
		}

		public void PushTrailingTrivia(ref SyntaxToken token)
		{
			triviaStack.PushTrailingTrivia(ref token);
			discriminatorStack.Push(Which.Trivia);
		}

		public void PushToken(ref SyntaxNodeOrToken value)
		{
			tokenStack.Push(value);
			discriminatorStack.Push(Which.Token);
		}

		public void Dispose()
		{
			nodeStack.Dispose();
			triviaStack.Dispose();
			tokenStack?.Free();
			discriminatorStack?.Free();
		}
	}

	private readonly SyntaxNode parent;

	private SyntaxTree syntaxTree;

	public SyntaxKind Kind => Node.Kind;

	protected string KindText => Node.KindText;

	public bool IsMissing => Node.IsMissing;

	public SyntaxNode Parent => parent;

	public virtual SyntaxTrivia ParentTrivia => default;

	internal static AbstractSyntaxNavigator Navigator => SyntaxNavigator.Instance;

	public int Position { get; private set; }

	internal int EndPosition => Position + Node.FullWidth;

	public TextSpan FullSpan => new TextSpan(Position, Node.FullWidth);

	public TextSpan Span
	{
		get
		{
			int position = Position;
			int fullWidth = Node.FullWidth;
			int leadingTriviaWidth = Node.GetLeadingTriviaWidth();
			int start = position + leadingTriviaWidth;
			fullWidth -= leadingTriviaWidth;
			fullWidth -= Node.GetTrailingTriviaWidth();
			DebugAssertHelper.Assert(fullWidth >= 0);
			return new TextSpan(start, fullWidth);
		}
	}

	public int SpanStart => Position + Node.GetLeadingTriviaWidth();

	public int SpanEnd => SpanStart + Width;

	public bool IsList => Node.IsList;

	internal InternalSyntaxNode Node { get; private set; }

	internal int SlotCount => Node.SlotCount;

	internal int Width => Node.Width;

	internal int FullWidth => Node.FullWidth;

	public bool IsStructuredTrivia => Node.IsStructuredTrivia;

	public bool HasStructuredTrivia
	{
		get
		{
			if (Node.ContainsStructuredTrivia)
			{
				return !Node.IsStructuredTrivia;
			}
			return false;
		}
	}

	public bool ContainsAnnotations => Node.ContainsAnnotations;

	internal Location Location
	{
		get
		{
			SyntaxTree syntaxTree = SyntaxTree;
			DebugAssertHelper.Assert(syntaxTree != null);
			if (syntaxTree.HasCompilationUnitRoot)
			{
				return new SourceLocation(this);
			}
			return NoLocation.Singleton;
		}
	}

	public SyntaxTree SyntaxTree
	{
		get
		{
			ComputeSyntaxTree(this);
			DebugAssertHelper.Assert(syntaxTree != null);
			return syntaxTree;
		}
	}

	public bool ContainsDiagnostics => Node.ContainsDiagnostics;

	internal bool HasErrors
	{
		get
		{
			if (!ContainsDiagnostics)
			{
				return false;
			}
			return HasErrorsSlow();
		}
	}

	internal SyntaxNode ParentOrStructuredTriviaParent => GetParent(this, ascendOutOfTrivia: true);

	public bool HasTrailingTrivia => GetTrailingTrivia().Count > 0;

	private IEnumerable<SyntaxNode> DescendantNodesImpl(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren, bool descendIntoTrivia, bool includeSelf)
	{
		if (!descendIntoTrivia)
		{
			return DescendantNodesOnly(span, descendIntoChildren, includeSelf);
		}
		return from e in DescendantNodesAndTokensImpl(span, descendIntoChildren, descendIntoTrivia: true, includeSelf)
			where e.IsNode
			select e.AsNode();
	}

	private IEnumerable<SyntaxNodeOrToken> DescendantNodesAndTokensImpl(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren, bool descendIntoTrivia, bool includeSelf)
	{
		if (!descendIntoTrivia)
		{
			return DescendantNodesAndTokensOnly(span, descendIntoChildren, includeSelf);
		}
		return DescendantNodesAndTokensIntoTrivia(span, descendIntoChildren, includeSelf);
	}

	private IEnumerable<SyntaxTrivia> DescendantTriviaImpl(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
	{
		if (!descendIntoTrivia)
		{
			return DescendantTriviaOnly(span, descendIntoChildren);
		}
		return DescendantTriviaIntoTrivia(span, descendIntoChildren);
	}

	private static bool IsInSpan(ref TextSpan span, TextSpan childSpan)
	{
		if (!span.OverlapsWith(childSpan))
		{
			if (childSpan.Length == 0)
			{
				return span.IntersectsWith(childSpan);
			}
			return false;
		}
		return true;
	}

	private IEnumerable<SyntaxNode> DescendantNodesOnly(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren, bool includeSelf)
	{
		if (includeSelf && IsInSpan(ref span, FullSpan))
		{
			yield return this;
		}
		using ChildSyntaxListEnumeratorStack stack = new ChildSyntaxListEnumeratorStack(this, descendIntoChildren);
		while (stack.IsNotEmpty)
		{
			SyntaxNode syntaxNode = stack.TryGetNextAsNodeInSpan(ref span);
			if (syntaxNode != null)
			{
				stack.PushChildren(syntaxNode, descendIntoChildren);
				yield return syntaxNode;
			}
		}
	}

	private IEnumerable<SyntaxNodeOrToken> DescendantNodesAndTokensOnly(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren, bool includeSelf)
	{
		if (includeSelf && IsInSpan(ref span, FullSpan))
		{
			yield return this;
		}
		using ChildSyntaxListEnumeratorStack stack = new ChildSyntaxListEnumeratorStack(this, descendIntoChildren);
		while (stack.IsNotEmpty)
		{
			if (stack.TryGetNextInSpan(ref span, out var value))
			{
				SyntaxNode syntaxNode = value.AsNode();
				if (syntaxNode != null)
				{
					stack.PushChildren(syntaxNode, descendIntoChildren);
				}
				yield return value;
			}
		}
	}

	private IEnumerable<SyntaxNodeOrToken> DescendantNodesAndTokensIntoTrivia(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren, bool includeSelf)
	{
		if (includeSelf && IsInSpan(ref span, FullSpan))
		{
			yield return this;
		}
		using ThreeEnumeratorListStack stack = new ThreeEnumeratorListStack(this, descendIntoChildren);
		while (stack.IsNotEmpty)
		{
			switch (stack.PeekNext())
			{
			case ThreeEnumeratorListStack.Which.Node:
			{
				if (!stack.TryGetNextInSpan(ref span, out var value2))
				{
					break;
				}
				if (value2.IsNode)
				{
					stack.PushChildren(value2.AsNode(), descendIntoChildren);
				}
				else if (value2.IsToken)
				{
					SyntaxToken token = value2.AsToken();
					if (token.HasStructuredTrivia)
					{
						if (token.HasTrailingTrivia)
						{
							stack.PushTrailingTrivia(ref token);
						}
						stack.PushToken(ref value2);
						if (token.HasLeadingTrivia)
						{
							stack.PushLeadingTrivia(ref token);
						}
						break;
					}
				}
				yield return value2;
				break;
			}
			case ThreeEnumeratorListStack.Which.Trivia:
			{
				if (stack.TryGetNext(out var value) && value.HasStructure && IsInSpan(ref span, value.FullSpan))
				{
					SyntaxNode structure = value.GetStructure();
					stack.PushChildren(structure, descendIntoChildren);
					yield return structure;
				}
				break;
			}
			case ThreeEnumeratorListStack.Which.Token:
				yield return stack.PopToken();
				break;
			}
		}
	}

	private IEnumerable<SyntaxTrivia> DescendantTriviaOnly(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren)
	{
		using ChildSyntaxListEnumeratorStack stack = new ChildSyntaxListEnumeratorStack(this, descendIntoChildren);
		while (stack.IsNotEmpty)
		{
			if (!stack.TryGetNextInSpan(ref span, out var value))
			{
				continue;
			}
			if (value.IsNode)
			{
				SyntaxNode node = value.AsNode();
				stack.PushChildren(node, descendIntoChildren);
			}
			else
			{
				if (!value.IsToken)
				{
					continue;
				}
				SyntaxToken token = value.AsToken();
				foreach (SyntaxTrivia leadingTrivium in token.LeadingTrivia)
				{
					if (IsInSpan(ref span, leadingTrivium.FullSpan))
					{
						yield return leadingTrivium;
					}
				}
				foreach (SyntaxTrivia trailingTrivium in token.TrailingTrivia)
				{
					if (IsInSpan(ref span, trailingTrivium.FullSpan))
					{
						yield return trailingTrivium;
					}
				}
			}
		}
	}

	private IEnumerable<SyntaxTrivia> DescendantTriviaIntoTrivia(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren)
	{
		using TwoEnumeratorListStack stack = new TwoEnumeratorListStack(this, descendIntoChildren);
		while (stack.IsNotEmpty)
		{
			switch (stack.PeekNext())
			{
			case TwoEnumeratorListStack.Which.Node:
			{
				if (!stack.TryGetNextInSpan(ref span, out var value2))
				{
					break;
				}
				if (value2.IsNode)
				{
					SyntaxNode node = value2.AsNode();
					stack.PushChildren(node, descendIntoChildren);
				}
				else if (value2.IsToken)
				{
					SyntaxToken token = value2.AsToken();
					if (token.HasTrailingTrivia)
					{
						stack.PushTrailingTrivia(ref token);
					}
					if (token.HasLeadingTrivia)
					{
						stack.PushLeadingTrivia(ref token);
					}
				}
				break;
			}
			case TwoEnumeratorListStack.Which.Trivia:
			{
				if (stack.TryGetNext(out var value))
				{
					if (value.HasStructure)
					{
						SyntaxNode structure = value.GetStructure();
						stack.PushChildren(structure, descendIntoChildren);
					}
					if (IsInSpan(ref span, value.FullSpan))
					{
						yield return value;
					}
				}
				break;
			}
			}
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private string GetDebuggerDisplay()
	{
		return GetType().Name + " " + KindText + " " + ToString();
	}

	internal SyntaxNode(InternalSyntaxNode internalSyntaxNode, SyntaxNode parent, int position)
	{
		Node = internalSyntaxNode;
		this.parent = parent;
		Position = position;
	}

	internal SyntaxNode(InternalSyntaxNode internalSyntaxNode, int position, SyntaxTree syntaxTree)
		: this(internalSyntaxNode, null, position)
	{
		this.syntaxTree = syntaxTree;
	}

	public sealed override string ToString()
	{
		return Node.ToString();
	}

	public string ToFullString()
	{
		return Node.ToFullString();
	}

	public virtual void WriteTo(TextWriter writer)
	{
		Node.WriteTo(writer, leading: true, trailing: true);
	}

	public SourceText GetText(Encoding encoding = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		WriteTo(new StringWriter(stringBuilder, CultureInfo.InvariantCulture));
		return new StringBuilderText(stringBuilder, encoding);
	}

	internal abstract SyntaxNode GetCachedSlot(int index);

	internal abstract SyntaxNode GetNodeSlot(int index);

	[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
	protected T GetRedAtZero<T>(ref T field) where T : SyntaxNode
	{
		T val = field;
		if (val == null)
		{
			InternalSyntaxNode slot = Node.GetSlot(0);
			if (slot != null)
			{
				val = (T)slot.CreateRed(this, Position);
				val = Interlocked.CompareExchange(ref field, val, null) ?? val;
			}
		}
		return val;
	}

	internal SyntaxNode GetRedElement(ref SyntaxNode element, int slot)
	{
		DebugAssertHelper.Assert(IsList);
		SyntaxNode syntaxNode = element;
		if (syntaxNode == null)
		{
			syntaxNode = Node.GetSlot(slot).CreateRed(Parent, GetChildPosition(slot));
			if (Interlocked.CompareExchange(ref element, syntaxNode, null) != null)
			{
				syntaxNode = element;
			}
		}
		return syntaxNode;
	}

	internal SyntaxNode GetRedElementIfNotToken(ref SyntaxNode element)
	{
		DebugAssertHelper.Assert(IsList);
		SyntaxNode syntaxNode = element;
		if (syntaxNode == null)
		{
			InternalSyntaxNode slot = Node.GetSlot(1);
			if (!slot.IsToken)
			{
				syntaxNode = slot.CreateRed(Parent, GetChildPosition(1));
				if (Interlocked.CompareExchange(ref element, syntaxNode, null) != null)
				{
					syntaxNode = element;
				}
			}
		}
		return syntaxNode;
	}

	protected T GetRed<T>(ref T field, int slot) where T : SyntaxNode
	{
		T val = field;
		if (val == null)
		{
			InternalSyntaxNode slot2 = Node.GetSlot(slot);
			if (slot2 != null)
			{
				val = (T)slot2.CreateRed(this, GetChildPosition(slot));
				val = Interlocked.CompareExchange(ref field, val, null) ?? val;
			}
		}
		return val;
	}

	public bool IsKind(SyntaxKind kind)
	{
		return Kind == kind;
	}

	public bool IsPartOfStructuredTrivia()
	{
		for (SyntaxNode syntaxNode = this; syntaxNode != null; syntaxNode = syntaxNode.Parent)
		{
			if (syntaxNode.IsStructuredTrivia)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnnotations(IEnumerable<AnnotationKind> annotationKinds)
	{
		return Node.HasAnnotations(annotationKinds);
	}

	public bool HasAnnotations(AnnotationKind annotationKind)
	{
		return Node.HasAnnotations(annotationKind);
	}

	public bool HasAnnotation(SyntaxAnnotation annotation)
	{
		return Node.HasAnnotation(annotation);
	}

	internal SyntaxNode GetNodeWithoutAnnotations(IEnumerable<SyntaxAnnotation> annotations)
	{
		return Node.WithoutAnnotationsGreen(annotations).CreateRed();
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(AnnotationKind annotationKind)
	{
		return Node.GetAnnotations(annotationKind);
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(IEnumerable<AnnotationKind> annotationKinds)
	{
		return Node.GetAnnotations(annotationKinds);
	}

	public IEnumerable<SyntaxNodeOrToken> GetAnnotatedNodesAndTokens(AnnotationKind annotationKind)
	{
		return from t in DescendantNodesAndTokensAndSelf((SyntaxNode n) => n.ContainsAnnotations, descendIntoTrivia: true)
			where t.HasAnnotations(annotationKind)
			select t;
	}

	public IEnumerable<SyntaxNodeOrToken> GetAnnotatedNodesAndTokens(params AnnotationKind[] annotationKinds)
	{
		return from t in DescendantNodesAndTokensAndSelf((SyntaxNode n) => n.ContainsAnnotations, descendIntoTrivia: true)
			where t.HasAnnotations(annotationKinds)
			select t;
	}

	public IEnumerable<SyntaxNodeOrToken> GetAnnotatedNodesAndTokens(SyntaxAnnotation annotation)
	{
		return from t in DescendantNodesAndTokensAndSelf((SyntaxNode n) => n.ContainsAnnotations, descendIntoTrivia: true)
			where t.HasAnnotation(annotation)
			select t;
	}

	public IEnumerable<SyntaxNode> GetAnnotatedNodes(SyntaxAnnotation syntaxAnnotation)
	{
		return from n in GetAnnotatedNodesAndTokens(syntaxAnnotation)
			where n.IsNode
			select n.AsNode();
	}

	internal SyntaxAnnotation[] GetAnnotations()
	{
		return Node.GetAnnotations();
	}

	internal SyntaxNode WithAdditionalAnnotationsInternal(IEnumerable<SyntaxAnnotation> annotations)
	{
		return Node.WithAdditionalAnnotationsGreen(annotations).CreateRed();
	}

	public IEnumerable<SyntaxTrivia> GetAnnotatedTrivia(AnnotationKind annotationKind)
	{
		return from tr in DescendantTrivia((SyntaxNode n) => n.ContainsAnnotations, descendIntoTrivia: true)
			where tr.HasAnnotations(annotationKind)
			select tr;
	}

	public IEnumerable<SyntaxTrivia> GetAnnotatedTrivia(params AnnotationKind[] annotationKinds)
	{
		return from tr in DescendantTrivia((SyntaxNode n) => n.ContainsAnnotations, descendIntoTrivia: true)
			where tr.HasAnnotations(annotationKinds)
			select tr;
	}

	public IEnumerable<SyntaxTrivia> GetAnnotatedTrivia(SyntaxAnnotation annotation)
	{
		return from tr in DescendantTrivia((SyntaxNode n) => n.ContainsAnnotations, descendIntoTrivia: true)
			where tr.HasAnnotation(annotation)
			select tr;
	}

	internal static T CloneNodeAsRoot<T>(T node, SyntaxTree syntaxTree) where T : SyntaxNode
	{
		T val = (T)node.Node.CreateRed(null, 0);
		val.syntaxTree = syntaxTree;
		return val;
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public Location GetLocation()
	{
		return new SourceLocation(this);
	}

	private static void ComputeSyntaxTree(SyntaxNode node)
	{
		ArrayBuilder<SyntaxNode> arrayBuilder = null;
		SyntaxTree syntaxTree;
		while (true)
		{
			syntaxTree = node.syntaxTree;
			if (syntaxTree != null)
			{
				break;
			}
			SyntaxNode syntaxNode = node.Parent;
			if (syntaxNode == null)
			{
				Interlocked.CompareExchange(ref node.syntaxTree, SyntaxTree.CreateWithoutClone(node), null);
				syntaxTree = node.syntaxTree;
				break;
			}
			if (syntaxNode.syntaxTree != null)
			{
				Interlocked.CompareExchange(ref node.syntaxTree, syntaxNode.syntaxTree, null);
				syntaxTree = node.syntaxTree;
				break;
			}
			(arrayBuilder ?? (arrayBuilder = ArrayBuilder<SyntaxNode>.GetInstance())).Add(node);
			node = syntaxNode;
		}
		if (arrayBuilder == null)
		{
			return;
		}
		DebugAssertHelper.Assert(syntaxTree != null);
		foreach (SyntaxNode item in arrayBuilder)
		{
			SyntaxTree syntaxTree2 = Interlocked.CompareExchange(ref item.syntaxTree, syntaxTree, null);
			if (syntaxTree2 != null)
			{
				syntaxTree = syntaxTree2;
			}
		}
		arrayBuilder.Free();
	}

	public virtual TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		throw new NotImplementedException();
	}

	public virtual void Accept(SyntaxVisitor visitor)
	{
		throw new NotImplementedException();
	}

	private bool HasErrorsSlow()
	{
		return new InternalSyntaxDiagnosticInfoList(Node).Any((DiagnosticInfo info) => info.Severity == DiagnosticSeverity.Error);
	}

	internal virtual int GetChildPosition(int index)
	{
		int num = 0;
		InternalSyntaxNode node = Node;
		while (index > 0)
		{
			index--;
			SyntaxNode cachedSlot = GetCachedSlot(index);
			if (cachedSlot != null)
			{
				return cachedSlot.EndPosition + num;
			}
			InternalSyntaxNode slot = node.GetSlot(index);
			if (slot != null)
			{
				num += slot.FullWidth;
			}
		}
		return Position + num;
	}

	internal int GetChildIndex(int slot)
	{
		int num = 0;
		for (int i = 0; i < slot; i++)
		{
			InternalSyntaxNode slot2 = Node.GetSlot(i);
			if (slot2 != null)
			{
				num = ((!slot2.IsList) ? (num + 1) : (num + slot2.SlotCount));
			}
		}
		return num;
	}

	public ChildSyntaxList ChildNodesAndTokens()
	{
		return new ChildSyntaxList(this);
	}

	public SyntaxToken GetFirstToken(bool includeZeroWidth = false, bool includeSkipped = false)
	{
		return Navigator.GetFirstToken(this, includeZeroWidth, includeSkipped);
	}

	public SyntaxToken GetLastToken(bool includeZeroWidth = false, bool includeSkipped = false)
	{
		return Navigator.GetLastToken(this, includeZeroWidth, includeSkipped);
	}

	public SyntaxNodeOrToken ChildThatContainsPosition(int position)
	{
		if (!FullSpan.Contains(position))
		{
			throw new ArgumentOutOfRangeException("position");
		}
		SyntaxNodeOrToken result = ChildSyntaxList.ChildThatContainsPosition(this, position);
		DebugAssertHelper.Assert(result.FullSpan.Contains(position), "ChildThatContainsPosition's return value does not contain the requested position.");
		return result;
	}

	internal SyntaxToken FindTokenInternal(int position)
	{
		SyntaxNodeOrToken syntaxNodeOrToken = this;
		while (true)
		{
			DebugAssertHelper.Assert(syntaxNodeOrToken.Kind != SyntaxKind.None);
			DebugAssertHelper.Assert(syntaxNodeOrToken.FullSpan.Contains(position));
			SyntaxNode syntaxNode = syntaxNodeOrToken.AsNode();
			if (syntaxNode == null)
			{
				break;
			}
			syntaxNodeOrToken = syntaxNode.ChildThatContainsPosition(position);
		}
		return syntaxNodeOrToken.AsToken();
	}

	private bool TryGetEofAt(int position, out SyntaxToken eof)
	{
		if (position == EndPosition && this is CompilationUnitSyntax compilationUnitSyntax)
		{
			eof = compilationUnitSyntax.GetEndOfFileToken();
			DebugAssertHelper.Assert(eof.EndPosition == position);
			return true;
		}
		eof = default;
		return false;
	}

	public SyntaxToken FindToken(int position)
	{
		if (TryGetEofAt(position, out var eof))
		{
			return eof;
		}
		if (!FullSpan.Contains(position))
		{
			throw new ArgumentOutOfRangeException("position", string.Format(CultureInfo.InvariantCulture, "Position: {0}, FullSpan: {1}", position, FullSpan.ToString()));
		}
		return FindTokenInternal(position);
	}

	protected internal SyntaxNode ReplaceCore<TNode>(IEnumerable<TNode> nodes = null, Func<TNode, TNode, SyntaxNode> computeReplacementNode = null, IEnumerable<SyntaxToken> tokens = null, Func<SyntaxToken, SyntaxToken, SyntaxToken> computeReplacementToken = null, IEnumerable<SyntaxTrivia> trivia = null, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia = null) where TNode : SyntaxNode
	{
		return SyntaxReplacer.Replace(this, nodes, computeReplacementNode, tokens, computeReplacementToken, trivia, computeReplacementTrivia);
	}

	protected internal virtual SyntaxNode NormalizeWhiteSpaceCore(string indentation, bool elasticTrivia)
	{
		return SyntaxNormalizer.Format(this, indentation, elasticTrivia);
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public SyntaxReference GetReference()
	{
		return SyntaxTree.GetReference(this);
	}

	public IEnumerable<SyntaxNode> ChildNodes()
	{
		foreach (SyntaxNodeOrToken item in ChildNodesAndTokens())
		{
			if (item.IsNode)
			{
				yield return item.AsNode();
			}
		}
	}

	public IEnumerable<SyntaxToken> DescendantTokens(Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
	{
		return from sn in DescendantNodesAndTokens(descendIntoChildren, descendIntoTrivia)
			where sn.IsToken
			select sn.AsToken();
	}

	public IEnumerable<SyntaxToken> DescendantTokens(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
	{
		return from sn in DescendantNodesAndTokens(span, descendIntoChildren, descendIntoTrivia)
			where sn.IsToken
			select sn.AsToken();
	}

	public IEnumerable<SyntaxNode> DescendantNodesAndSelf(Func<SyntaxNode, bool> descendIntoChildren, bool descendIntoTrivia)
	{
		return DescendantNodesImpl(FullSpan, descendIntoChildren, descendIntoTrivia, includeSelf: true);
	}

	public IEnumerable<SyntaxNode> DescendantNodesAndSelf(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren, bool descendIntoTrivia)
	{
		return DescendantNodesImpl(span, descendIntoChildren, descendIntoTrivia, includeSelf: true);
	}

	public IEnumerable<SyntaxNodeOrToken> DescendantNodesAndTokens(Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
	{
		return DescendantNodesAndTokensImpl(FullSpan, descendIntoChildren, descendIntoTrivia, includeSelf: false);
	}

	public IEnumerable<SyntaxNodeOrToken> DescendantNodesAndTokens(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
	{
		return DescendantNodesAndTokensImpl(span, descendIntoChildren, descendIntoTrivia, includeSelf: false);
	}

	public IEnumerable<SyntaxNodeOrToken> DescendantNodesAndTokensAndSelf(Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
	{
		return DescendantNodesAndTokensImpl(FullSpan, descendIntoChildren, descendIntoTrivia, includeSelf: true);
	}

	public IEnumerable<SyntaxNodeOrToken> DescendantNodesAndTokensAndSelf(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
	{
		return DescendantNodesAndTokensImpl(span, descendIntoChildren, descendIntoTrivia, includeSelf: true);
	}

	public IEnumerable<SyntaxNode> Ancestors(bool ascendOutOfTrivia = true)
	{
		if (Parent == null)
		{
			return SpecializedCollections.EmptyEnumerable<SyntaxNode>();
		}
		return Parent.AncestorsAndSelf(ascendOutOfTrivia);
	}

	public IEnumerable<SyntaxNode> AncestorsAndSelf(bool ascendOutOfTrivia = true)
	{
		for (SyntaxNode node = this; node != null; node = GetParent(node, ascendOutOfTrivia))
		{
			yield return node;
		}
	}

	private static SyntaxNode GetParent(SyntaxNode node, bool ascendOutOfTrivia)
	{
		SyntaxNode syntaxNode = node.Parent;
		if (((syntaxNode == null) & ascendOutOfTrivia) && node is IStructuredTriviaSyntax { ParentTrivia: { Token: var token } })
		{
			syntaxNode = token.Parent;
		}
		return syntaxNode;
	}

	public TNode FirstAncestorOrSelf<TNode>(Func<TNode, bool> predicate = null, bool ascendOutOfTrivia = true) where TNode : SyntaxNode
	{
		for (SyntaxNode syntaxNode = this; syntaxNode != null; syntaxNode = GetParent(syntaxNode, ascendOutOfTrivia))
		{
			if (syntaxNode is TNode val && (predicate == null || predicate(val)))
			{
				return val;
			}
		}
		return null;
	}

	public IEnumerable<SyntaxNode> DescendantNodes(Func<SyntaxNode, bool> descendIntoChildren = null)
	{
		return DescendantNodesImpl(FullSpan, descendIntoChildren, includeSelf: false);
	}

	public IEnumerable<SyntaxNode> DescendantNodesAndSelf(Func<SyntaxNode, bool> descendIntoChildren = null)
	{
		return DescendantNodesImpl(FullSpan, descendIntoChildren, includeSelf: true);
	}

	public IEnumerable<SyntaxNode> DescendantNodesAndSelf(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren = null)
	{
		return DescendantNodesImpl(span, descendIntoChildren, includeSelf: true);
	}

	private IEnumerable<SyntaxNode> DescendantNodesImpl(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren, bool includeSelf)
	{
		return DescendantNodesOnly(span, descendIntoChildren, includeSelf);
	}

	public SyntaxToken FindToken(int position, bool findInsideTrivia)
	{
		if (findInsideTrivia)
		{
			return FindToken(position, SyntaxTrivia.Any);
		}
		if (TryGetEofAt(position, out var eof))
		{
			return eof;
		}
		if (!FullSpan.Contains(position))
		{
			throw new ArgumentOutOfRangeException("position", string.Format(CultureInfo.InvariantCulture, "Node span: {0}-{1}, position: {2}", FullSpan.Start, FullSpan.End, position));
		}
		return FindTokenInternal(position);
	}

	private SyntaxToken FindToken(int position, Func<SyntaxTrivia, bool> findInsideTrivia)
	{
		SyntaxToken syntaxToken = FindToken(position, findInsideTrivia: false);
		if (findInsideTrivia != null)
		{
			SyntaxTrivia triviaFromSyntaxToken = GetTriviaFromSyntaxToken(position, syntaxToken);
			if (triviaFromSyntaxToken.HasStructure && findInsideTrivia(triviaFromSyntaxToken))
			{
				syntaxToken = triviaFromSyntaxToken.GetStructure().FindTokenInternal(position);
			}
		}
		return syntaxToken;
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public SyntaxTriviaList GetLeadingTrivia()
	{
		return GetFirstToken(includeZeroWidth: true).LeadingTrivia;
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public SyntaxTriviaList GetTrailingTrivia()
	{
		return GetLastToken(includeZeroWidth: true).TrailingTrivia;
	}

	public SyntaxTrivia FindTrivia(int position, bool findInsideTrivia = false)
	{
		return FindTrivia(position, findInsideTrivia ? SyntaxTrivia.Any : null);
	}

	public SyntaxTrivia FindTrivia(int position, Func<SyntaxTrivia, bool> stepInto)
	{
		if (FullSpan.Contains(position))
		{
			return FindTriviaByOffset(this, position - Position, stepInto);
		}
		return default;
	}

	internal static SyntaxTrivia FindTriviaByOffset(SyntaxNode node, int textOffset, Func<SyntaxTrivia, bool> stepInto = null)
	{
		while (textOffset >= 0)
		{
			int fullWidth;
			SyntaxNodeOrToken current;
			for (ChildSyntaxList.Enumerator enumerator = node.ChildNodesAndTokens().GetEnumerator(); enumerator.MoveNext(); textOffset -= fullWidth)
			{
				current = enumerator.Current;
				fullWidth = current.FullWidth;
				if (textOffset >= fullWidth)
				{
					continue;
				}
				if (current.IsNode)
				{
					goto IL_003b;
				}
				if (!current.IsToken)
				{
					continue;
				}
				goto IL_0052;
			}
			break;
			IL_0114:
			SyntaxTrivia current2;
			if (current2.HasStructure && stepInto != null && stepInto(current2))
			{
				node = current2.GetStructure();
				continue;
			}
			return current2;
			IL_0095:
			SyntaxTrivia current3;
			if (current3.HasStructure && stepInto != null && stepInto(current3))
			{
				node = current3.GetStructure();
				continue;
			}
			return current3;
			IL_003b:
			node = current.AsNode();
			continue;
			IL_0052:
			SyntaxToken syntaxToken = current.AsToken();
			int leadingWidth = syntaxToken.LeadingWidth;
			if (textOffset < syntaxToken.LeadingWidth)
			{
				foreach (SyntaxTrivia leadingTrivium in syntaxToken.LeadingTrivia)
				{
					current3 = leadingTrivium;
					if (textOffset >= current3.FullWidth)
					{
						textOffset -= current3.FullWidth;
						continue;
					}
					goto IL_0095;
				}
			}
			else if (textOffset >= leadingWidth + syntaxToken.Width)
			{
				textOffset -= leadingWidth + syntaxToken.Width;
				foreach (SyntaxTrivia trailingTrivium in syntaxToken.TrailingTrivia)
				{
					current2 = trailingTrivium;
					if (textOffset >= current2.FullWidth)
					{
						textOffset -= current2.FullWidth;
						continue;
					}
					goto IL_0114;
				}
			}
			return default;
		}
		return default;
	}

	public IEnumerable<SyntaxTrivia> DescendantTrivia(Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
	{
		return DescendantTriviaImpl(FullSpan, descendIntoChildren, descendIntoTrivia);
	}

	public IEnumerable<SyntaxTrivia> DescendantTrivia(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
	{
		return DescendantTriviaImpl(span, descendIntoChildren, descendIntoTrivia);
	}

	private static SyntaxTrivia GetTriviaFromSyntaxToken(int position, SyntaxToken token)
	{
		TextSpan span = token.Span;
		SyntaxTrivia result = default;
		if (position < span.Start && token.HasLeadingTrivia)
		{
			return GetTriviaThatContainsPosition(token.LeadingTrivia, position);
		}
		if (position >= span.End && token.HasTrailingTrivia)
		{
			return GetTriviaThatContainsPosition(token.TrailingTrivia, position);
		}
		return result;
	}

	internal static SyntaxTrivia GetTriviaThatContainsPosition(SyntaxTriviaList list, int position)
	{
		foreach (SyntaxTrivia item in list)
		{
			if (item.FullSpan.Contains(position))
			{
				return item;
			}
			if (item.Position > position)
			{
				break;
			}
		}
		return default;
	}

	public bool IsEquivalentTo(SyntaxNode node, bool topLevel = false)
	{
		return SyntaxFactory.AreEquivalent(this, node, topLevel);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stream")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
	public static SyntaxNode DeserializeFrom(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotImplementedException();
	}

	public SyntaxNode FindNode(TextSpan span, bool findInsideTrivia = false, bool getInnermostNodeForTie = false)
	{
		if (!FullSpan.Contains(span))
		{
			throw new ArgumentOutOfRangeException("span");
		}
		SyntaxNode syntaxNode = FindToken(span.Start, findInsideTrivia).Parent.FirstAncestorOrSelf((SyntaxNode a) => a.FullSpan.Contains(span));
		SyntaxNode syntaxNode2 = syntaxNode.SyntaxTree?.GetRoot();
		if (!getInnermostNodeForTie)
		{
			while (true)
			{
				SyntaxNode syntaxNode3 = syntaxNode.Parent;
				if (syntaxNode3 == null || syntaxNode3.FullWidth != syntaxNode.FullWidth || syntaxNode3 == syntaxNode2)
				{
					break;
				}
				syntaxNode = syntaxNode3;
			}
		}
		return syntaxNode;
	}

	internal void WalkDescendantsAndPerformAction(Action<SyntaxNode> action, Func<SyntaxNode, bool> descendIntoChildren = null)
	{
		TextSpan span = FullSpan;
		using ChildSyntaxListEnumeratorStack childSyntaxListEnumeratorStack = new ChildSyntaxListEnumeratorStack(this, descendIntoChildren);
		while (childSyntaxListEnumeratorStack.IsNotEmpty)
		{
			SyntaxNode syntaxNode = childSyntaxListEnumeratorStack.TryGetNextAsNodeInSpan(ref span);
			if (syntaxNode != null)
			{
				childSyntaxListEnumeratorStack.PushChildren(syntaxNode, descendIntoChildren);
				action(syntaxNode);
			}
		}
	}
}
