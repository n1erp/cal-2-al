using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public class SourceFileResolver : SourceReferenceResolver, IEquatable<SourceFileResolver>
{
	private readonly string _baseDirectory;

	private readonly ImmutableArray<string> _searchPaths;

	private readonly ImmutableArray<KeyValuePair<string, string>> _pathMap;

	public static SourceFileResolver Default { get; } = new SourceFileResolver(ImmutableArray<string>.Empty, null);

	public string BaseDirectory => _baseDirectory;

	public ImmutableArray<string> SearchPaths => _searchPaths;

	public ImmutableArray<KeyValuePair<string, string>> PathMap => _pathMap;

	public SourceFileResolver(IEnumerable<string> searchPaths, string baseDirectory)
		: this(searchPaths.AsImmutableOrNull(), baseDirectory)
	{
	}

	public SourceFileResolver(ImmutableArray<string> searchPaths, string baseDirectory)
		: this(searchPaths, baseDirectory, ImmutableArray<KeyValuePair<string, string>>.Empty)
	{
	}

	public SourceFileResolver(ImmutableArray<string> searchPaths, string baseDirectory, ImmutableArray<KeyValuePair<string, string>> pathMap)
	{
		if (searchPaths.IsDefault)
		{
			throw new ArgumentNullException("searchPaths");
		}
		if (baseDirectory != null && PathUtilities.GetPathKind(baseDirectory) != PathKind.Absolute)
		{
			throw new ArgumentException(CodeAnalysisResources.AbsolutePathExpected, "baseDirectory");
		}
		_baseDirectory = baseDirectory;
		_searchPaths = searchPaths;
		if (!pathMap.IsDefaultOrEmpty)
		{
			ArrayBuilder<KeyValuePair<string, string>> instance = ArrayBuilder<KeyValuePair<string, string>>.GetInstance(pathMap.Length);
			foreach (KeyValuePair<string, string> item in pathMap)
			{
				string key = item.Key;
				if (key == null || key.Length == 0)
				{
					throw new ArgumentException(CodeAnalysisResources.EmptyKeyInPathMap, "pathMap");
				}
				string s = item.Value ?? throw new ArgumentException(CodeAnalysisResources.NullValueInPathMap, "pathMap");
				string key2 = PathUtilities.EnsureTrailingSeparator(key);
				string value = PathUtilities.EnsureTrailingSeparator(s);
				instance.Add(new KeyValuePair<string, string>(key2, value));
			}
			_pathMap = instance.ToImmutableAndFree();
		}
		else
		{
			_pathMap = ImmutableArray<KeyValuePair<string, string>>.Empty;
		}
	}

	public override string NormalizePath(string path, string baseFilePath)
	{
		string text = FileUtilities.NormalizeRelativePath(path, baseFilePath, _baseDirectory);
		if (text != null && !_pathMap.IsDefaultOrEmpty)
		{
			return PathUtilities.NormalizePathPrefix(text, _pathMap);
		}
		return text;
	}

	public override string ResolveReference(string path, string baseFilePath)
	{
		string text = FileUtilities.ResolveRelativePath(path, baseFilePath, _baseDirectory, _searchPaths, FileExists);
		if (text == null)
		{
			return null;
		}
		return FileUtilities.TryNormalizeAbsolutePath(text);
	}

	public override Stream OpenRead(string resolvedPath)
	{
		CompilerPathUtilities.RequireAbsolutePath(resolvedPath, "resolvedPath");
		return FileUtilities.OpenRead(resolvedPath);
	}

	protected virtual bool FileExists(string resolvedPath)
	{
		return File.Exists(resolvedPath);
	}

	public override bool Equals(object obj)
	{
		if (obj == null || (object)GetType() != obj.GetType())
		{
			return false;
		}
		return Equals((SourceFileResolver)obj);
	}

	public bool Equals(SourceFileResolver other)
	{
		if (string.Equals(_baseDirectory, other._baseDirectory, StringComparison.Ordinal) && _searchPaths.SequenceEqual(other._searchPaths, StringComparer.Ordinal))
		{
			return _pathMap.SequenceEqual(other._pathMap);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine((_baseDirectory != null) ? StringComparer.Ordinal.GetHashCode(_baseDirectory) : 0, Hash.Combine(Hash.CombineValues(_searchPaths, StringComparer.Ordinal), Hash.CombineValues(_pathMap)));
	}
}
