using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceMethodTypeParameterSymbol : SourceTypeParameterSymbolBase
{
	private readonly SourceMethodSymbol _owner;

	public override TypeParameterKind TypeParameterKind => TypeParameterKind.Method;

	public override Symbol ContainingSymbol => _owner;

	public override bool HasConstructorConstraint => (GetDeclaredConstraints() & TypeParameterConstraintKind.Constructor) != 0;

	public override bool HasValueTypeConstraint => (GetDeclaredConstraints() & (TypeParameterConstraintKind.ValueType | TypeParameterConstraintKind.Unmanaged)) != 0;

	public override bool HasReferenceTypeConstraint => (GetDeclaredConstraints() & TypeParameterConstraintKind.ReferenceType) != 0;

	public override bool HasUnmanagedTypeConstraint => (GetDeclaredConstraints() & TypeParameterConstraintKind.Unmanaged) != 0;

	protected override ImmutableArray<TypeParameterSymbol> ContainerTypeParameters => _owner.TypeParameters;

	public SourceMethodTypeParameterSymbol(SourceMethodSymbol owner, string name, int ordinal, ImmutableArray<Location> locations, ImmutableArray<SyntaxReference> syntaxRefs)
		: base(name, ordinal, locations, syntaxRefs)
	{
		_owner = owner;
	}

	internal override void AddDeclarationDiagnostics(DiagnosticBag diagnostics)
	{
		_owner.AddDeclarationDiagnostics(diagnostics);
	}

	protected override TypeParameterBounds ResolveBounds(ConsList<TypeParameterSymbol> inProgress, DiagnosticBag diagnostics)
	{
		ImmutableArray<TypeParameterConstraintClause> typeParameterConstraintClauses = _owner.TypeParameterConstraintClauses;
		ImmutableArray<TypeSymbol> constraintTypes = (typeParameterConstraintClauses.IsEmpty ? ImmutableArray<TypeSymbol>.Empty : (typeParameterConstraintClauses[Ordinal]?.ConstraintTypes ?? ImmutableArray<TypeSymbol>.Empty));
		return this.ResolveBounds(ContainingAssembly.CorLibrary, inProgress.Prepend(this), constraintTypes, inherited: false, DeclaringCompilation, diagnostics);
	}

	private TypeParameterConstraintKind GetDeclaredConstraints()
	{
		ImmutableArray<TypeParameterConstraintClause> typeParameterConstraintClauses = _owner.TypeParameterConstraintClauses;
		if (!typeParameterConstraintClauses.IsEmpty)
		{
			return typeParameterConstraintClauses[Ordinal]?.Constraints ?? TypeParameterConstraintKind.None;
		}
		return TypeParameterConstraintKind.None;
	}
}
