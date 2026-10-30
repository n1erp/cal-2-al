using System;

namespace Microsoft.CodeAnalysis;

public struct SymbolDisplayPart
{
	private readonly SymbolDisplayPartKind _kind;

	private readonly string _text;

	private readonly ISymbol _symbol;

	public SymbolDisplayPartKind Kind => _kind;

	public ISymbol Symbol => _symbol;

	public SymbolDisplayPart(SymbolDisplayPartKind kind, ISymbol symbol, string text)
	{
		if (!kind.IsValid())
		{
			throw new ArgumentOutOfRangeException("kind");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		_kind = kind;
		_text = text;
		_symbol = symbol;
	}

	public override string ToString()
	{
		return _text;
	}
}
