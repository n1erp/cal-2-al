using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class CodeAnalysisResources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Microsoft.CodeAnalysis.CodeAnalysisResources", typeof(CodeAnalysisResources).GetTypeInfo().Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string AbsolutePathExpected => ResourceManager.GetString("AbsolutePathExpected", resourceCulture);

	internal static string AnalyzerDriverFailure => ResourceManager.GetString("AnalyzerDriverFailure", resourceCulture);

	internal static string AnalyzerDriverThrows => ResourceManager.GetString("AnalyzerDriverThrows", resourceCulture);

	internal static string AnalyzerDriverThrowsDescription => ResourceManager.GetString("AnalyzerDriverThrowsDescription", resourceCulture);

	internal static string AnalyzerExecutionTimeColumnHeader => ResourceManager.GetString("AnalyzerExecutionTimeColumnHeader", resourceCulture);

	internal static string AnalyzerNameColumnHeader => ResourceManager.GetString("AnalyzerNameColumnHeader", resourceCulture);

	internal static string AnalyzerTotalExecutionTime => ResourceManager.GetString("AnalyzerTotalExecutionTime", resourceCulture);

	internal static string AnonymousTypeArgumentCountMismatch2 => ResourceManager.GetString("AnonymousTypeArgumentCountMismatch2", resourceCulture);

	internal static string AnonymousTypeMemberAndNamesCountMismatch2 => ResourceManager.GetString("AnonymousTypeMemberAndNamesCountMismatch2", resourceCulture);

	internal static string ArgumentCannotBeEmpty => ResourceManager.GetString("ArgumentCannotBeEmpty", resourceCulture);

	internal static string ArgumentElementCannotBeNull => ResourceManager.GetString("ArgumentElementCannotBeNull", resourceCulture);

	internal static string Arrays_with_more_than_one_dimension_cannot_be_serialized => ResourceManager.GetString("Arrays_with_more_than_one_dimension_cannot_be_serialized", resourceCulture);

	internal static string Assembly => ResourceManager.GetString("Assembly", resourceCulture);

	internal static string AssemblyFileNotFound => ResourceManager.GetString("AssemblyFileNotFound", resourceCulture);

	internal static string AssemblyMustHaveAtLeastOneModule => ResourceManager.GetString("AssemblyMustHaveAtLeastOneModule", resourceCulture);

	internal static string AssemblySigningNotSupported => ResourceManager.GetString("AssemblySigningNotSupported", resourceCulture);

	internal static string AsyncAnalyzerActionCannotBeRegistered => ResourceManager.GetString("AsyncAnalyzerActionCannotBeRegistered", resourceCulture);

	internal static string Cannot_deserialize_type_0 => ResourceManager.GetString("Cannot_deserialize_type_0", resourceCulture);

	internal static string Cannot_serialize_type_0 => ResourceManager.GetString("Cannot_serialize_type_0", resourceCulture);

	internal static string CannotAliasModule => ResourceManager.GetString("CannotAliasModule", resourceCulture);

	internal static string CannotCreateReferenceToModule => ResourceManager.GetString("CannotCreateReferenceToModule", resourceCulture);

	internal static string CannotCreateReferenceToSubmission => ResourceManager.GetString("CannotCreateReferenceToSubmission", resourceCulture);

	internal static string CannotEmbedInteropTypesFromModule => ResourceManager.GetString("CannotEmbedInteropTypesFromModule", resourceCulture);

	internal static string CannotTargetNetModuleWhenEmittingRefAssembly => ResourceManager.GetString("CannotTargetNetModuleWhenEmittingRefAssembly", resourceCulture);

	internal static string CantCreateModuleReferenceToAssembly => ResourceManager.GetString("CantCreateModuleReferenceToAssembly", resourceCulture);

	internal static string CantCreateReferenceToAssemblyWithoutLocation => ResourceManager.GetString("CantCreateReferenceToAssemblyWithoutLocation", resourceCulture);

	internal static string CantCreateReferenceToDynamicAssembly => ResourceManager.GetString("CantCreateReferenceToDynamicAssembly", resourceCulture);

	internal static string ChangesMustNotOverlap => ResourceManager.GetString("ChangesMustNotOverlap", resourceCulture);

	internal static string Class1 => ResourceManager.GetString("Class1", resourceCulture);

	internal static string CoffResourceInvalidRelocation => ResourceManager.GetString("CoffResourceInvalidRelocation", resourceCulture);

	internal static string CoffResourceInvalidSectionSize => ResourceManager.GetString("CoffResourceInvalidSectionSize", resourceCulture);

	internal static string CoffResourceInvalidSymbol => ResourceManager.GetString("CoffResourceInvalidSymbol", resourceCulture);

	internal static string CoffResourceMissingSection => ResourceManager.GetString("CoffResourceMissingSection", resourceCulture);

	internal static string CompilationOptionsMustNotHaveErrors => ResourceManager.GetString("CompilationOptionsMustNotHaveErrors", resourceCulture);

	internal static string CompilationReferencesAssembliesWithDifferentAutoGeneratedVersion => ResourceManager.GetString("CompilationReferencesAssembliesWithDifferentAutoGeneratedVersion", resourceCulture);

	internal static string CompilerAnalyzerFailure => ResourceManager.GetString("CompilerAnalyzerFailure", resourceCulture);

	internal static string CompilerAnalyzerThrows => ResourceManager.GetString("CompilerAnalyzerThrows", resourceCulture);

	internal static string CompilerAnalyzerThrowsDescription => ResourceManager.GetString("CompilerAnalyzerThrowsDescription", resourceCulture);

	internal static string Constructor => ResourceManager.GetString("Constructor", resourceCulture);

	internal static string Delegate1 => ResourceManager.GetString("Delegate1", resourceCulture);

	internal static string Deserialization_reader_for_0_read_incorrect_number_of_values => ResourceManager.GetString("Deserialization_reader_for_0_read_incorrect_number_of_values", resourceCulture);

	internal static string DiagnosticIdCantBeNullOrWhitespace => ResourceManager.GetString("DiagnosticIdCantBeNullOrWhitespace", resourceCulture);

	internal static string DuplicateAnalyzerInstances => ResourceManager.GetString("DuplicateAnalyzerInstances", resourceCulture);

	internal static string ElementIsExpected => ResourceManager.GetString("ElementIsExpected", resourceCulture);

	internal static string EmbeddedTextsRequirePdb => ResourceManager.GetString("EmbeddedTextsRequirePdb", resourceCulture);

	internal static string EmbeddingPdbUnexpectedWhenEmittingMetadata => ResourceManager.GetString("EmbeddingPdbUnexpectedWhenEmittingMetadata", resourceCulture);

	internal static string EmptyKeyInPathMap => ResourceManager.GetString("EmptyKeyInPathMap", resourceCulture);

	internal static string EmptyOrInvalidFileName => ResourceManager.GetString("EmptyOrInvalidFileName", resourceCulture);

	internal static string EmptyOrInvalidResourceName => ResourceManager.GetString("EmptyOrInvalidResourceName", resourceCulture);

	internal static string EndMustNotBeLessThanStart => ResourceManager.GetString("EndMustNotBeLessThanStart", resourceCulture);

	internal static string Enum1 => ResourceManager.GetString("Enum1", resourceCulture);

	internal static string Event1 => ResourceManager.GetString("Event1", resourceCulture);

	internal static string ExceptionContext => ResourceManager.GetString("ExceptionContext", resourceCulture);

	internal static string ExceptionEnablingMulticoreJit => ResourceManager.GetString("ExceptionEnablingMulticoreJit", resourceCulture);

	internal static string ExpectedNonEmptyPublicKey => ResourceManager.GetString("ExpectedNonEmptyPublicKey", resourceCulture);

	internal static string FailedToResolveRuleSetName => ResourceManager.GetString("FailedToResolveRuleSetName", resourceCulture);

	internal static string Field => ResourceManager.GetString("Field", resourceCulture);

	internal static string FileNotFound => ResourceManager.GetString("FileNotFound", resourceCulture);

	internal static string FileSizeExceedsMaximumAllowed => ResourceManager.GetString("FileSizeExceedsMaximumAllowed", resourceCulture);

	internal static string FlowAnalysisFeatureDisabled => ResourceManager.GetString("FlowAnalysisFeatureDisabled", resourceCulture);

	internal static string GetMetadataMustReturnInstance => ResourceManager.GetString("GetMetadataMustReturnInstance", resourceCulture);

	internal static string IconStreamUnexpectedFormat => ResourceManager.GetString("IconStreamUnexpectedFormat", resourceCulture);

	internal static string IncludingPrivateMembersUnexpectedWhenEmittingToMetadataPeStream => ResourceManager.GetString("IncludingPrivateMembersUnexpectedWhenEmittingToMetadataPeStream", resourceCulture);

	internal static string InconsistentLanguageVersions => ResourceManager.GetString("InconsistentLanguageVersions", resourceCulture);

	internal static string InconsistentSyntaxTreeFeature => ResourceManager.GetString("InconsistentSyntaxTreeFeature", resourceCulture);

	internal static string InMemoryAssembly => ResourceManager.GetString("InMemoryAssembly", resourceCulture);

	internal static string InMemoryModule => ResourceManager.GetString("InMemoryModule", resourceCulture);

	internal static string Interface1 => ResourceManager.GetString("Interface1", resourceCulture);

	internal static string InvalidAlias => ResourceManager.GetString("InvalidAlias", resourceCulture);

	internal static string InvalidAssemblyName => ResourceManager.GetString("InvalidAssemblyName", resourceCulture);

	internal static string InvalidCharactersInAssemblyCultureName => ResourceManager.GetString("InvalidCharactersInAssemblyCultureName", resourceCulture);

	internal static string InvalidCharactersInAssemblyName => ResourceManager.GetString("InvalidCharactersInAssemblyName", resourceCulture);

	internal static string InvalidCompilationOptions => ResourceManager.GetString("InvalidCompilationOptions", resourceCulture);

	internal static string InvalidContentType => ResourceManager.GetString("InvalidContentType", resourceCulture);

	internal static string InvalidCultureName => ResourceManager.GetString("InvalidCultureName", resourceCulture);

	internal static string InvalidDataAtOffset => ResourceManager.GetString("InvalidDataAtOffset", resourceCulture);

	internal static string InvalidDiagnosticIdReported => ResourceManager.GetString("InvalidDiagnosticIdReported", resourceCulture);

	internal static string InvalidDiagnosticLocationReported => ResourceManager.GetString("InvalidDiagnosticLocationReported", resourceCulture);

	internal static string InvalidDiagnosticSpanReported => ResourceManager.GetString("InvalidDiagnosticSpanReported", resourceCulture);

	internal static string InvalidHash => ResourceManager.GetString("InvalidHash", resourceCulture);

	internal static string InvalidModuleName => ResourceManager.GetString("InvalidModuleName", resourceCulture);

	internal static string InvalidNodeToTrack => ResourceManager.GetString("InvalidNodeToTrack", resourceCulture);

	internal static string InvalidOperationBlockForAnalysisContext => ResourceManager.GetString("InvalidOperationBlockForAnalysisContext", resourceCulture);

	internal static string InvalidOutputKindForSubmission => ResourceManager.GetString("InvalidOutputKindForSubmission", resourceCulture);

	internal static string InvalidPublicKey => ResourceManager.GetString("InvalidPublicKey", resourceCulture);

	internal static string InvalidPublicKeyToken => ResourceManager.GetString("InvalidPublicKeyToken", resourceCulture);

	internal static string InvalidRuleSetInclude => ResourceManager.GetString("InvalidRuleSetInclude", resourceCulture);

	internal static string InvalidSizeOfPublicKeyToken => ResourceManager.GetString("InvalidSizeOfPublicKeyToken", resourceCulture);

	internal static string InvalidTree => ResourceManager.GetString("InvalidTree", resourceCulture);

	internal static string KeepAliveIsNotAnInteger => ResourceManager.GetString("KeepAliveIsNotAnInteger", resourceCulture);

	internal static string KeepAliveIsTooSmall => ResourceManager.GetString("KeepAliveIsTooSmall", resourceCulture);

	internal static string KeepAliveWithoutShared => ResourceManager.GetString("KeepAliveWithoutShared", resourceCulture);

	internal static string MetadataPeStreamUnexpectedWhenEmittingMetadataOnly => ResourceManager.GetString("MetadataPeStreamUnexpectedWhenEmittingMetadataOnly", resourceCulture);

	internal static string MetadataRefNotFoundToRemove1 => ResourceManager.GetString("MetadataRefNotFoundToRemove1", resourceCulture);

	internal static string Method => ResourceManager.GetString("Method", resourceCulture);

	internal static string MismatchedVersion => ResourceManager.GetString("MismatchedVersion", resourceCulture);

	internal static string MissingKeepAlive => ResourceManager.GetString("MissingKeepAlive", resourceCulture);

	internal static string MissingListItem => ResourceManager.GetString("MissingListItem", resourceCulture);

	internal static string Module => ResourceManager.GetString("Module", resourceCulture);

	internal static string ModuleCopyCannotBeUsedToCreateAssemblyMetadata => ResourceManager.GetString("ModuleCopyCannotBeUsedToCreateAssemblyMetadata", resourceCulture);

	internal static string MultithreadedAnalyzerExecutionNote => ResourceManager.GetString("MultithreadedAnalyzerExecutionNote", resourceCulture);

	internal static string MustIncludePrivateMembersUnlessRefAssembly => ResourceManager.GetString("MustIncludePrivateMembersUnlessRefAssembly", resourceCulture);

	internal static string NameCannotBeEmpty => ResourceManager.GetString("NameCannotBeEmpty", resourceCulture);

	internal static string NameCannotBeNull => ResourceManager.GetString("NameCannotBeNull", resourceCulture);

	internal static string NameCannotStartWithWhitespace => ResourceManager.GetString("NameCannotStartWithWhitespace", resourceCulture);

	internal static string NameContainsInvalidCharacter => ResourceManager.GetString("NameContainsInvalidCharacter", resourceCulture);

	internal static string NoAnalyzersFound => ResourceManager.GetString("NoAnalyzersFound", resourceCulture);

	internal static string NodeOrTokenOutOfSequence => ResourceManager.GetString("NodeOrTokenOutOfSequence", resourceCulture);

	internal static string NotARootOperation => ResourceManager.GetString("NotARootOperation", resourceCulture);

	internal static string NullValueInPathMap => ResourceManager.GetString("NullValueInPathMap", resourceCulture);

	internal static string OperationHasNullSemanticModel => ResourceManager.GetString("OperationHasNullSemanticModel", resourceCulture);

	internal static string OutputKindNotSupported => ResourceManager.GetString("OutputKindNotSupported", resourceCulture);

	internal static string Parameter => ResourceManager.GetString("Parameter", resourceCulture);

	internal static string PathReturnedByResolveMetadataFileMustBeAbsolute => ResourceManager.GetString("PathReturnedByResolveMetadataFileMustBeAbsolute", resourceCulture);

	internal static string PathReturnedByResolveStrongNameKeyFileMustBeAbsolute => ResourceManager.GetString("PathReturnedByResolveStrongNameKeyFileMustBeAbsolute", resourceCulture);

	internal static string PdbStreamUnexpectedWhenEmbedding => ResourceManager.GetString("PdbStreamUnexpectedWhenEmbedding", resourceCulture);

	internal static string PdbStreamUnexpectedWhenEmittingMetadataOnly => ResourceManager.GetString("PdbStreamUnexpectedWhenEmittingMetadataOnly", resourceCulture);

	internal static string PEImageDoesntContainManagedMetadata => ResourceManager.GetString("PEImageDoesntContainManagedMetadata", resourceCulture);

	internal static string PEImageNotAvailable => ResourceManager.GetString("PEImageNotAvailable", resourceCulture);

	internal static string PreviousSubmissionHasErrors => ResourceManager.GetString("PreviousSubmissionHasErrors", resourceCulture);

	internal static string Property => ResourceManager.GetString("Property", resourceCulture);

	internal static string ReferenceOfTypeIsInvalid1 => ResourceManager.GetString("ReferenceOfTypeIsInvalid1", resourceCulture);

	internal static string ReferenceResolverShouldReturnReadableNonNullStream => ResourceManager.GetString("ReferenceResolverShouldReturnReadableNonNullStream", resourceCulture);

	internal static string ResourceDataProviderShouldReturnNonNullStream => ResourceManager.GetString("ResourceDataProviderShouldReturnNonNullStream", resourceCulture);

	internal static string ResourceStreamEndedUnexpectedly => ResourceManager.GetString("ResourceStreamEndedUnexpectedly", resourceCulture);

	internal static string ResourceStreamProviderShouldReturnNonNullStream => ResourceManager.GetString("ResourceStreamProviderShouldReturnNonNullStream", resourceCulture);

	internal static string Return1 => ResourceManager.GetString("Return1", resourceCulture);

	internal static string ReturnTypeCannotBeValuePointerbyRefOrOpen => ResourceManager.GetString("ReturnTypeCannotBeValuePointerbyRefOrOpen", resourceCulture);

	internal static string ReturnTypeCannotBeVoidByRefOrOpen => ResourceManager.GetString("ReturnTypeCannotBeVoidByRefOrOpen", resourceCulture);

	internal static string RuleSetBadAttributeValue => ResourceManager.GetString("RuleSetBadAttributeValue", resourceCulture);

	internal static string RuleSetHasDuplicateRules => ResourceManager.GetString("RuleSetHasDuplicateRules", resourceCulture);

	internal static string RuleSetMissingAttribute => ResourceManager.GetString("RuleSetMissingAttribute", resourceCulture);

	internal static string SeparatorIsExpected => ResourceManager.GetString("SeparatorIsExpected", resourceCulture);

	internal static string SharedArgumentMissing => ResourceManager.GetString("SharedArgumentMissing", resourceCulture);

	internal static string SizeHasToBePositive => ResourceManager.GetString("SizeHasToBePositive", resourceCulture);

	internal static string SourceTextCannotBeEmbedded => ResourceManager.GetString("SourceTextCannotBeEmbedded", resourceCulture);

	internal static string SpanDoesNotIncludeEndOfLine => ResourceManager.GetString("SpanDoesNotIncludeEndOfLine", resourceCulture);

	internal static string SpanDoesNotIncludeStartOfLine => ResourceManager.GetString("SpanDoesNotIncludeStartOfLine", resourceCulture);

	internal static string StartMustNotBeNegative => ResourceManager.GetString("StartMustNotBeNegative", resourceCulture);

	internal static string Stream_contains_invalid_data => ResourceManager.GetString("Stream_contains_invalid_data", resourceCulture);

	internal static string StreamIsTooLong => ResourceManager.GetString("StreamIsTooLong", resourceCulture);

	internal static string StreamMustSupportRead => ResourceManager.GetString("StreamMustSupportRead", resourceCulture);

	internal static string StreamMustSupportReadAndSeek => ResourceManager.GetString("StreamMustSupportReadAndSeek", resourceCulture);

	internal static string StreamMustSupportWrite => ResourceManager.GetString("StreamMustSupportWrite", resourceCulture);

	internal static string Struct1 => ResourceManager.GetString("Struct1", resourceCulture);

	internal static string SymWriterDoesNotSupportSourceLink => ResourceManager.GetString("SymWriterDoesNotSupportSourceLink", resourceCulture);

	internal static string SymWriterNotDeterministic => ResourceManager.GetString("SymWriterNotDeterministic", resourceCulture);

	internal static string SymWriterOlderVersionThanRequired => ResourceManager.GetString("SymWriterOlderVersionThanRequired", resourceCulture);

	internal static string The_type_0_is_not_understood_by_the_serialization_binder => ResourceManager.GetString("The_type_0_is_not_understood_by_the_serialization_binder", resourceCulture);

	internal static string TheStreamCannotBeReadFrom => ResourceManager.GetString("TheStreamCannotBeReadFrom", resourceCulture);

	internal static string TheStreamCannotBeWrittenTo => ResourceManager.GetString("TheStreamCannotBeWrittenTo", resourceCulture);

	internal static string TupleElementLocationCountMismatch => ResourceManager.GetString("TupleElementLocationCountMismatch", resourceCulture);

	internal static string TupleElementNameCountMismatch => ResourceManager.GetString("TupleElementNameCountMismatch", resourceCulture);

	internal static string TupleElementNameEmpty => ResourceManager.GetString("TupleElementNameEmpty", resourceCulture);

	internal static string TuplesNeedAtLeastTwoElements => ResourceManager.GetString("TuplesNeedAtLeastTwoElements", resourceCulture);

	internal static string TupleUnderlyingTypeMustBeTupleCompatible => ResourceManager.GetString("TupleUnderlyingTypeMustBeTupleCompatible", resourceCulture);

	internal static string TypeMustBeASubclassOfSyntaxAnnotation => ResourceManager.GetString("TypeMustBeASubclassOfSyntaxAnnotation", resourceCulture);

	internal static string TypeMustBeSameAsHostObjectTypeOfPreviousSubmission => ResourceManager.GetString("TypeMustBeSameAsHostObjectTypeOfPreviousSubmission", resourceCulture);

	internal static string TypeParameter => ResourceManager.GetString("TypeParameter", resourceCulture);

	internal static string UnexpectedTypeOfNodeInList => ResourceManager.GetString("UnexpectedTypeOfNodeInList", resourceCulture);

	internal static string UnrecognizedResourceFileFormat => ResourceManager.GetString("UnrecognizedResourceFileFormat", resourceCulture);

	internal static string Unresolved => ResourceManager.GetString("Unresolved", resourceCulture);

	internal static string UnsupportedAnalyzerInstance => ResourceManager.GetString("UnsupportedAnalyzerInstance", resourceCulture);

	internal static string UnsupportedDiagnosticReported => ResourceManager.GetString("UnsupportedDiagnosticReported", resourceCulture);

	internal static string UnsupportedHashAlgorithm => ResourceManager.GetString("UnsupportedHashAlgorithm", resourceCulture);

	internal static string Value_too_large_to_be_represented_as_a_30_bit_unsigned_integer => ResourceManager.GetString("Value_too_large_to_be_represented_as_a_30_bit_unsigned_integer", resourceCulture);

	internal static string WinRTIdentityCantBeRetargetable => ResourceManager.GetString("WinRTIdentityCantBeRetargetable", resourceCulture);

	internal static string XmlReferencesNotSupported => ResourceManager.GetString("XmlReferencesNotSupported", resourceCulture);

	internal CodeAnalysisResources()
	{
	}
}
