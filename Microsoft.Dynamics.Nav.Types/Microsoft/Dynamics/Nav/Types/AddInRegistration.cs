using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class AddInRegistration
{
	[DataMember]
	public string Name { get; private set; }

	[DataMember]
	public byte[] PublicKeyToken { get; private set; }

	[DataMember]
	public string Version { get; private set; }

	public AddInRegistration(string name, byte[] publicKeyToken)
		: this(name, publicKeyToken, null)
	{
	}

	public AddInRegistration(string name, byte[] publicKeyToken, string version)
	{
		Name = name;
		PublicKeyToken = publicKeyToken;
		Version = version;
	}
}
