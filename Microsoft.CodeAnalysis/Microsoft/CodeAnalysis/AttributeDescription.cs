using System.Collections.Immutable;
using System.Reflection.Metadata;

namespace Microsoft.CodeAnalysis;

internal struct AttributeDescription(string @namespace, string name, byte[][] signatures, bool matchIgnoringCase = false)
{
	internal enum TypeHandleTarget : byte
	{
		AttributeTargets,
		AssemblyNameFlags,
		MethodImplOptions,
		CharSet,
		LayoutKind,
		UnmanagedType,
		TypeLibTypeFlags,
		ClassInterfaceType,
		ComInterfaceType,
		CompilationRelaxations,
		DebuggingModes,
		SecurityCriticalScope,
		CallingConvention,
		AssemblyHashAlgorithm,
		TransactionOption,
		SecurityAction,
		SystemType,
		DeprecationType,
		Platform
	}

	internal struct TypeHandleTargetInfo(string @namespace, string name, SerializationTypeCode underlying)
	{
		public readonly string Namespace = @namespace;

		public readonly string Name = name;

		public readonly SerializationTypeCode Underlying = underlying;
	}

	public readonly string Namespace = @namespace;

	public readonly string Name = name;

	public readonly byte[][] Signatures = signatures;

	public readonly bool MatchIgnoringCase = matchIgnoringCase;

	private const byte Void = 1;

	private const byte Boolean = 2;

	private const byte Char = 3;

	private const byte SByte = 4;

	private const byte Byte = 5;

	private const byte Int16 = 6;

	private const byte UInt16 = 7;

	private const byte Int32 = 8;

	private const byte UInt32 = 9;

	private const byte Int64 = 10;

	private const byte UInt64 = 11;

	private const byte Single = 12;

	private const byte Double = 13;

	private const byte String = 14;

	private const byte Object = 28;

	private const byte SzArray = 29;

	private const byte TypeHandle = 64;

	internal static ImmutableArray<TypeHandleTargetInfo> TypeHandleTargets;

	private static readonly byte[] s_signature_HasThis_Void;

	private static readonly byte[] s_signature_HasThis_Void_Int16;

	private static readonly byte[] s_signature_HasThis_Void_Int32;

	private static readonly byte[] s_signature_HasThis_Void_UInt32;

	private static readonly byte[] s_signature_HasThis_Void_Int32_Int32;

	private static readonly byte[] s_signature_HasThis_Void_Int32_Int32_Int32_Int32;

	private static readonly byte[] s_signature_HasThis_Void_String;

	private static readonly byte[] s_signature_HasThis_Void_Object;

	private static readonly byte[] s_signature_HasThis_Void_String_String;

	private static readonly byte[] s_signature_HasThis_Void_String_Boolean;

	private static readonly byte[] s_signature_HasThis_Void_String_String_String;

	private static readonly byte[] s_signature_HasThis_Void_String_String_String_String;

	private static readonly byte[] s_signature_HasThis_Void_AttributeTargets;

	private static readonly byte[] s_signature_HasThis_Void_AssemblyNameFlags;

	private static readonly byte[] s_signature_HasThis_Void_MethodImplOptions;

	private static readonly byte[] s_signature_HasThis_Void_CharSet;

	private static readonly byte[] s_signature_HasThis_Void_LayoutKind;

	private static readonly byte[] s_signature_HasThis_Void_UnmanagedType;

	private static readonly byte[] s_signature_HasThis_Void_TypeLibTypeFlags;

	private static readonly byte[] s_signature_HasThis_Void_ClassInterfaceType;

	private static readonly byte[] s_signature_HasThis_Void_ComInterfaceType;

	private static readonly byte[] s_signature_HasThis_Void_CompilationRelaxations;

	private static readonly byte[] s_signature_HasThis_Void_DebuggingModes;

	private static readonly byte[] s_signature_HasThis_Void_SecurityCriticalScope;

	private static readonly byte[] s_signature_HasThis_Void_CallingConvention;

	private static readonly byte[] s_signature_HasThis_Void_AssemblyHashAlgorithm;

	private static readonly byte[] s_signature_HasThis_Void_Int64;

	private static readonly byte[] s_signature_HasThis_Void_UInt8_UInt8_UInt32_UInt32_UInt32;

	private static readonly byte[] s_signature_HasThis_Void_UIn8_UInt8_Int32_Int32_Int32;

	private static readonly byte[] s_signature_HasThis_Void_Boolean;

	private static readonly byte[] s_signature_HasThis_Void_Boolean_Boolean;

	private static readonly byte[] s_signature_HasThis_Void_Boolean_TransactionOption;

	private static readonly byte[] s_signature_HasThis_Void_Boolean_TransactionOption_Int32;

	private static readonly byte[] s_signature_HasThis_Void_Boolean_TransactionOption_Int32_Boolean;

	private static readonly byte[] s_signature_HasThis_Void_SecurityAction;

	private static readonly byte[] s_signature_HasThis_Void_Type;

	private static readonly byte[] s_signature_HasThis_Void_Type_Type;

	private static readonly byte[] s_signature_HasThis_Void_Type_Type_Type;

	private static readonly byte[] s_signature_HasThis_Void_Type_Type_Type_Type;

	private static readonly byte[] s_signature_HasThis_Void_Type_Int32;

	private static readonly byte[] s_signature_HasThis_Void_SzArray_Boolean;

	private static readonly byte[] s_signature_HasThis_Void_SzArray_String;

	private static readonly byte[] s_signature_HasThis_Void_String_DeprecationType_UInt32;

	private static readonly byte[] s_signature_HasThis_Void_String_DeprecationType_UInt32_Platform;

	private static readonly byte[] s_signature_HasThis_Void_String_DeprecationType_UInt32_Type;

	private static readonly byte[] s_signature_HasThis_Void_String_DeprecationType_UInt32_String;

	private static readonly byte[][] s_signaturesOfTypeIdentifierAttribute;

	private static readonly byte[][] s_signaturesOfStandardModuleAttribute;

	private static readonly byte[][] s_signaturesOfExtensionAttribute;

	private static readonly byte[][] s_signaturesOfAttributeUsage;

	private static readonly byte[][] s_signaturesOfAssemblySignatureKeyAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyKeyFileAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyKeyNameAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyDelaySignAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyVersionAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyFileVersionAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyTitleAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyDescriptionAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyCultureAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyCompanyAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyProductAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyInformationalVersionAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyCopyrightAttribute;

	private static readonly byte[][] s_signaturesOfSatelliteContractVersionAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyTrademarkAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyFlagsAttribute;

	private static readonly byte[][] s_signaturesOfDefaultMemberAttribute;

	private static readonly byte[][] s_signaturesOfAccessedThroughPropertyAttribute;

	private static readonly byte[][] s_signaturesOfIndexerNameAttribute;

	private static readonly byte[][] s_signaturesOfInternalsVisibleToAttribute;

	private static readonly byte[][] s_signaturesOfOptionalAttribute;

	private static readonly byte[][] s_signaturesOfDefaultParameterValueAttribute;

	private static readonly byte[][] s_signaturesOfDateTimeConstantAttribute;

	private static readonly byte[][] s_signaturesOfDecimalConstantAttribute;

