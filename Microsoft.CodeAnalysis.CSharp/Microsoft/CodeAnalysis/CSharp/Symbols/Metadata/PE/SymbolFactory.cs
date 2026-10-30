using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal sealed class SymbolFactory : SymbolFactory<PEModuleSymbol, TypeSymbol>
{
	internal static readonly SymbolFactory Instance = new SymbolFactory();

	internal override TypeSymbol GetMDArrayTypeSymbol(PEModuleSymbol moduleSymbol, int rank, TypeSymbol elementType, ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers, ImmutableArray<int> sizes, ImmutableArray<int> lowerBounds)
	{
		if (elementType is UnsupportedMetadataTypeSymbol)
		{
			return elementType;
		}
		return ArrayTypeSymbol.CreateMDArray(moduleSymbol.ContainingAssembly, elementType, rank, sizes, lowerBounds, CSharpCustomModifier.Convert(customModifiers));
	}

	internal override TypeSymbol GetSpecialType(PEModuleSymbol moduleSymbol, SpecialType specialType)
	{
		return moduleSymbol.ContainingAssembly.GetSpecialType(specialType);
	}

	internal override TypeSymbol GetSystemTypeSymbol(PEModuleSymbol moduleSymbol)
	{
		return moduleSymbol.SystemTypeSymbol;
	}

	internal override TypeSymbol MakePointerTypeSymbol(PEModuleSymbol moduleSymbol, TypeSymbol type, ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers)
	{
		if (type is UnsupportedMetadataTypeSymbol)
		{
			return type;
		}
		return new PointerTypeSymbol(type, CSharpCustomModifier.Convert(customModifiers));
	}

	internal override TypeSymbol GetEnumUnderlyingType(PEModuleSymbol moduleSymbol, TypeSymbol type)
	{
		return type.GetEnumUnderlyingType();
	}

	internal override PrimitiveTypeCode GetPrimitiveTypeCode(PEModuleSymbol moduleSymbol, TypeSymbol type)
	{
		return type.PrimitiveTypeCode;
	}

	internal override bool IsAcceptedVolatileModifierType(PEModuleSymbol moduleSymbol, TypeSymbol type)
	{
		return type.SpecialType == SpecialType.System_Runtime_CompilerServices_IsVolatile;
	}

	internal override bool IsAcceptedInAttributeModifierType(TypeSymbol type)
	{
		return type.IsWellKnownTypeInAttribute();
	}

	internal override bool IsAcceptedUnmanagedTypeModifierType(TypeSymbol type)
	{
		return type.IsWellKnownTypeUnmanagedType();
	}

	internal override TypeSymbol GetSZArrayTypeSymbol(PEModuleSymbol moduleSymbol, TypeSymbol elementType, ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers)
	{
		if (elementType is UnsupportedMetadataTypeSymbol)
		{
			return elementType;
		}
		return ArrayTypeSymbol.CreateSZArray(moduleSymbol.ContainingAssembly, elementType, CSharpCustomModifier.Convert(customModifiers));
	}

	internal override TypeSymbol GetUnsupportedMetadataTypeSymbol(PEModuleSymbol moduleSymbol, BadImageFormatException exception)
	{
		return new UnsupportedMetadataTypeSymbol(exception);
	}

	internal override TypeSymbol SubstituteTypeParameters(PEModuleSymbol moduleSymbol, TypeSymbol genericTypeDef, ImmutableArray<KeyValuePair<TypeSymbol, ImmutableArray<ModifierInfo<TypeSymbol>>>> arguments, ImmutableArray<bool> refersToNoPiaLocalType)
	{
		if (genericTypeDef is UnsupportedMetadataTypeSymbol)
		{
			return genericTypeDef;
		}
		foreach (KeyValuePair<TypeSymbol, ImmutableArray<ModifierInfo<TypeSymbol>>> item in arguments)
		{
			if (item.Key.Kind == SymbolKind.ErrorType && item.Key is UnsupportedMetadataTypeSymbol)
			{
				return new UnsupportedMetadataTypeSymbol();
			}
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)genericTypeDef;
		ImmutableArray<AssemblySymbol> linkedReferencedAssemblies = moduleSymbol.ContainingAssembly.GetLinkedReferencedAssemblies();
		bool flag = false;
		if (!linkedReferencedAssemblies.IsDefaultOrEmpty || moduleSymbol.Module.ContainsNoPiaLocalTypes())
		{
			NamedTypeSymbol namedTypeSymbol2 = namedTypeSymbol;
			int num = refersToNoPiaLocalType.Length - 1;
			while (namedTypeSymbol2.IsInterface)
			{
				num -= namedTypeSymbol2.Arity;
				namedTypeSymbol2 = namedTypeSymbol2.ContainingType;
				if ((object)namedTypeSymbol2 == null)
				{
					break;
				}
			}
			for (int num2 = num; num2 >= 0; num2--)
			{
				if (refersToNoPiaLocalType[num2] || (!linkedReferencedAssemblies.IsDefaultOrEmpty && MetadataDecoder.IsOrClosedOverATypeFromAssemblies(arguments[num2].Key, linkedReferencedAssemblies)))
				{
					flag = true;
					break;
				}
			}
		}
		ImmutableArray<TypeParameterSymbol> allTypeParameters = namedTypeSymbol.GetAllTypeParameters();
		if (allTypeParameters.Length != arguments.Length)
		{
			return new UnsupportedMetadataTypeSymbol();
		}
		NamedTypeSymbol namedTypeSymbol3 = new TypeMap(allTypeParameters, arguments.SelectAsArray((KeyValuePair<TypeSymbol, ImmutableArray<ModifierInfo<TypeSymbol>>> arg) => new TypeWithModifiers(arg.Key, CSharpCustomModifier.Convert(arg.Value)))).SubstituteNamedType(namedTypeSymbol);
		if (flag)
		{
			namedTypeSymbol3 = new NoPiaIllegalGenericInstantiationSymbol(moduleSymbol, namedTypeSymbol3);
		}
		return namedTypeSymbol3;
	}

	internal override TypeSymbol MakeUnboundIfGeneric(PEModuleSymbol moduleSymbol, TypeSymbol type)
	{
		if (!(type is NamedTypeSymbol { IsGenericType: not false } namedTypeSymbol))
		{
			return type;
		}
		return namedTypeSymbol.AsUnboundGenericType();
	}
}
