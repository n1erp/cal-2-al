using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class UpdateRequestEventArgs : EventArgs
{
	private NavFormUpdateTypes updateRequestType;

	public NavFormUpdateTypes UpdateRequestType => updateRequestType;

	public UpdateRequestEventArgs(NavFormUpdateTypes updateRequestTypeParameter)
	{
		updateRequestType = updateRequestTypeParameter;
	}
}
