using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ViewSortingInformation
{
	[DataMember]
	public int[] FieldIds { get; set; }

	[DataMember]
	public bool Ascending { get; set; }
}
