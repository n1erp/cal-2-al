using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceOverridingMethodTypeParameterSymbol : SourceTypeParameterSymbolBase
{
	private readonly OverriddenMethodTypeParameterMapBase _map;

	public SourceOrdinaryMethodSymbol Owner => _map.OverridingMethod;

	public override TypeParameterKind TypeParameterKind => TypeParameterKind.Method;

	public override Symbol ContainingSymbol => Owner;

	public override bool HasConstructorConstraint => OverriddenTypeParameter?.HasConstructorConstraint ?? false;

	public override bool HasValueTypeConstraint => OverriddenTypeParameter?.HasValueTypeConstraint ?? false;

	public override bool HasReferenceTypeConstraint => OverriddenTypeParameter?.HasReferenceTypeConstraint ?? false;

	public override bool HasUnmanagedTypeConstraint => OverriddenTypeParameter?.HasUnmanagedTypeConstraint ?? false;

	protected override ImmutableArray<TypeParameterSymbol> ContainerTypeParameters => Owner.TypeParameters;

	private TypeParameterSymbol OverriddenTypeParameter => _map.GetOverriddenTypeParameter(Ordinal);

	public SourceOverridingMethodTypeParameterSymbol(OverriddenMethodTypeParameterMapBase map, string name, int ordinal, ImmutableArray<Location> locations, ImmutableArray<SyntaxReference> syntaxRefs)
		: base(name, ordinal, locations, syntaxRefs)
	{
		_map = map;
	}

	protected override TypeParameterBounds ResolveBounds(ConsList<TypeParameterSymbol> inProgress, DiagnosticBag diagnostics)
	{
		TypeParameterSymbol overriddenTypeParameter = OverriddenTypeParameter;
		if ((object)overriddenTypeParameter == null)
		{
			return null;
		}
		ImmutableArray<TypeSymbol> constraintTypes = _map.TypeMap.SubstituteTypesWithoutModifiers(overriddenTypeParameter.ConstraintTypesNoUseSiteDiagnostics);
		return this.ResolveBounds(ContainingAssembly.CorLibrary, inProgress.Prepend(this), constraintTypes, inherited: true, DeclaringCompilation, diagnostics);
	}
}
