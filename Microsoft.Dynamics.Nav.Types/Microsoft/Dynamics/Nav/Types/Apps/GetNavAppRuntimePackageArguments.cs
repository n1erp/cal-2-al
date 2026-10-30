using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types.Apps;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[MessageContract(WrapperNamespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class GetNavAppRuntimePackageArguments : IExtensibleDataObject
{
	private ExtensionDataObject unknownDataBag;

	[MessageHeader(MustUnderstand = true)]
	[DataMember]
	[TelemetrySystemMetadata]
	public string Name { get; set; }

	[MessageHeader(MustUnderstand = true)]
	[DataMember]
	[TelemetrySystemMetadata]
	public string Publisher { get; set; }

	[MessageHeader(MustUnderstand = true)]
	[DataMember]
	[TelemetrySystemMetadata]
	public Version Version { get; set; }

	[MessageHeader(MustUnderstand = true)]
	[DataMember]
	[TelemetrySystemMetadata]
	public Guid? AppId { get; set; }

	[MessageHeader(MustUnderstand = true)]
	[DataMember]
	public string Tenant { get; set; }

	[MessageHeader(MustUnderstand = true)]
	[DataMember]
	[TelemetrySystemMetadata]
	public bool? ShowMyCode { get; set; }

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
}
