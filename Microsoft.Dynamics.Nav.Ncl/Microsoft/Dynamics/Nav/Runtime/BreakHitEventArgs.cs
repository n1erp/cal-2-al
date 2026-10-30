using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class BreakHitEventArgs : EventArgs
{
	private string errorMessage;

	public string ErrorMessage => errorMessage;

	public BreakHitEventArgs(string errorMessage)
	{
		this.errorMessage = errorMessage;
	}
}
