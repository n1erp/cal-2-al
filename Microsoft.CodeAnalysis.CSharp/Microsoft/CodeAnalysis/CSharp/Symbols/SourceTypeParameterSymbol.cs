using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceTypeParameterSymbol : SourceTypeParameterSymbolBase
{
	private readonly SourceNamedTypeSymbol _owner;

	private readonly VarianceKind _varianceKind;

	public override TypeParameterKind TypeParameterKind => TypeParameterKind.Type;

	public override Symbol ContainingSymbol => _owner;

	public override VarianceKind Variance => _varianceKind;

	public override bool HasConstructorConstraint => (GetDeclaredConstraints() & TypeParameterConstraintKind.Constructor) != 0;

	public override bool HasValueTypeConstraint => (GetDeclaredConstraints() & (TypeParameterConstraintKind.ValueType | TypeParameterConstraintKind.Unmanaged)) != 0;

	public override bool HasReferenceTypeConstraint => (GetDeclaredConstraints() & TypeParameterConstraintKind.ReferenceType) != 0;

	public override bool HasUnmanagedTypeConstraint => (GetDeclaredConstraints() & TypeParameterConstraintKind.Unmanaged) != 0;

	protected override ImmutableArray<TypeParameterSymbol> ContainerTypeParameters => _owner.TypeParameters;

	public SourceTypeParameterSymbol(SourceNamedTypeSymbol owner, string name, int ordinal, VarianceKind varianceKind, ImmutableArray<Location> locations, ImmutableArray<SyntaxReference> syntaxRefs)
		: base(name, ordinal, locations, syntaxRefs)
	{
		_owner = owner;
		_varianceKind = varianceKind;
	}

	protected override TypeParameterBounds ResolveBounds(ConsList<TypeParameterSymbol> inProgress, DiagnosticBag diagnostics)
	{
		ImmutableArray<TypeSymbol> typeParameterConstraintTypes = _owner.GetTypeParameterConstraintTypes(Ordinal);
		return this.ResolveBounds(ContainingAssembly.CorLibrary, inProgress.Prepend(this), typeParameterConstraintTypes, inherited: false, DeclaringCompilation, diagnostics);
	}

	private TypeParameterConstraintKind GetDeclaredConstraints()
	{
		return _owner.GetTypeParameterConstraints(Ordinal);
	}
}
