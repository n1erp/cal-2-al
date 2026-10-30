using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[KnownType(typeof(DialogMessageRequest))]
[KnownType(typeof(HyperlinkRequest))]
[KnownType(typeof(FormRunRequest))]
[KnownType(typeof(DataSetRequest))]
[KnownType(typeof(DisposeAutomationRequest))]
[KnownType(typeof(SendNotificationRequest))]
[KnownType(typeof(CompanyInformationChangedRequest))]
[KnownType(typeof(WorkDateChangedRequest))]
[KnownType(typeof(SendSessionUpdateRequestHandler))]
public abstract class ServerCallbackRequest : IExtensibleDataObject
{
	private Guid automationId;

	[NonSerialized]
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

	[DataMember]
	public Guid AutomationId
	{
		get
		{
			return automationId;
		}
		set
		{
			automationId = value;
		}
	}

	public abstract void Process(IClientCallbackHandler callbackHandler);
}
