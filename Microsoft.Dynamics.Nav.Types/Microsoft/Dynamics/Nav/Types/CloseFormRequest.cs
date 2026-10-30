using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class CloseFormRequest : OpenCloseFormRequest
{
	[DataMember]
	public int[] ExitActions { get; set; }

	[DataMember]
	public bool Force { get; set; }
}
