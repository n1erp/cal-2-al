using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ConstructedNamedTypeSymbol : SubstitutedNamedTypeSymbol
{
	private readonly ImmutableArray<TypeSymbol> _typeArguments;

	private readonly bool _hasTypeArgumentsCustomModifiers;

	private readonly NamedTypeSymbol _constructedFrom;

	public override NamedTypeSymbol ConstructedFrom => _constructedFrom;

	internal override ImmutableArray<TypeSymbol> TypeArgumentsNoUseSiteDiagnostics => _typeArguments;

	internal override bool HasTypeArgumentsCustomModifiers => _hasTypeArgumentsCustomModifiers;

	internal ConstructedNamedTypeSymbol(NamedTypeSymbol constructedFrom, ImmutableArray<TypeWithModifiers> typeArguments, bool unbound = false)
		: base(constructedFrom.ContainingSymbol, new TypeMap(constructedFrom.ContainingType, constructedFrom.OriginalDefinition.TypeParameters, typeArguments), constructedFrom.OriginalDefinition, constructedFrom, unbound)
	{
		_typeArguments = typeArguments.ToTypes(out _hasTypeArgumentsCustomModifiers);
		_constructedFrom = constructedFrom;
	}

	public override ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal)
	{
		if (_hasTypeArgumentsCustomModifiers)
		{
			return TypeSubstitution.GetTypeArgumentsCustomModifiersFor(_constructedFrom.OriginalDefinition.TypeParameters[ordinal]);
		}
		return GetEmptyTypeArgumentCustomModifiers(ordinal);
	}

	internal static bool TypeParametersMatchTypeArguments(ImmutableArray<TypeParameterSymbol> typeParameters, ImmutableArray<TypeWithModifiers> typeArguments)
	{
		int length = typeParameters.Length;
		for (int i = 0; i < length; i++)
		{
			if (!typeArguments[i].Is(typeParameters[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal sealed override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		if (ConstructedFrom.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes) || Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, _typeArguments, owner, ref checkedTypes))
		{
			return true;
		}
		if (_hasTypeArgumentsCustomModifiers)
		{
			for (int i = 0; i < Arity; i++)
			{
				if (Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, GetTypeArgumentCustomModifiers(i), owner, ref checkedTypes))
				{
					return true;
				}
			}
		}
		return false;
	}
}
