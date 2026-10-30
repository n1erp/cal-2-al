namespace Microsoft.Dynamics.Nav.XmlMetadata;

public interface IMetadataRecord
{
	int PageId { get; set; }

	string PersonalizationId { get; set; }

	NavXmlDelta PageMetadataDelta { get; set; }
}
