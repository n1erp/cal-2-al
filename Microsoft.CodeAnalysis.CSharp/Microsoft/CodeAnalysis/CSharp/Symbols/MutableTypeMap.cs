namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class MutableTypeMap : AbstractTypeParameterMap
{
	internal MutableTypeMap()
		: base(new SmallDictionary<TypeParameterSymbol, TypeWithModifiers>())
	{
	}

	internal void Add(TypeParameterSymbol key, TypeWithModifiers value)
	{
		Mapping.Add(key, value);
	}
}
