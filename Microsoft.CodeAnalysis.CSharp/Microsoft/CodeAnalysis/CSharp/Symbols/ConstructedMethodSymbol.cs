using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ConstructedMethodSymbol : SubstitutedMethodSymbol
{
	private readonly ImmutableArray<TypeSymbol> _typeArguments;

	public override ImmutableArray<TypeSymbol> TypeArguments => _typeArguments;

	public override bool IsTupleMethod => ConstructedFrom.IsTupleMethod;

	public override MethodSymbol TupleUnderlyingMethod => ConstructedFrom.TupleUnderlyingMethod?.Construct(_typeArguments);

	internal ConstructedMethodSymbol(MethodSymbol constructedFrom, ImmutableArray<TypeSymbol> typeArguments)
		: base(constructedFrom.ContainingType, new TypeMap(constructedFrom.ContainingType, constructedFrom.OriginalDefinition.TypeParameters, typeArguments.SelectAsArray(TypeMap.TypeSymbolAsTypeWithModifiers)), constructedFrom.OriginalDefinition, constructedFrom)
	{
		_typeArguments = typeArguments;
	}
}
