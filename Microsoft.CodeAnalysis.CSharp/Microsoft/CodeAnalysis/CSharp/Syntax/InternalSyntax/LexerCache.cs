using System;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class LexerCache
{
	private static readonly ObjectPool<CachingIdentityFactory<string, SyntaxKind>> s_keywordKindPool = CachingIdentityFactory<string, SyntaxKind>.CreatePool(512, (string key) =>
	{
		SyntaxKind syntaxKind = SyntaxFacts.GetKeywordKind(key);
		if (syntaxKind == SyntaxKind.None)
		{
			syntaxKind = SyntaxFacts.GetContextualKeywordKind(key);
		}
		return syntaxKind;
	});

	private readonly TextKeyedCache<SyntaxTrivia> _triviaMap;

	private readonly TextKeyedCache<SyntaxToken> _tokenMap;

	private readonly CachingIdentityFactory<string, SyntaxKind> _keywordKindMap;

	internal const int MaxKeywordLength = 10;

	internal LexerCache()
	{
		_triviaMap = TextKeyedCache<SyntaxTrivia>.GetInstance();
		_tokenMap = TextKeyedCache<SyntaxToken>.GetInstance();
		_keywordKindMap = s_keywordKindPool.Allocate();
	}

	internal void Free()
	{
		_keywordKindMap.Free();
		_triviaMap.Free();
		_tokenMap.Free();
	}

	internal bool TryGetKeywordKind(string key, out SyntaxKind kind)
	{
		if (key.Length > 10)
		{
			kind = SyntaxKind.None;
			return false;
		}
		kind = _keywordKindMap.GetOrMakeValue(key);
		return kind != SyntaxKind.None;
	}

	internal SyntaxTrivia LookupTrivia(char[] textBuffer, int keyStart, int keyLength, int hashCode, Func<SyntaxTrivia> createTriviaFunction)
	{
		SyntaxTrivia syntaxTrivia = _triviaMap.FindItem(textBuffer, keyStart, keyLength, hashCode);
		if (syntaxTrivia == null)
		{
			syntaxTrivia = createTriviaFunction();
			_triviaMap.AddItem(textBuffer, keyStart, keyLength, hashCode, syntaxTrivia);
		}
		return syntaxTrivia;
	}

	internal SyntaxToken LookupToken(char[] textBuffer, int keyStart, int keyLength, int hashCode, Func<SyntaxToken> createTokenFunction)
	{
		SyntaxToken syntaxToken = _tokenMap.FindItem(textBuffer, keyStart, keyLength, hashCode);
		if (syntaxToken == null)
		{
			syntaxToken = createTokenFunction();
			_tokenMap.AddItem(textBuffer, keyStart, keyLength, hashCode, syntaxToken);
		}
		return syntaxToken;
	}
}
