using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class OpenCompanyResponse : ServerResponse
{
	[DataMember]
	public SystemIndicator Indicator { get; set; }

	[DataMember]
	public string CompanyDisplayName { get; set; }
}
