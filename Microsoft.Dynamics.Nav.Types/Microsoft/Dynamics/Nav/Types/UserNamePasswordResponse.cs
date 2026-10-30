using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[KnownType(typeof(UserNamePasswordCredentials))]
public class UserNamePasswordResponse : IExtensibleDataObject
{
	[DataMember]
	private readonly UserNamePasswordCredentials userNamePasswordCredentials;

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

	public UserNamePasswordCredentials UserNamePasswordCredentials => userNamePasswordCredentials;

	public UserNamePasswordResponse(UserNamePasswordCredentials credentials)
	{
		userNamePasswordCredentials = credentials;
	}
}
