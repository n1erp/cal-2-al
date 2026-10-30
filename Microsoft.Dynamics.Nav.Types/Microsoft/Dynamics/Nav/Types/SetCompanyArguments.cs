using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SetCompanyArguments : IExtensibleDataObject
{
	[DataMember]
	[TelemetryOii]
	public string Name { get; set; }

	[DataMember]
	[TelemetryOii]
	public string DisplayName { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }
}
