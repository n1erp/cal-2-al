namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ContextualAttributeBinder : Binder
{
	private readonly Symbol _attributeTarget;

	private readonly Symbol _attributedMember;

	internal Symbol AttributedMember => _attributedMember;

	internal Symbol AttributeTarget => _attributeTarget;

	public ContextualAttributeBinder(Binder enclosing, Symbol symbol)
		: base(enclosing)
	{
		_attributeTarget = symbol;
		_attributedMember = GetAttributedMember(symbol);
	}

	private static Symbol GetAttributedMember(Symbol symbol)
	{
		while ((object)symbol != null)
		{
			SymbolKind kind = symbol.Kind;
			if (kind == SymbolKind.Event || kind == SymbolKind.Method || kind == SymbolKind.Property)
			{
				return symbol;
			}
			symbol = symbol.ContainingSymbol;
		}
		return symbol;
	}
}
