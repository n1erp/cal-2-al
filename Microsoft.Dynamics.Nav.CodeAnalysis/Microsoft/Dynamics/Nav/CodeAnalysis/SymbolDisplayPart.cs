using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public struct SymbolDisplayPart
{
	private readonly string text;

	public SymbolDisplayPartKind Kind { get; }

	public ISymbol Symbol { get; }

	public SymbolDisplayPart(SymbolDisplayPartKind kind, ISymbol symbol, string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		Kind = kind;
		this.text = text;
		Symbol = symbol;
	}

	public override string ToString()
	{
		return text;
	}

	public override bool Equals(object obj)
	{
		if (obj is SymbolDisplayPart)
		{
			return Equals((SymbolDisplayPart)obj);
		}
		return false;
	}

	public bool Equals(SymbolDisplayPart other)
	{
		if (Kind == other.Kind && Symbol.Equals(other.Symbol))
		{
			return text == other.text;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.CombineValues(Kind, Symbol, text);
	}

	public static bool operator ==(SymbolDisplayPart left, SymbolDisplayPart right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SymbolDisplayPart left, SymbolDisplayPart right)
	{
		return !(left == right);
	}
}
