using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Data;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class OpenCloseFormRequest : IExtensibleDataObject
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

	[DataMember(EmitDefaultValue = false)]
	public NavRecordState[] States { get; set; }

	[DataMember]
	public NavDataSet DataSet { get; set; }

	[DataMember(EmitDefaultValue = false)]
	public bool HasMainForm { get; set; }
}
