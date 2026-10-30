using System;
using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public struct SymbolInfo : IEquatable<SymbolInfo>
{
	internal static readonly SymbolInfo None = new SymbolInfo(null, ImmutableArray<ISymbol>.Empty, CandidateReason.None);

	private ImmutableArray<ISymbol> _candidateSymbols;

	public ISymbol Symbol { get; }

	public ImmutableArray<ISymbol> CandidateSymbols => _candidateSymbols.NullToEmpty();

	public CandidateReason CandidateReason { get; }

	internal bool IsEmpty
	{
		get
		{
			if (Symbol == null)
			{
				return CandidateSymbols.Length == 0;
			}
			return false;
		}
	}

	internal ImmutableArray<ISymbol> GetAllSymbols()
	{
		if (Symbol != null)
		{
			return ImmutableArray.Create(Symbol);
		}
		return _candidateSymbols;
	}

	internal SymbolInfo(ISymbol symbol)
		: this(symbol, ImmutableArray<ISymbol>.Empty, CandidateReason.None)
	{
	}

	internal SymbolInfo(ISymbol symbol, CandidateReason reason)
		: this(symbol, ImmutableArray<ISymbol>.Empty, reason)
	{
	}

	internal SymbolInfo(ImmutableArray<ISymbol> candidateSymbols, CandidateReason candidateReason)
		: this(null, candidateSymbols, candidateReason)
	{
	}

	internal SymbolInfo(ISymbol symbol, ImmutableArray<ISymbol> candidateSymbols, CandidateReason candidateReason)
	{
		this = default;
		Symbol = symbol;
		_candidateSymbols = (candidateSymbols.IsDefault ? ImmutableArray.Create<ISymbol>() : candidateSymbols);
		CandidateReason = candidateReason;
	}

	public override bool Equals(object obj)
	{
		if (obj is SymbolInfo)
		{
			return Equals((SymbolInfo)obj);
		}
		return false;
	}

	public bool Equals(SymbolInfo other)
	{
		if (object.Equals(Symbol, other.Symbol) && ((_candidateSymbols.IsDefault && other._candidateSymbols.IsDefault) || _candidateSymbols.SequenceEqual(other._candidateSymbols)))
		{
			return CandidateReason == other.CandidateReason;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Symbol, Hash.Combine(Hash.CombineValues(_candidateSymbols, 4), (int)CandidateReason));
	}
}
