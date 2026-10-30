using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public sealed class CompanyInformationChanges
{
	[DataMember]
	public string CompanyDisplayName { get; set; }
}
