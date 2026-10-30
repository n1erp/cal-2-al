using System;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal struct TupleTypeDecoder
{
	private readonly ImmutableArray<string> _elementNames;

	private int _namesIndex;

	private TupleTypeDecoder(ImmutableArray<string> elementNames)
	{
		_elementNames = elementNames;
		_namesIndex = ((!elementNames.IsDefault) ? elementNames.Length : 0);
	}

	public static TypeSymbol DecodeTupleTypesIfApplicable(TypeSymbol metadataType, EntityHandle targetHandle, PEModuleSymbol containingModule)
	{
		bool flag = containingModule.Module.HasTupleElementNamesAttribute(targetHandle, out var tupleElementNames);
		if (flag && tupleElementNames.IsDefaultOrEmpty)
		{
			return new UnsupportedMetadataTypeSymbol();
		}
		return DecodeTupleTypesInternal(metadataType, tupleElementNames, flag);
	}

	public static TypeSymbol DecodeTupleTypesIfApplicable(TypeSymbol metadataType, ImmutableArray<string> elementNames)
	{
		return DecodeTupleTypesInternal(metadataType, elementNames, !elementNames.IsDefaultOrEmpty);
	}

	private static TypeSymbol DecodeTupleTypesInternal(TypeSymbol metadataType, ImmutableArray<string> elementNames, bool hasTupleElementNamesAttribute)
	{
		TupleTypeDecoder tupleTypeDecoder = new TupleTypeDecoder(elementNames);
		try
		{
			TypeSymbol result = tupleTypeDecoder.DecodeType(metadataType);
			if (!hasTupleElementNamesAttribute || tupleTypeDecoder._namesIndex == 0)
			{
				return result;
			}
		}
		catch (InvalidOperationException)
		{
		}
		if (metadataType.HasUseSiteError)
		{
			return metadataType;
		}
		return new UnsupportedMetadataTypeSymbol();
	}

	private TypeSymbol DecodeType(TypeSymbol type)
	{
		switch (type.Kind)
		{
		case SymbolKind.DynamicType:
		case SymbolKind.ErrorType:
		case SymbolKind.PointerType:
		case SymbolKind.TypeParameter:
			return type;
		case SymbolKind.NamedType:
			if (!type.IsTupleType)
			{
				return DecodeNamedType((NamedTypeSymbol)type);
			}
			return DecodeNamedType(type.TupleUnderlyingType);
		case SymbolKind.ArrayType:
			return DecodeArrayType((ArrayTypeSymbol)type);
		default:
			throw ExceptionUtilities.UnexpectedValue(type.TypeKind);
		}
	}

	private NamedTypeSymbol DecodeNamedType(NamedTypeSymbol type)
	{
		ImmutableArray<TypeSymbol> typeArgumentsNoUseSiteDiagnostics = type.TypeArgumentsNoUseSiteDiagnostics;
		ImmutableArray<TypeSymbol> immutableArray = DecodeTypeArguments(typeArgumentsNoUseSiteDiagnostics);
		NamedTypeSymbol namedTypeSymbol = type;
		NamedTypeSymbol containingType = type.ContainingType;
		NamedTypeSymbol namedTypeSymbol2 = (((object)containingType == null || !containingType.IsGenericType) ? containingType : DecodeNamedType(containingType));
		bool flag = (object)namedTypeSymbol2 != containingType;
		if ((typeArgumentsNoUseSiteDiagnostics != immutableArray) | flag)
		{
			ImmutableArray<TypeWithModifiers> immutableArray2 = (type.HasTypeArgumentsCustomModifiers ? immutableArray.SelectAsArray((TypeSymbol t, int i, NamedTypeSymbol m) => new TypeWithModifiers(t, m.GetTypeArgumentCustomModifiers(i)), type) : immutableArray.SelectAsArray(TypeMap.TypeSymbolAsTypeWithModifiers));
			if (flag)
			{
				namedTypeSymbol = namedTypeSymbol.OriginalDefinition.AsMember(namedTypeSymbol2);
				return namedTypeSymbol.ConstructIfGeneric(immutableArray2);
			}
			namedTypeSymbol = type.ConstructedFrom.Construct(immutableArray2, unbound: false);
		}
		if (namedTypeSymbol.IsTupleCompatible(out var tupleCardinality))
		{
			ImmutableArray<string> elementNames = EatElementNamesIfAvailable(tupleCardinality);
			namedTypeSymbol = TupleTypeSymbol.Create(namedTypeSymbol, elementNames);
		}
		return namedTypeSymbol;
	}

	private ImmutableArray<TypeSymbol> DecodeTypeArguments(ImmutableArray<TypeSymbol> typeArgs)
	{
		if (typeArgs.IsEmpty)
		{
			return typeArgs;
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance(typeArgs.Length);
		bool flag = false;
		for (int num = typeArgs.Length - 1; num >= 0; num--)
		{
			TypeSymbol typeSymbol = typeArgs[num];
			TypeSymbol typeSymbol2 = DecodeType(typeSymbol);
			flag |= (object)typeSymbol2 != typeSymbol;
			instance.Add(typeSymbol2);
		}
		if (!flag)
		{
			instance.Free();
			return typeArgs;
		}
		instance.ReverseContents();
		return instance.ToImmutableAndFree();
	}

	private ArrayTypeSymbol DecodeArrayType(ArrayTypeSymbol type)
	{
		TypeSymbol typeSymbol = DecodeType(type.ElementType);
		if ((object)typeSymbol != type.ElementType)
		{
			return type.WithElementType(typeSymbol);
		}
		return type;
	}

	private ImmutableArray<string> EatElementNamesIfAvailable(int numberOfElements)
	{
		if (_elementNames.IsDefault)
		{
			return _elementNames;
		}
		if (numberOfElements > _namesIndex)
		{
			throw new InvalidOperationException();
		}
		int num = (_namesIndex -= numberOfElements);
		bool flag = true;
		for (int i = 0; i < numberOfElements; i++)
		{
			if (_elementNames[num + i] != null)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return default;
		}
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance(numberOfElements);
		for (int j = 0; j < numberOfElements; j++)
		{
			instance.Add(_elementNames[num + j]);
		}
		return instance.ToImmutableAndFree();
	}
}