	private static readonly byte[][] s_signaturesOfIUnknownConstantAttribute;

	private static readonly byte[][] s_signaturesOfCallerFilePathAttribute;

	private static readonly byte[][] s_signaturesOfCallerLineNumberAttribute;

	private static readonly byte[][] s_signaturesOfCallerMemberNameAttribute;

	private static readonly byte[][] s_signaturesOfIDispatchConstantAttribute;

	private static readonly byte[][] s_signaturesOfParamArrayAttribute;

	private static readonly byte[][] s_signaturesOfDllImportAttribute;

	private static readonly byte[][] s_signaturesOfUnverifiableCodeAttribute;

	private static readonly byte[][] s_signaturesOfSecurityPermissionAttribute;

	private static readonly byte[][] s_signaturesOfCoClassAttribute;

	private static readonly byte[][] s_signaturesOfComImportAttribute;

	private static readonly byte[][] s_signaturesOfGuidAttribute;

	private static readonly byte[][] s_signaturesOfCLSCompliantAttribute;

	private static readonly byte[][] s_signaturesOfMethodImplAttribute;

	private static readonly byte[][] s_signaturesOfPreserveSigAttribute;

	private static readonly byte[][] s_signaturesOfDefaultCharSetAttribute;

	private static readonly byte[][] s_signaturesOfSpecialNameAttribute;

	private static readonly byte[][] s_signaturesOfNonSerializedAttribute;

	private static readonly byte[][] s_signaturesOfFieldOffsetAttribute;

	private static readonly byte[][] s_signaturesOfSerializableAttribute;

	private static readonly byte[][] s_signaturesOfInAttribute;

	private static readonly byte[][] s_signaturesOfOutAttribute;

	private static readonly byte[][] s_signaturesOfIsReadOnlyAttribute;

	private static readonly byte[][] s_signaturesOfIsUnmanagedAttribute;

	private static readonly byte[][] s_signaturesOfFixedBufferAttribute;

	private static readonly byte[][] s_signaturesOfSuppressUnmanagedCodeSecurityAttribute;

	private static readonly byte[][] s_signaturesOfPrincipalPermissionAttribute;

	private static readonly byte[][] s_signaturesOfPermissionSetAttribute;

	private static readonly byte[][] s_signaturesOfStructLayoutAttribute;

	private static readonly byte[][] s_signaturesOfMarshalAsAttribute;

	private static readonly byte[][] s_signaturesOfTypeLibTypeAttribute;

	private static readonly byte[][] s_signaturesOfWebMethodAttribute;

	private static readonly byte[][] s_signaturesOfHostProtectionAttribute;

	private static readonly byte[][] s_signaturesOfVisualBasicEmbedded;

	private static readonly byte[][] s_signaturesOfCodeAnalysisEmbedded;

	private static readonly byte[][] s_signaturesOfVisualBasicComClassAttribute;

	private static readonly byte[][] s_signaturesOfClassInterfaceAttribute;

	private static readonly byte[][] s_signaturesOfInterfaceTypeAttribute;

	private static readonly byte[][] s_signaturesOfCompilationRelaxationsAttribute;

	private static readonly byte[][] s_signaturesOfReferenceAssemblyAttribute;

	private static readonly byte[][] s_signaturesOfDebuggableAttribute;

	private static readonly byte[][] s_signaturesOfComSourceInterfacesAttribute;

	private static readonly byte[][] s_signaturesOfComVisibleAttribute;

	private static readonly byte[][] s_signaturesOfConditionalAttribute;

	private static readonly byte[][] s_signaturesOfTypeLibVersionAttribute;

	private static readonly byte[][] s_signaturesOfComCompatibleVersionAttribute;

	private static readonly byte[][] s_signaturesOfWindowsRuntimeImportAttribute;

	private static readonly byte[][] s_signaturesOfDynamicSecurityMethodAttribute;

	private static readonly byte[][] s_signaturesOfRequiredAttributeAttribute;

	private static readonly byte[][] s_signaturesOfAsyncMethodBuilderAttribute;

	private static readonly byte[][] s_signaturesOfAsyncStateMachineAttribute;

	private static readonly byte[][] s_signaturesOfIteratorStateMachineAttribute;

	private static readonly byte[][] s_signaturesOfRuntimeCompatibilityAttribute;

	private static readonly byte[][] s_signaturesOfTypeForwardedToAttribute;

	private static readonly byte[][] s_signaturesOfSTAThreadAttribute;

	private static readonly byte[][] s_signaturesOfMTAThreadAttribute;

	private static readonly byte[][] s_signaturesOfOptionCompareAttribute;

	private static readonly byte[][] s_signaturesOfObsoleteAttribute;

	private static readonly byte[][] s_signaturesOfDynamicAttribute;

	private static readonly byte[][] s_signaturesOfTupleElementNamesAttribute;

	private static readonly byte[][] s_signaturesOfIsByRefLikeAttribute;

	private static readonly byte[][] s_signaturesOfDebuggerHiddenAttribute;

	private static readonly byte[][] s_signaturesOfDebuggerNonUserCodeAttribute;

	private static readonly byte[][] s_signaturesOfDebuggerStepperBoundaryAttribute;

	private static readonly byte[][] s_signaturesOfDebuggerStepThroughAttribute;

	private static readonly byte[][] s_signaturesOfSecurityCriticalAttribute;

	private static readonly byte[][] s_signaturesOfSecuritySafeCriticalAttribute;

	private static readonly byte[][] s_signaturesOfDesignerGeneratedAttribute;

	private static readonly byte[][] s_signaturesOfMyGroupCollectionAttribute;

	private static readonly byte[][] s_signaturesOfComEventInterfaceAttribute;

	private static readonly byte[][] s_signaturesOfBestFitMappingAttribute;

	private static readonly byte[][] s_signaturesOfFlagsAttribute;

	private static readonly byte[][] s_signaturesOfLCIDConversionAttribute;

	private static readonly byte[][] s_signaturesOfUnmanagedFunctionPointerAttribute;

	private static readonly byte[][] s_signaturesOfPrimaryInteropAssemblyAttribute;

	private static readonly byte[][] s_signaturesOfImportedFromTypeLibAttribute;

	private static readonly byte[][] s_signaturesOfDefaultEventAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyConfigurationAttribute;

	private static readonly byte[][] s_signaturesOfAssemblyAlgorithmIdAttribute;

	private static readonly byte[][] s_signaturesOfDeprecatedAttribute;

	private static readonly byte[][] s_signaturesOfExperimentalAttribute;

	private static readonly byte[][] s_signaturesOfExcludeFromCodeCoverageAttribute;

	internal static readonly AttributeDescription OptionalAttribute;

	internal static readonly AttributeDescription ComImportAttribute;

	internal static readonly AttributeDescription AttributeUsageAttribute;

	internal static readonly AttributeDescription ConditionalAttribute;

	internal static readonly AttributeDescription CaseInsensitiveExtensionAttribute;

	internal static readonly AttributeDescription CaseSensitiveExtensionAttribute;

	internal static readonly AttributeDescription InternalsVisibleToAttribute;

	internal static readonly AttributeDescription AssemblySignatureKeyAttribute;

