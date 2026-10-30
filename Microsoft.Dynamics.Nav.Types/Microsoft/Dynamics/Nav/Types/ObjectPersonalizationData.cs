namespace Microsoft.Dynamics.Nav.Types;

public class ObjectPersonalizationData
{
	public int ObjectId { get; set; }

	public ObjectType ObjectType { get; set; }

	public string PersonalizationId { get; set; }

	public string PageDataIdentifier { get; set; }

	public string DataToStore { get; set; }
}
