using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class DialogOpenRequest : DialogRequest
{
	[DataMember]
	private Guid automationId;

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

	public DialogOpenRequest(Guid dialogHandle, Guid automationId)
		: base(dialogHandle)
	{
		this.automationId = automationId;
	}
}
