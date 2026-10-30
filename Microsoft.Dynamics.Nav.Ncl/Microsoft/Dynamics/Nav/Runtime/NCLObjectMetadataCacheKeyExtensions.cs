namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NCLObjectMetadataCacheKeyExtensions
{
	public static long GetOffsetTimestamp(this NCLObjectMetadataCacheKey key)
	{
		return AssemblyGetter.OffsetTimestamp(key.Timestamp, key.Hash.ToString());
	}

	public static string GetMetadataFileName(this NCLObjectMetadataCacheKey key)
	{
		return AssemblyGetter.BuildMetadataFileNameUsingStaticDependency(key.ObjectType, key.ObjectId);
	}
}
