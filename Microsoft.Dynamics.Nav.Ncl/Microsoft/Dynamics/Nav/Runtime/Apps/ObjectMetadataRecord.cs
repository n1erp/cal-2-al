using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class ObjectMetadataRecord
{
	public ObjectType ObjectType { get; private set; }

	public int ObjectId { get; private set; }

	public int MetadataVersion { get; private set; }

	public string Metadata { get; private set; }

	public string UserCode { get; private set; }

	public string UserALCode { get; private set; }

	public string ObjectSubtype { get; private set; }

	public ObjectMetadataRecord(ObjectType type, int id, int version, string metadata, string userCode, string userALCode, string subtype)
	{
		ObjectType = type;
		ObjectId = id;
		MetadataVersion = version;
		Metadata = metadata;
		UserCode = userCode;
		UserALCode = userALCode;
		ObjectSubtype = subtype;
	}
}
