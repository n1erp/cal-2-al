namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;

internal abstract class ContentMetadataBuilder
{
	protected abstract string ContentElementName { get; }

	protected void SetContentMetadata(ref ContentMetadata metadata)
	{
		metadata.ContentElementName = ContentElementName;
	}
}