	internal static readonly AttributeDescription AssemblyKeyFileAttribute;

	internal static readonly AttributeDescription AssemblyKeyNameAttribute;

	internal static readonly AttributeDescription ParamArrayAttribute;

	internal static readonly AttributeDescription DefaultMemberAttribute;

	internal static readonly AttributeDescription IndexerNameAttribute;

	internal static readonly AttributeDescription AssemblyDelaySignAttribute;

	internal static readonly AttributeDescription AssemblyVersionAttribute;

	internal static readonly AttributeDescription AssemblyFileVersionAttribute;

	internal static readonly AttributeDescription AssemblyTitleAttribute;

	internal static readonly AttributeDescription AssemblyDescriptionAttribute;

	internal static readonly AttributeDescription AssemblyCultureAttribute;

	internal static readonly AttributeDescription AssemblyCompanyAttribute;

	internal static readonly AttributeDescription AssemblyProductAttribute;

	internal static readonly AttributeDescription AssemblyInformationalVersionAttribute;

	internal static readonly AttributeDescription AssemblyCopyrightAttribute;

	internal static readonly AttributeDescription SatelliteContractVersionAttribute;

	internal static readonly AttributeDescription AssemblyTrademarkAttribute;

	internal static readonly AttributeDescription AssemblyFlagsAttribute;

	internal static readonly AttributeDescription DecimalConstantAttribute;

	internal static readonly AttributeDescription IUnknownConstantAttribute;

	internal static readonly AttributeDescription CallerFilePathAttribute;

	internal static readonly AttributeDescription CallerLineNumberAttribute;

	internal static readonly AttributeDescription CallerMemberNameAttribute;

	internal static readonly AttributeDescription IDispatchConstantAttribute;

	internal static readonly AttributeDescription DefaultParameterValueAttribute;

	internal static readonly AttributeDescription UnverifiableCodeAttribute;

	internal static readonly AttributeDescription SecurityPermissionAttribute;

	internal static readonly AttributeDescription DllImportAttribute;

	internal static readonly AttributeDescription MethodImplAttribute;

	internal static readonly AttributeDescription PreserveSigAttribute;

	internal static readonly AttributeDescription DefaultCharSetAttribute;

	internal static readonly AttributeDescription SpecialNameAttribute;

	internal static readonly AttributeDescription SerializableAttribute;

	internal static readonly AttributeDescription NonSerializedAttribute;

	internal static readonly AttributeDescription StructLayoutAttribute;

	internal static readonly AttributeDescription FieldOffsetAttribute;

	internal static readonly AttributeDescription FixedBufferAttribute;

	internal static readonly AttributeDescription MarshalAsAttribute;

	internal static readonly AttributeDescription InAttribute;

	internal static readonly AttributeDescription OutAttribute;

	internal static readonly AttributeDescription IsReadOnlyAttribute;

	internal static readonly AttributeDescription IsUnmanagedAttribute;

	internal static readonly AttributeDescription CoClassAttribute;

	internal static readonly AttributeDescription GuidAttribute;

	internal static readonly AttributeDescription CLSCompliantAttribute;

	internal static readonly AttributeDescription HostProtectionAttribute;

	internal static readonly AttributeDescription SuppressUnmanagedCodeSecurityAttribute;

	internal static readonly AttributeDescription PrincipalPermissionAttribute;

	internal static readonly AttributeDescription PermissionSetAttribute;

	internal static readonly AttributeDescription TypeIdentifierAttribute;

	internal static readonly AttributeDescription VisualBasicEmbeddedAttribute;

	internal static readonly AttributeDescription CodeAnalysisEmbeddedAttribute;

	internal static readonly AttributeDescription VisualBasicComClassAttribute;

	internal static readonly AttributeDescription StandardModuleAttribute;

	internal static readonly AttributeDescription OptionCompareAttribute;

	internal static readonly AttributeDescription AccessedThroughPropertyAttribute;

	internal static readonly AttributeDescription WebMethodAttribute;

	internal static readonly AttributeDescription DateTimeConstantAttribute;

	internal static readonly AttributeDescription ClassInterfaceAttribute;

	internal static readonly AttributeDescription ComSourceInterfacesAttribute;

	internal static readonly AttributeDescription ComVisibleAttribute;

	internal static readonly AttributeDescription DispIdAttribute;

	internal static readonly AttributeDescription TypeLibVersionAttribute;

	internal static readonly AttributeDescription ComCompatibleVersionAttribute;

	internal static readonly AttributeDescription InterfaceTypeAttribute;

	internal static readonly AttributeDescription WindowsRuntimeImportAttribute;

	internal static readonly AttributeDescription DynamicSecurityMethodAttribute;

	internal static readonly AttributeDescription RequiredAttributeAttribute;

	internal static readonly AttributeDescription AsyncMethodBuilderAttribute;

	internal static readonly AttributeDescription AsyncStateMachineAttribute;

	internal static readonly AttributeDescription IteratorStateMachineAttribute;

	internal static readonly AttributeDescription CompilationRelaxationsAttribute;

	internal static readonly AttributeDescription ReferenceAssemblyAttribute;

	internal static readonly AttributeDescription RuntimeCompatibilityAttribute;

	internal static readonly AttributeDescription DebuggableAttribute;

	internal static readonly AttributeDescription TypeForwardedToAttribute;

	internal static readonly AttributeDescription STAThreadAttribute;

	internal static readonly AttributeDescription MTAThreadAttribute;

	internal static readonly AttributeDescription ObsoleteAttribute;

	internal static readonly AttributeDescription TypeLibTypeAttribute;

	internal static readonly AttributeDescription DynamicAttribute;

	internal static readonly AttributeDescription TupleElementNamesAttribute;

	internal static readonly AttributeDescription IsByRefLikeAttribute;

	internal static readonly AttributeDescription DebuggerHiddenAttribute;

	internal static readonly AttributeDescription DebuggerNonUserCodeAttribute;

	internal static readonly AttributeDescription DebuggerStepperBoundaryAttribute;

	internal static readonly AttributeDescription DebuggerStepThroughAttribute;

	internal static readonly AttributeDescription SecurityCriticalAttribute;

	internal static readonly AttributeDescription SecuritySafeCriticalAttribute;

	internal static readonly AttributeDescription DesignerGeneratedAttribute;

	internal static readonly AttributeDescription MyGroupCollectionAttribute;

	internal static readonly AttributeDescription ComEventInterfaceAttribute;

	internal static readonly AttributeDescription BestFitMappingAttribute;

	internal static readonly AttributeDescription FlagsAttribute;

	internal static readonly AttributeDescription LCIDConversionAttribute;

	internal static readonly AttributeDescription UnmanagedFunctionPointerAttribute;

	internal static readonly AttributeDescription PrimaryInteropAssemblyAttribute;

	internal static readonly AttributeDescription ImportedFromTypeLibAttribute;

	internal static readonly AttributeDescription DefaultEventAttribute;

	internal static readonly AttributeDescription AssemblyConfigurationAttribute;

	internal static readonly AttributeDescription AssemblyAlgorithmIdAttribute;

	internal static readonly AttributeDescription DeprecatedAttribute;

