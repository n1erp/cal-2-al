using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SubstitutedNestedErrorTypeSymbol : SubstitutedErrorTypeSymbol
{
	private readonly NamedTypeSymbol _containingSymbol;

	private readonly ImmutableArray<TypeParameterSymbol> _typeParameters;

	private readonly TypeMap _map;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

	internal override ImmutableArray<TypeSymbol> TypeArgumentsNoUseSiteDiagnostics => TypeParameters.Cast<TypeParameterSymbol, TypeSymbol>();

	internal override bool HasTypeArgumentsCustomModifiers => false;

	public override NamedTypeSymbol ConstructedFrom => this;

	public override Symbol ContainingSymbol => _containingSymbol;

	internal override TypeMap TypeSubstitution => _map;

	public SubstitutedNestedErrorTypeSymbol(NamedTypeSymbol containingSymbol, ErrorTypeSymbol originalDefinition)
		: base(originalDefinition)
	{
		_containingSymbol = containingSymbol;
		_map = containingSymbol.TypeSubstitution.WithAlphaRename(originalDefinition, this, out _typeParameters);
	}

	public override ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal)
	{
		return GetEmptyTypeArgumentCustomModifiers(ordinal);
	}
}
