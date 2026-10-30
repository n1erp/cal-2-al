using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

internal sealed class DebugDocumentsBuilder
{
	private readonly ConcurrentDictionary<string, DebugSourceDocument> _debugDocuments;

	private readonly ConcurrentCache<(string, string), string> _normalizedPathsCache;

	private readonly SourceReferenceResolver _resolverOpt;

	private ImmutableArray<DebugSourceDocument> _embeddedDocuments;

	internal ImmutableArray<DebugSourceDocument> EmbeddedDocuments
	{
		get
		{
			return _embeddedDocuments;
		}
		set
		{
			_embeddedDocuments = value;
		}
	}

	internal int DebugDocumentCount => _debugDocuments.Count;

	public DebugDocumentsBuilder(SourceReferenceResolver resolverOpt, bool isDocumentNameCaseSensitive)
	{
		_resolverOpt = resolverOpt;
		_debugDocuments = new ConcurrentDictionary<string, DebugSourceDocument>(isDocumentNameCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
		_normalizedPathsCache = new ConcurrentCache<(string, string), string>(16);
		_embeddedDocuments = ImmutableArray<DebugSourceDocument>.Empty;
	}

	internal void AddDebugDocument(DebugSourceDocument document)
	{
		_debugDocuments.Add(document.Location, document);
	}

	internal DebugSourceDocument TryGetDebugDocument(string path, string basePath)
	{
		return TryGetDebugDocumentForNormalizedPath(NormalizeDebugDocumentPath(path, basePath));
	}

	internal DebugSourceDocument TryGetDebugDocumentForNormalizedPath(string normalizedPath)
	{
		_debugDocuments.TryGetValue(normalizedPath, out var value);
		return value;
	}

	internal DebugSourceDocument GetOrAddDebugDocument(string path, string basePath, Func<string, DebugSourceDocument> factory)
	{
		return _debugDocuments.GetOrAdd(NormalizeDebugDocumentPath(path, basePath), factory);
	}

	internal string NormalizeDebugDocumentPath(string path, string basePath)
	{
		if (_resolverOpt == null)
		{
			return path;
		}
		(string, string) key = (path, basePath);
		if (!_normalizedPathsCache.TryGetValue(key, out var value))
		{
			value = _resolverOpt.NormalizePath(path, basePath) ?? path;
			_normalizedPathsCache.TryAdd(key, value);
		}
		return value;
	}
}
