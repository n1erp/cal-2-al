using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal class LexicalOrderSymbolComparer : IComparer<Symbol>
{
	public static readonly LexicalOrderSymbolComparer Instance = new LexicalOrderSymbolComparer();

	private LexicalOrderSymbolComparer()
	{
	}

	public int Compare(Symbol x, Symbol y)
	{
		if (x == y)
		{
			return 0;
		}
		LexicalSortKey lexicalSortKey = x.GetLexicalSortKey();
		LexicalSortKey lexicalSortKey2 = y.GetLexicalSortKey();
		int num = LexicalSortKey.Compare(lexicalSortKey, lexicalSortKey2);
		if (num != 0)
		{
			return num;
		}
		num = ((ISymbol)x).Kind.ToSortOrder() - ((ISymbol)y).Kind.ToSortOrder();
		if (num != 0)
		{
			return num;
		}
		return string.CompareOrdinal(x.Name, y.Name);
	}
}
