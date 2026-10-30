using System;
using System.Collections.Immutable;
using System.Reflection.Metadata;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal sealed class MemberRefMetadataDecoder : MetadataDecoder
{
	private readonly TypeSymbol _containingType;

	public MemberRefMetadataDecoder(PEModuleSymbol moduleSymbol, TypeSymbol containingType)
		: base(moduleSymbol, containingType as PENamedTypeSymbol)
	{
		_containingType = containingType;
	}

	protected override TypeSymbol GetGenericMethodTypeParamSymbol(int position)
	{
		return IndexedTypeParameterSymbol.GetTypeParameter(position);
	}

	protected override TypeSymbol GetGenericTypeParamSymbol(int position)
	{
		PENamedTypeSymbol pENamedTypeSymbol = _containingType as PENamedTypeSymbol;
		if ((object)pENamedTypeSymbol != null)
		{
			while ((object)pENamedTypeSymbol != null && pENamedTypeSymbol.MetadataArity - pENamedTypeSymbol.Arity > position)
			{
				pENamedTypeSymbol = pENamedTypeSymbol.ContainingSymbol as PENamedTypeSymbol;
			}
			if ((object)pENamedTypeSymbol == null || pENamedTypeSymbol.MetadataArity <= position)
			{
				return new UnsupportedMetadataTypeSymbol();
			}
			position -= pENamedTypeSymbol.MetadataArity - pENamedTypeSymbol.Arity;
			return pENamedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics[position];
		}
		if (_containingType is NamedTypeSymbol namedType)
		{
			GetGenericTypeArgumentSymbol(position, namedType, out var _, out var typeArgument);
			if ((object)typeArgument != null)
			{
				return typeArgument;
			}
			return new UnsupportedMetadataTypeSymbol();
		}
		return new UnsupportedMetadataTypeSymbol();
	}

	private static void GetGenericTypeArgumentSymbol(int position, NamedTypeSymbol namedType, out int cumulativeArity, out TypeSymbol typeArgument)
	{
		cumulativeArity = namedType.Arity;
		typeArgument = null;
		int num = 0;
		NamedTypeSymbol containingType = namedType.ContainingType;
		if ((object)containingType != null)
		{
			GetGenericTypeArgumentSymbol(position, containingType, out var cumulativeArity2, out typeArgument);
			cumulativeArity += cumulativeArity2;
			num = cumulativeArity2;
		}
		if (num <= position && position < cumulativeArity)
		{
			typeArgument = namedType.TypeArgumentsNoUseSiteDiagnostics[position - num];
		}
	}

	internal Symbol FindMember(TypeSymbol targetTypeSymbol, MemberReferenceHandle memberRef, bool methodsOnly)
	{
		if ((object)targetTypeSymbol == null)
		{
			return null;
		}
		try
		{
			string memberRefNameOrThrow = Module.GetMemberRefNameOrThrow(memberRef);
			BlobHandle signatureOrThrow = Module.GetSignatureOrThrow(memberRef);
			BlobReader signatureReader = DecodeSignatureHeaderOrThrow(signatureOrThrow, out var signatureHeader);
			switch (signatureHeader.RawValue & 0xF)
			{
			case 0:
			case 5:
			{
				ParamInfo<TypeSymbol>[] targetParamInfo = DecodeSignatureParametersOrThrow(ref signatureReader, signatureHeader, out var typeParameterCount);
				return FindMethodBySignature(targetTypeSymbol, memberRefNameOrThrow, signatureHeader, typeParameterCount, targetParamInfo);
			}
			case 6:
			{
				if (methodsOnly)
				{
					return null;
				}
				TypeSymbol type = DecodeFieldSignature(ref signatureReader, out var _, out var customModifiers);
				return FindFieldBySignature(targetTypeSymbol, memberRefNameOrThrow, customModifiers, type);
			}
			default:
				return null;
			}
		}
		catch (BadImageFormatException)
		{
			return null;
		}
	}

	private static FieldSymbol FindFieldBySignature(TypeSymbol targetTypeSymbol, string targetMemberName, ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers, TypeSymbol type)
	{
		foreach (Symbol member in targetTypeSymbol.GetMembers(targetMemberName))
		{
			if (member is FieldSymbol fieldSymbol && fieldSymbol.Type == type && CustomModifiersMatch(fieldSymbol.CustomModifiers, customModifiers))
			{
				return fieldSymbol;
			}
		}
		return null;
	}

	private static MethodSymbol FindMethodBySignature(TypeSymbol targetTypeSymbol, string targetMemberName, SignatureHeader targetMemberSignatureHeader, int targetMemberTypeParamCount, ParamInfo<TypeSymbol>[] targetParamInfo)
	{
		foreach (Symbol member in targetTypeSymbol.GetMembers(targetMemberName))
		{
			if (member is MethodSymbol methodSymbol && (byte)methodSymbol.CallingConvention == targetMemberSignatureHeader.RawValue && targetMemberTypeParamCount == methodSymbol.Arity && MethodSymbolMatchesParamInfo(methodSymbol, targetParamInfo))
			{
				return methodSymbol;
			}
		}
		return null;
	}

	private static bool MethodSymbolMatchesParamInfo(MethodSymbol candidateMethod, ParamInfo<TypeSymbol>[] targetParamInfo)
	{
		int num = targetParamInfo.Length - 1;
		if (candidateMethod.ParameterCount != num)
		{
			return false;
		}
		TypeMap candidateMethodTypeMap = new TypeMap(candidateMethod.TypeParameters, IndexedTypeParameterSymbol.Take(candidateMethod.Arity), allowAlpha: true);
		if (!ReturnTypesMatch(candidateMethod, candidateMethodTypeMap, ref targetParamInfo[0]))
		{
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			if (!ParametersMatch(candidateMethod.Parameters[i], candidateMethodTypeMap, ref targetParamInfo[i + 1]))
			{
				return false;
			}
		}
		return true;
	}

	private static bool ParametersMatch(ParameterSymbol candidateParam, TypeMap candidateMethodTypeMap, ref ParamInfo<TypeSymbol> targetParam)
	{
		if (candidateParam.RefKind != RefKind.None != targetParam.IsByRef)
		{
			return false;
		}
		TypeWithModifiers typeWithModifiers = new TypeWithModifiers(candidateParam.Type, candidateParam.CustomModifiers).SubstituteType(candidateMethodTypeMap);
		if (typeWithModifiers.Type != targetParam.Type)
		{
			return false;
		}
		if (!CustomModifiersMatch(typeWithModifiers.CustomModifiers, targetParam.CustomModifiers) || !CustomModifiersMatch(candidateMethodTypeMap.SubstituteCustomModifiers(candidateParam.RefCustomModifiers), targetParam.RefCustomModifiers))
		{
			return false;
		}
		return true;
	}

	private static bool ReturnTypesMatch(MethodSymbol candidateMethod, TypeMap candidateMethodTypeMap, ref ParamInfo<TypeSymbol> targetReturnParam)
	{
		if (candidateMethod.ReturnsByRef != targetReturnParam.IsByRef)
		{
			return false;
		}
		TypeSymbol returnType = candidateMethod.ReturnType;
		TypeSymbol type = targetReturnParam.Type;
		TypeWithModifiers typeWithModifiers = new TypeWithModifiers(returnType, candidateMethod.ReturnTypeCustomModifiers).SubstituteType(candidateMethodTypeMap);
		if (typeWithModifiers.Type != type)
		{
			return false;
		}
		if (!CustomModifiersMatch(typeWithModifiers.CustomModifiers, targetReturnParam.CustomModifiers) || !CustomModifiersMatch(candidateMethodTypeMap.SubstituteCustomModifiers(candidateMethod.RefCustomModifiers), targetReturnParam.RefCustomModifiers))
		{
			return false;
		}
		return true;
	}

	private static bool CustomModifiersMatch(ImmutableArray<CustomModifier> candidateCustomModifiers, ImmutableArray<ModifierInfo<TypeSymbol>> targetCustomModifiers)
	{
		if (targetCustomModifiers.IsDefault || targetCustomModifiers.IsEmpty)
		{
			if (!candidateCustomModifiers.IsDefault)
			{
				return candidateCustomModifiers.IsEmpty;
			}
			return true;
		}
		if (candidateCustomModifiers.IsDefault)
		{
			return false;
		}
		int length = candidateCustomModifiers.Length;
		if (targetCustomModifiers.Length != length)
		{
			return false;
		}
		for (int i = 0; i < length; i++)
		{
			ModifierInfo<TypeSymbol> modifierInfo = targetCustomModifiers[i];
			CustomModifier customModifier = candidateCustomModifiers[i];
			if (modifierInfo.IsOptional != customModifier.IsOptional || !object.Equals(modifierInfo.Modifier, customModifier.Modifier))
			{
				return false;
			}
		}
		return true;
	}
}
