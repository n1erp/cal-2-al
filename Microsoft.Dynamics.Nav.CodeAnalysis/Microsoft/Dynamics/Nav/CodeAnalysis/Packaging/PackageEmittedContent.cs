namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class PackageEmittedContent
{
	public bool IsProfileObject { get; set; }

	public int? Id { get; set; }

	public string Name { get; set; }

	public NavTypeKind TypeKind { get; set; }

	public string ApplicationObjectSubtype { get; set; }

	public string CodeFileName { get; set; }

	public string MetadataFileName { get; set; }

	public string SourceFileName { get; set; }

	public int MetadataVersion { get; set; }
}
