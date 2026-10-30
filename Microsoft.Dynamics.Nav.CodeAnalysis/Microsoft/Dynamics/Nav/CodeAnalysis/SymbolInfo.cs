using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public struct SymbolInfo : IEquatable<SymbolInfo>
{
	internal static readonly SymbolInfo None = new SymbolInfo(null, ImmutableArray<ISymbol>.Empty, CandidateReason.None);

	private readonly ImmutableArray<ISymbol> candidateSymbols;

	public ISymbol Symbol { get; }

	public ImmutableArray<ISymbol> CandidateSymbols => candidateSymbols.NullToEmpty();

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
		return candidateSymbols;
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
		this.candidateSymbols = (candidateSymbols.IsDefault ? ImmutableArray.Create<ISymbol>() : candidateSymbols);
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
		if (object.Equals(Symbol, other.Symbol) && ((candidateSymbols.IsDefault && other.candidateSymbols.IsDefault) || candidateSymbols.SequenceEqual(other.candidateSymbols)))
		{
			return CandidateReason == other.CandidateReason;
		}
		return false;
	}

	public static bool operator ==(SymbolInfo left, SymbolInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SymbolInfo left, SymbolInfo right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Symbol.GetHashCode(), Hash.Combine(Hash.CombineValues(candidateSymbols, 4), (int)CandidateReason));
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private string GetDebuggerDisplay()
	{
		if (IsEmpty)
		{
			return "(empty)";
		}
		if (Symbol != null)
		{
			return Symbol.ToDisplayString();
		}
		return "(multiple...)";
	}
}
