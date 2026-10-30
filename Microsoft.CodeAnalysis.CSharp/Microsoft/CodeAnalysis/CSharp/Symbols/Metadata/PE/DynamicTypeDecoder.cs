using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal struct DynamicTypeDecoder
{
	private readonly ImmutableArray<bool> _dynamicTransformFlags;

	private readonly AssemblySymbol _containingAssembly;

	private readonly bool _haveCustomModifierFlags;

	private readonly bool _checkLength;

	private int _index;

	private bool HasFlag
	{
		get
		{
			if (_index >= _dynamicTransformFlags.Length)
			{
				return !_checkLength;
			}
			return true;
		}
	}

	private DynamicTypeDecoder(ImmutableArray<bool> dynamicTransformFlags, bool haveCustomModifierFlags, bool checkLength, AssemblySymbol containingAssembly)
	{
		_dynamicTransformFlags = dynamicTransformFlags;
		_containingAssembly = containingAssembly;
		_haveCustomModifierFlags = haveCustomModifierFlags;
		_checkLength = checkLength;
		_index = 0;
	}

	internal static TypeSymbol TransformType(TypeSymbol metadataType, int targetSymbolCustomModifierCount, EntityHandle targetSymbolToken, PEModuleSymbol containingModule, RefKind targetSymbolRefKind = RefKind.None)
	{
		if (containingModule.Module.HasDynamicAttribute(targetSymbolToken, out var dynamicTransforms))
		{
			return TransformTypeInternal(metadataType, containingModule.ContainingAssembly, targetSymbolCustomModifierCount, targetSymbolRefKind, dynamicTransforms, haveCustomModifierFlags: true, checkLength: true);
		}
		return metadataType;
	}

	internal static TypeSymbol TransformTypeWithoutCustomModifierFlags(TypeSymbol type, AssemblySymbol containingAssembly, RefKind targetSymbolRefKind, ImmutableArray<bool> dynamicTransformFlags, bool checkLength = true)
	{
		return TransformTypeInternal(type, containingAssembly, 0, targetSymbolRefKind, dynamicTransformFlags, haveCustomModifierFlags: false, checkLength);
	}

	private static TypeSymbol TransformTypeInternal(TypeSymbol metadataType, AssemblySymbol containingAssembly, int targetSymbolCustomModifierCount, RefKind targetSymbolRefKind, ImmutableArray<bool> dynamicTransformFlags, bool haveCustomModifierFlags, bool checkLength)
	{
		if (dynamicTransformFlags.Length == 0)
		{
			return new UnsupportedMetadataTypeSymbol();
		}
		DynamicTypeDecoder dynamicTypeDecoder = new DynamicTypeDecoder(dynamicTransformFlags, haveCustomModifierFlags, checkLength, containingAssembly);
		if (dynamicTypeDecoder.HandleCustomModifiers(targetSymbolCustomModifierCount) && dynamicTypeDecoder.HandleParameterRefKind(targetSymbolRefKind))
		{
			TypeSymbol typeSymbol = dynamicTypeDecoder.TransformType(metadataType);
			if ((object)typeSymbol != null && (!checkLength || dynamicTypeDecoder._index == dynamicTransformFlags.Length))
			{
				return typeSymbol;
			}
		}
		return metadataType;
	}

	private TypeSymbol TransformType(TypeSymbol type)
	{
		if (!HasFlag || (PeekFlag() && type.SpecialType != SpecialType.System_Object && !type.IsDynamic()))
		{
			return null;
		}
		switch (type.Kind)
		{
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
			if (type.SpecialType == SpecialType.System_Object)
			{
				if (!ConsumeFlag())
				{
					return type;
				}
				return DynamicTypeSymbol.Instance;
			}
			return TransformNamedType((NamedTypeSymbol)type);
		case SymbolKind.ArrayType:
			return TransformArrayType((ArrayTypeSymbol)type);
		case SymbolKind.PointerType:
			return TransformPointerType((PointerTypeSymbol)type);
		case SymbolKind.DynamicType:
			if (!ConsumeFlag())
			{
				return _containingAssembly.GetSpecialType(SpecialType.System_Object);
			}
			return type;
		default:
			ConsumeFlag();
			if (!HandleCustomModifiers(type.CustomModifierCount()))
			{
				return null;
			}
			return type;
		}
	}

	private bool HandleCustomModifiers(int customModifiersCount)
	{
		if (!_haveCustomModifierFlags)
		{
			return true;
		}
		for (int i = 0; i < customModifiersCount; i++)
		{
			if (!HasFlag || ConsumeFlag())
			{
				return false;
			}
		}
		return true;
	}

	private bool HandleParameterRefKind(RefKind refKind)
	{
		if (refKind != RefKind.None)
		{
			return !ConsumeFlag();
		}
		return true;
	}

	private NamedTypeSymbol TransformNamedType(NamedTypeSymbol namedType, bool isContaining = false)
	{
		if (namedType.IsTupleType)
		{
			return TransformTupleType(namedType, isContaining);
		}
		if (!isContaining)
		{
			ConsumeFlag();
		}
		NamedTypeSymbol containingType = namedType.ContainingType;
		NamedTypeSymbol namedTypeSymbol;
		if ((object)containingType != null && containingType.IsGenericType)
		{
			namedTypeSymbol = TransformNamedType(namedType.ContainingType, isContaining: true);
			if ((object)namedTypeSymbol == null)
			{
				return null;
			}
		}
		else
		{
			namedTypeSymbol = containingType;
		}
		ImmutableArray<TypeSymbol> typeArgumentsNoUseSiteDiagnostics = namedType.TypeArgumentsNoUseSiteDiagnostics;
		ImmutableArray<TypeSymbol> immutableArray = TransformTypeArguments(typeArgumentsNoUseSiteDiagnostics);
		if (immutableArray.IsDefault)
		{
			return null;
		}
		bool flag = namedTypeSymbol != containingType;
		if (flag || immutableArray != typeArgumentsNoUseSiteDiagnostics)
		{
			ImmutableArray<TypeWithModifiers> immutableArray2 = (namedType.HasTypeArgumentsCustomModifiers ? immutableArray.SelectAsArray((TypeSymbol t, int i, NamedTypeSymbol nt) => new TypeWithModifiers(t, nt.GetTypeArgumentCustomModifiers(i)), namedType) : immutableArray.SelectAsArray(TypeMap.TypeSymbolAsTypeWithModifiers));
			if (flag)
			{
				namedType = namedType.OriginalDefinition.AsMember(namedTypeSymbol);
				return namedType.ConstructIfGeneric(immutableArray2);
			}
			return namedType.ConstructedFrom.Construct(immutableArray2, unbound: false);
		}
		return namedType;
	}

	private NamedTypeSymbol TransformTupleType(NamedTypeSymbol tupleType, bool isContaining)
	{
		NamedTypeSymbol tupleUnderlyingType = tupleType.TupleUnderlyingType;
		NamedTypeSymbol namedTypeSymbol = TransformNamedType(tupleUnderlyingType, isContaining);
		if (namedTypeSymbol == null)
		{
			return null;
		}
		return TupleTypeSymbol.Create(namedTypeSymbol, tupleType.TupleElementNames);
	}

	private ImmutableArray<TypeSymbol> TransformTypeArguments(ImmutableArray<TypeSymbol> typeArguments)
	{
		if (!typeArguments.Any())
		{
			return typeArguments;
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		bool flag = false;
		foreach (TypeSymbol item in typeArguments)
		{
			TypeSymbol typeSymbol = TransformType(item);
			if ((object)typeSymbol == null)
			{
				instance.Free();
				return default;
			}
			instance.Add(typeSymbol);
			flag |= typeSymbol != item;
		}
		if (!flag)
		{
			instance.Free();
			return typeArguments;
		}
		return instance.ToImmutableAndFree();
	}

	private ArrayTypeSymbol TransformArrayType(ArrayTypeSymbol arrayType)
	{
		ConsumeFlag();
		if (!HandleCustomModifiers(arrayType.CustomModifiers.Length))
		{
			return null;
		}
		TypeSymbol typeSymbol = TransformType(arrayType.ElementType);
		if ((object)typeSymbol == null)
		{
			return null;
		}
		if (!(typeSymbol == arrayType.ElementType))
		{
			if (!arrayType.IsSZArray)
			{
				return ArrayTypeSymbol.CreateMDArray(_containingAssembly, typeSymbol, arrayType.Rank, arrayType.Sizes, arrayType.LowerBounds, arrayType.CustomModifiers);
			}
			return ArrayTypeSymbol.CreateSZArray(_containingAssembly, typeSymbol, arrayType.CustomModifiers);
		}
		return arrayType;
	}

	private PointerTypeSymbol TransformPointerType(PointerTypeSymbol pointerType)
	{
		ConsumeFlag();
		if (!HandleCustomModifiers(pointerType.CustomModifiers.Length))
		{
			return null;
		}
		TypeSymbol typeSymbol = TransformType(pointerType.PointedAtType);
		if ((object)typeSymbol == null)
		{
			return null;
		}
		if (!(typeSymbol == pointerType.PointedAtType))
		{
			return new PointerTypeSymbol(typeSymbol, pointerType.CustomModifiers);
		}
		return pointerType;
	}

	private bool PeekFlag()
	{
		if (_index < _dynamicTransformFlags.Length)
		{
			return _dynamicTransformFlags[_index];
		}
		return false;
	}

	private bool ConsumeFlag()
	{
		bool result = PeekFlag();
		_index++;
		return result;
	}
}
