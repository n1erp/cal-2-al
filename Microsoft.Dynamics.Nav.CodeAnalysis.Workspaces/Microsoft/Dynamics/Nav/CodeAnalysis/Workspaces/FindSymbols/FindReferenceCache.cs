using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal static class FindReferenceCache
{
	private class Entry
	{
		public readonly ConcurrentDictionary<string, ImmutableArray<SyntaxToken>> IdentifierCache = new ConcurrentDictionary<string, ImmutableArray<SyntaxToken>>();

		public readonly ConcurrentDictionary<SyntaxNode, SymbolInfo> SymbolInfoCache = new ConcurrentDictionary<SyntaxNode, SymbolInfo>();

		public int Count;
	}

	private static readonly ReaderWriterLockSlim s_gate = new ReaderWriterLockSlim();

	private static readonly Dictionary<SemanticModel, Entry> s_cache = new Dictionary<SemanticModel, Entry>();

	private static readonly Func<SemanticModel, Entry> s_entryCreator = (SemanticModel _) => new Entry();

	public static SymbolInfo GetSymbolInfo(SemanticModel model, SyntaxNode node, CancellationToken cancellationToken)
	{
		return GetNodeCache(model)?.GetOrAdd(node, (SyntaxNode n) => model.GetSymbolInfo(n, cancellationToken)) ?? model.GetSymbolInfo(node, cancellationToken);
	}

	[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
	public static ImmutableArray<SyntaxToken> GetIdentifierTokensWithText(ISyntaxFactsService syntaxFacts, SemanticModel model, SyntaxNode root, SourceText sourceText, string text, CancellationToken cancellationToken)
	{
		string text2 = (syntaxFacts.IsCaseSensitive ? text : text.ToLowerInvariant());
		return GetCachedEntry(model)?.IdentifierCache.GetOrAdd(text2, (string key) => GetIdentifierTokensWithText(syntaxFacts, root, sourceText, key, cancellationToken)) ?? GetIdentifierTokensWithText(syntaxFacts, root, sourceText, text2, cancellationToken);
	}

	public static void Start(SemanticModel model)
	{
		using (s_gate.DisposableWrite())
		{
			s_cache.GetOrAdd(model, s_entryCreator).Count++;
		}
	}

	public static void Stop(SemanticModel model)
	{
		if (model == null)
		{
			return;
		}
		using (s_gate.DisposableWrite())
		{
			if (s_cache.TryGetValue(model, out var value))
			{
				value.Count--;
				if (value.Count == 0)
				{
					s_cache.Remove(model);
				}
			}
		}
	}

	private static ImmutableArray<SyntaxToken> GetIdentifierTokensWithText(ISyntaxFactsService syntaxFacts, SyntaxNode root, SourceText sourceText, string text, CancellationToken cancellationToken)
	{
		Func<SyntaxToken, bool> func = (SyntaxToken t) => syntaxFacts.IsIdentifier(t) && SemanticFacts.IsSameName(t.ValueText.UnquoteIdentifier(), text);
		if (sourceText != null)
		{
			return GetTokensFromText(syntaxFacts, root, sourceText, text, func, cancellationToken);
		}
		return root.DescendantTokens(null, descendIntoTrivia: true).Where(func).ToImmutableArray();
	}

	private static ImmutableArray<SyntaxToken> GetTokensFromText(ISyntaxFactsService syntaxFacts, SyntaxNode root, SourceText content, string text, Func<SyntaxToken, bool> candidate, CancellationToken cancellationToken)
	{
		if (text.Length == 0)
		{
			return ImmutableArray<SyntaxToken>.Empty;
		}
		ImmutableArray<SyntaxToken>.Builder builder = ImmutableArray.CreateBuilder<SyntaxToken>();
		int startIndex = 0;
		while ((startIndex = content.IndexOf(text, startIndex, syntaxFacts.IsCaseSensitive)) >= 0)
		{
			cancellationToken.ThrowIfCancellationRequested();
			int val = startIndex + text.Length;
			SyntaxToken syntaxToken = root.FindToken(startIndex, findInsideTrivia: true);
			TextSpan span = syntaxToken.Span;
			if (!syntaxToken.IsMissing && span.Start == startIndex && span.Length == text.Length && candidate(syntaxToken))
			{
				builder.Add(syntaxToken);
			}
			startIndex = Math.Max(val, syntaxToken.SpanStart);
		}
		return builder.ToImmutable();
	}

	private static ConcurrentDictionary<SyntaxNode, SymbolInfo> GetNodeCache(SemanticModel model)
	{
		return GetCachedEntry(model)?.SymbolInfoCache;
	}

	private static Entry GetCachedEntry(SemanticModel model)
	{
		using (s_gate.DisposableRead())
		{
			if (s_cache.TryGetValue(model, out var value))
			{
				return value;
			}
			return null;
		}
	}
}
