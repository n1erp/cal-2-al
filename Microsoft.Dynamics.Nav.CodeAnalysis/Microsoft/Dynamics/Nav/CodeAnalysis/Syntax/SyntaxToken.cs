using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public struct SyntaxToken : IEquatable<SyntaxToken>
{
	internal static readonly Func<SyntaxToken, bool> NonZeroWidth = (SyntaxToken t) => t.Width > 0;

	internal static readonly Func<SyntaxToken, bool> Any = (SyntaxToken t) => true;

	private readonly SyntaxNode parent;

	private readonly InternalSyntaxNode node;

	public bool ContainsAnnotations => Node?.ContainsAnnotations ?? false;

	public bool HasStructuredTrivia
	{
		get
		{
			if (Node != null)
			{
				return Node.ContainsStructuredTrivia;
			}
			return false;
		}
	}

	public SyntaxNode Parent => parent;

	public SyntaxKind Kind => Node?.Kind ?? SyntaxKind.None;

	internal InternalSyntaxNode Node => node;

	internal int Index { get; }

	internal int Position { get; }

	internal int Width
	{
		get
		{
			if (Node == null)
			{
				return 0;
			}
			return Node.Width;
		}
	}

	public bool IsMissing
	{
		get
		{
			if (Node != null)
			{
				return Node.IsMissing;
			}
			return false;
		}
	}

	internal int FullWidth
	{
		get
		{
			if (Node == null)
			{
				return 0;
			}
			return Node.FullWidth;
		}
	}

	public TextSpan Span
	{
		get
		{
			if (Node == null)
			{
				return default;
			}
			return new TextSpan(Position + Node.GetLeadingTriviaWidth(), Node.Width);
		}
	}

	public TextSpan FullSpan => new TextSpan(Position, FullWidth);

	internal int EndPosition
	{
		get
		{
			if (Node == null)
			{
				return 0;
			}
			return Position + Node.FullWidth;
		}
	}

	public int SpanStart
	{
		get
		{
			if (Node == null)
			{
				return 0;
			}
			return Position + Node.GetLeadingTriviaWidth();
		}
	}

	public int SpanEnd
	{
		get
		{
			if (Node == null)
			{
				return 0;
			}
			return Span.End;
		}
	}

	public object Value
	{
		get
		{
			if (Node == null)
			{
				return null;
			}
			return Node.GetValue();
		}
	}

	public string ValueText
	{
		get
		{
			if (Node == null)
			{
				return null;
			}
			return Node.GetValueText();
		}
	}

	public string Text => ToString();

	public bool ContainsDiagnostics
	{
		get
		{
			if (Node != null)
			{
				return Node.ContainsDiagnostics;
			}
			return false;
		}
	}

	public bool HasLeadingTrivia => LeadingTrivia.Count > 0;

	public bool HasTrailingTrivia => TrailingTrivia.Count > 0;

	internal int LeadingWidth
	{
		get
		{
			if (Node == null)
			{
				return 0;
			}
			return Node.GetLeadingTriviaWidth();
		}
	}

	internal int TrailingWidth
	{
		get
		{
			if (Node == null)
			{
				return 0;
			}
			return Node.GetTrailingTriviaWidth();
		}
	}

	public SyntaxTriviaList LeadingTrivia
	{
		get
		{
			if (Node == null)
			{
				return default;
			}
			return new SyntaxTriviaList(this, Node.GetLeadingTrivia(), Position);
		}
	}

	public SyntaxTriviaList TrailingTrivia
	{
		get
		{
			if (Node == null)
			{
				return default;
			}
			InternalSyntaxNode leadingTrivia = Node.GetLeadingTrivia();
			int index = 0;
			if (leadingTrivia != null)
			{
				index = ((!leadingTrivia.IsList) ? 1 : leadingTrivia.SlotCount);
			}
			InternalSyntaxNode trailingTrivia = Node.GetTrailingTrivia();
			int num = Position + FullWidth;
			if (trailingTrivia != null)
			{
				num -= trailingTrivia.FullWidth;
			}
			return new SyntaxTriviaList(this, trailingTrivia, num, index);
		}
	}

	public SyntaxTree SyntaxTree
	{
		get
		{
			if (Parent == null)
			{
				return null;
			}
			return Parent.SyntaxTree;
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private string GetDebuggerDisplay()
	{
		return GetType().Name + " " + ((Node != null) ? Node.KindText : "None") + " " + ToString();
	}

	internal SyntaxToken(SyntaxNode parent, InternalSyntaxNode token, int position, int index)
	{
		DebugAssertHelper.Assert(token?.IsToken ?? true, "token must be a token");
		this.parent = parent;
		node = token;
		Position = position;
		Index = index;
	}

	internal SyntaxToken(InternalSyntaxNode token)
	{
		DebugAssertHelper.Assert(token?.IsToken ?? true, "token must be a token");
		node = token;
		parent = null;
		Position = 0;
		Index = 0;
	}

	public bool IsPartOfStructuredTrivia()
	{
		if (Parent != null)
		{
			return Parent.IsPartOfStructuredTrivia();
		}
		return false;
	}

	public override string ToString()
	{
		if (Node == null)
		{
			return string.Empty;
		}
		return Node.ToString();
	}

	public string ToFullString()
	{
		if (Node == null)
		{
			return string.Empty;
		}
		return Node.ToFullString();
	}

	public void WriteTo(TextWriter writer)
	{
		if (Node != null)
		{
			Node.WriteTo(writer);
		}
	}

	internal void WriteTo(TextWriter writer, bool leading, bool trailing)
	{
		if (Node != null)
		{
			Node.WriteTo(writer, leading, trailing);
		}
	}

	public SyntaxToken WithTriviaFrom(SyntaxToken token)
	{
		return WithLeadingTrivia(token.LeadingTrivia).WithTrailingTrivia(token.TrailingTrivia);
	}

	public SyntaxToken WithLeadingTrivia(SyntaxTriviaList trivia)
	{
		return WithLeadingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public SyntaxToken WithLeadingTrivia(params SyntaxTrivia[] trivia)
	{
		return WithLeadingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public SyntaxToken WithLeadingTrivia(IEnumerable<SyntaxTrivia> trivia)
	{
		IEnumerable<InternalSyntaxNode> nodes = trivia?.Select((SyntaxTrivia t) => t.UnderlyingNode);
		if (Node == null)
		{
			return default;
		}
		return new SyntaxToken(null, Node.WithLeadingTriviaBase(Node.CreateList(nodes)), 0, 0);
	}

	public SyntaxToken WithTrailingTrivia(SyntaxTriviaList trivia)
	{
		return WithTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public SyntaxToken WithTrailingTrivia(params SyntaxTrivia[] trivia)
	{
		return WithTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public SyntaxToken WithTrailingTrivia(IEnumerable<SyntaxTrivia> trivia)
	{
		IEnumerable<InternalSyntaxNode> nodes = trivia?.Select((SyntaxTrivia t) => t.UnderlyingNode);
		if (Node == null)
		{
			return default;
		}
		return new SyntaxToken(null, Node.WithTrailingTrivia(Node.CreateList(nodes)), 0, 0);
	}

	public bool HasAnnotations(AnnotationKind annotationKind)
	{
		if (Node != null)
		{
			return Node.HasAnnotations(annotationKind);
		}
		return false;
	}

	public bool HasAnnotations(params AnnotationKind[] annotationKinds)
	{
		if (Node != null)
		{
			return Node.HasAnnotations(annotationKinds);
		}
		return false;
	}

	public bool HasAnnotation(SyntaxAnnotation annotation)
	{
		if (Node != null)
		{
			return Node.HasAnnotation(annotation);
		}
		return false;
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(AnnotationKind annotationKind)
	{
		if (Node == null)
		{
			return SpecializedCollections.EmptyEnumerable<SyntaxAnnotation>();
		}
		return Node.GetAnnotations(annotationKind);
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(params AnnotationKind[] annotationKinds)
	{
		return GetAnnotations((IEnumerable<AnnotationKind>)annotationKinds);
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(IEnumerable<AnnotationKind> annotationKinds)
	{
		if (Node == null)
		{
			return SpecializedCollections.EmptyEnumerable<SyntaxAnnotation>();
		}
		return Node.GetAnnotations(annotationKinds);
	}

	public SyntaxToken WithAdditionalAnnotations(params SyntaxAnnotation[] annotations)
	{
		return WithAdditionalAnnotations((IEnumerable<SyntaxAnnotation>)annotations);
	}

	public SyntaxToken WithAdditionalAnnotations(IEnumerable<SyntaxAnnotation> annotations)
	{
		if (annotations == null)
		{
			throw new ArgumentNullException("annotations");
		}
		if (Node != null)
		{
			return new SyntaxToken(null, Node.WithAdditionalAnnotationsGreen(annotations), 0, 0);
		}
		return default;
	}

	public SyntaxToken WithoutAnnotations(params SyntaxAnnotation[] annotations)
	{
		return WithoutAnnotations((IEnumerable<SyntaxAnnotation>)annotations);
	}

	public SyntaxToken WithoutAnnotations(IEnumerable<SyntaxAnnotation> annotations)
	{
		if (annotations == null)
		{
			throw new ArgumentNullException("annotations");
		}
		if (Node != null)
		{
			return new SyntaxToken(null, Node.WithoutAnnotationsGreen(annotations), 0, 0);
		}
		return default;
	}

	public SyntaxToken WithoutAnnotations(AnnotationKind annotationKind)
	{
		if (HasAnnotations(annotationKind))
		{
			return WithoutAnnotations(GetAnnotations(annotationKind));
		}
		return this;
	}

	public static bool operator ==(SyntaxToken left, SyntaxToken right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SyntaxToken left, SyntaxToken right)
	{
		return !left.Equals(right);
	}

	public bool Equals(SyntaxToken other)
	{
		if (Parent == other.Parent && Node == other.Node && Position == other.Position)
		{
			return Index == other.Index;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SyntaxToken)
		{
			return Equals((SyntaxToken)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Parent, Hash.Combine(Node, Hash.Combine(Position, Index)));
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public Location GetLocation()
	{
		if (Node == null)
		{
			return Location.None;
		}
		return SyntaxTree.GetLocation(Span);
	}

	public SyntaxToken GetNextToken(bool includeZeroWidth = false, bool includeSkipped = false)
	{
		if (Node == null)
		{
			return default;
		}
		return SyntaxNavigator.Instance.GetNextToken(this, includeZeroWidth, includeSkipped);
	}

	internal SyntaxToken GetNextToken(Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto = null)
	{
		if (Node == null)
		{
			return default;
		}
		return SyntaxNavigator.Instance.GetNextToken(this, predicate, stepInto);
	}

	public SyntaxToken GetPreviousToken(bool includeZeroWidth = false, bool includeSkipped = false)
	{
		if (Node == null)
		{
			return default;
		}
		return SyntaxNavigator.Instance.GetPreviousToken(this, includeZeroWidth, includeSkipped);
	}

	internal SyntaxToken GetPreviousToken(Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto = null)
	{
		return SyntaxNavigator.Instance.GetPreviousToken(this, predicate, stepInto);
	}
}
