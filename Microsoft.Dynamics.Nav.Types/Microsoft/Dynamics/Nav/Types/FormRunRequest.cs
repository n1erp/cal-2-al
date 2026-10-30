using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class FormRunRequest : ServerCallbackRequest
{
	private FormRunData formRunData = new FormRunData();

	[DataMember]
	public FormRunData Data
	{
		get
		{
			return formRunData;
		}
		set
		{
			formRunData = value;
		}
	}

	public override void Process(IClientCallbackHandler callbackHandler)
	{
		if (callbackHandler == null)
		{
			throw new ArgumentNullException("callbackHandler");
		}
		callbackHandler.FormRun(this);
	}
}
