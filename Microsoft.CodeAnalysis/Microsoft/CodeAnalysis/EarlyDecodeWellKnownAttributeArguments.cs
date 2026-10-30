namespace Microsoft.CodeAnalysis;

internal struct EarlyDecodeWellKnownAttributeArguments<TEarlyBinder, TNamedTypeSymbol, TAttributeSyntax, TAttributeLocation> where TNamedTypeSymbol : INamedTypeSymbol where TAttributeSyntax : SyntaxNode
{
	private EarlyWellKnownAttributeData _lazyDecodeData;

	public bool HasDecodedData
	{
		get
		{
			if (_lazyDecodeData != null)
			{
				return true;
			}
			return false;
		}
	}

	public EarlyWellKnownAttributeData DecodedData => _lazyDecodeData;

	public TEarlyBinder Binder { get; set; }

	public TNamedTypeSymbol AttributeType { get; set; }

	public TAttributeSyntax AttributeSyntax { get; set; }

	public TAttributeLocation SymbolPart { get; set; }

	public T GetOrCreateData<T>() where T : EarlyWellKnownAttributeData, new()
	{
		if (_lazyDecodeData == null)
		{
			_lazyDecodeData = new T();
		}
		return (T)_lazyDecodeData;
	}
}
