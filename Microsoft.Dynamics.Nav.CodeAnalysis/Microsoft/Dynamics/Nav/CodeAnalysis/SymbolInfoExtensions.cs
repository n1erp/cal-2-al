using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class SymbolInfoExtensions
{
	public static IEnumerable<ISymbol> GetAllSymbols(this SymbolInfo info)
	{
		if (info.Symbol != null || info.CandidateSymbols.Length != 0)
		{
			return info.GetAllSymbolsWorker().Distinct();
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}

	private static IEnumerable<ISymbol> GetAllSymbolsWorker(this SymbolInfo info)
	{
		if (info.Symbol != null)
		{
			yield return info.Symbol;
		}
		foreach (ISymbol candidateSymbol in info.CandidateSymbols)
		{
			yield return candidateSymbol;
		}
	}

	public static ISymbol GetAnySymbol(this SymbolInfo info)
	{
		return info.GetAllSymbols().FirstOrDefault();
	}

	public static IEnumerable<ISymbol> GetBestOrAllSymbols(this SymbolInfo info)
	{
		if (info.Symbol != null)
		{
			return SpecializedCollections.SingletonEnumerable(info.Symbol);
		}
		if (info.CandidateSymbols.Length > 0)
		{
			return info.CandidateSymbols;
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}
}
