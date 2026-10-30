using System;
using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal struct TypeWithModifiers : IEquatable<TypeWithModifiers>
{
	public readonly TypeSymbol Type;

	public readonly ImmutableArray<CustomModifier> CustomModifiers;

	public TypeWithModifiers(TypeSymbol type, ImmutableArray<CustomModifier> customModifiers)
	{
		Type = type;
		CustomModifiers = customModifiers.NullToEmpty();
	}

	public TypeWithModifiers(TypeSymbol type)
	{
		Type = type;
		CustomModifiers = ImmutableArray<CustomModifier>.Empty;
	}

	[Obsolete("Use the strongly typed overload.", true)]
	public override bool Equals(object obj)
	{
		if (obj is TypeWithModifiers)
		{
			return Equals((TypeWithModifiers)obj);
		}
		return false;
	}

	public bool Equals(TypeWithModifiers other)
	{
		return Equals(other, TypeCompareKind.ConsiderEverything);
	}

	public bool Equals(TypeWithModifiers other, TypeCompareKind comparison)
	{
		if (((object)Type == null) ? ((object)other.Type == null) : Type.Equals(other.Type, comparison))
		{
			if (!CustomModifiers.IsDefault)
			{
				if (!other.CustomModifiers.IsDefault)
				{
					return CustomModifiers.SequenceEqual(other.CustomModifiers);
				}
				return false;
			}
			return other.CustomModifiers.IsDefault;
		}
		return false;
	}

	public static bool operator ==(TypeWithModifiers x, TypeWithModifiers y)
	{
		return x.Equals(y);
	}

	public static bool operator !=(TypeWithModifiers x, TypeWithModifiers y)
	{
		return !x.Equals(y);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Type, Hash.CombineValues(CustomModifiers));
	}

	public bool Is(TypeSymbol other)
	{
		if (Type == other)
		{
			return CustomModifiers.IsEmpty;
		}
		return false;
	}

	[Obsolete("Use Is method.", true)]
	public bool Equals(TypeSymbol other)
	{
		return Is(other);
	}

	public TypeSymbol AsTypeSymbolOnly()
	{
		return Type;
	}

	public TypeWithModifiers SubstituteType(AbstractTypeMap typeMap)
	{
		ImmutableArray<CustomModifier> immutableArray = typeMap.SubstituteCustomModifiers(CustomModifiers);
		TypeWithModifiers typeWithModifiers = typeMap.SubstituteType(Type);
		if (!typeWithModifiers.Is(Type) || immutableArray != CustomModifiers)
		{
			return new TypeWithModifiers(typeWithModifiers.Type, immutableArray.Concat(typeWithModifiers.CustomModifiers));
		}
		return this;
	}

	public TypeWithModifiers SubstituteTypeWithTupleUnification(AbstractTypeMap typeMap)
	{
		ImmutableArray<CustomModifier> immutableArray = typeMap.SubstituteCustomModifiers(CustomModifiers);
		TypeWithModifiers typeWithModifiers = typeMap.SubstituteTypeWithTupleUnification(Type);
		if (!typeWithModifiers.Is(Type) || immutableArray != CustomModifiers)
		{
			return new TypeWithModifiers(typeWithModifiers.Type, immutableArray.Concat(typeWithModifiers.CustomModifiers));
		}
		return this;
	}
}
