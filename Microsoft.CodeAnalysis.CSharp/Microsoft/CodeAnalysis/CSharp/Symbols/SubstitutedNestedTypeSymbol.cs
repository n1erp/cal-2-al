using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SubstitutedNestedTypeSymbol : SubstitutedNamedTypeSymbol
{
	internal override ImmutableArray<TypeSymbol> TypeArgumentsNoUseSiteDiagnostics => TypeParameters.Cast<TypeParameterSymbol, TypeSymbol>();

	internal override bool HasTypeArgumentsCustomModifiers => false;

	public override NamedTypeSymbol ConstructedFrom => this;

	internal SubstitutedNestedTypeSymbol(SubstitutedNamedTypeSymbol newContainer, NamedTypeSymbol originalDefinition)
		: base(newContainer, newContainer.TypeSubstitution, originalDefinition, null, newContainer.IsUnboundGenericType && originalDefinition.Arity == 0)
	{
	}

	public override ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal)
	{
		return GetEmptyTypeArgumentCustomModifiers(ordinal);
	}
}
