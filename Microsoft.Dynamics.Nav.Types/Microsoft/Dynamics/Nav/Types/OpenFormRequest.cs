using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class OpenFormRequest : OpenCloseFormRequest
{
	[DataMember]
	public long VersionNumber { get; set; }

	[DataMember]
	public string[] ControlIds { get; set; }
}
