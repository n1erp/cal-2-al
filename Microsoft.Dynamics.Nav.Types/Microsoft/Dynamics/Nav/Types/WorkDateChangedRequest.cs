using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
internal sealed class WorkDateChangedRequest : ServerCallbackRequest
{
	[DataMember]
	public DateTime WorkDate { get; set; }

	public WorkDateChangedRequest(DateTime workDate)
	{
		WorkDate = workDate;
	}

	public override void Process(IClientCallbackHandler callbackHandler)
	{
		if (callbackHandler == null)
		{
			throw new ArgumentNullException("callbackHandler");
		}
		callbackHandler.WorkDateChanged(WorkDate);
	}
}
