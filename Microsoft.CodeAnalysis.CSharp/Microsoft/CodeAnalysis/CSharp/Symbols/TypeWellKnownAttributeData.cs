namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TypeWellKnownAttributeData : CommonTypeWellKnownAttributeData
{
	private NamedTypeSymbol _comImportCoClass;

	public NamedTypeSymbol ComImportCoClass
	{
		get
		{
			return _comImportCoClass;
		}
		set
		{
			_comImportCoClass = value;
		}
	}
}
