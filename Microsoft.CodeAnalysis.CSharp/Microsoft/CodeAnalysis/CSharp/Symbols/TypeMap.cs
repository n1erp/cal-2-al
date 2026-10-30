using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TypeMap : AbstractTypeParameterMap
{
	public static readonly Func<TypeSymbol, TypeWithModifiers> TypeSymbolAsTypeWithModifiers = (TypeSymbol t) => new TypeWithModifiers(t);

	private static readonly SmallDictionary<TypeParameterSymbol, TypeWithModifiers> s_emptyDictionary = new SmallDictionary<TypeParameterSymbol, TypeWithModifiers>(ReferenceEqualityComparer.Instance);

	private static readonly TypeMap s_emptyTypeMap = new TypeMap();

	public static TypeMap Empty => s_emptyTypeMap;

	internal TypeMap(ImmutableArray<TypeParameterSymbol> from, ImmutableArray<TypeWithModifiers> to, bool allowAlpha = false)
		: base(ConstructMapping(from, to))
	{
	}

	internal TypeMap(ImmutableArray<TypeParameterSymbol> from, ImmutableArray<TypeParameterSymbol> to, bool allowAlpha = false)
		: this(from, to.SelectAsArray(TypeSymbolAsTypeWithModifiers), allowAlpha)
	{
	}

	private TypeMap(SmallDictionary<TypeParameterSymbol, TypeWithModifiers> mapping)
		: base(new SmallDictionary<TypeParameterSymbol, TypeWithModifiers>(mapping, ReferenceEqualityComparer.Instance))
	{
	}

	private static SmallDictionary<TypeParameterSymbol, TypeWithModifiers> ForType(NamedTypeSymbol containingType)
	{
		if (!(containingType is SubstitutedNamedTypeSymbol substitutedNamedTypeSymbol))
		{
			return new SmallDictionary<TypeParameterSymbol, TypeWithModifiers>(ReferenceEqualityComparer.Instance);
		}
		return new SmallDictionary<TypeParameterSymbol, TypeWithModifiers>(substitutedNamedTypeSymbol.TypeSubstitution.Mapping, ReferenceEqualityComparer.Instance);
	}

	internal TypeMap(NamedTypeSymbol containingType, ImmutableArray<TypeParameterSymbol> typeParameters, ImmutableArray<TypeWithModifiers> typeArguments)
		: base(ForType(containingType))
	{
		for (int i = 0; i < typeParameters.Length; i++)
		{
			TypeParameterSymbol typeParameterSymbol = typeParameters[i];
			TypeWithModifiers value = typeArguments[i];
			if (!value.Is(typeParameterSymbol))
			{
				Mapping.Add(typeParameterSymbol, value);
			}
		}
	}

	private TypeMap()
		: base(s_emptyDictionary)
	{
	}

	private TypeMap WithAlphaRename(ImmutableArray<TypeParameterSymbol> oldTypeParameters, Symbol newOwner, out ImmutableArray<TypeParameterSymbol> newTypeParameters)
	{
		if (oldTypeParameters.Length == 0)
		{
			newTypeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
			return this;
		}
		TypeMap typeMap = new TypeMap(Mapping);
		ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance();
		bool flag = (object)oldTypeParameters[0].ContainingSymbol.OriginalDefinition != newOwner.OriginalDefinition;
		int num = 0;
		foreach (TypeParameterSymbol item in oldTypeParameters)
		{
			SubstitutedTypeParameterSymbol substitutedTypeParameterSymbol = (flag ? new SynthesizedSubstitutedTypeParameterSymbol(newOwner, typeMap, item, num) : new SubstitutedTypeParameterSymbol(newOwner, typeMap, item, num));
			typeMap.Mapping.Add(item, new TypeWithModifiers(substitutedTypeParameterSymbol));
			instance.Add(substitutedTypeParameterSymbol);
			num++;
		}
		newTypeParameters = instance.ToImmutableAndFree();
		return typeMap;
	}

	internal TypeMap WithAlphaRename(NamedTypeSymbol oldOwner, NamedTypeSymbol newOwner, out ImmutableArray<TypeParameterSymbol> newTypeParameters)
	{
		return WithAlphaRename(oldOwner.OriginalDefinition.TypeParameters, newOwner, out newTypeParameters);
	}

	internal TypeMap WithAlphaRename(MethodSymbol oldOwner, Symbol newOwner, out ImmutableArray<TypeParameterSymbol> newTypeParameters)
	{
		return WithAlphaRename(oldOwner.OriginalDefinition.TypeParameters, newOwner, out newTypeParameters);
	}

	internal TypeMap WithConcatAlphaRename(MethodSymbol oldOwner, Symbol newOwner, out ImmutableArray<TypeParameterSymbol> newTypeParameters, out ImmutableArray<TypeParameterSymbol> oldTypeParameters, MethodSymbol stopAt = null)
	{
		ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance();
		while (oldOwner != null && oldOwner != stopAt)
		{
			ImmutableArray<TypeParameterSymbol> typeParameters = oldOwner.OriginalDefinition.TypeParameters;
			for (int num = typeParameters.Length - 1; num >= 0; num--)
			{
				instance.Add(typeParameters[num]);
			}
			oldOwner = oldOwner.ContainingSymbol.OriginalDefinition as MethodSymbol;
		}
		instance.ReverseContents();
		oldTypeParameters = instance.ToImmutableAndFree();
		return WithAlphaRename(oldTypeParameters, newOwner, out newTypeParameters);
	}

	private static SmallDictionary<TypeParameterSymbol, TypeWithModifiers> ConstructMapping(ImmutableArray<TypeParameterSymbol> from, ImmutableArray<TypeWithModifiers> to)
	{
		SmallDictionary<TypeParameterSymbol, TypeWithModifiers> smallDictionary = new SmallDictionary<TypeParameterSymbol, TypeWithModifiers>(ReferenceEqualityComparer.Instance);
		for (int i = 0; i < from.Length; i++)
		{
			TypeParameterSymbol typeParameterSymbol = from[i];
			TypeWithModifiers value = to[i];
			if (!value.Is(typeParameterSymbol))
			{
				smallDictionary.Add(typeParameterSymbol, value);
			}
		}
		return smallDictionary;
	}

	public ImmutableArray<CustomModifier> GetTypeArgumentsCustomModifiersFor(TypeParameterSymbol originalDefinition)
	{
		return SubstituteTypeParameter(originalDefinition).CustomModifiers;
	}
}
