using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public sealed class SyntaxAnnotation : IEquatable<SyntaxAnnotation>
{
	[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
	public static readonly SyntaxAnnotation ElasticAnnotation = new SyntaxAnnotation();

	private static long nextId;

	public AnnotationKind Kind { get; }

	public object Data { get; }

	internal long Id { get; }

	public SyntaxAnnotation()
	{
		Id = Interlocked.Increment(ref nextId);
	}

	public SyntaxAnnotation(AnnotationKind kind, object data)
		: this(kind)
	{
		Data = data;
	}

	public SyntaxAnnotation(AnnotationKind kind)
		: this()
	{
		Kind = kind;
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private string GetDebuggerDisplay()
	{
		return string.Format(CultureInfo.InvariantCulture, "Annotation: Kind='{0}' Data='{1}'", Kind.ToString(), Data ?? string.Empty);
	}

	public bool Equals(SyntaxAnnotation other)
	{
		if ((object)other != null)
		{
			return Id == other.Id;
		}
		return false;
	}

	public static bool operator ==(SyntaxAnnotation left, SyntaxAnnotation right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if ((object)left == null || (object)right == null)
		{
			return false;
		}
		return left.Equals(right);
	}

	public static bool operator !=(SyntaxAnnotation left, SyntaxAnnotation right)
	{
		if ((object)left == right)
		{
			return false;
		}
		if ((object)left == null || (object)right == null)
		{
			return true;
		}
		return !left.Equals(right);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SyntaxAnnotation);
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}
}
