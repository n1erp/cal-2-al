using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public struct SyntaxTrivia : IEquatable<SyntaxTrivia>
{
	internal static readonly Func<SyntaxTrivia, bool> Any = (SyntaxTrivia t) => true;

	private readonly SyntaxToken token;

	private readonly InternalSyntaxNode triviaNode;

	private readonly int position;

	private readonly int index;

	public SyntaxKind Kind => UnderlyingNode?.Kind ?? SyntaxKind.None;

	public SyntaxToken Token => token;

	internal InternalSyntaxNode UnderlyingNode => triviaNode;

	internal int Position => position;

	internal int Index => index;

	internal int Width
	{
		get
		{
			if (UnderlyingNode == null)
			{
				return 0;
			}
			return UnderlyingNode.Width;
		}
	}

	internal int FullWidth
	{
		get
		{
			if (UnderlyingNode == null)
			{
				return 0;
			}
			return UnderlyingNode.FullWidth;
		}
	}

	public TextSpan Span
	{
		get
		{
			if (UnderlyingNode == null)
			{
				return default;
			}
			return new TextSpan(Position + UnderlyingNode.GetLeadingTriviaWidth(), UnderlyingNode.Width);
		}
	}

	public int SpanStart
	{
		get
		{
			if (UnderlyingNode == null)
			{
				return 0;
			}
			return Position + UnderlyingNode.GetLeadingTriviaWidth();
		}
	}

	public TextSpan FullSpan
	{
		get
		{
			if (UnderlyingNode == null)
			{
				return default;
			}
			return new TextSpan(Position, UnderlyingNode.FullWidth);
		}
	}

	public bool ContainsDiagnostics
	{
		get
		{
			if (UnderlyingNode != null)
			{
				return UnderlyingNode.ContainsDiagnostics;
			}
			return false;
		}
	}

	public bool HasStructure
	{
		get
		{
			if (UnderlyingNode != null)
			{
				return UnderlyingNode.IsStructuredTrivia;
			}
			return false;
		}
	}

	public SyntaxTree SyntaxTree => Token.SyntaxTree;

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private string GetDebuggerDisplay()
	{
		return GetType().Name + " " + ((UnderlyingNode != null) ? UnderlyingNode.KindText : "None") + " " + ToString();
	}

	internal SyntaxTrivia(SyntaxToken token, InternalSyntaxNode triviaNode, int position, int index)
	{
		this.token = token;
		this.triviaNode = triviaNode;
		this.position = position;
		this.index = index;
	}

	public bool IsPartOfStructuredTrivia()
	{
		if (Token.Parent != null)
		{
			return Token.Parent.IsPartOfStructuredTrivia();
		}
		return false;
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public SyntaxNode GetStructure()
	{
		if (!HasStructure)
		{
			return null;
		}
		return InternalSyntaxNode.GetStructure(this);
	}

	public override string ToString()
	{
		if (UnderlyingNode == null)
		{
			return string.Empty;
		}
		return UnderlyingNode.ToString();
	}

	public string ToFullString()
	{
		if (UnderlyingNode == null)
		{
			return string.Empty;
		}
		return UnderlyingNode.ToFullString();
	}

	public void WriteTo(TextWriter writer)
	{
		if (UnderlyingNode != null)
		{
			UnderlyingNode.WriteTo(writer);
		}
	}

	public static bool operator ==(SyntaxTrivia left, SyntaxTrivia right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SyntaxTrivia left, SyntaxTrivia right)
	{
		return !left.Equals(right);
	}

	public bool Equals(SyntaxTrivia other)
	{
		if (Token == other.Token && UnderlyingNode == other.UnderlyingNode && Position == other.Position)
		{
			return Index == other.Index;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SyntaxTrivia)
		{
			return Equals((SyntaxTrivia)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Token.GetHashCode() ^ Position ^ Index ^ ((UnderlyingNode != null) ? UnderlyingNode.GetHashCode() : 0);
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public Location GetLocation()
	{
		return SyntaxTree.GetLocation(Span);
	}

	public bool HasAnnotations(params AnnotationKind[] annotationKinds)
	{
		return UnderlyingNode?.HasAnnotations(annotationKinds) ?? false;
	}

	public bool HasAnnotations(AnnotationKind annotationKind)
	{
		return UnderlyingNode?.HasAnnotations(annotationKind) ?? false;
	}

	public bool HasAnnotation(SyntaxAnnotation annotation)
	{
		return UnderlyingNode?.HasAnnotation(annotation) ?? false;
	}

	public IEnumerable<SyntaxAnnotation> GetAnnotations(AnnotationKind annotationKind)
	{
		if (UnderlyingNode == null)
		{
			return SpecializedCollections.EmptyEnumerable<SyntaxAnnotation>();
		}
		return UnderlyingNode.GetAnnotations(annotationKind);
	}

	public SyntaxTrivia WithAdditionalAnnotations(params SyntaxAnnotation[] annotations)
	{
		return WithAdditionalAnnotations((IEnumerable<SyntaxAnnotation>)annotations);
	}

	public SyntaxTrivia WithAdditionalAnnotations(IEnumerable<SyntaxAnnotation> annotations)
	{
		if (annotations == null)
		{
			throw new ArgumentNullException("annotations");
		}
		if (UnderlyingNode != null)
		{
			return new SyntaxTrivia(default, UnderlyingNode.WithAdditionalAnnotationsGreen(annotations), 0, 0);
		}
		return default;
	}

	public SyntaxTrivia WithoutAnnotations(params SyntaxAnnotation[] annotations)
	{
		return WithoutAnnotations((IEnumerable<SyntaxAnnotation>)annotations);
	}

	public SyntaxTrivia WithoutAnnotations(IEnumerable<SyntaxAnnotation> annotations)
	{
		if (annotations == null)
		{
			throw new ArgumentNullException("annotations");
		}
		if (UnderlyingNode != null)
		{
			return new SyntaxTrivia(default, UnderlyingNode.WithoutAnnotationsGreen(annotations), 0, 0);
		}
		return default;
	}

	public SyntaxTrivia WithoutAnnotations(AnnotationKind annotationKind)
	{
		if (HasAnnotations(annotationKind))
		{
			return WithoutAnnotations(GetAnnotations(annotationKind));
		}
		return this;
	}

	public SyntaxTrivia CopyAnnotationsTo(SyntaxTrivia trivia)
	{
		if (trivia.UnderlyingNode == null)
		{
			return default;
		}
		if (UnderlyingNode == null)
		{
			return trivia;
		}
		SyntaxAnnotation[] annotations = UnderlyingNode.GetAnnotations();
		if (annotations == null || annotations.Length == 0)
		{
			return trivia;
		}
		return new SyntaxTrivia(default, trivia.UnderlyingNode.WithAdditionalAnnotationsGreen(annotations), 0, 0);
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "By design")]
	public IEnumerable<Diagnostic> GetDiagnostics()
	{
		return SyntaxTree.GetDiagnostics(this);
	}

	public bool IsEquivalentTo(SyntaxTrivia trivia)
	{
		if (UnderlyingNode != null || trivia.UnderlyingNode != null)
		{
			if (UnderlyingNode != null && trivia.UnderlyingNode != null)
			{
				return UnderlyingNode.IsEquivalentTo(trivia.UnderlyingNode);
			}
			return false;
		}
		return true;
	}
}
