using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public struct SyntaxNodeOrToken : IEquatable<SyntaxNodeOrToken>
{
	private readonly SyntaxNode nodeOrParent;

	private readonly InternalSyntaxNode token;

	private readonly int position;

	private readonly int tokenIndex;

	internal LexerMode LexedMode
	{
		get
		{
			if (token != null)
			{
				return token.LexedMode;
			}
			if (nodeOrParent != null && nodeOrParent.Node != null)
			{
				return nodeOrParent.Node.LexedMode;
			}
			return LexerMode.Default;
		}
	}

	private string KindText
	{
		get
		{
			if (token != null)
			{
				return token.KindText;
			}
			if (nodeOrParent != null)
			{
				return nodeOrParent.Node.KindText;
			}
			return "None";
		}
	}

	public SyntaxKind Kind
	{
		get
		{
			if (token == null)
			{
				if (nodeOrParent == null)
				{
					return SyntaxKind.None;
				}
				return nodeOrParent.Kind;
			}
			return token.Kind;
		}
	}

	public bool IsMissing
	{
		get
		{
			if (token == null)
			{
				if (nodeOrParent == null)
				{
					return false;
				}
				return nodeOrParent.IsMissing;
			}
			return token.IsMissing;
		}
	}

	public SyntaxNode Parent
	{
		get
		{
			if (token == null)
			{
				if (nodeOrParent == null)
				{
					return null;
				}
				return nodeOrParent.Parent;
			}
			return nodeOrParent;
		}
	}

	internal InternalSyntaxNode UnderlyingNode
	{
		get
		{
			InternalSyntaxNode node = token;
			if (node == null)
			{
				if (nodeOrParent == null)
				{
					return null;
				}
				node = nodeOrParent.Node;
			}
			return node;
		}
	}

	internal int Position => position;

	public bool IsToken => !IsNode;

	public bool IsNode => tokenIndex < 0;

	public TextSpan Span
	{
		get
		{
			if (token != null)
			{
				return AsToken().Span;
			}
			if (nodeOrParent != null)
			{
				return nodeOrParent.Span;
			}
			return default;
		}
	}

	public int SpanStart
	{
		get
		{
			if (token != null)
			{
				return position + token.GetLeadingTriviaWidth();
			}
			if (nodeOrParent != null)
			{
				return nodeOrParent.SpanStart;
			}
			return 0;
		}
	}

	public TextSpan FullSpan
	{
		get
		{
			if (token != null)
			{
				return new TextSpan(Position, token.FullWidth);
			}
			if (nodeOrParent != null)
			{
				return nodeOrParent.FullSpan;
			}
			return default;
		}
	}

	public bool HasLeadingTrivia => GetLeadingTrivia().Count > 0;

	public bool HasTrailingTrivia => GetTrailingTrivia().Count > 0;

	public bool ContainsDiagnostics
	{
		get
		{
			if (token != null)
			{
				return token.ContainsDiagnostics;
			}
			if (nodeOrParent != null)
			{
				return nodeOrParent.ContainsDiagnostics;
			}
			return false;
		}
	}

	public bool ContainsAnnotations
	{
		get
		{
			if (token != null)
			{
				return token.ContainsAnnotations;
			}
			if (nodeOrParent != null)
			{
				return nodeOrParent.ContainsAnnotations;
			}
			return false;
		}
	}

	public SyntaxTree SyntaxTree
	{
		get
		{
			if (nodeOrParent == null)
			{
				return null;
			}
			return nodeOrParent.SyntaxTree;
		}
	}

	internal int Width
	{
		get
		{
			if (token == null)
			{
				if (nodeOrParent == null)
				{
					return 0;
				}
				return nodeOrParent.Width;
			}
			return token.Width;
		}
	}

	internal int FullWidth
	{
		get
		{
			if (token == null)
			{
				if (nodeOrParent == null)
				{
					return 0;
				}
				return nodeOrParent.FullWidth;
			}
			return token.FullWidth;
		}
	}

	internal int EndPosition => position + FullWidth;

	internal SyntaxNodeOrToken(SyntaxNode node)
	{
		this = default;
		if (node != null)
		{
			DebugAssertHelper.Assert(!node.Node.IsList, "node cannot be a list");
			position = node.Position;
			nodeOrParent = node;
		}
		tokenIndex = -1;
	}

	internal SyntaxNodeOrToken(SyntaxNode parent, InternalSyntaxNode token, int position, int index)
	{
		DebugAssertHelper.Assert(token != null || (parent == null && position == 0 && index == 0), "parts must form a token");
		DebugAssertHelper.Assert(token?.IsToken ?? true, "token must be a token");
		DebugAssertHelper.Assert(index >= 0, "index must not be negative");
		DebugAssertHelper.Assert(parent == null || token != null, "null token cannot have parent");
		this.position = position;
		tokenIndex = index;
		nodeOrParent = parent;
		this.token = token;
	}

	internal string GetDebuggerDisplay()
	{
		return GetType().Name + " " + KindText + " " + this;
	}

	public SyntaxToken AsToken()
	{
		if (token != null)
		{
			return new SyntaxToken(nodeOrParent, token, Position, tokenIndex);
		}
		return default;
	}

	public SyntaxNode AsNode()
	{
		if (token != null)
		{
			return null;
		}
		return nodeOrParent;
	}

	public ChildSyntaxList ChildNodesAndTokens()
	{
		if (!IsToken)
		{
			return nodeOrParent.ChildNodesAndTokens();
		}
		return default;
	}

	public override string ToString()
	{
		if (token != null)
		{
			return token.ToString();
		}
		if (nodeOrParent != null)
		{
			return nodeOrParent.ToString();
		}
		return string.Empty;
	}

	public string ToFullString()
	{
		if (token != null)
		{
			return token.ToFullString();
		}
		if (nodeOrParent != null)
		{
			return nodeOrParent.ToFullString();
		}
		return string.Empty;
	}

	public void WriteTo(TextWriter writer)
	{
		if (token != null)
		{
			token.WriteTo(writer);
		}
		else if (nodeOrParent != null)
		{
			nodeOrParent.WriteTo(writer);
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public SyntaxTriviaList GetLeadingTrivia()
	{
		if (token != null)
		{
			return AsToken().LeadingTrivia;
		}
		if (nodeOrParent != null)
		{
			return nodeOrParent.GetLeadingTrivia();
		}
		return default;
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public SyntaxTriviaList GetTrailingTrivia()
	{
		if (token != null)
		{
			return AsToken().TrailingTrivia;
		}
		if (nodeOrParent != null)
		{
			return nodeOrParent.GetTrailingTrivia();
		}
		return default;
	}

	public SyntaxNodeOrToken WithLeadingTrivia(IEnumerable<SyntaxTrivia> trivia)
	{
		if (token != null)
		{
			return AsToken().WithLeadingTrivia(trivia);
		}
		if (nodeOrParent != null)
		{
			return nodeOrParent.WithLeadingTrivia(trivia);
		}
		return this;
	}

	public SyntaxNodeOrToken WithLeadingTrivia(params SyntaxTrivia[] trivia)
	{
		return WithLeadingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public SyntaxNodeOrToken WithTrailingTrivia(IEnumerable<SyntaxTrivia> trivia)
	{
		if (token != null)
		{
			return AsToken().WithTrailingTrivia(trivia);
		}
		if (nodeOrParent != null)
		{
			return nodeOrParent.WithTrailingTrivia(trivia);
		}
		return this;
	}

	public SyntaxNodeOrToken WithTrailingTrivia(params SyntaxTrivia[] trivia)
	{
		return WithTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public bool HasAnnotations(AnnotationKind annotationKind)
	{
		if (token != null)
		{
			return token.HasAnnotations(annotationKind);
		}
		if (nodeOrParent != null)
		{
			return nodeOrParent.HasAnnotations(annotationKind);
		}
		return false;
	}

	public bool HasAnnotations(IEnumerable<AnnotationKind> annotationKinds)
	{
		if (token != null)
		{
			return token.HasAnnotations(annotationKinds);
		}
		if (nodeOrParent != null)
		{
			return nodeOrParent.HasAnnotations(annotationKinds);
		}
		return false;
	}

	public bool HasAnnotation(SyntaxAnnotation annotation)
	{
		if (token != null)
		{
			return token.HasAnnotation(annotation);
		}
		if (nodeOrParent != null)
		{
			return nodeOrParent.HasAnnotation(annotation);
		}
		return false;
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(AnnotationKind annotationKind)
	{
		if (token != null)
		{
			return token.GetAnnotations(annotationKind);
		}
		if (nodeOrParent != null)
		{
			return nodeOrParent.GetAnnotations(annotationKind);
		}
		return SpecializedCollections.EmptyEnumerable<SyntaxAnnotation>();
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(IEnumerable<AnnotationKind> annotationKinds)
	{
		if (token != null)
		{
			return token.GetAnnotations(annotationKinds);
		}
		if (nodeOrParent != null)
		{
			return nodeOrParent.GetAnnotations(annotationKinds);
		}
		return SpecializedCollections.EmptyEnumerable<SyntaxAnnotation>();
	}

	public SyntaxNodeOrToken WithAdditionalAnnotations(params SyntaxAnnotation[] annotations)
	{
		return WithAdditionalAnnotations((IEnumerable<SyntaxAnnotation>)annotations);
	}

	public SyntaxNodeOrToken WithAdditionalAnnotations(IEnumerable<SyntaxAnnotation> annotations)
	{
		if (annotations == null)
		{
			throw new ArgumentNullException("annotations");
		}
		if (token != null)
		{
			return AsToken().WithAdditionalAnnotations(annotations);
		}
		if (nodeOrParent != null)
		{
			return nodeOrParent.WithAdditionalAnnotations(annotations);
		}
		return this;
	}

	public SyntaxNodeOrToken WithoutAnnotations(params SyntaxAnnotation[] annotations)
	{
		return WithoutAnnotations((IEnumerable<SyntaxAnnotation>)annotations);
	}

	public SyntaxNodeOrToken WithoutAnnotations(IEnumerable<SyntaxAnnotation> annotations)
	{
		if (annotations == null)
		{
			throw new ArgumentNullException("annotations");
		}
		if (token != null)
		{
			return AsToken().WithoutAnnotations(annotations);
		}
		if (nodeOrParent != null)
		{
			return nodeOrParent.WithoutAnnotations(annotations);
		}
		return this;
	}

	public SyntaxNodeOrToken WithoutAnnotations(AnnotationKind annotationKind)
	{
		if (HasAnnotations(annotationKind))
		{
			return WithoutAnnotations(GetAnnotations(annotationKind));
		}
		return this;
	}

	public bool Equals(SyntaxNodeOrToken other)
	{
		DebugAssertHelper.Assert((nodeOrParent == other.nodeOrParent && token == other.token && position == other.position && tokenIndex == other.tokenIndex) == (nodeOrParent == other.nodeOrParent && token == other.token && tokenIndex == other.tokenIndex));
		if (nodeOrParent == other.nodeOrParent && token == other.token)
		{
			return tokenIndex == other.tokenIndex;
		}
		return false;
	}

	public static bool operator ==(SyntaxNodeOrToken left, SyntaxNodeOrToken right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SyntaxNodeOrToken left, SyntaxNodeOrToken right)
	{
		return !left.Equals(right);
	}

	public override bool Equals(object obj)
	{
		if (obj is SyntaxNodeOrToken)
		{
			return Equals((SyntaxNodeOrToken)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(nodeOrParent.GetHashCode(), Hash.Combine(token.GetHashCode(), tokenIndex.GetHashCode()));
	}

	public bool IsEquivalentTo(SyntaxNodeOrToken other)
	{
		if (IsNode != other.IsNode)
		{
			return false;
		}
		InternalSyntaxNode underlyingNode = UnderlyingNode;
		InternalSyntaxNode underlyingNode2 = other.UnderlyingNode;
		if (underlyingNode != underlyingNode2)
		{
			return underlyingNode?.IsEquivalentTo(underlyingNode2) ?? false;
		}
		return true;
	}

	public static implicit operator SyntaxNodeOrToken(SyntaxToken token)
	{
		return new SyntaxNodeOrToken(token.Parent, token.Node, token.Position, token.Index);
	}

	public static explicit operator SyntaxToken(SyntaxNodeOrToken nodeOrToken)
	{
		return nodeOrToken.AsToken();
	}

	public static implicit operator SyntaxNodeOrToken(SyntaxNode node)
	{
		return new SyntaxNodeOrToken(node);
	}

	public static explicit operator SyntaxNode(SyntaxNodeOrToken nodeOrToken)
	{
		return nodeOrToken.AsNode();
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public Location GetLocation()
	{
		if (token == null)
		{
			if (nodeOrParent == null)
			{
				return null;
			}
			return nodeOrParent.GetLocation();
		}
		return AsToken().GetLocation();
	}

	internal IList<TDirective> GetDirectives<TDirective>(Func<TDirective, bool> filter = null) where TDirective : SyntaxNode
	{
		List<TDirective> directives = null;
		GetDirectives(this, filter, ref directives);
		IList<TDirective> list = directives;
		return list ?? SpecializedCollections.EmptyList<TDirective>();
	}

	private static void GetDirectives<TDirective>(SyntaxNodeOrToken node, Func<TDirective, bool> filter, ref List<TDirective> directives) where TDirective : SyntaxNode
	{
		if (node.token != null)
		{
			GetDirectives(node.AsToken(), filter, ref directives);
		}
		else if (node.nodeOrParent != null)
		{
			GetDirectives(node.nodeOrParent, filter, ref directives);
		}
	}

	public static int GetFirstChildIndexSpanningPosition(SyntaxNode node, int position)
	{
		if (!node.FullSpan.IntersectsWith(position))
		{
			throw new ArgumentException("Must be within node's FullSpan", "position");
		}
		return GetFirstChildIndexSpanningPosition(node.ChildNodesAndTokens(), position);
	}

	internal static int GetFirstChildIndexSpanningPosition(ChildSyntaxList list, int position)
	{
		int num = 0;
		int num2 = list.Count - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num >> 1);
			SyntaxNodeOrToken syntaxNodeOrToken = list[num3];
			if (position < syntaxNodeOrToken.Position)
			{
				num2 = num3 - 1;
				continue;
			}
			if (position == syntaxNodeOrToken.Position)
			{
				while (num3 > 0 && list[num3 - 1].FullWidth == 0)
				{
					num3--;
				}
				return num3;
			}
			if (position >= syntaxNodeOrToken.EndPosition)
			{
				num = num3 + 1;
				continue;
			}
			return num3;
		}
		throw new NotSupportedException();
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public SyntaxNodeOrToken GetNextSibling()
	{
		SyntaxNode parent = Parent;
		if (parent == null)
		{
			return default;
		}
		ChildSyntaxList siblings = parent.ChildNodesAndTokens();
		if (siblings.Count >= 8)
		{
			return GetNextSiblingWithSearch(siblings);
		}
		return GetNextSiblingFromStart(siblings);
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public SyntaxNodeOrToken GetPreviousSibling()
	{
		if (Parent != null)
		{
			bool flag = false;
			foreach (SyntaxNodeOrToken item in Parent.ChildNodesAndTokens().Reverse())
			{
				if (flag)
				{
					return item;
				}
				if (item == this)
				{
					flag = true;
				}
			}
		}
		return default;
	}

	private SyntaxNodeOrToken GetNextSiblingFromStart(ChildSyntaxList siblings)
	{
		bool flag = false;
		foreach (SyntaxNodeOrToken item in siblings)
		{
			if (flag)
			{
				return item;
			}
			if (item == this)
			{
				flag = true;
			}
		}
		return default;
	}

	private SyntaxNodeOrToken GetNextSiblingWithSearch(ChildSyntaxList siblings)
	{
		int firstChildIndexSpanningPosition = GetFirstChildIndexSpanningPosition(siblings, position);
		int count = siblings.Count;
		bool flag = false;
		for (int i = firstChildIndexSpanningPosition; i < count; i++)
		{
			if (flag)
			{
				return siblings[i];
			}
			if (siblings[i] == this)
			{
				flag = true;
			}
		}
		return default;
	}
}
