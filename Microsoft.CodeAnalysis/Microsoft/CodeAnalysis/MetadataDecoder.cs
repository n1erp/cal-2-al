using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal abstract class MetadataDecoder<ModuleSymbol, TypeSymbol, MethodSymbol, FieldSymbol, Symbol> : TypeNameDecoder<ModuleSymbol, TypeSymbol> where ModuleSymbol : class where TypeSymbol : class, Symbol, ITypeSymbol where MethodSymbol : class, Symbol, IMethodSymbol where FieldSymbol : class, Symbol, IFieldSymbol where Symbol : class, ISymbol
{
	private enum AllowedRequiredModifierType
	{
		None,
		System_Runtime_CompilerServices_Volatile,
		System_Runtime_InteropServices_InAttribute,
		System_Runtime_InteropServices_UnmanagedType
	}

	public readonly PEModule Module;

	private readonly AssemblyIdentity _containingAssemblyIdentity;

	internal MetadataDecoder(PEModule module, AssemblyIdentity containingAssemblyIdentity, SymbolFactory<ModuleSymbol, TypeSymbol> factory, ModuleSymbol moduleSymbol)
		: base(factory, moduleSymbol)
	{
		Module = module;
		_containingAssemblyIdentity = containingAssemblyIdentity;
	}

	internal TypeSymbol GetTypeOfToken(EntityHandle token)
	{
		bool isNoPiaLocalType;
		return GetTypeOfToken(token, out isNoPiaLocalType);
	}

	internal TypeSymbol GetTypeOfToken(EntityHandle token, out bool isNoPiaLocalType)
	{
		switch (token.Kind)
		{
		case HandleKind.TypeDefinition:
			return GetTypeOfTypeDef((TypeDefinitionHandle)token, out isNoPiaLocalType, isContainingType: false);
		case HandleKind.TypeSpecification:
			isNoPiaLocalType = false;
			return GetTypeOfTypeSpec((TypeSpecificationHandle)token);
		case HandleKind.TypeReference:
			return GetTypeOfTypeRef((TypeReferenceHandle)token, out isNoPiaLocalType);
		default:
			isNoPiaLocalType = false;
			return GetUnsupportedMetadataTypeSymbol();
		}
	}

	private TypeSymbol GetTypeOfTypeSpec(TypeSpecificationHandle typeSpec)
	{
		try
		{
			BlobReader ppSig = Module.GetTypeSpecificationSignatureReaderOrThrow(typeSpec);
			bool refersToNoPiaLocalType;
			return DecodeTypeOrThrow(ref ppSig, out refersToNoPiaLocalType);
		}
		catch (BadImageFormatException exception)
		{
			return GetUnsupportedMetadataTypeSymbol(exception);
		}
		catch (UnsupportedSignatureContent)
		{
			return GetUnsupportedMetadataTypeSymbol();
		}
	}

	private TypeSymbol DecodeTypeOrThrow(ref BlobReader ppSig, out bool refersToNoPiaLocalType)
	{
		SignatureTypeCode typeCode = ppSig.ReadSignatureTypeCode();
		return DecodeTypeOrThrow(ref ppSig, typeCode, out refersToNoPiaLocalType);
	}

	private TypeSymbol DecodeTypeOrThrow(ref BlobReader ppSig, SignatureTypeCode typeCode, out bool refersToNoPiaLocalType)
	{
		refersToNoPiaLocalType = false;
		bool requiredModifierFound;
		int value;
		switch (typeCode)
		{
		case SignatureTypeCode.Void:
		case SignatureTypeCode.Boolean:
		case SignatureTypeCode.Char:
		case SignatureTypeCode.SByte:
		case SignatureTypeCode.Byte:
		case SignatureTypeCode.Int16:
		case SignatureTypeCode.UInt16:
		case SignatureTypeCode.Int32:
		case SignatureTypeCode.UInt32:
		case SignatureTypeCode.Int64:
		case SignatureTypeCode.UInt64:
		case SignatureTypeCode.Single:
		case SignatureTypeCode.Double:
		case SignatureTypeCode.String:
		case SignatureTypeCode.TypedReference:
		case SignatureTypeCode.IntPtr:
		case SignatureTypeCode.UIntPtr:
		case SignatureTypeCode.Object:
			return GetSpecialType(typeCode.ToSpecialType());
		case SignatureTypeCode.TypeHandle:
			return GetSymbolForTypeHandleOrThrow(ppSig.ReadTypeHandle(), out refersToNoPiaLocalType, allowTypeSpec: false, requireShortForm: true);
		case SignatureTypeCode.Array:
		{
			ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers = DecodeModifiersOrThrow(ref ppSig, AllowedRequiredModifierType.None, out typeCode, out requiredModifierFound);
			TypeSymbol elementType = DecodeTypeOrThrow(ref ppSig, typeCode, out refersToNoPiaLocalType);
			if (!ppSig.TryReadCompressedInteger(out var value2) || !ppSig.TryReadCompressedInteger(out var value3))
			{
				throw new UnsupportedSignatureContent();
			}
			ImmutableArray<int> sizes;
			if (value3 == 0)
			{
				sizes = ImmutableArray<int>.Empty;
			}
			else
			{
				ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance(value3);
				for (int i = 0; i < value3; i++)
				{
					if (ppSig.TryReadCompressedInteger(out var value4))
					{
						instance.Add(value4);
						continue;
					}
					throw new UnsupportedSignatureContent();
				}
				sizes = instance.ToImmutableAndFree();
			}
			if (!ppSig.TryReadCompressedInteger(out var value5))
			{
				throw new UnsupportedSignatureContent();
			}
			ImmutableArray<int> lowerBounds = default;
			if (value5 == 0)
			{
				lowerBounds = ImmutableArray<int>.Empty;
			}
			else
			{
				ArrayBuilder<int> arrayBuilder = ((value5 != value2) ? ArrayBuilder<int>.GetInstance(value5, 0) : null);
				for (int j = 0; j < value5; j++)
				{
					if (ppSig.TryReadCompressedSignedInteger(out var value6))
					{
						if (value6 != 0)
						{
							if (arrayBuilder == null)
							{
								arrayBuilder = ArrayBuilder<int>.GetInstance(value5, 0);
							}
							arrayBuilder[j] = value6;
						}
						continue;
					}
					throw new UnsupportedSignatureContent();
				}
				if (arrayBuilder != null)
				{
					lowerBounds = arrayBuilder.ToImmutableAndFree();
				}
			}
			return GetMDArrayTypeSymbol(value2, elementType, customModifiers, sizes, lowerBounds);
		}
		case SignatureTypeCode.SZArray:
		{
			ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers = DecodeModifiersOrThrow(ref ppSig, AllowedRequiredModifierType.None, out typeCode, out requiredModifierFound);
			TypeSymbol elementType = DecodeTypeOrThrow(ref ppSig, typeCode, out refersToNoPiaLocalType);
			return GetSZArrayTypeSymbol(elementType, customModifiers);
		}
		case SignatureTypeCode.Pointer:
		{
			ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers = DecodeModifiersOrThrow(ref ppSig, AllowedRequiredModifierType.None, out typeCode, out requiredModifierFound);
			TypeSymbol elementType = DecodeTypeOrThrow(ref ppSig, typeCode, out refersToNoPiaLocalType);
			return MakePointerTypeSymbol(elementType, customModifiers);
		}
		case SignatureTypeCode.GenericTypeParameter:
			if (!ppSig.TryReadCompressedInteger(out value))
			{
				throw new UnsupportedSignatureContent();
			}
			return GetGenericTypeParamSymbol(value);
		case SignatureTypeCode.GenericMethodParameter:
			if (!ppSig.TryReadCompressedInteger(out value))
			{
				throw new UnsupportedSignatureContent();
			}
			return GetGenericMethodTypeParamSymbol(value);
		case SignatureTypeCode.GenericTypeInstance:
			return DecodeGenericTypeInstanceOrThrow(ref ppSig, out refersToNoPiaLocalType);
		default:
			throw new UnsupportedSignatureContent();
		}
	}

	private TypeSymbol DecodeGenericTypeInstanceOrThrow(ref BlobReader ppSig, out bool refersToNoPiaLocalType)
	{
		if (ppSig.ReadSignatureTypeCode() != SignatureTypeCode.TypeHandle)
		{
			throw new UnsupportedSignatureContent();
		}
		EntityHandle token = ppSig.ReadTypeHandle();
		if (!ppSig.TryReadCompressedInteger(out var value))
		{
			throw new UnsupportedSignatureContent();
		}
		TypeSymbol typeOfToken = GetTypeOfToken(token, out refersToNoPiaLocalType);
		ArrayBuilder<KeyValuePair<TypeSymbol, ImmutableArray<ModifierInfo<TypeSymbol>>>> instance = ArrayBuilder<KeyValuePair<TypeSymbol, ImmutableArray<ModifierInfo<TypeSymbol>>>>.GetInstance(value);
		ArrayBuilder<bool> instance2 = ArrayBuilder<bool>.GetInstance(value);
		for (int i = 0; i < value; i++)
		{
			ImmutableArray<ModifierInfo<TypeSymbol>> value2 = DecodeModifiersOrThrow(ref ppSig, AllowedRequiredModifierType.None, out var typeCode, out var _);
			instance.Add(KeyValuePairUtil.Create(DecodeTypeOrThrow(ref ppSig, typeCode, out var refersToNoPiaLocalType2), value2));
			instance2.Add(refersToNoPiaLocalType2);
		}
		ImmutableArray<KeyValuePair<TypeSymbol, ImmutableArray<ModifierInfo<TypeSymbol>>>> arguments = instance.ToImmutableAndFree();
		ImmutableArray<bool> refersToNoPiaLocalType3 = instance2.ToImmutableAndFree();
		TypeSymbol result = SubstituteTypeParameters(typeOfToken, arguments, refersToNoPiaLocalType3);
		foreach (bool item in refersToNoPiaLocalType3)
		{
			if (item)
			{
				refersToNoPiaLocalType = true;
				break;
			}
		}
		return result;
	}

	internal TypeSymbol GetSymbolForTypeHandleOrThrow(EntityHandle handle, out bool isNoPiaLocalType, bool allowTypeSpec, bool requireShortForm)
	{
		if (handle.IsNil)
		{
			throw new UnsupportedSignatureContent();
		}
		TypeSymbol val;
		switch (handle.Kind)
		{
		case HandleKind.TypeDefinition:
			val = GetTypeOfTypeDef((TypeDefinitionHandle)handle, out isNoPiaLocalType, isContainingType: false);
			break;
		case HandleKind.TypeReference:
			val = GetTypeOfTypeRef((TypeReferenceHandle)handle, out isNoPiaLocalType);
			break;
		case HandleKind.TypeSpecification:
			if (!allowTypeSpec)
			{
				throw new UnsupportedSignatureContent();
			}
			isNoPiaLocalType = false;
			val = GetTypeOfTypeSpec((TypeSpecificationHandle)handle);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(handle.Kind);
		}
		if (requireShortForm && val.SpecialType.HasShortFormSignatureEncoding())
		{
			throw new UnsupportedSignatureContent();
		}
		return val;
	}

	private TypeSymbol GetTypeOfTypeRef(TypeReferenceHandle typeRef, out bool isNoPiaLocalType)
	{
		ConcurrentDictionary<TypeReferenceHandle, TypeSymbol> typeRefHandleToTypeMap = GetTypeRefHandleToTypeMap();
		if (typeRefHandleToTypeMap != null && typeRefHandleToTypeMap.TryGetValue(typeRef, out var value))
		{
			isNoPiaLocalType = false;
			return value;
		}
		try
		{
			Module.GetTypeRefPropsOrThrow(typeRef, out var name, out var @namespace, out var resolutionScope);
			MetadataTypeName fullName = ((@namespace.Length > 0) ? MetadataTypeName.FromNamespaceAndTypeName(@namespace, name) : MetadataTypeName.FromTypeName(name));
			value = GetTypeByNameOrThrow(ref fullName, resolutionScope, out isNoPiaLocalType);
		}
		catch (BadImageFormatException exception)
		{
			value = GetUnsupportedMetadataTypeSymbol(exception);
			isNoPiaLocalType = false;
		}
		if (typeRefHandleToTypeMap != null && !isNoPiaLocalType)
		{
			typeRefHandleToTypeMap.GetOrAdd(typeRef, value);
		}
		return value;
	}

	private TypeSymbol GetTypeByNameOrThrow(ref MetadataTypeName fullName, EntityHandle tokenResolutionScope, out bool isNoPiaLocalType)
	{
		switch (tokenResolutionScope.Kind)
		{
		case HandleKind.TypeReference:
		{
			if (tokenResolutionScope.IsNil)
			{
				throw new BadImageFormatException();
			}
			TypeSymbol typeOfToken = GetTypeOfToken(tokenResolutionScope);
			isNoPiaLocalType = false;
			return LookupNestedTypeDefSymbol(typeOfToken, ref fullName);
		}
		case HandleKind.AssemblyReference:
		{
			isNoPiaLocalType = false;
			AssemblyReferenceHandle assemblyRef = (AssemblyReferenceHandle)tokenResolutionScope;
			if (assemblyRef.IsNil)
			{
				throw new BadImageFormatException();
			}
			return LookupTopLevelTypeDefSymbol(Module.GetAssemblyReferenceIndexOrThrow(assemblyRef), ref fullName);
		}
		case HandleKind.ModuleReference:
		{
			ModuleReferenceHandle moduleRef = (ModuleReferenceHandle)tokenResolutionScope;
			if (moduleRef.IsNil)
			{
				throw new BadImageFormatException();
			}
			return LookupTopLevelTypeDefSymbol(Module.GetModuleRefNameOrThrow(moduleRef), ref fullName, out isNoPiaLocalType);
		}
		default:
			if (tokenResolutionScope == EntityHandle.ModuleDefinition)
			{
				return LookupTopLevelTypeDefSymbol(ref fullName, out isNoPiaLocalType);
			}
			isNoPiaLocalType = false;
			return GetUnsupportedMetadataTypeSymbol();
		}
	}

	private TypeSymbol GetTypeOfTypeDef(TypeDefinitionHandle typeDef)
	{
		bool isNoPiaLocalType;
		return GetTypeOfTypeDef(typeDef, out isNoPiaLocalType, isContainingType: false);
	}

	private TypeSymbol GetTypeOfTypeDef(TypeDefinitionHandle typeDef, out bool isNoPiaLocalType, bool isContainingType)
	{
		try
		{
			ConcurrentDictionary<TypeDefinitionHandle, TypeSymbol> typeHandleToTypeMap = GetTypeHandleToTypeMap();
			if (typeHandleToTypeMap != null && typeHandleToTypeMap.TryGetValue(typeDef, out var value))
			{
				if (!Module.IsNestedTypeDefOrThrow(typeDef) && Module.IsNoPiaLocalType(typeDef))
				{
					isNoPiaLocalType = true;
				}
				else
				{
					isNoPiaLocalType = false;
				}
				return value;
			}
			string typeDefNameOrThrow = Module.GetTypeDefNameOrThrow(typeDef);
			MetadataTypeName emittedName;
			if (Module.IsNestedTypeDefOrThrow(typeDef))
			{
				TypeDefinitionHandle containingTypeOrThrow = Module.GetContainingTypeOrThrow(typeDef);
				if (containingTypeOrThrow.IsNil)
				{
					isNoPiaLocalType = false;
					return GetUnsupportedMetadataTypeSymbol();
				}
				TypeSymbol typeOfTypeDef = GetTypeOfTypeDef(containingTypeOrThrow, out isNoPiaLocalType, isContainingType: true);
				if (isNoPiaLocalType)
				{
					if (!isContainingType)
					{
						isNoPiaLocalType = false;
					}
					return GetUnsupportedMetadataTypeSymbol();
				}
				emittedName = MetadataTypeName.FromTypeName(typeDefNameOrThrow);
				return LookupNestedTypeDefSymbol(typeOfTypeDef, ref emittedName);
			}
			string typeDefNamespaceOrThrow = Module.GetTypeDefNamespaceOrThrow(typeDef);
			emittedName = ((typeDefNamespaceOrThrow.Length > 0) ? MetadataTypeName.FromNamespaceAndTypeName(typeDefNamespaceOrThrow, typeDefNameOrThrow) : MetadataTypeName.FromTypeName(typeDefNameOrThrow));
			if (Module.IsNoPiaLocalType(typeDef, out var interfaceGuid, out var scope, out var identifier))
			{
				isNoPiaLocalType = true;
				if (!Module.HasGenericParametersOrThrow(typeDef))
				{
					MetadataTypeName name = MetadataTypeName.FromNamespaceAndTypeName(emittedName.NamespaceName, emittedName.TypeName, useCLSCompliantNameArityEncoding: false, 0);
					return SubstituteNoPiaLocalType(typeDef, ref name, interfaceGuid, scope, identifier);
				}
				value = GetUnsupportedMetadataTypeSymbol();
				if (typeHandleToTypeMap != null)
				{
					value = typeHandleToTypeMap.GetOrAdd(typeDef, value);
				}
				return value;
			}
			isNoPiaLocalType = false;
			return LookupTopLevelTypeDefSymbol(ref emittedName, out isNoPiaLocalType);
		}
		catch (BadImageFormatException exception)
		{
			isNoPiaLocalType = false;
			return GetUnsupportedMetadataTypeSymbol(exception);
		}
	}

	private ImmutableArray<ModifierInfo<TypeSymbol>> DecodeModifiersOrThrow(ref BlobReader signatureReader, AllowedRequiredModifierType allowedRequiredModifierType, out SignatureTypeCode typeCode, out bool requiredModifierFound)
	{
		requiredModifierFound = false;
		ArrayBuilder<ModifierInfo<TypeSymbol>> arrayBuilder = null;
		while (true)
		{
			typeCode = signatureReader.ReadSignatureTypeCode();
			bool flag;
			if (typeCode == SignatureTypeCode.RequiredModifier)
			{
				flag = false;
			}
			else
			{
				if (typeCode != SignatureTypeCode.OptionalModifier)
				{
					break;
				}
				flag = true;
			}
			TypeSymbol val = DecodeModifierTypeOrThrow(ref signatureReader);
			if (!flag)
			{
				bool flag2 = false;
				switch (allowedRequiredModifierType)
				{
				case AllowedRequiredModifierType.System_Runtime_InteropServices_InAttribute:
					flag2 = IsAcceptedInAttributeModifierType(val);
					break;
				case AllowedRequiredModifierType.System_Runtime_CompilerServices_Volatile:
					flag2 = IsAcceptedVolatileModifierType(val);
					break;
				case AllowedRequiredModifierType.System_Runtime_InteropServices_UnmanagedType:
					flag2 = IsAcceptedUnmanagedTypeModifierType(val);
					break;
				}
				if (!flag2)
				{
					throw new UnsupportedSignatureContent();
				}
				requiredModifierFound = true;
			}
			ModifierInfo<TypeSymbol> item = new ModifierInfo<TypeSymbol>(flag, val);
			if (arrayBuilder == null)
			{
				arrayBuilder = ArrayBuilder<ModifierInfo<TypeSymbol>>.GetInstance();
			}
			arrayBuilder.Add(item);
		}
		return arrayBuilder?.ToImmutableAndFree() ?? default(ImmutableArray<ModifierInfo<TypeSymbol>>);
	}

	private TypeSymbol DecodeModifierTypeOrThrow(ref BlobReader signatureReader)
	{
		EntityHandle entityHandle = signatureReader.ReadTypeHandle();
		while (true)
		{
			bool isNoPiaLocalType;
			BlobReader ppSig;
			switch (entityHandle.Kind)
			{
			case HandleKind.TypeDefinition:
			{
				TypeSymbol typeOfTypeRef = GetTypeOfTypeDef((TypeDefinitionHandle)entityHandle, out isNoPiaLocalType, isContainingType: false);
				return SubstituteWithUnboundIfGeneric(typeOfTypeRef);
			}
			case HandleKind.TypeReference:
			{
				TypeSymbol typeOfTypeRef = GetTypeOfTypeRef((TypeReferenceHandle)entityHandle, out isNoPiaLocalType);
				return SubstituteWithUnboundIfGeneric(typeOfTypeRef);
			}
			case HandleKind.TypeSpecification:
			{
				ppSig = Module.GetTypeSpecificationSignatureReaderOrThrow((TypeSpecificationHandle)entityHandle);
				SignatureTypeCode signatureTypeCode = ppSig.ReadSignatureTypeCode();
				switch (signatureTypeCode)
				{
				case SignatureTypeCode.Void:
				case SignatureTypeCode.Boolean:
				case SignatureTypeCode.Char:
				case SignatureTypeCode.SByte:
				case SignatureTypeCode.Byte:
				case SignatureTypeCode.Int16:
				case SignatureTypeCode.UInt16:
				case SignatureTypeCode.Int32:
				case SignatureTypeCode.UInt32:
				case SignatureTypeCode.Int64:
				case SignatureTypeCode.UInt64:
				case SignatureTypeCode.Single:
				case SignatureTypeCode.Double:
				case SignatureTypeCode.String:
				case SignatureTypeCode.TypedReference:
				case SignatureTypeCode.IntPtr:
				case SignatureTypeCode.UIntPtr:
				case SignatureTypeCode.Object:
					return GetSpecialType(signatureTypeCode.ToSpecialType());
				case SignatureTypeCode.TypeHandle:
					break;
				case SignatureTypeCode.GenericTypeInstance:
				{
					bool refersToNoPiaLocalType;
					return DecodeGenericTypeInstanceOrThrow(ref ppSig, out refersToNoPiaLocalType);
				}
				default:
					throw new UnsupportedSignatureContent();
				}
				break;
			}
			default:
				throw new UnsupportedSignatureContent();
			}
			entityHandle = ppSig.ReadTypeHandle();
		}
	}

	internal ImmutableArray<LocalInfo<TypeSymbol>> DecodeLocalSignatureOrThrow(ref BlobReader signatureReader)
	{
		SignatureHeader signatureHeader = signatureReader.ReadSignatureHeader();
		if (signatureHeader.Kind != SignatureKind.LocalVariables)
		{
			throw new UnsupportedSignatureContent();
		}
		GetSignatureCountsOrThrow(ref signatureReader, signatureHeader, out var parameterCount, out var _);
		ArrayBuilder<LocalInfo<TypeSymbol>> instance = ArrayBuilder<LocalInfo<TypeSymbol>>.GetInstance(parameterCount);
		ArrayBuilder<int> instance2 = ArrayBuilder<int>.GetInstance(parameterCount);
		try
		{
			for (int i = 0; i < parameterCount; i++)
			{
				instance2.Add(signatureReader.Offset);
				instance.Add(DecodeLocalVariableOrThrow(ref signatureReader));
			}
			if (signatureReader.RemainingBytes > 0)
			{
				throw new UnsupportedSignatureContent();
			}
			signatureReader.Reset();
			for (int j = 0; j < parameterCount; j++)
			{
				int num = instance2[j];
				while (signatureReader.Offset < num)
				{
					signatureReader.ReadByte();
				}
				int byteCount = ((j < parameterCount - 1) ? (instance2[j + 1] - num) : signatureReader.RemainingBytes);
				byte[] signature = signatureReader.ReadBytes(byteCount);
				instance[j] = instance[j].WithSignature(signature);
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance2.Free();
			instance.Free();
		}
	}

	internal TypeSymbol DecodeGenericParameterConstraint(EntityHandle token, out bool isUnmanagedConstraint)
	{
		isUnmanagedConstraint = false;
		bool refersToNoPiaLocalType;
		switch (token.Kind)
		{
		case HandleKind.TypeSpecification:
			try
			{
				BlobReader signatureReader = Module.GetTypeSpecificationSignatureReaderOrThrow((TypeSpecificationHandle)token);
				ImmutableArray<ModifierInfo<TypeSymbol>> immutableArray = DecodeModifiersOrThrow(ref signatureReader, AllowedRequiredModifierType.System_Runtime_InteropServices_UnmanagedType, out var typeCode, out var requiredModifierFound);
				TypeSymbol val = DecodeTypeOrThrow(ref signatureReader, typeCode, out refersToNoPiaLocalType);
				if (requiredModifierFound)
				{
					if (val.SpecialType != SpecialType.System_ValueType || immutableArray.Length != 1)
					{
						return GetUnsupportedMetadataTypeSymbol();
					}
					isUnmanagedConstraint = true;
				}
				else if (!immutableArray.IsDefaultOrEmpty)
				{
					return GetUnsupportedMetadataTypeSymbol();
				}
				return val;
			}
			catch (BadImageFormatException exception)
			{
				return GetUnsupportedMetadataTypeSymbol(exception);
			}
			catch (UnsupportedSignatureContent)
			{
				return GetUnsupportedMetadataTypeSymbol();
			}
		case HandleKind.TypeReference:
			return GetTypeOfTypeRef((TypeReferenceHandle)token, out refersToNoPiaLocalType);
		case HandleKind.TypeDefinition:
			return GetTypeOfTypeDef((TypeDefinitionHandle)token);
		default:
			return GetUnsupportedMetadataTypeSymbol();
		}
	}

	internal LocalInfo<TypeSymbol> DecodeLocalVariableOrThrow(ref BlobReader signatureReader)
	{
		ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers = DecodeModifiersOrThrow(ref signatureReader, AllowedRequiredModifierType.None, out var typeCode, out var _);
		LocalSlotConstraints localSlotConstraints = LocalSlotConstraints.None;
		if (typeCode == SignatureTypeCode.Pinned)
		{
			localSlotConstraints |= LocalSlotConstraints.Pinned;
			typeCode = signatureReader.ReadSignatureTypeCode();
		}
		if (typeCode == SignatureTypeCode.ByReference)
		{
			localSlotConstraints |= LocalSlotConstraints.ByRef;
			typeCode = signatureReader.ReadSignatureTypeCode();
		}
		TypeSymbol type;
		if (typeCode == SignatureTypeCode.TypedReference && localSlotConstraints != LocalSlotConstraints.None)
		{
			type = GetUnsupportedMetadataTypeSymbol();
		}
		else
		{
			try
			{
				type = DecodeTypeOrThrow(ref signatureReader, typeCode, out var _);
			}
			catch (UnsupportedSignatureContent)
			{
				type = GetUnsupportedMetadataTypeSymbol();
			}
		}
		return new LocalInfo<TypeSymbol>(type, customModifiers, localSlotConstraints, null);
	}

	internal void DecodeLocalConstantBlobOrThrow(ref BlobReader sigReader, out TypeSymbol type, out ConstantValue value)
	{
		DecodeModifiersOrThrow(ref sigReader, AllowedRequiredModifierType.None, out var typeCode, out var _);
		if (typeCode == SignatureTypeCode.TypeHandle)
		{
			type = GetSymbolForTypeHandleOrThrow(sigReader.ReadTypeHandle(), out var _, allowTypeSpec: true, requireShortForm: true);
			if (type.SpecialType == SpecialType.System_Decimal)
			{
				value = ConstantValue.Create(sigReader.ReadDecimal());
			}
			else if (type.SpecialType == SpecialType.System_DateTime)
			{
				value = ConstantValue.Create(sigReader.ReadDateTime());
			}
			else if (sigReader.RemainingBytes == 0)
			{
				value = ((type.IsReferenceType || type is IPointerTypeSymbol) ? ConstantValue.Null : ConstantValue.Bad);
			}
			else
			{
				value = ConstantValue.Bad;
			}
			return;
		}
		value = DecodePrimitiveConstantValue(ref sigReader, typeCode, out var isEnumTypeCode);
		SpecialType specialType = typeCode.ToSpecialType();
		if (isEnumTypeCode && sigReader.RemainingBytes > 0)
		{
			type = GetSymbolForTypeHandleOrThrow(sigReader.ReadTypeHandle(), out var _, allowTypeSpec: true, requireShortForm: true);
			TypeSymbol enumUnderlyingType = GetEnumUnderlyingType(type);
			if (enumUnderlyingType == null || enumUnderlyingType.SpecialType != specialType)
			{
				throw new UnsupportedSignatureContent();
			}
		}
		else
		{
			type = GetSpecialType(specialType);
		}
		if (sigReader.RemainingBytes <= 0)
		{
			return;
		}
		throw new UnsupportedSignatureContent();
	}

	private static ConstantValue DecodePrimitiveConstantValue(ref BlobReader sigReader, SignatureTypeCode typeCode, out bool isEnumTypeCode)
	{
		switch (typeCode)
		{
		case SignatureTypeCode.Boolean:
			isEnumTypeCode = true;
			return ConstantValue.Create(sigReader.ReadBoolean());
		case SignatureTypeCode.Char:
			isEnumTypeCode = true;
			return ConstantValue.Create(sigReader.ReadChar());
		case SignatureTypeCode.SByte:
			isEnumTypeCode = true;
			return ConstantValue.Create(sigReader.ReadSByte());
		case SignatureTypeCode.Byte:
			isEnumTypeCode = true;
			return ConstantValue.Create(sigReader.ReadByte());
		case SignatureTypeCode.Int16:
			isEnumTypeCode = true;
			return ConstantValue.Create(sigReader.ReadInt16());
		case SignatureTypeCode.UInt16:
			isEnumTypeCode = true;
			return ConstantValue.Create(sigReader.ReadUInt16());
		case SignatureTypeCode.Int32:
			isEnumTypeCode = true;
			return ConstantValue.Create(sigReader.ReadInt32());
		case SignatureTypeCode.UInt32:
			isEnumTypeCode = true;
			return ConstantValue.Create(sigReader.ReadUInt32());
		case SignatureTypeCode.Int64:
			isEnumTypeCode = true;
			return ConstantValue.Create(sigReader.ReadInt64());
		case SignatureTypeCode.UInt64:
			isEnumTypeCode = true;
			return ConstantValue.Create(sigReader.ReadUInt64());
		case SignatureTypeCode.Single:
			isEnumTypeCode = false;
			return ConstantValue.Create(sigReader.ReadSingle());
		case SignatureTypeCode.Double:
			isEnumTypeCode = false;
			return ConstantValue.Create(sigReader.ReadDouble());
		case SignatureTypeCode.String:
			isEnumTypeCode = false;
			if (sigReader.RemainingBytes == 1)
			{
				if (sigReader.ReadByte() != byte.MaxValue)
				{
					return ConstantValue.Bad;
				}
				return ConstantValue.Null;
			}
			if (sigReader.RemainingBytes % 2 != 0)
			{
				return ConstantValue.Bad;
			}
			return ConstantValue.Create(sigReader.ReadUTF16(sigReader.RemainingBytes));
		case SignatureTypeCode.Object:
			isEnumTypeCode = false;
			return ConstantValue.Null;
		default:
			throw new UnsupportedSignatureContent();
		}
	}

	internal ImmutableArray<LocalInfo<TypeSymbol>> GetLocalsOrThrow(StandaloneSignatureHandle handle)
	{
		BlobHandle signature = Module.MetadataReader.GetStandaloneSignature(handle).Signature;
		BlobReader signatureReader = Module.MetadataReader.GetBlobReader(signature);
		return DecodeLocalSignatureOrThrow(ref signatureReader);
	}

	internal unsafe TypeSymbol DecodeLocalVariableTypeOrThrow(ImmutableArray<byte> signature)
	{
		if (signature.IsDefaultOrEmpty)
		{
			throw new UnsupportedSignatureContent();
		}
		fixed (byte* buffer = signature.DangerousGetUnderlyingArray())
		{
			BlobReader signatureReader = new BlobReader(buffer, signature.Length);
			LocalInfo<TypeSymbol> localInfo = DecodeLocalVariableOrThrow(ref signatureReader);
			if (localInfo.IsByRef || localInfo.IsPinned)
			{
				throw new UnsupportedSignatureContent();
			}
			return localInfo.Type;
		}
	}

	internal ImmutableArray<LocalInfo<TypeSymbol>> GetLocalInfo(StandaloneSignatureHandle localSignatureHandle)
	{
		if (localSignatureHandle.IsNil)
		{
			return ImmutableArray<LocalInfo<TypeSymbol>>.Empty;
		}
		MetadataReader metadataReader = Module.MetadataReader;
		BlobHandle signature = metadataReader.GetStandaloneSignature(localSignatureHandle).Signature;
		BlobReader signatureReader = metadataReader.GetBlobReader(signature);
		return DecodeLocalSignatureOrThrow(ref signatureReader);
	}

	private void DecodeParameterOrThrow(ref BlobReader signatureReader, ref ParamInfo<TypeSymbol> info)
	{
		info.CustomModifiers = DecodeModifiersOrThrow(ref signatureReader, AllowedRequiredModifierType.System_Runtime_InteropServices_InAttribute, out var typeCode, out var requiredModifierFound);
		bool requiredModifierFound2;
		if (typeCode == SignatureTypeCode.ByReference)
		{
			info.IsByRef = true;
			info.RefCustomModifiers = info.CustomModifiers;
			info.CustomModifiers = DecodeModifiersOrThrow(ref signatureReader, AllowedRequiredModifierType.None, out typeCode, out requiredModifierFound2);
		}
		else if (requiredModifierFound)
		{
			throw new UnsupportedSignatureContent();
		}
		info.Type = DecodeTypeOrThrow(ref signatureReader, typeCode, out requiredModifierFound2);
	}

	internal ParamInfo<TypeSymbol>[] GetSignatureForMethod(MethodDefinitionHandle methodDef, out SignatureHeader signatureHeader, out BadImageFormatException metadataException, bool setParamHandles = true)
	{
		ParamInfo<TypeSymbol>[] array = null;
		signatureHeader = default;
		try
		{
			BlobHandle methodSignatureOrThrow = Module.GetMethodSignatureOrThrow(methodDef);
			BlobReader signatureReader = DecodeSignatureHeaderOrThrow(methodSignatureOrThrow, out signatureHeader);
			array = DecodeSignatureParametersOrThrow(ref signatureReader, signatureHeader, out var _);
			if (setParamHandles)
			{
				int num = array.Length;
				foreach (ParameterHandle item in Module.GetParametersOfMethodOrThrow(methodDef))
				{
					int parameterSequenceNumberOrThrow = Module.GetParameterSequenceNumberOrThrow(item);
					if (parameterSequenceNumberOrThrow >= 0 && parameterSequenceNumberOrThrow < num && array[parameterSequenceNumberOrThrow].Handle.IsNil)
					{
						array[parameterSequenceNumberOrThrow].Handle = item;
					}
				}
			}
			metadataException = null;
		}
		catch (BadImageFormatException ex)
		{
			BadImageFormatException exception = (metadataException = ex);
			if (array == null)
			{
				array = new ParamInfo<TypeSymbol>[1];
				array[0].Type = GetUnsupportedMetadataTypeSymbol(exception);
			}
		}
		return array;
	}

	internal static void GetSignatureCountsOrThrow(PEModule module, MethodDefinitionHandle methodDef, out int parameterCount, out int typeParameterCount)
	{
		BlobHandle methodSignatureOrThrow = module.GetMethodSignatureOrThrow(methodDef);
		BlobReader signatureReader = DecodeSignatureHeaderOrThrow(module, methodSignatureOrThrow, out var signatureHeader);
		GetSignatureCountsOrThrow(ref signatureReader, signatureHeader, out parameterCount, out typeParameterCount);
	}

	internal ParamInfo<TypeSymbol>[] GetSignatureForProperty(PropertyDefinitionHandle handle, out SignatureHeader signatureHeader, out BadImageFormatException BadImageFormatException)
	{
		ParamInfo<TypeSymbol>[] array = null;
		signatureHeader = default;
		try
		{
			BlobHandle propertySignatureOrThrow = Module.GetPropertySignatureOrThrow(handle);
			BlobReader signatureReader = DecodeSignatureHeaderOrThrow(propertySignatureOrThrow, out signatureHeader);
			array = DecodeSignatureParametersOrThrow(ref signatureReader, signatureHeader, out var _);
			BadImageFormatException = null;
		}
		catch (BadImageFormatException ex)
		{
			BadImageFormatException exception = (BadImageFormatException = ex);
			if (array == null)
			{
				array = new ParamInfo<TypeSymbol>[1];
				array[0].Type = GetUnsupportedMetadataTypeSymbol(exception);
			}
		}
		return array;
	}

	internal SignatureHeader GetSignatureHeaderForProperty(PropertyDefinitionHandle handle)
	{
		try
		{
			BlobHandle propertySignatureOrThrow = Module.GetPropertySignatureOrThrow(handle);
			DecodeSignatureHeaderOrThrow(propertySignatureOrThrow, out var signatureHeader);
			return signatureHeader;
		}
		catch (BadImageFormatException)
		{
			return default;
		}
	}

	private void DecodeCustomAttributeParameterTypeOrThrow(ref BlobReader sigReader, out SerializationTypeCode typeCode, out TypeSymbol type, out SerializationTypeCode elementTypeCode, out TypeSymbol elementType, bool isElementType)
	{
		SignatureTypeCode signatureTypeCode = sigReader.ReadSignatureTypeCode();
		if (signatureTypeCode == SignatureTypeCode.SZArray)
		{
			if (isElementType)
			{
				throw new UnsupportedSignatureContent();
			}
			DecodeCustomAttributeParameterTypeOrThrow(ref sigReader, out elementTypeCode, out elementType, out var _, out var _, isElementType: true);
			type = GetSZArrayTypeSymbol(elementType, default);
			typeCode = SerializationTypeCode.SZArray;
			return;
		}
		elementTypeCode = SerializationTypeCode.Invalid;
		elementType = null;
		switch (signatureTypeCode)
		{
		case SignatureTypeCode.Object:
			type = GetSpecialType(SpecialType.System_Object);
			typeCode = SerializationTypeCode.TaggedObject;
			return;
		case SignatureTypeCode.Boolean:
		case SignatureTypeCode.Char:
		case SignatureTypeCode.SByte:
		case SignatureTypeCode.Byte:
		case SignatureTypeCode.Int16:
		case SignatureTypeCode.UInt16:
		case SignatureTypeCode.Int32:
		case SignatureTypeCode.UInt32:
		case SignatureTypeCode.Int64:
		case SignatureTypeCode.UInt64:
		case SignatureTypeCode.Single:
		case SignatureTypeCode.Double:
		case SignatureTypeCode.String:
			type = GetSpecialType(signatureTypeCode.ToSpecialType());
			typeCode = (SerializationTypeCode)signatureTypeCode;
			return;
		case SignatureTypeCode.TypeHandle:
		{
			type = GetSymbolForTypeHandleOrThrow(sigReader.ReadTypeHandle(), out var _, allowTypeSpec: true, requireShortForm: true);
			TypeSymbol enumUnderlyingType = GetEnumUnderlyingType(type);
			if (enumUnderlyingType != null)
			{
				typeCode = enumUnderlyingType.SpecialType.ToSerializationType();
				return;
			}
			if (type == SystemTypeSymbol)
			{
				typeCode = SerializationTypeCode.Type;
				return;
			}
			break;
		}
		}
		throw new UnsupportedSignatureContent();
	}

	private void DecodeCustomAttributeFieldOrPropTypeOrThrow(ref BlobReader argReader, out SerializationTypeCode typeCode, out TypeSymbol type, out SerializationTypeCode elementTypeCode, out TypeSymbol elementType, bool isElementType)
	{
		typeCode = argReader.ReadSerializationTypeCode();
		if (typeCode == SerializationTypeCode.SZArray)
		{
			if (isElementType)
			{
				throw new UnsupportedSignatureContent();
			}
			DecodeCustomAttributeFieldOrPropTypeOrThrow(ref argReader, out elementTypeCode, out elementType, out var _, out var _, isElementType: true);
			type = GetSZArrayTypeSymbol(elementType, default);
			return;
		}
		elementTypeCode = SerializationTypeCode.Invalid;
		elementType = null;
		switch (typeCode)
		{
		case SerializationTypeCode.TaggedObject:
			type = GetSpecialType(SpecialType.System_Object);
			break;
		case SerializationTypeCode.Enum:
		{
			if (!PEModule.CrackStringInAttributeValue(out var value, ref argReader))
			{
				throw new UnsupportedSignatureContent();
			}
			type = GetTypeSymbolForSerializedType(value);
			TypeSymbol enumUnderlyingType = GetEnumUnderlyingType(type);
			if (enumUnderlyingType == null)
			{
				throw new UnsupportedSignatureContent();
			}
			typeCode = enumUnderlyingType.SpecialType.ToSerializationType();
			break;
		}
		case SerializationTypeCode.Type:
			type = SystemTypeSymbol;
			break;
		case SerializationTypeCode.Boolean:
		case SerializationTypeCode.Char:
		case SerializationTypeCode.SByte:
		case SerializationTypeCode.Byte:
		case SerializationTypeCode.Int16:
		case SerializationTypeCode.UInt16:
		case SerializationTypeCode.Int32:
		case SerializationTypeCode.UInt32:
		case SerializationTypeCode.Int64:
		case SerializationTypeCode.UInt64:
		case SerializationTypeCode.Single:
		case SerializationTypeCode.Double:
		case SerializationTypeCode.String:
			type = GetSpecialType(((SignatureTypeCode)typeCode).ToSpecialType());
			break;
		default:
			throw new UnsupportedSignatureContent();
		}
	}

	private TypedConstant DecodeCustomAttributeFixedArgumentOrThrow(ref BlobReader sigReader, ref BlobReader argReader)
	{
		DecodeCustomAttributeParameterTypeOrThrow(ref sigReader, out var typeCode, out var type, out var elementTypeCode, out var elementType, isElementType: false);
		if (typeCode == SerializationTypeCode.SZArray)
		{
			return DecodeCustomAttributeElementArrayOrThrow(ref argReader, elementTypeCode, elementType, type);
		}
		return DecodeCustomAttributeElementOrThrow(ref argReader, typeCode, type);
	}

	private TypedConstant DecodeCustomAttributeElementOrThrow(ref BlobReader argReader, SerializationTypeCode typeCode, TypeSymbol type)
	{
		if (typeCode == SerializationTypeCode.TaggedObject)
		{
			DecodeCustomAttributeFieldOrPropTypeOrThrow(ref argReader, out typeCode, out type, out var elementTypeCode, out var elementType, isElementType: false);
			if (typeCode == SerializationTypeCode.SZArray)
			{
				return DecodeCustomAttributeElementArrayOrThrow(ref argReader, elementTypeCode, elementType, type);
			}
		}
		return DecodeCustomAttributePrimitiveElementOrThrow(ref argReader, typeCode, type);
	}

	private TypedConstant DecodeCustomAttributeElementArrayOrThrow(ref BlobReader argReader, SerializationTypeCode elementTypeCode, TypeSymbol elementType, TypeSymbol arrayType)
	{
		int num = argReader.ReadInt32();
		TypedConstant[] array;
		switch (num)
		{
		case -1:
			array = null;
			break;
		case 0:
			array = Array.Empty<TypedConstant>();
			break;
		default:
		{
			array = new TypedConstant[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = DecodeCustomAttributeElementOrThrow(ref argReader, elementTypeCode, elementType);
			}
			break;
		}
		}
		return CreateArrayTypedConstant(arrayType, array.AsImmutableOrNull());
	}

	private TypedConstant DecodeCustomAttributePrimitiveElementOrThrow(ref BlobReader argReader, SerializationTypeCode typeCode, TypeSymbol type)
	{
		switch (typeCode)
		{
		case SerializationTypeCode.Boolean:
			return CreateTypedConstant(type, GetPrimitiveOrEnumTypedConstantKind(type), argReader.ReadSByte() != 0);
		case SerializationTypeCode.SByte:
			return CreateTypedConstant(type, GetPrimitiveOrEnumTypedConstantKind(type), argReader.ReadSByte());
		case SerializationTypeCode.Byte:
			return CreateTypedConstant(type, GetPrimitiveOrEnumTypedConstantKind(type), argReader.ReadByte());
		case SerializationTypeCode.Int16:
			return CreateTypedConstant(type, GetPrimitiveOrEnumTypedConstantKind(type), argReader.ReadInt16());
		case SerializationTypeCode.UInt16:
			return CreateTypedConstant(type, GetPrimitiveOrEnumTypedConstantKind(type), argReader.ReadUInt16());
		case SerializationTypeCode.Int32:
			return CreateTypedConstant(type, GetPrimitiveOrEnumTypedConstantKind(type), argReader.ReadInt32());
		case SerializationTypeCode.UInt32:
			return CreateTypedConstant(type, GetPrimitiveOrEnumTypedConstantKind(type), argReader.ReadUInt32());
		case SerializationTypeCode.Int64:
			return CreateTypedConstant(type, GetPrimitiveOrEnumTypedConstantKind(type), argReader.ReadInt64());
		case SerializationTypeCode.UInt64:
			return CreateTypedConstant(type, GetPrimitiveOrEnumTypedConstantKind(type), argReader.ReadUInt64());
		case SerializationTypeCode.Single:
			return CreateTypedConstant(type, GetPrimitiveOrEnumTypedConstantKind(type), argReader.ReadSingle());
		case SerializationTypeCode.Double:
			return CreateTypedConstant(type, GetPrimitiveOrEnumTypedConstantKind(type), argReader.ReadDouble());
		case SerializationTypeCode.Char:
			return CreateTypedConstant(type, GetPrimitiveOrEnumTypedConstantKind(type), argReader.ReadChar());
		case SerializationTypeCode.String:
		{
			TypedConstantKind kind = (PEModule.CrackStringInAttributeValue(out var value3, ref argReader) ? TypedConstantKind.Primitive : TypedConstantKind.Error);
			return CreateTypedConstant(type, kind, value3);
		}
		case SerializationTypeCode.Type:
		{
			TypeSymbol value2;
			if (PEModule.CrackStringInAttributeValue(out var value, ref argReader))
			{
				value2 = ((value != null) ? GetTypeSymbolForSerializedType(value) : null);
			}
			else
			{
				value2 = GetUnsupportedMetadataTypeSymbol();
			}
			return CreateTypedConstant(type, TypedConstantKind.Type, value2);
		}
		default:
			throw new UnsupportedSignatureContent();
		}
	}

	private static TypedConstantKind GetPrimitiveOrEnumTypedConstantKind(TypeSymbol type)
	{
		if (type.TypeKind != TypeKind.Enum)
		{
			return TypedConstantKind.Primitive;
		}
		return TypedConstantKind.Enum;
	}

	private KeyValuePair<string, TypedConstant> DecodeCustomAttributeNamedArgumentOrThrow(ref BlobReader argReader)
	{
		CustomAttributeNamedArgumentKind customAttributeNamedArgumentKind = (CustomAttributeNamedArgumentKind)argReader.ReadCompressedInteger();
		if (customAttributeNamedArgumentKind != CustomAttributeNamedArgumentKind.Field && customAttributeNamedArgumentKind != CustomAttributeNamedArgumentKind.Property)
		{
			throw new UnsupportedSignatureContent();
		}
		DecodeCustomAttributeFieldOrPropTypeOrThrow(ref argReader, out var typeCode, out var type, out var elementTypeCode, out var elementType, isElementType: false);
		if (!PEModule.CrackStringInAttributeValue(out var value, ref argReader))
		{
			throw new UnsupportedSignatureContent();
		}
		TypedConstant value2 = ((typeCode == SerializationTypeCode.SZArray) ? DecodeCustomAttributeElementArrayOrThrow(ref argReader, elementTypeCode, elementType, type) : DecodeCustomAttributeElementOrThrow(ref argReader, typeCode, type));
		return new KeyValuePair<string, TypedConstant>(value, value2);
	}

	internal bool IsTargetAttribute(CustomAttributeHandle customAttribute, string namespaceName, string typeName, bool ignoreCase = false)
	{
		try
		{
			EntityHandle ctor;
			return Module.IsTargetAttribute(customAttribute, namespaceName, typeName, out ctor, ignoreCase);
		}
		catch (BadImageFormatException)
		{
			return false;
		}
	}

	internal int GetTargetAttributeSignatureIndex(CustomAttributeHandle customAttribute, AttributeDescription description)
	{
		try
		{
			return Module.GetTargetAttributeSignatureIndex(customAttribute, description);
		}
		catch (BadImageFormatException)
		{
			return -1;
		}
	}

	internal bool GetCustomAttribute(CustomAttributeHandle handle, out TypedConstant[] positionalArgs, out KeyValuePair<string, TypedConstant>[] namedArgs)
	{
		try
		{
			positionalArgs = Array.Empty<TypedConstant>();
			namedArgs = Array.Empty<KeyValuePair<string, TypedConstant>>();
			if (Module.GetTypeAndConstructor(handle, out var _, out var attributeCtor))
			{
				BlobReader argReader = Module.GetMemoryReaderOrThrow(Module.GetCustomAttributeValueOrThrow(handle));
				BlobReader sigReader = Module.GetMemoryReaderOrThrow(Module.GetMethodSignatureOrThrow(attributeCtor));
				if (argReader.ReadUInt16() != 1)
				{
					return false;
				}
				if (sigReader.ReadSignatureHeader().IsGeneric && sigReader.ReadCompressedInteger() != 0)
				{
					return false;
				}
				int num = sigReader.ReadCompressedInteger();
				if (sigReader.ReadSignatureTypeCode() != SignatureTypeCode.Void)
				{
					return false;
				}
				if (num > 0)
				{
					positionalArgs = new TypedConstant[num];
					for (int i = 0; i < positionalArgs.Length; i++)
					{
						positionalArgs[i] = DecodeCustomAttributeFixedArgumentOrThrow(ref sigReader, ref argReader);
					}
				}
				short num2 = argReader.ReadInt16();
				if (num2 > 0)
				{
					namedArgs = new KeyValuePair<string, TypedConstant>[num2];
					for (int j = 0; j < namedArgs.Length; j++)
					{
						namedArgs[j] = DecodeCustomAttributeNamedArgumentOrThrow(ref argReader);
					}
				}
				return true;
			}
		}
		catch (Exception ex) when (ex is UnsupportedSignatureContent || ex is BadImageFormatException)
		{
			positionalArgs = Array.Empty<TypedConstant>();
			namedArgs = Array.Empty<KeyValuePair<string, TypedConstant>>();
		}
		return false;
	}

	internal bool GetCustomAttribute(CustomAttributeHandle handle, out TypeSymbol attributeClass, out MethodSymbol attributeCtor)
	{
		EntityHandle ctorType;
		EntityHandle attributeCtor2;
		try
		{
			if (!Module.GetTypeAndConstructor(handle, out ctorType, out attributeCtor2))
			{
				attributeClass = null;
				attributeCtor = null;
				return false;
			}
		}
		catch (BadImageFormatException)
		{
			attributeClass = null;
			attributeCtor = null;
			return false;
		}
		attributeClass = GetTypeOfToken(ctorType);
		attributeCtor = GetMethodSymbolForMethodDefOrMemberRef(attributeCtor2, attributeClass);
		return true;
	}

	internal bool GetCustomAttributeWellKnownType(CustomAttributeHandle handle, out WellKnownType wellKnownAttribute)
	{
		wellKnownAttribute = WellKnownType.Unknown;
		try
		{
			if (!Module.GetTypeAndConstructor(handle, out var ctorType, out var _))
			{
				return false;
			}
			if (!Module.GetAttributeNamespaceAndName(ctorType, out var namespaceHandle, out var nameHandle))
			{
				return false;
			}
			string fullNameOrThrow = Module.GetFullNameOrThrow(namespaceHandle, nameHandle);
			wellKnownAttribute = WellKnownTypes.GetTypeFromMetadataName(fullNameOrThrow);
			return true;
		}
		catch (BadImageFormatException)
		{
			return false;
		}
	}

	private TypeSymbol[] DecodeMethodSpecTypeArgumentsOrThrow(BlobHandle signature)
	{
		BlobReader ppSig = DecodeSignatureHeaderOrThrow(signature, out var signatureHeader);
		if (signatureHeader.Kind != SignatureKind.MethodSpecification)
		{
			throw new BadImageFormatException();
		}
		int num = ppSig.ReadCompressedInteger();
		if (num == 0)
		{
			throw new BadImageFormatException();
		}
		TypeSymbol[] array = new TypeSymbol[num];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = DecodeTypeOrThrow(ref ppSig, out var _);
		}
		return array;
	}

	internal BlobReader DecodeSignatureHeaderOrThrow(BlobHandle signature, out SignatureHeader signatureHeader)
	{
		return DecodeSignatureHeaderOrThrow(Module, signature, out signatureHeader);
	}

	internal static BlobReader DecodeSignatureHeaderOrThrow(PEModule module, BlobHandle signature, out SignatureHeader signatureHeader)
	{
		BlobReader memoryReaderOrThrow = module.GetMemoryReaderOrThrow(signature);
		signatureHeader = memoryReaderOrThrow.ReadSignatureHeader();
		return memoryReaderOrThrow;
	}

	protected ParamInfo<TypeSymbol>[] DecodeSignatureParametersOrThrow(ref BlobReader signatureReader, SignatureHeader signatureHeader, out int typeParameterCount)
	{
		GetSignatureCountsOrThrow(ref signatureReader, signatureHeader, out var parameterCount, out typeParameterCount);
		ParamInfo<TypeSymbol>[] array = new ParamInfo<TypeSymbol>[parameterCount + 1];
		uint num = 0u;
		try
		{
			DecodeParameterOrThrow(ref signatureReader, ref array[0]);
			for (num = 1u; num <= parameterCount; num++)
			{
				DecodeParameterOrThrow(ref signatureReader, ref array[num]);
			}
			if (signatureReader.RemainingBytes > 0)
			{
				throw new UnsupportedSignatureContent();
			}
		}
		catch (Exception ex) when (ex is UnsupportedSignatureContent || ex is BadImageFormatException)
		{
			for (; num <= parameterCount; num++)
			{
				array[num].Type = GetUnsupportedMetadataTypeSymbol(ex as BadImageFormatException);
			}
		}
		return array;
	}

	private static void GetSignatureCountsOrThrow(ref BlobReader signatureReader, SignatureHeader signatureHeader, out int parameterCount, out int typeParameterCount)
	{
		typeParameterCount = (signatureHeader.IsGeneric ? signatureReader.ReadCompressedInteger() : 0);
		parameterCount = signatureReader.ReadCompressedInteger();
	}

	internal TypeSymbol DecodeFieldSignature(FieldDefinitionHandle fieldHandle, out bool isVolatile, out ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers)
	{
		try
		{
			BlobHandle fieldSignatureOrThrow = Module.GetFieldSignatureOrThrow(fieldHandle);
			BlobReader signatureReader = DecodeSignatureHeaderOrThrow(fieldSignatureOrThrow, out var signatureHeader);
			if (signatureHeader.Kind != SignatureKind.Field)
			{
				isVolatile = false;
				customModifiers = default;
				return GetUnsupportedMetadataTypeSymbol();
			}
			return DecodeFieldSignature(ref signatureReader, out isVolatile, out customModifiers);
		}
		catch (BadImageFormatException exception)
		{
			isVolatile = false;
			customModifiers = default;
			return GetUnsupportedMetadataTypeSymbol(exception);
		}
	}

	protected TypeSymbol DecodeFieldSignature(ref BlobReader signatureReader, out bool isVolatile, out ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers)
	{
		isVolatile = false;
		customModifiers = default;
		try
		{
			customModifiers = DecodeModifiersOrThrow(ref signatureReader, AllowedRequiredModifierType.System_Runtime_CompilerServices_Volatile, out var typeCode, out isVolatile);
			bool refersToNoPiaLocalType;
			return DecodeTypeOrThrow(ref signatureReader, typeCode, out refersToNoPiaLocalType);
		}
		catch (UnsupportedSignatureContent)
		{
			return GetUnsupportedMetadataTypeSymbol();
		}
		catch (BadImageFormatException exception)
		{
			return GetUnsupportedMetadataTypeSymbol(exception);
		}
	}

	internal ImmutableArray<MethodSymbol> GetExplicitlyOverriddenMethods(TypeDefinitionHandle implementingTypeDef, MethodDefinitionHandle implementingMethodDef, TypeSymbol implementingTypeSymbol)
	{
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		try
		{
			foreach (MethodImplementationHandle item in Module.GetMethodImplementationsOrThrow(implementingTypeDef))
			{
				Module.GetMethodImplPropsOrThrow(item, out var body, out var declaration);
				if (body.Kind == HandleKind.MemberReference)
				{
					MethodSymbol methodSymbolForMemberRef = GetMethodSymbolForMemberRef((MemberReferenceHandle)body, implementingTypeSymbol);
					if (methodSymbolForMemberRef != null)
					{
						body = GetMethodHandle(methodSymbolForMemberRef);
					}
				}
				if (body == implementingMethodDef && !declaration.IsNil)
				{
					HandleKind kind = declaration.Kind;
					MethodSymbol val = null;
					switch (kind)
					{
					case HandleKind.MethodDefinition:
						val = FindMethodSymbolInSuperType(implementingTypeDef, (MethodDefinitionHandle)declaration);
						break;
					case HandleKind.MemberReference:
						val = GetMethodSymbolForMemberRef((MemberReferenceHandle)declaration, implementingTypeSymbol);
						break;
					}
					if (val != null)
					{
						instance.Add(val);
					}
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		return instance.ToImmutableAndFree();
	}

	private MethodSymbol FindMethodSymbolInSuperType(TypeDefinitionHandle searchTypeDef, MethodDefinitionHandle targetMethodDef)
	{
		try
		{
			Queue<TypeDefinitionHandle> queue = new Queue<TypeDefinitionHandle>();
			Queue<TypeSymbol> queue2 = new Queue<TypeSymbol>();
			EnqueueTypeDefInterfacesAndBaseTypeOrThrow(queue, queue2, searchTypeDef);
			HashSet<TypeDefinitionHandle> hashSet = new HashSet<TypeDefinitionHandle>();
			HashSet<TypeSymbol> hashSet2 = new HashSet<TypeSymbol>();
			bool flag;
			while ((flag = queue.Count > 0) || queue2.Count > 0)
			{
				if (flag)
				{
					TypeDefinitionHandle typeDefinitionHandle = queue.Dequeue();
					if (hashSet.Contains(typeDefinitionHandle))
					{
						continue;
					}
					hashSet.Add(typeDefinitionHandle);
					foreach (MethodDefinitionHandle item in Module.GetMethodsOfTypeOrThrow(typeDefinitionHandle))
					{
						if (item == targetMethodDef)
						{
							TypeSymbol typeOfToken = GetTypeOfToken(typeDefinitionHandle);
							return FindMethodSymbolInType(typeOfToken, targetMethodDef);
						}
					}
					EnqueueTypeDefInterfacesAndBaseTypeOrThrow(queue, queue2, typeDefinitionHandle);
				}
				else
				{
					TypeSymbol val = queue2.Dequeue();
					if (!hashSet2.Contains(val))
					{
						hashSet2.Add(val);
						EnqueueTypeSymbolInterfacesAndBaseTypes(queue, queue2, val);
					}
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		return null;
	}

	private void EnqueueTypeDefInterfacesAndBaseTypeOrThrow(Queue<TypeDefinitionHandle> typeDefsToSearch, Queue<TypeSymbol> typeSymbolsToSearch, TypeDefinitionHandle searchTypeDef)
	{
		foreach (InterfaceImplementationHandle item in Module.GetInterfaceImplementationsOrThrow(searchTypeDef))
		{
			EnqueueTypeToken(typeDefsToSearch, typeSymbolsToSearch, Module.MetadataReader.GetInterfaceImplementation(item).Interface);
		}
		EnqueueTypeToken(typeDefsToSearch, typeSymbolsToSearch, Module.GetBaseTypeOfTypeOrThrow(searchTypeDef));
	}

	private void EnqueueTypeToken(Queue<TypeDefinitionHandle> typeDefsToSearch, Queue<TypeSymbol> typeSymbolsToSearch, EntityHandle typeToken)
	{
		if (!typeToken.IsNil)
		{
			if (typeToken.Kind == HandleKind.TypeDefinition)
			{
				typeDefsToSearch.Enqueue((TypeDefinitionHandle)typeToken);
			}
			else
			{
				EnqueueTypeSymbol(typeDefsToSearch, typeSymbolsToSearch, GetTypeOfToken(typeToken));
			}
		}
	}

	protected abstract void EnqueueTypeSymbolInterfacesAndBaseTypes(Queue<TypeDefinitionHandle> typeDefsToSearch, Queue<TypeSymbol> typeSymbolsToSearch, TypeSymbol typeSymbol);

	protected abstract void EnqueueTypeSymbol(Queue<TypeDefinitionHandle> typeDefsToSearch, Queue<TypeSymbol> typeSymbolsToSearch, TypeSymbol typeSymbol);

	protected abstract MethodSymbol FindMethodSymbolInType(TypeSymbol type, MethodDefinitionHandle methodDef);

	protected abstract FieldSymbol FindFieldSymbolInType(TypeSymbol type, FieldDefinitionHandle fieldDef);

	internal abstract Symbol GetSymbolForMemberRef(MemberReferenceHandle memberRef, TypeSymbol implementingTypeSymbol = null, bool methodsOnly = false);

	internal MethodSymbol GetMethodSymbolForMemberRef(MemberReferenceHandle methodRef, TypeSymbol implementingTypeSymbol)
	{
		return (MethodSymbol)GetSymbolForMemberRef(methodRef, implementingTypeSymbol, methodsOnly: true);
	}

	internal FieldSymbol GetFieldSymbolForMemberRef(MemberReferenceHandle methodRef, TypeSymbol implementingTypeSymbol)
	{
		return (FieldSymbol)GetSymbolForMemberRef(methodRef, implementingTypeSymbol, methodsOnly: true);
	}

	protected override bool IsContainingAssembly(AssemblyIdentity identity)
	{
		if (_containingAssemblyIdentity != null)
		{
			return _containingAssemblyIdentity.Equals(identity);
		}
		return false;
	}

	protected abstract MethodDefinitionHandle GetMethodHandle(MethodSymbol method);

	protected abstract ConcurrentDictionary<TypeDefinitionHandle, TypeSymbol> GetTypeHandleToTypeMap();

	protected abstract ConcurrentDictionary<TypeReferenceHandle, TypeSymbol> GetTypeRefHandleToTypeMap();

	protected abstract TypeSymbol SubstituteNoPiaLocalType(TypeDefinitionHandle typeDef, ref MetadataTypeName name, string interfaceGuid, string scope, string identifier);

	protected abstract TypeSymbol LookupTopLevelTypeDefSymbol(string moduleName, ref MetadataTypeName emittedName, out bool isNoPiaLocalType);

	protected abstract TypeSymbol GetGenericTypeParamSymbol(int position);

	protected abstract TypeSymbol GetGenericMethodTypeParamSymbol(int position);

	private static TypedConstant CreateArrayTypedConstant(TypeSymbol type, ImmutableArray<TypedConstant> array)
	{
		if (type.TypeKind == TypeKind.Error)
		{
			return new TypedConstant(type, TypedConstantKind.Error, null);
		}
		return new TypedConstant(type, array);
	}

	private static TypedConstant CreateTypedConstant(TypeSymbol type, TypedConstantKind kind, object value)
	{
		if (type.TypeKind == TypeKind.Error)
		{
			return new TypedConstant(type, TypedConstantKind.Error, null);
		}
		return new TypedConstant(type, kind, value);
	}

	private static TypedConstant CreateTypedConstant(TypeSymbol type, TypedConstantKind kind, bool value)
	{
		return CreateTypedConstant(type, kind, Boxes.Box(value));
	}

	internal Symbol GetSymbolForILToken(EntityHandle token)
	{
		try
		{
			switch (token.Kind)
			{
			case HandleKind.TypeReference:
			case HandleKind.TypeDefinition:
			case HandleKind.TypeSpecification:
				return (Symbol)GetTypeOfToken(token);
			case HandleKind.MethodDefinition:
			{
				TypeDefinitionHandle typeDef = Module.FindContainingTypeOrThrow((MethodDefinitionHandle)token);
				if (typeDef.IsNil)
				{
					return null;
				}
				TypeSymbol typeOfTypeDef = GetTypeOfTypeDef(typeDef);
				if (typeOfTypeDef == null)
				{
					return null;
				}
				return (Symbol)GetMethodSymbolForMethodDefOrMemberRef(token, typeOfTypeDef);
			}
			case HandleKind.FieldDefinition:
			{
				TypeDefinitionHandle typeDefinitionHandle = Module.FindContainingTypeOrThrow((FieldDefinitionHandle)token);
				if (typeDefinitionHandle.IsNil)
				{
					return null;
				}
				TypeSymbol typeOfToken = GetTypeOfToken(typeDefinitionHandle);
				if (typeOfToken == null)
				{
					return null;
				}
				return (Symbol)GetFieldSymbolForFieldDefOrMemberRef(token, typeOfToken);
			}
			case HandleKind.MethodSpecification:
			{
				Module.GetMethodSpecificationOrThrow((MethodSpecificationHandle)token, out var method, out var instantiation);
				MethodSymbol val = (MethodSymbol)GetSymbolForILToken(method);
				if (val == null)
				{
					return null;
				}
				TypeSymbol[] array = DecodeMethodSpecTypeArgumentsOrThrow(instantiation);
				ITypeSymbol[] typeArguments = array;
				return (Symbol)(MethodSymbol)val.Construct(typeArguments);
			}
			case HandleKind.MemberReference:
				return GetSymbolForMemberRef((MemberReferenceHandle)token);
			}
		}
		catch (BadImageFormatException)
		{
		}
		return null;
	}

	internal TypeSymbol GetMemberRefTypeSymbol(MemberReferenceHandle memberRef)
	{
		try
		{
			EntityHandle containingTypeOrThrow = Module.GetContainingTypeOrThrow(memberRef);
			HandleKind kind = containingTypeOrThrow.Kind;
			if (kind != HandleKind.TypeDefinition && kind != HandleKind.TypeReference && kind != HandleKind.TypeSpecification)
			{
				return null;
			}
			return GetTypeOfToken(containingTypeOrThrow);
		}
		catch (BadImageFormatException)
		{
			return null;
		}
	}

	internal MethodSymbol GetMethodSymbolForMethodDefOrMemberRef(EntityHandle memberToken, TypeSymbol container)
	{
		if (memberToken.Kind != HandleKind.MethodDefinition)
		{
			return GetMethodSymbolForMemberRef((MemberReferenceHandle)memberToken, container);
		}
		return FindMethodSymbolInType(container, (MethodDefinitionHandle)memberToken);
	}

	internal FieldSymbol GetFieldSymbolForFieldDefOrMemberRef(EntityHandle memberToken, TypeSymbol container)
	{
		if (memberToken.Kind != HandleKind.FieldDefinition)
		{
			return GetFieldSymbolForMemberRef((MemberReferenceHandle)memberToken, container);
		}
		return FindFieldSymbolInType(container, (FieldDefinitionHandle)memberToken);
	}

	internal bool DoPropertySignaturesMatch(ParamInfo<TypeSymbol>[] signature1, ParamInfo<TypeSymbol>[] signature2, bool comparingToSetter, bool compareParamByRef, bool compareReturnType)
	{
		int num = (comparingToSetter ? 1 : 0);
		if (signature2.Length - num != signature1.Length)
		{
			return false;
		}
		if (comparingToSetter && GetPrimitiveTypeCode(signature2[0].Type) != Microsoft.Cci.PrimitiveTypeCode.Void)
		{
			return false;
		}
		for (int i = ((!compareReturnType) ? 1 : 0); i < signature1.Length; i++)
		{
			int num2 = (((i == 0) & comparingToSetter) ? signature1.Length : i);
			ParamInfo<TypeSymbol> paramInfo = signature1[i];
			ParamInfo<TypeSymbol> paramInfo2 = signature2[num2];
			if (compareParamByRef && paramInfo2.IsByRef != paramInfo.IsByRef)
			{
				return false;
			}
			if (!paramInfo2.Type.Equals(paramInfo.Type))
			{
				return false;
			}
		}
		return true;
	}

	internal bool DoesSignatureMatchEvent(TypeSymbol eventType, ParamInfo<TypeSymbol>[] methodParams)
	{
		if (methodParams.Length != 2)
		{
			return false;
		}
		if (GetPrimitiveTypeCode(methodParams[0].Type) != Microsoft.Cci.PrimitiveTypeCode.Void)
		{
			return false;
		}
		ParamInfo<TypeSymbol> paramInfo = methodParams[1];
		if (!paramInfo.IsByRef)
		{
			return paramInfo.Type.Equals(eventType);
		}
		return false;
	}
}
