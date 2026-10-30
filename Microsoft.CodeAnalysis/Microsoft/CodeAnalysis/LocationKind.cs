namespace Microsoft.CodeAnalysis;

public enum LocationKind : byte
{
	None,
	SourceFile,
	MetadataFile,
	XmlFile,
	ExternalFile
}
