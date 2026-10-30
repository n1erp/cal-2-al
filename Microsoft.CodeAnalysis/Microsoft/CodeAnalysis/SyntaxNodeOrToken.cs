using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public readonly struct SyntaxNodeOrToken : IEquatable<SyntaxNodeOrToken>
{
	private readonly SyntaxNode _nodeOrParent;

	private readonly GreenNode _token;

	private readonly int _position;

	private readonly int _tokenIndex;

	private string KindText
	{
		get
		{
			if (_token != null)
			{
				return _token.KindText;
			}
			if (_nodeOrParent != null)
			{
				return _nodeOrParent.Green.KindText;
			}
			return "None";
		}
	}

	public int RawKind => _token?.RawKind ?? _nodeOrParent?.RawKind ?? 0;

	public string Language
	{
		get
		{
			if (_token != null)
			{
				return _token.Language;
			}
			if (_nodeOrParent != null)
			{
				return _nodeOrParent.Language;
			}
			return string.Empty;
		}
	}

	public bool IsMissing => _token?.IsMissing ?? _nodeOrParent?.IsMissing ?? false;

	public SyntaxNode Parent
	{
		get
		{
			if (_token == null)
			{
				return _nodeOrParent?.Parent;
			}
			return _nodeOrParent;
		}
	}

	internal GreenNode UnderlyingNode
	{
		get
		{
			GreenNode greenNode = _token;
			if (greenNode == null)
			{
				SyntaxNode nodeOrParent = _nodeOrParent;
				if (nodeOrParent == null)
				{
					return null;
				}
				greenNode = nodeOrParent.Green;
			}
			return greenNode;
		}
	}

	internal int Position => _position;

	public bool IsToken => !IsNode;

	public bool IsNode => _tokenIndex < 0;

	public TextSpan Span
	{
		get
		{
			if (_token != null)
			{
				return AsToken().Span;
			}
			if (_nodeOrParent != null)
			{
				return _nodeOrParent.Span;
			}
			return default;
		}
	}

	public int SpanStart
	{
		get
		{
			if (_token != null)
			{
				return _position + _token.GetLeadingTriviaWidth();
			}
			if (_nodeOrParent != null)
			{
				return _nodeOrParent.SpanStart;
			}
			return 0;
		}
	}

	public TextSpan FullSpan
	{
		get
		{
			if (_token != null)
			{
				return new TextSpan(Position, _token.FullWidth);
			}
			if (_nodeOrParent != null)
			{
				return _nodeOrParent.FullSpan;
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
			if (_token != null)
			{
				return _token.ContainsDiagnostics;
			}
			if (_nodeOrParent != null)
			{
				return _nodeOrParent.ContainsDiagnostics;
			}
			return false;
		}
	}

	public bool ContainsDirectives
	{
		get
		{
			if (_token != null)
			{
				return _token.ContainsDirectives;
			}
			if (_nodeOrParent != null)
			{
				return _nodeOrParent.ContainsDirectives;
			}
			return false;
		}
	}

	public bool ContainsAnnotations
	{
		get
		{
			if (_token != null)
			{
				return _token.ContainsAnnotations;
			}
			if (_nodeOrParent != null)
			{
				return _nodeOrParent.ContainsAnnotations;
			}
			return false;
		}
	}

	public SyntaxTree SyntaxTree => _nodeOrParent?.SyntaxTree;

	internal int Width => _token?.Width ?? _nodeOrParent?.Width ?? 0;

	internal int FullWidth => _token?.FullWidth ?? _nodeOrParent?.FullWidth ?? 0;

	internal int EndPosition => _position + FullWidth;

	internal SyntaxNodeOrToken(SyntaxNode node)
	{
		this = default;
		if (node != null)
		{
			_position = node.Position;
			_nodeOrParent = node;
		}
		_tokenIndex = -1;
	}

	internal SyntaxNodeOrToken(SyntaxNode parent, GreenNode token, int position, int index)
	{
		_position = position;
		_tokenIndex = index;
		_nodeOrParent = parent;
		_token = token;
	}

	internal string GetDebuggerDisplay()
	{
		return GetType().Name + " " + KindText + " " + ToString();
	}

	public SyntaxToken AsToken()
	{
		if (_token != null)
		{
			return new SyntaxToken(_nodeOrParent, _token, Position, _tokenIndex);
		}
		return default;
	}

	public SyntaxNode AsNode()
	{
		if (_token != null)
		{
			return null;
		}
		return _nodeOrParent;
	}

	public ChildSyntaxList ChildNodesAndTokens()
	{
		if (!IsToken)
		{
			return _nodeOrParent.ChildNodesAndTokens();
		}
		return default;
	}

	public override string ToString()
	{
		if (_token != null)
		{
			return _token.ToString();
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.ToString();
		}
		return string.Empty;
	}

	public string ToFullString()
	{
		if (_token != null)
		{
			return _token.ToFullString();
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.ToFullString();
		}
		return string.Empty;
	}

	public void WriteTo(TextWriter writer)
	{
		if (_token != null)
		{
			_token.WriteTo(writer);
		}
		else
		{
			_nodeOrParent?.WriteTo(writer);
		}
	}

	public SyntaxTriviaList GetLeadingTrivia()
	{
		if (_token != null)
		{
			return AsToken().LeadingTrivia;
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.GetLeadingTrivia();
		}
		return default;
	}

	public SyntaxTriviaList GetTrailingTrivia()
	{
		if (_token != null)
		{
			return AsToken().TrailingTrivia;
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.GetTrailingTrivia();
		}
		return default;
	}

	public SyntaxNodeOrToken WithLeadingTrivia(IEnumerable<SyntaxTrivia> trivia)
	{
		if (_token != null)
		{
			return AsToken().WithLeadingTrivia(trivia);
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.WithLeadingTrivia(trivia);
		}
		return this;
	}

	public SyntaxNodeOrToken WithLeadingTrivia(params SyntaxTrivia[] trivia)
	{
		return WithLeadingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public SyntaxNodeOrToken WithTrailingTrivia(IEnumerable<SyntaxTrivia> trivia)
	{
		if (_token != null)
		{
			return AsToken().WithTrailingTrivia(trivia);
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.WithTrailingTrivia(trivia);
		}
		return this;
	}

	public SyntaxNodeOrToken WithTrailingTrivia(params SyntaxTrivia[] trivia)
	{
		return WithTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public IEnumerable<Diagnostic> GetDiagnostics()
	{
		if (_token != null)
		{
			return AsToken().GetDiagnostics();
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.GetDiagnostics();
		}
		return SpecializedCollections.EmptyEnumerable<Diagnostic>();
	}

	public bool HasAnnotations(string annotationKind)
	{
		if (_token != null)
		{
			return _token.HasAnnotations(annotationKind);
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.HasAnnotations(annotationKind);
		}
		return false;
	}

	public bool HasAnnotations(IEnumerable<string> annotationKinds)
	{
		if (_token != null)
		{
			return _token.HasAnnotations(annotationKinds);
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.HasAnnotations(annotationKinds);
		}
		return false;
	}

	public bool HasAnnotation(SyntaxAnnotation annotation)
	{
		if (_token != null)
		{
			return _token.HasAnnotation(annotation);
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.HasAnnotation(annotation);
		}
		return false;
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(string annotationKind)
	{
		if (_token != null)
		{
			return _token.GetAnnotations(annotationKind);
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.GetAnnotations(annotationKind);
		}
		return SpecializedCollections.EmptyEnumerable<SyntaxAnnotation>();
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(IEnumerable<string> annotationKinds)
	{
		if (_token != null)
		{
			return _token.GetAnnotations(annotationKinds);
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.GetAnnotations(annotationKinds);
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
		if (_token != null)
		{
			return AsToken().WithAdditionalAnnotations(annotations);
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.WithAdditionalAnnotations(annotations);
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
		if (_token != null)
		{
			return AsToken().WithoutAnnotations(annotations);
		}
		if (_nodeOrParent != null)
		{
			return _nodeOrParent.WithoutAnnotations(annotations);
		}
		return this;
	}

	public SyntaxNodeOrToken WithoutAnnotations(string annotationKind)
	{
		if (annotationKind == null)
		{
			throw new ArgumentNullException("annotationKind");
		}
		if (HasAnnotations(annotationKind))
		{
			return WithoutAnnotations(GetAnnotations(annotationKind));
		}
		return this;
	}

	public bool Equals(SyntaxNodeOrToken other)
	{
		if (_nodeOrParent == other._nodeOrParent && _token == other._token)
		{
			return _tokenIndex == other._tokenIndex;
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
		return Hash.Combine(_nodeOrParent, Hash.Combine(_token, _tokenIndex));
	}

	public bool IsEquivalentTo(SyntaxNodeOrToken other)
	{
		if (IsNode != other.IsNode)
		{
			return false;
		}
		GreenNode underlyingNode = UnderlyingNode;
		GreenNode underlyingNode2 = other.UnderlyingNode;
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

	public Location GetLocation()
	{
		if (_token == null)
		{
			return _nodeOrParent?.GetLocation();
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

	private static void GetDirectives<TDirective>(in SyntaxNodeOrToken node, Func<TDirective, bool> filter, ref List<TDirective> directives) where TDirective : SyntaxNode
	{
		if (node._token != null)
		{
			GetDirectives(node.AsToken(), filter, ref directives);
		}
		else if (node._nodeOrParent != null)
		{
			GetDirectives(node._nodeOrParent, filter, ref directives);
		}
	}

	private static void GetDirectives<TDirective>(SyntaxNode node, Func<TDirective, bool> filter, ref List<TDirective> directives) where TDirective : SyntaxNode
	{
		if (node.ContainsDirectives)
		{
			foreach (SyntaxNodeOrToken item in node.ChildNodesAndTokens())
			{
				GetDirectives(item, filter, ref directives);
			}
		}
	}

	private static void GetDirectives<TDirective>(SyntaxToken token, Func<TDirective, bool> filter, ref List<TDirective> directives) where TDirective : SyntaxNode
	{
		if (token.ContainsDirectives)
		{
			GetDirectives(token.LeadingTrivia, filter, ref directives);
			GetDirectives(token.TrailingTrivia, filter, ref directives);
		}
	}

	private static void GetDirectives<TDirective>(in SyntaxTriviaList trivia, Func<TDirective, bool> filter, ref List<TDirective> directives) where TDirective : SyntaxNode
	{
		foreach (SyntaxTrivia trivium in trivia)
		{
			if (trivium.IsDirective)
			{
				if (trivium.GetStructure() is TDirective val && (filter == null || filter(val)))
				{
					if (directives == null)
					{
						directives = new List<TDirective>();
					}
					directives.Add(val);
				}
			}
			else if (trivium.HasStructure)
			{
				GetDirectives(trivium.GetStructure(), filter, ref directives);
			}
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
		throw ExceptionUtilities.Unreachable;
	}

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
		int firstChildIndexSpanningPosition = GetFirstChildIndexSpanningPosition(siblings, _position);
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
