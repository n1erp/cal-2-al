using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract]
public class UserNamePasswordRequestOptions
{
	[DataMember]
	public string DialogCaption { get; set; }

	[DataMember]
	public string Description { get; set; }

	[DataMember]
	public string UserName { get; set; }

	[DataMember]
	public bool RequestIsRetry { get; set; }

	[DataMember]
	public string ErrorOnPreviousAuthentication { get; set; }
}
