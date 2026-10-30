using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class DialogMessageRequest : ServerCallbackRequest
{
	private string message;

	[DataMember]
	public string Message
	{
		get
		{
			return message;
		}
		set
		{
			message = value;
		}
	}

	public DialogMessageRequest(string message)
	{
		this.message = message;
	}

	public override void Process(IClientCallbackHandler callbackHandler)
	{
		if (callbackHandler == null)
		{
			throw new ArgumentNullException("callbackHandler");
		}
		callbackHandler.Message(message, AutomationId);
	}
}
