using System;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

public sealed class DebugSessionStartEventArgs : EventArgs
{
	public string DebuggingContext { get; set; }

	public DebugSessionStartEventArgs(string debuggingContext)
	{
		DebuggingContext = debuggingContext;
	}
}
