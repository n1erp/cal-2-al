namespace Microsoft.Dynamics.Nav.Runtime;

internal interface INCLObjectMetadataBlobCache
{
	NavBLOB GetOrAddBlob(NCLObjectMetadataCacheKey key, BlobGenerator blobGenerator);
}
