using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class DisposeAutomationRequest : ServerCallbackRequest
{
	private int handle;

	[DataMember]
	public int Handle
	{
		get
		{
			return handle;
		}
		set
		{
			handle = value;
		}
	}

	public DisposeAutomationRequest(int handle)
	{
		this.handle = handle;
	}

	public override void Process(IClientCallbackHandler callbackHandler)
	{
		if (callbackHandler == null)
		{
			throw new ArgumentNullException("callbackHandler");
		}
		callbackHandler.DisposeAutomationObject(handle, suppressDispose: false);
	}
}
