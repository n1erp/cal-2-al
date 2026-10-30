using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class CrefTypeParameterSymbol : TypeParameterSymbol
{
	private readonly string _name;

	private readonly int _ordinal;

	private readonly SyntaxReference _declaringSyntax;

	public override TypeParameterKind TypeParameterKind => TypeParameterKind.Cref;

	public override string Name => _name;

	public override int Ordinal => _ordinal;

	public override VarianceKind Variance => VarianceKind.None;

	public override bool HasValueTypeConstraint => false;

	public override bool HasReferenceTypeConstraint => false;

	public override bool HasUnmanagedTypeConstraint => false;

	public override bool HasConstructorConstraint => false;

	public override Symbol ContainingSymbol => null;

	public override ImmutableArray<Location> Locations => ImmutableArray.Create(_declaringSyntax.GetLocation());

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(_declaringSyntax);

	public override bool IsImplicitlyDeclared => false;

	public CrefTypeParameterSymbol(string name, int ordinal, IdentifierNameSyntax declaringSyntax)
	{
		_name = name;
		_ordinal = ordinal;
		_declaringSyntax = declaringSyntax.GetReference();
	}

	internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
	{
		if ((object)this == t2)
		{
			return true;
		}
		if ((object)t2 == null)
		{
			return false;
		}
		if (t2 is CrefTypeParameterSymbol crefTypeParameterSymbol && crefTypeParameterSymbol._name == _name && crefTypeParameterSymbol._ordinal == _ordinal)
		{
			return crefTypeParameterSymbol._declaringSyntax.GetSyntax() == _declaringSyntax.GetSyntax();
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(_name, _ordinal);
	}

	internal override void EnsureAllConstraintsAreResolved()
	{
	}

	internal override ImmutableArray<TypeSymbol> GetConstraintTypes(ConsList<TypeParameterSymbol> inProgress)
	{
		return ImmutableArray<TypeSymbol>.Empty;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfaces(ConsList<TypeParameterSymbol> inProgress)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override NamedTypeSymbol GetEffectiveBaseClass(ConsList<TypeParameterSymbol> inProgress)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override TypeSymbol GetDeducedBaseType(ConsList<TypeParameterSymbol> inProgress)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
