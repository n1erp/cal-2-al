using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ViewInformation
{
	[DataMember]
	public ViewFilterInformation[] Filters { get; set; }

	[DataMember]
	public ViewSortingInformation Sorting { get; set; }
}
