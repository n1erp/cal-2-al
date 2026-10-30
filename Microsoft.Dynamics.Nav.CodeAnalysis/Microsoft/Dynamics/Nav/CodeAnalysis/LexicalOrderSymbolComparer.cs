using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class LexicalOrderSymbolComparer : IComparer<Symbol>
{
	public static readonly LexicalOrderSymbolComparer Instance = new LexicalOrderSymbolComparer();

	private LexicalOrderSymbolComparer()
	{
	}

	public int Compare(Symbol left, Symbol right)
	{
		if (left == right)
		{
			return 0;
		}
		LexicalSortKey lexicalSortKey = left.GetLexicalSortKey();
		LexicalSortKey lexicalSortKey2 = right.GetLexicalSortKey();
		int num = LexicalSortKey.Compare(lexicalSortKey, lexicalSortKey2);
		if (num != 0)
		{
			return num;
		}
		num = ((ISymbol)left).Kind.ToSortOrder() - ((ISymbol)right).Kind.ToSortOrder();
		if (num != 0)
		{
			return num;
		}
		num = string.CompareOrdinal(left.Name, right.Name);
		DebugAssertHelper.Assert(num != 0);
		return num;
	}
}
