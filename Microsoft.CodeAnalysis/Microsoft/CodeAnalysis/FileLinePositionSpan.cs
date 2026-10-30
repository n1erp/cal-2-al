using System;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public struct FileLinePositionSpan : IEquatable<FileLinePositionSpan>
{
	private readonly string _path;

	private readonly LinePositionSpan _span;

	private readonly bool _hasMappedPath;

	public string Path => _path;

	public bool HasMappedPath => _hasMappedPath;

	public LinePosition StartLinePosition => _span.Start;

	public LinePosition EndLinePosition => _span.End;

	public LinePositionSpan Span => _span;

	public bool IsValid => _path != null;

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
		_path = path;
		_span = span;
		_hasMappedPath = false;
	}

	internal FileLinePositionSpan(string path, LinePositionSpan span, bool hasMappedPath)
	{
		_path = path;
		_span = span;
		_hasMappedPath = hasMappedPath;
	}

	public bool Equals(FileLinePositionSpan other)
	{
		if (_span.Equals(other._span) && _hasMappedPath == other._hasMappedPath)
		{
			return string.Equals(_path, other._path, StringComparison.Ordinal);
		}
		return false;
	}

	public override bool Equals(object other)
	{
		if (other is FileLinePositionSpan)
		{
			return Equals((FileLinePositionSpan)other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(_path, Hash.Combine(_hasMappedPath, _span.GetHashCode()));
	}

	public override string ToString()
	{
		return _path + ": " + _span;
	}
}
