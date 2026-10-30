using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ConstructedErrorTypeSymbol : SubstitutedErrorTypeSymbol
{
	private readonly ErrorTypeSymbol _constructedFrom;

	private readonly ImmutableArray<TypeSymbol> _typeArguments;

	private readonly bool _hasTypeArgumentsCustomModifiers;

	private readonly TypeMap _map;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => _constructedFrom.TypeParameters;

	internal override ImmutableArray<TypeSymbol> TypeArgumentsNoUseSiteDiagnostics => _typeArguments;

	internal override bool HasTypeArgumentsCustomModifiers => _hasTypeArgumentsCustomModifiers;

	public override NamedTypeSymbol ConstructedFrom => _constructedFrom;

	public override Symbol ContainingSymbol => _constructedFrom.ContainingSymbol;

	internal override TypeMap TypeSubstitution => _map;

	public ConstructedErrorTypeSymbol(ErrorTypeSymbol constructedFrom, ImmutableArray<TypeWithModifiers> typeArguments)
		: base((ErrorTypeSymbol)constructedFrom.OriginalDefinition)
	{
		_constructedFrom = constructedFrom;
		_typeArguments = typeArguments.ToTypes(out _hasTypeArgumentsCustomModifiers);
		_map = new TypeMap(constructedFrom.ContainingType, constructedFrom.OriginalDefinition.TypeParameters, typeArguments);
	}

	public override ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal)
	{
		if (_hasTypeArgumentsCustomModifiers)
		{
			return _map.GetTypeArgumentsCustomModifiersFor(_constructedFrom.OriginalDefinition.TypeParameters[ordinal]);
		}
		return GetEmptyTypeArgumentCustomModifiers(ordinal);
	}
}
