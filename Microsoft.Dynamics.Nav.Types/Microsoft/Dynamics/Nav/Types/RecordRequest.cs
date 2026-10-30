using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Data;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RecordRequest : IExtensibleDataObject
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
	public int TableId { get; set; }

	[DataMember]
	public NavDataSet RecDataSet { get; set; }

	[DataMember]
	public bool BelowXRec { get; set; }

	[DataMember]
	public NavRecordState DataSetState { get; set; }
}
