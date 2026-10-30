using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class IndexedTypeParameterSymbol : TypeParameterSymbol
{
	private static TypeParameterSymbol[] s_parameterPool = Array.Empty<TypeParameterSymbol>();

	private readonly int _index;

	public override TypeParameterKind TypeParameterKind => TypeParameterKind.Method;

	public override int Ordinal => _index;

	public override VarianceKind Variance => VarianceKind.None;

	public override bool HasValueTypeConstraint => false;

	public override bool HasReferenceTypeConstraint => false;

	public override bool HasUnmanagedTypeConstraint => false;

	public override bool HasConstructorConstraint => false;

	public override Symbol ContainingSymbol => null;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool IsImplicitlyDeclared => true;

	private IndexedTypeParameterSymbol(int index)
	{
		_index = index;
	}

	internal static TypeParameterSymbol GetTypeParameter(int index)
	{
		if (index >= s_parameterPool.Length)
		{
			GrowPool(index + 1);
		}
		return s_parameterPool[index];
	}

	private static void GrowPool(int count)
	{
		TypeParameterSymbol[] array = s_parameterPool;
		while (count > array.Length)
		{
			TypeParameterSymbol[] array2 = new TypeParameterSymbol[(count + 15) & -16];
			Array.Copy(array, array2, array.Length);
			for (int i = array.Length; i < array2.Length; i++)
			{
				array2[i] = new IndexedTypeParameterSymbol(i);
			}
			Interlocked.CompareExchange(ref s_parameterPool, array2, array);
			array = s_parameterPool;
		}
	}

	internal static ImmutableArray<TypeParameterSymbol> Take(int count)
	{
		if (count > s_parameterPool.Length)
		{
			GrowPool(count);
		}
		ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance();
		for (int i = 0; i < count; i++)
		{
			instance.Add(GetTypeParameter(i));
		}
		return instance.ToImmutableAndFree();
	}

	internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
	{
		return (object)this == t2;
	}

	public override int GetHashCode()
	{
		return _index;
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
		return null;
	}

	internal override TypeSymbol GetDeducedBaseType(ConsList<TypeParameterSymbol> inProgress)
	{
		return null;
	}
}
