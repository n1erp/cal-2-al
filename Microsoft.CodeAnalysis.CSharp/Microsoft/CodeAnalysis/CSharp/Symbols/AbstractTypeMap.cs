using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class AbstractTypeMap
{
	private NamedTypeSymbol SubstituteMemberType(NamedTypeSymbol previous)
	{
		NamedTypeSymbol namedTypeSymbol = SubstituteNamedType(previous.ContainingType);
		if ((object)namedTypeSymbol == null)
		{
			return previous;
		}
		return previous.OriginalDefinition.AsMember(namedTypeSymbol);
	}

	internal NamedTypeSymbol SubstituteNamedType(NamedTypeSymbol previous)
	{
		if ((object)previous == null)
		{
			return null;
		}
		if (previous.IsUnboundGenericType)
		{
			return previous;
		}
		if (previous.IsAnonymousType)
		{
			ImmutableArray<TypeSymbol> anonymousTypePropertyTypes = AnonymousTypeManager.GetAnonymousTypePropertyTypes(previous);
			ImmutableArray<TypeSymbol> immutableArray = SubstituteTypesWithoutModifiers(anonymousTypePropertyTypes);
			if (!(anonymousTypePropertyTypes == immutableArray))
			{
				return AnonymousTypeManager.ConstructAnonymousTypeSymbol(previous, immutableArray);
			}
			return previous;
		}
		if (previous.IsTupleType)
		{
			TupleTypeSymbol tupleTypeSymbol = (TupleTypeSymbol)previous;
			NamedTypeSymbol tupleUnderlyingType = tupleTypeSymbol.TupleUnderlyingType;
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)SubstituteType(tupleUnderlyingType).Type;
			if ((object)namedTypeSymbol != tupleUnderlyingType)
			{
				return tupleTypeSymbol.WithUnderlyingType(namedTypeSymbol);
			}
			return previous;
		}
		NamedTypeSymbol constructedFrom = previous.ConstructedFrom;
		NamedTypeSymbol namedTypeSymbol2 = SubstituteMemberType(constructedFrom);
		ImmutableArray<TypeSymbol> typeArgumentsNoUseSiteDiagnostics = previous.TypeArgumentsNoUseSiteDiagnostics;
		bool flag = (object)constructedFrom != namedTypeSymbol2;
		bool hasTypeArgumentsCustomModifiers = previous.HasTypeArgumentsCustomModifiers;
		ArrayBuilder<TypeWithModifiers> instance = ArrayBuilder<TypeWithModifiers>.GetInstance(typeArgumentsNoUseSiteDiagnostics.Length);
		for (int i = 0; i < typeArgumentsNoUseSiteDiagnostics.Length; i++)
		{
			TypeWithModifiers typeWithModifiers = (hasTypeArgumentsCustomModifiers ? new TypeWithModifiers(typeArgumentsNoUseSiteDiagnostics[i], previous.GetTypeArgumentCustomModifiers(i)) : new TypeWithModifiers(typeArgumentsNoUseSiteDiagnostics[i]));
			TypeWithModifiers typeWithModifiers2 = typeWithModifiers.SubstituteTypeWithTupleUnification(this);
			if (!flag && typeWithModifiers != typeWithModifiers2)
			{
				flag = true;
			}
			instance.Add(typeWithModifiers2);
		}
		if (!flag)
		{
			instance.Free();
			return previous;
		}
		return namedTypeSymbol2.ConstructIfGeneric(instance.ToImmutableAndFree());
	}

	internal TypeWithModifiers SubstituteType(TypeSymbol previous)
	{
		if ((object)previous == null)
		{
			return default;
		}
		TypeSymbol type;
		switch (previous.Kind)
		{
		case SymbolKind.NamedType:
			type = SubstituteNamedType((NamedTypeSymbol)previous);
			break;
		case SymbolKind.TypeParameter:
			return SubstituteTypeParameter((TypeParameterSymbol)previous);
		case SymbolKind.ArrayType:
			type = SubstituteArrayType((ArrayTypeSymbol)previous);
			break;
		case SymbolKind.PointerType:
			type = SubstitutePointerType((PointerTypeSymbol)previous);
			break;
		case SymbolKind.DynamicType:
			type = SubstituteDynamicType();
			break;
		case SymbolKind.ErrorType:
			return ((ErrorTypeSymbol)previous).Substitute(this);
		default:
			type = previous;
			break;
		}
		return new TypeWithModifiers(type);
	}

	internal TypeWithModifiers SubstituteTypeWithTupleUnification(TypeSymbol previous)
	{
		TypeWithModifiers result = SubstituteType(previous);
		if ((object)result.Type != null && !previous.IsTupleCompatible())
		{
			TypeSymbol typeSymbol = TupleTypeSymbol.TransformToTupleIfCompatible(result.Type);
			if ((object)result.Type != typeSymbol)
			{
				result = new TypeWithModifiers(typeSymbol, result.CustomModifiers);
			}
		}
		return result;
	}

	internal ImmutableArray<CustomModifier> SubstituteCustomModifiers(TypeSymbol type, ImmutableArray<CustomModifier> customModifiers)
	{
		if (type.IsTypeParameter())
		{
			return new TypeWithModifiers(type, customModifiers).SubstituteType(this).CustomModifiers;
		}
		return SubstituteCustomModifiers(customModifiers);
	}

	internal ImmutableArray<CustomModifier> SubstituteCustomModifiers(ImmutableArray<CustomModifier> customModifiers)
	{
		if (customModifiers.IsDefaultOrEmpty)
		{
			return customModifiers;
		}
		for (int i = 0; i < customModifiers.Length; i++)
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)customModifiers[i].Modifier;
			NamedTypeSymbol namedTypeSymbol2 = SubstituteNamedType(namedTypeSymbol);
			if (!(namedTypeSymbol != namedTypeSymbol2))
			{
				continue;
			}
			ArrayBuilder<CustomModifier> instance = ArrayBuilder<CustomModifier>.GetInstance(customModifiers.Length);
			instance.AddRange(customModifiers, i);
			instance.Add(customModifiers[i].IsOptional ? CSharpCustomModifier.CreateOptional(namedTypeSymbol2) : CSharpCustomModifier.CreateRequired(namedTypeSymbol2));
			for (i++; i < customModifiers.Length; i++)
			{
				namedTypeSymbol = (NamedTypeSymbol)customModifiers[i].Modifier;
				namedTypeSymbol2 = SubstituteNamedType(namedTypeSymbol);
				if (namedTypeSymbol != namedTypeSymbol2)
				{
					instance.Add(customModifiers[i].IsOptional ? CSharpCustomModifier.CreateOptional(namedTypeSymbol2) : CSharpCustomModifier.CreateRequired(namedTypeSymbol2));
				}
				else
				{
					instance.Add(customModifiers[i]);
				}
			}
			return instance.ToImmutableAndFree();
		}
		return customModifiers;
	}

	protected virtual TypeSymbol SubstituteDynamicType()
	{
		return DynamicTypeSymbol.Instance;
	}

	protected virtual TypeWithModifiers SubstituteTypeParameter(TypeParameterSymbol typeParameter)
	{
		return new TypeWithModifiers(typeParameter);
	}

	private ArrayTypeSymbol SubstituteArrayType(ArrayTypeSymbol t)
	{
		TypeWithModifiers typeWithModifiers = new TypeWithModifiers(t.ElementType, t.CustomModifiers);
		TypeWithModifiers typeWithModifiers2 = typeWithModifiers.SubstituteTypeWithTupleUnification(this);
		if (typeWithModifiers2 == typeWithModifiers)
		{
			return t;
		}
		if (t.IsSZArray)
		{
			ImmutableArray<NamedTypeSymbol> constructedInterfaces = t.InterfacesNoUseSiteDiagnostics();
			if (constructedInterfaces.Length == 1)
			{
				constructedInterfaces = ImmutableArray.Create((NamedTypeSymbol)SubstituteType(constructedInterfaces[0]).AsTypeSymbolOnly());
			}
			else if (constructedInterfaces.Length == 2)
			{
				constructedInterfaces = ImmutableArray.Create((NamedTypeSymbol)SubstituteType(constructedInterfaces[0]).AsTypeSymbolOnly(), (NamedTypeSymbol)SubstituteType(constructedInterfaces[1]).AsTypeSymbolOnly());
			}
			else if (constructedInterfaces.Length != 0)
			{
				throw ExceptionUtilities.Unreachable;
			}
			return ArrayTypeSymbol.CreateSZArray(typeWithModifiers2.Type, t.BaseTypeNoUseSiteDiagnostics, constructedInterfaces, typeWithModifiers2.CustomModifiers);
		}
		return ArrayTypeSymbol.CreateMDArray(typeWithModifiers2.Type, t.Rank, t.Sizes, t.LowerBounds, t.BaseTypeNoUseSiteDiagnostics, typeWithModifiers2.CustomModifiers);
	}

	private PointerTypeSymbol SubstitutePointerType(PointerTypeSymbol t)
	{
		TypeWithModifiers typeWithModifiers = new TypeWithModifiers(t.PointedAtType, t.CustomModifiers);
		TypeWithModifiers typeWithModifiers2 = typeWithModifiers.SubstituteTypeWithTupleUnification(this);
		if (typeWithModifiers2 == typeWithModifiers)
		{
			return t;
		}
		return new PointerTypeSymbol(typeWithModifiers2.Type, typeWithModifiers2.CustomModifiers);
	}

	internal ImmutableArray<TypeSymbol> SubstituteTypesWithoutModifiers(ImmutableArray<TypeSymbol> original)
	{
		if (original.IsDefault)
		{
			return original;
		}
		TypeSymbol[] array = null;
		for (int i = 0; i < original.Length; i++)
		{
			TypeSymbol typeSymbol = original[i];
			TypeSymbol type = SubstituteType(typeSymbol).Type;
			if ((object)type != typeSymbol && array == null)
			{
				array = new TypeSymbol[original.Length];
				for (int j = 0; j < i; j++)
				{
					array[j] = original[j];
				}
			}
			if (array != null)
			{
				array[i] = type;
			}
		}
		return array?.AsImmutableOrNull() ?? original;
	}

	internal ImmutableArray<TypeWithModifiers> SubstituteTypes(ImmutableArray<TypeSymbol> original)
	{
		if (original.IsDefault)
		{
			return default;
		}
		ArrayBuilder<TypeWithModifiers> instance = ArrayBuilder<TypeWithModifiers>.GetInstance(original.Length);
		foreach (TypeSymbol item in original)
		{
			instance.Add(SubstituteType(item));
		}
		return instance.ToImmutableAndFree();
	}

	internal void SubstituteTypesDistinctWithoutModifiers(ImmutableArray<TypeSymbol> original, ArrayBuilder<TypeSymbol> result, HashSet<TypeParameterSymbol> ignoreTypesDependentOnTypeParametersOpt)
	{
		if (original.Length == 0)
		{
			return;
		}
		if (original.Length == 1)
		{
			TypeSymbol typeSymbol = original[0];
			if (ignoreTypesDependentOnTypeParametersOpt == null || !typeSymbol.ContainsTypeParameters(ignoreTypesDependentOnTypeParametersOpt))
			{
				result.Add(SubstituteType(typeSymbol).Type);
			}
			return;
		}
		HashSet<TypeSymbol> hashSet = new HashSet<TypeSymbol>();
		foreach (TypeSymbol item in original)
		{
			if (ignoreTypesDependentOnTypeParametersOpt == null || !item.ContainsTypeParameters(ignoreTypesDependentOnTypeParametersOpt))
			{
				TypeSymbol type = SubstituteType(item).Type;
				if (hashSet.Add(type))
				{
					result.Add(type);
				}
			}
		}
	}

	internal ImmutableArray<TypeParameterSymbol> SubstituteTypeParameters(ImmutableArray<TypeParameterSymbol> original)
	{
		return original.SelectAsArray((TypeParameterSymbol tp, AbstractTypeMap m) => (TypeParameterSymbol)m.SubstituteTypeParameter(tp).AsTypeSymbolOnly(), this);
	}

	internal ImmutableArray<NamedTypeSymbol> SubstituteNamedTypes(ImmutableArray<NamedTypeSymbol> original)
	{
		NamedTypeSymbol[] array = null;
		for (int i = 0; i < original.Length; i++)
		{
			NamedTypeSymbol namedTypeSymbol = original[i];
			NamedTypeSymbol namedTypeSymbol2 = SubstituteNamedType(namedTypeSymbol);
			if ((object)namedTypeSymbol2 != namedTypeSymbol && array == null)
			{
				array = new NamedTypeSymbol[original.Length];
				for (int j = 0; j < i; j++)
				{
					array[j] = original[j];
				}
			}
			if (array != null)
			{
				array[i] = namedTypeSymbol2;
			}
		}
		return array?.AsImmutableOrNull() ?? original;
	}
}
