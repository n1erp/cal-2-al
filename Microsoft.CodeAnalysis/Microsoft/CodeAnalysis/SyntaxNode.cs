using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public abstract class SyntaxNode
{
	private struct ChildSyntaxListEnumeratorStack : IDisposable
	{
		private static readonly ObjectPool<ChildSyntaxList.Enumerator[]> s_stackPool = new ObjectPool<ChildSyntaxList.Enumerator[]>(() => new ChildSyntaxList.Enumerator[16]);

		private ChildSyntaxList.Enumerator[] _stack;

		private int _stackPtr;

		public bool IsNotEmpty => _stackPtr >= 0;

		public ChildSyntaxListEnumeratorStack(SyntaxNode startingNode, Func<SyntaxNode, bool> descendIntoChildren)
		{
			if (descendIntoChildren == null || descendIntoChildren(startingNode))
			{
				_stack = s_stackPool.Allocate();
				_stackPtr = 0;
				_stack[0].InitializeFrom(startingNode);
			}
			else
			{
				_stack = null;
				_stackPtr = -1;
			}
		}

		public bool TryGetNextInSpan(in TextSpan span, out SyntaxNodeOrToken value)
		{
			while (_stack[_stackPtr].TryMoveNextAndGetCurrent(out value))
			{
				if (IsInSpan(in span, value.FullSpan))
				{
					return true;
				}
			}
			_stackPtr--;
			return false;
		}

		public SyntaxNode TryGetNextAsNodeInSpan(in TextSpan span)
		{
			SyntaxNode syntaxNode;
			while ((syntaxNode = _stack[_stackPtr].TryMoveNextAndGetCurrentAsNode()) != null)
			{
				if (IsInSpan(in span, syntaxNode.FullSpan))
				{
					return syntaxNode;
				}
			}
			_stackPtr--;
			return null;
		}

		public void PushChildren(SyntaxNode node)
		{
			if (++_stackPtr >= _stack.Length)
			{
				Array.Resize(ref _stack, checked(_stackPtr * 2));
			}
			_stack[_stackPtr].InitializeFrom(node);
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
			ChildSyntaxList.Enumerator[] stack = _stack;
			if (stack != null && stack.Length < 256)
			{
				Array.Clear(_stack, 0, _stack.Length);
				s_stackPool.Free(_stack);
			}
		}
	}

	private struct TriviaListEnumeratorStack : IDisposable
	{
		private static readonly ObjectPool<SyntaxTriviaList.Enumerator[]> s_stackPool = new ObjectPool<SyntaxTriviaList.Enumerator[]>(() => new SyntaxTriviaList.Enumerator[16]);

		private SyntaxTriviaList.Enumerator[] _stack;

		private int _stackPtr;

		public bool TryGetNext(out SyntaxTrivia value)
		{
			if (_stack[_stackPtr].TryMoveNextAndGetCurrent(out value))
			{
				return true;
			}
			_stackPtr--;
			return false;
		}

		public void PushLeadingTrivia(in SyntaxToken token)
		{
			Grow();
			_stack[_stackPtr].InitializeFromLeadingTrivia(in token);
		}

		public void PushTrailingTrivia(in SyntaxToken token)
		{
			Grow();
			_stack[_stackPtr].InitializeFromTrailingTrivia(in token);
		}

		private void Grow()
		{
			if (_stack == null)
			{
				_stack = s_stackPool.Allocate();
				_stackPtr = -1;
			}
			if (++_stackPtr >= _stack.Length)
			{
				Array.Resize(ref _stack, checked(_stackPtr * 2));
			}
		}

		public void Dispose()
		{
			SyntaxTriviaList.Enumerator[] stack = _stack;
			if (stack != null && stack.Length < 256)
			{
				Array.Clear(_stack, 0, _stack.Length);
				s_stackPool.Free(_stack);
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

		private ChildSyntaxListEnumeratorStack _nodeStack;

		private TriviaListEnumeratorStack _triviaStack;

		private readonly ArrayBuilder<Which> _discriminatorStack;

		public bool IsNotEmpty
		{
			get
			{
				ArrayBuilder<Which> discriminatorStack = _discriminatorStack;
				if (discriminatorStack == null)
				{
					return false;
				}
				return discriminatorStack.Count > 0;
			}
		}

		public TwoEnumeratorListStack(SyntaxNode startingNode, Func<SyntaxNode, bool> descendIntoChildren)
		{
			_nodeStack = new ChildSyntaxListEnumeratorStack(startingNode, descendIntoChildren);
			_triviaStack = default;
			if (_nodeStack.IsNotEmpty)
			{
				_discriminatorStack = ArrayBuilder<Which>.GetInstance();
				_discriminatorStack.Push(Which.Node);
			}
			else
			{
				_discriminatorStack = null;
			}
		}

		public Which PeekNext()
		{
			return _discriminatorStack.Peek();
		}

		public bool TryGetNextInSpan(in TextSpan span, out SyntaxNodeOrToken value)
		{
			if (_nodeStack.TryGetNextInSpan(in span, out value))
			{
				return true;
			}
			_discriminatorStack.Pop();
			return false;
		}

		public bool TryGetNext(out SyntaxTrivia value)
		{
			if (_triviaStack.TryGetNext(out value))
			{
				return true;
			}
			_discriminatorStack.Pop();
			return false;
		}

		public void PushChildren(SyntaxNode node, Func<SyntaxNode, bool> descendIntoChildren)
		{
			if (descendIntoChildren == null || descendIntoChildren(node))
			{
				_nodeStack.PushChildren(node);
				_discriminatorStack.Push(Which.Node);
			}
		}

		public void PushLeadingTrivia(in SyntaxToken token)
		{
			_triviaStack.PushLeadingTrivia(in token);
			_discriminatorStack.Push(Which.Trivia);
		}

		public void PushTrailingTrivia(in SyntaxToken token)
		{
			_triviaStack.PushTrailingTrivia(in token);
			_discriminatorStack.Push(Which.Trivia);
		}

		public void Dispose()
		{
			_nodeStack.Dispose();
			_triviaStack.Dispose();
			_discriminatorStack?.Free();
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

		private ChildSyntaxListEnumeratorStack _nodeStack;

		private TriviaListEnumeratorStack _triviaStack;

		private readonly ArrayBuilder<SyntaxNodeOrToken> _tokenStack;

		private readonly ArrayBuilder<Which> _discriminatorStack;

		public bool IsNotEmpty
		{
			get
			{
				ArrayBuilder<Which> discriminatorStack = _discriminatorStack;
				if (discriminatorStack == null)
				{
					return false;
				}
				return discriminatorStack.Count > 0;
			}
		}

		public ThreeEnumeratorListStack(SyntaxNode startingNode, Func<SyntaxNode, bool> descendIntoChildren)
		{
			_nodeStack = new ChildSyntaxListEnumeratorStack(startingNode, descendIntoChildren);
			_triviaStack = default;
			if (_nodeStack.IsNotEmpty)
			{
				_tokenStack = ArrayBuilder<SyntaxNodeOrToken>.GetInstance();
				_discriminatorStack = ArrayBuilder<Which>.GetInstance();
				_discriminatorStack.Push(Which.Node);
			}
			else
			{
				_tokenStack = null;
				_discriminatorStack = null;
			}
		}

		public Which PeekNext()
		{
			return _discriminatorStack.Peek();
		}

		public bool TryGetNextInSpan(in TextSpan span, out SyntaxNodeOrToken value)
		{
			if (_nodeStack.TryGetNextInSpan(in span, out value))
			{
				return true;
			}
			_discriminatorStack.Pop();
			return false;
		}

		public bool TryGetNext(out SyntaxTrivia value)
		{
			if (_triviaStack.TryGetNext(out value))
			{
				return true;
			}
			_discriminatorStack.Pop();
			return false;
		}

		public SyntaxNodeOrToken PopToken()
		{
			_discriminatorStack.Pop();
			return _tokenStack.Pop();
		}

		public void PushChildren(SyntaxNode node, Func<SyntaxNode, bool> descendIntoChildren)
		{
			if (descendIntoChildren == null || descendIntoChildren(node))
			{
				_nodeStack.PushChildren(node);
				_discriminatorStack.Push(Which.Node);
			}
		}

		public void PushLeadingTrivia(in SyntaxToken token)
		{
			_triviaStack.PushLeadingTrivia(in token);
			_discriminatorStack.Push(Which.Trivia);
		}

		public void PushTrailingTrivia(in SyntaxToken token)
		{
			_triviaStack.PushTrailingTrivia(in token);
			_discriminatorStack.Push(Which.Trivia);
		}

		public void PushToken(in SyntaxNodeOrToken value)
		{
			_tokenStack.Push(value);
			_discriminatorStack.Push(Which.Token);
		}

		public void Dispose()
		{
			_nodeStack.Dispose();
			_triviaStack.Dispose();
			_tokenStack?.Free();
			_discriminatorStack?.Free();
		}
	}

	private readonly SyntaxNode _parent;

	internal SyntaxTree _syntaxTree;

	public int RawKind => Green.RawKind;

	protected string KindText => Green.KindText;

	public abstract string Language { get; }

	internal GreenNode Green { get; }

	internal int Position { get; }

	internal int EndPosition => Position + Green.FullWidth;

	public SyntaxTree SyntaxTree => SyntaxTreeCore;

	internal bool IsList => Green.IsList;

	public TextSpan FullSpan => new TextSpan(Position, Green.FullWidth);

	internal int SlotCount => Green.SlotCount;

	public TextSpan Span
	{
		get
		{
			int position = Position;
			int fullWidth = Green.FullWidth;
			int leadingTriviaWidth = Green.GetLeadingTriviaWidth();
			int start = position + leadingTriviaWidth;
			fullWidth -= leadingTriviaWidth;
			fullWidth -= Green.GetTrailingTriviaWidth();
			return new TextSpan(start, fullWidth);
		}
	}

	public int SpanStart => Position + Green.GetLeadingTriviaWidth();

	internal int Width => Green.Width;

	internal int FullWidth => Green.FullWidth;

	public bool IsMissing => Green.IsMissing;

	public bool IsStructuredTrivia => Green.IsStructuredTrivia;

	public bool HasStructuredTrivia
	{
		get
		{
			if (Green.ContainsStructuredTrivia)
			{
				return !Green.IsStructuredTrivia;
			}
			return false;
		}
	}

	public bool ContainsSkippedText => Green.ContainsSkippedText;

	public bool ContainsDirectives => Green.ContainsDirectives;

	public bool ContainsDiagnostics => Green.ContainsDiagnostics;

	public bool HasLeadingTrivia => GetLeadingTrivia().Count > 0;

	public bool HasTrailingTrivia => GetTrailingTrivia().Count > 0;

	internal Location Location
	{
		get
		{
			if (SyntaxTree.SupportsLocations)
			{
				return new SourceLocation(this);
			}
			return NoLocation.Singleton;
		}
	}

	public SyntaxNode Parent => _parent;

	public virtual SyntaxTrivia ParentTrivia => default;

	internal SyntaxNode ParentOrStructuredTriviaParent => GetParent(this, ascendOutOfTrivia: true);

	public bool ContainsAnnotations => Green.ContainsAnnotations;

	protected abstract SyntaxTree SyntaxTreeCore { get; }

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

	internal SyntaxNode(GreenNode green, SyntaxNode parent, int position)
	{
		Position = position;
		Green = green;
		_parent = parent;
	}

	internal SyntaxNode(GreenNode green, int position, SyntaxTree syntaxTree)
		: this(green, null, position)
	{
		_syntaxTree = syntaxTree;
	}

	private string GetDebuggerDisplay()
	{
		return GetType().Name + " " + KindText + " " + ToString();
	}

	internal SyntaxNode GetRed(ref SyntaxNode field, int slot)
	{
		SyntaxNode syntaxNode = field;
		if (syntaxNode == null)
		{
			GreenNode slot2 = Green.GetSlot(slot);
			if (slot2 != null)
			{
				Interlocked.CompareExchange(ref field, slot2.CreateRed(this, GetChildPosition(slot)), null);
				syntaxNode = field;
			}
		}
		return syntaxNode;
	}

	internal SyntaxNode GetRedAtZero(ref SyntaxNode field)
	{
		SyntaxNode syntaxNode = field;
		if (syntaxNode == null)
		{
			GreenNode slot = Green.GetSlot(0);
			if (slot != null)
			{
				Interlocked.CompareExchange(ref field, slot.CreateRed(this, Position), null);
				syntaxNode = field;
			}
		}
		return syntaxNode;
	}

	protected T GetRed<T>(ref T field, int slot) where T : SyntaxNode
	{
		T val = field;
		if (val == null)
		{
			GreenNode slot2 = Green.GetSlot(slot);
			if (slot2 != null)
			{
				Interlocked.CompareExchange(ref field, (T)slot2.CreateRed(this, GetChildPosition(slot)), null);
				val = field;
			}
		}
		return val;
	}

	protected T GetRedAtZero<T>(ref T field) where T : SyntaxNode
	{
		T val = field;
		if (val == null)
		{
			GreenNode slot = Green.GetSlot(0);
			if (slot != null)
			{
				Interlocked.CompareExchange(ref field, (T)slot.CreateRed(this, Position), null);
				val = field;
			}
		}
		return val;
	}

	internal SyntaxNode GetRedElement(ref SyntaxNode element, int slot)
	{
		SyntaxNode syntaxNode = element;
		if (syntaxNode == null)
		{
			GreenNode slot2 = Green.GetSlot(slot);
			Interlocked.CompareExchange(ref element, slot2.CreateRed(Parent, GetChildPosition(slot)), null);
			syntaxNode = element;
		}
		return syntaxNode;
	}

	internal SyntaxNode GetRedElementIfNotToken(ref SyntaxNode element)
	{
		SyntaxNode syntaxNode = element;
		if (syntaxNode == null)
		{
			GreenNode slot = Green.GetSlot(1);
			if (!slot.IsToken)
			{
				Interlocked.CompareExchange(ref element, slot.CreateRed(Parent, GetChildPosition(1)), null);
				syntaxNode = element;
			}
		}
		return syntaxNode;
	}

	internal SyntaxNode GetWeakRedElement(ref WeakReference<SyntaxNode> slot, int index)
	{
		SyntaxNode target = null;
		WeakReference<SyntaxNode> obj = slot;
		if (obj != null && obj.TryGetTarget(out target))
		{
			return target;
		}
		return CreateWeakItem(ref slot, index);
	}

	private SyntaxNode CreateWeakItem(ref WeakReference<SyntaxNode> slot, int index)
	{
		SyntaxNode syntaxNode = Green.GetSlot(index).CreateRed(Parent, GetChildPosition(index));
		WeakReference<SyntaxNode> value = new WeakReference<SyntaxNode>(syntaxNode);
		WeakReference<SyntaxNode> weakReference;
		do
		{
			SyntaxNode target = null;
			weakReference = slot;
			if (weakReference != null && weakReference.TryGetTarget(out target))
			{
				return target;
			}
		}
		while (Interlocked.CompareExchange(ref slot, value, weakReference) != weakReference);
		return syntaxNode;
	}

	public override string ToString()
	{
		return Green.ToString();
	}

	public virtual string ToFullString()
	{
		return Green.ToFullString();
	}

	public virtual void WriteTo(TextWriter writer)
	{
		Green.WriteTo(writer, leading: true, trailing: true);
	}

	public SourceText GetText(Encoding encoding = null, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1)
	{
		StringBuilder stringBuilder = new StringBuilder();
		WriteTo(new StringWriter(stringBuilder));
		return new StringBuilderText(stringBuilder, encoding, checksumAlgorithm);
	}

	public bool IsEquivalentTo(SyntaxNode other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		return Green.IsEquivalentTo(other.Green);
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

	public bool Contains(SyntaxNode node)
	{
		if (node == null || !FullSpan.Contains(node.FullSpan))
		{
			return false;
		}
		while (node != null)
		{
			if (node == this)
			{
				return true;
			}
			node = ((node.Parent == null) ? ((!node.IsStructuredTrivia) ? null : ((IStructuredTriviaSyntax)node).ParentTrivia.Token.Parent) : node.Parent);
		}
		return false;
	}

	internal abstract SyntaxNode GetCachedSlot(int index);

	internal int GetChildIndex(int slot)
	{
		int num = 0;
		for (int i = 0; i < slot; i++)
		{
			GreenNode slot2 = Green.GetSlot(i);
			if (slot2 != null)
			{
				num = ((!slot2.IsList) ? (num + 1) : (num + slot2.SlotCount));
			}
		}
		return num;
	}

	internal virtual int GetChildPosition(int index)
	{
		int num = 0;
		GreenNode green = Green;
		while (index > 0)
		{
			index--;
			SyntaxNode cachedSlot = GetCachedSlot(index);
			if (cachedSlot != null)
			{
				return cachedSlot.EndPosition + num;
			}
			GreenNode slot = green.GetSlot(index);
			if (slot != null)
			{
				num += slot.FullWidth;
			}
		}
		return Position + num;
	}

	public Location GetLocation()
	{
		return SyntaxTree.GetLocation(Span);
	}

	public IEnumerable<Diagnostic> GetDiagnostics()
	{
		return SyntaxTree.GetDiagnostics(this);
	}

	public SyntaxReference GetReference()
	{
		return SyntaxTree.GetReference(this);
	}

	public ChildSyntaxList ChildNodesAndTokens()
	{
		return new ChildSyntaxList(this);
	}

	public virtual SyntaxNodeOrToken ChildThatContainsPosition(int position)
	{
		if (!FullSpan.Contains(position))
		{
			throw new ArgumentOutOfRangeException("position");
		}
		return ChildSyntaxList.ChildThatContainsPosition(this, position);
	}

	internal abstract SyntaxNode GetNodeSlot(int slot);

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

	public IEnumerable<SyntaxNode> Ancestors(bool ascendOutOfTrivia = true)
	{
		return Parent?.AncestorsAndSelf(ascendOutOfTrivia) ?? SpecializedCollections.EmptyEnumerable<SyntaxNode>();
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
		SyntaxNode parent = node.Parent;
		if (((parent == null) & ascendOutOfTrivia) && node is IStructuredTriviaSyntax { ParentTrivia: { Token: var token } })
		{
			parent = token.Parent;
		}
		return parent;
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

	public IEnumerable<SyntaxNode> DescendantNodes(Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
	{
		return DescendantNodesImpl(FullSpan, descendIntoChildren, descendIntoTrivia, includeSelf: false);
	}

	public IEnumerable<SyntaxNode> DescendantNodes(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
	{
		return DescendantNodesImpl(span, descendIntoChildren, descendIntoTrivia, includeSelf: false);
	}

	public IEnumerable<SyntaxNode> DescendantNodesAndSelf(Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
	{
		return DescendantNodesImpl(FullSpan, descendIntoChildren, descendIntoTrivia, includeSelf: true);
	}

	public IEnumerable<SyntaxNode> DescendantNodesAndSelf(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
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
				SyntaxNode parent = syntaxNode.Parent;
				if (parent == null || parent.FullWidth != syntaxNode.FullWidth || parent == syntaxNode2)
				{
					break;
				}
				syntaxNode = parent;
			}
		}
		return syntaxNode;
	}

	public SyntaxToken FindToken(int position, bool findInsideTrivia = false)
	{
		return FindTokenCore(position, findInsideTrivia);
	}

	public SyntaxToken GetFirstToken(bool includeZeroWidth = false, bool includeSkipped = false, bool includeDirectives = false, bool includeDocumentationComments = false)
	{
		return SyntaxNavigator.Instance.GetFirstToken(this, includeZeroWidth, includeSkipped, includeDirectives, includeDocumentationComments);
	}

	public SyntaxToken GetLastToken(bool includeZeroWidth = false, bool includeSkipped = false, bool includeDirectives = false, bool includeDocumentationComments = false)
	{
		return SyntaxNavigator.Instance.GetLastToken(this, includeZeroWidth, includeSkipped, includeDirectives, includeDocumentationComments);
	}

	public IEnumerable<SyntaxToken> ChildTokens()
	{
		foreach (SyntaxNodeOrToken item in ChildNodesAndTokens())
		{
			if (item.IsToken)
			{
				yield return item.AsToken();
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

	public SyntaxTriviaList GetLeadingTrivia()
	{
		return GetFirstToken(includeZeroWidth: true).LeadingTrivia;
	}

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

	public bool HasAnnotations(string annotationKind)
	{
		return Green.HasAnnotations(annotationKind);
	}

	public bool HasAnnotations(IEnumerable<string> annotationKinds)
	{
		return Green.HasAnnotations(annotationKinds);
	}

	public bool HasAnnotation(SyntaxAnnotation annotation)
	{
		return Green.HasAnnotation(annotation);
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(string annotationKind)
	{
		return Green.GetAnnotations(annotationKind);
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(IEnumerable<string> annotationKinds)
	{
		return Green.GetAnnotations(annotationKinds);
	}

	internal SyntaxAnnotation[] GetAnnotations()
	{
		return Green.GetAnnotations();
	}

	public IEnumerable<SyntaxNodeOrToken> GetAnnotatedNodesAndTokens(string annotationKind)
	{
		return from t in DescendantNodesAndTokensAndSelf((SyntaxNode n) => n.ContainsAnnotations, descendIntoTrivia: true)
			where t.HasAnnotations(annotationKind)
			select t;
	}

	public IEnumerable<SyntaxNodeOrToken> GetAnnotatedNodesAndTokens(params string[] annotationKinds)
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

	public IEnumerable<SyntaxNode> GetAnnotatedNodes(string annotationKind)
	{
		return from n in GetAnnotatedNodesAndTokens(annotationKind)
			where n.IsNode
			select n.AsNode();
	}

	public IEnumerable<SyntaxToken> GetAnnotatedTokens(SyntaxAnnotation syntaxAnnotation)
	{
		return from n in GetAnnotatedNodesAndTokens(syntaxAnnotation)
			where n.IsToken
			select n.AsToken();
	}

	public IEnumerable<SyntaxToken> GetAnnotatedTokens(string annotationKind)
	{
		return from n in GetAnnotatedNodesAndTokens(annotationKind)
			where n.IsToken
			select n.AsToken();
	}

	public IEnumerable<SyntaxTrivia> GetAnnotatedTrivia(string annotationKind)
	{
		return from tr in DescendantTrivia((SyntaxNode n) => n.ContainsAnnotations, descendIntoTrivia: true)
			where tr.HasAnnotations(annotationKind)
			select tr;
	}

	public IEnumerable<SyntaxTrivia> GetAnnotatedTrivia(params string[] annotationKinds)
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

	internal SyntaxNode WithAdditionalAnnotationsInternal(IEnumerable<SyntaxAnnotation> annotations)
	{
		return Green.WithAdditionalAnnotationsGreen(annotations).CreateRed();
	}

	internal SyntaxNode GetNodeWithoutAnnotations(IEnumerable<SyntaxAnnotation> annotations)
	{
		return Green.WithoutAnnotationsGreen(annotations).CreateRed();
	}

	public T CopyAnnotationsTo<T>(T node) where T : SyntaxNode
	{
		if (node == null)
		{
			return null;
		}
		SyntaxAnnotation[] annotations = Green.GetAnnotations();
		if (annotations != null && annotations.Length != 0)
		{
			return (T)node.Green.WithAdditionalAnnotationsGreen(annotations).CreateRed();
		}
		return node;
	}

	public bool IsEquivalentTo(SyntaxNode node, bool topLevel = false)
	{
		return IsEquivalentToCore(node, topLevel);
	}

	public virtual void SerializeTo(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanWrite)
		{
			throw new InvalidOperationException(CodeAnalysisResources.TheStreamCannotBeWrittenTo);
		}
		using ObjectWriter objectWriter = new ObjectWriter(stream, cancellationToken);
		objectWriter.WriteValue(Green);
	}

	protected virtual bool EquivalentToCore(SyntaxNode other)
	{
		return IsEquivalentTo(other);
	}

	protected virtual SyntaxToken FindTokenCore(int position, bool findInsideTrivia)
	{
		if (findInsideTrivia)
		{
			return FindToken(position, SyntaxTrivia.Any);
		}
		if (TryGetEofAt(position, out var Eof))
		{
			return Eof;
		}
		if (!FullSpan.Contains(position))
		{
			throw new ArgumentOutOfRangeException("position");
		}
		return FindTokenInternal(position);
	}

	private bool TryGetEofAt(int position, out SyntaxToken Eof)
	{
		if (position == EndPosition && this is ICompilationUnitSyntax compilationUnitSyntax)
		{
			Eof = compilationUnitSyntax.EndOfFileToken;
			return true;
		}
		Eof = default;
		return false;
	}

	internal SyntaxToken FindTokenInternal(int position)
	{
		SyntaxNodeOrToken syntaxNodeOrToken = this;
		while (true)
		{
			SyntaxNode syntaxNode = syntaxNodeOrToken.AsNode();
			if (syntaxNode == null)
			{
				break;
			}
			syntaxNodeOrToken = syntaxNode.ChildThatContainsPosition(position);
		}
		return syntaxNodeOrToken.AsToken();
	}

	private SyntaxToken FindToken(int position, Func<SyntaxTrivia, bool> findInsideTrivia)
	{
		return FindTokenCore(position, findInsideTrivia);
	}

	protected virtual SyntaxToken FindTokenCore(int position, Func<SyntaxTrivia, bool> stepInto)
	{
		SyntaxToken token = FindToken(position);
		if (stepInto != null)
		{
			SyntaxTrivia triviaFromSyntaxToken = GetTriviaFromSyntaxToken(position, in token);
			if (triviaFromSyntaxToken.HasStructure && stepInto(triviaFromSyntaxToken))
			{
				token = triviaFromSyntaxToken.GetStructure().FindTokenInternal(position);
			}
		}
		return token;
	}

	internal static SyntaxTrivia GetTriviaFromSyntaxToken(int position, in SyntaxToken token)
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

	internal static SyntaxTrivia GetTriviaThatContainsPosition(in SyntaxTriviaList list, int position)
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

	protected virtual SyntaxTrivia FindTriviaCore(int position, bool findInsideTrivia)
	{
		return FindTrivia(position, findInsideTrivia);
	}

	protected internal abstract SyntaxNode ReplaceCore<TNode>(IEnumerable<TNode> nodes = null, Func<TNode, TNode, SyntaxNode> computeReplacementNode = null, IEnumerable<SyntaxToken> tokens = null, Func<SyntaxToken, SyntaxToken, SyntaxToken> computeReplacementToken = null, IEnumerable<SyntaxTrivia> trivia = null, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia = null) where TNode : SyntaxNode;

	protected internal abstract SyntaxNode ReplaceNodeInListCore(SyntaxNode originalNode, IEnumerable<SyntaxNode> replacementNodes);

	protected internal abstract SyntaxNode InsertNodesInListCore(SyntaxNode nodeInList, IEnumerable<SyntaxNode> nodesToInsert, bool insertBefore);

	protected internal abstract SyntaxNode ReplaceTokenInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens);

	protected internal abstract SyntaxNode InsertTokensInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens, bool insertBefore);

	protected internal abstract SyntaxNode ReplaceTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia);

	protected internal abstract SyntaxNode InsertTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia, bool insertBefore);

	protected internal abstract SyntaxNode RemoveNodesCore(IEnumerable<SyntaxNode> nodes, SyntaxRemoveOptions options);

	protected internal abstract SyntaxNode NormalizeWhitespaceCore(string indentation, string eol, bool elasticTrivia);

	protected abstract bool IsEquivalentToCore(SyntaxNode node, bool topLevel = false);

	internal virtual bool ShouldCreateWeakList()
	{
		return false;
	}

	private bool HasErrorsSlow()
	{
		return new SyntaxDiagnosticInfoList(Green).Any((DiagnosticInfo info) => info.Severity == DiagnosticSeverity.Error);
	}

	internal static T CloneNodeAsRoot<T>(T node, SyntaxTree syntaxTree) where T : SyntaxNode
	{
		T val = (T)node.Green.CreateRed(null, 0);
		val._syntaxTree = syntaxTree;
		return val;
	}

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

	private static bool IsInSpan(in TextSpan span, TextSpan childSpan)
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
		if (includeSelf && IsInSpan(in span, FullSpan))
		{
			yield return this;
		}
		using ChildSyntaxListEnumeratorStack stack = new ChildSyntaxListEnumeratorStack(this, descendIntoChildren);
		while (stack.IsNotEmpty)
		{
			SyntaxNode syntaxNode = stack.TryGetNextAsNodeInSpan(in span);
			if (syntaxNode != null)
			{
				stack.PushChildren(syntaxNode, descendIntoChildren);
				yield return syntaxNode;
			}
		}
	}

	private IEnumerable<SyntaxNodeOrToken> DescendantNodesAndTokensOnly(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren, bool includeSelf)
	{
		if (includeSelf && IsInSpan(in span, FullSpan))
		{
			yield return this;
		}
		using ChildSyntaxListEnumeratorStack stack = new ChildSyntaxListEnumeratorStack(this, descendIntoChildren);
		while (stack.IsNotEmpty)
		{
			if (stack.TryGetNextInSpan(in span, out var value))
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
		if (includeSelf && IsInSpan(in span, FullSpan))
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
				if (!stack.TryGetNextInSpan(in span, out var value2))
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
							stack.PushTrailingTrivia(in token);
						}
						stack.PushToken(in value2);
						if (token.HasLeadingTrivia)
						{
							stack.PushLeadingTrivia(in token);
						}
						break;
					}
				}
				yield return value2;
				break;
			}
			case ThreeEnumeratorListStack.Which.Trivia:
			{
				if (stack.TryGetNext(out var value) && value.HasStructure && IsInSpan(in span, value.FullSpan))
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
			if (!stack.TryGetNextInSpan(in span, out var value))
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
					if (IsInSpan(in span, leadingTrivium.FullSpan))
					{
						yield return leadingTrivium;
					}
				}
				foreach (SyntaxTrivia trailingTrivium in token.TrailingTrivia)
				{
					if (IsInSpan(in span, trailingTrivium.FullSpan))
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
				if (!stack.TryGetNextInSpan(in span, out var value2))
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
						stack.PushTrailingTrivia(in token);
					}
					if (token.HasLeadingTrivia)
					{
						stack.PushLeadingTrivia(in token);
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
					if (IsInSpan(in span, value.FullSpan))
					{
						yield return value;
					}
				}
				break;
			}
			}
		}
	}
}
