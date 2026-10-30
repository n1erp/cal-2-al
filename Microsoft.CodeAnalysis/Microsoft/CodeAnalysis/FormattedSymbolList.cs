using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal sealed class FormattedSymbolList : IFormattable
{
	private readonly IEnumerable<ISymbol> _symbols;

	private readonly SymbolDisplayFormat _symbolDisplayFormat;

	internal FormattedSymbolList(IEnumerable<ISymbol> symbols, SymbolDisplayFormat symbolDisplayFormat = null)
	{
		_symbols = symbols;
		_symbolDisplayFormat = symbolDisplayFormat;
	}

	public override string ToString()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		bool flag = true;
		foreach (ISymbol symbol in _symbols)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				builder.Append(", ");
			}
			builder.Append(symbol.ToDisplayString(_symbolDisplayFormat));
		}
		return instance.ToStringAndFree();
	}

	string IFormattable.ToString(string format, IFormatProvider formatProvider)
	{
		return ToString();
	}
}
