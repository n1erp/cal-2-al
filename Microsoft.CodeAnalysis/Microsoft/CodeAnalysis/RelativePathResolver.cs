using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal class RelativePathResolver : IEquatable<RelativePathResolver>
{
	public ImmutableArray<string> SearchPaths { get; }

	public string BaseDirectory { get; }

	public RelativePathResolver(ImmutableArray<string> searchPaths, string baseDirectory)
	{
		SearchPaths = searchPaths;
		BaseDirectory = baseDirectory;
	}

	public string ResolvePath(string reference, string baseFilePath)
	{
		string text = FileUtilities.ResolveRelativePath(reference, baseFilePath, BaseDirectory, SearchPaths, FileExists);
		if (text == null)
		{
			return null;
		}
		return FileUtilities.TryNormalizeAbsolutePath(text);
	}

	protected virtual bool FileExists(string fullPath)
	{
		return File.Exists(fullPath);
	}

	public RelativePathResolver WithSearchPaths(ImmutableArray<string> searchPaths)
	{
		return new RelativePathResolver(searchPaths, BaseDirectory);
	}

	public RelativePathResolver WithBaseDirectory(string baseDirectory)
	{
		return new RelativePathResolver(SearchPaths, baseDirectory);
	}

	public bool Equals(RelativePathResolver other)
	{
		if (BaseDirectory == other.BaseDirectory)
		{
			return SearchPaths.SequenceEqual(other.SearchPaths);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(BaseDirectory, Hash.CombineValues(SearchPaths));
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as RelativePathResolver);
	}
}
