using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ViewFilterInformation
{
	[DataMember]
	public int FieldId { get; set; }

	[DataMember]
	public string FilterValue { get; set; }

	[DataMember]
	public string FilterType { get; set; }
}
