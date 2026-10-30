using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class SymbolInfoFactory
{
	internal static SymbolInfo Create(ImmutableArray<Symbol> symbols, LookupResultKind resultKind, bool isDynamic = false)
	{
		if (isDynamic)
		{
			if (symbols.Length == 1)
			{
				return new SymbolInfo(symbols[0], CandidateReason.LateBound);
			}
			return new SymbolInfo(StaticCast<ISymbol>.From(symbols), CandidateReason.LateBound);
		}
		if (resultKind == LookupResultKind.Viable)
		{
			if (symbols.Length > 0)
			{
				DebugAssertHelper.Assert(symbols.Length == 1);
				return new SymbolInfo(symbols[0]);
			}
			return SymbolInfo.None;
		}
		return new SymbolInfo(StaticCast<ISymbol>.From(symbols), (symbols.Length > 0) ? resultKind.ToCandidateReason() : CandidateReason.None);
	}
}
