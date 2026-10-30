using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[KnownType(typeof(BufferTenantArguments))]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class SetTenantArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }
}
