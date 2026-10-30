namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ContentDestinationMetadata : ContentMetadata
{
	public string ParentContainer { get; set; }

	public ContentMetadataChangeKind Operation { get; set; }

	public int? AnchorId { get; set; }

	public string AnchorName { get; set; }
}