	internal static readonly AttributeDescription ExperimentalAttribute;

	internal static readonly AttributeDescription ExcludeFromCodeCoverageAttribute;

	public string FullName => Namespace + "." + Name;

	public override string ToString()
	{
		return FullName + "(" + Signatures.Length + ")";
	}

	internal int GetParameterCount(int signatureIndex)
	{
		return Signatures[signatureIndex][1];
	}

	static AttributeDescription()
	{
		s_signature_HasThis_Void = new byte[3] { 32, 0, 1 };
		s_signature_HasThis_Void_Int16 = new byte[4] { 32, 1, 1, 6 };
		s_signature_HasThis_Void_Int32 = new byte[4] { 32, 1, 1, 8 };
		s_signature_HasThis_Void_UInt32 = new byte[4] { 32, 1, 1, 9 };
		s_signature_HasThis_Void_Int32_Int32 = new byte[5] { 32, 2, 1, 8, 8 };
		s_signature_HasThis_Void_Int32_Int32_Int32_Int32 = new byte[7] { 32, 4, 1, 8, 8, 8, 8 };
		s_signature_HasThis_Void_String = new byte[4] { 32, 1, 1, 14 };
		s_signature_HasThis_Void_Object = new byte[4] { 32, 1, 1, 28 };
		s_signature_HasThis_Void_String_String = new byte[5] { 32, 2, 1, 14, 14 };
		s_signature_HasThis_Void_String_Boolean = new byte[5] { 32, 2, 1, 14, 2 };
		s_signature_HasThis_Void_String_String_String = new byte[6] { 32, 3, 1, 14, 14, 14 };
		s_signature_HasThis_Void_String_String_String_String = new byte[7] { 32, 4, 1, 14, 14, 14, 14 };
		s_signature_HasThis_Void_AttributeTargets = new byte[5] { 32, 1, 1, 64, 0 };
		s_signature_HasThis_Void_AssemblyNameFlags = new byte[5] { 32, 1, 1, 64, 1 };
		s_signature_HasThis_Void_MethodImplOptions = new byte[5] { 32, 1, 1, 64, 2 };
		s_signature_HasThis_Void_CharSet = new byte[5] { 32, 1, 1, 64, 3 };
		s_signature_HasThis_Void_LayoutKind = new byte[5] { 32, 1, 1, 64, 4 };
		s_signature_HasThis_Void_UnmanagedType = new byte[5] { 32, 1, 1, 64, 5 };
		s_signature_HasThis_Void_TypeLibTypeFlags = new byte[5] { 32, 1, 1, 64, 6 };
		s_signature_HasThis_Void_ClassInterfaceType = new byte[5] { 32, 1, 1, 64, 7 };
		s_signature_HasThis_Void_ComInterfaceType = new byte[5] { 32, 1, 1, 64, 8 };
		s_signature_HasThis_Void_CompilationRelaxations = new byte[5] { 32, 1, 1, 64, 9 };
		s_signature_HasThis_Void_DebuggingModes = new byte[5] { 32, 1, 1, 64, 10 };
		s_signature_HasThis_Void_SecurityCriticalScope = new byte[5] { 32, 1, 1, 64, 11 };
		s_signature_HasThis_Void_CallingConvention = new byte[5] { 32, 1, 1, 64, 12 };
		s_signature_HasThis_Void_AssemblyHashAlgorithm = new byte[5] { 32, 1, 1, 64, 13 };
		s_signature_HasThis_Void_Int64 = new byte[4] { 32, 1, 1, 10 };
		s_signature_HasThis_Void_UInt8_UInt8_UInt32_UInt32_UInt32 = new byte[8] { 32, 5, 1, 5, 5, 9, 9, 9 };
		s_signature_HasThis_Void_UIn8_UInt8_Int32_Int32_Int32 = new byte[8] { 32, 5, 1, 5, 5, 8, 8, 8 };
		s_signature_HasThis_Void_Boolean = new byte[4] { 32, 1, 1, 2 };
		s_signature_HasThis_Void_Boolean_Boolean = new byte[5] { 32, 2, 1, 2, 2 };
		s_signature_HasThis_Void_Boolean_TransactionOption = new byte[6] { 32, 2, 1, 2, 64, 14 };
		s_signature_HasThis_Void_Boolean_TransactionOption_Int32 = new byte[7] { 32, 3, 1, 2, 64, 14, 8 };
		s_signature_HasThis_Void_Boolean_TransactionOption_Int32_Boolean = new byte[8] { 32, 4, 1, 2, 64, 14, 8, 2 };
		s_signature_HasThis_Void_SecurityAction = new byte[5] { 32, 1, 1, 64, 15 };
		s_signature_HasThis_Void_Type = new byte[5] { 32, 1, 1, 64, 16 };
		s_signature_HasThis_Void_Type_Type = new byte[7] { 32, 2, 1, 64, 16, 64, 16 };
		s_signature_HasThis_Void_Type_Type_Type = new byte[9] { 32, 3, 1, 64, 16, 64, 16, 64, 16 };
		s_signature_HasThis_Void_Type_Type_Type_Type = new byte[11]
		{
			32, 4, 1, 64, 16, 64, 16, 64, 16, 64,
			16
		};
		s_signature_HasThis_Void_Type_Int32 = new byte[6] { 32, 2, 1, 64, 16, 8 };
		s_signature_HasThis_Void_SzArray_Boolean = new byte[5] { 32, 1, 1, 29, 2 };
		s_signature_HasThis_Void_SzArray_String = new byte[5] { 32, 1, 1, 29, 14 };
		s_signature_HasThis_Void_String_DeprecationType_UInt32 = new byte[7] { 32, 3, 1, 14, 64, 17, 9 };
		s_signature_HasThis_Void_String_DeprecationType_UInt32_Platform = new byte[9] { 32, 4, 1, 14, 64, 17, 9, 64, 18 };
		s_signature_HasThis_Void_String_DeprecationType_UInt32_Type = new byte[9] { 32, 4, 1, 14, 64, 17, 9, 64, 16 };
		s_signature_HasThis_Void_String_DeprecationType_UInt32_String = new byte[8] { 32, 4, 1, 14, 64, 17, 9, 14 };
		s_signaturesOfTypeIdentifierAttribute = new byte[2][] { s_signature_HasThis_Void, s_signature_HasThis_Void_String_String };
		s_signaturesOfStandardModuleAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfExtensionAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfAttributeUsage = new byte[1][] { s_signature_HasThis_Void_AttributeTargets };
		s_signaturesOfAssemblySignatureKeyAttribute = new byte[1][] { s_signature_HasThis_Void_String_String };
		s_signaturesOfAssemblyKeyFileAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyKeyNameAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyDelaySignAttribute = new byte[1][] { s_signature_HasThis_Void_Boolean };
		s_signaturesOfAssemblyVersionAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyFileVersionAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyTitleAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyDescriptionAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyCultureAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyCompanyAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyProductAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyInformationalVersionAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyCopyrightAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfSatelliteContractVersionAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyTrademarkAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyFlagsAttribute = new byte[3][] { s_signature_HasThis_Void_AssemblyNameFlags, s_signature_HasThis_Void_Int32, s_signature_HasThis_Void_UInt32 };
		s_signaturesOfDefaultMemberAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAccessedThroughPropertyAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfIndexerNameAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfInternalsVisibleToAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfOptionalAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfDefaultParameterValueAttribute = new byte[1][] { s_signature_HasThis_Void_Object };
		s_signaturesOfDateTimeConstantAttribute = new byte[1][] { s_signature_HasThis_Void_Int64 };
		s_signaturesOfDecimalConstantAttribute = new byte[2][] { s_signature_HasThis_Void_UInt8_UInt8_UInt32_UInt32_UInt32, s_signature_HasThis_Void_UIn8_UInt8_Int32_Int32_Int32 };
		s_signaturesOfIUnknownConstantAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfCallerFilePathAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfCallerLineNumberAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfCallerMemberNameAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfIDispatchConstantAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfParamArrayAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfDllImportAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfUnverifiableCodeAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfSecurityPermissionAttribute = new byte[1][] { s_signature_HasThis_Void_SecurityAction };
		s_signaturesOfCoClassAttribute = new byte[1][] { s_signature_HasThis_Void_Type };
		s_signaturesOfComImportAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfGuidAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfCLSCompliantAttribute = new byte[1][] { s_signature_HasThis_Void_Boolean };
		s_signaturesOfMethodImplAttribute = new byte[3][] { s_signature_HasThis_Void, s_signature_HasThis_Void_Int16, s_signature_HasThis_Void_MethodImplOptions };
		s_signaturesOfPreserveSigAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfDefaultCharSetAttribute = new byte[1][] { s_signature_HasThis_Void_CharSet };
		s_signaturesOfSpecialNameAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfNonSerializedAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfFieldOffsetAttribute = new byte[1][] { s_signature_HasThis_Void_Int32 };
		s_signaturesOfSerializableAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfInAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfOutAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfIsReadOnlyAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfIsUnmanagedAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfFixedBufferAttribute = new byte[1][] { s_signature_HasThis_Void_Type_Int32 };
		s_signaturesOfSuppressUnmanagedCodeSecurityAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfPrincipalPermissionAttribute = new byte[1][] { s_signature_HasThis_Void_SecurityAction };
		s_signaturesOfPermissionSetAttribute = new byte[1][] { s_signature_HasThis_Void_SecurityAction };
		s_signaturesOfStructLayoutAttribute = new byte[2][] { s_signature_HasThis_Void_Int16, s_signature_HasThis_Void_LayoutKind };
		s_signaturesOfMarshalAsAttribute = new byte[2][] { s_signature_HasThis_Void_Int16, s_signature_HasThis_Void_UnmanagedType };
		s_signaturesOfTypeLibTypeAttribute = new byte[2][] { s_signature_HasThis_Void_Int16, s_signature_HasThis_Void_TypeLibTypeFlags };
		s_signaturesOfWebMethodAttribute = new byte[5][] { s_signature_HasThis_Void, s_signature_HasThis_Void_Boolean, s_signature_HasThis_Void_Boolean_TransactionOption, s_signature_HasThis_Void_Boolean_TransactionOption_Int32, s_signature_HasThis_Void_Boolean_TransactionOption_Int32_Boolean };
		s_signaturesOfHostProtectionAttribute = new byte[2][] { s_signature_HasThis_Void, s_signature_HasThis_Void_SecurityAction };
		s_signaturesOfVisualBasicEmbedded = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfCodeAnalysisEmbedded = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfVisualBasicComClassAttribute = new byte[4][] { s_signature_HasThis_Void, s_signature_HasThis_Void_String, s_signature_HasThis_Void_String_String, s_signature_HasThis_Void_String_String_String };
		s_signaturesOfClassInterfaceAttribute = new byte[2][] { s_signature_HasThis_Void_Int16, s_signature_HasThis_Void_ClassInterfaceType };
		s_signaturesOfInterfaceTypeAttribute = new byte[2][] { s_signature_HasThis_Void_Int16, s_signature_HasThis_Void_ComInterfaceType };
		s_signaturesOfCompilationRelaxationsAttribute = new byte[2][] { s_signature_HasThis_Void_Int32, s_signature_HasThis_Void_CompilationRelaxations };
		s_signaturesOfReferenceAssemblyAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfDebuggableAttribute = new byte[2][] { s_signature_HasThis_Void_Boolean_Boolean, s_signature_HasThis_Void_DebuggingModes };
		s_signaturesOfComSourceInterfacesAttribute = new byte[5][] { s_signature_HasThis_Void_String, s_signature_HasThis_Void_Type, s_signature_HasThis_Void_Type_Type, s_signature_HasThis_Void_Type_Type_Type, s_signature_HasThis_Void_Type_Type_Type_Type };
		s_signaturesOfComVisibleAttribute = new byte[1][] { s_signature_HasThis_Void_Boolean };
		s_signaturesOfConditionalAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfTypeLibVersionAttribute = new byte[1][] { s_signature_HasThis_Void_Int32_Int32 };
		s_signaturesOfComCompatibleVersionAttribute = new byte[1][] { s_signature_HasThis_Void_Int32_Int32_Int32_Int32 };
		s_signaturesOfWindowsRuntimeImportAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfDynamicSecurityMethodAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfRequiredAttributeAttribute = new byte[1][] { s_signature_HasThis_Void_Type };
		s_signaturesOfAsyncMethodBuilderAttribute = new byte[1][] { s_signature_HasThis_Void_Type };
		s_signaturesOfAsyncStateMachineAttribute = new byte[1][] { s_signature_HasThis_Void_Type };
		s_signaturesOfIteratorStateMachineAttribute = new byte[1][] { s_signature_HasThis_Void_Type };
		s_signaturesOfRuntimeCompatibilityAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfTypeForwardedToAttribute = new byte[1][] { s_signature_HasThis_Void_Type };
		s_signaturesOfSTAThreadAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfMTAThreadAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfOptionCompareAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfObsoleteAttribute = new byte[3][] { s_signature_HasThis_Void, s_signature_HasThis_Void_String, s_signature_HasThis_Void_String_Boolean };
		s_signaturesOfDynamicAttribute = new byte[2][] { s_signature_HasThis_Void, s_signature_HasThis_Void_SzArray_Boolean };
		s_signaturesOfTupleElementNamesAttribute = new byte[2][] { s_signature_HasThis_Void, s_signature_HasThis_Void_SzArray_String };
		s_signaturesOfIsByRefLikeAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfDebuggerHiddenAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfDebuggerNonUserCodeAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfDebuggerStepperBoundaryAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfDebuggerStepThroughAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfSecurityCriticalAttribute = new byte[2][] { s_signature_HasThis_Void, s_signature_HasThis_Void_SecurityCriticalScope };
		s_signaturesOfSecuritySafeCriticalAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfDesignerGeneratedAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfMyGroupCollectionAttribute = new byte[1][] { s_signature_HasThis_Void_String_String_String_String };
		s_signaturesOfComEventInterfaceAttribute = new byte[1][] { s_signature_HasThis_Void_Type_Type };
		s_signaturesOfBestFitMappingAttribute = new byte[1][] { s_signature_HasThis_Void_Boolean };
		s_signaturesOfFlagsAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfLCIDConversionAttribute = new byte[1][] { s_signature_HasThis_Void_Int32 };
		s_signaturesOfUnmanagedFunctionPointerAttribute = new byte[1][] { s_signature_HasThis_Void_CallingConvention };
		s_signaturesOfPrimaryInteropAssemblyAttribute = new byte[1][] { s_signature_HasThis_Void_Int32_Int32 };
		s_signaturesOfImportedFromTypeLibAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfDefaultEventAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyConfigurationAttribute = new byte[1][] { s_signature_HasThis_Void_String };
		s_signaturesOfAssemblyAlgorithmIdAttribute = new byte[2][] { s_signature_HasThis_Void_AssemblyHashAlgorithm, s_signature_HasThis_Void_UInt32 };
		s_signaturesOfDeprecatedAttribute = new byte[4][] { s_signature_HasThis_Void_String_DeprecationType_UInt32, s_signature_HasThis_Void_String_DeprecationType_UInt32_Platform, s_signature_HasThis_Void_String_DeprecationType_UInt32_Type, s_signature_HasThis_Void_String_DeprecationType_UInt32_String };
		s_signaturesOfExperimentalAttribute = new byte[1][] { s_signature_HasThis_Void };
		s_signaturesOfExcludeFromCodeCoverageAttribute = new byte[1][] { s_signature_HasThis_Void };
		OptionalAttribute = new AttributeDescription("System.Runtime.InteropServices", "OptionalAttribute", s_signaturesOfOptionalAttribute);
		ComImportAttribute = new AttributeDescription("System.Runtime.InteropServices", "ComImportAttribute", s_signaturesOfComImportAttribute);
		AttributeUsageAttribute = new AttributeDescription("System", "AttributeUsageAttribute", s_signaturesOfAttributeUsage);
		ConditionalAttribute = new AttributeDescription("System.Diagnostics", "ConditionalAttribute", s_signaturesOfConditionalAttribute);
		CaseInsensitiveExtensionAttribute = new AttributeDescription("System.Runtime.CompilerServices", "ExtensionAttribute", s_signaturesOfExtensionAttribute, matchIgnoringCase: true);
		CaseSensitiveExtensionAttribute = new AttributeDescription("System.Runtime.CompilerServices", "ExtensionAttribute", s_signaturesOfExtensionAttribute);
		InternalsVisibleToAttribute = new AttributeDescription("System.Runtime.CompilerServices", "InternalsVisibleToAttribute", s_signaturesOfInternalsVisibleToAttribute);
		AssemblySignatureKeyAttribute = new AttributeDescription("System.Reflection", "AssemblySignatureKeyAttribute", s_signaturesOfAssemblySignatureKeyAttribute);
		AssemblyKeyFileAttribute = new AttributeDescription("System.Reflection", "AssemblyKeyFileAttribute", s_signaturesOfAssemblyKeyFileAttribute);
		AssemblyKeyNameAttribute = new AttributeDescription("System.Reflection", "AssemblyKeyNameAttribute", s_signaturesOfAssemblyKeyNameAttribute);
		ParamArrayAttribute = new AttributeDescription("System", "ParamArrayAttribute", s_signaturesOfParamArrayAttribute);
		DefaultMemberAttribute = new AttributeDescription("System.Reflection", "DefaultMemberAttribute", s_signaturesOfDefaultMemberAttribute);
		IndexerNameAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IndexerNameAttribute", s_signaturesOfIndexerNameAttribute);
		AssemblyDelaySignAttribute = new AttributeDescription("System.Reflection", "AssemblyDelaySignAttribute", s_signaturesOfAssemblyDelaySignAttribute);
		AssemblyVersionAttribute = new AttributeDescription("System.Reflection", "AssemblyVersionAttribute", s_signaturesOfAssemblyVersionAttribute);
		AssemblyFileVersionAttribute = new AttributeDescription("System.Reflection", "AssemblyFileVersionAttribute", s_signaturesOfAssemblyFileVersionAttribute);
		AssemblyTitleAttribute = new AttributeDescription("System.Reflection", "AssemblyTitleAttribute", s_signaturesOfAssemblyTitleAttribute);
		AssemblyDescriptionAttribute = new AttributeDescription("System.Reflection", "AssemblyDescriptionAttribute", s_signaturesOfAssemblyDescriptionAttribute);
		AssemblyCultureAttribute = new AttributeDescription("System.Reflection", "AssemblyCultureAttribute", s_signaturesOfAssemblyCultureAttribute);
		AssemblyCompanyAttribute = new AttributeDescription("System.Reflection", "AssemblyCompanyAttribute", s_signaturesOfAssemblyCompanyAttribute);
		AssemblyProductAttribute = new AttributeDescription("System.Reflection", "AssemblyProductAttribute", s_signaturesOfAssemblyProductAttribute);
		AssemblyInformationalVersionAttribute = new AttributeDescription("System.Reflection", "AssemblyInformationalVersionAttribute", s_signaturesOfAssemblyInformationalVersionAttribute);
		AssemblyCopyrightAttribute = new AttributeDescription("System.Reflection", "AssemblyCopyrightAttribute", s_signaturesOfAssemblyCopyrightAttribute);
		SatelliteContractVersionAttribute = new AttributeDescription("System.Resources", "SatelliteContractVersionAttribute", s_signaturesOfSatelliteContractVersionAttribute);
		AssemblyTrademarkAttribute = new AttributeDescription("System.Reflection", "AssemblyTrademarkAttribute", s_signaturesOfAssemblyTrademarkAttribute);
		AssemblyFlagsAttribute = new AttributeDescription("System.Reflection", "AssemblyFlagsAttribute", s_signaturesOfAssemblyFlagsAttribute);
		DecimalConstantAttribute = new AttributeDescription("System.Runtime.CompilerServices", "DecimalConstantAttribute", s_signaturesOfDecimalConstantAttribute);
		IUnknownConstantAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IUnknownConstantAttribute", s_signaturesOfIUnknownConstantAttribute);
		CallerFilePathAttribute = new AttributeDescription("System.Runtime.CompilerServices", "CallerFilePathAttribute", s_signaturesOfCallerFilePathAttribute);
		CallerLineNumberAttribute = new AttributeDescription("System.Runtime.CompilerServices", "CallerLineNumberAttribute", s_signaturesOfCallerLineNumberAttribute);
		CallerMemberNameAttribute = new AttributeDescription("System.Runtime.CompilerServices", "CallerMemberNameAttribute", s_signaturesOfCallerMemberNameAttribute);
		IDispatchConstantAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IDispatchConstantAttribute", s_signaturesOfIDispatchConstantAttribute);
		DefaultParameterValueAttribute = new AttributeDescription("System.Runtime.InteropServices", "DefaultParameterValueAttribute", s_signaturesOfDefaultParameterValueAttribute);
		UnverifiableCodeAttribute = new AttributeDescription("System.Runtime.InteropServices", "UnverifiableCodeAttribute", s_signaturesOfUnverifiableCodeAttribute);
		SecurityPermissionAttribute = new AttributeDescription("System.Runtime.InteropServices", "SecurityPermissionAttribute", s_signaturesOfSecurityPermissionAttribute);
		DllImportAttribute = new AttributeDescription("System.Runtime.InteropServices", "DllImportAttribute", s_signaturesOfDllImportAttribute);
		MethodImplAttribute = new AttributeDescription("System.Runtime.CompilerServices", "MethodImplAttribute", s_signaturesOfMethodImplAttribute);
		PreserveSigAttribute = new AttributeDescription("System.Runtime.InteropServices", "PreserveSigAttribute", s_signaturesOfPreserveSigAttribute);
		DefaultCharSetAttribute = new AttributeDescription("System.Runtime.InteropServices", "DefaultCharSetAttribute", s_signaturesOfDefaultCharSetAttribute);
		SpecialNameAttribute = new AttributeDescription("System.Runtime.CompilerServices", "SpecialNameAttribute", s_signaturesOfSpecialNameAttribute);
		SerializableAttribute = new AttributeDescription("System", "SerializableAttribute", s_signaturesOfSerializableAttribute);
		NonSerializedAttribute = new AttributeDescription("System", "NonSerializedAttribute", s_signaturesOfNonSerializedAttribute);
		StructLayoutAttribute = new AttributeDescription("System.Runtime.InteropServices", "StructLayoutAttribute", s_signaturesOfStructLayoutAttribute);
		FieldOffsetAttribute = new AttributeDescription("System.Runtime.InteropServices", "FieldOffsetAttribute", s_signaturesOfFieldOffsetAttribute);
		FixedBufferAttribute = new AttributeDescription("System.Runtime.CompilerServices", "FixedBufferAttribute", s_signaturesOfFixedBufferAttribute);
		MarshalAsAttribute = new AttributeDescription("System.Runtime.InteropServices", "MarshalAsAttribute", s_signaturesOfMarshalAsAttribute);
		InAttribute = new AttributeDescription("System.Runtime.InteropServices", "InAttribute", s_signaturesOfInAttribute);
		OutAttribute = new AttributeDescription("System.Runtime.InteropServices", "OutAttribute", s_signaturesOfOutAttribute);
		IsReadOnlyAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IsReadOnlyAttribute", s_signaturesOfIsReadOnlyAttribute);
		IsUnmanagedAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IsUnmanagedAttribute", s_signaturesOfIsUnmanagedAttribute);
		CoClassAttribute = new AttributeDescription("System.Runtime.InteropServices", "CoClassAttribute", s_signaturesOfCoClassAttribute);
		GuidAttribute = new AttributeDescription("System.Runtime.InteropServices", "GuidAttribute", s_signaturesOfGuidAttribute);
		CLSCompliantAttribute = new AttributeDescription("System", "CLSCompliantAttribute", s_signaturesOfCLSCompliantAttribute);
		HostProtectionAttribute = new AttributeDescription("System.Security.Permissions", "HostProtectionAttribute", s_signaturesOfHostProtectionAttribute);
		SuppressUnmanagedCodeSecurityAttribute = new AttributeDescription("System.Security", "SuppressUnmanagedCodeSecurityAttribute", s_signaturesOfSuppressUnmanagedCodeSecurityAttribute);
		PrincipalPermissionAttribute = new AttributeDescription("System.Security.Permissions", "PrincipalPermissionAttribute", s_signaturesOfPrincipalPermissionAttribute);
		PermissionSetAttribute = new AttributeDescription("System.Security.Permissions", "PermissionSetAttribute", s_signaturesOfPermissionSetAttribute);
		TypeIdentifierAttribute = new AttributeDescription("System.Runtime.InteropServices", "TypeIdentifierAttribute", s_signaturesOfTypeIdentifierAttribute);
		VisualBasicEmbeddedAttribute = new AttributeDescription("Microsoft.VisualBasic", "Embedded", s_signaturesOfVisualBasicEmbedded);
		CodeAnalysisEmbeddedAttribute = new AttributeDescription("Microsoft.CodeAnalysis", "EmbeddedAttribute", s_signaturesOfCodeAnalysisEmbedded);
		VisualBasicComClassAttribute = new AttributeDescription("Microsoft.VisualBasic", "ComClassAttribute", s_signaturesOfVisualBasicComClassAttribute);
		StandardModuleAttribute = new AttributeDescription("Microsoft.VisualBasic.CompilerServices", "StandardModuleAttribute", s_signaturesOfStandardModuleAttribute);
		OptionCompareAttribute = new AttributeDescription("Microsoft.VisualBasic.CompilerServices", "OptionCompareAttribute", s_signaturesOfOptionCompareAttribute);
		AccessedThroughPropertyAttribute = new AttributeDescription("System.Runtime.CompilerServices", "AccessedThroughPropertyAttribute", s_signaturesOfAccessedThroughPropertyAttribute);
		WebMethodAttribute = new AttributeDescription("System.Web.Services", "WebMethodAttribute", s_signaturesOfWebMethodAttribute);
		DateTimeConstantAttribute = new AttributeDescription("System.Runtime.CompilerServices", "DateTimeConstantAttribute", s_signaturesOfDateTimeConstantAttribute);
		ClassInterfaceAttribute = new AttributeDescription("System.Runtime.InteropServices", "ClassInterfaceAttribute", s_signaturesOfClassInterfaceAttribute);
		ComSourceInterfacesAttribute = new AttributeDescription("System.Runtime.InteropServices", "ComSourceInterfacesAttribute", s_signaturesOfComSourceInterfacesAttribute);
		ComVisibleAttribute = new AttributeDescription("System.Runtime.InteropServices", "ComVisibleAttribute", s_signaturesOfComVisibleAttribute);
		DispIdAttribute = new AttributeDescription("System.Runtime.InteropServices", "DispIdAttribute", new byte[1][] { s_signature_HasThis_Void_Int32 });
		TypeLibVersionAttribute = new AttributeDescription("System.Runtime.InteropServices", "TypeLibVersionAttribute", s_signaturesOfTypeLibVersionAttribute);
		ComCompatibleVersionAttribute = new AttributeDescription("System.Runtime.InteropServices", "ComCompatibleVersionAttribute", s_signaturesOfComCompatibleVersionAttribute);
		InterfaceTypeAttribute = new AttributeDescription("System.Runtime.InteropServices", "InterfaceTypeAttribute", s_signaturesOfInterfaceTypeAttribute);
		WindowsRuntimeImportAttribute = new AttributeDescription("System.Runtime.InteropServices.WindowsRuntime", "WindowsRuntimeImportAttribute", s_signaturesOfWindowsRuntimeImportAttribute);
		DynamicSecurityMethodAttribute = new AttributeDescription("System.Security", "DynamicSecurityMethodAttribute", s_signaturesOfDynamicSecurityMethodAttribute);
		RequiredAttributeAttribute = new AttributeDescription("System.Runtime.CompilerServices", "RequiredAttributeAttribute", s_signaturesOfRequiredAttributeAttribute);
		AsyncMethodBuilderAttribute = new AttributeDescription("System.Runtime.CompilerServices", "AsyncMethodBuilderAttribute", s_signaturesOfAsyncMethodBuilderAttribute);
		AsyncStateMachineAttribute = new AttributeDescription("System.Runtime.CompilerServices", "AsyncStateMachineAttribute", s_signaturesOfAsyncStateMachineAttribute);
		IteratorStateMachineAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IteratorStateMachineAttribute", s_signaturesOfIteratorStateMachineAttribute);
		CompilationRelaxationsAttribute = new AttributeDescription("System.Runtime.CompilerServices", "CompilationRelaxationsAttribute", s_signaturesOfCompilationRelaxationsAttribute);
		ReferenceAssemblyAttribute = new AttributeDescription("System.Runtime.CompilerServices", "ReferenceAssemblyAttribute", s_signaturesOfReferenceAssemblyAttribute);
		RuntimeCompatibilityAttribute = new AttributeDescription("System.Runtime.CompilerServices", "RuntimeCompatibilityAttribute", s_signaturesOfRuntimeCompatibilityAttribute);
		DebuggableAttribute = new AttributeDescription("System.Diagnostics", "DebuggableAttribute", s_signaturesOfDebuggableAttribute);
		TypeForwardedToAttribute = new AttributeDescription("System.Runtime.CompilerServices", "TypeForwardedToAttribute", s_signaturesOfTypeForwardedToAttribute);
		STAThreadAttribute = new AttributeDescription("System", "STAThreadAttribute", s_signaturesOfSTAThreadAttribute);
		MTAThreadAttribute = new AttributeDescription("System", "MTAThreadAttribute", s_signaturesOfMTAThreadAttribute);
		ObsoleteAttribute = new AttributeDescription("System", "ObsoleteAttribute", s_signaturesOfObsoleteAttribute);
		TypeLibTypeAttribute = new AttributeDescription("System.Runtime.InteropServices", "TypeLibTypeAttribute", s_signaturesOfTypeLibTypeAttribute);
		DynamicAttribute = new AttributeDescription("System.Runtime.CompilerServices", "DynamicAttribute", s_signaturesOfDynamicAttribute);
		TupleElementNamesAttribute = new AttributeDescription("System.Runtime.CompilerServices", "TupleElementNamesAttribute", s_signaturesOfTupleElementNamesAttribute);
		IsByRefLikeAttribute = new AttributeDescription("System.Runtime.CompilerServices", "IsByRefLikeAttribute", s_signaturesOfIsByRefLikeAttribute);
		DebuggerHiddenAttribute = new AttributeDescription("System.Diagnostics", "DebuggerHiddenAttribute", s_signaturesOfDebuggerHiddenAttribute);
		DebuggerNonUserCodeAttribute = new AttributeDescription("System.Diagnostics", "DebuggerNonUserCodeAttribute", s_signaturesOfDebuggerNonUserCodeAttribute);
		DebuggerStepperBoundaryAttribute = new AttributeDescription("System.Diagnostics", "DebuggerStepperBoundaryAttribute", s_signaturesOfDebuggerStepperBoundaryAttribute);
		DebuggerStepThroughAttribute = new AttributeDescription("System.Diagnostics", "DebuggerStepThroughAttribute", s_signaturesOfDebuggerStepThroughAttribute);
		SecurityCriticalAttribute = new AttributeDescription("System.Security", "SecurityCriticalAttribute", s_signaturesOfSecurityCriticalAttribute);
		SecuritySafeCriticalAttribute = new AttributeDescription("System.Security", "SecuritySafeCriticalAttribute", s_signaturesOfSecuritySafeCriticalAttribute);
		DesignerGeneratedAttribute = new AttributeDescription("Microsoft.VisualBasic.CompilerServices", "DesignerGeneratedAttribute", s_signaturesOfDesignerGeneratedAttribute);
		MyGroupCollectionAttribute = new AttributeDescription("Microsoft.VisualBasic", "MyGroupCollectionAttribute", s_signaturesOfMyGroupCollectionAttribute);
		ComEventInterfaceAttribute = new AttributeDescription("System.Runtime.InteropServices", "ComEventInterfaceAttribute", s_signaturesOfComEventInterfaceAttribute);
		BestFitMappingAttribute = new AttributeDescription("System.Runtime.InteropServices", "BestFitMappingAttribute", s_signaturesOfBestFitMappingAttribute);
		FlagsAttribute = new AttributeDescription("System", "FlagsAttribute", s_signaturesOfFlagsAttribute);
		LCIDConversionAttribute = new AttributeDescription("System.Runtime.InteropServices", "LCIDConversionAttribute", s_signaturesOfLCIDConversionAttribute);
		UnmanagedFunctionPointerAttribute = new AttributeDescription("System.Runtime.InteropServices", "UnmanagedFunctionPointerAttribute", s_signaturesOfUnmanagedFunctionPointerAttribute);
		PrimaryInteropAssemblyAttribute = new AttributeDescription("System.Runtime.InteropServices", "PrimaryInteropAssemblyAttribute", s_signaturesOfPrimaryInteropAssemblyAttribute);
		ImportedFromTypeLibAttribute = new AttributeDescription("System.Runtime.InteropServices", "ImportedFromTypeLibAttribute", s_signaturesOfImportedFromTypeLibAttribute);
		DefaultEventAttribute = new AttributeDescription("System.ComponentModel", "DefaultEventAttribute", s_signaturesOfDefaultEventAttribute);
		AssemblyConfigurationAttribute = new AttributeDescription("System.Reflection", "AssemblyConfigurationAttribute", s_signaturesOfAssemblyConfigurationAttribute);
		AssemblyAlgorithmIdAttribute = new AttributeDescription("System.Reflection", "AssemblyAlgorithmIdAttribute", s_signaturesOfAssemblyAlgorithmIdAttribute);
		DeprecatedAttribute = new AttributeDescription("Windows.Foundation.Metadata", "DeprecatedAttribute", s_signaturesOfDeprecatedAttribute);
		ExperimentalAttribute = new AttributeDescription("Windows.Foundation.Metadata", "ExperimentalAttribute", s_signaturesOfExperimentalAttribute);
		ExcludeFromCodeCoverageAttribute = new AttributeDescription("System.Diagnostics.CodeAnalysis", "ExcludeFromCodeCoverageAttribute", s_signaturesOfExcludeFromCodeCoverageAttribute);
		TypeHandleTargets = new TypeHandleTargetInfo[19]
		{
			new TypeHandleTargetInfo("System", "AttributeTargets", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Reflection", "AssemblyNameFlags", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.CompilerServices", "MethodImplOptions", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "CharSet", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "LayoutKind", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "UnmanagedType", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "TypeLibTypeFlags", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "ClassInterfaceType", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "ComInterfaceType", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.CompilerServices", "CompilationRelaxations", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Diagnostics.DebuggableAttribute", "DebuggingModes", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Security", "SecurityCriticalScope", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Runtime.InteropServices", "CallingConvention", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Configuration.Assemblies", "AssemblyHashAlgorithm", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.EnterpriseServices", "TransactionOption", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System.Security.Permissions", "SecurityAction", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("System", "Type", SerializationTypeCode.Type),
			new TypeHandleTargetInfo("Windows.Foundation.Metadata", "DeprecationType", SerializationTypeCode.Int32),
			new TypeHandleTargetInfo("Windows.Foundation.Metadata", "Platform", SerializationTypeCode.Int32)
		}.AsImmutable();
	}
}
