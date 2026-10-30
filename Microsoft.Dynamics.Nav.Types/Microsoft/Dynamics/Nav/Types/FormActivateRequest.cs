using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class FormActivateRequest : ServerResponse
{
	[DataMember]
	private readonly Guid serverFormHandle;

	[DataMember]
	private readonly bool refresh;

	public Guid ServerFormHandle
	{
		[DebuggerStepThrough]
		get
		{
			return serverFormHandle;
		}
	}

	public bool Refresh
	{
		[DebuggerStepThrough]
		get
		{
			return refresh;
		}
	}

	public FormActivateRequest(Guid serverFormHandle, bool refresh)
	{
		this.serverFormHandle = serverFormHandle;
		this.refresh = refresh;
	}
}
