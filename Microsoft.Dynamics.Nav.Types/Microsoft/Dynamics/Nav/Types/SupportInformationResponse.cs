using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SupportInformationResponse : ServerResponse
{
	[DataMember]
	private string email;

	[DataMember]
	private string name;

	[DataMember]
	private string url;

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public string Email
	{
		get
		{
			return email;
		}
		set
		{
			email = value;
		}
	}

	public string Url
	{
		get
		{
			return url;
		}
		set
		{
			url = value;
		}
	}
}
