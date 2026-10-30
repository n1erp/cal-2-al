namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class DynamicTypeEraser : AbstractTypeMap
{
	private readonly TypeSymbol _objectType;

	public DynamicTypeEraser(TypeSymbol objectType)
	{
		_objectType = objectType;
	}

	public TypeSymbol EraseDynamic(TypeSymbol type)
	{
		return SubstituteType(type).AsTypeSymbolOnly();
	}

	protected override TypeSymbol SubstituteDynamicType()
	{
		return _objectType;
	}
}
