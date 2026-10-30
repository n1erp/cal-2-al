using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SessionSettings : IExtensibleDataObject
{
	public virtual ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	public ClientSettings ClientSettings { get; set; }
}
