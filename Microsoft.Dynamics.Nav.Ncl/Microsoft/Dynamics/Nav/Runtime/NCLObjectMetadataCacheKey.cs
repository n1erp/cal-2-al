using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NCLObjectMetadataCacheKey
{
	public ObjectType ObjectType { get; }

	public int ObjectId { get; }

	public int ObjectVersion { get; }

	public long Timestamp { get; }

	public NavText Hash { get; }

	public NCLObjectMetadataCacheKey(ObjectType objectType, int objectId, int objectVersion, long timestamp, NavText hash)
	{
		ObjectType = objectType;
		ObjectId = objectId;
		ObjectVersion = objectVersion;
		Timestamp = timestamp;
		Hash = hash;
	}
}
