using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class HyperlinkRequest : ServerCallbackRequest
{
	private string hyperlink;

	[DataMember]
	public string Hyperlink
	{
		get
		{
			return hyperlink;
		}
		set
		{
			hyperlink = value;
		}
	}

	public override void Process(IClientCallbackHandler callbackHandler)
	{
		if (callbackHandler == null)
		{
			throw new ArgumentNullException("callbackHandler");
		}
		callbackHandler.Hyperlink(hyperlink, AutomationId);
	}
}
