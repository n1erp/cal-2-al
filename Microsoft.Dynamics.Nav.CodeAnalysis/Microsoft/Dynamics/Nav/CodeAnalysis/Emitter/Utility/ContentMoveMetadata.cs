namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal class ContentMoveMetadata : ContentDestinationMetadata
{
	public string TargetType { get; set; }

	public int TargetId { get; set; }

	public string TargetName { get; set; }

	public int? SourceParentGroupId { get; set; }
}
