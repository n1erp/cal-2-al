using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SubstitutedParameterSymbol : WrappedParameterSymbol
{
	private object _mapOrType;

	private readonly Symbol _containingSymbol;

	public override ParameterSymbol OriginalDefinition => _underlyingParameter.OriginalDefinition;

	public override Symbol ContainingSymbol => _containingSymbol;

	public override TypeSymbol Type
	{
		get
		{
			object mapOrType = _mapOrType;
			TypeSymbol typeSymbol = mapOrType as TypeSymbol;
			if (typeSymbol != null)
			{
				return typeSymbol;
			}
			TypeWithModifiers typeWithModifiers = ((TypeMap)mapOrType).SubstituteTypeWithTupleUnification(_underlyingParameter.Type);
			typeSymbol = typeWithModifiers.Type;
			if (typeWithModifiers.CustomModifiers.IsEmpty && _underlyingParameter.CustomModifiers.IsEmpty && _underlyingParameter.RefCustomModifiers.IsEmpty)
			{
				_mapOrType = typeSymbol;
			}
			return typeSymbol;
		}
	}

	public override ImmutableArray<CustomModifier> CustomModifiers
	{
		get
		{
			if (!(_mapOrType is TypeMap typeMap))
			{
				return _underlyingParameter.CustomModifiers;
			}
			return typeMap.SubstituteCustomModifiers(_underlyingParameter.Type, _underlyingParameter.CustomModifiers);
		}
	}

	public override ImmutableArray<CustomModifier> RefCustomModifiers
	{
		get
		{
			if (!(_mapOrType is TypeMap typeMap))
			{
				return _underlyingParameter.RefCustomModifiers;
			}
			return typeMap.SubstituteCustomModifiers(_underlyingParameter.RefCustomModifiers);
		}
	}

	internal SubstitutedParameterSymbol(MethodSymbol containingSymbol, TypeMap map, ParameterSymbol originalParameter)
		: this((Symbol)containingSymbol, map, originalParameter)
	{
	}

	internal SubstitutedParameterSymbol(PropertySymbol containingSymbol, TypeMap map, ParameterSymbol originalParameter)
		: this((Symbol)containingSymbol, map, originalParameter)
	{
	}

	private SubstitutedParameterSymbol(Symbol containingSymbol, TypeMap map, ParameterSymbol originalParameter)
		: base(originalParameter)
	{
		_containingSymbol = containingSymbol;
		_mapOrType = map;
	}

	public sealed override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj is SubstitutedParameterSymbol substitutedParameterSymbol && Ordinal == substitutedParameterSymbol.Ordinal)
		{
			return ContainingSymbol.Equals(substitutedParameterSymbol.ContainingSymbol);
		}
		return false;
	}

	public sealed override int GetHashCode()
	{
		return Hash.Combine(ContainingSymbol, _underlyingParameter.Ordinal);
	}
}
