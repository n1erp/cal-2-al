using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SendNotificationRequest : ServerCallbackRequest
{
	[DataMember]
	public NotificationInfo Notification { get; set; }

	public SendNotificationRequest(NotificationInfo notification)
	{
		Notification = notification;
	}

	public override void Process(IClientCallbackHandler callbackHandler)
	{
		if (callbackHandler == null)
		{
			throw new ArgumentNullException("callbackHandler");
		}
		callbackHandler.SendNotification(Notification);
	}
}
