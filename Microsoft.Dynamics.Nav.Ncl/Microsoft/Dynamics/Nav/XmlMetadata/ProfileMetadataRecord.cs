namespace Microsoft.Dynamics.Nav.XmlMetadata;

public class ProfileMetadataRecord : IMetadataRecord
{
	public string ProfileId { get; set; }

	public int PageId { get; set; }

	public string PersonalizationId { get; set; }

	public NavXmlDelta PageMetadataDelta { get; set; }
}
