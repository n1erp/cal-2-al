using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SendSessionUpdateRequestHandler : ServerCallbackRequest
{
	[DataMember]
	public SessionSettingsInfo SessionSettings { get; set; }

	public SendSessionUpdateRequestHandler(SessionSettingsInfo notification)
	{
		SessionSettings = notification;
	}

	public override void Process(IClientCallbackHandler callbackHandler)
	{
		if (callbackHandler == null)
		{
			throw new ArgumentNullException("callbackHandler");
		}
		callbackHandler.SendSessionUpdateRequest(SessionSettings);
	}
}
