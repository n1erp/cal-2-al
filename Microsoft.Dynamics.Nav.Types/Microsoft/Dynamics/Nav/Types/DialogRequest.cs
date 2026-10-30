using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class DialogRequest : ServerResponse
{
	[DataMember]
	private Guid dialogHandle;

	public Guid DialogHandle
	{
		get
		{
			return dialogHandle;
		}
		set
		{
			dialogHandle = value;
		}
	}

	public DialogRequest(Guid dialogHandle)
	{
		this.dialogHandle = dialogHandle;
	}
}
