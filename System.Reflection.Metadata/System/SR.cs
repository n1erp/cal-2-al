using System.Resources;
using System.Runtime.CompilerServices;
using FxResources.System.Reflection.Metadata;

namespace System;

internal static class SR
{
	private static ResourceManager s_resourceManager;

	private static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(ResourceType));

	internal static Type ResourceType { get; } = typeof(FxResources.System.Reflection.Metadata.SR);

	internal static string ImageTooSmall => GetResourceString("ImageTooSmall", null);

	internal static string InvalidCorHeaderSize => GetResourceString("InvalidCorHeaderSize", null);

	internal static string InvalidHandle => GetResourceString("InvalidHandle", null);

	internal static string UnexpectedHandleKind => GetResourceString("UnexpectedHandleKind", null);

	internal static string UnexpectedOpCode => GetResourceString("UnexpectedOpCode", null);

	internal static string InvalidLocalSignatureToken => GetResourceString("InvalidLocalSignatureToken", null);

	internal static string InvalidMetadataSectionSpan => GetResourceString("InvalidMetadataSectionSpan", null);

	internal static string InvalidMethodHeader1 => GetResourceString("InvalidMethodHeader1", null);

	internal static string InvalidMethodHeader2 => GetResourceString("InvalidMethodHeader2", null);

	internal static string InvalidPESignature => GetResourceString("InvalidPESignature", null);

	internal static string InvalidSehHeader => GetResourceString("InvalidSehHeader", null);

	internal static string InvalidToken => GetResourceString("InvalidToken", null);

	internal static string MetadataImageDoesNotRepresentAnAssembly => GetResourceString("MetadataImageDoesNotRepresentAnAssembly", null);

	internal static string StandaloneDebugMetadataImageDoesNotContainModuleTable => GetResourceString("StandaloneDebugMetadataImageDoesNotContainModuleTable", null);

	internal static string PEImageNotAvailable => GetResourceString("PEImageNotAvailable", null);

	internal static string MissingDataDirectory => GetResourceString("MissingDataDirectory", null);

	internal static string NotMetadataHeapHandle => GetResourceString("NotMetadataHeapHandle", null);

	internal static string NotMetadataTableOrUserStringHandle => GetResourceString("NotMetadataTableOrUserStringHandle", null);

	internal static string SectionTooSmall => GetResourceString("SectionTooSmall", null);

	internal static string StreamMustSupportReadAndSeek => GetResourceString("StreamMustSupportReadAndSeek", null);

	internal static string UnknownFileFormat => GetResourceString("UnknownFileFormat", null);

	internal static string UnknownPEMagicValue => GetResourceString("UnknownPEMagicValue", null);

	internal static string MetadataTableNotSorted => GetResourceString("MetadataTableNotSorted", null);

	internal static string ModuleTableInvalidNumberOfRows => GetResourceString("ModuleTableInvalidNumberOfRows", null);

	internal static string UnknownTables => GetResourceString("UnknownTables", null);

	internal static string IllegalTablesInCompressedMetadataStream => GetResourceString("IllegalTablesInCompressedMetadataStream", null);

	internal static string TableRowCountSpaceTooSmall => GetResourceString("TableRowCountSpaceTooSmall", null);

	internal static string OutOfBoundsRead => GetResourceString("OutOfBoundsRead", null);

	internal static string OutOfBoundsWrite => GetResourceString("OutOfBoundsWrite", null);

	internal static string MetadataHeaderTooSmall => GetResourceString("MetadataHeaderTooSmall", null);

	internal static string MetadataSignature => GetResourceString("MetadataSignature", null);

	internal static string NotEnoughSpaceForVersionString => GetResourceString("NotEnoughSpaceForVersionString", null);

	internal static string StreamHeaderTooSmall => GetResourceString("StreamHeaderTooSmall", null);

	internal static string NotEnoughSpaceForStreamHeaderName => GetResourceString("NotEnoughSpaceForStreamHeaderName", null);

	internal static string NotEnoughSpaceForStringStream => GetResourceString("NotEnoughSpaceForStringStream", null);

	internal static string NotEnoughSpaceForBlobStream => GetResourceString("NotEnoughSpaceForBlobStream", null);

	internal static string NotEnoughSpaceForGUIDStream => GetResourceString("NotEnoughSpaceForGUIDStream", null);

	internal static string NotEnoughSpaceForMetadataStream => GetResourceString("NotEnoughSpaceForMetadataStream", null);

	internal static string InvalidMetadataStreamFormat => GetResourceString("InvalidMetadataStreamFormat", null);

	internal static string MetadataTablesTooSmall => GetResourceString("MetadataTablesTooSmall", null);

	internal static string MetadataTableHeaderTooSmall => GetResourceString("MetadataTableHeaderTooSmall", null);

	internal static string WinMDMissingMscorlibRef => GetResourceString("WinMDMissingMscorlibRef", null);

	internal static string UnexpectedStreamEnd => GetResourceString("UnexpectedStreamEnd", null);

	internal static string InvalidMethodRva => GetResourceString("InvalidMethodRva", null);

	internal static string CantGetOffsetForVirtualHeapHandle => GetResourceString("CantGetOffsetForVirtualHeapHandle", null);

	internal static string InvalidNumberOfSections => GetResourceString("InvalidNumberOfSections", null);

	internal static string InvalidSignature => GetResourceString("InvalidSignature", null);

	internal static string PEImageDoesNotHaveMetadata => GetResourceString("PEImageDoesNotHaveMetadata", null);

	internal static string InvalidCodedIndex => GetResourceString("InvalidCodedIndex", null);

	internal static string InvalidCompressedInteger => GetResourceString("InvalidCompressedInteger", null);

	internal static string InvalidDocumentName => GetResourceString("InvalidDocumentName", null);

	internal static string RowIdOrHeapOffsetTooLarge => GetResourceString("RowIdOrHeapOffsetTooLarge", null);

	internal static string EnCMapNotSorted => GetResourceString("EnCMapNotSorted", null);

	internal static string InvalidSerializedString => GetResourceString("InvalidSerializedString", null);

	internal static string StreamTooLarge => GetResourceString("StreamTooLarge", null);

	internal static string ImageTooSmallOrContainsInvalidOffsetOrCount => GetResourceString("ImageTooSmallOrContainsInvalidOffsetOrCount", null);

	internal static string MetadataStringDecoderEncodingMustBeUtf8 => GetResourceString("MetadataStringDecoderEncodingMustBeUtf8", null);

	internal static string InvalidConstantValue => GetResourceString("InvalidConstantValue", null);

	internal static string InvalidConstantValueOfType => GetResourceString("InvalidConstantValueOfType", null);

	internal static string InvalidImportDefinitionKind => GetResourceString("InvalidImportDefinitionKind", null);

	internal static string ValueTooLarge => GetResourceString("ValueTooLarge", null);

	internal static string BlobTooLarge => GetResourceString("BlobTooLarge", null);

	internal static string InvalidTypeSize => GetResourceString("InvalidTypeSize", null);

	internal static string HandleBelongsToFutureGeneration => GetResourceString("HandleBelongsToFutureGeneration", null);

	internal static string InvalidRowCount => GetResourceString("InvalidRowCount", null);

	internal static string InvalidEntryPointToken => GetResourceString("InvalidEntryPointToken", null);

	internal static string TooManySubnamespaces => GetResourceString("TooManySubnamespaces", null);

	internal static string TooManyExceptionRegions => GetResourceString("TooManyExceptionRegions", null);

	internal static string SequencePointValueOutOfRange => GetResourceString("SequencePointValueOutOfRange", null);

	internal static string InvalidDirectoryRVA => GetResourceString("InvalidDirectoryRVA", null);

	internal static string InvalidDirectorySize => GetResourceString("InvalidDirectorySize", null);

	internal static string InvalidDebugDirectoryEntryCharacteristics => GetResourceString("InvalidDebugDirectoryEntryCharacteristics", null);

	internal static string UnexpectedCodeViewDataSignature => GetResourceString("UnexpectedCodeViewDataSignature", null);

	internal static string UnexpectedEmbeddedPortablePdbDataSignature => GetResourceString("UnexpectedEmbeddedPortablePdbDataSignature", null);

	internal static string InvalidPdbChecksumDataFormat => GetResourceString("InvalidPdbChecksumDataFormat", null);

	internal static string UnexpectedSignatureHeader => GetResourceString("UnexpectedSignatureHeader", null);

	internal static string UnexpectedSignatureHeader2 => GetResourceString("UnexpectedSignatureHeader2", null);

	internal static string NotTypeDefOrRefHandle => GetResourceString("NotTypeDefOrRefHandle", null);

	internal static string UnexpectedSignatureTypeCode => GetResourceString("UnexpectedSignatureTypeCode", null);

	internal static string SignatureTypeSequenceMustHaveAtLeastOneElement => GetResourceString("SignatureTypeSequenceMustHaveAtLeastOneElement", null);

	internal static string NotTypeDefOrRefOrSpecHandle => GetResourceString("NotTypeDefOrRefOrSpecHandle", null);

	internal static string UnexpectedDebugDirectoryType => GetResourceString("UnexpectedDebugDirectoryType", null);

	internal static string HeapSizeLimitExceeded => GetResourceString("HeapSizeLimitExceeded", null);

	internal static string BuilderMustAligned => GetResourceString("BuilderMustAligned", null);

	internal static string BuilderAlreadyLinked => GetResourceString("BuilderAlreadyLinked", null);

	internal static string ReturnedBuilderSizeTooSmall => GetResourceString("ReturnedBuilderSizeTooSmall", null);

	internal static string SignatureNotVarArg => GetResourceString("SignatureNotVarArg", null);

	internal static string LabelDoesntBelongToBuilder => GetResourceString("LabelDoesntBelongToBuilder", null);

	internal static string ControlFlowBuilderNotAvailable => GetResourceString("ControlFlowBuilderNotAvailable", null);

	internal static string BaseReaderMustBeFullMetadataReader => GetResourceString("BaseReaderMustBeFullMetadataReader", null);

	internal static string ModuleAlreadyAdded => GetResourceString("ModuleAlreadyAdded", null);

	internal static string AssemblyAlreadyAdded => GetResourceString("AssemblyAlreadyAdded", null);

	internal static string ExpectedListOfSize => GetResourceString("ExpectedListOfSize", null);

	internal static string ExpectedArrayOfSize => GetResourceString("ExpectedArrayOfSize", null);

	internal static string ExpectedNonEmptyList => GetResourceString("ExpectedNonEmptyList", null);

	internal static string ExpectedNonEmptyArray => GetResourceString("ExpectedNonEmptyArray", null);

	internal static string ExpectedNonEmptyString => GetResourceString("ExpectedNonEmptyString", null);

	internal static string ReadersMustBeDeltaReaders => GetResourceString("ReadersMustBeDeltaReaders", null);

	internal static string SignatureProviderReturnedInvalidSignature => GetResourceString("SignatureProviderReturnedInvalidSignature", null);

	internal static string UnknownSectionName => GetResourceString("UnknownSectionName", null);

	internal static string HashTooShort => GetResourceString("HashTooShort", null);

	internal static string UnexpectedArrayLength => GetResourceString("UnexpectedArrayLength", null);

	internal static string ValueMustBeMultiple => GetResourceString("ValueMustBeMultiple", null);

	internal static string MustNotReturnNull => GetResourceString("MustNotReturnNull", null);

	internal static string MetadataVersionTooLong => GetResourceString("MetadataVersionTooLong", null);

	internal static string RowCountMustBeZero => GetResourceString("RowCountMustBeZero", null);

	internal static string RowCountOutOfRange => GetResourceString("RowCountOutOfRange", null);

	internal static string SizeMismatch => GetResourceString("SizeMismatch", null);

	internal static string DataTooBig => GetResourceString("DataTooBig", null);

	internal static string UnsupportedFormatVersion => GetResourceString("UnsupportedFormatVersion", null);

	internal static string DistanceBetweenInstructionAndLabelTooBig => GetResourceString("DistanceBetweenInstructionAndLabelTooBig", null);

	internal static string LabelNotMarked => GetResourceString("LabelNotMarked", null);

	internal static string MethodHasNoExceptionRegions => GetResourceString("MethodHasNoExceptionRegions", null);

	internal static string InvalidExceptionRegionBounds => GetResourceString("InvalidExceptionRegionBounds", null);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool UsingResourceKeys()
	{
		return false;
	}

	internal static string GetResourceString(string resourceKey, string defaultString)
	{
		string text = null;
		try
		{
			text = ResourceManager.GetString(resourceKey);
		}
		catch (MissingManifestResourceException)
		{
		}
		if (defaultString != null && resourceKey.Equals(text, StringComparison.Ordinal))
		{
			return defaultString;
		}
		return text;
	}

	internal static string Format(string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + string.Join(", ", args);
			}
			return string.Format(resourceFormat, args);
		}
		return resourceFormat;
	}

	internal static string Format(string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(resourceFormat, new object[1] { p1 });
	}

	internal static string Format(string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(resourceFormat, new object[2] { p1, p2 });
	}

	internal static string Format(string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(resourceFormat, new object[3] { p1, p2, p3 });
	}
}
