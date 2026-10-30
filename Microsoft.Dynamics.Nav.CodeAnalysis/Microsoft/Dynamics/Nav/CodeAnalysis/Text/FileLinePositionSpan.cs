using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

public struct FileLinePositionSpan : IEquatable<FileLinePositionSpan>
{
	public string Path { get; }

	public bool HasMappedPath { get; }

	public LinePosition StartLinePosition => Span.Start;

	public LinePosition EndLinePosition => Span.End;

	public LinePositionSpan Span { get; }

	public bool IsValid => Path != null;

	public FileLinePositionSpan(string path, LinePosition start, LinePosition end)
		: this(path, new LinePositionSpan(start, end))
	{
	}

	public FileLinePositionSpan(string path, LinePositionSpan span)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		Path = path;
		Span = span;
		HasMappedPath = false;
	}

	internal FileLinePositionSpan(string path, LinePositionSpan span, bool hasMappedPath)
	{
		Path = path;
		Span = span;
		HasMappedPath = hasMappedPath;
	}

	public bool Equals(FileLinePositionSpan other)
	{
		if (Span.Equals(other.Span) && HasMappedPath == other.HasMappedPath)
		{
			return string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is FileLinePositionSpan)
		{
			return Equals((FileLinePositionSpan)obj);
		}
		return false;
	}

	public static bool operator ==(FileLinePositionSpan left, FileLinePositionSpan right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(FileLinePositionSpan left, FileLinePositionSpan right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Path.GetHashCode(), Hash.Combine(HasMappedPath.GetHashCode(), Span.GetHashCode()));
	}

	public override string ToString()
	{
		return Path + ": " + Span;
	}
}
