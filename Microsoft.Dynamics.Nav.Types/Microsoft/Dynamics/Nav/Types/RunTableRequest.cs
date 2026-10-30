using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RunTableRequest : IExtensibleDataObject
{
	private ExtensionDataObject unknownDataBag;

	public virtual ExtensionDataObject ExtensionData
	{
		get
		{
			return unknownDataBag;
		}
		set
		{
			unknownDataBag = value;
		}
	}

	[DataMember]
	public int ObjectId { get; set; }

	[DataMember]
	public string PersonalizationId { get; set; }

	[DataMember]
	public NavRecordState State { get; set; }
}
