using System;
using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class SubstitutedTypeParameterSymbol : WrappedTypeParameterSymbol
{
	private readonly Symbol _container;

	private readonly TypeMap _map;

	private readonly int _ordinal;

	private static readonly Func<TypeSymbol, bool> s_isNotObjectFunc = (TypeSymbol type) => type.SpecialType != SpecialType.System_Object;

	public override Symbol ContainingSymbol => _container;

	public override TypeParameterSymbol OriginalDefinition
	{
		get
		{
			if (!(ContainingSymbol.OriginalDefinition != _underlyingTypeParameter.ContainingSymbol.OriginalDefinition))
			{
				return _underlyingTypeParameter.OriginalDefinition;
			}
			return this;
		}
	}

	public override TypeParameterSymbol ReducedFrom
	{
		get
		{
			if (_container.Kind == SymbolKind.Method)
			{
				MethodSymbol reducedFrom = ((MethodSymbol)_container).ReducedFrom;
				if ((object)reducedFrom != null)
				{
					return reducedFrom.TypeParameters[Ordinal];
				}
			}
			return null;
		}
	}

	public override int Ordinal => _ordinal;

	public override string Name => base.Name;

	internal SubstitutedTypeParameterSymbol(Symbol newContainer, TypeMap map, TypeParameterSymbol substitutedFrom, int ordinal)
		: base(substitutedFrom)
	{
		_container = newContainer;
		_map = map;
		_ordinal = ordinal;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _underlyingTypeParameter.GetAttributes();
	}

	internal override ImmutableArray<TypeSymbol> GetConstraintTypes(ConsList<TypeParameterSymbol> inProgress)
	{
		return _map.SubstituteTypesWithoutModifiers(_underlyingTypeParameter.GetConstraintTypes(inProgress)).WhereAsArray(s_isNotObjectFunc).Distinct();
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfaces(ConsList<TypeParameterSymbol> inProgress)
	{
		return _map.SubstituteNamedTypes(_underlyingTypeParameter.GetInterfaces(inProgress));
	}

	internal override NamedTypeSymbol GetEffectiveBaseClass(ConsList<TypeParameterSymbol> inProgress)
	{
		return _map.SubstituteNamedType(_underlyingTypeParameter.GetEffectiveBaseClass(inProgress));
	}

	internal override TypeSymbol GetDeducedBaseType(ConsList<TypeParameterSymbol> inProgress)
	{
		return _map.SubstituteType(_underlyingTypeParameter.GetDeducedBaseType(inProgress)).AsTypeSymbolOnly();
	}
}
