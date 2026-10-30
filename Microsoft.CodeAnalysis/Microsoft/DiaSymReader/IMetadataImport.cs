using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.DiaSymReader;

[ComImport]
[ComVisible(false)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44")]
internal interface IMetadataImport
{
	[PreserveSig]
	unsafe void CloseEnum(void* enumHandle);

	[PreserveSig]
	unsafe int CountEnum(void* enumHandle, out int count);

	[PreserveSig]
	unsafe int ResetEnum(void* enumHandle, int position);

	[PreserveSig]
	unsafe int EnumTypeDefs(ref void* enumHandle, [Out] int* typeDefs, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int EnumInterfaceImpls(ref void* enumHandle, int typeDef, [Out] int* interfaceImpls, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int EnumTypeRefs(ref void* enumHandle, [Out] int* typeRefs, int bufferLength, [Out] int* count);

	[PreserveSig]
	int FindTypeDefByName(string name, int enclosingClass, out int typeDef);

	[PreserveSig]
	unsafe int GetScopeProps([Out] char* name, int bufferLength, [Out] int* nameLength, [Out] Guid* mvid);

	[PreserveSig]
	int GetModuleFromScope(out int moduleDef);

	[PreserveSig]
	unsafe int GetTypeDefProps(int typeDef, [Out] char* qualifiedName, int qualifiedNameBufferLength, [Out] int* qualifiedNameLength, [Out] TypeAttributes* attributes, [Out] int* baseType);

	[PreserveSig]
	unsafe int GetInterfaceImplProps(int interfaceImpl, [Out] int* typeDef, [Out] int* interfaceDefRefSpec);

	[PreserveSig]
	unsafe int GetTypeRefProps(int typeRef, [Out] int* resolutionScope, [Out] char* qualifiedName, int qualifiedNameBufferLength, [Out] int* qualifiedNameLength);

	[PreserveSig]
	int ResolveTypeRef(int typeRef, [In] ref Guid scopeInterfaceId, [MarshalAs(UnmanagedType.Interface)] out object scope, out int typeDef);

	[PreserveSig]
	unsafe int EnumMembers(ref void* enumHandle, int typeDef, [Out] int* memberDefs, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int EnumMembersWithName(ref void* enumHandle, int typeDef, string name, [Out] int* memberDefs, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int EnumMethods(ref void* enumHandle, int typeDef, [Out] int* methodDefs, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int EnumMethodsWithName(ref void* enumHandle, int typeDef, string name, [Out] int* methodDefs, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int EnumFields(ref void* enumHandle, int typeDef, [Out] int* fieldDefs, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int EnumFieldsWithName(ref void* enumHandle, int typeDef, string name, [Out] int* fieldDefs, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int EnumParams(ref void* enumHandle, int methodDef, [Out] int* paramDefs, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int EnumMemberRefs(ref void* enumHandle, int parentToken, [Out] int* memberRefs, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int EnumMethodImpls(ref void* enumHandle, int typeDef, [Out] int* implementationTokens, [Out] int* declarationTokens, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int EnumPermissionSets(ref void* enumHandle, int token, uint action, [Out] int* declSecurityTokens, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int FindMember(int typeDef, string name, [In] byte* signature, int signatureLength, out int memberDef);

	[PreserveSig]
	unsafe int FindMethod(int typeDef, string name, [In] byte* signature, int signatureLength, out int methodDef);

	[PreserveSig]
	unsafe int FindField(int typeDef, string name, [In] byte* signature, int signatureLength, out int fieldDef);

	[PreserveSig]
	unsafe int FindMemberRef(int typeDef, string name, [In] byte* signature, int signatureLength, out int memberRef);

	[PreserveSig]
	unsafe int GetMethodProps(int methodDef, [Out] int* declaringTypeDef, [Out] char* name, int nameBufferLength, [Out] int* nameLength, [Out] MethodAttributes* attributes, [Out] byte** signature, [Out] int* signatureLength, [Out] int* relativeVirtualAddress, [Out] MethodImplAttributes* implAttributes);

	[PreserveSig]
	unsafe int GetMemberRefProps(int memberRef, [Out] int* declaringType, [Out] char* name, int nameBufferLength, [Out] int* nameLength, [Out] byte** signature, [Out] int* signatureLength);

	[PreserveSig]
	unsafe int EnumProperties(ref void* enumHandle, int typeDef, [Out] int* properties, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe uint EnumEvents(ref void* enumHandle, int typeDef, [Out] int* events, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int GetEventProps(int @event, [Out] int* declaringTypeDef, [Out] char* name, int nameBufferLength, [Out] int* nameLength, [Out] int* attributes, [Out] int* eventType, [Out] int* adderMethodDef, [Out] int* removerMethodDef, [Out] int* raiserMethodDef, [Out] int* otherMethodDefs, int otherMethodDefBufferLength, [Out] int* methodMethodDefsLength);

	[PreserveSig]
	unsafe int EnumMethodSemantics(ref void* enumHandle, int methodDef, [Out] int* eventsAndProperties, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int GetMethodSemantics(int methodDef, int eventOrProperty, [Out] int* semantics);

	[PreserveSig]
	unsafe int GetClassLayout(int typeDef, [Out] int* packSize, [Out] MetadataImportFieldOffset* fieldOffsets, int bufferLength, [Out] int* count, [Out] int* typeSize);

	[PreserveSig]
	unsafe int GetFieldMarshal(int fieldDef, [Out] byte** nativeTypeSignature, [Out] int* nativeTypeSignatureLengvth);

	[PreserveSig]
	unsafe int GetRVA(int methodDef, [Out] int* relativeVirtualAddress, [Out] int* implAttributes);

	[PreserveSig]
	unsafe int GetPermissionSetProps(int declSecurity, [Out] uint* action, [Out] byte** permissionBlob, [Out] int* permissionBlobLength);

	[PreserveSig]
	unsafe int GetSigFromToken(int standaloneSignature, [Out] byte** signature, [Out] int* signatureLength);

	[PreserveSig]
	unsafe int GetModuleRefProps(int moduleRef, [Out] char* name, int nameBufferLength, [Out] int* nameLength);

	[PreserveSig]
	unsafe int EnumModuleRefs(ref void* enumHandle, [Out] int* moduleRefs, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int GetTypeSpecFromToken(int typeSpec, [Out] byte** signature, [Out] int* signatureLength);

	[PreserveSig]
	unsafe int GetNameFromToken(int token, [Out] byte* nameUTF8);

	[PreserveSig]
	unsafe int EnumUnresolvedMethods(ref void* enumHandle, [Out] int* methodDefs, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int GetUserString(int userStringToken, [Out] char* buffer, int bufferLength, [Out] int* length);

	[PreserveSig]
	unsafe int GetPinvokeMap(int memberDef, [Out] int* attributes, [Out] char* importName, int importNameBufferLength, [Out] int* importNameLength, [Out] int* moduleRef);

	[PreserveSig]
	unsafe int EnumSignatures(ref void* enumHandle, [Out] int* signatureTokens, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int EnumTypeSpecs(ref void* enumHandle, [Out] int* typeSpecs, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int EnumUserStrings(ref void* enumHandle, [Out] int* userStrings, int bufferLength, [Out] int* count);

	[PreserveSig]
	int GetParamForMethodIndex(int methodDef, int sequenceNumber, out int parameterToken);

	[PreserveSig]
	unsafe int EnumCustomAttributes(ref void* enumHandle, int parent, int attributeType, [Out] int* customAttributes, int bufferLength, [Out] int* count);

	[PreserveSig]
	unsafe int GetCustomAttributeProps(int customAttribute, [Out] int* parent, [Out] int* constructor, [Out] byte** value, [Out] int* valueLength);

	[PreserveSig]
	int FindTypeRef(int resolutionScope, string name, out int typeRef);

	[PreserveSig]
	unsafe int GetMemberProps(int member, [Out] int* declaringTypeDef, [Out] char* name, int nameBufferLength, [Out] int* nameLength, [Out] int* attributes, [Out] byte** signature, [Out] int* signatureLength, [Out] int* relativeVirtualAddress, [Out] int* implAttributes, [Out] int* constantType, [Out] byte** constantValue, [Out] int* constantValueLength);

	[PreserveSig]
	unsafe int GetFieldProps(int fieldDef, [Out] int* declaringTypeDef, [Out] char* name, int nameBufferLength, [Out] int* nameLength, [Out] int* attributes, [Out] byte** signature, [Out] int* signatureLength, [Out] int* constantType, [Out] byte** constantValue, [Out] int* constantValueLength);

	[PreserveSig]
	unsafe int GetPropertyProps(int propertyDef, [Out] int* declaringTypeDef, [Out] char* name, int nameBufferLength, [Out] int* nameLength, [Out] int* attributes, [Out] byte** signature, [Out] int* signatureLength, [Out] int* constantType, [Out] byte** constantValue, [Out] int* constantValueLength, [Out] int* setterMethodDef, [Out] int* getterMethodDef, [Out] int* outerMethodDefs, int outerMethodDefsBufferLength, [Out] int* otherMethodDefCount);

	[PreserveSig]
	unsafe int GetParamProps(int parameter, [Out] int* declaringMethodDef, [Out] int* sequenceNumber, [Out] char* name, int nameBufferLength, [Out] int* nameLength, [Out] int* attributes, [Out] int* constantType, [Out] byte** constantValue, [Out] int* constantValueLength);

	[PreserveSig]
	unsafe int GetCustomAttributeByName(int parent, string name, [Out] byte** value, [Out] int* valueLength);

	[PreserveSig]
	bool IsValidToken(int token);

	[PreserveSig]
	int GetNestedClassProps(int nestedClass, out int enclosingClass);

	[PreserveSig]
	unsafe int GetNativeCallConvFromSig([In] byte* signature, int signatureLength, [Out] int* callingConvention);

	[PreserveSig]
	int IsGlobal(int token, [Out] bool value);
}
