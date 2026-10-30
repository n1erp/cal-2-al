using System;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public class UserMetadataRecord : IMetadataRecord
{
	public Guid UserSid { get; set; }

	public int PageId { get; set; }

	public string PersonalizationId { get; set; }

	public NavXmlDelta PageMetadataDelta { get; set; }
}
