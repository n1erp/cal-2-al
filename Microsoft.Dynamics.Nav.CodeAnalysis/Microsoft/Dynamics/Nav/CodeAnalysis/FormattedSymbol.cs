using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class FormattedSymbol
{
	private readonly ISymbol symbol;

	private readonly SymbolDisplayFormat symbolDisplayFormat;

	internal FormattedSymbol(ISymbol symbol, SymbolDisplayFormat symbolDisplayFormat)
	{
		DebugAssertHelper.Assert(symbol != null && symbolDisplayFormat != null);
		this.symbol = symbol;
		this.symbolDisplayFormat = symbolDisplayFormat;
	}

	public override string ToString()
	{
		return symbol.ToDisplayString(symbolDisplayFormat);
	}

	public override bool Equals(object obj)
	{
		if (obj is FormattedSymbol formattedSymbol && symbol.Equals(formattedSymbol.symbol))
		{
			return symbolDisplayFormat == formattedSymbol.symbolDisplayFormat;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(symbol.GetHashCode(), symbolDisplayFormat.GetHashCode());
	}
}
